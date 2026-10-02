using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Wildfire;
using Csp.Api.Model.ElaEsa;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IElaEsaLogic
    {
        Task<IEnumerable<ElaEsaCapInfo>> GetElaCapsAsync(long supportAccountId, string authcode = null, int? startMonth = null, int? endMonth = null);

        Task<IEnumerable<ElaEsaDevicePriceInfo>> GetElaDevicePriceAsync(long supportAccountId, string serialNumber);

        Task<IEnumerable<ElaEsaDevicePriceInfo>> GetElaDevicesPricesAsync(long supportAccountId, string authcode = null);

        IEnumerable<string> GetElaDevicesForDecom(long supportAccountId);

        void DecommissionDevices(long supportAccountId, long userAccountId, IEnumerable<ElaEsaDecomissionDevice> decomDevices);

        Task<ElaCapsPagedResponse<IEnumerable<ElaEsaDecomedDevice>>> GetElaDecomHistoryAsync(ElaEsaDecomRequest request);

        void ElaDecomDevicesComplete(ElaEsaDecomissionDeviceCompleteRequest request);

        Dictionary<long, string> GetElaAssociatedSupportAccounts(long supportAccountId);

        CommonApiResponse<dynamic> GetEaContracts(long supportAccountId, long userAccountId);

        CommonApiResponse<dynamic> GetEaContractSummary(string authCode, long supportAccountId, long userAccountId);

        CommonApiResponse<dynamic> GetEaAssociatedAccounts(string authCode, long supportAccountId, long userAccountId);

        dynamic GetEaTotalFirewalls(string authCode, long supportAccountId, long userAccountId, bool export = false);
        dynamic GetEAStatus(long supportAccountId, string serialNumber);

        CommonApiResponse<dynamic> GetEaContractsHistory(long supportAccountId, long userAccountId, string authCode);
    }
}