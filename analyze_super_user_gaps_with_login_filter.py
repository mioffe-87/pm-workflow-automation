#!/usr/bin/env python3
"""
Super User Gap Analysis Script - WITH LOGIN ACTIVITY FILTER
Analyzes CSP database to identify accounts with login activity since Jan 1, 2026
that lack Super User roles.

This script:
1. Connects to the CSP database using credentials from .env
2. Executes the super_user_gap_analysis_with_login_filter.sql query
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
SQL_QUERY_FILE = SCRIPT_DIR / 'super_user_gap_analysis_with_login_filter.sql'
CSV_OUTPUT_FILE = SCRIPT_DIR / 'super_user_gap_report_with_login_filter.csv'
REPORT_OUTPUT_FILE = SCRIPT_DIR / 'super_user_gap_report_with_login_filter.txt'


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
    """Execute the super user gap analysis query with login filter."""
    try:
        cursor = connection.cursor(dictionary=True)
        sql_query = load_sql_query()
        
        print("\nExecuting analysis query (filtering for accounts with login activity since 2026-01-01)...")
        cursor.execute(sql_query)
        results = cursor.fetchall()
        cursor.close()
        
        print(f"✅ Query executed successfully. Found {len(results)} accounts without Super Users (with recent login activity).")
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


def get_accounts_with_login_activity(connection):
    """Get count of active accounts with login activity since 2026-01-01."""
    try:
        cursor = connection.cursor()
        cursor.execute("""
            SELECT COUNT(DISTINCT sa.SupportAccountId)
            FROM supportaccount sa
            INNER JOIN loginhistory lh ON sa.SupportAccountId = lh.SupportAccountId
            WHERE sa.IsActive = 1
            AND lh.LoginTime >= '2026-01-01'
            AND lh.LoginStatus = 'Success'
        """)
        result = cursor.fetchone()
        cursor.close()
        return result[0] if result else 0
    except mysql.connector.Error as err:
        print(f"❌ Error fetching accounts with login activity: {err}")
        return 0


def generate_report(results, total_accounts, accounts_with_login):
    """Generate a formatted text report."""
    report_lines = []
    
    # Header
    report_lines.append("=" * 80)
    report_lines.append("CSP SUPER USER GAP ANALYSIS REPORT")
    report_lines.append("(Filtered for accounts with login activity since January 1, 2026)")
    report_lines.append("=" * 80)
    report_lines.append(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    report_lines.append("")
    
    # Summary Statistics
    accounts_missing_su = len(results)
    accounts_with_su = accounts_with_login - accounts_missing_su
    percentage_at_risk = (accounts_missing_su / accounts_with_login * 100) if accounts_with_login > 0 else 0
    
    report_lines.append("SUMMARY STATISTICS")
    report_lines.append("-" * 80)
    report_lines.append(f"Total Active Support Accounts:        {total_accounts}")
    report_lines.append(f"Accounts with Login Activity (2026+): {accounts_with_login}")
    report_lines.append(f"  - WITH Super User:                 {accounts_with_su}")
    report_lines.append(f"  - WITHOUT Super User:              {accounts_missing_su}")
    report_lines.append(f"Percentage at Risk (of active logins):{percentage_at_risk:.1f}%")
    report_lines.append("")
    
    # Risk Assessment
    if accounts_missing_su == 0:
        report_lines.append("✅ EXCELLENT: All accounts with recent login activity have Super Users!")
    elif percentage_at_risk < 5:
        report_lines.append("⚠️  LOW RISK: Less than 5% of active accounts are missing Super Users")
    elif percentage_at_risk < 20:
        report_lines.append("⚠️  MEDIUM RISK: 5-20% of active accounts are missing Super Users")
    else:
        report_lines.append("🔴 HIGH RISK: More than 20% of active accounts are missing Super Users")
    
    report_lines.append("")
    
    # Detailed Findings
    if results:
        report_lines.append("ACCOUNTS MISSING SUPER USER (with recent login activity)")
        report_lines.append("-" * 80)
        report_lines.append(f"{'Account ID':<12} {'Account Name':<30} {'Type':<15} {'Last Login':<12} {'Logins':<8}")
        report_lines.append("-" * 80)
        
        for row in results:
            account_id = str(row['SupportAccountId'])
            account_name = row['AccountName'][:28] if row['AccountName'] else 'N/A'
            account_type = row['AccountType'][:13] if row['AccountType'] else 'N/A'
            last_login = row['LastLoginDate']
            if isinstance(last_login, datetime):
                last_login = last_login.strftime('%Y-%m-%d')
            login_count = str(row['LoginsSince2026'])
            
            report_lines.append(f"{account_id:<12} {account_name:<30} {account_type:<15} {last_login:<12} {login_count:<8}")
        
        report_lines.append("")
    
    # Recommendations
    report_lines.append("RECOMMENDATIONS")
    report_lines.append("-" * 80)
    report_lines.append("1. IMMEDIATE ACTION REQUIRED:")
    report_lines.append("   - These accounts are ACTIVELY USING CSP but lack Super User oversight")
    report_lines.append("   - Prioritize these accounts for Super User assignment")
    report_lines.append("   - Contact account owners immediately")
    report_lines.append("")
    report_lines.append("2. REMEDIATION STEPS:")
    report_lines.append("   - Identify active users in each account")
    report_lines.append("   - Assign Super User role to at least one active user")
    report_lines.append("   - Verify Super User can perform administrative functions")
    report_lines.append("   - Document the assignment for audit trail")
    report_lines.append("")
    report_lines.append("3. PREVENTIVE MEASURES:")
    report_lines.append("   - Implement automated alerts when last Super User is removed")
    report_lines.append("   - Require Super User approval for critical operations")
    report_lines.append("   - Conduct monthly audits of active accounts")
    report_lines.append("")
    report_lines.append("4. GOVERNANCE:")
    report_lines.append("   - Establish minimum Super User count per account (recommend: 2-3)")
    report_lines.append("   - Document Super User responsibilities and rotation policies")
    report_lines.append("   - Create escalation procedures for accounts without Super Users")
    report_lines.append("")
    
    # Footer
    report_lines.append("=" * 80)
    report_lines.append("For detailed data, see: super_user_gap_report_with_login_filter.csv")
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
                'LastLoginDate',
                'LoginsSince2026',
                'Status'
            ]
            writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
            
            writer.writeheader()
            for row in results:
                # Convert datetime to string for CSV
                row_copy = row.copy()
                if row_copy['CreatedOn']:
                    row_copy['CreatedOn'] = row_copy['CreatedOn'].strftime('%Y-%m-%d %H:%M:%S')
                if isinstance(row_copy['LastLoginDate'], datetime):
                    row_copy['LastLoginDate'] = row_copy['LastLoginDate'].strftime('%Y-%m-%d %H:%M:%S')
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
    print("CSP SUPER USER GAP ANALYSIS (WITH LOGIN ACTIVITY FILTER)")
    print("=" * 80 + "\n")
    
    # Connect to database
    connection = connect_to_database()
    
    try:
        # Get total active accounts
        total_accounts = get_total_active_accounts(connection)
        print(f"Total active support accounts: {total_accounts}")
        
        # Get accounts with login activity
        accounts_with_login = get_accounts_with_login_activity(connection)
        print(f"Accounts with login activity (2026+): {accounts_with_login}")
        
        # Execute analysis
        results = execute_analysis(connection)
        
        # Generate report
        report_text = generate_report(results, total_accounts, accounts_with_login)
        
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
