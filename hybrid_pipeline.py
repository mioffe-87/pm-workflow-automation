#!/usr/bin/env python3
"""
Hybrid BigQuery + MySQL Example
Shows how to use both data sources in the same workflow
"""

import os
import sys
from google.cloud import bigquery
from mysql_connection import MySQLConnection
import pandas as pd
from dotenv import load_dotenv

load_dotenv()


class HybridDataPipeline:
    """
    Manage both BigQuery and MySQL connections
    """
    
    def __init__(self):
        self.bq_client = None
        self.mysql_db = None
    
    def connect_bigquery(self, project_id: str = "itp-aia-datalake"):
        """Connect to BigQuery"""
        try:
            os.environ['GOOGLE_APPLICATION_CREDENTIALS'] = os.path.expanduser(
                '~/.config/gcloud/application_default_credentials.json'
            )
            self.bq_client = bigquery.Client(project=project_id)
            print(f"✅ Connected to BigQuery: {project_id}")
            return True
        except Exception as e:
            print(f"❌ BigQuery connection failed: {e}")
            return False
    
    def connect_mysql(self):
        """Connect to MySQL"""
        config = {
            'host': os.getenv('MYSQL_HOST', 'localhost'),
            'user': os.getenv('MYSQL_USER', 'root'),
            'password': os.getenv('MYSQL_PASSWORD', ''),
            'database': os.getenv('MYSQL_DATABASE', 'csp'),
            'port': int(os.getenv('MYSQL_PORT', 3306))
        }
        
        self.mysql_db = MySQLConnection(**config)
        if self.mysql_db.connect():
            print(f"✅ Connected to MySQL: {config['database']}")
            return True
        else:
            print("❌ MySQL connection failed")
            return False
    
    def query_bigquery(self, query: str) -> pd.DataFrame:
        """Execute BigQuery query and return DataFrame"""
        if not self.bq_client:
            print("❌ Not connected to BigQuery")
            return None
        
        try:
            query_job = self.bq_client.query(query)
            results = query_job.result()
            df = results.to_dataframe()
            print(f"✅ BigQuery query returned {len(df)} rows")
            return df
        except Exception as e:
            print(f"❌ BigQuery query failed: {e}")
            return None
    
    def query_mysql(self, query: str) -> pd.DataFrame:
        """Execute MySQL query and return DataFrame"""
        if not self.mysql_db:
            print("❌ Not connected to MySQL")
            return None
        
        df = self.mysql_db.query_to_dataframe(query)
        if df is not None:
            print(f"✅ MySQL query returned {len(df)} rows")
        return df
    
    def insert_to_mysql(self, query: str) -> bool:
        """Execute INSERT/UPDATE/DELETE on MySQL"""
        if not self.mysql_db:
            print("❌ Not connected to MySQL")
            return False
        
        return self.mysql_db.execute_update(query)
    
    def disconnect(self):
        """Close all connections"""
        if self.mysql_db:
            self.mysql_db.disconnect()
        print("✅ All connections closed")


# ============================================================================
# EXAMPLE WORKFLOWS
# ============================================================================

def example_bigquery_to_mysql():
    """
    Example 1: Query BigQuery, save results to MySQL
    """
    print("\n" + "="*80)
    print("EXAMPLE 1: BigQuery → MySQL")
    print("="*80)
    
    pipeline = HybridDataPipeline()
    
    if not pipeline.connect_bigquery():
        return
    if not pipeline.connect_mysql():
        return
    
    # Query BigQuery
    bq_query = """
    SELECT
      DATE(u.createdon) AS user_creation_date,
      COUNT(DISTINCT u.useraccountid) AS total_users,
      COUNT(DISTINCT CASE WHEN el.event_type = 'login' THEN u.useraccountid END) AS users_with_login
    FROM
      `itp-aia-datalake.csp.useraccount` u
    LEFT JOIN
      `itp-aia-datalake.csp.event_log` el
      ON u.useraccountid = CAST(el.identifier AS INT64)
    WHERE
      u.isdeleted = FALSE
      AND DATE(u.createdon) >= DATE_SUB(CURRENT_DATE(), INTERVAL 30 DAY)
    GROUP BY
      user_creation_date
    ORDER BY
      user_creation_date DESC
    """
    
    print("\n📊 Querying BigQuery...")
    df = pipeline.query_bigquery(bq_query)
    
    if df is not None:
        print(f"\n📈 Results ({len(df)} rows):")
        print(df.head())
        
        # Save to MySQL
        print("\n💾 Saving to MySQL...")
        for idx, row in df.iterrows():
            insert_query = f"""
            INSERT INTO cohort_analysis (user_creation_date, total_users, users_with_login, created_at)
            VALUES ('{row['user_creation_date']}', {row['total_users']}, {row['users_with_login']}, NOW())
            """
            pipeline.insert_to_mysql(insert_query)
        
        print(f"✅ Saved {len(df)} rows to MySQL")
    
    pipeline.disconnect()


