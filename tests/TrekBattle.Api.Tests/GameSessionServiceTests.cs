using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TrekBattle.Api.Contracts;
using TrekBattle.Api.Data;
using TrekBattle.Api.Services;

namespace TrekBattle.Api.Tests;

public class GameSessionServiceTests
{
    [Fact]
    public async Task StartNewSessionAsync_PersistsSessionAndReturnsCanonicalResumeCode()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);
        await using var dbContext = new TrekBattleDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var service = new GameSessionService(dbContext, new FixedResumeCodeGenerator("DarkAnchor012"));

        var session = await service.StartNewSessionAsync(
            new StartGameRequest("Elgin", "USS Horizon"),
            CancellationToken.None);

        Assert.Equal("Elgin", session.PlayerName);
        Assert.Equal("USS Horizon", session.ShipName);
        Assert.Equal("DarkAnchor012", session.ResumeCode);
        Assert.Matches(@"^[A-Z][a-z]+[A-Z][a-z]+\d{3}$", session.ResumeCode);

        var savedSession = await dbContext.GameSessions.SingleAsync();
        Assert.Equal(session.ResumeCode, savedSession.ResumeCode);
        Assert.Equal(session.ResumeCode.ToUpperInvariant(), savedSession.ResumeCodeNormalized);
    }

    [Fact]
    public async Task ResumeSessionAsync_FindsSessionUsingCaseInsensitiveResumeCode()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = CreateOptions(connection);
        await using var dbContext = new TrekBattleDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var service = new GameSessionService(dbContext, new FixedResumeCodeGenerator("MerlinBoat444"));

        var created = await service.StartNewSessionAsync(
            new StartGameRequest("Elgin", "USS Horizon"),
            CancellationToken.None);

        var resumed = await service.ResumeSessionAsync(
            new ResumeGameRequest("merlinboat444"),
            CancellationToken.None);

        Assert.NotNull(resumed);
        Assert.Equal(created.SessionId, resumed!.SessionId);
        Assert.Equal(created.ResumeCode, resumed.ResumeCode);
        Assert.Equal(created.PlayerName, resumed.PlayerName);
        Assert.Equal(created.ShipName, resumed.ShipName);
    }

    private static DbContextOptions<TrekBattleDbContext> CreateOptions(SqliteConnection connection)
    {
        return new DbContextOptionsBuilder<TrekBattleDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    private sealed class FixedResumeCodeGenerator(string resumeCode) : IResumeCodeGenerator
    {
        public string Generate()
        {
            return resumeCode;
        }
    }
}
