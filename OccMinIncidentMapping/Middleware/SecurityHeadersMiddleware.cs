namespace OccMinIncidentMapping.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ContentSecurityPolicy =
            "default-src 'none'; " +
            "script-src 'self'; " +
            "style-src 'self'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self'; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "form-action 'self'; " +
            "base-uri 'self'; " +
            "upgrade-insecure-requests";

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

            // Restrictive Content Security Policy - mitigates content injection attacks
            context.Response.Headers.Add("Content-Security-Policy", ContentSecurityPolicy);

            // Report CSP violations (optional - for monitoring)
            context.Response.Headers.Add("Content-Security-Policy-Report-Only",
                ContentSecurityPolicy + "; report-uri /api/csp-report");

            // Referrer Policy
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            // Permissions Policy - restricts access to sensitive browser features
            context.Response.Headers.Add("Permissions-Policy",
                "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=(), microphone=()");

            // Strict Transport Security (HSTS)
            if (!context.Request.IsHttps && !IsLocalhost(context))
            {
                context.Response.Headers.Add("Strict-Transport-Security",
                    "max-age=31536000; includeSubDomains; preload");
            }

            await _next(context);
        }

        private static bool IsLocalhost(HttpContext context)
        {
            return context.Request.Host.Host == "localhost" ||
                   context.Request.Host.Host == "127.0.0.1" ||
                   context.Request.Host.Host == "::1";
        }
    }
}
