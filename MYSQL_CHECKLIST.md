# MySQL Connection Setup Checklist

## ✅ Pre-Setup

- [ ] You have MySQL installed locally or access to a MySQL server
- [ ] You know your MySQL credentials (host, user, password, database)
- [ ] You have Python 3.7+ installed
- [ ] You're in your project directory: `/Users/mioffe/Documents/pm-workflow-automation`

## ✅ Installation

- [ ] Install dependencies: `pip install -r requirements_mysql.txt`
- [ ] Verify installation: `python -c "import mysql.connector; print('✅ mysql-connector installed')"`
- [ ] Verify pandas: `python -c "import pandas; print('✅ pandas installed')"`
- [ ] Verify dotenv: `python -c "from dotenv import load_dotenv; print('✅ python-dotenv installed')"`

## ✅ Configuration

- [ ] Create `.env` file in project root
- [ ] Add `MYSQL_HOST` to `.env`
- [ ] Add `MYSQL_USER` to `.env`
- [ ] Add `MYSQL_PASSWORD` to `.env`
- [ ] Add `MYSQL_DATABASE` to `.env`
- [ ] Add `MYSQL_PORT` to `.env` (default: 3306)
- [ ] Add `.env` to `.gitignore`
- [ ] Verify `.env` is not tracked: `git status | grep .env` (should be empty)

## ✅ Testing

- [ ] Test connection: `python run_mysql_cohort_query.py --test`
- [ ] Verify MySQL is running
- [ ] Verify credentials are correct
- [ ] Check MySQL version is displayed
- [ ] Check tables are listed

## ✅ Basic Usage

- [ ] Import MySQLConnection: `from mysql_connection import MySQLConnection`
- [ ] Create connection object
- [ ] Call `db.connect()`
- [ ] Execute a simple query: `db.execute_query("SELECT 1")`
- [ ] Call `db.disconnect()`

## ✅ Data Operations

- [ ] Query to DataFrame: `df = db.query_to_dataframe("SELECT * FROM table LIMIT 5")`
- [ ] Insert data: `db.execute_update("INSERT INTO table VALUES (...)")`
- [ ] Update data: `db.execute_update("UPDATE table SET col = val WHERE id = 1")`
- [ ] Delete data: `db.execute_update("DELETE FROM table WHERE id = 1")`
- [ ] Get tables: `tables = db.get_tables()`

## ✅ Advanced Features

- [ ] Run cohort analysis: `python run_mysql_cohort_query.py --query`
- [ ] Export to CSV: `df.to_csv('output.csv', index=False)`
- [ ] Use hybrid pipeline: `python hybrid_pipeline.py --example 1`
- [ ] Compare BigQuery and MySQL: `python hybrid_pipeline.py --example 3`

## ✅ Security

- [ ] `.env` file created and contains credentials
- [ ] `.env` added to `.gitignore`
- [ ] No credentials in Python code
- [ ] No credentials in git history
- [ ] Credentials only in environment variables

## ✅ Documentation

- [ ] Read `MYSQL_SETUP.md` for full documentation
- [ ] Read `MYSQL_QUICK_REFERENCE.md` for quick reference
- [ ] Read `MYSQL_ARCHITECTURE.md` for architecture overview
- [ ] Bookmark MySQL docs: https://dev.mysql.com/doc/

## ✅ Integration

- [ ] Import `MySQLConnection` in your scripts
- [ ] Replace BigQuery queries with MySQL equivalents
- [ ] Test queries return expected results
- [ ] Update your analysis scripts to use MySQL
- [ ] Export results to CSV for sharing

## ✅ Troubleshooting

- [ ] Connection refused? Check MySQL is running
- [ ] Access denied? Check credentials in `.env`
- [ ] Module not found? Run `pip install -r requirements_mysql.txt`
- [ ] Timeout? Check network connectivity
- [ ] Query errors? Check MySQL syntax (not BigQuery syntax)

## 📋 Files Created

| File | Purpose | Status |
|------|---------|--------|
| `mysql_connection.py` | Main connection module | ✅ |
| `run_mysql_cohort_query.py` | Example: Cohort analysis | ✅ |
| `hybrid_pipeline.py` | Example: BigQuery + MySQL | ✅ |
| `requirements_mysql.txt` | Dependencies | ✅ |
| `MYSQL_SETUP.md` | Full documentation | ✅ |
| `MYSQL_QUICK_REFERENCE.md` | Quick reference | ✅ |
| `MYSQL_SETUP_SUMMARY.txt` | Setup summary | ✅ |
| `MYSQL_ARCHITECTURE.md` | Architecture guide | ✅ |
| `MYSQL_CHECKLIST.md` | This checklist | ✅ |
| `.env` | Your credentials | ⏳ Create this |

## 🚀 Quick Start Commands

```bash
# 1. Install dependencies
pip install -r requirements_mysql.txt

# 2. Create .env file with your credentials
# Edit the file with your actual MySQL credentials

# 3. Test connection
python run_mysql_cohort_query.py --test

# 4. Run cohort analysis
python run_mysql_cohort_query.py --query

# 5. Try hybrid pipeline
python hybrid_pipeline.py --example 1
```

## 📞 Support

- **Connection Issues?** Check `MYSQL_SETUP.md` → Troubleshooting
- **Syntax Questions?** Check `MYSQL_QUICK_REFERENCE.md`
- **Architecture Questions?** Check `MYSQL_ARCHITECTURE.md`
- **MySQL Docs?** https://dev.mysql.com/doc/

## ✨ You're All Set!

Once you've completed all checkboxes above, you're ready to:
- ✅ Query MySQL from Python
- ✅ Export data to CSV
- ✅ Analyze data with pandas
- ✅ Sync data between BigQuery and MySQL
- ✅ Build data pipelines

**Next Step:** Run `python run_mysql_cohort_query.py --test` to verify everything works!

---

**Last Updated:** 2026-09-17
**Status:** Ready for use ✅
