SELECT s.AccountTypeId FROM supportaccount s WHERE s.SupportAccountId = 245;

UPDATE supportaccount s
  set s.AccountTypeId = 4
  WHERE s.SupportAccountId = 245;


/*
  SELECT Sales order item devices/licenses
  that are unregistered/unapplied
  and are not evals (guaranteed)
  and for inactive accounts
  */
SELECT d.SerialNumber, l.AuthorizationCode, so.SAPCustomerId, sa.Name, sa.SupportAccountId
  FROM salesorderitem soi
  LEFT JOIN device d ON soi.DeviceId = d.DeviceId
  LEFT JOIN license l ON soi.LicenseId = l.LicenseId
  JOIN salesorder so ON soi.SalesOrderId = so.SalesOrderId
  JOIN sapsupportaccountmapping map ON so.SAPCustomerId = map.SAPId
  JOIN (
    SELECT s.*
      FROM supportaccount s
      LEFT JOIN membership m ON s.SupportAccountId = m.SupportAccountId
      WHERE m.MembershipId IS null
  ) sa ON map.SupportAccountId = sa.SupportAccountId
  WHERE 
    ((d.DeviceId IS NOT NULL AND d.RegistrationDate IS NULL)
    || (l.LicenseId IS NOT NULL AND l.DeviceId IS NULL))
  ORDER BY d.DeviceId DESC, l.LicenseId DESC, so.SAPCustomerId;

-- 210260
SELECT *
  FROM sapsupportaccountmapping s WHERE s.SupportAccountId = 26590;
SELECT *
  FROM sapsupportaccountmapping s WHERE s.SAPId = 210260;


