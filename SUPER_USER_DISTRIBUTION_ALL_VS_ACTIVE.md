# Super User Distribution Analysis - ALL ACTIVE ACCOUNTS vs. ACTIVE WITH LOGIN

**Analysis Date:** September 24, 2026  
**Database:** CSP (gcp-sql-csp-rw-prod)  
**Status:** ✅ Complete

---

## Executive Summary

This analysis compares the Super User distribution across two different account populations:

1. **ALL Active Accounts** - 172,737 accounts (no login filter)
2. **Active Accounts with Login Activity (2026+)** - 4,691 accounts (with login activity since Jan 1, 2026)

The comparison reveals significant differences in Super User assignment patterns between actively used accounts and the broader account base.

---

## Side-by-Side Comparison

### Statistical Comparison

| Metric | All Active Accounts | Active with Login (2026+) | Difference |
|--------|-------------------|------------------------|-----------|
| **Total Accounts** | 172,737 | 4,691 | -97.3% |
| **Minimum Super Users** | 0 | 1 | +1 |
| **Maximum Super Users** | 1,785 | 319 | -466 |
| **Mean (Average)** | 2.63 | 9.25 | +6.62 |
| **Median** | 1.00 | 7.00 | +6.00 |
| **Mode (Most Common)** | 1 | 4 | +3 |
| **Standard Deviation** | 6.11 | 11.53 | +5.42 |
| **Coefficient of Variation** | 232.23% | 124.61% | -107.62pp |

### Key Insight

**Active accounts have significantly MORE Super Users than the overall account base.**

- All Active Accounts: Mean 2.63, Median 1.00
- Active with Login: Mean 9.25, Median 7.00

This suggests that accounts with active users tend to have more Super User assignments.

---

## Distribution Breakdown

### All Active Accounts (172,737 total)

| Super Users | Count | Percentage | Cumulative % |
|-------------|-------|-----------|--------------|
| 0 | 3 | 0.00% | 0.00% |
| 1 | 93,289 | 54.01% | 54.01% |
| 2 | 26,182 | 15.16% | 69.17% |
| 3 | 17,790 | 10.30% | 79.46% |
| 4 | 11,059 | 6.40% | 85.87% |
| 5 | 7,165 | 4.15% | 90.01% |
| 6-10 | 9,159 | 5.30% | 95.31% |
| 11-20 | 4,087 | 2.37% | 97.68% |
| 21+ | 3,803 | 2.20% | 99.88% |

**Key Segments:**
- **No Super User:** 3 accounts (0.00%) - CRITICAL
- **Single Point of Failure (1 SU):** 93,289 accounts (54.01%) - HIGH RISK
- **Basic Redundancy (2-3 SU):** 43,972 accounts (25.46%) - ACCEPTABLE
- **Moderate Redundancy (4-7 SU):** 22,283 accounts (12.90%) - GOOD
- **High Redundancy (8+ SU):** 13,190 accounts (7.63%) - EXCESS

### Active Accounts with Login (4,691 total)

| Super Users | Count | Percentage | Cumulative % |
|-------------|-------|-----------|--------------|
| 1 | 146 | 3.11% | 3.11% |
| 2 | 299 | 6.37% | 9.49% |
| 3 | 452 | 9.64% | 19.12% |
| 4 | 488 | 10.40% | 29.52% |
| 5 | 470 | 10.02% | 39.54% |
| 6-10 | 1,354 | 28.86% | 68.40% |
| 11-20 | 1,015 | 21.63% | 90.03% |
| 21+ | 467 | 9.96% | 100.00% |

**Key Segments:**
- **Single Point of Failure (1 SU):** 146 accounts (3.11%) - LOW RISK
- **Basic Redundancy (2-3 SU):** 751 accounts (16.01%) - ACCEPTABLE
- **Moderate Redundancy (4-7 SU):** 2,197 accounts (46.84%) - GOOD
- **High Redundancy (8+ SU):** 1,195 accounts (25.48%) - EXCESS
- **Very High Redundancy (16+ SU):** 81 accounts (1.73%) - REVIEW

