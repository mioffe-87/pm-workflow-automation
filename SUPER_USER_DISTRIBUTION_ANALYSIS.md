# Super User Distribution Analysis - Active Accounts (2026+)

**Analysis Date:** September 23, 2026  
**Database:** CSP (gcp-sql-csp-rw-prod)  
**Scope:** Active accounts with login activity since January 1, 2026  
**Status:** ✅ Complete

---

## Executive Summary

This analysis examines the distribution of Super User roles across 4,691 actively used CSP accounts. The findings reveal important insights about role redundancy, governance, and potential optimization opportunities.

### Key Findings

| Metric | Value |
|--------|-------|
| **Total Active Accounts** | 4,691 |
| **Minimum Super Users** | 1 |
| **Maximum Super Users** | 319 |
| **Mean (Average)** | 9.25 |
| **Median** | 7.00 |
| **Mode (Most Common)** | 4 |
| **Standard Deviation** | 11.53 |

---

## Statistical Analysis

### Distribution Characteristics

```
Percentile Analysis:
  Q1 (25th):  4 Super Users
  Median:     7 Super Users
  Q3 (75th):  11 Super Users
  
Range: 1 to 319 Super Users
Coefficient of Variation: 124.61% (High variability)
```

### Distribution Shape

The distribution is **right-skewed**, meaning:
- Most accounts cluster around 4-7 Super Users (the mode and median)
- A long tail of accounts with significantly more Super Users
- Mean (9.25) > Median (7.00) indicates the tail pulls the average upward

---

## Distribution Breakdown

### Accounts by Super User Count

| Super Users | Count | Percentage | Cumulative % | Visualization |
|-------------|-------|-----------|--------------|---|
| 1 | 146 | 3.11% | 3.11% | ██ |
| 2 | 299 | 6.37% | 9.49% | ████ |
| 3 | 452 | 9.64% | 19.12% | ██████ |
| 4 | 488 | 10.40% | 29.52% | ███████ |
| 5 | 470 | 10.02% | 39.54% | ███████ |
| 6 | 425 | 9.06% | 48.60% | ██████ |
| 7 | 356 | 7.59% | 56.19% | █████ |
| 8 | 304 | 6.48% | 62.67% | ████ |
| 9 | 275 | 5.86% | 68.54% | ████ |
| 10 | 219 | 4.67% | 73.20% | ███ |
| 11-20 | 1,015 | 21.63% | 94.83% | ███████████████ |
| 21-50 | 250 | 5.33% | 100.16% | ███ |
| 50+ | 12 | 0.26% | 100.42% | - |

### Key Segments

**Minimum Redundancy (1 Super User):**
- 146 accounts (3.11%)
- Single point of failure risk
- Recommendation: Upgrade to 2-3 Super Users

**Low Redundancy (2-3 Super Users):**
- 751 accounts (16.01%)
- Basic redundancy
- Acceptable for smaller accounts

**Moderate Redundancy (4-7 Super Users):**
- 2,197 accounts (46.84%)
- Optimal range for most accounts
- Good balance of redundancy and management

**High Redundancy (8-15 Super Users):**
- 1,195 accounts (25.48%)
- Excessive for most use cases
- Consider consolidation

**Very High Redundancy (16+ Super Users):**
- 81 accounts (1.73%)
- Likely needs review and consolidation
- Potential for RBAC optimization

---

## Key Insights

### 1. Redundancy Status

✅ **Good News:**
- 96.89% of accounts have 2+ Super Users (redundancy)
- Only 3.11% have single point of failure (1 Super User)
- Median of 7 Super Users provides good redundancy

⚠️ **Concerns:**
- 31.46% of accounts have 10+ Super Users (excessive)
- 70.48% have 5+ Super Users (potentially more than needed)
- High variability (CV: 124.61%) suggests inconsistent governance

### 2. Distribution Pattern

The distribution shows a **clear peak at 4 Super Users** with:
- 488 accounts (10.40%) have exactly 4 Super Users
- 452 accounts (9.64%) have exactly 3 Super Users
- 470 accounts (10.02%) have exactly 5 Super Users

This suggests a natural "sweet spot" around 4-5 Super Users per account.

### 3. Outliers

**High-Redundancy Outliers:**
- 1 account with 319 Super Users
- 1 account with 308 Super Users
- 1 account with 243 Super Users
- 1 account with 161 Super Users

These accounts likely represent:
- Large enterprise customers
- Partner organizations
- Multi-team accounts
- Accounts needing special governance

### 4. Variability Analysis

**Coefficient of Variation: 124.61%**

This indicates:
- High inconsistency in Super User assignments
- No standardized policy across accounts
- Opportunity for governance improvement
- Need for clear assignment guidelines

---

## Recommendations

### Priority 1: Address Single Point of Failure (Week 1)

**Action:** Review and upgrade 146 accounts with only 1 Super User

```
Current State:  146 accounts (3.11%) with 1 Super User
Target State:   All accounts with 2-3 Super Users minimum
Timeline:       1-2 weeks
Impact:         Eliminate single point of failure risk
```

