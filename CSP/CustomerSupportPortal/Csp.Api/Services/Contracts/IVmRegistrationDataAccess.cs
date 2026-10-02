using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IVmRegistrationDataAccess
    {
        long[] GetRegisteredDevicesWithoutSupportAccountIds(EVmType vmType);

        long? GetRegisteringUserSupportAccountIfOnlyOne(EVmType vmType, long deviceId);

        void SetDeviceSupportAccountId(EVmType vmType, long deviceId, long supportAccountId);

        long? GetRelatedVmSupportAccountIfExists(EVmType vmType, long deviceId);
    }
}
