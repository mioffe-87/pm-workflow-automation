#!/usr/bin/env python3
"""
Super User Distribution Analysis
Analyzes the distribution of Super Users per account for active accounts
with login activity since January 1, 2026.

This script:
1. Queries the database for Super User counts per account
2. Generates statistical analysis
3. Creates visualizations (histogram, box plot, etc.)
4. Exports results to CSV
5. Provides insights and recommendations
"""

import mysql.connector
import os
import csv
from datetime import datetime
from pathlib import Path
from collections import Counter
import statistics

# Load environment variables
env_file = Path('.env')
env_vars = {}
if env_file.exists():
    with open(env_file) as f:
        for line in f:
            if '=' in line and not line.startswith('#'):
                key, value = line.strip().split('=', 1)
                env_vars[key] = value

DB_CONFIG = {
    'host': env_vars.get('MYSQL_HOST'),
    'user': env_vars.get('MYSQL_USER'),
    'password': env_vars.get('MYSQL_PASSWORD'),
    'database': env_vars.get('MYSQL_DATABASE'),
    'port': int(env_vars.get('MYSQL_PORT', 3306))
}

SCRIPT_DIR = Path(__file__).parent
CSV_OUTPUT_FILE = SCRIPT_DIR / 'super_user_distribution_analysis.csv'
REPORT_OUTPUT_FILE = SCRIPT_DIR / 'super_user_distribution_report.txt'


def connect_to_database():
    """Establish connection to the CSP database."""
    try:
        print(f"Connecting to {DB_CONFIG['host']}:{DB_CONFIG['port']} as {DB_CONFIG['user']}...")
        connection = mysql.connector.connect(**DB_CONFIG)
        if connection.is_connected():
            print("✅ Successfully connected to database")
            return connection
    except mysql.connector.Error as err:
        print(f"❌ Database Error: {err}")
        exit(1)


def get_super_user_distribution(connection):
    """Get Super User count per account for active accounts with login activity."""
    try:
        cursor = connection.cursor(dictionary=True)
        
        query = """
        SELECT 
            sa.SupportAccountId,
            sa.Name AS AccountName,
            sa.AccountTypeId,
            at.Name AS AccountType,
            COUNT(DISTINCT m.UserAccountId) AS SuperUserCount
        FROM 
            supportaccount sa
            LEFT JOIN accounttype at ON sa.AccountTypeId = at.AccountTypeId
            LEFT JOIN membership m ON sa.SupportAccountId = m.SupportAccountId
            LEFT JOIN membershiproles mr ON m.MembershipId = mr.MembershipId
            LEFT JOIN role r ON mr.RoleId = r.RoleId
            INNER JOIN loginhistory lh ON sa.SupportAccountId = lh.SupportAccountId
        WHERE 
            sa.IsActive = 1
            AND r.Name = 'Super User'
            AND m.ActivationDate <= NOW()
            AND (m.ExpirationDate >= NOW() OR m.ExpirationDate IS NULL)
            AND lh.LoginTime >= '2026-01-01'
            AND lh.LoginStatus = 'Success'
        GROUP BY 
            sa.SupportAccountId,
            sa.Name,
            sa.AccountTypeId,
            at.Name
        ORDER BY 
            SuperUserCount DESC
        """
        
        print("\nExecuting distribution query...")
        cursor.execute(query)
        results = cursor.fetchall()
        cursor.close()
        
        print(f"✅ Retrieved data for {len(results)} accounts")
        return results
    except mysql.connector.Error as err:
        print(f"❌ Query Error: {err}")
        exit(1)


