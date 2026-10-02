using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Csp.Api.Services.Contracts
{
    public interface ISiteLicenseDetailsRepository
    {
        IEnumerable<SiteLicenseDetailModel> GetSiteLicenseDetails(long supportAccountId);
        void SetSiteLicenseDetail(long supportAccountId, long licenseId, AfApiKeyStatuses state);
        void ClearSiteLicenseDetail(long supportAccountId, long licenseId);
    }

    public class SiteLicenseDetailModel
    {
        public long LicenseId { get; set; }
        public AfApiKeyStatuses ApiKeyState { get; private set; }

        public string ApiKeyStateString
        {
            set
            {
                AfApiKeyStatuses state;
                if (!Enum.TryParse(value, out state))
                    throw new InvalidEnumArgumentException();
                
                ApiKeyState = state;
            }
        }
    }
}