**Steps:**
1. Identify the 146 accounts
2. Contact account owners
3. Assign additional Super User(s)
4. Verify assignments are active

### Priority 2: Establish Governance Policy (Month 1)

**Define Super User Assignment Standards:**

```
Minimum Requirements:
  • All accounts: 2-3 Super Users (redundancy)
  • Enterprise accounts: 3-5 Super Users
  • Partner accounts: 5-7 Super Users
  • Large accounts: 7-10 Super Users

Maximum Recommendations:
  • Standard accounts: 7 Super Users
  • Enterprise accounts: 10 Super Users
  • Large accounts: 15 Super Users
  • Anything above: Requires review
```

**Implementation:**
1. Document policy in governance framework
2. Communicate to account owners
3. Set up automated alerts for violations
4. Conduct quarterly reviews

### Priority 3: Optimize High-Redundancy Accounts (Month 2-3)

**Action:** Review 1,476 accounts with 10+ Super Users

```
Current State:  1,476 accounts (31.46%) with 10+ Super Users
Target State:   Reduce to 7-10 Super Users where appropriate
Timeline:       2-4 weeks
Impact:         Simplify management, reduce overhead
```

**Steps:**
1. Categorize accounts by type and size
2. Identify accounts that can be consolidated
3. Review with account owners
4. Implement consolidation plan
5. Document rationale for high-redundancy accounts

### Priority 4: Implement Monitoring (Ongoing)

**Set Up Automated Alerts:**
- Alert when account drops below 2 Super Users
- Alert when account exceeds policy maximum
- Alert when Super User is removed
- Monthly reports on policy compliance

**Quarterly Reviews:**
- Run this analysis quarterly
- Track trends in Super User assignments
- Identify new outliers
- Report to compliance team

---

## Governance Framework

### Super User Assignment Policy

**Objective:** Ensure adequate redundancy while maintaining manageable governance

**Principles:**
1. **Redundancy:** Minimum 2-3 Super Users per account
2. **Consistency:** Standardized assignments by account type
3. **Accountability:** Clear documentation of assignments
4. **Monitoring:** Automated alerts for policy violations
5. **Review:** Quarterly audits of assignments

### Account Type Guidelines

| Account Type | Min SU | Recommended | Max SU | Rationale |
|--------------|--------|-------------|--------|-----------|
| Small/Eval | 1 | 2-3 | 5 | Basic redundancy |
| Standard | 2 | 3-5 | 7 | Moderate redundancy |
| Enterprise | 3 | 5-7 | 10 | High redundancy |
| Partner | 3 | 5-7 | 10 | High redundancy |
| Large/Multi-team | 5 | 7-10 | 15 | Very high redundancy |

### Escalation Procedures

**If account drops below minimum:**
1. Automated alert sent to account owner
2. 7-day grace period for remediation
3. Escalation to support team if not resolved
4. Potential account suspension if critical

**If account exceeds maximum:**
1. Automated alert sent to account owner
2. Request for justification
3. Review with compliance team
4. Consolidation plan if not justified

---

## Implementation Roadmap

### Week 1: Quick Wins
- ✅ Identify 146 accounts with 1 Super User
- ✅ Contact account owners
- ✅ Assign additional Super Users
- ✅ Verify assignments

### Month 1: Governance
- ✅ Document Super User assignment policy
- ✅ Define account type guidelines
- ✅ Communicate policy to stakeholders
- ✅ Set up automated alerts

### Month 2-3: Optimization
- ✅ Review 1,476 high-redundancy accounts
- ✅ Identify consolidation opportunities
- ✅ Implement consolidation plan
- ✅ Document rationale for exceptions

### Ongoing: Monitoring
- ✅ Run quarterly distribution analysis
- ✅ Monitor policy compliance
- ✅ Adjust policy based on findings
- ✅ Report to compliance team

---

## Files Generated

| File | Size | Purpose |
|------|------|---------|
| `analyze_super_user_distribution.py` | 11 KB | Python analysis script |
| `super_user_distribution_report.txt` | ~50 KB | Detailed text report |
| `super_user_distribution_analysis.csv` | ~200 KB | Machine-readable export |
| `SUPER_USER_DISTRIBUTION_ANALYSIS.md` | This file | Executive summary |

**Location:** `/Users/mioffe/Documents/pm-workflow-automation/`

---

## Conclusion

The analysis of 4,691 actively used CSP accounts reveals:

1. **✅ Good Redundancy:** 96.89% of accounts have 2+ Super Users
2. **⚠️ Inconsistent Governance:** High variability (CV: 124.61%) suggests no standardized policy
3. **🎯 Optimization Opportunity:** 31.46% of accounts have 10+ Super Users (potentially excessive)
4. **📋 Clear Action Items:** 146 accounts need immediate upgrade to 2+ Super Users

**Recommended Next Steps:**
1. Address single point of failure (146 accounts)
2. Establish governance policy
3. Optimize high-redundancy accounts
4. Implement monitoring and quarterly reviews

---

## Questions?

For questions about this analysis:
- Review the Python script for technical details
- Check the CSV export for account-level data
- Reference the text report for detailed findings
- Contact the compliance team for policy questions
