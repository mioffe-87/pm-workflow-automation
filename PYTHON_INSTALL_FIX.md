# Python Installation Permission Issue - Solutions

## Problem
You're getting permission errors when trying to install Python packages:
```
ERROR: Could not install packages due to an OSError: [Errno 1] Operation not permitted
```

This is a common issue on macOS with system Python installations.

---

## ✅ Solution 1: Use Homebrew Python (Recommended)

### Step 1: Install Homebrew (if not already installed)
```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

### Step 2: Install Python via Homebrew
```bash
brew install python@3.11
```

### Step 3: Verify installation
```bash
/usr/local/bin/python3.11 --version
```

### Step 4: Create alias (optional)
Add to `~/.zshrc`:
```bash
alias python3=/usr/local/bin/python3.11
```

Then reload:
```bash
source ~/.zshrc
```

### Step 5: Install dependencies
```bash
python3 -m pip install -r requirements_mysql.txt
```

---

## ✅ Solution 2: Use pyenv (Best for Multiple Python Versions)

### Step 1: Install pyenv
```bash
brew install pyenv
```

### Step 2: Install Python
```bash
pyenv install 3.11.0
pyenv global 3.11.0
```

### Step 3: Add to shell profile
Add to `~/.zshrc`:
```bash
eval "$(pyenv init --path)"
eval "$(pyenv init -)"
```

### Step 4: Reload and verify
```bash
source ~/.zshrc
python3 --version
```

### Step 5: Install dependencies
```bash
python3 -m pip install -r requirements_mysql.txt
```

---

## ✅ Solution 3: Fix System Python Permissions

If you want to use the system Python, you can fix permissions:

```bash
# Fix the Python directory permissions
sudo chown -R $(whoami) ~/Library/Python

# Then install
python3 -m pip install -r requirements_mysql.txt
```

---

## ✅ Solution 4: Use Virtual Environment (Works with Current Python)

Even with permission issues, you can use a virtual environment in a different location:

```bash
# Create venv in a temp location
python3 -m venv /tmp/mysql_env

# Activate it
source /tmp/mysql_env/bin/activate

# Install dependencies
pip install -r requirements_mysql.txt

# Now run your scripts
python3 run_mysql_cohort_query.py --test

# Deactivate when done
deactivate
```

---

## 🎯 Quick Fix (Try This First)

Run this command in your terminal:

```bash
python3 -m pip install --user --no-cache-dir pandas mysql-connector-python pymysql sqlalchemy python-dotenv
```

If that fails, try:

```bash
python3 -m pip install --user --no-cache-dir --break-system-packages pandas mysql-connector-python pymysql sqlalchemy python-dotenv
```

---

## 📞 If All Else Fails

You can still use the MySQL connection module without installing packages globally:

1. **Download packages manually** from PyPI
2. **Add them to your project** in a `lib/` folder
3. **Update sys.path** in your scripts

Or contact your system administrator to fix the Python installation.

---

## ✨ Recommended Next Steps

1. **Try Solution 1** (Homebrew Python) - Most reliable
2. **If that doesn't work**, try **Solution 2** (pyenv)
3. **If you're in a hurry**, try **Solution 4** (Virtual Environment)

Let me know which solution you'd like to try!
