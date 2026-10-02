using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IAssetHistoryRepository
    {
        List<AssetHistoryListItem> GetAssetHistory(string code, DateTime? startDate, DateTime? endDate, long defaultSupportAccountId, long restrictedByUserAccountId, DataParameters parameters, bool isExportReport);
        List<AssetHistoryDeviceRegDetails> GetDeviceRegDetailsForAssetHistory(long[] deviceIds, long supportAccountId, long restrictedByUserAccountId, string code, DateTime? startDate, DateTime? endDate);
        List<AssetHistorySpareLicenseTransferDetails> GetSpareLicenseTransferDetailsForAssetHistory(long[] deviceIds, long supportAccountId, long restrictedByUserAccountId, string code, DateTime? startDate, DateTime? endDate);
    }

    public static class AssetTypeEnum
    {
        public const string Device = "Device";
        public const string Spare = "Spare";
        public const string AdvancedEndpointProtection = "Advanced Endpoint Protection";
        public const string VmSeriesAuthCodes = "VM Series Auth Code";
        public const string License = "License";
        public const string SiteLicense = "Site License";
        public const string EnterpriseAgreement = "Enterprise Agreement";
    }
}