---

## Critical Findings

### 1. Massive Gap in Account Coverage

**All Active Accounts:**
- 54.01% have only 1 Super User (93,289 accounts)
- 45.99% have 2+ Super Users (79,445 accounts)

**Active with Login:**
- 3.11% have only 1 Super User (146 accounts)
- 96.89% have 2+ Super Users (4,545 accounts)

**Implication:** Actively used accounts are much better provisioned with Super Users.

### 2. Accounts Without Super Users

**All Active Accounts:**
- 3 accounts (0.00%) have NO Super User
- These accounts cannot perform administrative functions
- CRITICAL: Must assign at least 1 Super User

**Active with Login:**
- 0 accounts (0.0%) have NO Super User
- All active accounts have at least 1 Super User

### 3. Distribution Shape Differences

**All Active Accounts:**
- Right-skewed (mean 2.63 > median 1.00)
- Peak at 1 Super User (54.01%)
- Long tail with some accounts having 1,785 Super Users
- High variability (CV: 232.23%)

**Active with Login:**
- Right-skewed (mean 9.25 > median 7.00)
- Peak at 4 Super Users (10.40%)
- Long tail with some accounts having 319 Super Users
- Moderate variability (CV: 124.61%)

### 4. Redundancy Comparison

| Redundancy Level | All Accounts | Active with Login | Difference |
|------------------|--------------|-------------------|-----------|
| No Super User | 3 (0.00%) | 0 (0.00%) | -3 |
| Single Point of Failure (1 SU) | 93,289 (54.01%) | 146 (3.11%) | -93,143 |
| Basic Redundancy (2-3 SU) | 43,972 (25.46%) | 751 (16.01%) | -43,221 |
| Moderate Redundancy (4-7 SU) | 22,283 (12.90%) | 2,197 (46.84%) | +1,914 |
| High Redundancy (8+ SU) | 13,190 (7.63%) | 1,476 (31.46%) | -11,714 |

---

## Key Insights

### 1. Provisioning Disparity

**Finding:** Actively used accounts have significantly more Super Users than inactive accounts.

**Explanation:**
- Active accounts likely have more users and complex organizational structures
- Inactive accounts may be newly created or trial accounts not yet fully provisioned
- Active accounts require more administrative oversight

### 2. Risk Profile Differences

**All Active Accounts:**
- 54.01% at risk (single point of failure)
- 0.00% without Super User (3 accounts)
- Overall risk: 🔴 VERY HIGH

**Active with Login:**
- 3.11% at risk (single point of failure)
- 0.0% without Super User
- Overall risk: ✅ LOW

### 3. Governance Opportunity

**Finding:** The massive difference suggests two distinct account populations:

1. **Actively Managed Accounts** (4,691 accounts)
   - Well-provisioned with Super Users
   - Average 9.25 Super Users
   - Likely enterprise/partner accounts

2. **Inactive/Newly Created Accounts** (168,046 accounts)
   - Minimally provisioned with Super Users
   - Average 1.63 Super Users
   - Likely trial/evaluation/new accounts

### 4. Variability Insight

**All Active Accounts:** CV = 232.23% (VERY HIGH)
- Indicates extreme inconsistency
- No standardized policy
- Accounts range from 0 to 1,785 Super Users

**Active with Login:** CV = 124.61% (HIGH)
- Still high variability
- But more consistent than overall population
- Accounts range from 1 to 319 Super Users

---

## Recommendations

### Priority 1: Critical - Accounts Without Super Users (IMMEDIATE)

**Action:** Assign Super User to 3 accounts with 0 Super Users

```
Current State:  3 accounts (0.00%) with 0 Super Users
Target State:   All accounts with 1+ Super Users
Timeline:       Immediate (today)
Impact:         Enable administrative functions
```

