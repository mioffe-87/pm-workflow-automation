# 🎯 QUICK START: Run Cohort Query in Cursor

## What You Need to Do (3 Steps)

### Step 1: Open Cursor Terminal
```bash
cd /Users/mioffe/Documents/pm-workflow-automation
```

### Step 2: Test MySQL Connection
```bash
python3.12 run_mysql_cohort_query.py --test
```

### Step 3: Run the Cohort Analysis
```bash
python3.12 run_mysql_cohort_query.py --query
```

---

## ✅ Everything is Already Set Up

| Item | Status |
|------|--------|
| Python script | ✅ Ready (`run_mysql_cohort_query.py`) |
| SQL query | ✅ Ready (embedded in script) |
| MySQL credentials | ✅ Configured in `.env` |
| Dependencies | ✅ Installed (`mysql-connector-python`, `pandas`, `python-dotenv`) |

---

## 📊 What You'll Get

The query will show:
- **How many users were created** per day
- **How many logged in within 7 days** (first-week activation)
- **Activation rate %** (key metric)
- **Time to first login** (average, median, min, max)
- **Results saved to** `mysql_cohort_analysis.csv`

---

## 🔍 Example Output

```
NEW USER COHORT ANALYSIS - FIRST WEEK ACTIVATION

user_creation_date | total_users_created | users_with_first_week_login | first_week_activation_rate
2024-09-15         | 45                  | 28                          | 62.22%
2024-09-14         | 52                  | 31                          | 59.62%
2024-09-13         | 38                  | 19                          | 50.00%

KEY INSIGHTS
📊 Overall Metrics:
   • Total Users Created: 12,345
   • Users Activated (First Week): 7,890
   • Overall Activation Rate: 63.92%
```

---

## ⚠️ If Connection Fails

**Error**: "Unknown MySQL server host"
- **Cause**: Network connectivity issue
- **Solution**: Run from your local machine (not Cursor sandbox)

**Error**: "Access denied for user"
- **Cause**: Wrong credentials
- **Solution**: Check `.env` file has correct MySQL password

---

## 📁 Reference Files

- `COMPLETE_GUIDE_RUN_QUERY.md` - Full detailed guide
- `RUN_QUERY_IN_CURSOR.md` - Detailed setup instructions
- `NEW_USER_COHORT_ANALYSIS.md` - Analysis interpretation guide
- `new_user_cohort_query.sql` - Raw SQL query
- `mysql_connection.py` - Connection wrapper
- `run_mysql_cohort_query.py` - Main script

---

## 🚀 Ready to Go!

Just run these 3 commands in Cursor terminal:

```bash
cd /Users/mioffe/Documents/pm-workflow-automation
python3.12 run_mysql_cohort_query.py --test
python3.12 run_mysql_cohort_query.py --query
```

That's it! 🎉
