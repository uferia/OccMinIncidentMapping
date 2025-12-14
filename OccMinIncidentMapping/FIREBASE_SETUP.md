# Firebase Credentials Setup Guide

## Overview

Firebase credentials are **NOT** included in the repository for security reasons. You must set up your own credentials for local development and deployment.

## For Local Development

### Step 1: Obtain Firebase Service Account Credentials

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Select your Firebase project: `occimin-hazard-incident`
3. Navigate to **Service Accounts** (APIs & Services ? Service Accounts)
4. Select the service account (or create a new one)
5. Click **Keys** tab
6. Create a new key (JSON format)
7. Save the downloaded file

### Step 2: Set Up Credentials File

**Option A: Local File (Recommended for development)**
```bash
# Copy the template
cp firebase.json.template firebase.json

# Replace the contents with your downloaded credentials file
# The firebase.json file is git-ignored and will not be committed
```

**Option B: Environment Variable**
```bash
# Set the environment variable with the full JSON content
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/your/firebase-credentials.json"

# Or on Windows PowerShell
$env:GOOGLE_APPLICATION_CREDENTIALS="C:\path\to\firebase-credentials.json"
```

### Step 3: Run the Application

```bash
# Build and run
dotnet build
dotnet run --project OccMinIncidentMapping

# The application will automatically load credentials from:
# 1. GOOGLE_APPLICATION_CREDENTIALS environment variable (if set)
# 2. Local firebase.json file (if it exists)
# 3. FIREBASE_CREDENTIALS_JSON environment variable (if set)
```

## For Deployment (Docker/Kubernetes)

### Step 1: Create a Secret

**Docker:**
```dockerfile
# In your Dockerfile or docker-compose
ENV GOOGLE_APPLICATION_CREDENTIALS=/app/secrets/firebase.json

# Copy credentials at runtime (not in image)
COPY firebase.json /app/secrets/firebase.json
```

**Kubernetes:**
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: firebase-credentials
type: Opaque
stringData:
  firebase.json: |
    {
      "type": "service_account",
      ...
    }
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: occmin-app
spec:
  template:
    spec:
      containers:
      - name: app
        env:
        - name: GOOGLE_APPLICATION_CREDENTIALS
          value: /var/secrets/firebase.json
        volumeMounts:
        - name: firebase-creds
          mountPath: /var/secrets
      volumes:
      - name: firebase-creds
        secret:
          secretName: firebase-credentials
```

### Step 2: Store Secrets Securely

**GitHub Actions:**
```yaml
- name: Deploy
  env:
    FIREBASE_CREDENTIALS_JSON: ${{ secrets.FIREBASE_CREDENTIALS_JSON }}
  run: |
    dotnet build
    dotnet publish -o ./publish
```

**Azure DevOps:**
```yaml
variables:
  - group: firebase-secrets

steps:
- task: DotNetCoreCLI@2
  env:
    FIREBASE_CREDENTIALS_JSON: $(firebaseCredentialsJson)
  inputs:
    command: 'build'
```

## For Cloud Deployment (Azure, AWS, GCP)

### Azure App Service
1. Go to Application Settings in Azure Portal
2. Add new application setting:
   - Name: `GOOGLE_APPLICATION_CREDENTIALS`
   - Value: `/app/secrets/firebase.json`
3. Add new application setting:
   - Name: `FIREBASE_CREDENTIALS_JSON`
   - Value: (Copy entire JSON content)

### AWS Lambda
```python
import json
import os

credentials_json = os.environ.get('FIREBASE_CREDENTIALS_JSON')
if credentials_json:
    # Application automatically loads from environment
    pass
```

### Google Cloud Run
```bash
gcloud run deploy occmin-app \
  --set-env-vars GOOGLE_APPLICATION_CREDENTIALS=/app/secrets/firebase.json \
  --secret firebase-creds=/app/secrets/firebase.json \
  ...
```

## Troubleshooting

### "Firebase credentials not found" error
**Solution:** Ensure one of these is set:
```bash
# Option 1: Set file path
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/firebase.json"

# Option 2: Set JSON content
export FIREBASE_CREDENTIALS_JSON='{"type":"service_account",...}'

# Option 3: Create firebase.json in project root
cp firebase.json.template firebase.json
# Edit with real credentials
```

### "firebase.json not found in git"
This is expected! The file is intentionally git-ignored for security. Set it up using the steps above.

### Permission Denied errors
Check that:
1. Service account has `Editor` role or required Firestore permissions
2. File path is correct (if using file-based approach)
3. JSON is valid (if using environment variable)

### Still having issues?
Run with debug logging:
```csharp
// In JwtAuthenticationService or ApplicationDbContext
System.Console.WriteLine($"Credentials path: {Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")}");
```

## Security Reminders

?? **IMPORTANT**
- ? Never commit `firebase.json` to git
- ? Never log or print credential content
- ? Never share credentials in Slack, email, or chat
- ? Always use `.gitignore` for credential files
- ? Always use environment variables in production
- ? Rotate credentials regularly
- ? Use secrets management tools (Vault, Key Vault, Secrets Manager)

## Questions?

See: `FIREBASE_CREDENTIALS_REMEDIATION.md` for incident details and best practices.
