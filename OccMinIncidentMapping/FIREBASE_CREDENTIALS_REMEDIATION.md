# Security Incident Remediation - Exposed Firebase Credentials

## Incident Summary

A Firebase service account credential file (`firebase.json`) containing sensitive authentication information was accidentally committed to the public GitHub repository. This file contained:

- **Service Account Email**: firebase-adminsdk-fbsvc@occimin-hazard-incident.iam.gserviceaccount.com
- **Key ID**: a82804aaa87e927c709923e5a56dc73f2807a1fe
- **Private Key**: Exposed in the JSON file

## Immediate Actions Taken

### 1. ? Removed Exposed Credentials
- Deleted `firebase.json` from the repository
- Added `firebase.json` to `.gitignore` to prevent future commits

### 2. ? Created Credentials Template
- Added `firebase.json.template` as a reference for developers
- Template contains placeholder values, not real credentials

### 3. ? Updated Firebase Configuration
- Modified `Infrastructure/Extensions/ServiceCollectionExtensions.cs` to support environment variables
- Now supports three credential loading methods (in order of preference):
  1. `GOOGLE_APPLICATION_CREDENTIALS` environment variable (path to credentials file)
  2. `FIREBASE_CREDENTIALS_JSON` environment variable (inline JSON credentials)
  3. Local `firebase.json` file (development only, not recommended)

## Required Actions for Deployment

### Step 1: Rotate Firebase Service Account Key
1. Log into Google Cloud Console
2. Navigate to Service Accounts under IAM
3. Select the exposed service account: `firebase-adminsdk-fbsvc@occimin-hazard-incident.iam.gserviceaccount.com`
4. Delete the compromised key (ID: a82804aaa87e927c709923e5a56dc73f2807a1fe)
5. Create a new service account key
6. Download the new credentials JSON file

### Step 2: Configure Credentials in Deployment Environment

#### Option A: File-based (Recommended for Docker/Kubernetes)
```bash
# Download new firebase.json from Google Cloud Console
# Place it in the deployment environment
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/firebase.json"
```

#### Option B: Environment Variable (Recommended for Cloud PaaS)
```bash
# Set the entire JSON as an environment variable
export FIREBASE_CREDENTIALS_JSON='{"type":"service_account",...}'
```

#### Option C: Local Development (Not for Production)
```bash
# Copy firebase.json.template to firebase.json
# Replace placeholder values with actual credentials
# This file is in .gitignore and will not be committed
cp firebase.json.template firebase.json
# Edit firebase.json with real credentials
```

### Step 3: Update CI/CD Pipelines
- Ensure credentials are stored as secrets, not hardcoded
- For GitHub Actions: Use repository secrets
- For Azure DevOps: Use secure variable groups
- For other CI/CD systems: Use their secret management tools

## Verification Checklist

- [ ] Firebase service account key has been rotated
- [ ] Old key (a82804aaa87e927c709923e5a56dc73f2807a1fe) is confirmed deleted in Google Cloud
- [ ] Environment variables are configured in production
- [ ] CI/CD pipelines use secret management (not plain text)
- [ ] Local development setup uses `firebase.json` (git-ignored)
- [ ] Application builds and runs successfully with new credentials
- [ ] All tests pass with new credentials

## Credential Loading Flow

```
???????????????????????????????????????
? ServiceCollectionExtensions.cs       ?
? AddInfrastructure() method           ?
???????????????????????????????????????
               ?
               ?
     ???????????????????????
     ? Check environment   ?
     ? variables in order: ?
     ???????????????????????
                ?
                ??? GOOGLE_APPLICATION_CREDENTIALS? ??? Use as file path
                ?
                ??? Local firebase.json exists? ??? Use file (dev only)
                ?
                ??? FIREBASE_CREDENTIALS_JSON? ??? Write to temp file
                ?
                ??? None found? ??? Throw error (requires configuration)
```

## Best Practices Going Forward

1. **Never commit credentials** - Use `.gitignore` for all credential files
2. **Use environment variables** - Load sensitive data from environment, not files
3. **Rotate keys regularly** - Especially after exposure
4. **Audit access logs** - Check for unauthorized use of exposed credentials
5. **Use secrets management** - Git secrets, AWS Secrets Manager, Azure Key Vault, etc.
6. **Code reviews** - Require reviews to catch accidental credential commits

## Additional Resources

- [Google Cloud Security Documentation](https://cloud.google.com/docs/authentication/provide-credentials-adc)
- [Firebase Security Best Practices](https://firebase.google.com/docs/database/security)
- [GitHub Secret Scanning](https://docs.github.com/en/code-security/secret-scanning)
- [OWASP Credential Management](https://cheatsheetseries.owasp.org/cheatsheets/Credential_Management_Cheat_Sheet.html)

## Questions?

Contact the security team or refer to the SECURITY.md and AUTH_QUICKSTART.md documentation in the repository.
