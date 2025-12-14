# Content Security Policy Security Hardening

## Summary of Changes

The Content Security Policy (CSP) has been significantly hardened to mitigate content injection attacks (XSS, clickjacking, CSS injection, etc.).

## Previous CSP (Vulnerable)
```
default-src 'self'
script-src 'self' 'unsafe-inline'
style-src 'self' 'unsafe-inline'
img-src 'self' data: https:
```

**Issues with previous policy:**
- ? `'unsafe-inline'` allows arbitrary inline JavaScript execution (XSS vulnerability)
- ? `'unsafe-inline'` allows arbitrary CSS injection (style-based attacks)
- ? Missing important directives like `frame-ancestors`, `form-action`, `base-uri`
- ? No protection against MIME-type confusion
- ? No HTTP ? HTTPS upgrade mechanism

## New CSP (Hardened)
```
default-src 'none'
script-src 'self'
style-src 'self'
img-src 'self' data: https:
font-src 'self'
connect-src 'self'
frame-ancestors 'none'
form-action 'self'
base-uri 'self'
upgrade-insecure-requests
```

**Security improvements:**
- ? `default-src 'none'` - Blocks all content by default (whitelist approach)
- ? Removed `'unsafe-inline'` from both scripts and styles
- ? Added `frame-ancestors 'none'` - Blocks clickjacking
- ? Added `form-action 'self'` - Prevents credential theft via form hijacking
- ? Added `base-uri 'self'` - Prevents base URL manipulation
- ? Added `font-src 'self'` - Prevents font-based attacks
- ? Added `connect-src 'self'` - Restricts API calls to same origin
- ? Added `upgrade-insecure-requests` - Automatic HTTP ? HTTPS upgrade

## Attack Mitigations

### 1. **Cross-Site Scripting (XSS)**
- **Before:** Inline scripts could execute, allowing XSS attacks
- **After:** Inline scripts are blocked; only scripts from same origin allowed
- **Mitigation strength:** ?? HIGH

### 2. **Content Injection / Style-Based Attacks**
- **Before:** Inline styles could manipulate page appearance, enable CSS-based attacks
- **After:** Only same-origin stylesheets allowed
- **Mitigation strength:** ?? HIGH

### 3. **Clickjacking / Framing Attacks**
- **Before:** No `frame-ancestors` directive; only X-Frame-Options fallback
- **After:** `frame-ancestors 'none'` prevents embedding in any frame
- **Mitigation strength:** ?? HIGH

### 4. **Form Hijacking / Credential Theft**
- **Before:** No restriction on form submission targets
- **After:** `form-action 'self'` restricts form submission to same origin
- **Mitigation strength:** ?? HIGH

### 5. **URL Manipulation via Base Tag**
- **Before:** No protection against `<base>` tag injection
- **After:** `base-uri 'self'` prevents base URL changes
- **Mitigation strength:** ?? MEDIUM

### 6. **Data Exfiltration**
- **Before:** No restriction on external connections
- **After:** `connect-src 'self'` blocks all external API calls/data exfiltration
- **Mitigation strength:** ?? HIGH

### 7. **Man-in-the-Middle (MITM) / SSL Stripping**
- **Before:** No enforcement of HTTPS
- **After:** `upgrade-insecure-requests` automatically upgrades HTTP to HTTPS
- **Mitigation strength:** ?? HIGH

### 8. **Third-Party Library Vulnerabilities**
- **Before:** Could load scripts from CDNs
- **After:** Only same-origin scripts allowed
- **Mitigation strength:** ?? MEDIUM

## Security Headers Added/Enhanced

| Header | Value | Purpose |
|--------|-------|---------|
| **Content-Security-Policy** | (restrictive policy) | Enforces CSP |
| **Content-Security-Policy-Report-Only** | (same policy) | Monitors violations |
| **X-Frame-Options** | DENY | Prevents clickjacking (fallback) |
| **X-Content-Type-Options** | nosniff | Prevents MIME-type sniffing |
| **X-XSS-Protection** | 1; mode=block | Enables browser XSS protection |
| **Referrer-Policy** | strict-origin-when-cross-origin | Prevents URL leakage |
| **Permissions-Policy** | (restrictive) | Disables sensitive features |
| **Strict-Transport-Security** | max-age=31536000; includeSubDomains; preload | Enforces HTTPS |

## Migration Impact

### If Your Application Uses:

**External CDN Scripts** (e.g., Bootstrap JS, jQuery)
```
? Will be blocked
? Solution: Host locally or add to CSP exception
```

**External Stylesheets** (e.g., Bootstrap CSS, Google Fonts)
```
? Will be blocked
? Solution: Host locally or add to CSP exception
```

**Inline Scripts** (e.g., `<script>alert('hello')</script>`)
```
? Will be blocked
? Solution: Move to external `.js` files
```

**Inline Styles** (e.g., `style="color: red"`)
```
? Will be blocked
? Solution: Move to external `.css` files
```

## Adding Exceptions

If you need to allow specific external resources, modify `SecurityHeadersMiddleware.cs`:

### Example: Allow Google Fonts
```csharp
private const string ContentSecurityPolicy = 
    "default-src 'none'; " +
    "script-src 'self'; " +
    "style-src 'self' https://fonts.googleapis.com; " +
    "font-src 'self' https://fonts.gstatic.com; " +
    // ... other directives
```

### Example: Allow Bootstrap CDN
```csharp
private const string ContentSecurityPolicy = 
    "default-src 'none'; " +
    "script-src 'self' https://cdn.jsdelivr.net/npm/bootstrap@5; " +
    "style-src 'self' https://cdn.jsdelivr.net/npm/bootstrap@5; " +
    // ... other directives
```

### Example: Allow Your API Gateway
```csharp
private const string ContentSecurityPolicy = 
    "default-src 'none'; " +
    "connect-src 'self' https://api.example.com; " +
    // ... other directives
```

## Testing the CSP

### 1. **Browser DevTools**
```
Open DevTools (F12) ? Console
Look for red warnings with "Content Security Policy"
These indicate resources being blocked
```

### 2. **curl Command**
```bash
curl -I https://yourapp.com
# Look for Content-Security-Policy header
```

### 3. **CSP Evaluator**
Visit: https://csp-evaluator.withgoogle.com/
Paste your CSP header to get security recommendations

## Compliance

This implementation helps achieve compliance with:
- ? **OWASP Top 10 (A03:2021 – Injection)**
- ? **NIST Cybersecurity Framework**
- ? **CIS Controls**
- ? **SANS Secure Development**

## Files Modified

1. `OccMinIncidentMapping/Middleware/SecurityHeadersMiddleware.cs` - Updated CSP implementation
2. `OccMinIncidentMapping/CONTENT_SECURITY_POLICY.md` - Comprehensive documentation

## Test Results

? **Build:** Successful
? **All 30 unit tests:** Passing
? **Code Security:** Enhanced

## Recommendations

1. **Monitor CSP Violations**
   - Implement `/api/csp-report` endpoint
   - Log and analyze violations
   - Alert on suspicious patterns

2. **Gradual Rollout**
   - Start with `Content-Security-Policy-Report-Only`
   - Monitor for 1-2 weeks
   - Switch to strict enforcement

3. **Regular Audits**
   - Review CSP quarterly
   - Test with security tools
   - Update as application evolves

4. **Developer Education**
   - Train team on CSP concepts
   - Avoid `'unsafe-inline'`
   - Follow secure coding practices

## Resources

- [MDN: Content Security Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)
- [OWASP CSP Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Content_Security_Policy_Cheat_Sheet.html)
- [CSP Reference](https://content-security-policy.com/)
