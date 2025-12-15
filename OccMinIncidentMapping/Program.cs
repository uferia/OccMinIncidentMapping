using System.Text;
using Application.Common;
using Core.Features.Auth.Commands;
using Core.Features.Incidents.Commands;
using Infrastructure.Extensions;
using MediatR;
using OccMinIncidentMapping.Extensions;
using OccMinIncidentMapping.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Google Cloud Secret Manager for production/GCP environments
if (!builder.Environment.IsDevelopment())
{
    var projectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID");
    if (!string.IsNullOrEmpty(projectId))
    {
        // Specify the exact secrets to load from GCP Secret Manager
        var secretNames = new[]
        {
            "jwt-signing-key",      // Maps to Jwt:SigningKey
            "Firebase-ProjectId",
            "Firebase-PrivateKey",
            "Firebase-ClientEmail"
        };
        builder.Configuration.AddGoogleCloudSecrets(projectId, secretNames);
    }
}

// Add secure JWT authentication (secrets from environment variables, user secrets, or GCP Secret Manager)
builder.Services.AddSecureJwtAuthentication(builder.Configuration, builder.Environment);

builder.Services.AddAuthorization();

// Add services to the container
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(
        typeof(CreateIncidentCommand).Assembly,
        typeof(LoginCommand).Assembly,
        typeof(Program).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Configure request size limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 10485760; // 10 MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10485760; // 10 MB
});

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        // Configure enum serialization
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        
        // Configure null handling
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        
        // Configure reference loop handling
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT Bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
            new string[] { }
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCORS", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // In production, redirect HTTP to HTTPS
    app.UseHttpsRedirection();
}

// Add middleware in the correct order
app.UseSecurityHeaders();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();
app.UseCors("AllowCORS");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
