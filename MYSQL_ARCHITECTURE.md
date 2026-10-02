# MySQL Connection Architecture

## 🏗️ System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    Your Python Application                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │         mysql_connection.py (Main Module)                │   │
│  │  ┌────────────────────────────────────────────────────┐  │   │
│  │  │  MySQLConnection Class                             │  │   │
│  │  │  • connect()                                       │  │   │
│  │  │  • disconnect()                                   │  │   │
│  │  │  • execute_query()                                │  │   │
│  │  │  • execute_update()                               │  │   │
│  │  │  • query_to_dataframe()                           │  │   │
│  │  │  • get_tables()                                   │  │   │
│  │  └────────────────────────────────────────────────────┘  │   │
│  └──────────────────────────────────────────────────────────┘   │
│                              ↓                                    │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │         Driver Layer (Choose One)                        │   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐   │   │
│  │  │   mysql-     │  │   PyMySQL    │  │  SQLAlchemy  │   │   │
│  │  │  connector   │  │              │  │              │   │   │
│  │  └──────────────┘  └──────────────┘  └──────────────┘   │   │
│  └──────────────────────────────────────────────────────────┘   │
│                              ↓                                    │
└─────────────────────────────────────────────────────────────────┘
                              ↓
                    ┌─────────────────────┐
                    │   MySQL Server      │
                    │  (localhost:3306)   │
                    │                     │
                    │  ┌───────────────┐  │
                    │  │   Database    │  │
                    │  │   Tables      │  │
                    │  │   Data        │  │
                    │  └───────────────┘  │
                    └─────────────────────┘
```

## 📊 Data Flow

### Query Flow
```
Your Code
    ↓
MySQLConnection.query_to_dataframe()
    ↓
Driver (mysql-connector/pymysql/sqlalchemy)
    ↓
MySQL Server
    ↓
Results
    ↓
pandas DataFrame
    ↓
Your Analysis
```

### Insert/Update Flow
```
Your Code
    ↓
MySQLConnection.execute_update()
    ↓
Driver
    ↓
MySQL Server
    ↓
Commit/Rollback
    ↓
Confirmation
```

## 🔌 Driver Comparison

```
┌─────────────────┬──────────────────┬──────────────────┬──────────────────┐
│ Feature         │ mysql-connector  │ PyMySQL          │ SQLAlchemy       │
├─────────────────┼──────────────────┼──────────────────┼──────────────────┤
│ Official        │ ✅ Yes           │ ❌ No            │ ❌ No            │
│ Pure Python     │ ❌ No (C ext)    │ ✅ Yes           │ ✅ Yes           │
│ Performance     │ ⭐⭐⭐⭐⭐       │ ⭐⭐⭐⭐         │ ⭐⭐⭐⭐         │
│ Features        │ ⭐⭐⭐⭐⭐       │ ⭐⭐⭐           │ ⭐⭐⭐⭐⭐       │
│ ORM Support     │ ❌ No           │ ❌ No            │ ✅ Yes           │
│ Connection Pool │ ❌ No           │ ❌ No            │ ✅ Yes           │
│ Easy Install    │ ⭐⭐⭐⭐         │ ⭐⭐⭐⭐⭐       │ ⭐⭐⭐⭐         │
└─────────────────┴──────────────────┴──────────────────┴──────────────────┘
```

## 🔐 Security Architecture

```
┌──────────────────────────────────────────────────────────┐
│                  Your Application                         │
├──────────────────────────────────────────────────────────┤
│                                                            │
│  ┌────────────────────────────────────────────────────┐  │
│  │  .env File (Local Development)                     │  │
│  │  ├─ MYSQL_HOST=localhost                           │  │
│  │  ├─ MYSQL_USER=root                               │  │
│  │  ├─ MYSQL_PASSWORD=***                            │  │
│  │  └─ MYSQL_DATABASE=mydb                           │  │
│  └────────────────────────────────────────────────────┘  │
│         ↓ (loaded by python-dotenv)                       │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Environment Variables                             │  │
│  │  (in memory only, not in code)                     │  │
│  └────────────────────────────────────────────────────┘  │
│         ↓ (passed to MySQLConnection)                     │
│  ┌────────────────────────────────────────────────────┐  │
│  │  MySQLConnection                                   │  │
│  │  (credentials never logged or exposed)             │  │
│  └────────────────────────────────────────────────────┘  │
│         ↓ (encrypted connection)                          │
└──────────────────────────────────────────────────────────┘
                      ↓
            ┌─────────────────────┐
            │   MySQL Server      │
            │  (SSL/TLS optional) │
            └─────────────────────┘

⚠️  IMPORTANT:
  • .env file is in .gitignore (never committed)
  • Credentials never hardcoded in source
  • Environment variables only in memory
  • Use secrets manager for production
```

## 📦 Module Dependencies

```
mysql_connection.py
├── google.cloud (optional, for BigQuery)
├── mysql.connector (optional, if using mysql-connector driver)
├── pymysql (optional, if using pymysql driver)
├── sqlalchemy (optional, if using sqlalchemy driver)
├── pandas (for DataFrame support)
└── dotenv (for .env file support)

