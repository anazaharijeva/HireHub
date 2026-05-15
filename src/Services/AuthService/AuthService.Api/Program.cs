using AuthService.Application.Validation;
using AuthService.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using HireHub.ApiCommon;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHireHubDevCors();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddAuthInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHireHubDevCors();
app.UseHttpsRedirection();
app.MapControllers();

await app.Services.EnsureAuthDatabaseAsync().ConfigureAwait(false);

app.Run();