def example_mysql_to_bigquery():
    """
    Example 2: Query MySQL, export to BigQuery
    """
    print("\n" + "="*80)
    print("EXAMPLE 2: MySQL → BigQuery (Export)")
    print("="*80)
    
    pipeline = HybridDataPipeline()
    
    if not pipeline.connect_mysql():
        return
    if not pipeline.connect_bigquery():
        return
    
    # Query MySQL
    mysql_query = """
    SELECT
      user_id,
      email,
      created_at,
      last_login,
      status
    FROM users
    WHERE created_at > DATE_SUB(NOW(), INTERVAL 90 DAY)
    """
    
    print("\n📊 Querying MySQL...")
    df = pipeline.query_mysql(mysql_query)
    
    if df is not None:
        print(f"\n📈 Results ({len(df)} rows):")
        print(df.head())
        
        # Save to CSV for BigQuery import
        csv_file = "mysql_export_for_bq.csv"
        df.to_csv(csv_file, index=False)
        print(f"\n💾 Exported to {csv_file}")
        print("📤 Next: Upload to BigQuery using:")
        print(f"   bq load --source_format=CSV --skip_leading_rows=1 \\")
        print(f"   my_dataset.users_import {csv_file}")
    
    pipeline.disconnect()


def example_compare_data():
    """
    Example 3: Compare data between BigQuery and MySQL
    """
    print("\n" + "="*80)
    print("EXAMPLE 3: Compare BigQuery vs MySQL")
    print("="*80)
    
    pipeline = HybridDataPipeline()
    
    if not pipeline.connect_bigquery():
        return
    if not pipeline.connect_mysql():
        return
    
    # Query both sources
    bq_query = """
    SELECT
      DATE(createdon) AS date,
      COUNT(DISTINCT useraccountid) AS user_count
    FROM `itp-aia-datalake.csp.useraccount`
    WHERE isdeleted = FALSE
    GROUP BY date
    ORDER BY date DESC
    LIMIT 30
    """
    
    mysql_query = """
    SELECT
      DATE(created_at) AS date,
      COUNT(DISTINCT user_id) AS user_count
    FROM users
    WHERE deleted_at IS NULL
    GROUP BY date
    ORDER BY date DESC
    LIMIT 30
    """
    
    print("\n📊 Querying BigQuery...")
    bq_df = pipeline.query_bigquery(bq_query)
    
    print("📊 Querying MySQL...")
    mysql_df = pipeline.query_mysql(mysql_query)
    
    if bq_df is not None and mysql_df is not None:
        # Merge and compare
        comparison = pd.merge(
            bq_df.rename(columns={'user_count': 'bq_count'}),
            mysql_df.rename(columns={'user_count': 'mysql_count'}),
            on='date',
            how='outer'
        )
        
        comparison['difference'] = comparison['bq_count'] - comparison['mysql_count']
        comparison['pct_diff'] = (comparison['difference'] / comparison['bq_count'] * 100).round(2)
        
        print("\n📈 Comparison Results:")
        print(comparison.to_string(index=False))
        
        # Save comparison
        comparison.to_csv('data_comparison.csv', index=False)
        print("\n💾 Comparison saved to data_comparison.csv")
    
    pipeline.disconnect()


def example_sync_data():
    """
    Example 4: Sync data between MySQL and BigQuery
    """
    print("\n" + "="*80)
    print("EXAMPLE 4: Sync Data (MySQL ↔ BigQuery)")
    print("="*80)
    
    pipeline = HybridDataPipeline()
    
    if not pipeline.connect_mysql():
        return
    if not pipeline.connect_bigquery():
        return
    
    print("\n🔄 Sync Workflow:")
    print("1. Query MySQL for recent changes")
    print("2. Export to CSV")
    print("3. Upload to BigQuery staging table")
    print("4. Merge into production table")
    
    # Step 1: Query MySQL
    mysql_query = """
    SELECT
      user_id,
      email,
      status,
      updated_at
    FROM users
    WHERE updated_at > DATE_SUB(NOW(), INTERVAL 1 DAY)
    """
    
    df = pipeline.query_mysql(mysql_query)
    
    if df is not None:
        # Step 2: Export to CSV
        csv_file = "sync_data.csv"
        df.to_csv(csv_file, index=False)
        print(f"\n✅ Exported {len(df)} rows to {csv_file}")
        
        print("\n📤 Next steps:")
        print(f"1. Upload to BigQuery:")
        print(f"   bq load --source_format=CSV --skip_leading_rows=1 \\")
        print(f"   my_dataset.users_staging {csv_file}")
        print(f"\n2. Merge into production:")
        print(f"   MERGE my_dataset.users T")
        print(f"   USING my_dataset.users_staging S")
        print(f"   ON T.user_id = S.user_id")
        print(f"   WHEN MATCHED THEN UPDATE SET email = S.email, status = S.status")
        print(f"   WHEN NOT MATCHED THEN INSERT (user_id, email, status) VALUES (user_id, email, status)")
    
    pipeline.disconnect()


if __name__ == "__main__":
    import argparse
    
    parser = argparse.ArgumentParser(description='Hybrid BigQuery + MySQL Pipeline')
    parser.add_argument('--example', type=int, choices=[1, 2, 3, 4],
                       help='Run specific example (1-4)')
    
    args = parser.parse_args()
    
    if args.example == 1:
        example_bigquery_to_mysql()
    elif args.example == 2:
        example_mysql_to_bigquery()
    elif args.example == 3:
        example_compare_data()
    elif args.example == 4:
        example_sync_data()
    else:
        print("Hybrid BigQuery + MySQL Pipeline Examples")
        print("\nUsage:")
        print("  python hybrid_pipeline.py --example 1  # BigQuery → MySQL")
        print("  python hybrid_pipeline.py --example 2  # MySQL → BigQuery")
        print("  python hybrid_pipeline.py --example 3  # Compare data")
        print("  python hybrid_pipeline.py --example 4  # Sync data")
