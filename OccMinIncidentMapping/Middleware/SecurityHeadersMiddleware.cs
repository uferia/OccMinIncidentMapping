namespace OccMinIncidentMapping.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Prevent clickjacking attacks
            context.Response.Headers.Add("X-Frame-Options", "DENY");

            // Prevent MIME type sniffing
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

            // Enable XSS protection
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

            // Content Security Policy
            context.Response.Headers.Add("Content-Security-Policy",
                "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:;");

            // Referrer Policy
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            // Permissions Policy
            context.Response.Headers.Add("Permissions-Policy",
                "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

            await _next(context);
        }
    }
}
