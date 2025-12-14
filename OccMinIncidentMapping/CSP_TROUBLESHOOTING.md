# CSP Troubleshooting Guide

## Common CSP Violations and Solutions

### Issue: "Refused to load the script from 'https://external-cdn.com/script.js'"

**Cause:** Script is from an external origin not in `script-src` directive

**Solution 1: Host locally**
```bash
# Download the script
curl -o src/js/external-script.js https://external-cdn.com/script.js

# Reference locally
<script src="/js/external-script.js"></script>
```

**Solution 2: Add to CSP exception**
```csharp
script-src 'self' https://external-cdn.com;
```

### Issue: "Refused to apply inline style"

**Cause:** Inline `style="..."` attributes are blocked

**Solution: Move to external CSS**

? Before (Blocked):
```html
<div style="color: red; font-size: 16px;">Text</div>
```

? After (Allowed):
```html
<!-- In HTML -->
<div class="custom-text">Text</div>

<!-- In styles.css -->
.custom-text {
    color: red;
    font-size: 16px;
}
```

### Issue: "Refused to execute inline script"

**Cause:** Inline `<script>` tags are blocked

**Solution: Move to external JavaScript file**

? Before (Blocked):
```html
<script>
    console.log('Hello World');
    document.getElementById('btn').addEventListener('click', () => {
        alert('Clicked!');
    });
</script>
```

? After (Allowed):
```html
<!-- In HTML -->
<script src="/js/app.js"></script>

<!-- In app.js -->
console.log('Hello World');
document.getElementById('btn').addEventListener('click', () => {
    alert('Clicked!');
});
```

### Issue: "Refused to connect to 'https://external-api.com/data'"

**Cause:** API call to external origin not in `connect-src` directive

**Solution 1: Use your own backend as proxy**
```csharp
// In your API controller
[HttpGet("api/proxy/external-data")]
public async Task<IActionResult> GetExternalData()
{
    using var httpClient = new HttpClient();
    var response = await httpClient.GetAsync("https://external-api.com/data");
    var content = await response.Content.ReadAsStringAsync();
    return Ok(content);
}
```

Then call your API from frontend:
```javascript
fetch('/api/proxy/external-data')
    .then(response => response.json())
    .then(data => console.log(data));
```

**Solution 2: Add to CSP exception**
```csharp
connect-src 'self' https://external-api.com;
```

### Issue: "Refused to load font from 'https://fonts.gstatic.com/...'"

**Cause:** Font from external origin not in `font-src` directive

**Solution 1: Host fonts locally**
```bash
# Download Google Fonts locally
# Then reference: <link rel="stylesheet" href="/fonts/google-fonts.css">
```

**Solution 2: Add to CSP exception**
```csharp
font-src 'self' https://fonts.gstatic.com;
style-src 'self' https://fonts.googleapis.com;
```

### Issue: "Refused to load image from 'http://example.com/image.jpg'"

**Cause:** Image is from HTTP (not HTTPS)

**Solution 1: Use HTTPS URL**
```html
<!-- Before (HTTP - blocked) -->
<img src="http://example.com/image.jpg" />

<!-- After (HTTPS - allowed) -->
<img src="https://example.com/image.jpg" />
```

**Solution 2: Host image locally**
```html
<img src="/images/my-image.jpg" />
```

### Issue: "Refused to load stylesheet from 'https://cdn.example.com/style.css'"

**Cause:** Stylesheet from external origin not in `style-src` directive

**Solution:** (Same as external scripts)

## Using Nonce for Inline Code (Advanced)

If you absolutely need inline scripts/styles, use a nonce (number used once):

```csharp
// In SecurityHeadersMiddleware.cs
public async Task InvokeAsync(HttpContext context)
{
    // Generate random nonce
    var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
    
    // Update CSP to allow nonce
    var cspWithNonce = $"script-src 'self' 'nonce-{nonce}'; " +
                       $"style-src 'self' 'nonce-{nonce}'; " +
                       // ... other directives
    
    context.Response.Headers.Add("Content-Security-Policy", cspWithNonce);
    context.Items["nonce"] = nonce;
    
    await _next(context);
}
```

Then use in HTML:

```html
<!-- Pass nonce from context to view -->
<script nonce="@Context.Items["nonce"]">
    console.log('This is allowed!');
</script>

<style nonce="@Context.Items["nonce"]">
    body { color: blue; }
</style>
```

## Debugging Tools

### 1. Browser DevTools

Open DevTools ? Network ? Headers
Look for `Content-Security-Policy` header:

