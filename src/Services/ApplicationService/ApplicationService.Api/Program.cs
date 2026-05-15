using ApplicationService.Application.Applications;
using ApplicationService.Infrastructure;
using HireHub.ApiCommon;
using MediatR;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHireHubDevCors();
builder.Services.AddMediatR(typeof(ApplyToJobCommandHandler).Assembly);
builder.Services.AddHireHubJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHireHubDevCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.Services.EnsureApplicationDatabaseAsync().ConfigureAwait(false);

app.Run();
