# Credential Management Best Practices

## Overview
This document outlines security best practices for managing credentials and secrets in the OccMin Incident Mapping application.

## 1. Never Commit Secrets to Version Control

### ? DON'T
```csharp
// Don't hardcode credentials
var apiKey = "sk_live_51234567890abcdef";
var connectionString = "Server=db;User=admin;Password=SuperSecret123";
var firebaseJson = "{\"private_key\": \"...\"}";
```

```json
// Don't commit config files with secrets
{
  "Firebase": {
    "ApiKey": "AIzaSyD123...",
    "DatabaseUrl": "https://myapp.firebaseio.com"
  },
  "Jwt": {
    "SecretKey": "super-secret-key-do-not-share"
  }
}
```

### ? DO
```csharp
// Load from environment variables
var apiKey = Environment.GetEnvironmentVariable("API_KEY");
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
var credentialsJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
```

```json
// Use placeholders in committed config
{
  "Firebase": {
    "ProjectId": "your-project-id"
  },
  "Jwt": {
    "Issuer": "MyApp",
    "Audience": "MyAppClient"
  }
}
```

## 2. Use .gitignore for All Credential Files

### Essential Patterns
```gitignore
# Firebase and GCP
firebase.json
*.serviceaccount.json
google-credentials.json
service-account-key.json

# General credentials
credentials.json
secrets.json
.env
.env.local
.env.*.local
*.pem
*.key
*.crt

# API Keys
*.apikey
api-keys.json

# Database credentials
db-credentials.json
connectionstring.txt
```

### Verify Files Are Ignored
```bash
# Check if a file would be ignored
git check-ignore -v firebase.json

# List all ignored files in repo
git status --ignored
```

## 3. Environment Variable Naming Conventions

### Standard Prefixes
```
GOOGLE_* - Google Cloud Platform
FIREBASE_* - Firebase specific
AZURE_* - Azure services
AWS_* - AWS services
JWT_* - JWT authentication
DB_* - Database
API_* - API credentials
```

### Example Environment Variables
```bash
# Google/Firebase
GOOGLE_APPLICATION_CREDENTIALS=/app/secrets/firebase.json
FIREBASE_PROJECT_ID=occimin-hazard-incident
FIREBASE_API_KEY=AIzaSyD...

# JWT
JWT_SECRET_KEY=your-super-secret-key
JWT_ISSUER=OccMinIncidentMapping
JWT_AUDIENCE=OccMinIncidentMappingClient

# Database
DB_CONNECTION_STRING=Server=...
DB_USER=admin
DB_PASSWORD=...

# External APIs
API_KEY_STRIPE=sk_live_...
API_KEY_SENDGRID=SG....
```

## 4. Loading Credentials at Application Startup

### Recommended Pattern
```csharp
public static void ConfigureSecrets(IConfiguration configuration, IHostEnvironment env)
{
    // For development, use user secrets or local files
    if (env.IsDevelopment())
    {
        // User secrets or local .env
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env.local");
        if (File.Exists(envPath))
        {
            // Load from .env file using library like DotNetEnv
            DotNetEnv.Env.Load(envPath);
        }
    }
    else
    {
        // Production: must use environment variables or secret management
        ValidateRequiredSecrets(configuration);
    }
}

private static void ValidateRequiredSecrets(IConfiguration configuration)
{
    var requiredSecrets = new[]
    {
        "GOOGLE_APPLICATION_CREDENTIALS",
        "JWT_SECRET_KEY",
        "DB_CONNECTION_STRING"
    };

    var missing = new List<string>();
    foreach (var secret in requiredSecrets)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(secret)))
        {
            missing.Add(secret);
        }
    }

    if (missing.Any())
    {
        throw new InvalidOperationException(
            $"Missing required environment variables: {string.Join(", ", missing)}");
    }
}
```

## 5. Secrets Management Tools

### Local Development
- **User Secrets** (.NET built-in for development)
  ```bash
  dotnet user-secrets init
  dotnet user-secrets set "Jwt:SecretKey" "dev-secret-key"
  ```

- **Environment Files** (.env)
  ```bash
  # .env or .env.local (git-ignored)
  GOOGLE_APPLICATION_CREDENTIALS=./firebase.json
  JWT_SECRET_KEY=dev-secret
  ```