run_mysql_cohort_query.py
├── mysql_connection.py
├── pandas
├── dotenv
└── os, sys

hybrid_pipeline.py
├── google.cloud.bigquery
├── mysql_connection.py
├── pandas
└── dotenv
```

## 🔄 Workflow Examples

### Example 1: Simple Query
```
┌─────────────────────────────────────────────────────────┐
│ from mysql_connection import MySQLConnection            │
│ db = MySQLConnection(host, user, password, database)    │
│ db.connect()                                            │
│ df = db.query_to_dataframe("SELECT * FROM users")       │
│ print(df)                                               │
│ db.disconnect()                                         │
└─────────────────────────────────────────────────────────┘
         ↓
    ✅ Success: DataFrame with results
```

### Example 2: Insert Data
```
┌─────────────────────────────────────────────────────────┐
│ db.connect()                                            │
│ db.execute_update(                                      │
│   "INSERT INTO users (name) VALUES ('John')"            │
│ )                                                       │
│ db.disconnect()                                         │
└─────────────────────────────────────────────────────────┘
         ↓
    ✅ Success: Row inserted and committed
```

### Example 3: BigQuery → MySQL
```
┌──────────────────────────────────────────────────────────┐
│ 1. Query BigQuery                                        │
│    bq_client.query(sql).result().to_dataframe()          │
│                                                          │
│ 2. Process DataFrame                                     │
│    df.head(), df.describe(), etc.                        │
│                                                          │
│ 3. Insert to MySQL                                       │
│    for row in df.iterrows():                             │
│      db.execute_update(INSERT_SQL)                       │
│                                                          │
│ 4. Verify                                                │
│    mysql_df = db.query_to_dataframe(SELECT_SQL)          │
│    assert len(mysql_df) == len(df)                       │
└──────────────────────────────────────────────────────────┘
         ↓
    ✅ Success: Data synced from BigQuery to MySQL
```

## 🎯 Use Cases

```
┌─────────────────────────────────────────────────────────┐
│                    Use Cases                             │
├─────────────────────────────────────────────────────────┤
│                                                          │
│ 1. Local Development                                    │
│    └─ Use MySQL locally instead of BigQuery             │
│    └─ Faster iteration, no GCP costs                    │
│                                                          │
│ 2. Data Analysis                                        │
│    └─ Query MySQL, export to CSV/Excel                  │
│    └─ Use pandas for analysis                           │
│                                                          │
│ 3. Data Sync                                            │
│    └─ Sync data between BigQuery and MySQL              │
│    └─ Hybrid workflows                                  │
│                                                          │
│ 4. Reporting                                            │
│    └─ Query MySQL, generate reports                     │
│    └─ Schedule with cron/scheduler                      │
│                                                          │
│ 5. ETL Pipelines                                        │
│    └─ Extract from MySQL                                │
│    └─ Transform with pandas                             │
│    └─ Load to BigQuery/warehouse                        │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

## 📈 Performance Considerations

```
┌──────────────────────────────────────────────────────────┐
│ Optimization Tips                                        │
├──────────────────────────────────────────────────────────┤
│                                                          │
│ ✅ DO:                                                   │
│   • Use indexes on frequently queried columns            │
│   • Batch inserts when possible                          │
│   • Use connection pooling for production                │
│   • Limit result sets with WHERE/LIMIT                   │
│   • Use EXPLAIN to analyze queries                       │
│                                                          │
│ ❌ DON'T:                                                │
│   • SELECT * without LIMIT                              │
│   • Create new connection for each query                 │
│   • Use string concatenation for SQL (SQL injection)     │
│   • Ignore query performance                             │
│   • Store large files in database                        │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

## 🚀 Deployment Architecture

```
Development                Production
─────────────────────────────────────────────────────────

Local MySQL              Cloud MySQL (RDS/CloudSQL)
    ↓                              ↓
.env file                Secrets Manager
    ↓                              ↓
MySQLConnection          MySQLConnection
    ↓                              ↓
Python Script            Docker Container
    ↓                              ↓
Results                  Results
```

## 📚 File Organization

```
pm-workflow-automation/
├── mysql_connection.py           # Main module
├── run_mysql_cohort_query.py     # Example: Cohort analysis
├── hybrid_pipeline.py            # Example: BigQuery + MySQL
├── requirements_mysql.txt        # Dependencies
├── MYSQL_SETUP.md               # Full documentation
├── MYSQL_QUICK_REFERENCE.md     # Quick reference
├── MYSQL_SETUP_SUMMARY.txt      # This summary
├── MYSQL_ARCHITECTURE.md        # Architecture (this file)
├── .env                         # Your credentials (create this)
├── .gitignore                   # Add .env here
└── run_cohort_query.py          # Your existing BigQuery script
```

---

**Ready to connect?** Start with: `python run_mysql_cohort_query.py --test`
