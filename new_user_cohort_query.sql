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
