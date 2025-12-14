using FluentValidation;
using System.Net;
using System.Text.Json;

namespace OccMinIncidentMapping.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex) // FluentValidation's exception
            {
                _logger.LogWarning(ex, "Validation error occurred");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                // Return structured error details
                var errors = ex.Errors.Select(e => new {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                });

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = "Validation failed",
                    details = errors
                }));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = "Unauthorized",
                    details = "Invalid credentials or access denied"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                // Only expose detailed error messages in development
                var errorResponse = _environment.IsDevelopment()
                    ? new { error = "Internal server error", details = ex.Message }
                    : new { error = "Internal server error", details = "An unexpected error occurred" };

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            }
        }
    }
}
