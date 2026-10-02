#!/usr/bin/env python3
"""
Super User Gap Analysis Script
Analyzes CSP database to identify active accounts without Super User roles.

This script:
1. Connects to the CSP database using credentials from .env
2. Executes the super_user_gap_analysis.sql query
3. Generates a formatted report with summary statistics
4. Exports results to CSV for stakeholder review
5. Provides recommendations for remediation
"""

import mysql.connector
import os
import csv
from datetime import datetime
from dotenv import load_dotenv
from pathlib import Path

# Load environment variables
load_dotenv()

# Database configuration
DB_CONFIG = {
    'host': os.getenv('MYSQL_HOST'),
    'user': os.getenv('MYSQL_USER'),
    'password': os.getenv('MYSQL_PASSWORD'),
    'database': os.getenv('MYSQL_DATABASE'),
    'port': int(os.getenv('MYSQL_PORT', 3306))
}

# File paths
SCRIPT_DIR = Path(__file__).parent
SQL_QUERY_FILE = SCRIPT_DIR / 'super_user_gap_analysis.sql'
CSV_OUTPUT_FILE = SCRIPT_DIR / 'super_user_gap_report.csv'
REPORT_OUTPUT_FILE = SCRIPT_DIR / 'super_user_gap_report.txt'


def load_sql_query():
    """Load the SQL query from file."""
    try:
        with open(SQL_QUERY_FILE, 'r') as f:
            return f.read()
    except FileNotFoundError:
        print(f"❌ Error: SQL query file not found at {SQL_QUERY_FILE}")
        exit(1)


def connect_to_database():
    """Establish connection to the CSP database."""
    try:
        print(f"Connecting to {DB_CONFIG['host']}:{DB_CONFIG['port']} as {DB_CONFIG['user']}...")
        connection = mysql.connector.connect(**DB_CONFIG)
        if connection.is_connected():
            print("✅ Successfully connected to database")
            return connection
    except mysql.connector.Error as err:
        if err.errno == 2003:
            print(f"❌ Error: Cannot connect to MySQL server at '{DB_CONFIG['host']}'")
        elif err.errno == 1045:
            print(f"❌ Error: Access denied for user '{DB_CONFIG['user']}'")
        else:
            print(f"❌ Database Error: {err}")
        exit(1)


def execute_analysis(connection):
    """Execute the super user gap analysis query."""
    try:
        cursor = connection.cursor(dictionary=True)
        sql_query = load_sql_query()
        
        print("\nExecuting analysis query...")
        cursor.execute(sql_query)
        results = cursor.fetchall()
        cursor.close()
        
        print(f"✅ Query executed successfully. Found {len(results)} accounts without Super Users.")
        return results
    except mysql.connector.Error as err:
        print(f"❌ Query Error: {err}")
        exit(1)


def get_total_active_accounts(connection):
    """Get total count of active support accounts."""
    try:
        cursor = connection.cursor()
        cursor.execute("SELECT COUNT(*) FROM supportaccount WHERE IsActive = 1")
        result = cursor.fetchone()
        cursor.close()
        return result[0] if result else 0
    except mysql.connector.Error as err:
        print(f"❌ Error fetching total accounts: {err}")
        return 0


