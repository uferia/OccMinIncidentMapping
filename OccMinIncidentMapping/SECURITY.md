# Security Implementation Guide

## Overview
This document outlines all security measures implemented in the OccMin Incident Mapping application.

## 1. Authentication & Authorization

### JWT (JSON Web Token) Authentication
- **Implementation**: JWT Bearer tokens with HS256 signing algorithm
- **Location**: `/api/auth/login` endpoint
- **Usage**: 
  - Client sends credentials to login endpoint
  - Server returns JWT token valid for 60 minutes (configurable)
  - Client includes token in `Authorization: Bearer {token}` header for subsequent requests

### Protected Endpoints
All incident management endpoints require authentication:
- `POST /api/incidents` - Create incident (requires `[Authorize]`)
- `GET /api/incidents` - Get all incidents (requires `[Authorize]`)

### Login Credentials (Development Only)
- Admin user: `username: admin` / `password: admin123`
- Regular user: `username: user` / `password: user123`

**?? WARNING**: Replace with actual user database in production!

## 2. Exception Handling & Information Disclosure

### Secure Exception Middleware
- **Development**: Shows detailed error messages for debugging
- **Production**: Shows generic error messages (no sensitive information)
- **Location**: `OccMinIncidentMapping\Middleware\ExceptionMiddleware.cs`

#### Error Response Examples
**Development:**
```json
{
  "error": "Internal server error",
  "details": "Actual error message here"
}
```

**Production:**
```json
{
  "error": "Internal server error",
  "details": "An unexpected error occurred"
}
```

## 3. Security Headers

### Implemented Headers
All responses include the following security headers:

| Header | Value | Purpose |
|--------|-------|---------|
| `X-Frame-Options` | DENY | Prevents clickjacking attacks |
| `X-Content-Type-Options` | nosniff | Prevents MIME type sniffing |
| `X-XSS-Protection` | 1; mode=block | Enables XSS protection |
| `Content-Security-Policy` | default-src 'self' | Restricts resource loading |
| `Referrer-Policy` | strict-origin-when-cross-origin | Controls referrer information |
| `Permissions-Policy` | (various) | Restricts browser features |

**Location**: `OccMinIncidentMapping\Middleware\SecurityHeadersMiddleware.cs`

## 4. Audit Logging

### What's Logged
- Username and timestamp of all requests
- HTTP method and endpoint path
- Response status code

### Log Format
```
Audit: {Username} performed {Method} on {Path} at {Timestamp}
Audit: {Username} - Response status {StatusCode} for {METHOD} {Path}
```

**Location**: `OccMinIncidentMapping\Middleware\AuditLoggingMiddleware.cs`

## 5. Input Validation

### Server-Side Validation
All inputs are validated using FluentValidation:

**Login Command:**
- Username: Required, 3-50 characters
- Password: Required, minimum 6 characters

**Incident Creation:**
- Latitude: Between -90 and 90
- Longitude: Between -180 and 180
- Hazard: Must be valid enum value
- Status: Must be valid enum value
- Description: Maximum 500 characters

## 6. Request Size Limits

### Configured Limits
- Maximum request body size: **10 MB**
- Applies to both IIS and Kestrel

**Configuration Location**: `OccMinIncidentMapping\Program.cs`

## 7. CORS (Cross-Origin Resource Sharing)

### Configuration
- **Development**: Allows `http://localhost:4200` and `https://localhost:4200`
- **Production**: Restricted to configured domain only
- **Credentials**: Enabled (required for cookies/auth headers)

### Configuration File
```json
"Cors": {
  "AllowedOrigins": [ "http://localhost:4200", "https://localhost:4200" ]
}
```

**Location**: `appsettings.json`

## 8. HTTPS Enforcement

### Development
- Optional HTTPS (allows both HTTP and HTTPS)
- Swagger UI available for testing

### Production
- HTTPS redirect enabled
- Swagger UI disabled
- Strict certificate validation

## 9. Swagger/OpenAPI Security

### Access Control
- **Development**: Available at `/swagger/index.html`
- **Production**: Disabled (not available)

**Configuration Location**: `OccMinIncidentMapping\Program.cs`

## 10. Configuration Management

### Environment-Specific Settings

**Development** (`appsettings.Development.json`):
- Verbose logging
- Longer token expiry (60 minutes)
- Localhost origins allowed

**Production** (`appsettings.Production.json`):
- Minimal logging (warnings only)
- Shorter token expiry (30 minutes)
- Restricted origins
- Custom domain configuration

## Security Checklist for Deployment

- [ ] Update JWT `SecretKey` to a long random string (minimum 32 characters)
- [ ] Change default credentials (admin/user) to actual user database
- [ ] Update `AllowedOrigins` CORS settings for production domain
- [ ] Update `AllowedHosts` to your domain
- [ ] Update Firebase ProjectId for production
- [ ] Enable HTTPS with valid SSL certificate
- [ ] Review and adjust token expiry time as needed
- [ ] Set up proper logging aggregation (ELK, Azure Monitor, etc.)
- [ ] Implement rate limiting (future enhancement)
- [ ] Set up Web Application Firewall (WAF)
- [ ] Enable CORS only for required domains
- [ ] Review Firebase Firestore security rules

## Rate Limiting (Future Enhancement)

Consider implementing rate limiting for:
- Login endpoint (brute force protection)
- General API endpoints (DoS prevention)

## Database Security (Firebase Firestore)

### Recommended Firestore Rules
```
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /incidents/{document=**} {
      allow read: if request.auth != null;
      allow create: if request.auth != null && request.resource.data.keys().hasAll(['latitude', 'longitude', 'hazard', 'status']);
      allow update: if request.auth != null && request.resource.data.latitude == resource.data.latitude;
      allow delete: if request.auth.token.email_verified && request.auth.token.role == 'admin';
    }
  }
}
```

## Sensitive Data Protection

### Firebase Credentials
- `firebase.json` is not committed to version control
- Credentials stored in environment variables or secret management system
- Consider using Google Cloud Secret Manager in production

### Password Handling
- Currently using plain text (demonstration only)
- **Production**: Implement bcrypt or PBKDF2 hashing
- **Recommendation**: Use ASP.NET Identity or similar framework

## Security Testing

### Test Credentials
```
# Login with admin
curl -X POST https://localhost:7061/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'

# Use returned token
curl https://localhost:7061/api/incidents \
  -H "Authorization: Bearer {token}"
```

## References

- [OWASP Top 10 2021](https://owasp.org/Top10/)
- [Microsoft Security Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [Content Security Policy](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)

## Contact & Support

For security issues, please report them privately to the project maintainers.

**DO NOT** create public security issues on GitHub.
