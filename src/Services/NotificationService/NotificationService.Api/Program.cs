using HireHub.ApiCommon;
using NotificationService.Infrastructure;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHireHubDevCors();
builder.Services.AddHireHubJwtAuthentication(builder.Configuration);
builder.Services.AddNotificationInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHireHubDevCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.Services.EnsureNotificationDatabaseAsync().ConfigureAwait(false);

app.Run();
