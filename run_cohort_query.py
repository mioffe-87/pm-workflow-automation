#!/usr/bin/env python3
import os
import sys
from google.cloud import bigquery
import pandas as pd

# Set environment variable to use application default credentials
os.environ['GOOGLE_APPLICATION_CREDENTIALS'] = os.path.expanduser('~/.config/gcloud/application_default_credentials.json')

try:
    client = bigquery.Client(project="itp-aia-datalake")
    
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
        `itp-aia-datalake.csp.useraccount` u
      WHERE
        u.isdeleted = FALSE
        AND u.createdon IS NOT NULL
    ),

    first_week_logins AS (
      SELECT DISTINCT
        nu.useraccountid,
        nu.user_creation_date,
        MIN(el.event_time) AS first_login_time,
        TIMESTAMP_DIFF(el.event_time, nu.creation_timestamp, HOUR) AS hours_to_first_login
      FROM
        new_users nu
      LEFT JOIN
        `itp-aia-datalake.csp.event_log` el
        ON nu.useraccountid = CAST(el.identifier AS INT64)
        AND el.event_type IN ('login', 'authentication', 'session_start', 'user_login')
        AND el.event_time BETWEEN nu.creation_timestamp AND TIMESTAMP_ADD(nu.creation_timestamp, INTERVAL 7 DAY)
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
      APPROX_QUANTILES(fwl.hours_to_first_login, 100)[OFFSET(50)] AS median_hours_to_first_login
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
    
    print("🔍 Running New User Cohort Analysis Query...")
    print("=" * 100)
    
    query_job = client.query(query)
    results = query_job.result()
    
    df = results.to_dataframe()
    
    if len(df) == 0:
        print("⚠️  No results found. Checking event_log table...")
        
        event_query = """
        SELECT DISTINCT event_type, COUNT(*) as count
        FROM `itp-aia-datalake.csp.event_log`
        WHERE isdeleted = FALSE
        GROUP BY event_type
        ORDER BY count DESC
        LIMIT 20
        """
        
        event_results = client.query(event_query).result()
        event_df = event_results.to_dataframe()
        
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
        
        output_file = "new_user_cohort_analysis.csv"
        df.to_csv(output_file, index=False)
        print(f"\n💾 Results saved to: {output_file}")

except Exception as e:
    print(f"❌ Error: {str(e)}")
    print(f"\nError type: {type(e).__name__}")
    import traceback
    traceback.print_exc()
