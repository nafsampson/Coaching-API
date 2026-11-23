// Configures Firebase token validation, services, HTTPS and the API routes.

using FormUp_API.Endpoints;
using FormUp_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Require an explicit project ID so the API cannot silently use another Firebase project.
var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
if (string.IsNullOrWhiteSpace(firebaseProjectId))
{
    throw new InvalidOperationException("Firebase:ProjectId is missing from configuration.");
}

var firebaseIssuer = $"https://securetoken.google.com/{firebaseProjectId}";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = firebaseIssuer;
        options.MapInboundClaims = false;
        // Accept only valid, unexpired tokens issued for the configured Firebase project.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = firebaseIssuer,
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

// Require authentication unless an endpoint explicitly provides another access check.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddSingleton<CoachService>();
builder.Services.AddSingleton<ReminderService>();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
// Expose Swagger only during local development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Allow health checks without a user token.
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }))
   .AllowAnonymous();

app.MapCoachEndpoints();
app.MapReminderEndpoints();
app.MapPlayerEndpoints();
app.MapTeamEndpoints();
app.MapMatchEndpoints();
app.MapSessionEndpoints();
app.MapTeamStatsEndpoints();
app.MapAttendanceEndpoints();
app.MapSeasonDataEndpoints();
app.MapFormationEndpoints();

// Let the Android queue check whether create retries are supported.
app.MapGet("/api/client-capabilities", () => Results.Ok(new { idempotentCreates = true }))
    .RequireAuthorization();

app.Run();
