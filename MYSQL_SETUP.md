# MySQL Connection Setup Guide

## 🚀 Quick Start

### 1. Install Dependencies

```bash
# Install MySQL drivers and dependencies
pip install -r requirements_mysql.txt

# Or install individually:
pip install mysql-connector-python pandas sqlalchemy
```

### 2. Configure Connection

**Option A: Environment Variables (.env file)**

Create a `.env` file in your project root:

```env
MYSQL_HOST=localhost
MYSQL_USER=root
MYSQL_PASSWORD=your_secure_password
MYSQL_DATABASE=your_database
MYSQL_PORT=3306
```

Then load it in your Python code:

```python
from dotenv import load_dotenv
import os

load_dotenv()

config = {
    'host': os.getenv('MYSQL_HOST'),
    'user': os.getenv('MYSQL_USER'),
    'password': os.getenv('MYSQL_PASSWORD'),
    'database': os.getenv('MYSQL_DATABASE'),
    'port': int(os.getenv('MYSQL_PORT', 3306))
}
```

**Option B: Direct Configuration**

```python
from mysql_connection import MySQLConnection

config = {
    'host': 'localhost',
    'user': 'root',
    'password': 'your_password',
    'database': 'your_database',
    'port': 3306
}

db = MySQLConnection(**config)
db.connect()
```

### 3. Basic Usage

```python
from mysql_connection import MySQLConnection

# Connect
db = MySQLConnection(
    host='localhost',
    user='root',
    password='password',
    database='mydb'
)
db.connect()

# Query as list of dicts
results = db.execute_query("SELECT * FROM users LIMIT 5")
print(results)

# Query as pandas DataFrame
df = db.query_to_dataframe("SELECT * FROM users")
print(df.head())

# Insert/Update/Delete
db.execute_update("INSERT INTO users (name, email) VALUES ('John', 'john@example.com')")

# Get table list
tables = db.get_tables()
print(tables)

# Disconnect
db.disconnect()
```

---

## 🔌 Connection Methods

### Method 1: mysql-connector-python (Official)

**Pros:**
- Official MySQL connector
- Full feature support
- Good performance

**Cons:**
- Requires C dependencies on some systems
- Slightly heavier

```python
db = MySQLConnection(
    host='localhost',
    user='root',
    password='password',
    database='mydb',
    driver='mysql-connector'
)
```

### Method 2: PyMySQL (Pure Python)

**Pros:**
- Pure Python, no C dependencies
- Easy to install
- Good for development

**Cons:**
- Slightly slower than mysql-connector
- Less feature-rich

```python
db = MySQLConnection(
    host='localhost',
    user='root',
    password='password',
    database='mydb',
    driver='pymysql'
)
```

### Method 3: SQLAlchemy (ORM)

**Pros:**
- Database-agnostic
- Built-in connection pooling
- ORM capabilities

**Cons:**
- More overhead
- Steeper learning curve

```python
db = MySQLConnection(
    host='localhost',
    user='root',
    password='password',
    database='mydb',
    driver='sqlalchemy'
)
```

---

## 🔐 Security Best Practices

### ❌ DON'T: Hardcode credentials

```python
# BAD - Never do this!
db = MySQLConnection(
    host='localhost',
    user='root',
    password='my_secret_password',  # ❌ Exposed!
    database='mydb'
)
```

### ✅ DO: Use environment variables

```python
# GOOD - Use environment variables
import os
from dotenv import load_dotenv

load_dotenv()

db = MySQLConnection(
    host=os.getenv('MYSQL_HOST'),
    user=os.getenv('MYSQL_USER'),
    password=os.getenv('MYSQL_PASSWORD'),
    database=os.getenv('MYSQL_DATABASE')
)
```

### ✅ DO: Use .gitignore for .env

```bash
# Add to .gitignore
.env
.env.local
*.key
```

---

## 🐳 Docker Setup (Optional)

### Run MySQL in Docker

```bash
# Pull and run MySQL container
docker run --name mysql-container \
  -e MYSQL_ROOT_PASSWORD=root_password \
  -e MYSQL_DATABASE=mydb \
  -p 3306:3306 \
  -d mysql:8.0

# Connect to container
docker exec -it mysql-container mysql -u root -p

# Stop container
docker stop mysql-container
```

### Docker Compose (docker-compose.yml)

