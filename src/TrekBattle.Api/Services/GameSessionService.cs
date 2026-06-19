using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrekBattle.Api.Contracts;
using TrekBattle.Api.Data;
using TrekBattle.Api.Domain;

namespace TrekBattle.Api.Services;

public sealed class GameSessionService
{
    private const int GalaxyWidth = 12;
    private const int GalaxyHeight = 12;
    private const int JumpRange = 5;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly TrekBattleDbContext _dbContext;
    private readonly IResumeCodeGenerator _resumeCodeGenerator;
    private readonly ILogger<GameSessionService> _logger;

    public GameSessionService(
        TrekBattleDbContext dbContext,
        IResumeCodeGenerator resumeCodeGenerator,
        ILogger<GameSessionService> logger)
    {
        _dbContext = dbContext;
        _resumeCodeGenerator = resumeCodeGenerator;
        _logger = logger;
    }

    public async Task<GameSessionState> StartNewSessionAsync(
        StartGameRequest request,
        CancellationToken cancellationToken)
    {
        var playerName = NormalizeRequiredValue(request.PlayerName, nameof(request.PlayerName));
        var shipName = NormalizeRequiredValue(request.ShipName, nameof(request.ShipName));
        var createdUtc = DateTimeOffset.UtcNow;

        _logger.LogInformation(
            "Generating a new session for player {PlayerName} and ship {ShipName}.",
            playerName,
            shipName);

        for (var attempt = 0; attempt < 32; attempt++)
        {
            var resumeCode = _resumeCodeGenerator.Generate();
            var resumeCodeNormalized = NormalizeResumeCode(resumeCode);

            if (await _dbContext.GameSessions.AnyAsync(
                session => session.ResumeCodeNormalized == resumeCodeNormalized,
                cancellationToken))
            {
                _logger.LogInformation(
                    "Generated recovery code {ResumeCode} was already in use. Retrying attempt {Attempt}.",
                    resumeCode,
                    attempt + 1);
                continue;
            }

            var state = CreateInitialState(
                sessionId: Guid.NewGuid(),
                playerName,
                shipName,
                resumeCode,
                createdUtc);

            var entity = CreateEntity(state, resumeCodeNormalized, createdUtc);
            _dbContext.GameSessions.Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Saved session {SessionId} with recovery code {ResumeCode}.",
                    state.SessionId,
                    state.ResumeCode);
                return state;
            }
            catch (DbUpdateException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Database write failed while saving session {SessionId}. Retrying if attempts remain.",
                    state.SessionId);
                _dbContext.Entry(entity).State = EntityState.Detached;
            }
        }

        _logger.LogError("Unable to generate a unique resume code after multiple attempts.");
        throw new InvalidOperationException("Unable to generate a unique resume code.");
    }

    public async Task<GameSessionState?> ResumeSessionAsync(
        ResumeGameRequest request,
        CancellationToken cancellationToken)
    {
        var resumeCodeNormalized = NormalizeResumeCode(NormalizeRequiredValue(request.ResumeCode, nameof(request.ResumeCode)));

        _logger.LogInformation("Looking up session for recovery code {ResumeCode}.", request.ResumeCode);

        var entity = await _dbContext.GameSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(session => session.ResumeCodeNormalized == resumeCodeNormalized, cancellationToken);

        if (entity is null)
        {
            _logger.LogInformation("No session was found for recovery code {ResumeCode}.", request.ResumeCode);
            return null;
        }

        _logger.LogInformation(
            "Loaded session {SessionId} for recovery code {ResumeCode}.",
            entity.SessionId,
            entity.ResumeCode);

        return DeserializeState(entity.StateJson);
    }

    public Task<GameSessionState?> ActivateGalaxyViewAsync(string resumeCode, CancellationToken cancellationToken)
    {
        return UpdateSessionAsync(
            resumeCode,
            state => state with { CurrentScreen = "Galaxy" },
            cancellationToken);
    }

    public Task<GameSessionState?> ToggleLongRangeScanAsync(string resumeCode, CancellationToken cancellationToken)
    {
        return UpdateSessionAsync(
            resumeCode,
            state =>
            {
                var galaxyMap = EnsureGalaxyMap(state.GalaxyMap);

                return state with
                {
                    CurrentScreen = "Galaxy",
                    GalaxyMap = galaxyMap with
                    {
                        ActionUsed = !galaxyMap.ActionUsed
                    }
                };
            },
            cancellationToken);
    }

    public Task<GameSessionState?> WarpJumpAsync(
        string resumeCode,
        WarpJumpRequest request,
        CancellationToken cancellationToken)
    {
        return UpdateSessionAsync(
            resumeCode,
            state =>
            {
                var galaxyMap = EnsureGalaxyMap(state.GalaxyMap);

                if (request.DestinationX is null && request.DestinationY is null)
                {
                    return state with
                    {
                        CurrentScreen = "Galaxy",
                        GalaxyMap = galaxyMap with
                        {
                            MovementUsed = false,
                            QueuedDestinationX = null,
                            QueuedDestinationY = null
                        }
                    };
                }

                if (request.DestinationX is null || request.DestinationY is null)
                {
                    throw new ArgumentException("Warp destination must include both coordinates or neither.", nameof(request));
                }

                if (
                    request.DestinationX == galaxyMap.CurrentX &&
                    request.DestinationY == galaxyMap.CurrentY)
                {
                    throw new ArgumentException("Destination must be different from the current location.", nameof(request));
                }

                var destinationX = request.DestinationX.Value;
                var destinationY = request.DestinationY.Value;
                var deltaX = Math.Abs(destinationX - galaxyMap.CurrentX);
                var deltaY = Math.Abs(destinationY - galaxyMap.CurrentY);
                if (Math.Max(deltaX, deltaY) > galaxyMap.JumpRange)
                {
                    throw new ArgumentException("Destination is outside the warp range.", nameof(request));
                }

                return state with
                {
                    CurrentScreen = "Galaxy",
                    GalaxyMap = galaxyMap with
                    {
                        MovementUsed = false,
                        QueuedDestinationX = destinationX,
                        QueuedDestinationY = destinationY
                    }
                };
            },
            cancellationToken);
    }

    public Task<GameSessionState?> EndTurnAsync(string resumeCode, CancellationToken cancellationToken)
    {
        return UpdateSessionAsync(
            resumeCode,
            state =>
            {
                var galaxyMap = EnsureGalaxyMap(state.GalaxyMap);
                var updatedMap = galaxyMap;

                if (galaxyMap.ActionUsed)
                {
                    updatedMap = ApplyLongRangeScan(updatedMap);
                }

                updatedMap = ApplyQueuedWarpJump(updatedMap);

                return state with
                {
                    CurrentScreen = "Galaxy",
                    GalaxyMap = updatedMap with
                    {
                        CompletedTurns = galaxyMap.CompletedTurns + 1,
                        ActionUsed = false,
                        MovementUsed = false,
                        QueuedDestinationX = null,
                        QueuedDestinationY = null
                    }
                };
            },
            cancellationToken);
    }

    private static GalaxyMapState ApplyLongRangeScan(GalaxyMapState galaxyMap)
    {
        var updatedSectors = galaxyMap.Sectors
            .Select(sector =>
            {
                var isVisible = Math.Abs(sector.X - galaxyMap.CurrentX) <= 1
                    && Math.Abs(sector.Y - galaxyMap.CurrentY) <= 1
                    && IsWithinBounds(sector.X, sector.Y);

                return isVisible
                    ? sector with { Scanned = true }
                    : sector;
            })
            .ToArray();

        return galaxyMap with
        {
            Sectors = updatedSectors
        };
    }

    private static GalaxyMapState ApplyQueuedWarpJump(GalaxyMapState galaxyMap)
    {
        if (galaxyMap.QueuedDestinationX is null || galaxyMap.QueuedDestinationY is null)
        {
            return galaxyMap;
        }

        var destinationX = ClampToGalaxy(galaxyMap.QueuedDestinationX.Value, GalaxyWidth);
        var destinationY = ClampToGalaxy(galaxyMap.QueuedDestinationY.Value, GalaxyHeight);
        var updatedSectors = galaxyMap.Sectors
            .Select(sector => sector.X == destinationX && sector.Y == destinationY
                ? sector with { Visited = true, Scanned = true }
                : sector)
            .ToArray();

        return galaxyMap with
        {
            CurrentX = destinationX,
            CurrentY = destinationY,
            MovementUsed = false,
            QueuedDestinationX = null,
            QueuedDestinationY = null,
            Sectors = updatedSectors
        };
    }

    private async Task<GameSessionState?> UpdateSessionAsync(
        string resumeCode,
        Func<GameSessionState, GameSessionState> updater,
        CancellationToken cancellationToken)
    {
        var resumeCodeNormalized = NormalizeResumeCode(NormalizeRequiredValue(resumeCode, nameof(resumeCode)));

        var entity = await _dbContext.GameSessions
            .SingleOrDefaultAsync(session => session.ResumeCodeNormalized == resumeCodeNormalized, cancellationToken);

        if (entity is null)
        {
            _logger.LogInformation("No session was found for recovery code {ResumeCode}.", resumeCode);
            return null;
        }

        var currentState = DeserializeState(entity.StateJson);
        var updatedState = updater(currentState);

        entity.StateJson = JsonSerializer.Serialize(updatedState, JsonOptions);
        entity.UpdatedUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return updatedState;
    }

    private static GameSessionState CreateInitialState(
        Guid sessionId,
        string playerName,
        string shipName,
        string resumeCode,
        DateTimeOffset createdUtc)
    {
        var currentX = RandomNumberGenerator.GetInt32(GalaxyWidth);
        var currentY = RandomNumberGenerator.GetInt32(GalaxyHeight);
        var sectors = new List<GalaxySectorState>(GalaxyWidth * GalaxyHeight);

        for (var y = 0; y < GalaxyHeight; y++)
        {
            for (var x = 0; x < GalaxyWidth; x++)
            {
                sectors.Add(new GalaxySectorState(
                    x,
                    y,
                    EnemyCount: 0,
                    PlanetCount: 0,
                    BaseCount: 0,
                    Visited: x == currentX && y == currentY,
                    Scanned: x == currentX && y == currentY));
            }
        }

        return new GameSessionState(
            sessionId,
            playerName,
            shipName,
            resumeCode,
            GameContent.MissionTitle,
            GameContent.MissionBrief,
            GameContent.MissionObjective,
            "Launch",
            createdUtc,
            new GalaxyMapState(
                GalaxyWidth,
                GalaxyHeight,
                JumpRange,
                currentX,
                currentY,
                CompletedTurns: 0,
                ActionUsed: false,
                MovementUsed: false,
                QueuedDestinationX: null,
                QueuedDestinationY: null,
                Sectors: sectors));
    }

    private static GameSessionEntity CreateEntity(
        GameSessionState state,
        string resumeCodeNormalized,
        DateTimeOffset createdUtc)
    {
        return new GameSessionEntity
        {
            SessionId = state.SessionId,
            PlayerName = state.PlayerName,
            ShipName = state.ShipName,
            ResumeCode = state.ResumeCode,
            ResumeCodeNormalized = resumeCodeNormalized,
            StateJson = JsonSerializer.Serialize(state, JsonOptions),
            CreatedUtc = createdUtc,
            UpdatedUtc = createdUtc
        };
    }

    private static GameSessionState DeserializeState(string stateJson)
    {
        return JsonSerializer.Deserialize<GameSessionState>(stateJson, JsonOptions)
            ?? throw new InvalidOperationException("Saved session state could not be read.");
    }

    private static GalaxyMapState EnsureGalaxyMap(GalaxyMapState galaxyMap)
    {
        return galaxyMap with { };
    }

    private static bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < GalaxyWidth && y >= 0 && y < GalaxyHeight;
    }

    private static int ClampToGalaxy(int value, int upperExclusive)
    {
        return Math.Clamp(value, 0, upperExclusive - 1);
    }

    private static string NormalizeRequiredValue(string? value, string parameterName)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException($"'{parameterName}' is required.", parameterName);
        }

        return normalized;
    }

    private static string NormalizeResumeCode(string resumeCode)
    {
        return resumeCode.Trim().ToUpperInvariant();
    }
}
