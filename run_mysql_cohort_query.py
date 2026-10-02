#!/usr/bin/env python3
"""
MySQL Cohort Analysis Example
Migrated from BigQuery run_cohort_query.py
"""

import os
import sys
from mysql_connection import MySQLConnection
import pandas as pd
from dotenv import load_dotenv

# Load environment variables
load_dotenv()


def run_mysql_cohort_query():
    """
    Run new user cohort analysis on MySQL
    Equivalent to run_cohort_query.py but for MySQL
    """
    
    # MySQL Configuration
    config = {
        'host': os.getenv('MYSQL_HOST', 'localhost'),
        'user': os.getenv('MYSQL_USER', 'root'),
        'password': os.getenv('MYSQL_PASSWORD', ''),
        'database': os.getenv('MYSQL_DATABASE', 'csp'),
        'port': int(os.getenv('MYSQL_PORT', 3306)),
        'driver': 'mysql-connector'  # or 'pymysql'
    }
    
    # Connect to MySQL
    db = MySQLConnection(**config)
    if not db.connect():
        print("❌ Failed to connect to MySQL")
        return
    
    try:
        # MySQL version of the cohort query
        query = """
        WITH new_users AS (
          SELECT
            DATE(u.createdon) AS user_creation_date,
            u.useraccountid,
            u.email,
            u.displayname,
            u.createdon AS creation_timestamp,
            DATE_ADD(DATE(u.createdon), INTERVAL 7 DAY) AS first_week_end_date
          FROM
            useraccount u
          WHERE
            u.isdeleted = FALSE
            AND u.createdon IS NOT NULL
        ),
        
        first_week_logins AS (
          SELECT DISTINCT
            nu.useraccountid,
            nu.user_creation_date,
            MIN(el.event_time) AS first_login_time,
            TIMESTAMPDIFF(HOUR, nu.creation_timestamp, el.event_time) AS hours_to_first_login
          FROM
            new_users nu
          LEFT JOIN
            event_log el
            ON nu.useraccountid = CAST(el.identifier AS SIGNED)
            AND el.event_type IN ('login', 'authentication', 'session_start', 'user_login')
            AND el.event_time BETWEEN nu.creation_timestamp AND DATE_ADD(nu.creation_timestamp, INTERVAL 7 DAY)
            AND el.isdeleted = FALSE
          WHERE
            el.event_time IS NOT NULL
          GROUP BY
            nu.useraccountid,
            nu.user_creation_date,
            nu.creation_timestamp
        )
        
        SELECT
          nu.user_creation_date,
          COUNT(DISTINCT nu.useraccountid) AS total_users_created,
          COUNT(DISTINCT fwl.useraccountid) AS users_with_first_week_login,
          ROUND(100.0 * COUNT(DISTINCT fwl.useraccountid) / COUNT(DISTINCT nu.useraccountid), 2) AS first_week_activation_rate,
          ROUND(AVG(fwl.hours_to_first_login), 2) AS avg_hours_to_first_login,
          MIN(fwl.hours_to_first_login) AS min_hours_to_first_login,
          MAX(fwl.hours_to_first_login) AS max_hours_to_first_login,
          PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY fwl.hours_to_first_login) AS median_hours_to_first_login
        FROM
          new_users nu
        LEFT JOIN
          first_week_logins fwl
          ON nu.useraccountid = fwl.useraccountid
        GROUP BY
          nu.user_creation_date
        ORDER BY
          nu.user_creation_date DESC
        LIMIT 100
        """
        
        print("🔍 Running New User Cohort Analysis Query on MySQL...")
        print("=" * 100)
        
        # Execute query and get DataFrame
        df = db.query_to_dataframe(query)
        
        if df is None or len(df) == 0:
            print("⚠️  No results found. Checking event_log table...")
            
            event_query = """
            SELECT DISTINCT event_type, COUNT(*) as count
            FROM event_log
            WHERE isdeleted = FALSE
            GROUP BY event_type
            ORDER BY count DESC
            LIMIT 20
            """
            
            event_df = db.query_to_dataframe(event_query)
            
            if event_df is not None:
                print("\n📊 Available Event Types:")
                print(event_df.to_string(index=False))
        
        else:
            print(f"✅ Query completed successfully!")
            print(f"📈 Analysis Period: {df['user_creation_date'].min()} to {df['user_creation_date'].max()}")
            print(f"📊 Total cohorts analyzed: {len(df)}")
            print("\n" + "=" * 100)
            print("NEW USER COHORT ANALYSIS - FIRST WEEK ACTIVATION")
            print("=" * 100 + "\n")
            
            pd.set_option('display.max_columns', None)
            pd.set_option('display.width', None)
            pd.set_option('display.max_colwidth', None)
            print(df.to_string(index=False))
            
            print("\n" + "=" * 100)
            print("KEY INSIGHTS")
            print("=" * 100)
            
            total_users = df['total_users_created'].sum()
            total_activated = df['users_with_first_week_login'].sum()
            overall_activation_rate = (total_activated / total_users * 100) if total_users > 0 else 0
            
            print(f"\n📊 Overall Metrics:")
            print(f"   • Total Users Created: {total_users:,}")
            print(f"   • Users Activated (First Week): {total_activated:,}")
            print(f"   • Overall Activation Rate: {overall_activation_rate:.2f}%")
            
            print(f"\n📈 Cohort Performance:")
            best_idx = df['first_week_activation_rate'].idxmax()
            worst_idx = df['first_week_activation_rate'].idxmin()
            print(f"   • Best Activation Rate: {df.loc[best_idx, 'first_week_activation_rate']:.2f}% ({df.loc[best_idx, 'user_creation_date']})")
            print(f"   • Worst Activation Rate: {df.loc[worst_idx, 'first_week_activation_rate']:.2f}% ({df.loc[worst_idx, 'user_creation_date']})")
            print(f"   • Average Activation Rate: {df['first_week_activation_rate'].mean():.2f}%")
            
            print(f"\n⏱️  Time to First Login:")
            print(f"   • Average Hours: {df['avg_hours_to_first_login'].mean():.2f}")
            print(f"   • Median Hours: {df['median_hours_to_first_login'].median():.2f}")
            print(f"   • Min Hours: {df['min_hours_to_first_login'].min():.2f}")
            print(f"   • Max Hours: {df['max_hours_to_first_login'].max():.2f}")
            
            output_file = "mysql_cohort_analysis.csv"
            df.to_csv(output_file, index=False)
            print(f"\n💾 Results saved to: {output_file}")
    
    except Exception as e:
        print(f"❌ Error: {str(e)}")
        print(f"\nError type: {type(e).__name__}")
        import traceback
        traceback.print_exc()
    
    finally:
        db.disconnect()


