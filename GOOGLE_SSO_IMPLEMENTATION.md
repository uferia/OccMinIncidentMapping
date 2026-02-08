# Google SSO Authentication Implementation

## Overview

This project implements **Google Single Sign-On (SSO)** authentication using Google ID tokens and JWT token generation.

## Features

? Google ID Token Validation
? Automatic JWT Token Generation
? User Email Extraction from Google Account
? Role-Based Access Control
? Secure Token Management
? Comprehensive Error Handling

## Architecture

```
???????????????????????????????????????????????????????????
?  Frontend (Browser/Mobile App)                          ?
?  - Google Sign-In SDK                                   ?
?  - Receives ID Token from Google                        ?
???????????????????????????????????????????????????????????
                     ?
                     ? POST /api/auth/google-sso
                     ? { "idToken": "google_token_here" }
                     ?
???????????????????????????????????????????????????????????
?  AuthController.GoogleSso()                             ?
?  - Validates input                                      ?
?  - Sends GoogleSsoCommand via MediatR                   ?
???????????????????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????????????????
?  GoogleSsoCommandHandler.Handle()                       ?
?  - Calls GoogleAuthenticationService                    ?
???????????????????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????????????????
?  GoogleAuthenticationService.VerifyIdTokenAsync()       ?
?  - Validates token with Google's servers               ?
?  - Extracts user information (email, name, picture)    ?
???????????????????????????????????????????????????????????
                     ?
        ???????????????????????????
        ?                         ?
    ? Valid                 ? Invalid
        ?                         ?
        ?                         ?
   Generate JWT             Return 401
   (JwtAuthenticationService)
        ?
        ?
???????????????????????????????????????????????????????????
?  Response to Client                                     ?
?  {                                                      ?
?    "accessToken": "jwt_token_here",                    ?
?    "tokenType": "Bearer",                              ?
?    "expiresIn": 3600,                                  ?
?    "username": "user@gmail.com",                       ?
?    "role": "User"                                      ?
?  }                                                      ?
???????????????????????????????????????????????????????????
```

## Implementation Files

### Core Authentication Services

| File | Purpose |
|------|---------|
| `Core/Features/Auth/Commands/GoogleSsoCommand.cs` | MediatR command definition |
| `Core/Features/Auth/Commands/GoogleSsoCommandHandler.cs` | Command handler with authentication logic |
| `Infrastructure/Services/GoogleAuthenticationService.cs` | Google token validation |
| `Infrastructure/Services/JwtAuthenticationService.cs` | JWT token generation and validation |
| `OccMinIncidentMapping/Controllers/AuthController.cs` | HTTP endpoint |

### Configuration Files

| File | Purpose |
|------|---------|
| `OccMinIncidentMapping/appsettings.json` | Application settings (non-sensitive) |
| User Secrets (stored locally) | Sensitive configuration (signing key, Google Client ID) |

## Configuration

### Required Secrets

Set these using `dotnet user-secrets`:

```powershell
# JWT Signing Key (64 hex characters minimum)
dotnet user-secrets set "Jwt:SigningKey" "<your-secret-key>"

# Google OAuth Client ID
dotnet user-secrets set "Google:ClientId" "<your-google-client-id>"
```

### Required Settings (appsettings.json)

```json
{
  "Jwt": {
    "Issuer": "OccMinIncidentMapping",
    "Audience": "OccMinIncidentMappingClient",
    "ExpiryMinutes": 60
  }
}
```

## API Endpoints

### Google SSO Login

**Endpoint:** `POST /api/auth/google-sso`

**Request:**
```json
{
  "idToken": "google_id_token_from_frontend"
}
```

**Success Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "username": "user@gmail.com",
  "role": "User"
}
```

**Error Responses:**

- `400 Bad Request` - Empty or missing ID token
- `401 Unauthorized` - Invalid or expired Google ID token
- `500 Internal Server Error` - Server error during processing

## Testing

### Prerequisites

1. Google OAuth 2.0 credentials (Client ID)
2. Valid Google ID token from user authentication
3. Application running on configured port (default: 7061)

### Manual Testing in Swagger

1. Start the application: `dotnet run --configuration Debug`
2. Navigate to: `https://localhost:7061/swagger/`
3. Find `POST /api/auth/google-sso` endpoint
4. Click "Try it out"
5. Provide a valid Google ID token in the request body
6. Click "Execute"

### Getting a Test Google ID Token

Use Google OAuth Playground:
1. Go to: https://developers.google.com/oauthplayground
2. Configure with your Google Client ID
3. Authorize with your Google account
4. Exchange authorization code for tokens
5. Copy the `id_token` value

## Security Considerations

? **Token Validation:** Google tokens are validated server-side against Google's public keys
? **JWT Signing:** Tokens are signed with a secure secret key
? **Error Messages:** Don't leak sensitive information
? **HTTPS Only:** Should be used over HTTPS in production
? **Token Expiration:** Tokens expire after configured duration (default: 60 minutes)
? **Secrets Management:** Sensitive data stored in user secrets, not in version control

### Production Checklist

- [ ] Use HTTPS for all endpoints
- [ ] Configure JWT signing key in secure environment (Azure Key Vault, AWS Secrets Manager, etc.)
- [ ] Set Google Client ID from secure configuration source
- [ ] Implement token refresh mechanism
- [ ] Add rate limiting to prevent brute force
- [ ] Enable proper logging for audit trails
- [ ] Implement database user creation on first SSO
- [ ] Add multi-factor authentication (optional)

## Troubleshooting

### Common Issues

**Issue: 401 Unauthorized**
- Cause: Invalid or expired Google ID token
- Solution: Ensure token is fresh and issued by Google

**Issue: JWT Signing Key not found**
- Cause: Missing `Jwt:SigningKey` in configuration
- Solution: Set user secret: `dotnet user-secrets set "Jwt:SigningKey" "<key>"`

**Issue: Google ClientId not configured**
- Cause: Missing `Google:ClientId` in configuration
- Solution: Set user secret: `dotnet user-secrets set "Google:ClientId" "<client-id>"`

## Future Enhancements

- [ ] Token refresh endpoint
- [ ] Multiple OAuth providers (GitHub, Microsoft, Facebook)
- [ ] Database user management
- [ ] Custom claims and roles
- [ ] Token revocation/blacklisting
- [ ] Passwordless authentication
- [ ] Biometric authentication

## References

- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [Google Sign-In for Web](https://developers.google.com/identity/sign-in/web)
- [JWT.io](https://jwt.io)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

## License

This implementation is part of the OccMin Incident Mapping project.
