# Content Security Policy (CSP) Implementation

## Overview

This application implements a restrictive Content Security Policy to mitigate content injection attacks (XSS, clickjacking, etc.). The CSP is enforced via the `SecurityHeadersMiddleware` class.

## Current CSP Directives

```
default-src 'none';
script-src 'self';
style-src 'self';
img-src 'self' data: https:;
font-src 'self';
connect-src 'self';
frame-ancestors 'none';
form-action 'self';
base-uri 'self';
upgrade-insecure-requests
```

## Directive Explanations

### default-src 'none'
- **Blocks all content by default** unless explicitly allowed by other directives
- Most restrictive approach - requires explicit whitelisting for each resource type
- Prevents any unexpected content from loading

### script-src 'self'
- **Only allows scripts from the same origin**
- **NO `'unsafe-inline'` or `'unsafe-eval'`** - prevents inline script injection
- External scripts are blocked unless explicitly whitelisted
- **Mitigation**: Prevents malicious inline scripts from executing

### style-src 'self'
- **Only allows stylesheets from the same origin**
- **NO `'unsafe-inline'`** - prevents style injection attacks
- External CSS is blocked unless explicitly whitelisted
- **Mitigation**: Prevents CSS-based attacks (e.g., exfiltration via CSS)

### img-src 'self' data: https:
- **Allows images from the same origin**
- **Allows data URIs** (embedded images)
- **Allows HTTPS images** from any source
- **Blocks HTTP images** (forces HTTPS)
- **Mitigation**: Prevents loading of malicious images, enforces encrypted transport

### font-src 'self'
- **Only allows fonts from the same origin**
- Prevents loading fonts from untrusted CDNs
- **Mitigation**: Prevents font-based attacks and reduces third-party dependencies

### connect-src 'self'
- **Only allows connections (fetch, XHR, WebSocket, etc.) to the same origin**
- API calls must go to your own server
- **Mitigation**: Prevents CSRF and data exfiltration to external servers

### frame-ancestors 'none'
- **Prevents the application from being embedded in any frame**
- Blocks clickjacking attacks even if X-Frame-Options is bypassed
- **Mitigation**: Prevents framing attacks and clickjacking

### form-action 'self'
- **Form submissions are restricted to the same origin**
- Prevents forms from submitting to external servers
- **Mitigation**: Prevents credential theft via form hijacking

### base-uri 'self'
- **Prevents changing the base URL of the document**
- Blocks `<base>` tag injection attacks
- **Mitigation**: Prevents relative URL manipulation

### upgrade-insecure-requests
- **Automatically upgrades HTTP requests to HTTPS**
- If a resource URL is loaded via HTTP, it's upgraded to HTTPS
- **Mitigation**: Prevents man-in-the-middle attacks

## Additional Security Headers

### X-Frame-Options: DENY
- Prevents clickjacking by forbidding any iframe embedding
- Redundant with `frame-ancestors` but provides fallback for older browsers

### X-Content-Type-Options: nosniff
- Prevents browsers from MIME-type sniffing
- Forces the browser to respect the Content-Type header
- **Mitigation**: Prevents MIME-type confusion attacks

### X-XSS-Protection: 1; mode=block
- Enables browser XSS protection and blocks the page if XSS is detected
- Provides fallback protection for older browsers

### Referrer-Policy: strict-origin-when-cross-origin
- Only sends referrer information when navigating to same-origin
- Sends only the origin (not the full URL) for cross-origin requests
- **Mitigation**: Prevents sensitive URL data from leaking to third parties

### Permissions-Policy
- Disables access to sensitive browser features:
  - Camera, microphone, geolocation
  - Payment APIs, USB access
  - Accelerometer, gyroscope, magnetometer
- **Mitigation**: Prevents unauthorized access to device features

### Strict-Transport-Security (HSTS)
- Forces HTTPS for all subsequent requests
- `max-age=31536000` (1 year) - aggressive cache duration
- `includeSubDomains` - applies to all subdomains
- `preload` - eligible for browser HSTS preload list
- **Mitigation**: Prevents downgrade attacks and SSL stripping

## CSP Report-Only Mode

