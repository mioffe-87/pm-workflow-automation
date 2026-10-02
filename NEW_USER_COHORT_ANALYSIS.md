# New User Cohort Analysis: First-Week Activation Report

## Executive Summary
This analysis measures user acquisition and early engagement by tracking:
- **How many new users were created** by creation date
- **How many logged in within 7 days** of account creation
- **Time to first login** metrics

---

## SQL Query

```sql
WITH new_users AS (
  -- Get all new users by creation date
  SELECT
    DATE(u.createdon) AS user_creation_date,
    u.useraccountid,
    u.email,
    u.displayname,
    u.createdon AS creation_timestamp,
    DATE_ADD(DATE(u.createdon), INTERVAL 7 DAY) AS first_week_end_date
  FROM
    `itp-aia-datalake.csp.useraccount` u
  WHERE
    u.isdeleted = FALSE
    AND u.createdon IS NOT NULL
),

first_week_logins AS (
  -- Identify users who had login events in their first week
  SELECT DISTINCT
    nu.useraccountid,
    nu.user_creation_date,
    MIN(el.event_time) AS first_login_time,
    TIMESTAMP_DIFF(el.event_time, nu.creation_timestamp, HOUR) AS hours_to_first_login
  FROM
    new_users nu
  LEFT JOIN
    `itp-aia-datalake.csp.event_log` el
    ON nu.useraccountid = CAST(el.identifier AS INT64)
    AND el.event_type IN ('login', 'authentication', 'session_start', 'user_login')
    AND el.event_time BETWEEN nu.creation_timestamp AND TIMESTAMP_ADD(nu.creation_timestamp, INTERVAL 7 DAY)
    AND el.isdeleted = FALSE
  WHERE
    el.event_time IS NOT NULL
  GROUP BY
    nu.useraccountid,
    nu.user_creation_date,
    nu.creation_timestamp
)

-- Final aggregation by cohort
SELECT
  nu.user_creation_date,
  COUNT(DISTINCT nu.useraccountid) AS total_users_created,
  COUNT(DISTINCT fwl.useraccountid) AS users_with_first_week_login,
  ROUND(100.0 * COUNT(DISTINCT fwl.useraccountid) / COUNT(DISTINCT nu.useraccountid), 2) AS first_week_activation_rate,
  ROUND(AVG(fwl.hours_to_first_login), 2) AS avg_hours_to_first_login,
  MIN(fwl.hours_to_first_login) AS min_hours_to_first_login,
  MAX(fwl.hours_to_first_login) AS max_hours_to_first_login,
  APPROX_QUANTILES(fwl.hours_to_first_login, 100)[OFFSET(50)] AS median_hours_to_first_login
FROM
  new_users nu
LEFT JOIN
  first_week_logins fwl
  ON nu.useraccountid = fwl.useraccountid
GROUP BY
  nu.user_creation_date
ORDER BY
  nu.user_creation_date DESC
```

---

## Output Columns Explained

| Column | Definition | Use Case |
|--------|-----------|----------|
| `user_creation_date` | Date user account was created | Cohort grouping |
| `total_users_created` | Count of new users on that date | Acquisition volume |
| `users_with_first_week_login` | Users who logged in within 7 days | Early engagement |
| `first_week_activation_rate` | % of new users who logged in (0-100) | **Key metric** - activation efficiency |
| `avg_hours_to_first_login` | Average hours from creation to first login | Engagement speed |
| `min_hours_to_first_login` | Fastest user to log in | Best case scenario |
| `max_hours_to_first_login` | Slowest user to log in | Outlier detection |
| `median_hours_to_first_login` | 50th percentile time to login | Typical user behavior |

---

## Key Metrics to Track

### 1. **First-Week Activation Rate** (Primary KPI)
- **Definition**: % of new users who log in within 7 days
- **Target**: Typically 40-60% for SaaS products
- **Interpretation**: 
  - >60% = Strong onboarding
  - 30-60% = Moderate engagement
  - <30% = Potential onboarding friction

### 2. **Time to First Login** (Engagement Speed)
- **Median hours**: Typical user behavior
- **Average hours**: Influenced by outliers
- **Target**: <24 hours for optimal onboarding

### 3. **Cohort Trends**
- Compare activation rates across different creation dates
- Identify if recent cohorts have better/worse activation
- Detect seasonal patterns

