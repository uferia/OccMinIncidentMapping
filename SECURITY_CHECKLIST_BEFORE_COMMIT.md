# SECURITY CHECKLIST - Before Committing

## ? What to Check Before Git Push

### 1. Secrets & Credentials
- [ ] No hardcoded JWT signing keys in code
- [ ] No hardcoded Google Client IDs in code
- [ ] No API keys or tokens in files
- [ ] No Firebase service account keys in repository
- [ ] No AWS/Azure credentials anywhere

### 2. Authentication Files
- [ ] No `*.key` files committed
- [ ] No `*.pem` files committed
- [ ] No `credentials.json` files committed
- [ ] No `firebase.json` in repository root
- [ ] No environment files (`.env`) committed

### 3. Test Data
- [ ] No test Google ID tokens
- [ ] No test JWT tokens
- [ ] No test credentials or usernames/passwords
- [ ] No sample API responses with real data

### 4. Configuration
- [ ] No sensitive URLs with credentials
- [ ] No connection strings with passwords
- [ ] No API endpoints with authentication tokens
- [ ] No hardcoded database credentials

### 5. User Secrets
- [ ] User secrets are stored locally (`..\AppData\Roaming\Microsoft\UserSecrets`)
- [ ] User secrets are NOT in repository
- [ ] `.gitignore` includes user-secrets folder (automatic)

## Where Secrets Should Be Stored

### Development
```
User Secrets: ~/.microsoft/usersecrets/{project-id}/secrets.json
Command: dotnet user-secrets set "Key" "Value"
```

### Production
```
Cloud: Azure Key Vault / AWS Secrets Manager
Environment Variables: Set via deployment pipeline
Configuration: Use cloud provider's secret management
```

## Check Git Status Before Commit

```powershell
# See what files will be committed
git status

# See detailed changes
git diff

# See staged changes
git diff --cached

# Search for exposed secrets (optional)
git diff HEAD -- '*.json' '*.cs' '*.md' | grep -i "secret\|key\|token\|password"
```

## Files That Should NOT Be Committed

```
bin/
obj/
.vs/
.vscode/
*.suo
*.user
appsettings.*.json (with secrets)
*.serviceaccount.json
firebase.json
credentials.json
.env
.env.local
```

## Files That ARE Safe To Commit

```
? Source code (.cs files)
? Configuration templates (appsettings.json with placeholders)
? Project files (.csproj)
? Solution files (.sln)
? Documentation (.md files WITHOUT tokens)
? Gitignore rules
? CI/CD configuration (GitHub Actions, etc.)
```

## Before Final Commit

```powershell
# 1. Check for secrets in last commit
git log -p --all -S "password" -- '*.cs' '*.json' '*.md'

# 2. Verify user secrets are not in repo
git log --all --full-history -- "*/secrets.json"

# 3. Check for common secret patterns
git diff HEAD | grep -E "(api[_-]?key|secret|token|password)" -i

# 4. List all files that will be committed
git ls-files

# 5. Remove any files that shouldn't be there
git rm --cached <unwanted-file>
git commit --amend
```

## If You Accidentally Committed Secrets

### Immediate Action
```powershell
# 1. DO NOT PUSH to GitHub
# 2. Remove from recent commit
git reset --soft HEAD~1
git rm --cached <secret-file>
git commit -m "Remove sensitive data"

# 3. If already pushed, use BFG Repo-Cleaner
# Instructions: https://rtyley.github.io/bfg-repo-cleaner/

# 4. Rotate any compromised credentials immediately
```

## Recommended: Add Pre-Commit Hook

Create `.git/hooks/pre-commit`:
```bash
#!/bin/bash
# Prevent committing files with secrets

FILES_TO_CHECK=$(git diff --cached --name-only)

for file in $FILES_TO_CHECK; do
    if grep -E "(SECRET|KEY|PASSWORD|TOKEN|CREDENTIAL)" "$file" -i > /dev/null; then
        echo "ERROR: Potential secret found in $file"
        echo "Please remove sensitive data before committing"
        exit 1
    fi
done

exit 0
```

Make it executable:
```bash
chmod +x .git/hooks/pre-commit
```

## Commit Message Best Practices

? Good commit messages:
```
feat: implement Google SSO authentication

- Add Google ID token validation
- Generate JWT tokens for authenticated users
- Return user email in response
- Fix JWT signing key configuration
```

? Bad commit messages (avoid):
```
"fixed bug" - too vague
"update" - unclear what changed
"add jwt key: abc123xyz" - exposes secrets!
"temporary fix" - indicates incomplete work
```

## Final Checklist Before Push

- [ ] All tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build`
- [ ] No secrets in code: `git diff --cached | grep -i secret`
- [ ] No credentials in docs
- [ ] No hardcoded tokens
- [ ] `.gitignore` properly configured
- [ ] Commit message is clear and descriptive
- [ ] Ready to push to main branch

---

**When in doubt, check your changes before committing!** ??
