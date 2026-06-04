using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using TrekBattle.Api.Contracts;
using TrekBattle.Api.Data;
using TrekBattle.Api.Services;
using TrekBattle.AspireConstants;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString(Resources.Base.Database);
var useSqlServer = await CanConnectToSqlServerAsync(connectionString);

builder.Services.AddDbContext<TrekBattleDbContext>(options =>
{
    if (useSqlServer && connectionString is not null)
    {
        options.UseSqlServer(connectionString);
        return;
    }

    var sqlitePath = Path.Combine(builder.Environment.ContentRootPath, "trekbattle.local.db");
    options.UseSqlite($"Data Source={sqlitePath}");
});

builder.Services.AddScoped<GameSessionService>();
builder.Services.AddSingleton<IResumeCodeGenerator, ResumeCodeGenerator>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TrekBattleDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();
app.MapDefaultEndpoints();

var sessions = app.MapGroup("/api/sessions");

sessions.MapPost("", async (
    StartGameRequest request,
    GameSessionService sessionService,
    CancellationToken cancellationToken) =>
{
    return await ExecuteSessionAction(async () =>
    {
        var startup = await sessionService.StartNewSessionAsync(request, cancellationToken);
        return Results.Ok(startup);
    });
});

sessions.MapPost("/resume", async (
    ResumeGameRequest request,
    GameSessionService sessionService,
    CancellationToken cancellationToken) =>
{
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
    });
});

app.MapGet("/", () => Results.Ok(new
{
    name = "TrekBattle API",
    status = "running"
}));

app.Run();

static async Task<bool> CanConnectToSqlServerAsync(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return false;
    }

    try
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            ConnectTimeout = 2
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        return true;
    }
    catch
    {
        return false;
    }
}

static IResult ExecuteValidationProblem(string parameterName, string message)
{
    return Results.ValidationProblem(new Dictionary<string, string[]>
    {
        [parameterName] = [message]
    });
}

static async Task<IResult> ExecuteSessionAction(Func<Task<IResult>> action)
{
    try
    {
        return await action();
    }
    catch (ArgumentException exception)
    {
        return ExecuteValidationProblem(
            exception.ParamName ?? "request",
            exception.Message);
    }
}