### CI/CD Pipelines
- **GitHub Secrets**
  ```yaml
  - name: Deploy
    env:
      FIREBASE_CREDENTIALS_JSON: ${{ secrets.FIREBASE_CREDENTIALS_JSON }}
  ```

- **Azure DevOps Variable Groups**
  ```yaml
  variables:
  - group: production-secrets
  ```

- **GitLab CI/CD**
  ```yaml
  deploy:
    variables:
      API_KEY: $API_KEY  # Protected variable
  ```

### Cloud Platforms
- **Azure Key Vault**
- **AWS Secrets Manager**
- **Google Cloud Secret Manager**
- **HashiCorp Vault**

## 6. Credential Rotation Policy

### Quarterly Rotation
```
Q1: API Keys
Q2: Database Passwords
Q3: Service Account Keys
Q4: JWT Secrets
```

### Rotation Steps
1. Generate new credential
2. Update all references (env vars, config, docs)
3. Test in staging environment
4. Deploy to production
5. Revoke old credential (after confirmation)
6. Document in change log

## 7. Audit and Monitoring

### Log Credential Usage (Without Exposing Values)
```csharp
_logger.LogInformation("Authenticating user: {Username}", username);
_logger.LogWarning("Failed authentication attempt for user: {Username}", username);
_logger.LogError(ex, "Error initializing service: {ServiceName}", nameof(FirebaseService));
// ? DON'T: _logger.LogDebug("Using API key: {ApiKey}", apiKey);
```

### Monitor for Exposure
- Use GitHub secret scanning
- Use GitLab secret detection
- Use cloud provider secret scanning
- Implement pre-commit hooks to prevent commits with secrets

### Pre-Commit Hook Example
```bash
#!/bin/bash
# .git/hooks/pre-commit

# Check for common secret patterns
if git diff --cached | grep -E 'private_key|api_key|password|secret'; then
    echo "? ERROR: Detected potential secrets in staged changes"
    echo "Do not commit secrets!"
    exit 1
fi
```

## 8. Code Review Checklist

Before approving PRs:
- [ ] No hardcoded API keys, passwords, or tokens
- [ ] No credential files committed
- [ ] Environment variables used for secrets
- [ ] Secrets documentation follows naming conventions
- [ ] Error messages don't expose sensitive data
- [ ] Logging doesn't expose sensitive data

## 9. Documentation

### What to Document
- ? Configuration keys (without values)
- ? Required environment variables
- ? Credential setup instructions
- ? Rotation policies
- ? Access control

### What NOT to Document
- ? Actual secret values
- ? Private keys
- ? API keys
- ? Database passwords
- ? Connection strings with credentials

### Example Template
```markdown
## Configuration

The application requires the following environment variables:

| Variable | Required | Type | Description |
|----------|----------|------|-------------|
| GOOGLE_APPLICATION_CREDENTIALS | Yes | Path | Path to Firebase service account JSON |
| JWT_SECRET_KEY | Yes | Secret | JWT signing key (min 32 bytes) |
| DB_CONNECTION_STRING | Yes | Secret | Database connection string |

See FIREBASE_SETUP.md for detailed setup instructions.
```

## 10. Quick Audit Checklist

Run these commands to audit your repository:

```bash
# Find potential secrets in git history
git log --all -p --source -- '*.json' '*.config' | grep -i 'secret\|password\|api'

# Check for large binary files that might contain credentials
git ls-files -l | sort -k5 -rn | head -10

# Find recently added large files
git log --all --pretty=format: --name-only | grep -E '\.(json|xml|config)$' | sort | uniq

# Check .gitignore effectiveness
git ls-files --others --ignored --exclude-standard
```

## References

- [Microsoft: Protect Application Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [OWASP: Secrets Management](https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html)
- [GitHub: Secret Scanning](https://docs.github.com/en/code-security/secret-scanning)
- [Google Cloud: Credential Management](https://cloud.google.com/docs/authentication)

## Questions?

- See: `FIREBASE_SETUP.md` for Firebase-specific setup
- See: `FIREBASE_CREDENTIALS_REMEDIATION.md` for incident details
- See: `SECURITY.md` for overall security architecture