def test_connection():
    """Test MySQL connection and list tables"""
    print("\n" + "=" * 80)
    print("TESTING MYSQL CONNECTION")
    print("=" * 80)
    
    config = {
        'host': os.getenv('MYSQL_HOST', 'localhost'),
        'user': os.getenv('MYSQL_USER', 'root'),
        'password': os.getenv('MYSQL_PASSWORD', ''),
        'database': os.getenv('MYSQL_DATABASE', 'csp'),
        'port': int(os.getenv('MYSQL_PORT', 3306))
    }
    
    print(f"\n📋 Connection Configuration:")
    print(f"   Host: {config['host']}")
    print(f"   User: {config['user']}")
    print(f"   Database: {config['database']}")
    print(f"   Port: {config['port']}")
    
    db = MySQLConnection(**config)
    
    if db.connect():
        tables = db.get_tables()
        if tables:
            print(f"\n✅ Available tables ({len(tables)}):")
            for table in tables:
                print(f"   • {table}")
        
        # Test a simple query
        result = db.execute_query("SELECT VERSION()")
        if result:
            print(f"\n✅ MySQL Version: {result[0]}")
        
        db.disconnect()
    else:
        print("\n❌ Connection failed. Check your credentials and MySQL server status.")


if __name__ == "__main__":
    import argparse
    
    parser = argparse.ArgumentParser(description='MySQL Cohort Analysis')
    parser.add_argument('--test', action='store_true', help='Test connection only')
    parser.add_argument('--query', action='store_true', help='Run cohort query')
    
    args = parser.parse_args()
    
    if args.test:
        test_connection()
    elif args.query:
        run_mysql_cohort_query()
    else:
        print("MySQL Cohort Analysis Tool")
        print("\nUsage:")
        print("  python run_mysql_cohort_query.py --test    # Test connection")
        print("  python run_mysql_cohort_query.py --query   # Run cohort analysis")
        print("\nOr run both:")
        test_connection()
        run_mysql_cohort_query()
