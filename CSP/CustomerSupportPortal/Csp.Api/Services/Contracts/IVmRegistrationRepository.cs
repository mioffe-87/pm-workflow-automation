using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts 
{	
    public interface IVmRegistrationRepository 
    {     
		long[] GetRegisteredDevicesWithoutSupportAccountIds(EVmType vmType);

		long? GetRegisteringUserSupportAccountIfOnlyOne(EVmType vmType, long deviceId);

		long? GetRelatedVmSupportAccountIfExists(EVmType vmType, long deviceId);

		void SetDeviceSupportAccountId(EVmType vmType, long deviceId, long supportAccountId);

        IEnumerable<AvailableVm> GetAvailableVms(DataParameters dataParameters, long supportAccountId, long userAccountId, string authCode, bool onlyGetCsspVms = false);
        IEnumerable<AvailableVirtualSeries<AvailableCn>> GetAvailableCns(DataParameters dataParameters, long supportAccountId, long userAccountId, string authCode, string serialNumber, bool onlyGetCsspVms = false);

        bool IsInTheMiddelOfUpgrade(EVmType vmType, string serialNumber);

        void GetVmUpgradeModels(EVmType vmType, string vmSerialNumber, string upgradeAuthCode, out string fromModel, out string toModel);

	    string GetVmAuthCodeBeforeUpgrade(EVmType vmType, string vmSerialNumber);

        bool StoreVmUpgrade(EVmType vmType, long deviceId, string transactionId, string fromModel, string toModel, string originalAuthCode, string upgradeAuthCode, string createdBy);

        void GetVmUpgradeModels(EVmType vmType, long deviceId, string transactionId, out string originalVmAuthCode, out string currentVmAuthCode, out string fromModel, out string toModel);

        bool IsUpgradeComplete(EVmType vmType, long deviceId);

        string GetOriginalModelAfterRollback(EVmType vmType, string transactionId, string upgradeAuthcode);

        bool CanUserUpgradeVm(EVmType vmType, string email);

        int GetVmUseCountByAuthCode(EVmType vmType, string authCode);

        string GetLatestQuoteInfo(EVmType vmType, long supportAcctId, string authCode, out bool selectAll, out bool lockUi, out bool hasPendingRenewals);
        Task<IEnumerable<VmRenewalDevice>> GetProvisionedVmsWithPendingRenewals(EVmType vmType, long supportAcctId, string authcode, string quoteNumber, bool selectAll);
        Task<IEnumerable<VmRenewalDevice>> GetProvisionedVmsWithFulfilledRenewals(EVmType vmType, long supportAcctId, string authcode, bool selectAll);
        void SaveRenewalVMs(EVmType vmType, long supportAcctId, long userAcctId, VmRenewalModel model, bool allSelected, string quoteNumber);
        Task<IEnumerable<AvailableVmCsv>> GetChildAuthCodesInfo(EVmType vmType, long supportAcctId, AvailableVmCsv parentAuthCode);
        Task<VmRenewalModel> GetUnprovisionedRenewalDetails(EVmType vmType, string authcode, string latestQuoteNumber);
        Task<int> GetActualProvisionedVms(EVmType vmType, long supportAcctId, string authcode);
        Task<IEnumerable<VmItemToDeactivate>> GetProvisionedVmsToDeactivate(EVmType vmType, long supportAccountId, string parentAuthCode);
        Task DeactivateProvisionedVms(EVmType vmType, IEnumerable<VmItemToDeactivate> items, string email);
        Task<CnAllocateAuthCodeTokenDetails> GetCnAuthCodeTokenDetails(long supportAccountId);
        Task<List<CnAvailablePanorama>> GetCnAvailablePanorama(long supportAccountId);
    }
}
