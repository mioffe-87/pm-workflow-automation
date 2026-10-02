using System;
using System.Collections.Generic;
using System.Linq;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;

namespace Csp.Infrastructure.Data.Persistence
{
    public class BootstrapContainerRepository : IBootstrapContainerRepository
    {
        private const string DevPartTypePanDbId = "DEV";

        public bool ShouldShowDownloadButton(long deviceId)
        {
            var db = SessionDbContext.CspObjectContext;

            var isDevQuery =
                from d in db.devices
                join pm in db.partmasters on d.PartMasterId equals pm.PartMasterId
                join pt in db.parttypes on pm.PartTypeId equals pt.PartTypeId
                where
                    d.DeviceId == deviceId
                    && pt.PanDbIdentification == DevPartTypePanDbId
                    && (d.IsVirtualMachine == null || d.IsVirtualMachine == false)
                select d;

            return isDevQuery.Any();
        }

        public IEnumerable<BootstrapContainerLics> GetLicenseFiles(long deviceId)
        {
            var db = SessionDbContext.CspObjectContext;

            var now = DateTime.Now;

            // Comments By LK On 11/17
            // Using Store Proc to optimize the query performance
            /*
            var lics =
                from d in db.devices
                join l in db.licenses on d.DeviceId equals l.DeviceId
                join lpm in db.partmasters on l.PartMasterId equals lpm.PartMasterId
                join lpst in db.partsubtypes on lpm.PartSubtypeId equals lpst.PartSubtypeId into tempLpst
                from leftLpst in tempLpst.DefaultIfEmpty()

                let isPerpetual = lpm.PartSubtypeId != null && (leftLpst.LicenseType ?? false)
                let hasLicenseKey = l.LicenseKey != null && l.LicenseKey != ""

                where
                    d.DeviceId == deviceId
                    && l.IsActive
                    && hasLicenseKey
                    && (
                        isPerpetual
                        || (l.ExpirationDate > now)
                    )
                select new BootstrapContainerLics
                {
                   LicenseKey = l.LicenseKey,
                   SerialNumber = d.SerialNumber,
                   LicenseFeaturesName = lpm.PartSubtypeId != null ? leftLpst.LicenseFeaturesName : "",
                };*/

            var lics =
                db.Database.SqlQuery<BootstrapContainerLics>(
                    "CALL GetLicenseFilesByDeviceId(@p_DeviceId);",
                    new MySqlParameter("p_DeviceId", deviceId));

            return lics.ToArray();
        }
    }
}

/*
SELECT x.SerialNumber, COUNT(*) AS num
FROM (
SELECT d.SerialNumber, l.AuthorizationCode, l.LicenseKey, lpst.LicenseType, l.ExpirationDate
FROM device d
JOIN license l ON d.DeviceId = l.DeviceId
JOIN partmaster lpm ON l.PartMasterId = lpm.PartMasterId
LEFT JOIN partsubtype lpst ON lpm.PartSubtypeId = lpst.PartSubtypeId
WHERE 
d.SupportAccountId = 245
AND l.IsActive
AND l.LicenseKey IS NOT NULL AND l.LicenseKey <> ''
AND (
lpst.LicenseType = 1
OR l.ExpirationDate > CURRENT_DATE()
)
) x
GROUP BY
x.SerialNumber
ORDER BY COUNT(*) desc

;
 */