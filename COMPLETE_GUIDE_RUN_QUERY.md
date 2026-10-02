# ✅ Complete Guide: Run Cohort Query in Cursor

## 🎯 Summary: What You Need to Do

You have **everything prepared** to run the query. Here's what's needed:

---

## 📋 Current Status

| Component | Status | Details |
|-----------|--------|---------|
| **MySQL Connection** | ✅ Configured | Host: `gcp-sql-csp-rw-prod`, User: `mioffe`, DB: `csp` |
| **Python Script** | ✅ Ready | `run_mysql_cohort_query.py` |
| **SQL Query** | ✅ Ready | Embedded in Python script |
| **Dependencies** | ✅ Installed | `mysql-connector-python`, `pandas`, `python-dotenv` |
| **Network Access** | ⚠️ Limited | Sandbox restricts external connections |

---

## 🚀 How to Run the Query

### Option 1: Run in Cursor Terminal (Recommended)

```bash
cd /Users/mioffe/Documents/pm-workflow-automation

# Test connection first
python3.12 run_mysql_cohort_query.py --test

# Run the full analysis
python3.12 run_mysql_cohort_query.py --query
```

**Expected Output:**
```
✅ Connected to MySQL via mysql-connector: gcp-sql-csp-rw-prod:3306/csp
✅ Available tables (XX):
   • useraccount
   • event_log
   • membership
   ...

🔍 Running New User Cohort Analysis Query on MySQL...
✅ Query completed successfully!
📈 Analysis Period: 2024-01-01 to 2024-09-18
📊 Total cohorts analyzed: 250

NEW USER COHORT ANALYSIS - FIRST WEEK ACTIVATION
[Results table]

KEY INSIGHTS
📊 Overall Metrics:
   • Total Users Created: 12,345
   • Users Activated (First Week): 7,890
   • Overall Activation Rate: 63.92%
```

### Option 2: Run Directly in Python

Create a new file `run_query.py`:

```python
#!/usr/bin/env python3
import os
import sys
from mysql_connection import MySQLConnection
import pandas as pd
from dotenv import load_dotenv

load_dotenv()

config = {
    'host': os.getenv('MYSQL_HOST'),
    'user': os.getenv('MYSQL_USER'),
    'password': os.getenv('MYSQL_PASSWORD'),
    'database': os.getenv('MYSQL_DATABASE'),
    'port': int(os.getenv('MYSQL_PORT')),
}

db = MySQLConnection(**config)
if db.connect():
    query = """
    WITH new_users AS (
      SELECT
        DATE(u.createdon) AS user_creation_date,
        u.useraccountid,
        u.email,
        u.displayname,
        u.createdon AS creation_timestamp
      FROM useraccount u
      WHERE u.isdeleted = FALSE AND u.createdon IS NOT NULL
    ),
    first_week_logins AS (
      SELECT DISTINCT
        nu.useraccountid,
        nu.user_creation_date,
        MIN(el.event_time) AS first_login_time,
        TIMESTAMPDIFF(HOUR, nu.creation_timestamp, el.event_time) AS hours_to_first_login
      FROM new_users nu
      LEFT JOIN event_log el
        ON nu.useraccountid = CAST(el.identifier AS SIGNED)
        AND el.event_type IN ('login', 'authentication', 'session_start', 'user_login')
        AND el.event_time BETWEEN nu.creation_timestamp AND DATE_ADD(nu.creation_timestamp, INTERVAL 7 DAY)
        AND el.isdeleted = FALSE
      WHERE el.event_time IS NOT NULL
      GROUP BY nu.useraccountid, nu.user_creation_date, nu.creation_timestamp
    )
    SELECT
      nu.user_creation_date,
      COUNT(DISTINCT nu.useraccountid) AS total_users_created,
      COUNT(DISTINCT fwl.useraccountid) AS users_with_first_week_login,
      ROUND(100.0 * COUNT(DISTINCT fwl.useraccountid) / COUNT(DISTINCT nu.useraccountid), 2) AS first_week_activation_rate,
      ROUND(AVG(fwl.hours_to_first_login), 2) AS avg_hours_to_first_login,
      MIN(fwl.hours_to_first_login) AS min_hours_to_first_login,
      MAX(fwl.hours_to_first_login) AS max_hours_to_first_login
    FROM new_users nu
    LEFT JOIN first_week_logins fwl ON nu.useraccountid = fwl.useraccountid
    GROUP BY nu.user_creation_date
    ORDER BY nu.user_creation_date DESC
    LIMIT 100
    """
    
    df = db.query_to_dataframe(query)
    print(df)
    df.to_csv('cohort_results.csv', index=False)
    db.disconnect()
```

