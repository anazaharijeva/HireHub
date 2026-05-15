using HireHub.ApiCommon;
using JobService.Application.Mapping;
using JobService.Infrastructure;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHireHubDevCors();
builder.Services.AddAutoMapper(typeof(JobProfile));
builder.Services.AddHireHubJwtAuthentication(builder.Configuration);
builder.Services.AddJobInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHireHubDevCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.Services.EnsureJobDatabaseAsync().ConfigureAwait(false);

app.Run();
