# MySQL Installation Guide for macOS

## Current Status
- ✅ Python dependencies installed
- ❌ MySQL not installed
- ❌ Docker not installed
- ❌ Homebrew not installed

---

## 🚀 Option 1: Install Homebrew + MySQL (Recommended)

### Step 1: Install Homebrew
```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

### Step 2: Install MySQL
```bash
brew install mysql
```

### Step 3: Start MySQL
```bash
brew services start mysql
```

### Step 4: Verify Installation
```bash
mysql --version
mysql -u root
```

### Step 5: Create Database (if needed)
```bash
mysql -u root -e "CREATE DATABASE IF NOT EXISTS csp;"
```

---

## 🐳 Option 2: Install Docker + MySQL (Alternative)

### Step 1: Install Docker Desktop
Download from: https://www.docker.com/products/docker-desktop

### Step 2: Start Docker Desktop
Open Applications → Docker.app

### Step 3: Run MySQL Container
```bash
docker run --name mysql-dev \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=csp \
  -p 3306:3306 \
  -d mysql:8.0
```

### Step 4: Verify
```bash
docker ps
```

---

## 📥 Option 3: Download MySQL Directly

1. Go to: https://dev.mysql.com/downloads/mysql/
2. Download MySQL Community Server (macOS)
3. Follow the installer
4. Start MySQL from System Preferences

---

## ✅ After Installation

### Create .env file
```bash
cat > /Users/mioffe/Documents/pm-workflow-automation/.env << 'EOF'
MYSQL_HOST=localhost
MYSQL_USER=root
MYSQL_PASSWORD=root
MYSQL_DATABASE=csp
MYSQL_PORT=3306
EOF
```

### Test Connection
```bash
cd /Users/mioffe/Documents/pm-workflow-automation
python3 run_mysql_cohort_query.py --test
```

---

## 🎯 Recommended Path

1. **Install Homebrew** (if you don't have it)
2. **Install MySQL via Homebrew** (`brew install mysql`)
3. **Start MySQL** (`brew services start mysql`)
4. **Create .env file** (see above)
5. **Test connection** (`python3 run_mysql_cohort_query.py --test`)

---

## 💡 Quick Commands

```bash
# Check if MySQL is running
brew services list

# Start MySQL
brew services start mysql

# Stop MySQL
brew services stop mysql

# Connect to MySQL
mysql -u root

# Create database
mysql -u root -e "CREATE DATABASE csp;"

# List databases
mysql -u root -e "SHOW DATABASES;"
```

---

## 🆘 Troubleshooting

### "command not found: brew"
→ Install Homebrew first (see Step 1 above)

### "Can't connect to MySQL server"
→ Make sure MySQL is running: `brew services start mysql`

### "Access denied for user 'root'"
→ Check your .env file has correct password

### "Port 3306 already in use"
→ Another MySQL instance is running
→ Stop it: `brew services stop mysql`

---

## 📞 Need Help?

Let me know:
1. Which option you'd like to try
2. Any error messages you get
3. Your macOS version (if relevant)

I can help you through each step!
