# URGENT: Firebase Credentials Exposure - Action Items

## ?? CRITICAL - Do This Immediately

### For Google Cloud Project Owner
**Timeline: TODAY - within 1 hour**

1. **Disable the compromised service account key**
   - [ ] Log into [Google Cloud Console](https://console.cloud.google.com)
   - [ ] Go to: IAM & Admin ? Service Accounts
   - [ ] Select: `firebase-adminsdk-fbsvc@occimin-hazard-incident.iam.gserviceaccount.com`
   - [ ] Click "Keys" tab
   - [ ] Find key with ID: `a82804aaa87e927c709923e5a56dc73f2807a1fe`
   - [ ] Click the menu icon and select "Delete"
   - [ ] Confirm deletion
   - [ ] Screenshot proof of deletion

2. **Create a replacement service account key**
   - [ ] In the same Keys tab
   - [ ] Click "Add Key" ? "Create new key"
   - [ ] Select "JSON" format
   - [ ] Download and securely store the new credentials file
   - [ ] Send to team lead via secure channel (NOT email, NOT Slack)

3. **Review access logs for unauthorized activity**
   - [ ] Go to: Logging ? Log Explorer
   - [ ] Filter for service account activity in the last 30 days
   - [ ] Look for:
     - [ ] Database reads/writes from unexpected locations
     - [ ] API key usage from unusual IP addresses
     - [ ] Failed authentication attempts
     - [ ] Resource creation/deletion
   - [ ] Document any suspicious activity

4. **Verify no unauthorized resources were created**
   - [ ] Check Firestore databases (no unexpected collections)
   - [ ] Check Storage buckets (no unexpected files)
   - [ ] Check API keys (no unauthorized keys created)
   - [ ] Check OAuth consent screen (no suspicious apps)

### For Development Team Lead
**Timeline: TODAY - within 2 hours**

1. **Notify the team**
   - [ ] Post incident notification to #security or equivalent channel
   - [ ] Message: "Firebase credentials were exposed in GitHub. New setup required."
   - [ ] Share this file with developers

2. **Update Git History**
   - [ ] Review: [GitHub BFG Repo Cleaner](https://rtyley.github.io/bfg-repo-cleaner/)
   - [ ] Or: [Git Filter-Repo](https://github.com/newren/git-filter-repo)
   - [ ] Remove all historical versions of `firebase.json` from git
   - [ ] Force push to origin (requires admin privileges)
   - [ ] Verify with: `git log --all --source --full-history -- firebase.json`

3. **Update CI/CD Secrets**
   - [ ] GitHub Actions: Update `FIREBASE_CREDENTIALS_JSON` secret
   - [ ] Azure DevOps: Update `firebase-credentials` variable group
   - [ ] Jenkins/GitLab: Update respective secret managers
   - [ ] Verify old secrets are deleted

4. **Notify Deployment Teams**
   - [ ] Production deployment team
   - [ ] Staging environment manager
   - [ ] DevOps/Infrastructure team

---

## ?? HIGH PRIORITY - Do This Today

### For All Developers
**Timeline: TODAY - after receiving new credentials**

1. **Set up new credentials locally**
   - [ ] Receive new `firebase.json` from team lead
   - [ ] Save it to `OccMinIncidentMapping/firebase.json`
   - [ ] Do NOT commit to git (already in .gitignore)
   - [ ] Verify application starts: `dotnet run --project OccMinIncidentMapping`
   - [ ] Verify can read/write to Firestore
   - [ ] Confirm with team: "Firebase setup working"

2. **Delete old firebase.json (if it exists)**
   - [ ] If you have an old `firebase.json` file locally
   - [ ] Delete it: `rm firebase.json` (or delete via explorer)
   - [ ] Verify it's gone: `ls firebase.json` should fail

3. **Update your git configuration**
   - [ ] Ensure you have the latest `.gitignore`:
     ```bash
     git pull origin add_auth
     git status  # Should not show firebase.json as modified
     ```
   - [ ] Test that firebase.json is truly ignored:
     ```bash
     echo "test" > firebase.json
     git status  # Should NOT show firebase.json as untracked
     ```

### For DevOps/Deployment Team
**Timeline: TODAY**

1. **Update production deployment**
   - [ ] Update environment variables in production:
     ```bash
     GOOGLE_APPLICATION_CREDENTIALS=/app/secrets/firebase.json
     # OR
     FIREBASE_CREDENTIALS_JSON=<new-json-content>
     ```
   - [ ] Update Docker/Kubernetes manifests
   - [ ] Update deployment scripts

2. **Update staging environment**
   - [ ] Apply same changes to staging
   - [ ] Test application deployment
   - [ ] Verify Firestore connectivity

3. **Document all changes**
   - [ ] Create a change log entry
   - [ ] Note: "Updated Firebase credentials due to security incident"
   - [ ] Note key rotation details

---

## ? MEDIUM PRIORITY - Do This This Week

1. **Enable secret scanning on GitHub** (if not already enabled)
   - [ ] Repository Settings ? Security & analysis
   - [ ] Enable "Secret scanning"
   - [ ] Enable "Push protection"

2. **Implement pre-commit hooks** (optional but recommended)
   - [ ] Install: `npm install -g detect-secrets` or `pip install detect-secrets`
   - [ ] Set up pre-commit hook to scan for secrets
   - [ ] Document in README.md

3. **Audit other repositories**
   - [ ] Check for similar exposed credentials in other projects
   - [ ] Scan entire GitHub organization for exposed secrets
   - [ ] Use: `git-secrets` or `talisman` or `detect-secrets`

4. **Review deployment pipelines**
   - [ ] Verify credentials are stored as secrets, not in YAML
   - [ ] Verify CI/CD logs don't expose secrets
   - [ ] Enable secret masking in logs

5. **Update team security training**
   - [ ] Share: `CREDENTIAL_MANAGEMENT.md`
   - [ ] Review: `FIREBASE_SETUP.md` 
   - [ ] Review: `FIREBASE_CREDENTIALS_REMEDIATION.md`
   - [ ] Schedule security awareness training

---

## ?? DOCUMENTATION - Read This

1. **What happened?**
   - Read: `FIREBASE_CREDENTIALS_REMEDIATION.md`

2. **How do I set up Firebase credentials?**
   - Read: `FIREBASE_SETUP.md`

3. **How do I manage secrets going forward?**
   - Read: `CREDENTIAL_MANAGEMENT.md`

4. **What was fixed in the code?**
   - Read: `SECURITY_FIX_SUMMARY.md`

---

## ? Verification Checklist

### Before Declaring Incident Resolved
- [ ] Old service account key deleted in Google Cloud
- [ ] New service account key generated and distributed
- [ ] Git history cleaned of `firebase.json`
- [ ] All developers have new credentials
- [ ] Production deployment updated
- [ ] Staging environment tested successfully
- [ ] CI/CD pipelines using new credentials
- [ ] Access logs reviewed for unauthorized activity
- [ ] Team trained on credential management
- [ ] Secret scanning enabled on GitHub
- [ ] Pre-commit hooks implemented (optional)
- [ ] Documentation updated

### Ongoing Monitoring
- [ ] Weekly review of access logs
- [ ] Monthly credential rotation schedule
- [ ] Quarterly security audits
- [ ] CI/CD secret scanning results
- [ ] GitHub secret scanning alerts

---

## ?? Escalation

If you encounter any issues during remediation:

1. **Code-related issues**
   - Contact: Development Team Lead
   - Reference: `Infrastructure/Extensions/ServiceCollectionExtensions.cs`

2. **Google Cloud / Firebase issues**
   - Contact: GCP Project Owner
   - Reference: Google Cloud documentation
   - Reference: Firebase documentation

3. **Deployment / CI-CD issues**
   - Contact: DevOps Team Lead
   - Reference: Your deployment platform documentation

4. **Security policy questions**
   - Contact: Security Team
   - Reference: CREDENTIAL_MANAGEMENT.md

---

## ?? References

| Document | Purpose |
|----------|---------|
| `FIREBASE_CREDENTIALS_REMEDIATION.md` | Detailed incident information |
| `FIREBASE_SETUP.md` | Setup instructions for all environments |
| `CREDENTIAL_MANAGEMENT.md` | Best practices for credential handling |
| `SECURITY_FIX_SUMMARY.md` | Summary of code changes |
| `.gitignore` | Files excluded from git (updated) |
| `firebase.json.template` | Template for credentials file |

---

## ?? Security Reminder

**NEVER:**
- ? Share credentials via email
- ? Paste credentials in Slack/Teams chat
- ? Commit credentials to git
- ? Log credential values
- ? Share credentials in screenshots
- ? Store credentials in plain text files

**ALWAYS:**
- ? Use environment variables
- ? Use secrets management tools
- ? Rotate credentials regularly
- ? Audit access logs
- ? Review code changes for secrets
- ? Use .gitignore for credential files

---

**Status**: ?? CRITICAL - IMMEDIATE ACTION REQUIRED

**Last Updated**: [Current Date]

**Next Review**: [Date + 7 days]

**Owner**: [Security Team / Project Lead]