The application also sends a `Content-Security-Policy-Report-Only` header for monitoring. This allows you to:
1. Test your CSP without breaking anything
2. Collect reports of violations
3. Gradually transition to a stricter policy

**Note:** To enable CSP reporting, implement a `/api/csp-report` endpoint to collect violations.

## Adding Exceptions

If you need to allow content from external sources, modify the CSP directives:

### Example: Allow Google Fonts
```csharp
style-src 'self' https://fonts.googleapis.com;
font-src 'self' https://fonts.gstatic.com;
```

### Example: Allow Bootstrap CDN
```csharp
style-src 'self' https://cdn.jsdelivr.net/npm/bootstrap@5/dist/css/bootstrap.min.css;
script-src 'self' https://cdn.jsdelivr.net/npm/bootstrap@5/dist/js/bootstrap.bundle.min.js;
```

### Example: Allow API Gateway
```csharp
connect-src 'self' https://api.example.com;
```

## Testing CSP

### 1. Use Browser DevTools
- Open DevTools ? Console
- Look for CSP violation messages (red warnings)
- These indicate resources being blocked by CSP

### 2. Check Response Headers
```bash
curl -I https://yourapp.com
# Look for Content-Security-Policy header
```

### 3. Implement CSP Report Endpoint
Add this endpoint to receive CSP violation reports:

```csharp
[HttpPost("api/csp-report")]
public async Task<IActionResult> ReportCspViolation([FromBody] CspViolationReport report)
{
    _logger.LogWarning($"CSP Violation: {report.ViolatedDirective} - {report.BlockedUri}");
    return Ok();
}

public class CspViolationReport
{
    [JsonPropertyName("document-uri")]
    public string DocumentUri { get; set; }
    
    [JsonPropertyName("violated-directive")]
    public string ViolatedDirective { get; set; }
    
    [JsonPropertyName("blocked-uri")]
    public string BlockedUri { get; set; }
}
```

## Best Practices

? **DO:**
- Keep CSP as restrictive as possible
- Regularly review and test your CSP
- Use nonces for inline scripts (if absolutely necessary)
- Monitor CSP violations in production
- Update CSP as your application evolves

? **DON'T:**
- Use `'unsafe-inline'` for scripts
- Use `'unsafe-eval'`
- Allow `script-src *` or `style-src *`
- Ignore CSP violations in production
- Keep overly permissive CSP in production

## Production Recommendations

1. **Start with Report-Only Mode**
   - Deploy CSP-Report-Only first
   - Monitor violations for 1-2 weeks
   - Fix legitimate issues

2. **Gradually Tighten**
   - Remove report-only header
   - Deploy strict CSP
   - Continue monitoring

3. **Use Nonces for Inline Code**
   - If you need inline scripts/styles, use nonces
   - Example: `<script nonce="random-value">...</script>`

4. **Monitor Violations**
   - Implement CSP report endpoint
   - Alert on suspicious patterns
   - Investigate and fix violations

## Resources

- [MDN: Content Security Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)
- [CSP Reference](https://content-security-policy.com/)
- [OWASP: Content Security Policy](https://cheatsheetseries.owasp.org/cheatsheets/Content_Security_Policy_Cheat_Sheet.html)
- [CSP Evaluator Tool](https://csp-evaluator.withgoogle.com/)

## Migration Path

If you currently have:
```
style-src 'self' 'unsafe-inline'
script-src 'self' 'unsafe-inline'
```

### Step 1: Move inline styles to external stylesheets
- Create separate `.css` files
- Import them using `<link>` tags
- Remove `style="..."` from HTML

### Step 2: Move inline scripts to external files
- Create separate `.js` files
- Import them using `<script>` tags
- Remove `<script>alert()...</script>` inline code

### Step 3: Use bundlers
- Webpack, Vite, or Angular CLI handle this automatically
- Minifies and bundles code
- Separates concerns

### Step 4: Test thoroughly
- Run security scans
- Test all functionality
- Check browser console for violations

## Compliance

This CSP implementation helps meet:
- ? OWASP Top 10 (A03:2021 – Injection)
- ? NIST Cybersecurity Framework
- ? CIS Controls
- ? PCI DSS (if handling payments)
