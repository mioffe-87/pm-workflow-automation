using System;
using Csp.Api.Model;


namespace Csp.Api.Services.Contracts
{
    public interface ILicensingApiRepository
    {
        LicensingApi GetLicensingApiKey(long supportAccountId);
        bool ExtendLicensingApi(long supportAccountId, long userAccountId, long apiKeyId, DateTime newExpiryDate);
    }
}
