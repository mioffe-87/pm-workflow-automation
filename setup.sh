#!/bin/bash

# MySQL Connection Setup Script
# Run this script to set up your MySQL connection environment

echo "🚀 MySQL Connection Setup"
echo "=========================="
echo ""

# Check if python3 is available
if ! command -v python3 &> /dev/null; then
    echo "❌ Python 3 is not installed"
    exit 1
fi

echo "✅ Python 3 found: $(python3 --version)"
echo ""

# Try to install with --user flag
echo "📦 Installing dependencies..."
python3 -m pip install --user --upgrade pip

# Install each package individually to handle failures gracefully
echo ""
echo "Installing packages..."

packages=(
    "mysql-connector-python==8.2.0"
    "pymysql==1.1.0"
    "pandas>=2.0.0"
    "sqlalchemy>=2.0.0"
    "python-dotenv>=1.0.0"
)

for package in "${packages[@]}"; do
    echo "  Installing: $package"
    python3 -m pip install --user "$package" 2>/dev/null || echo "    ⚠️  Could not install $package (may already be installed)"
done

echo ""
echo "✅ Setup complete!"
echo ""
echo "📝 Next steps:"
echo "1. Create .env file with your MySQL credentials"
echo "2. Run: python3 run_mysql_cohort_query.py --test"
echo ""