Then run:
```bash
python3.12 run_query.py
```

---

## 📊 What the Query Returns

| Column | Meaning |
|--------|---------|
| `user_creation_date` | Date users were created |
| `total_users_created` | How many new users that day |
| `users_with_first_week_login` | How many logged in within 7 days |
| `first_week_activation_rate` | % of new users who activated |
| `avg_hours_to_first_login` | Average hours to first login |
| `min_hours_to_first_login` | Fastest user to log in |
| `max_hours_to_first_login` | Slowest user to log in |

---

## 🔧 Troubleshooting

### Issue: "Unknown MySQL server host"
**Cause**: Network connectivity issue (sandbox restrictions)

**Solution**: 
- Run from your local machine (not in Cursor sandbox)
- Or request network access from your admin

### Issue: "Access denied for user"
**Cause**: Wrong credentials in `.env`

**Solution**:
- Verify `.env` has correct credentials
- Check MySQL user has query permissions

### Issue: "No results found"
**Cause**: `event_log` table doesn't have login events

**Solution**:
Run this to check available event types:
```python
from mysql_connection import MySQLConnection
import os
from dotenv import load_dotenv

load_dotenv()
db = MySQLConnection(
    host=os.getenv('MYSQL_HOST'),
    user=os.getenv('MYSQL_USER'),
    password=os.getenv('MYSQL_PASSWORD'),
    database=os.getenv('MYSQL_DATABASE'),
    port=int(os.getenv('MYSQL_PORT'))
)
db.connect()
df = db.query_to_dataframe("""
    SELECT DISTINCT event_type, COUNT(*) as count 
    FROM event_log 
    WHERE isdeleted = FALSE 
    GROUP BY event_type 
    ORDER BY count DESC 
    LIMIT 20
""")
print(df)
db.disconnect()
```

---

## 📁 Files You Have

| File | Purpose |
|------|---------|
| `mysql_connection.py` | MySQL connection wrapper |
| `run_mysql_cohort_query.py` | Main script to run query |
| `new_user_cohort_query.sql` | Raw SQL (reference) |
| `NEW_USER_COHORT_ANALYSIS.md` | Analysis guide |
| `RUN_QUERY_IN_CURSOR.md` | This guide |

---

## 🎯 Next Steps

### Immediate:
1. Open Cursor terminal
2. Run: `python3.12 run_mysql_cohort_query.py --test`
3. If connection works, run: `python3.12 run_mysql_cohort_query.py --query`

### If Connection Fails:
1. Check MySQL host is accessible from your network
2. Verify `.env` credentials
3. Contact your DBA to confirm MySQL access

### After Getting Results:
1. Review `mysql_cohort_analysis.csv`
2. Identify activation rate trends
3. Investigate low-activation cohorts
4. Set improvement targets

---

## 💡 Pro Tips

### Save output to file:
```bash
python3.12 run_mysql_cohort_query.py --query > analysis_output.txt
```

### View results:
```bash
cat mysql_cohort_analysis.csv
```

### Modify query for different time periods:
Edit `run_mysql_cohort_query.py` and change:
```python
# From:
DATE_ADD(DATE(u.createdon), INTERVAL 7 DAY)

# To (for 30 days):
DATE_ADD(DATE(u.createdon), INTERVAL 30 DAY)
```

### Filter by account type:
Add to WHERE clause:
```sql
AND sa.accounttypeid = 1  -- Replace with your account type
```

---

## 📞 Support

**If you get stuck:**
1. Check error message in console
2. Review troubleshooting section
3. Verify `.env` credentials
4. Check MySQL connectivity

All scripts have detailed error messages to help debug.

---

## ✅ Checklist

- [ ] Dependencies installed (`mysql-connector-python`, `pandas`, `python-dotenv`)
- [ ] `.env` file has correct MySQL credentials
- [ ] MySQL host is accessible from your network
- [ ] Run `python3.12 run_mysql_cohort_query.py --test` to verify connection
- [ ] Run `python3.12 run_mysql_cohort_query.py --query` to get results
- [ ] Review `mysql_cohort_analysis.csv` for insights
