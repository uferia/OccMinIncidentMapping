using System.Security.Claims;

namespace OccMinIncidentMapping.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLoggingMiddleware> _logger;

        public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var username = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? "Unknown";

            // Log the incoming request
            _logger.LogInformation(
                "Audit: {Username} performed {Method} on {Path} at {Timestamp}",
                username, method, path, DateTime.UtcNow);

            // Call the next middleware
            await _next(context);

            // Log the response
            _logger.LogInformation(
                "Audit: {Username} - Response status {StatusCode} for {Method} {Path}",
                username, context.Response.StatusCode, method, path);
        }
    }
}
