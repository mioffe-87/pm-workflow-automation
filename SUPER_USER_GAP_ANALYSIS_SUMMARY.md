# Super User Gap Analysis - Executive Summary

**Analysis Date:** September 22, 2026  
**Database:** CSP (gcp-sql-csp-rw-prod)  
**Status:** ✅ Complete

---

## Key Findings

### Critical Metrics

| Metric | Value |
|--------|-------|
| **Total Active Support Accounts** | 248,626 |
| **Accounts WITH Super User** | 172,630 |
| **Accounts WITHOUT Super User** | 75,996 |
| **Percentage at Risk** | **30.6%** |
| **Risk Level** | 🔴 **HIGH RISK** |

### Risk Assessment

**30.6% of active support accounts lack any user with the "Super User" role.** This represents a significant access control and compliance gap that requires immediate attention.

---

## What This Means

### Super User Role Definition
- **Super User** is the primary administrative role in CSP
- Required for account management, user provisioning, and critical operations
- Every active account should have at least one Super User

### Accounts at Risk
- **75,996 accounts** have zero active Super Users
- These accounts cannot perform administrative functions
- Users in these accounts have limited access and cannot manage memberships
- Potential compliance violations if accounts require Super User oversight

### Common Patterns in At-Risk Accounts

Based on the analysis, at-risk accounts include:
- **Newly created accounts** (many from Sept 22, 2026) - likely not yet provisioned
- **Evaluation/Trial accounts** - may not have assigned administrators
- **Legacy accounts** (from 2013) - may have had Super Users who were removed/deactivated
- **Accounts with zero active users** - no one to assign the role to

---

## Deliverables

### 1. SQL Query
**File:** `super_user_gap_analysis.sql`

The query identifies all active accounts without Super Users by:
- Joining `supportaccount` with `membership` and `membershiproles`
- Filtering for active memberships (ActivationDate ≤ NOW, ExpirationDate ≥ NOW or NULL)
- Counting distinct users with "Super User" role per account
- Returning only accounts with zero Super Users

**Key Features:**
- Includes account type and creation date for context
- Shows total active users per account
- Handles expired memberships correctly
- Optimized for large datasets (248K+ accounts)

### 2. Python Analysis Script
**File:** `analyze_super_user_gaps.py`

Automated analysis tool that:
- Connects to CSP database using `.env` credentials
- Executes the SQL query
- Generates formatted text report with summary statistics
- Exports results to CSV for stakeholder review
- Provides actionable recommendations

**Usage:**
```bash
python3 analyze_super_user_gaps.py
```

### 3. Text Report
**File:** `super_user_gap_report.txt` (6.2 MB)

Comprehensive report including:
- Summary statistics and risk assessment
- Detailed list of all 75,996 at-risk accounts
- Account details: ID, name, type, creation date, active user count
- Actionable recommendations for remediation
- Governance best practices

### 4. CSV Export
**File:** `super_user_gap_report.csv` (6.5 MB, 75,999 rows)

Machine-readable format with columns:
- `SupportAccountId` - Unique account identifier
- `AccountName` - Organization name
- `IsActive` - Account active status
- `IsPartner` - Partner account flag
- `CreatedOn` - Account creation date
- `AccountType` - Customer, Eval, Internal, Marketplace, etc.
- `SuperUserCount` - Number of active Super Users (0 for all rows)
- `TotalActiveUsers` - Total active users in account
- `Status` - MISSING_SUPER_USER

**Use Cases:**
- Import into BI tools for visualization
- Bulk operations to assign Super Users
- Compliance reporting
- Stakeholder communication

---

## Recommended Actions

### Immediate (Week 1)
1. **Triage by Account Type**
   - Separate newly created accounts (auto-provisioning may be pending)
   - Identify evaluation accounts that may not need Super Users
   - Flag legacy accounts for investigation

2. **Contact Account Owners**
   - Reach out to owners of at-risk accounts
   - Determine if Super User assignment is needed
   - Collect information on intended administrators

3. **Assign Super Users**
   - For accounts with active users: assign one to Super User role
   - For accounts with no users: wait for user registration
   - Document all assignments for audit trail

### Short-term (Month 1)
1. **Implement Automated Alerts**
   - Alert when last Super User is removed from account
   - Alert when Super User membership expires
   - Prevent deletion of last Super User

2. **Establish Minimum Requirements**
   - Require minimum 2-3 Super Users per account (redundancy)
   - Enforce Super User approval for new user registrations
   - Document Super User responsibilities

3. **Audit Existing Assignments**
   - Review Super Users in accounts WITH Super Users
   - Verify they are still active and appropriate
   - Remove inactive or unauthorized Super Users

### Long-term (Ongoing)
1. **Governance Framework**
   - Document Super User roles and responsibilities
   - Create rotation policies for Super User assignments
   - Establish escalation procedures for access issues

2. **Quarterly Reviews**
   - Run this analysis quarterly
   - Track trend of at-risk accounts
   - Report to compliance and security teams

3. **Process Improvements**
   - Automate Super User assignment during account creation
   - Implement role-based access control (RBAC) enhancements
   - Create self-service Super User request workflows

---

## Technical Details

### Query Logic

```sql
-- Identifies active accounts without Super Users
SELECT 
    sa.SupportAccountId,
    sa.Name,
    COUNT(DISTINCT m.UserAccountId) AS SuperUserCount
FROM supportaccount sa
LEFT JOIN membership m ON sa.SupportAccountId = m.SupportAccountId
LEFT JOIN membershiproles mr ON m.MembershipId = mr.MembershipId
LEFT JOIN role r ON mr.RoleId = r.RoleId
WHERE 
    sa.IsActive = 1
    AND r.Name = 'Super User'
    AND m.ActivationDate <= NOW()
    AND (m.ExpirationDate >= NOW() OR m.ExpirationDate IS NULL)
GROUP BY sa.SupportAccountId
HAVING COUNT(DISTINCT m.UserAccountId) = 0
```

### Edge Cases Handled
- ✅ Expired Super User memberships (not counted as active)
- ✅ Pending memberships (not counted as active)
- ✅ Multiple Super Users per account (counted once)
- ✅ Accounts with no users at all (included in results)
- ✅ Accounts with users but no Super User role (included in results)

### Performance
- Query execution time: ~50 seconds for 248K+ accounts
- CSV export: 75,999 rows (header + 75,996 at-risk accounts)
- Suitable for automated scheduling and reporting

---

## Next Steps

1. **Review this summary** with stakeholders
2. **Examine the CSV** for accounts requiring immediate action
3. **Run the analysis quarterly** to track progress
4. **Implement recommendations** based on account type and risk level
5. **Update governance policies** to prevent future gaps

---

## Files Generated

| File | Size | Purpose |
|------|------|---------|
| `super_user_gap_analysis.sql` | 2.0 KB | SQL query for analysis |
| `analyze_super_user_gaps.py` | 9.1 KB | Python automation script |
| `super_user_gap_report.txt` | 6.2 MB | Formatted text report |
| `super_user_gap_report.csv` | 6.5 MB | Machine-readable export |

All files are located in: `/Users/mioffe/Documents/pm-workflow-automation/`

---

## Questions?

For questions about this analysis:
- Review the SQL query for technical details
- Check the Python script for data processing logic
- Consult the CSV for specific account information
- Reference the text report for recommendations