```
Content-Security-Policy: default-src 'none'; script-src 'self'; ...
```

### 2. CSP Report Endpoint (Recommended)

Implement this endpoint to collect violations:

```csharp
[ApiController]
[Route("api")]
public class SecurityReportController : ControllerBase
{
    private readonly ILogger<SecurityReportController> _logger;
    
    public SecurityReportController(ILogger<SecurityReportController> logger)
    {
        _logger = logger;
    }
    
    [HttpPost("csp-report")]
    public async Task<IActionResult> ReportCspViolation([FromBody] CspViolation report)
    {
        _logger.LogWarning(
            "CSP Violation: Directive={Directive}, BlockedUri={BlockedUri}, DocumentUri={DocumentUri}",
            report.ViolatedDirective,
            report.BlockedUri,
            report.DocumentUri);
        
        // Log to security monitoring system
        // Alert on suspicious patterns
        // Store for analysis
        
        return NoContent();
    }
}

public class CspViolation
{
    [JsonPropertyName("document-uri")]
    public string DocumentUri { get; set; }
    
    [JsonPropertyName("violated-directive")]
    public string ViolatedDirective { get; set; }
    
    [JsonPropertyName("blocked-uri")]
    public string BlockedUri { get; set; }
    
    [JsonPropertyName("original-policy")]
    public string OriginalPolicy { get; set; }
    
    [JsonPropertyName("disposition")]
    public string Disposition { get; set; }
}
```

Then enable CSP reporting in middleware:

```csharp
context.Response.Headers.Add("Content-Security-Policy-Report-Only",
    ContentSecurityPolicy + "; report-uri /api/csp-report");
```

### 3. CSP Evaluator Tool

Visit: https://csp-evaluator.withgoogle.com/
- Paste your CSP header
- Get security assessment
- View recommendations

### 4. Mozilla Observatory

Visit: https://observatory.mozilla.org/
- Scan your website
- Get overall security score
- View CSP recommendations

## Gradually Rolling Out CSP

### Phase 1: Report-Only Mode (Week 1-2)
```csharp
// Deploy with report-only, no enforcement
Content-Security-Policy-Report-Only: ...
```

? Advantages:
- Nothing breaks
- Collect real-world violations
- Understand application dependencies

### Phase 2: Permissive CSP (Week 3-4)
```csharp
// Enforce but with broad allowances
default-src 'self' https:; script-src 'self' 'unsafe-inline'; ...
```

? Advantages:
- Starting enforcement
- Gradual tightening
- Time to fix issues

### Phase 3: Strict CSP (Week 5+)
```csharp
// Current: No 'unsafe-inline', restrictive defaults
default-src 'none'; script-src 'self'; ...
```

? Advantages:
- Full security benefits
- Protects against injection attacks
- Demonstrates security maturity

## Common Mistakes

### ? Mistake 1: Using `script-src *`
```csharp
// DON'T DO THIS
script-src * 'unsafe-inline';
```

### ? Correct: Be specific
```csharp
// DO THIS
script-src 'self' https://trusted-cdn.com;
```

### ? Mistake 2: Setting default-src too broad
```csharp
// DON'T DO THIS
default-src *;
```

### ? Correct: Use whitelist approach
```csharp
// DO THIS
default-src 'none';
script-src 'self';
style-src 'self';
// ... etc
```

### ? Mistake 3: Ignoring CSP violations
```csharp
// DON'T DO THIS - using report-only indefinitely
Content-Security-Policy-Report-Only: ...
```

### ? Correct: Enforce and monitor
```csharp
// DO THIS
Content-Security-Policy: ...
// Also collect reports for monitoring
```

## Performance Considerations

? **CSP Benefits:**
- Smaller page size (no inline scripts)
- Better caching (external files are cached)
- Faster subsequent loads

?? **CSP Costs:**
- Additional HTTP requests for external resources
- More file downloads (unbundled)
- Slightly increased CPU for policy evaluation

**Optimization Tips:**
1. Bundle CSS/JS files
2. Use HTTP/2 server push
3. Enable compression (gzip)
4. Use CDN for static assets
5. Implement caching headers

## Security Best Practices

? **DO:**
- Keep CSP as restrictive as possible
- Regularly audit and update
- Monitor violations in production
- Test new features against CSP
- Document CSP exceptions

? **DON'T:**
- Use `'unsafe-inline'`
- Use `'unsafe-eval'`
- Allow `script-src *`
- Ignore CSP in development
- Deploy overly permissive CSP to production