---

## Troubleshooting Notes

### If query returns no results:

**Step 1: Verify event_log has login data**
```sql
SELECT DISTINCT event_type, COUNT(*) as count
FROM `itp-aia-datalake.csp.event_log`
WHERE isdeleted = FALSE
GROUP BY event_type
ORDER BY count DESC
LIMIT 20
```

**Step 2: Check identifier field format**
```sql
SELECT 
  identifier,
  event_type,
  event_time,
  COUNT(*) as count
FROM `itp-aia-datalake.csp.event_log`
WHERE isdeleted = FALSE
  AND event_type LIKE '%login%'
GROUP BY identifier, event_type, event_time
LIMIT 10
```

**Step 3: Alternative - Use membership activity instead of event_log**
```sql
WITH new_users AS (
  SELECT
    DATE(u.createdon) AS user_creation_date,
    u.useraccountid,
    u.createdon AS creation_timestamp
  FROM `itp-aia-datalake.csp.useraccount` u
  WHERE u.isdeleted = FALSE AND u.createdon IS NOT NULL
),

first_week_activity AS (
  SELECT DISTINCT
    nu.useraccountid,
    nu.user_creation_date,
    MIN(m.createdon) AS first_activity_time,
    TIMESTAMP_DIFF(m.createdon, nu.creation_timestamp, HOUR) AS hours_to_first_activity
  FROM new_users nu
  LEFT JOIN `itp-aia-datalake.csp.membership` m
    ON nu.useraccountid = m.useraccountid
    AND m.createdon BETWEEN nu.creation_timestamp AND TIMESTAMP_ADD(nu.creation_timestamp, INTERVAL 7 DAY)
    AND m.isdeleted = FALSE
  WHERE m.createdon IS NOT NULL
  GROUP BY nu.useraccountid, nu.user_creation_date, nu.creation_timestamp
)

SELECT
  nu.user_creation_date,
  COUNT(DISTINCT nu.useraccountid) AS total_users_created,
  COUNT(DISTINCT fwa.useraccountid) AS users_with_first_week_activity,
  ROUND(100.0 * COUNT(DISTINCT fwa.useraccountid) / COUNT(DISTINCT nu.useraccountid), 2) AS first_week_activity_rate,
  ROUND(AVG(fwa.hours_to_first_activity), 2) AS avg_hours_to_first_activity
FROM new_users nu
LEFT JOIN first_week_activity fwa ON nu.useraccountid = fwa.useraccountid
GROUP BY nu.user_creation_date
ORDER BY nu.user_creation_date DESC
```

---

## How to Run This Analysis

### Option 1: BigQuery Console
1. Go to [BigQuery Console](https://console.cloud.google.com/bigquery)
2. Copy the SQL query above
3. Click "Run" and wait for results
4. Export results to CSV/Sheets

### Option 2: Python Script
```bash
python3 analyze_new_user_cohorts.py
```

### Option 3: Google Cloud CLI
```bash
bq query --use_legacy_sql=false < query.sql > results.csv
```

---

## Expected Output Format

| user_creation_date | total_users_created | users_with_first_week_login | first_week_activation_rate | avg_hours_to_first_login | median_hours_to_first_login |
|---|---|---|---|---|---|
| 2024-09-15 | 45 | 28 | 62.22 | 18.5 | 12 |
| 2024-09-14 | 52 | 31 | 59.62 | 22.3 | 16 |
| 2024-09-13 | 38 | 19 | 50.00 | 36.7 | 28 |

---

## Analysis Recommendations

1. **Segment by Account Type**: Add `sa.accounttypeid` to see if certain customer types activate faster
2. **Geographic Analysis**: Join with `country` table to see regional differences
3. **Cohort Retention**: Extend to 30/60/90-day windows to measure retention
4. **Funnel Analysis**: Track progression from creation → first login → first action
5. **Correlation Analysis**: Compare activation rates with onboarding emails, support tickets, etc.

---

## Next Steps

1. **Run the query** in BigQuery Console
2. **Share results** - I can help interpret the data
3. **Identify bottlenecks** - If activation rate is low, we can investigate why
4. **Set targets** - Define what "good" activation looks like for your business
5. **Monitor trends** - Track this metric weekly/monthly