def generate_report(results, total_accounts):
    """Generate a formatted text report."""
    report_lines = []
    
    # Header
    report_lines.append("=" * 80)
    report_lines.append("CSP SUPER USER GAP ANALYSIS REPORT")
    report_lines.append("=" * 80)
    report_lines.append(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    report_lines.append("")
    
    # Summary Statistics
    accounts_missing_su = len(results)
    accounts_with_su = total_accounts - accounts_missing_su
    percentage_at_risk = (accounts_missing_su / total_accounts * 100) if total_accounts > 0 else 0
    
    report_lines.append("SUMMARY STATISTICS")
    report_lines.append("-" * 80)
    report_lines.append(f"Total Active Support Accounts:        {total_accounts}")
    report_lines.append(f"Accounts WITH Super User:             {accounts_with_su}")
    report_lines.append(f"Accounts WITHOUT Super User:          {accounts_missing_su}")
    report_lines.append(f"Percentage at Risk:                   {percentage_at_risk:.1f}%")
    report_lines.append("")
    
    # Risk Assessment
    if accounts_missing_su == 0:
        report_lines.append("✅ EXCELLENT: All active accounts have at least one Super User!")
    elif percentage_at_risk < 5:
        report_lines.append("⚠️  LOW RISK: Less than 5% of accounts are missing Super Users")
    elif percentage_at_risk < 20:
        report_lines.append("⚠️  MEDIUM RISK: 5-20% of accounts are missing Super Users")
    else:
        report_lines.append("🔴 HIGH RISK: More than 20% of accounts are missing Super Users")
    
    report_lines.append("")
    
    # Detailed Findings
    if results:
        report_lines.append("ACCOUNTS MISSING SUPER USER")
        report_lines.append("-" * 80)
        report_lines.append(f"{'Account ID':<12} {'Account Name':<30} {'Type':<15} {'Active Users':<12} {'Created':<12}")
        report_lines.append("-" * 80)
        
        for row in results:
            account_id = str(row['SupportAccountId'])
            account_name = row['AccountName'][:28] if row['AccountName'] else 'N/A'
            account_type = row['AccountType'][:13] if row['AccountType'] else 'N/A'
            active_users = str(row['TotalActiveUsers'])
            created = row['CreatedOn'].strftime('%Y-%m-%d') if row['CreatedOn'] else 'N/A'
            
            report_lines.append(f"{account_id:<12} {account_name:<30} {account_type:<15} {active_users:<12} {created:<12}")
        
        report_lines.append("")
    
    # Recommendations
    report_lines.append("RECOMMENDATIONS")
    report_lines.append("-" * 80)
    report_lines.append("1. IMMEDIATE ACTION REQUIRED:")
    report_lines.append("   - Review each account listed above")
    report_lines.append("   - Identify the account owner/administrator")
    report_lines.append("   - Assign at least one Super User role to an active user")
    report_lines.append("")
    report_lines.append("2. PREVENTIVE MEASURES:")
    report_lines.append("   - Implement automated alerts when last Super User is removed")
    report_lines.append("   - Require Super User approval for account registration")
    report_lines.append("   - Conduct quarterly audits of Super User assignments")
    report_lines.append("")
    report_lines.append("3. GOVERNANCE:")
    report_lines.append("   - Establish minimum Super User count per account (recommend: 2-3)")
    report_lines.append("   - Document Super User responsibilities and rotation policies")
    report_lines.append("   - Create escalation procedures for accounts without Super Users")
    report_lines.append("")
    
    # Footer
    report_lines.append("=" * 80)
    report_lines.append("For detailed data, see: super_user_gap_report.csv")
    report_lines.append("=" * 80)
    
    return "\n".join(report_lines)


def export_to_csv(results):
    """Export results to CSV file."""
    try:
        if not results:
            print("No data to export to CSV")
            return
        
        with open(CSV_OUTPUT_FILE, 'w', newline='') as csvfile:
            fieldnames = [
                'SupportAccountId',
                'AccountName',
                'IsActive',
                'IsPartner',
                'CreatedOn',
                'AccountType',
                'SuperUserCount',
                'TotalActiveUsers',
                'Status'
            ]
            writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
            
            writer.writeheader()
            for row in results:
                # Convert datetime to string for CSV
                row_copy = row.copy()
                if row_copy['CreatedOn']:
                    row_copy['CreatedOn'] = row_copy['CreatedOn'].strftime('%Y-%m-%d %H:%M:%S')
                writer.writerow(row_copy)
        
        print(f"✅ CSV report exported to: {CSV_OUTPUT_FILE}")
    except Exception as err:
        print(f"❌ Error exporting to CSV: {err}")


def save_report(report_text):
    """Save the formatted report to a text file."""
    try:
        with open(REPORT_OUTPUT_FILE, 'w') as f:
            f.write(report_text)
        print(f"✅ Report saved to: {REPORT_OUTPUT_FILE}")
    except Exception as err:
        print(f"❌ Error saving report: {err}")


def main():
    """Main execution function."""
    print("\n" + "=" * 80)
    print("CSP SUPER USER GAP ANALYSIS")
    print("=" * 80 + "\n")
    
    # Connect to database
    connection = connect_to_database()
    
    try:
        # Get total active accounts
        total_accounts = get_total_active_accounts(connection)
        print(f"Total active support accounts: {total_accounts}")
        
        # Execute analysis
        results = execute_analysis(connection)
        
        # Generate report
        report_text = generate_report(results, total_accounts)
        
        # Display report
        print("\n" + report_text)
        
        # Save report
        save_report(report_text)
        
        # Export to CSV
        export_to_csv(results)
        
        print("\n✅ Analysis complete!")
        
    finally:
        connection.close()
        print("Database connection closed.")


if __name__ == "__main__":
    main()
