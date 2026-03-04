using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QuizSystem.Api.QuestionSystem.Application;
using QuizSystem.Api.QuestionSystem.Infrastructure;
using QuizSystem.Api.QuestionSystem.Api.Middleware;
using System.Security.Claims;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application + Infrastructure
builder.Services.AddApplication();
builder.Services.AddApplicationCommand();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("connectionString")!
);


// ============================================
// JWT AUTHENTICATION SETUP
// ============================================
// Configuration: JWT settings are read from appsettings.json Jwt section
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // CRITICAL: Enable all validation checks
            ValidateIssuer = true,                      // Check issuer matches Auth API
            ValidateAudience = true,                    // Check audience is this API
            ValidateLifetime = true,                    // Check token hasn't expired
            ValidateIssuerSigningKey = true,            // Check signature is valid

            // SECURITY: These values MUST match what the Auth API uses
            ValidIssuer = jwtSettings["validIssuer"],                    // E.g., "RealWorldAPI"
            ValidAudience = jwtSettings["validAudience"],                // E.g., "http://localhost:5170"

            // SECURITY: Use the same signing key as the Auth API
            // In production: Store securely in Azure Key Vault or similar
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["key"]!)
            ),

            // Additional security settings
            ClockSkew = TimeSpan.FromSeconds(10),

            NameClaimType = "sub",  // Use 'sub' for user ID
            RoleClaimType = ClaimTypes.Role
        };

        // Log JWT bearer events for debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Log authentication failures
                Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                // Optional: Validate custom claims here
                return Task.CompletedTask;
            }
        };
    });

// ============================================
// AUTHORIZATION SETUP
// ============================================
builder.Services.AddAuthorization(options =>
{
    // Policy: User must be authenticated
    options.AddPolicy("Authenticated", policy =>
        policy.RequireAuthenticatedUser());

    // Policy: User must be Admin
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Policy: User must be Creator or Admin
    options.AddPolicy("CreatorOrAdmin", policy =>
        policy.RequireRole("Creator", "Admin"));

    // Policy: User can be any role
    options.AddPolicy("AnyRole", policy =>
        policy.RequireRole("Admin", "Creator", "Viewer"));
});

// Swagger configuration with Bearer token support
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// HTTP Context accessor for extracting user claims
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "QuizSystem API V1");
});

// SECURITY: Order matters! Authentication must come before Authorization
app.UseAuthentication();   // Validates JWT token
app.UseAuthorization();    // Checks [Authorize] attributes

// Exception handling middleware must be near the top
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