```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    container_name: mysql-dev
    environment:
      MYSQL_ROOT_PASSWORD: root_password
      MYSQL_DATABASE: mydb
      MYSQL_USER: devuser
      MYSQL_PASSWORD: devpassword
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      timeout: 20s
      retries: 10

volumes:
  mysql_data:
```

Start with: `docker-compose up -d`

---

## 📊 Common Queries

### Get Database Info

```python
# List all tables
tables = db.get_tables()
print(f"Tables: {tables}")

# Get table structure
query = """
SELECT COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, COLUMN_KEY
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = %s AND TABLE_NAME = %s
"""
db.cursor.execute(query, (db.database, 'users'))
columns = db.cursor.fetchall()
```

### Export to CSV

```python
df = db.query_to_dataframe("SELECT * FROM users")
df.to_csv('users_export.csv', index=False)
print("✅ Exported to users_export.csv")
```

### Bulk Insert

```python
import pandas as pd

# Read CSV
df = pd.read_csv('data.csv')

# Insert rows
for idx, row in df.iterrows():
    query = f"""
    INSERT INTO users (name, email, created_at)
    VALUES ('{row['name']}', '{row['email']}', NOW())
    """
    db.execute_update(query)

print(f"✅ Inserted {len(df)} rows")
```

---

## 🐛 Troubleshooting

### Connection Refused

```
Error: Connection refused
```

**Solutions:**
- Check MySQL is running: `mysql -u root -p`
- Verify host/port: `telnet localhost 3306`
- Check firewall rules

### Access Denied

```
Error: Access denied for user 'root'@'localhost'
```

**Solutions:**
- Verify credentials
- Check user permissions: `SHOW GRANTS FOR 'user'@'localhost';`
- Reset password if needed

### Module Not Found

```
ModuleNotFoundError: No module named 'mysql'
```

**Solutions:**
- Install: `pip install mysql-connector-python`
- Check virtual environment is activated

### Timeout

```
Error: Connection timeout
```

**Solutions:**
- Increase timeout: Add `connection_timeout=30` to config
- Check network connectivity
- Verify MySQL max_connections setting

---

## 📚 Resources

- [MySQL Connector/Python Docs](https://dev.mysql.com/doc/connector-python/en/)
- [PyMySQL Docs](https://pymysql.readthedocs.io/)
- [SQLAlchemy Docs](https://docs.sqlalchemy.org/)
- [MySQL Documentation](https://dev.mysql.com/doc/)

---

## 🎯 Next Steps

1. **Install dependencies**: `pip install -r requirements_mysql.txt`
2. **Set up .env file** with your MySQL credentials
3. **Test connection**: Run `python mysql_connection.py` (uncomment examples)
4. **Integrate with your project**: Import `MySQLConnection` in your scripts
5. **Migrate BigQuery queries**: Convert your existing BigQuery queries to MySQL syntax

---

## 💡 Tips for Migrating from BigQuery

### BigQuery → MySQL Syntax Changes

| BigQuery | MySQL |
|----------|-------|
| `DATE(timestamp)` | `DATE(timestamp)` ✓ Same |
| `DATE_ADD(date, INTERVAL 7 DAY)` | `DATE_ADD(date, INTERVAL 7 DAY)` ✓ Same |
| `TIMESTAMP_DIFF(ts1, ts2, HOUR)` | `TIMESTAMPDIFF(HOUR, ts2, ts1)` |
| `CAST(x AS INT64)` | `CAST(x AS SIGNED)` |
| `APPROX_QUANTILES()` | Use `PERCENTILE_CONT()` or `PERCENTILE_DISC()` |
| `OFFSET(n)` | Not needed in MySQL |
| Backticks for identifiers | Same ✓ |

### Example Migration

**BigQuery:**
```sql
SELECT
  DATE(u.createdon) AS user_creation_date,
  COUNT(DISTINCT u.useraccountid) AS total_users,
  APPROX_QUANTILES(el.event_time, 100)[OFFSET(50)] AS median_time
FROM `project.dataset.useraccount` u
LEFT JOIN `project.dataset.event_log` el
  ON u.useraccountid = CAST(el.identifier AS INT64)
GROUP BY user_creation_date
```

**MySQL:**
```sql
SELECT
  DATE(u.createdon) AS user_creation_date,
  COUNT(DISTINCT u.useraccountid) AS total_users,
  PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY el.event_time) AS median_time
FROM useraccount u
LEFT JOIN event_log el
  ON u.useraccountid = CAST(el.identifier AS SIGNED)
GROUP BY user_creation_date
```