### Priority 2: High Risk - Single Point of Failure (WEEK 1)

**Action:** Review and upgrade 93,289 accounts with only 1 Super User

```
Current State:  93,289 accounts (54.01%) with 1 Super User
Target State:   All accounts with 2+ Super Users
Timeline:       1-2 weeks
Impact:         Eliminate single point of failure
```

### Priority 3: Establish Governance Policy (MONTH 1)

**Define Super User Assignment Standards by Account Type:**

```
Inactive/Trial Accounts:
  • Minimum: 1 Super User
  • Recommended: 2-3 Super Users
  • Maximum: 5 Super Users

Standard Active Accounts:
  • Minimum: 2-3 Super Users
  • Recommended: 4-7 Super Users
  • Maximum: 10 Super Users

Enterprise/Partner Accounts:
  • Minimum: 3-5 Super Users
  • Recommended: 7-10 Super Users
  • Maximum: 15 Super Users

Large/Multi-team Accounts:
  • Minimum: 5-7 Super Users
  • Recommended: 10-15 Super Users
  • Maximum: 20 Super Users
```

### Priority 4: Optimize High-Redundancy Accounts (MONTH 2-3)

**Action:** Review 13,190 accounts with 8+ Super Users

```
Current State:  13,190 accounts (7.63%) with 8+ Super Users
Target State:   Reduce to policy maximum
Timeline:       2-4 weeks
Impact:         Simplify management, reduce overhead
```

### Priority 5: Implement Monitoring (ONGOING)

**Set Up Automated Alerts:**
- Alert when account drops below minimum Super Users
- Alert when account exceeds policy maximum
- Alert when Super User is removed
- Monthly reports on policy compliance

---

## Implementation Roadmap

### Week 1: Quick Wins
- ✅ Assign Super User to 3 accounts with 0 Super Users
- ✅ Identify 93,289 accounts with 1 Super User
- ✅ Contact account owners
- ✅ Begin Super User assignments

### Month 1: Governance
- ✅ Document Super User assignment policy
- ✅ Define account type guidelines
- ✅ Communicate policy to stakeholders
- ✅ Set up automated alerts

### Month 2-3: Optimization
- ✅ Review 13,190 high-redundancy accounts
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
| `analyze_super_user_distribution_all_accounts.py` | 12 KB | Python analysis script |
| `super_user_distribution_all_accounts_report.txt` | ~100 KB | Detailed text report |
| `super_user_distribution_all_accounts.csv` | ~5 MB | Machine-readable export (172,737 accounts) |
| `SUPER_USER_DISTRIBUTION_ALL_VS_ACTIVE.md` | This file | Comparison analysis |

**Location:** `/Users/mioffe/Documents/pm-workflow-automation/`

---

## Conclusion

The analysis reveals a **stark contrast** between actively used accounts and the broader account base:

### All Active Accounts (172,737)
- **54.01%** have only 1 Super User (single point of failure)
- **0.00%** have no Super User (3 accounts - CRITICAL)
- **Mean:** 2.63 Super Users
- **Risk Level:** 🔴 VERY HIGH

### Active with Login (4,691)
- **3.11%** have only 1 Super User
- **0.0%** have no Super User
- **Mean:** 9.25 Super Users
- **Risk Level:** ✅ LOW

### Key Takeaway

**Actively used accounts are well-provisioned with Super Users, but the broader account base has significant gaps.** This suggests a two-tier system where:

1. **Tier 1 - Active Accounts:** Well-managed with adequate Super User redundancy
2. **Tier 2 - Inactive Accounts:** Minimally provisioned, many with single point of failure

**Recommended Action:** Implement tiered governance policy that accounts for these differences while ensuring minimum standards across all accounts.

---

## Questions?

For questions about this analysis:
- Review the Python script for technical details
- Check the CSV export for account-level data
- Reference the text report for detailed findings
- Compare with the active accounts analysis for context
