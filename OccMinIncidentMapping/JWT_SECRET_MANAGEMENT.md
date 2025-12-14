# JWT Secret Key Management

## Overview

JWT secret keys should **never** be committed to version control or stored in configuration files. This document explains how to securely manage JWT secrets in this application.

## Security Best Practices

? **DO:**
- Store secrets in environment variables
- Use Azure Key Vault for cloud deployments
- Use User Secrets for local development (`.NET` secret manager)
- Rotate keys regularly
- Use strong, randomly generated keys (minimum 32 characters)

? **DON'T:**
- Commit secrets to Git or version control
- Store secrets in `appsettings.json` or `appsettings.Production.json`
- Use placeholder or weak secret keys in production
- Share secrets in code reviews or communication channels

## Setting Up JWT Secret

### Development (Local Machine)

Use the .NET User Secrets manager to store the JWT secret locally:

```bash
# Initialize user secrets for the project (run from OccMinIncidentMapping directory)
dotnet user-secrets init

# Set the JWT secret
dotnet user-secrets set "Jwt:SecretKey" "your-very-long-random-secret-key-at-least-32-characters-long"
```

Alternatively, set the environment variable:

```bash
# Windows (PowerShell)
$env:JWT_SECRET_KEY = "your-very-long-random-secret-key-at-least-32-characters-long"

# Windows (Command Prompt)
set JWT_SECRET_KEY=your-very-long-random-secret-key-at-least-32-characters-long

# Linux/macOS
export JWT_SECRET_KEY="your-very-long-random-secret-key-at-least-32-characters-long"
```

### Production (Cloud Deployment)

#### Option 1: Azure Key Vault (Recommended)

1. Create a Key Vault in Azure:
```bash
az keyvault create --name "your-keyvault-name" --resource-group "your-resource-group"
```

2. Add the JWT secret:
```bash
az keyvault secret set --vault-name "your-keyvault-name" --name "JwtSecretKey" --value "your-very-long-random-secret-key"
```

3. Configure the application to use Key Vault:
```csharp
// Add this to Program.cs
var keyVaultUrl = new Uri($"https://{builder.Configuration["KeyVault:Name"]}.vault.azure.net/");
builder.Configuration.AddAzureKeyVault(
    keyVaultUrl,
    new DefaultAzureCredential());
```

#### Option 2: Environment Variables

Set the `JWT_SECRET_KEY` environment variable in your deployment environment:

- **Azure App Service**: Application Settings
- **Docker**: Environment variable in container
- **Kubernetes**: Secret in pod environment
- **Traditional Server**: System/User environment variable

Example for Azure App Service:
```bash
az webapp config appsettings set \
    --resource-group "your-resource-group" \
    --name "your-app-name" \
    --settings JWT_SECRET_KEY="your-very-long-random-secret-key"
```

### Docker Deployment

Pass the secret via environment variable:

```dockerfile
ENV JWT_SECRET_KEY="your-very-long-random-secret-key"
```

Or at runtime:

```bash
docker run -e JWT_SECRET_KEY="your-secret-key" your-image-name
```

### Kubernetes Deployment

Create a Kubernetes secret:

```bash
kubectl create secret generic jwt-secrets --from-literal=JWT_SECRET_KEY="your-very-long-random-secret-key"
```

Reference in your deployment manifest:

```yaml
env:
  - name: JWT_SECRET_KEY
    valueFrom:
      secretKeyRef:
        name: jwt-secrets
        key: JWT_SECRET_KEY
```

## Generating a Strong Secret Key

Use a secure random generator to create a strong key:

```bash
# Linux/macOS
openssl rand -base64 32

# PowerShell
[Convert]::ToBase64String([System.Security.Cryptography.RNGCryptoServiceProvider]::new().GetBytes(32))

# Or use an online generator: https://randomkeygen.com/
```

## Application Configuration

The application retrieves the JWT secret in this order:

1. **Environment variable** `JWT_SECRET_KEY` (recommended for production)
2. **User Secrets** or configuration (development only)

If no valid secret is found, the application will throw an error at startup.

## Verification

To verify your setup is correct, the application should start without errors. If you see:

```
JWT SecretKey not found. Please set the 'JWT_SECRET_KEY' environment variable.
```

Then the environment variable is not properly set in your deployment environment.

## Key Rotation

To rotate keys securely:

1. Generate a new secret key
2. Deploy the new key to your secrets manager
3. Restart the application
4. Verify authentication still works
5. Consider implementing key versioning for zero-downtime rotation

## Additional Resources

- [Microsoft: Secure configuration data](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/secure-net-microservices-web-applications/)
- [Microsoft: Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/)
- [OWASP: Secrets Management](https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html)
