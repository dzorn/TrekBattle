using Aspire.Hosting;
using TrekBattle.AspireConstants;

var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer(Resources.Base.SqlServer)
    // Recreate the database fresh on each AppHost start so the SQL login and schema stay in sync.
    .WithLifetime(ContainerLifetime.Session);

var database = sqlServer.AddDatabase(Resources.Base.Database);

var api = builder.AddProject<Projects.TrekBattle_Api>(Resources.Projects.Api)
    .WithReference(database)
    .WaitFor(database);

builder.AddExecutable(
        Resources.Projects.Client,
        "npx",
        "../TrekBattle.Client",
        "-p",
        "node@24.15.0",
        "-c",
        "node ./node_modules/@angular/cli/bin/ng serve --port $PORT --proxy-config proxy.conf.cjs")
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithEnvironment("API_BASE_URL", api.GetEndpoint("http"))
    .WaitFor(api);

builder.Build().Run();
