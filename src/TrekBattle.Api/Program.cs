using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using TrekBattle.Api.Contracts;
using TrekBattle.Api.Data;
using TrekBattle.Api.Services;
using TrekBattle.AspireConstants;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ClientDev",
        policy => policy
            .WithOrigins(
                "http://localhost:4200",
                "http://127.0.0.1:4200",
                "https://localhost:4200",
                "https://127.0.0.1:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var connectionString = builder.Configuration.GetConnectionString(Resources.Base.Database);
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException($"Missing connection string '{Resources.Base.Database}'.");
}

builder.Services.AddDbContext<TrekBattleDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(sp.GetRequiredService<TrekBattleDbSaveChangesInterceptor>());
});

builder.Services.AddScoped<TrekBattleDbSaveChangesInterceptor>();
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
app.Use(async (context, next) =>
{
    var requestLogger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("TrekBattle.Api.Requests");

    var started = DateTimeOffset.UtcNow;

    requestLogger.LogInformation(
        "HTTP {Method} {Path} started.",
        context.Request.Method,
        context.Request.Path);

    try
    {
        await next();
        requestLogger.LogInformation(
            "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds} ms.",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            (DateTimeOffset.UtcNow - started).TotalMilliseconds);
    }
    catch (Exception exception)
    {
        requestLogger.LogError(
            exception,
            "HTTP {Method} {Path} failed after {ElapsedMilliseconds} ms.",
            context.Request.Method,
            context.Request.Path,
            (DateTimeOffset.UtcNow - started).TotalMilliseconds);
        throw;
    }
});
app.MapDefaultEndpoints();
app.UseCors("ClientDev");

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

sessions.MapPost("/{resumeCode}/activate-galaxy", async (
    string resumeCode,
    GameSessionService sessionService,
    ILogger<Program> requestLogger,
    CancellationToken cancellationToken) =>
{
    requestLogger.LogInformation("Activating galaxy view for recovery code {ResumeCode}.", resumeCode);

    return await ExecuteSessionAction(async () =>
    {
        var updated = await sessionService.ActivateGalaxyViewAsync(resumeCode, cancellationToken);
        return updated is null
            ? Results.NotFound(new ProblemDetails
            {
                Title = "Resume code not found",
                Detail = $"No saved session exists for resume code '{resumeCode}'."
            })
            : Results.Ok(updated);
    }, requestLogger, "activate the galaxy view");
});

sessions.MapPost("/{resumeCode}/lrs", async (
    string resumeCode,
    GameSessionService sessionService,
    ILogger<Program> requestLogger,
    CancellationToken cancellationToken) =>
{
    requestLogger.LogInformation("Toggling LRS for recovery code {ResumeCode}.", resumeCode);

    return await ExecuteSessionAction(async () =>
    {
        var updated = await sessionService.ToggleLongRangeScanAsync(resumeCode, cancellationToken);
        return updated is null
            ? Results.NotFound(new ProblemDetails
            {
                Title = "Resume code not found",
                Detail = $"No saved session exists for resume code '{resumeCode}'."
            })
            : Results.Ok(updated);
    }, requestLogger, "toggle the long range scan");
});

sessions.MapPost("/{resumeCode}/warp", async (
    string resumeCode,
    WarpJumpRequest request,
    GameSessionService sessionService,
    ILogger<Program> requestLogger,
    CancellationToken cancellationToken) =>
{
    requestLogger.LogInformation(
        "Queueing warp destination for recovery code {ResumeCode} to {DestinationX},{DestinationY}.",
        resumeCode,
        request.DestinationX,
        request.DestinationY);

    return await ExecuteSessionAction(async () =>
    {
        var updated = await sessionService.WarpJumpAsync(resumeCode, request, cancellationToken);
        return updated is null
            ? Results.NotFound(new ProblemDetails
            {
                Title = "Resume code not found",
                Detail = $"No saved session exists for resume code '{resumeCode}'."
            })
            : Results.Ok(updated);
    }, requestLogger, "warp to a new sector");
});

sessions.MapPost("/{resumeCode}/end-turn", async (
    string resumeCode,
    GameSessionService sessionService,
    ILogger<Program> requestLogger,
    CancellationToken cancellationToken) =>
{
    requestLogger.LogInformation("Ending turn for recovery code {ResumeCode}.", resumeCode);

    return await ExecuteSessionAction(async () =>
    {
        var updated = await sessionService.EndTurnAsync(resumeCode, cancellationToken);
        return updated is null
            ? Results.NotFound(new ProblemDetails
            {
                Title = "Resume code not found",
                Detail = $"No saved session exists for resume code '{resumeCode}'."
            })
            : Results.Ok(updated);
    }, requestLogger, "end the current turn");
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
