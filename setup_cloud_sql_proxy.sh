#!/bin/bash

# Cloud SQL Proxy Setup Script
# This script helps you set up Cloud SQL Proxy for MySQL connection

echo "🚀 Cloud SQL Proxy Setup"
echo "========================"
echo ""

# Check if cloud_sql_proxy exists
if [ ! -f "./cloud_sql_proxy" ]; then
    echo "❌ cloud_sql_proxy not found in current directory"
    echo "Download it first:"
    echo "  curl -o cloud_sql_proxy https://dl.google.com/cloudsql/cloud_sql_proxy.mac.amd64"
    echo "  chmod +x cloud_sql_proxy"
    exit 1
fi

echo "✅ cloud_sql_proxy found"
echo ""

# Get GCP Project ID
read -p "Enter your GCP Project ID: " PROJECT_ID

if [ -z "$PROJECT_ID" ]; then
    echo "❌ Project ID is required"
    exit 1
fi

echo ""
echo "📝 Configuration:"
echo "  Project ID: $PROJECT_ID"
echo "  Instance: csp"
echo "  Region: us-central1 (assumed)"
echo ""

# Create .env file for Cloud SQL Proxy
cat > .env << EOF
# Cloud SQL Proxy Configuration
MYSQL_HOST=127.0.0.1
MYSQL_USER=mioffe
MYSQL_PASSWORD=
MYSQL_DATABASE=csp
MYSQL_PORT=3306

# GCP Configuration (for reference)
GCP_PROJECT_ID=$PROJECT_ID
GCP_INSTANCE=csp
GCP_REGION=us-central1
EOF

echo "✅ .env file created"
echo ""
echo "📋 Next steps:"
echo ""
echo "1. Start Cloud SQL Proxy in a terminal:"
echo "   ./cloud_sql_proxy -instances=$PROJECT_ID:us-central1:csp=tcp:3306"
echo ""
echo "2. Keep that terminal open"
echo ""
echo "3. In another terminal, test the connection:"
echo "   python3 run_mysql_cohort_query.py --test"
echo ""
echo "4. Run your analysis:"
echo "   python3 run_mysql_cohort_query.py --query"
echo ""
echo "✨ Done! Cloud SQL Proxy is ready to use."
