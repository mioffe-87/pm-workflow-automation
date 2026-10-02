-- Super User Gap Analysis Query
-- Identifies all active support accounts that lack any user with the "Super User" role
-- 
-- This query:
-- 1. Finds all active support accounts
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
WHERE 
    sa.IsActive = 1
    AND COALESCE(su_count.SuperUserCount, 0) = 0
ORDER BY 
    sa.CreatedOn DESC,
    sa.Name ASC;
