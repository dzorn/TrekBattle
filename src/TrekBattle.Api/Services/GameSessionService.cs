using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrekBattle.Api.Contracts;
using TrekBattle.Api.Data;
using TrekBattle.Api.Domain;

namespace TrekBattle.Api.Services;

public sealed class GameSessionService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly TrekBattleDbContext _dbContext;
    private readonly IResumeCodeGenerator _resumeCodeGenerator;

    public GameSessionService(TrekBattleDbContext dbContext, IResumeCodeGenerator resumeCodeGenerator)
    {
        _dbContext = dbContext;
        _resumeCodeGenerator = resumeCodeGenerator;
    }

    public async Task<GameSessionState> StartNewSessionAsync(
        StartGameRequest request,
        CancellationToken cancellationToken)
    {
        var playerName = NormalizeRequiredValue(request.PlayerName, nameof(request.PlayerName));
        var shipName = NormalizeRequiredValue(request.ShipName, nameof(request.ShipName));
        var createdUtc = DateTimeOffset.UtcNow;

        for (var attempt = 0; attempt < 32; attempt++)
        {
            var resumeCode = _resumeCodeGenerator.Generate();
            var resumeCodeNormalized = NormalizeResumeCode(resumeCode);

            if (await _dbContext.GameSessions.AnyAsync(
                session => session.ResumeCodeNormalized == resumeCodeNormalized,
                cancellationToken))
            {
                continue;
            }

            var state = CreateState(
                sessionId: Guid.NewGuid(),
                playerName,
                shipName,
                resumeCode,
                createdUtc);

            var entity = new GameSessionEntity
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

            _dbContext.GameSessions.Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return state;
            }
            catch (DbUpdateException)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique resume code.");
    }

    public async Task<GameSessionState?> ResumeSessionAsync(
        ResumeGameRequest request,
        CancellationToken cancellationToken)
    {
        var resumeCodeNormalized = NormalizeResumeCode(NormalizeRequiredValue(request.ResumeCode, nameof(request.ResumeCode)));

        var entity = await _dbContext.GameSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(session => session.ResumeCodeNormalized == resumeCodeNormalized, cancellationToken);

        return entity is null
            ? null
            : JsonSerializer.Deserialize<GameSessionState>(entity.StateJson, JsonOptions);
    }

    private static GameSessionState CreateState(
        Guid sessionId,
        string playerName,
        string shipName,
        string resumeCode,
        DateTimeOffset createdUtc)
    {
        return new GameSessionState(
            sessionId,
            playerName,
            shipName,
            resumeCode,
            GameContent.MissionTitle,
            GameContent.MissionBrief,
            GameContent.MissionObjective,
            "Launch",
            createdUtc);
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
