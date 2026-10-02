-- Super User Gap Analysis Query - WITH LOGIN ACTIVITY FILTER
-- Identifies active support accounts that:
-- 1. Have had login activity since January 1st, 2026
-- 2. Lack any user with the "Super User" role
-- 
-- This query:
-- 1. Finds all active support accounts with recent login activity
-- 2. Counts active Super Users per account
-- 3. Filters for accounts with zero Super Users
-- 4. Shows account details and context metrics

SELECT 
    sa.SupportAccountId,
    sa.Name AS AccountName,
    sa.IsActive,
    sa.IsPartner,
    sa.CreatedOn,
    COALESCE(at.Name, 'Unknown') AS AccountType,
    COALESCE(su_count.SuperUserCount, 0) AS SuperUserCount,
    COALESCE(active_user_count.ActiveUserCount, 0) AS TotalActiveUsers,
    COALESCE(last_login.LastLoginDate, 'Never') AS LastLoginDate,
    COALESCE(login_count.LoginCount, 0) AS LoginsSince2026,
    CASE 
        WHEN COALESCE(su_count.SuperUserCount, 0) = 0 THEN 'MISSING_SUPER_USER'
        ELSE 'HAS_SUPER_USER'
    END AS Status
FROM 
    supportaccount sa
    LEFT JOIN accounttype at ON sa.AccountTypeId = at.AccountTypeId
    LEFT JOIN (
        -- Count active Super Users per account
        SELECT 
            m.SupportAccountId,
            COUNT(DISTINCT m.UserAccountId) AS SuperUserCount
        FROM 
            membership m
            INNER JOIN membershiproles mr ON m.MembershipId = mr.MembershipId
            INNER JOIN role r ON mr.RoleId = r.RoleId
        WHERE 
            r.Name = 'Super User'
            AND m.ActivationDate <= NOW()
            AND (m.ExpirationDate >= NOW() OR m.ExpirationDate IS NULL)
        GROUP BY 
            m.SupportAccountId
    ) su_count ON sa.SupportAccountId = su_count.SupportAccountId
    LEFT JOIN (
        -- Count total active users per account
        SELECT 
            SupportAccountId,
            COUNT(DISTINCT UserAccountId) AS ActiveUserCount
        FROM 
            membership
        WHERE 
            ActivationDate <= NOW()
            AND (ExpirationDate >= NOW() OR ExpirationDate IS NULL)
        GROUP BY 
            SupportAccountId
    ) active_user_count ON sa.SupportAccountId = active_user_count.SupportAccountId
    LEFT JOIN (
        -- Get last login date for account
        SELECT 
            SupportAccountId,
            MAX(LoginTime) AS LastLoginDate
        FROM 
            loginhistory
        WHERE 
            LoginTime >= '2026-01-01'
            AND LoginStatus = 'Success'
        GROUP BY 
            SupportAccountId
    ) last_login ON sa.SupportAccountId = last_login.SupportAccountId
    LEFT JOIN (
        -- Count logins since 2026-01-01
        SELECT 
            SupportAccountId,
            COUNT(*) AS LoginCount
        FROM 
            loginhistory
        WHERE 
            LoginTime >= '2026-01-01'
            AND LoginStatus = 'Success'
        GROUP BY 
            SupportAccountId
    ) login_count ON sa.SupportAccountId = login_count.SupportAccountId
WHERE 
    sa.IsActive = 1
    AND COALESCE(su_count.SuperUserCount, 0) = 0
    AND last_login.LastLoginDate IS NOT NULL
ORDER BY 
    last_login.LastLoginDate DESC,
    sa.Name ASC;
