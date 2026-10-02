using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence
{
    public class SiteLicenseDetailsRepository : ISiteLicenseDetailsRepository
    {
        public IEnumerable<SiteLicenseDetailModel> GetSiteLicenseDetails(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var siteLicenses =
                from sld in db.sitelicensedetails
                where sld.SupportAccountId == supportAccountId
                select new SiteLicenseDetailModel
                {
                    LicenseId = sld.LicenseId,
                    ApiKeyStateString = sld.ApiKeyState,
                };

            return siteLicenses.ToArray();
        }

        public void SetSiteLicenseDetail(long supportAccountId, long licenseId, AfApiKeyStatuses state)
        {
            var allowedStates = new[] {AfApiKeyStatuses.Enabled, AfApiKeyStatuses.Disabled, AfApiKeyStatuses.Deleted};
            if (!allowedStates.Contains(state))
            {
                throw new InvalidEnumArgumentException();
            }

            var db = SessionDbContext.CspObjectContext;

            var existingRec = db.sitelicensedetails.SingleOrDefault(x => x.SupportAccountId == supportAccountId && x.LicenseId == licenseId);

            if (existingRec == null)
            {
                db.sitelicensedetails.Add(new sitelicensedetail
                {
                    SupportAccountId = supportAccountId,
                    LicenseId = licenseId,
                    ApiKeyState = state.ToString(),
                    LastUpdated = DateTime.Now,
                });
            }
            else
            {
                existingRec.ApiKeyState = state.ToString();
                existingRec.LastUpdated = DateTime.Now;
            }
            db.SaveChanges();
        }

        public void ClearSiteLicenseDetail(long supportAccountId, long licenseId)
        {
            var db = SessionDbContext.CspObjectContext;

            var existingRec = db.sitelicensedetails.SingleOrDefault(x => x.SupportAccountId == supportAccountId && x.LicenseId == licenseId);

            if (existingRec != null)
            {
                db.sitelicensedetails.Remove(existingRec);
                db.SaveChanges();
            }
        }
    }
}
