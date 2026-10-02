using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Wildfire;
using Csp.Api.Model.ElaEsa;

namespace Csp.Api.Services.Contracts
{
    public interface IElaEsaDataAccess
    {
        bool ValidateElaDeviceForRegistration(string serialNumber);

        IEnumerable<string> GetElaDeviceForDecom(long supportAccountId);

        IEnumerable<ElaEsaDecomissionDevice> GetDecommissionPayloadBySessionId(string sessionId);

        IEnumerable<ElaEsaCapInfo> ValidateMasterAuthCodes(long supportAccountId, IEnumerable<ElaEsaCapInfo> contracts);

        ElaCapsPagedResponse<IEnumerable<ElaEsaDecomedDevice>> GetElaDecomHistory(ElaEsaDecomRequest request);

        void ElaDecomDevicesComplete(ElaEsaDecomissionDeviceCompleteRequest request, string log);

        void PrepDecommissionDevices(long supportAccountId, ref IEnumerable<ElaEsaDecomissionDevice> decomDevices, out long sessionId);

        void PostDecommissionDevices(long supportAccountId, long userAccountId, IEnumerable<ElaEsaDecomissionDevice> decomDevices, long sessionId, EElaEsaActionStatus statusId, string requestProps, string responseProps = null);

        Dictionary<long, string> GetElaAssociatedSupportAccounts(long supportAccountId);
    }
}