using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IPartnerEvalRepository
    {
        DeviceDetails GetDeviceInfo(string serialNumber);
        //LicenseDetails[] GetLicenseInfo(string serialNumber);
        List<long> GetPartnerMemberships(long userAccountId);
        long? DoesUserExistAndGetUserAccountId(string partnerLoginId);
        string GetDeviceTag(long deviceCat);
        DateTime? GetSupportExpiration(string serialNumber);
    }
}