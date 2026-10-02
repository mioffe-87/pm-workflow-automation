# MySQL Connection Quick Reference

## 📦 Installation

```bash
# Install all dependencies
pip install -r requirements_mysql.txt

# Or minimal install
pip install mysql-connector-python pandas
```

## 🔧 Setup (3 Steps)

### Step 1: Create `.env` file
```env
MYSQL_HOST=localhost
MYSQL_USER=root
MYSQL_PASSWORD=your_password
MYSQL_DATABASE=your_database
MYSQL_PORT=3306
```

### Step 2: Import and connect
```python
from mysql_connection import MySQLConnection
import os
from dotenv import load_dotenv

load_dotenv()

db = MySQLConnection(
    host=os.getenv('MYSQL_HOST'),
    user=os.getenv('MYSQL_USER'),
    password=os.getenv('MYSQL_PASSWORD'),
    database=os.getenv('MYSQL_DATABASE')
)
db.connect()
```

### Step 3: Query data
```python
# As list of dicts
results = db.execute_query("SELECT * FROM users LIMIT 5")

# As pandas DataFrame
df = db.query_to_dataframe("SELECT * FROM users")

# Insert/Update/Delete
db.execute_update("INSERT INTO users (name) VALUES ('John')")

# Disconnect
db.disconnect()
```

---

## 🎯 Common Operations

### Query to DataFrame
```python
df = db.query_to_dataframe("""
    SELECT user_id, email, created_at
    FROM users
    WHERE created_at > DATE_SUB(NOW(), INTERVAL 30 DAY)
""")
print(df.head())
df.to_csv('users.csv', index=False)
```

### Insert Data
```python
db.execute_update("""
    INSERT INTO users (name, email, created_at)
    VALUES ('John Doe', 'john@example.com', NOW())
""")
```

### Update Data
```python
db.execute_update("""
    UPDATE users
    SET email = 'newemail@example.com'
    WHERE user_id = 123
""")
```

### Delete Data
```python
db.execute_update("""
    DELETE FROM users
    WHERE created_at < DATE_SUB(NOW(), INTERVAL 1 YEAR)
""")
```

### Get Table List
```python
tables = db.get_tables()
print(tables)  # ['users', 'orders', 'products', ...]
```

---

## 🔌 Connection Drivers

| Driver | Install | Use Case |
|--------|---------|----------|
| `mysql-connector` | `pip install mysql-connector-python` | Official, full features |
| `pymysql` | `pip install pymysql` | Pure Python, lightweight |
| `sqlalchemy` | `pip install sqlalchemy` | ORM, connection pooling |

```python
# Use different driver
db = MySQLConnection(
    host='localhost',
    user='root',
    password='password',
    database='mydb',
    driver='pymysql'  # or 'mysql-connector' or 'sqlalchemy'
)
```

---

## 🐛 Troubleshooting

| Error | Solution |
|-------|----------|
| `Connection refused` | Check MySQL is running: `mysql -u root -p` |
| `Access denied` | Verify credentials in `.env` |
| `Module not found` | Install: `pip install mysql-connector-python` |
| `Timeout` | Add `connection_timeout=30` to config |

---

## 📚 Files Created

| File | Purpose |
|------|---------|
| `mysql_connection.py` | Main connection module |
| `run_mysql_cohort_query.py` | Example: Cohort analysis query |
| `requirements_mysql.txt` | Python dependencies |
| `MYSQL_SETUP.md` | Full setup guide |
| `.env` | Your credentials (create this) |

---

## ✅ Next Steps

1. **Install**: `pip install -r requirements_mysql.txt`
2. **Configure**: Create `.env` with your MySQL credentials
3. **Test**: `python run_mysql_cohort_query.py --test`
4. **Query**: `python run_mysql_cohort_query.py --query`
5. **Integrate**: Import `MySQLConnection` in your scripts

---

## 💡 Pro Tips

- **Always use `.env`** for credentials (add to `.gitignore`)
- **Use DataFrames** for data analysis with pandas
- **Export to CSV** for sharing: `df.to_csv('file.csv')`
- **Use connection pooling** for production (SQLAlchemy)
- **Test connection first** before running queries

---

## 🔗 Resources

- [MySQL Connector/Python](https://dev.mysql.com/doc/connector-python/en/)
- [PyMySQL](https://pymysql.readthedocs.io/)
- [SQLAlchemy](https://docs.sqlalchemy.org/)
- [MySQL Docs](https://dev.mysql.com/doc/)
