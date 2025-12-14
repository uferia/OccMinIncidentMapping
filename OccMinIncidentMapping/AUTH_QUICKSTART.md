# Authentication Quick Start Guide

## Getting Started with JWT Authentication

### 1. Login Endpoint

**Endpoint**: `POST /api/auth/login`

**Request Body**:
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Response**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin",
  "role": "Admin"
}
```

### 2. Using the Token

Include the token in the `Authorization` header:
```
Authorization: Bearer {token}
```

### 3. Example: Get All Incidents

```bash
# Get token
TOKEN=$(curl -X POST http://localhost:5094/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  | jq -r '.token')

# Use token to access protected endpoint
curl http://localhost:5094/api/incidents \
  -H "Authorization: Bearer $TOKEN"
```

### 4. Example: Create Incident

```bash
curl -X POST http://localhost:5094/api/incidents \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "latitude": 40.7128,
    "longitude": -74.0060,
    "hazard": "Flood",
    "status": "Ongoing",
    "description": "Flooding in downtown area"
  }'
```

## Testing in Swagger/OpenAPI UI

1. Navigate to `http://localhost:5094/swagger`
2. Click the "Authorize" button
3. Enter: `Bearer {token}`
4. Click "Authorize"
5. Now all endpoints can be tested with authentication

## Token Expiration

- Default expiry: **60 minutes** (configurable in `appsettings.json`)
- When expired, login again to get a new token
- Token expiry time is in the JWT payload as `exp` claim

## User Roles

**Development Users**:
- **admin** (role: Admin) - Full access
- **user** (role: User) - Standard access

Both roles currently have the same permissions. Configure role-based authorization in `IncidentsController` as needed.

## Troubleshooting

### "401 Unauthorized"
- Token missing or invalid
- Token has expired
- Token format incorrect (should be `Bearer {token}`)

### "403 Forbidden"
- User doesn't have required role
- Endpoint requires specific authorization

### Invalid Credentials
- Check username and password spelling
- Ensure credentials match users in `JwtAuthenticationService.cs`

## Security Notes

?? **Development Only**:
- Default credentials are hardcoded
- JWT secret key is in configuration file
- No password hashing

? **Production Requirements**:
1. Replace hardcoded users with database lookup
2. Implement proper password hashing (bcrypt, PBKDF2)
3. Move JWT secret to secure location (Azure Key Vault, etc.)
4. Use environment variables for sensitive config
5. Implement rate limiting on login endpoint
6. Add multi-factor authentication (MFA)
