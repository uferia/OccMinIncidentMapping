# Quick Start: JWT Secret Configuration

## For Immediate Testing (Development)

Run this command from the `OccMinIncidentMapping` directory:

```bash
# Generate a secure random key
# On Windows PowerShell:
$randomKey = [Convert]::ToBase64String([System.Security.Cryptography.RNGCryptoServiceProvider]::new().GetBytes(32)); echo "Your key: $randomKey"

# On Linux/macOS:
openssl rand -base64 32
```

Then set it:

```bash
# Windows PowerShell
$env:JWT_SECRET_KEY = "paste-your-generated-key-here"

# Linux/macOS
export JWT_SECRET_KEY="paste-your-generated-key-here"

# Run the application
dotnet run
```

## For Development with User Secrets (Recommended)

```bash
cd OccMinIncidentMapping
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "your-super-long-random-secret-key-at-least-32-characters-long"
dotnet run
```

## For Production Deployment

Set the `JWT_SECRET_KEY` environment variable in your deployment environment. See `JWT_SECRET_MANAGEMENT.md` for detailed instructions for your specific platform (Azure, Docker, Kubernetes, etc.).

## Verification

If configured correctly, you should see no errors starting the application. If you see:

```
JWT SecretKey not found. Please set the 'JWT_SECRET_KEY' environment variable
```

Then the environment variable is not set. Follow the setup instructions above.
