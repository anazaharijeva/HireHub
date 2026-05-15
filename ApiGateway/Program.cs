using Ocelot.DependencyInjection;
using Ocelot.Middleware;

// When the host is started by the debugger, cwd can be the IDE install dir instead of the
// project/output folder. Content root and relative JSON paths follow cwd unless we fix it.
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

var inContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);
var ocelotFile = inContainer ? "ocelot.json" : "ocelot.local.json";
builder.Configuration.AddJsonFile(ocelotFile, optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

var app = builder.Build();
app.UseCors();
await app.UseOcelot().ConfigureAwait(false);
await app.RunAsync().ConfigureAwait(false);
