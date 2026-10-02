# How to Run the Cohort Query in Cursor

## Current Status ✅

You have **two options** to run the query:

### Option 1: MySQL (Recommended - Already Configured) ✅
- **Status**: Ready to use
- **Connection**: Already configured in `.env`
- **Database**: `csp` on `gcp-sql-csp-rw-prod`
- **Script**: `run_mysql_cohort_query.py`

### Option 2: BigQuery (Requires Permissions)
- **Status**: Blocked - needs `bigquery.jobs.create` permission
- **Action**: Request permissions from GCP admin (see `SLACK_MESSAGE_GCP_ADMIN.md`)

---

## 🚀 Quick Start: Run Query in Cursor NOW

### Step 1: Install Required Dependencies

```bash
pip install mysql-connector-python pandas python-dotenv
```

Or if you prefer PyMySQL:
```bash
pip install pymysql pandas python-dotenv
```

### Step 2: Verify Your .env File

Your `.env` already has:
```
MYSQL_HOST=gcp-sql-csp-rw-prod
MYSQL_USER=mioffe
MYSQL_PASSWORD=***
MYSQL_DATABASE=csp
MYSQL_PORT=3306
```

✅ This is already configured!

### Step 3: Test the Connection

Run this in Cursor terminal:

```bash
python run_mysql_cohort_query.py --test
```

**Expected output:**
```
✅ Connected to MySQL via mysql-connector: gcp-sql-csp-rw-prod:3306/csp
✅ Available tables (XX):
   • useraccount
   • event_log
   • membership
   • ...
```

### Step 4: Run the Cohort Analysis Query

```bash
python run_mysql_cohort_query.py --query
```

**Expected output:**
```
🔍 Running New User Cohort Analysis Query on MySQL...
✅ Query completed successfully!
📈 Analysis Period: 2024-01-01 to 2024-09-18
📊 Total cohorts analyzed: 250

NEW USER COHORT ANALYSIS - FIRST WEEK ACTIVATION
[Results table with metrics]

KEY INSIGHTS
📊 Overall Metrics:
   • Total Users Created: 12,345
   • Users Activated (First Week): 7,890
   • Overall Activation Rate: 63.92%
```

---

## 📋 What Each Script Does

| Script | Purpose | Status |
|--------|---------|--------|
| `mysql_connection.py` | MySQL connection wrapper | ✅ Ready |
| `run_mysql_cohort_query.py` | Run cohort analysis on MySQL | ✅ Ready |
| `run_cohort_query.py` | Run cohort analysis on BigQuery | ⏳ Waiting for permissions |
| `new_user_cohort_query.sql` | Raw SQL query | ✅ Ready |

---

## 🔍 Understanding the Query

The query calculates:

```
For each day users were created:
├── How many users were created
├── How many logged in within 7 days
├── What % activated (first_week_activation_rate)
├── Average time to first login
├── Median time to first login
└── Min/Max time to first login
```

**Key Metric**: `first_week_activation_rate` = % of new users who logged in within 7 days

---

## 📊 Output Files

After running the query, you'll get:

- **`mysql_cohort_analysis.csv`** - Full results table
- **Console output** - Summary statistics and insights

---

## ⚠️ Troubleshooting

### Issue: "mysql-connector-python not installed"
**Solution:**
```bash
pip install mysql-connector-python
```

### Issue: "Connection refused"
**Solution:**
1. Check MySQL host is accessible: `ping gcp-sql-csp-rw-prod`
2. Verify credentials in `.env`
3. Check MySQL is running

### Issue: "No results found"
**Possible causes:**
1. `event_log` table doesn't have login events
2. `identifier` field doesn't contain user IDs
3. Event types are different than expected

**Solution:**
Run this to check available event types:
```bash
python -c "
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
df = db.query_to_dataframe('SELECT DISTINCT event_type, COUNT(*) as count FROM event_log WHERE isdeleted = FALSE GROUP BY event_type ORDER BY count DESC LIMIT 20')
print(df)
db.disconnect()
"
```

---

## 🎯 Next Steps

### Immediate (Do Now):
1. ✅ Install dependencies: `pip install mysql-connector-python pandas python-dotenv`
2. ✅ Test connection: `python run_mysql_cohort_query.py --test`
3. ✅ Run query: `python run_mysql_cohort_query.py --query`

### After Getting Results:
1. Review the CSV output
2. Identify activation rate trends
3. Investigate low-activation cohorts
4. Set targets for improvement

### For BigQuery (Later):
1. Request permissions from GCP admin
2. Once approved, run: `python run_cohort_query.py`

---

## 💡 Pro Tips

### Run in Cursor Terminal:
```bash
# Test connection
python run_mysql_cohort_query.py --test

# Run full analysis
python run_mysql_cohort_query.py --query

# Save output to file
python run_mysql_cohort_query.py --query > cohort_analysis_output.txt
```

### View Results:
```bash
# View CSV in terminal
cat mysql_cohort_analysis.csv

# Open in Excel/Sheets
open mysql_cohort_analysis.csv
```

### Modify Query:
Edit `run_mysql_cohort_query.py` to:
- Change date ranges
- Filter by account type
- Add geographic dimensions
- Extend to 30/60/90-day windows

---

## 📞 Support

If you hit issues:
1. Check error message in console
2. Review troubleshooting section above
3. Check `.env` credentials
4. Verify MySQL connectivity

All scripts have detailed error messages to help debug.
