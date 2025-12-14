# JWT & Authentication Industry Standards Implementation

## Overview
This document outlines the authentication and JWT implementation following OWASP, OAuth2, and industry best practices.

---

## 1. JWT Standards Compliance

### Token Structure
- **Algorithm**: HMAC SHA-256 (HS256)
- **Issuer (iss)**: OccMinIncidentMapping
- **Audience (aud)**: OccMinIncidentMappingClient
- **Expiration (exp)**: 60 minutes (configurable)
- **Unique Token ID (jti)**: Prevents token reuse
- **Issued At (iat)**: Token issuance timestamp

### Custom Claims
- `NameIdentifier`: Username
- `Name`: Username
- `Role`: User role (Admin, User)

---

## 2. Password Security (CRITICAL)

### Current Status
- **Algorithm**: PBKDF2-SHA256
- **Iterations**: 10,000
- **Salt Size**: 32 bytes (256-bit)
- **Hash Size**: 32 bytes (256-bit)

### Implementation
```csharp
// Password hashing (one-way)
string hash = passwordHasher.Hash("userPassword");

// Password verification (constant-time comparison)
bool isValid = passwordHasher.Verify("userPassword", storedHash);
```

### Storage Format
```
{iterations}:{base64Salt}:{base64Hash}
10000:4d++base64salthere==:4d++base64hashhere==
```

---

## 3. Password Policy Requirements

### Minimum Standards
- **Length**: Minimum 8 characters
- **Maximum**: 128 characters
- **Allowed**: Alphanumeric, special characters
- **No expiration**: Not enforced (modern best practice)

### Future Enhancements
```json
{
  "MinimumLength": 8,
  "RequireUppercase": true,
  "RequireLowercase": true,
  "RequireDigits": true,
  "RequireSpecialCharacters": false
}
```

---

## 4. OAuth2 Compliance

### Login Response Format
```json
{
  "access_token": "eyJ0eXAiOiJKV1QiLCJhbGc...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "username": "admin",
  "role": "Admin"
}
```

### Error Response Format
```json
{
  "error": "invalid_credentials",
  "error_description": "Invalid username or password",
  "details": []
}
```

---

## 5. Security Headers

### Required Headers
```
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Content-Security-Policy: default-src 'self'
```

---

## 6. Input Validation

### Username Validation
- **Pattern**: `^[a-zA-Z0-9._@-]+$`
- **Min Length**: 3 characters
- **Max Length**: 50 characters

### Password Validation
- **Min Length**: 8 characters
- **Max Length**: 128 characters

---

## 7. Rate Limiting (Recommended)

### Configuration
```json
{
  "RateLimiting": {
    "Enabled": true,
    "MaxLoginAttemptsPerMinute": 5,
    "LockoutDurationMinutes": 15
  }
}
```

---

## 8. HTTPS Requirements

### Production
- **HTTPS Only**: Enforced
- **HSTS**: Enabled (max-age=31536000)
- **SSL/TLS Version**: TLS 1.2 or higher

### Development
- HTTP allowed for localhost testing

---

## 9. Token Validation in API

### Implementation
```csharp
[Authorize]
[HttpGet("api/incidents")]
public async Task<IActionResult> GetIncidents()
{
    // Token automatically validated by middleware
    var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var role = User.FindFirst(ClaimTypes.Role)?.Value;
    
    return Ok(await _mediator.Send(new GetAllIncidentsQuery()));
}
```

---

## 10. Database Integration (TODO)

### Current Status
Placeholder password storage in code (development only).

### Production Implementation
1. **User Table Structure**
   ```sql
   CREATE TABLE Users (
       Id INT PRIMARY KEY,
       Username NVARCHAR(50) UNIQUE NOT NULL,
       PasswordHash NVARCHAR(MAX) NOT NULL,
       Role NVARCHAR(20) NOT NULL,
       IsActive BIT NOT NULL DEFAULT 1,
       LastLogin DATETIME,
       CreatedAt DATETIME,
       UpdatedAt DATETIME
   );
   ```

2. **Password Storage**
   ```csharp
   var hash = passwordHasher.Hash(password);
   user.PasswordHash = hash; // Store hash, NEVER plaintext
   ```

3. **Login Flow**
   ```csharp
   var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
   if (user != null && passwordHasher.Verify(password, user.PasswordHash))
   {
       // Generate token
   }
   ```

---

## 11. Logging & Monitoring

### Security Events to Log
- ? Successful login
- ? Failed login attempts
- ? Token generation
- ? Validation errors
- ? Unauthorized access attempts

### Log Redaction
```csharp
_logger.LogWarning("Failed login for user: {Username}", username);
// NEVER log passwords or tokens
```

---

## 12. Security Checklist

- [x] Password hashing with PBKDF2-SHA256
- [x] JWT with unique token IDs (jti)
- [x] 8+ character minimum password length
- [x] Username pattern validation
- [x] OAuth2 error response format
- [x] HTTPS enforcement
- [x] Security headers middleware
- [x] Input validation with FluentValidation
- [x] Comprehensive error logging
- [ ] Rate limiting implementation
- [ ] Database user store
- [ ] Multi-factor authentication
- [ ] Token refresh mechanism
- [ ] CORS configuration
- [ ] JWT blacklist/revocation

---

## 13. References

- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [OAuth2 RFC 6749](https://tools.ietf.org/html/rfc6749)
- [JWT RFC 7519](https://tools.ietf.org/html/rfc7519)
- [NIST Password Guidelines SP 800-63B](https://pages.nist.gov/800-63-3/sp800-63b.html)

---

## 14. Migration Path

### Phase 1 (Current)
- PBKDF2 password hashing
- Basic JWT implementation
- FluentValidation

### Phase 2 (Recommended)
- Implement rate limiting
- Add database user store
- Token refresh mechanism

### Phase 3 (Advanced)
- Multi-factor authentication
- OAuth2/OIDC provider integration
- Hardware security key support
