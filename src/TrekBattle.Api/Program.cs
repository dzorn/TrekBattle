using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using TrekBattle.Api.Contracts;
using TrekBattle.Api.Data;
using TrekBattle.Api.Services;
using TrekBattle.AspireConstants;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString(Resources.Base.Database);
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException($"Missing connection string '{Resources.Base.Database}'.");
}

builder.Services.AddDbContext<TrekBattleDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<GameSessionService>();
builder.Services.AddSingleton<IResumeCodeGenerator, ResumeCodeGenerator>();

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("TrekBattle.Api.Startup");

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TrekBattleDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied for {DatabaseProvider}.", dbContext.Database.ProviderName);
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Failed to initialize the game database.");
        throw;
    }
}

app.UseHttpsRedirection();
app.MapDefaultEndpoints();

var sessions = app.MapGroup("/api/sessions");

sessions.MapPost("", async (
    StartGameRequest request,
    GameSessionService sessionService,
    ILogger<Program> requestLogger,
    CancellationToken cancellationToken) =>
{
    requestLogger.LogInformation(
        "Starting session for player {PlayerName} and ship {ShipName}.",
        request.PlayerName,
        request.ShipName);

    return await ExecuteSessionAction(async () =>
    {
        var startup = await sessionService.StartNewSessionAsync(request, cancellationToken);
        requestLogger.LogInformation(
            "Created session {SessionId} with recovery code {ResumeCode}.",
            startup.SessionId,
            startup.ResumeCode);
        return Results.Ok(startup);
    }, requestLogger, "start a new session");
});

sessions.MapPost("/resume", async (
    ResumeGameRequest request,
    GameSessionService sessionService,
    ILogger<Program> requestLogger,
    CancellationToken cancellationToken) =>
{
    requestLogger.LogInformation("Resuming session using recovery code {ResumeCode}.", request.ResumeCode);

    return await ExecuteSessionAction(async () =>
    {
        var startup = await sessionService.ResumeSessionAsync(request, cancellationToken);
        return startup is null
            ? Results.NotFound(new ProblemDetails
            {
                Title = "Resume code not found",
                Detail = $"No saved session exists for resume code '{request.ResumeCode}'."
            })
            : Results.Ok(startup);
    }, requestLogger, "resume a session");
});

app.MapGet("/", () => Results.Ok(new
{
    name = "TrekBattle API",
    status = "running"
}));

app.Run();

static IResult ExecuteValidationProblem(string parameterName, string message)
{
    return Results.ValidationProblem(new Dictionary<string, string[]>
    {
        [parameterName] = [message]
    });
}

static async Task<IResult> ExecuteSessionAction(
    Func<Task<IResult>> action,
    ILogger logger,
    string operationName)
{
    try
    {
        return await action();
    }
    catch (ArgumentException exception)
    {
        logger.LogWarning(
            exception,
            "Validation failed while trying to {OperationName}.",
            operationName);
        return ExecuteValidationProblem(
            exception.ParamName ?? "request",
            exception.Message);
    }
    catch (Exception exception)
    {
        logger.LogError(
            exception,
            "Unexpected failure while trying to {OperationName}.",
            operationName);

        return Results.Problem(
            title: "Game session operation failed",
            detail: "The server could not complete the request. Check the server logs for details.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
}
