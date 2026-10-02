#!/usr/bin/env python3
"""
MySQL Connection Setup Script
Run this to install all dependencies
"""

import subprocess
import sys
import os

def run_command(cmd, description=""):
    """Run a shell command and return success status"""
    if description:
        print(f"  {description}...", end=" ", flush=True)
    try:
        result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
        if result.returncode == 0:
            if description:
                print("✅")
            return True
        else:
            if description:
                print("⚠️")
            return False
    except Exception as e:
        if description:
            print(f"❌ ({e})")
        return False

def main():
    print("\n" + "="*60)
    print("🚀 MySQL Connection Setup")
    print("="*60 + "\n")
    
    # Check Python version
    print(f"✅ Python {sys.version.split()[0]} found\n")
    
    # Upgrade pip
    print("📦 Upgrading pip...")
    run_command(f"{sys.executable} -m pip install --user --upgrade pip", "Upgrading pip")
    
    # Install packages
    print("\n📦 Installing dependencies...\n")
    
    packages = [
        ("mysql-connector-python==8.2.0", "MySQL Connector"),
        ("pymysql==1.1.0", "PyMySQL"),
        ("pandas>=2.0.0", "Pandas"),
        ("sqlalchemy>=2.0.0", "SQLAlchemy"),
        ("python-dotenv>=1.0.0", "python-dotenv"),
    ]
    
    failed = []
    for package, name in packages:
        success = run_command(
            f"{sys.executable} -m pip install --user {package}",
            f"Installing {name}"
        )
        if not success:
            failed.append(name)
    
    print("\n" + "="*60)
    if failed:
        print(f"⚠️  Some packages failed to install: {', '.join(failed)}")
        print("   You may need to:")
        print("   1. Check your Python installation")
        print("   2. Try: sudo chown -R $(whoami) ~/Library/Python")
        print("   3. Or use a virtual environment")
    else:
        print("✅ All dependencies installed successfully!")
    
    print("\n📝 Next steps:")
    print("1. Create .env file with your MySQL credentials:")
    print("   MYSQL_HOST=localhost")
    print("   MYSQL_USER=root")
    print("   MYSQL_PASSWORD=your_password")
    print("   MYSQL_DATABASE=your_database")
    print("   MYSQL_PORT=3306")
    print("\n2. Test connection:")
    print("   python3 run_mysql_cohort_query.py --test")
    print("\n" + "="*60 + "\n")

if __name__ == "__main__":
    main()