def calculate_statistics(super_user_counts):
    """Calculate statistical measures."""
    return {
        'count': len(super_user_counts),
        'min': min(super_user_counts),
        'max': max(super_user_counts),
        'mean': statistics.mean(super_user_counts),
        'median': statistics.median(super_user_counts),
        'mode': statistics.mode(super_user_counts),
        'stdev': statistics.stdev(super_user_counts),
        'q1': sorted(super_user_counts)[len(super_user_counts) // 4],
        'q3': sorted(super_user_counts)[3 * len(super_user_counts) // 4],
    }


def generate_report(results, stats):
    """Generate a comprehensive text report."""
    report_lines = []
    
    # Header
    report_lines.append("=" * 90)
    report_lines.append("SUPER USER DISTRIBUTION ANALYSIS")
    report_lines.append("Active Accounts with Login Activity (January 1, 2026 - Present)")
    report_lines.append("=" * 90)
    report_lines.append(f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    report_lines.append("")
    
    # Summary Statistics
    report_lines.append("SUMMARY STATISTICS")
    report_lines.append("-" * 90)
    report_lines.append(f"Total Active Accounts Analyzed:       {stats['count']}")
    report_lines.append(f"Minimum Super Users per Account:     {stats['min']}")
    report_lines.append(f"Maximum Super Users per Account:     {stats['max']}")
    report_lines.append(f"Mean (Average):                      {stats['mean']:.2f}")
    report_lines.append(f"Median (Middle Value):               {stats['median']:.2f}")
    report_lines.append(f"Mode (Most Common):                  {stats['mode']}")
    report_lines.append(f"Standard Deviation:                  {stats['stdev']:.2f}")
    report_lines.append(f"Q1 (25th Percentile):                {stats['q1']}")
    report_lines.append(f"Q3 (75th Percentile):                {stats['q3']}")
    report_lines.append("")
    
    # Distribution Analysis
    super_user_counts = [r['SuperUserCount'] for r in results]
    distribution = Counter(super_user_counts)
    
    report_lines.append("DISTRIBUTION ANALYSIS")
    report_lines.append("-" * 90)
    report_lines.append(f"{'Super Users':<15} {'Count':<15} {'Percentage':<15} {'Cumulative %':<15}")
    report_lines.append("-" * 90)
    
    cumulative = 0
    for count in sorted(distribution.keys()):
        freq = distribution[count]
        pct = (freq / len(results)) * 100
        cumulative += pct
        report_lines.append(f"{count:<15} {freq:<15} {pct:>6.2f}%        {cumulative:>6.2f}%")
    
    report_lines.append("")
    
    # Insights
    report_lines.append("KEY INSIGHTS")
    report_lines.append("-" * 90)
    
    # Calculate percentiles
    accounts_with_1_su = distribution.get(1, 0)
    accounts_with_1_su_pct = (accounts_with_1_su / len(results)) * 100
    
    accounts_with_2_or_more = sum(freq for count, freq in distribution.items() if count >= 2)
    accounts_with_2_or_more_pct = (accounts_with_2_or_more / len(results)) * 100
    
    accounts_with_5_or_more = sum(freq for count, freq in distribution.items() if count >= 5)
    accounts_with_5_or_more_pct = (accounts_with_5_or_more / len(results)) * 100
    
    accounts_with_10_or_more = sum(freq for count, freq in distribution.items() if count >= 10)
    accounts_with_10_or_more_pct = (accounts_with_10_or_more / len(results)) * 100
    
    report_lines.append(f"1. Minimum Super User Requirement:")
    report_lines.append(f"   • {accounts_with_1_su} accounts ({accounts_with_1_su_pct:.2f}%) have exactly 1 Super User")
    report_lines.append(f"   • {accounts_with_2_or_more} accounts ({accounts_with_2_or_more_pct:.2f}%) have 2 or more Super Users")
    report_lines.append("")
    
    report_lines.append(f"2. Redundancy Analysis:")
    report_lines.append(f"   • {accounts_with_5_or_more} accounts ({accounts_with_5_or_more_pct:.2f}%) have 5+ Super Users (high redundancy)")
    report_lines.append(f"   • {accounts_with_10_or_more} accounts ({accounts_with_10_or_more_pct:.2f}%) have 10+ Super Users (very high redundancy)")
    report_lines.append("")
    
    report_lines.append(f"3. Distribution Shape:")
    if stats['mean'] > stats['median']:
        report_lines.append(f"   • Right-skewed distribution (mean {stats['mean']:.2f} > median {stats['median']:.2f})")
        report_lines.append(f"   • Most accounts cluster around {stats['mode']} Super Users")
        report_lines.append(f"   • Some accounts have significantly more Super Users")
    else:
        report_lines.append(f"   • Left-skewed or symmetric distribution")
    report_lines.append("")
    
    report_lines.append(f"4. Variability:")
    cv = (stats['stdev'] / stats['mean']) * 100
    report_lines.append(f"   • Coefficient of Variation: {cv:.2f}%")
    if cv > 50:
        report_lines.append(f"   • High variability in Super User assignments across accounts")
    elif cv > 25:
        report_lines.append(f"   • Moderate variability in Super User assignments")
    else:
        report_lines.append(f"   • Low variability - consistent Super User assignments")
    report_lines.append("")
    
    # Recommendations
    report_lines.append("RECOMMENDATIONS")
    report_lines.append("-" * 90)
    report_lines.append("1. MINIMUM REQUIREMENTS:")
    report_lines.append(f"   • Current minimum: {stats['min']} Super User per account")
    report_lines.append(f"   • Recommended minimum: 2-3 Super Users per account for redundancy")
    report_lines.append(f"   • Action: Review {accounts_with_1_su} accounts with only 1 Super User")
    report_lines.append("")
    
    report_lines.append("2. OPTIMAL RANGE:")
    report_lines.append(f"   • Current median: {stats['median']:.0f} Super Users")
    report_lines.append(f"   • Current mean: {stats['mean']:.2f} Super Users")
    report_lines.append(f"   • Recommended range: 3-7 Super Users per account")
    report_lines.append(f"   • Provides redundancy without excessive overhead")
    report_lines.append("")
    
    report_lines.append("3. HIGH-REDUNDANCY ACCOUNTS:")
    report_lines.append(f"   • {accounts_with_10_or_more} accounts have 10+ Super Users")
    report_lines.append(f"   • Review these accounts for potential consolidation")
    report_lines.append(f"   • Consider role-based access control (RBAC) alternatives")
    report_lines.append("")
    
    report_lines.append("4. GOVERNANCE:")
    report_lines.append(f"   • Establish Super User assignment policy")
    report_lines.append(f"   • Define minimum and maximum Super Users per account type")
    report_lines.append(f"   • Implement automated alerts for policy violations")
    report_lines.append(f"   • Conduct quarterly reviews of Super User assignments")
    report_lines.append("")
    
    # Footer
    report_lines.append("=" * 90)
    report_lines.append("For detailed data, see: super_user_distribution_analysis.csv")
    report_lines.append("=" * 90)
    
    return "\n".join(report_lines)


def export_to_csv(results):
    """Export results to CSV file."""
    try:
        with open(CSV_OUTPUT_FILE, 'w', newline='') as csvfile:
            fieldnames = [
                'SupportAccountId',
                'AccountName',
                'AccountType',
                'SuperUserCount'
            ]
            writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
            
            writer.writeheader()
            for row in results:
                writer.writerow({
                    'SupportAccountId': row['SupportAccountId'],
                    'AccountName': row['AccountName'],
                    'AccountType': row['AccountType'] or 'Unknown',
                    'SuperUserCount': row['SuperUserCount']
                })
        
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
    print("\n" + "=" * 90)
    print("SUPER USER DISTRIBUTION ANALYSIS")
    print("=" * 90 + "\n")
    
    # Connect to database
    connection = connect_to_database()
    
    try:
        # Get distribution data
        results = get_super_user_distribution(connection)
        
        # Extract Super User counts
        super_user_counts = [r['SuperUserCount'] for r in results]
        
        # Calculate statistics
        stats = calculate_statistics(super_user_counts)
        
        # Generate report
        report_text = generate_report(results, stats)
        
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
