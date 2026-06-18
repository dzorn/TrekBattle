using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TrekBattle.Api.Contracts;
using TrekBattle.Api.Data;
using TrekBattle.Api.Services;

namespace TrekBattle.Api.Tests;

public class GameSessionServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task StartNewSessionAsync_InitializesGalaxyStateWithAVisitedAndScannedCurrentSectorAndZeroTurns()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);
        await using var dbContext = new TrekBattleDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var service = new GameSessionService(
            dbContext,
            new FixedResumeCodeGenerator("DarkAnchor012"),
            NullLogger<GameSessionService>.Instance);

        var session = await service.StartNewSessionAsync(
            new StartGameRequest("Elgin", "USS Horizon"),
            CancellationToken.None);

        Assert.Equal("Elgin", session.PlayerName);
        Assert.Equal("USS Horizon", session.ShipName);
        Assert.Equal("DarkAnchor012", session.ResumeCode);
        Assert.Equal("Launch", session.CurrentScreen);
        Assert.Equal(0, session.GalaxyMap.CompletedTurns);
        Assert.False(session.GalaxyMap.ActionUsed);
        Assert.False(session.GalaxyMap.MovementUsed);
        Assert.InRange(session.GalaxyMap.CurrentX, 0, 11);
        Assert.InRange(session.GalaxyMap.CurrentY, 0, 11);

        var currentSector = session.GalaxyMap.Sectors.Single(sector =>
            sector.X == session.GalaxyMap.CurrentX &&
            sector.Y == session.GalaxyMap.CurrentY);

        Assert.True(currentSector.Visited);
        Assert.True(currentSector.Scanned);
        Assert.All(session.GalaxyMap.Sectors, sector =>
        {
            Assert.Equal(0, sector.EnemyCount);
            Assert.Equal(0, sector.PlanetCount);
            Assert.Equal(0, sector.BaseCount);
        });

        var savedSession = await dbContext.GameSessions.SingleAsync();
        Assert.Equal(session.ResumeCode, savedSession.ResumeCode);
        Assert.Equal(session.ResumeCode.ToUpperInvariant(), savedSession.ResumeCodeNormalized);
    }

    [Fact]
    public async Task ActivateGalaxyViewAsync_PersistsTheGalaxyScreen()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);
        await using var dbContext = new TrekBattleDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var service = new GameSessionService(
            dbContext,
            new FixedResumeCodeGenerator("MerlinBoat444"),
            NullLogger<GameSessionService>.Instance);

        await service.StartNewSessionAsync(
            new StartGameRequest("Elgin", "USS Horizon"),
            CancellationToken.None);

        var activated = await service.ActivateGalaxyViewAsync("MerlinBoat444", CancellationToken.None);

        Assert.NotNull(activated);
        Assert.Equal("Galaxy", activated!.CurrentScreen);
    }

    [Fact]
    public async Task ToggleLongRangeScanAsync_QueuesTheActionWithoutRevealingUntilTurnEnds()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);
        await using var dbContext = new TrekBattleDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var service = new GameSessionService(
            dbContext,
            new FixedResumeCodeGenerator("SilverComet777"),
            NullLogger<GameSessionService>.Instance);

        var created = await service.StartNewSessionAsync(
            new StartGameRequest("Elgin", "USS Horizon"),
            CancellationToken.None);

        var beforeScannedCount = created.GalaxyMap.Sectors.Count(sector => sector.Scanned);

        var queued = await service.ToggleLongRangeScanAsync("SilverComet777", CancellationToken.None);

        Assert.NotNull(queued);
        Assert.True(queued!.GalaxyMap.ActionUsed);
        Assert.Equal(0, queued.GalaxyMap.CompletedTurns);

        var afterScannedCount = queued.GalaxyMap.Sectors.Count(sector => sector.Scanned);
        Assert.Equal(beforeScannedCount, afterScannedCount);

        Assert.DoesNotContain(queued.GalaxyMap.Sectors, sector =>
            sector.X < 0 || sector.X > 11 || sector.Y < 0 || sector.Y > 11);
    }

    [Fact]
    public async Task WarpJumpAsync_MovesWithinJumpRangeAndMarksTheDestinationVisitedAndScanned()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);
        await using var dbContext = new TrekBattleDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var service = new GameSessionService(
            dbContext,
            new FixedResumeCodeGenerator("GammaWave333"),
            NullLogger<GameSessionService>.Instance);

        var created = await service.StartNewSessionAsync(
            new StartGameRequest("Elgin", "USS Horizon"),
            CancellationToken.None);

        var destination = ChooseWarpDestination(created.GalaxyMap.CurrentX, created.GalaxyMap.CurrentY);
        var jumped = await service.WarpJumpAsync(
            "GammaWave333",
            new WarpJumpRequest(destination.DestinationX, destination.DestinationY),
            CancellationToken.None);

        Assert.NotNull(jumped);
        Assert.True(jumped!.GalaxyMap.MovementUsed);
        Assert.Equal(destination.DestinationX, jumped.GalaxyMap.CurrentX);
        Assert.Equal(destination.DestinationY, jumped.GalaxyMap.CurrentY);
        Assert.Contains(jumped.GalaxyMap.Sectors, sector =>
            sector.X == destination.DestinationX &&
            sector.Y == destination.DestinationY &&
            sector.Visited &&
            sector.Scanned);
    }

    [Fact]
    public async Task WarpJumpAsync_ClampsOutOfBoundsDestinationsToTheGalaxyEdge()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);
        await using var dbContext = new TrekBattleDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var service = new GameSessionService(
            dbContext,
            new FixedResumeCodeGenerator("EdgeClamp444"),
            NullLogger<GameSessionService>.Instance);

        var created = await service.StartNewSessionAsync(
            new StartGameRequest("Elgin", "USS Horizon"),
            CancellationToken.None);

        var currentX = created.GalaxyMap.CurrentX;
        var currentY = created.GalaxyMap.CurrentY;
        var intendedDestinationX = currentX <= 5 ? currentX - 6 : currentX + 6;

        var jumped = await service.WarpJumpAsync(
            "EdgeClamp444",
            new WarpJumpRequest(intendedDestinationX, currentY),
            CancellationToken.None);

        Assert.NotNull(jumped);
        Assert.True(jumped!.GalaxyMap.MovementUsed);
        Assert.Equal(currentY, jumped.GalaxyMap.CurrentY);
        Assert.Equal(currentX <= 5 ? 0 : 11, jumped.GalaxyMap.CurrentX);
    }

    [Fact]
    public async Task EndTurnAsync_ResolvesQueuedLongRangeScanAndCompletesTheTurn()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);
        await using var dbContext = new TrekBattleDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var service = new GameSessionService(
            dbContext,
            new FixedResumeCodeGenerator("TurnOrder555"),
            NullLogger<GameSessionService>.Instance);

        var created = await service.StartNewSessionAsync(
            new StartGameRequest("Elgin", "USS Horizon"),
            CancellationToken.None);

        var originX = created.GalaxyMap.CurrentX;
        var originY = created.GalaxyMap.CurrentY;
        await service.ToggleLongRangeScanAsync("TurnOrder555", CancellationToken.None);

        var ended = await service.EndTurnAsync("TurnOrder555", CancellationToken.None);

        Assert.NotNull(ended);
        Assert.Equal(1, ended!.GalaxyMap.CompletedTurns);
        Assert.False(ended.GalaxyMap.ActionUsed);
        Assert.False(ended.GalaxyMap.MovementUsed);
        Assert.Equal(originX, ended.GalaxyMap.CurrentX);
        Assert.Equal(originY, ended.GalaxyMap.CurrentY);

        foreach (var sector in ended.GalaxyMap.Sectors.Where(sector =>
                     Math.Abs(sector.X - originX) <= 1 &&
                     Math.Abs(sector.Y - originY) <= 1))
        {
            Assert.True(sector.Scanned);
        }

        Assert.Contains(ended.GalaxyMap.Sectors, sector =>
            sector.X == originX &&
            sector.Y == originY &&
            sector.Scanned);
    }

    [Fact]
    public async Task WarpJumpAsync_RejectsDestinationsThatDoNotMoveTheShip()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);
        await using var dbContext = new TrekBattleDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var service = new GameSessionService(
            dbContext,
            new FixedResumeCodeGenerator("NovaTrail222"),
            NullLogger<GameSessionService>.Instance);

        var created = await service.StartNewSessionAsync(
            new StartGameRequest("Elgin", "USS Horizon"),
            CancellationToken.None);

        var currentX = created.GalaxyMap.CurrentX;
        var currentY = created.GalaxyMap.CurrentY;

        await Assert.ThrowsAsync<ArgumentException>(() => service.WarpJumpAsync(
            "NovaTrail222",
            new WarpJumpRequest(currentX, currentY),
            CancellationToken.None));
    }

    [Fact]
    public async Task EndTurnAsync_CompletesAnEmptyTurnAndResetsTurnUsage()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);
        await using var dbContext = new TrekBattleDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var service = new GameSessionService(
            dbContext,
            new FixedResumeCodeGenerator("EmptyTurn111"),
            NullLogger<GameSessionService>.Instance);

        await service.StartNewSessionAsync(
            new StartGameRequest("Elgin", "USS Horizon"),
            CancellationToken.None);

        var ended = await service.EndTurnAsync("EmptyTurn111", CancellationToken.None);

        Assert.NotNull(ended);
        Assert.Equal(1, ended!.GalaxyMap.CompletedTurns);
        Assert.False(ended.GalaxyMap.ActionUsed);
        Assert.False(ended.GalaxyMap.MovementUsed);

        var savedSession = await dbContext.GameSessions.SingleAsync();
        var restored = JsonSerializer.Deserialize<GameSessionState>(savedSession.StateJson, JsonOptions);

        Assert.NotNull(restored);
        Assert.Equal(1, restored!.GalaxyMap.CompletedTurns);
    }

    private static DbContextOptions<TrekBattleDbContext> CreateOptions(SqliteConnection connection)
    {
        return new DbContextOptionsBuilder<TrekBattleDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    private static (int DestinationX, int DestinationY) ChooseWarpDestination(int currentX, int currentY)
    {
        var destinationX = currentX < 11 ? currentX + 1 : currentX - 1;
        var destinationY = currentY;

        if (Math.Abs(destinationX - currentX) > 5)
        {
            destinationX = currentX;
            destinationY = currentY < 11 ? currentY + 1 : currentY - 1;
        }

        return (destinationX, destinationY);
    }

    private sealed class FixedResumeCodeGenerator(string resumeCode) : IResumeCodeGenerator
    {
        public string Generate()
        {
            return resumeCode;
        }
    }
}
