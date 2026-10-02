# Connect to Your Existing MySQL

I can see you have **MySQL Workbench** installed on your computer, which means you likely have MySQL running somewhere.

## 🔍 Finding Your MySQL Connection Details

### Option 1: Check MySQL Workbench
1. Open **MySQL Workbench**
2. Look at your saved connections
3. Note the **Host**, **User**, **Password**, and **Database**

### Option 2: Check System Preferences
1. Open **System Preferences** → **MySQL**
2. Check if MySQL is running
3. Note the port (usually 3306)

### Option 3: Check Common Locations
- **Homebrew**: `/usr/local/mysql/bin/mysql`
- **DMG Installer**: `/usr/local/mysql/bin/mysql`
- **Docker**: Check Docker Desktop

---

## 📝 Create .env File

Once you have your connection details, create a `.env` file:

```bash
cat > /Users/mioffe/Documents/pm-workflow-automation/.env << 'EOF'
MYSQL_HOST=localhost
MYSQL_USER=root
MYSQL_PASSWORD=your_password
MYSQL_DATABASE=your_database
MYSQL_PORT=3306
EOF
```

Replace:
- `your_password` - Your MySQL root password
- `your_database` - Your database name (or use `csp` if you want to create it)

---

## 🧪 Test Connection

```bash
cd /Users/mioffe/Documents/pm-workflow-automation
python3 run_mysql_cohort_query.py --test
```

---

## 🆘 If Connection Fails

### Error: "Can't connect to MySQL server"
- MySQL is not running
- Check System Preferences → MySQL
- Or run: `brew services start mysql`

### Error: "Access denied for user 'root'"
- Wrong password in .env
- Check your MySQL Workbench connection settings

### Error: "Unknown database"
- Database doesn't exist
- Create it: `mysql -u root -p -e "CREATE DATABASE csp;"`

---

## 💡 Quick Troubleshooting

### Find MySQL installation
```bash
which mysql
# or
find /usr/local -name mysql -type f
```

### Check if MySQL is running
```bash
lsof -i :3306
```

### Connect to MySQL directly
```bash
mysql -u root -p
# Enter your password when prompted
```

### List databases
```bash
mysql -u root -p -e "SHOW DATABASES;"
```

---

## 📞 Next Steps

1. **Find your MySQL connection details** (from Workbench or System Preferences)
2. **Create .env file** with your credentials
3. **Test connection**: `python3 run_mysql_cohort_query.py --test`
4. **Let me know** if you get any errors!

---

**What are your MySQL connection details?** (Host, User, Password, Database)
