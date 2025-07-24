using FluentValidation;
using System.Net;
using System.Text.Json;

namespace OccMinIncidentMapping.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex) // FluentValidation's exception
            {
                _logger.LogWarning(ex, "Validation error");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = "Internal server error",
                    details = ex.Message
                }));
            }
        }
    }
}
