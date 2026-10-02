# Slack Message to GCP Admin - BigQuery Permissions Request

## Option 1: Concise Version (Recommended for Slack)

```
Hi @GCP-Admin,

I need BigQuery query permissions on the `itp-aia-datalake` project to run analytics on our CSP dataset.

**Current Issue:**
- User: mioffe@paloaltonetworks.com
- Project: itp-aia-datalake
- Error: Access Denied - User does not have bigquery.jobs.create permission

**Requested IAM Role:**
- Role: `roles/bigquery.jobUser` (minimum) or `roles/bigquery.admin` (if needed for dataset management)
- Scope: itp-aia-datalake project

**Use Case:**
Running analytics queries on the CSP dataset to analyze user activation metrics and cohort performance.

Thanks!
```

---

## Option 2: Detailed Version (For Email or Formal Request)

```
Hi @GCP-Admin,

I'm requesting BigQuery query execution permissions on the itp-aia-datalake project.

**Details:**
- User Email: mioffe@paloaltonetworks.com
- Project ID: itp-aia-datalake
- Dataset: csp
- Required Permission: bigquery.jobs.create

**Current Error:**
```
403 POST https://bigquery.googleapis.com/bigquery/v2/projects/itp-aia-datalake/jobs
Access Denied: Project itp-aia-datalake: User does not have bigquery.jobs.create permission
```

**Requested IAM Role:**
- Minimum: `roles/bigquery.jobUser` - allows running queries
- Alternative: `roles/bigquery.admin` - if I need to manage datasets/tables

**Purpose:**
I'm analyzing user activation metrics from the CSP dataset to understand:
- New user cohort creation rates
- First-week login activation rates
- Time-to-first-login metrics

This will help us track product adoption and onboarding effectiveness.

Please let me know if you need any additional information or if there's a different process for requesting these permissions.

Thanks!
```

---

## Option 3: Quick Slack Mention (If Admin is Available)

```
@GCP-Admin - Quick question: Can you grant bigquery.jobs.create permission for mioffe@paloaltonetworks.com on the itp-aia-datalake project? I'm getting a 403 error when trying to run queries. Thanks!
```

---

## What to Include in Your Message

✅ **Essential:**
- Your email: `mioffe@paloaltonetworks.com`
- Project: `itp-aia-datalake`
- Specific role needed: `roles/bigquery.jobUser`

✅ **Helpful Context:**
- What you're trying to do (analyze user activation)
- The error message you're getting
- Why you need it (business use case)

✅ **Optional:**
- Timeline (if urgent)
- Alternative solutions they might suggest

---

## After Permissions Are Granted

Once your admin grants the permissions, you can immediately run:

```bash
python3 run_cohort_query.py
```

This will execute the new user cohort analysis and generate results showing:
- How many users were created per day
- What % activated in their first week
- Average time to first login

---

## If You Need to Escalate

If the admin needs more context, you can share:
- **Query file**: `new_user_cohort_query.sql`
- **Analysis guide**: `NEW_USER_COHORT_ANALYSIS.md`
- **Python script**: `run_cohort_query.py`

All are saved in `/Users/mioffe/Documents/pm-workflow-automation/`
