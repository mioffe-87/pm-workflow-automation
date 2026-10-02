from google.cloud import bigquery
import pandas as pd
from datetime import datetime

client = bigquery.Client(project="itp-aia-datalake")

# Query to analyze new user cohorts and first-week login activity
query = """
WITH new_users AS (
  -- Get all new users by creation date
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
  -- Identify users who had login events in their first week
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

-- Final aggregation by cohort
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
print("=" * 80)

try:
    # Run the query
    query_job = client.query(query)
    results = query_job.result()
    
    # Convert to DataFrame for better analysis
    df = results.to_dataframe()
    
    if len(df) == 0:
        print("⚠️  No results found. Checking event_log table for available event types...")
        
        # Check what event types exist
        event_type_query = """
        SELECT DISTINCT event_type, COUNT(*) as count
        FROM `itp-aia-datalake.csp.event_log`
        WHERE isdeleted = FALSE
        GROUP BY event_type
        ORDER BY count DESC
        LIMIT 20
        """
        
        event_results = client.query(event_type_query).result()
        event_df = event_results.to_dataframe()
        
        print("\n📊 Available Event Types in event_log:")
        print(event_df.to_string(index=False))
        
    else:
        print(f"\n✅ Query completed successfully!")
        print(f"📈 Analysis Period: {df['user_creation_date'].min()} to {df['user_creation_date'].max()}")
        print(f"📊 Total cohorts analyzed: {len(df)}")
        print("\n" + "=" * 80)
        print("NEW USER COHORT ANALYSIS - FIRST WEEK ACTIVATION")
        print("=" * 80)
        
        # Display full results
        pd.set_option('display.max_columns', None)
        pd.set_option('display.width', None)
        pd.set_option('display.max_colwidth', None)
        print(df.to_string(index=False))
        
        print("\n" + "=" * 80)
        print("KEY INSIGHTS")
        print("=" * 80)
        
        # Calculate aggregate metrics
        total_users = df['total_users_created'].sum()
        total_activated = df['users_with_first_week_login'].sum()
        overall_activation_rate = (total_activated / total_users * 100) if total_users > 0 else 0
        
        print(f"\n📊 Overall Metrics:")
        print(f"   • Total Users Created: {total_users:,}")
        print(f"   • Users Activated (First Week): {total_activated:,}")
        print(f"   • Overall Activation Rate: {overall_activation_rate:.2f}%")
        
        print(f"\n📈 Cohort Performance:")
        print(f"   • Best Activation Rate: {df['first_week_activation_rate'].max():.2f}% ({df.loc[df['first_week_activation_rate'].idxmax(), 'user_creation_date']})")
        print(f"   • Worst Activation Rate: {df['first_week_activation_rate'].min():.2f}% ({df.loc[df['first_week_activation_rate'].idxmin(), 'user_creation_date']})")
        print(f"   • Average Activation Rate: {df['first_week_activation_rate'].mean():.2f}%")
        
        print(f"\n⏱️  Time to First Login:")
        print(f"   • Average Hours: {df['avg_hours_to_first_login'].mean():.2f}")
        print(f"   • Median Hours: {df['median_hours_to_first_login'].median():.2f}")
        print(f"   • Min Hours: {df['min_hours_to_first_login'].min():.2f}")
        print(f"   • Max Hours: {df['max_hours_to_first_login'].max():.2f}")
        
        # Save results to CSV
        output_file = "new_user_cohort_analysis.csv"
        df.to_csv(output_file, index=False)
        print(f"\n💾 Results saved to: {output_file}")

except Exception as e:
    print(f"❌ Error running query: {str(e)}")
    print("\nTroubleshooting steps:")
    print("1. Check if event_log table has login event data")
    print("2. Verify the identifier field contains user IDs")
    print("3. Check available event_type values")
