using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts 
{
	public interface IVirtualMachineLogic 
    {
        IEnumerable<AvailableVm> GetAvailableVms(DataParameters parameters, long supportAccountId, long userAccountId, string authCode);
        IEnumerable<AvailableVirtualSeries<AvailableCn>> GetAvailableCns(DataParameters parameters, long supportAccountId, long userAccountId, string authCode, string serialNumber);
        Task<byte[]> GetAvailableVmsCsv(long supportAccountId, long userAccountId, string authCode);
        Task<byte[]> GetAvailableCnsCsv(long supportAccountId, long userAccountId, string authCode, string serialNumber);
        RegisterVmResult RegisterVm(EVmType vmType, string uuId, string cpuId, string authCode, long supportAccountId, long userAccountId,
            string hyperVisor = null, string panOs = null, bool isCssp = false);
        ParseVmFileResult ParseVmFile(EVmType vmType, HttpPostedFileBase file);
        AddVmAuthCodeResult AddVmAuthCode(EVmType vmType, string authCode, long supportAccountId, long userAccountId);
        bool CanProvisionPanorama(EVmType vmType, string authCode);
        bool ProvisionPanorama(EVmType vmType, string authCode, long supportAccountId, long userAccountId);
        IEnumerable<PraProvisioningRec> GetProvisionedPraDevices(EVmType vmType, string authCode);
        bool IsInTheMiddelOfUpgrade(EVmType vmType, string serialNumber);
        void GetVmUpgradeModels(EVmType vmType, string vmSerialNumber, string upgradeAuthCode, out string fromModel, out string toModel);
        bool RollbackUpgrade(EVmType vmType, string transactionId);
        bool StoreVmUpgrade(EVmType vmType, long deviceId, string transactionId, string fromModel, string toModel, string originalAuthCode, string upgradeAuthCode, string createdBy);
        string GetVmAuthCodeBeforeUpgrade(EVmType vmType, string serialNumber);
        void GetVmUpgradeModels(EVmType vmType, long deviceId, string transactionId, out string originalVmAuthCode, out string currentVmAuthCode, out string fromModel, out string toModel);
        bool IsUpgradeComplete(EVmType vmType, long deviceId);
        string GetOriginalModelAfterRollback(EVmType vmType, string transactionId, string upgradeAuthcode);
        bool CanUserUpgradeVm(EVmType vmType, string email);
        int GetVmUseCountByAuthCode(EVmType vmType, string authCode);
        IEnumerable<ReleasedLicenseItem> GetReleasedLicenses(EVmType vmType, DataParameters parameters, long supportAccountId);
        VmAuthCodeDetails GetAuthCodeDetails(EVmType vmType, string authCode);
        Task<IEnumerable<VmRenewalDevice>> GetVmRenewalDetails(EVmType vmType, long supportAcctId, string authcode, string latestQuoteNumber, bool selectAll, bool hasPendingRenewals);
        string SaveRenewalVMs(EVmType vmType, long supportAcctId, long userAcctId, VmRenewalModel model, bool allSelected, string quoteNumber);
        string GetLatestQuoteInfo(EVmType vmType, long supportAcctId, string authCode, out bool selectAll, out bool lockUi, out bool hasPendingRenewals);
        Task<IEnumerable<AvailableVmCsv>> GetChildAuthCodesInfo(EVmType vmType, long supportAcctId, AvailableVmCsv parentAuthCode);
        Task<VmRenewalModel> GetUnprovisionedRenewalDetails(EVmType vmType, string authcode, string latestQuoteNumber);
        Task<int> GetActualProvisionedVms(EVmType vmType, long supportAcctId, string authcode);

        //VM ELA License APIs
        Task<AddVmAuthCodeResult> ActivateVmElaAuthCode(EVmType vmType, string authCode, long supportAccountId, long userAccountId);

        RegisterVmResult GenerateVmElaModels(EVmType vmType, string authCode, long supportAccountId, long userAccountId);
        Task<VmElaTokenModel> GetVmElaTokens(EVmType vmType, string authCode);

        Task<IEnumerable<VmElaChildModelInfo>> GetCurrentAccountVmElaModels(EVmType vmType, string grandParentAuthCode, long supportAccountId);
        Task<string> SaveVmElaAllocatedQuantities(EVmType vmType, string grandParentAuthCode, long totalAllocatedTokens, Dictionary<string, IEnumerable<VmElaChildModelInfo>> models);
        Task<string> GrantVmElaTokens(EVmType vmType, string grandParentAuthCode, long curUserId, string destinationEmail, bool notifyUser, long supportAccountId);
        bool HasPendingTokensGrantRequest(EVmType vmType, long supportAccountId, long userId);
        Task<IEnumerable<ElaTokensGrantModel>> GetPendingElaTokensGrantItems(EVmType vmType, long supportAccountId, long userId);
        Task<string> AcceptElaTokenGrant(EVmType vmType, ElaTokensGrantModel elaTokensGrantModel, bool accepted, long userAccountId);
        Task<IEnumerable<VmItemToDeactivate>> GetProvisionedVmsToDeactivate(EVmType vmType, long supportAccountId, string parentAuthCode);
        Task<string> DeactivateProvisionedVms(EVmType vmType, IEnumerable<VmItemToDeactivate> items, string email);
        string RevokeVmElaLicense(EVmType vmType, string authCode, long supportAccountId);
        ParseVmFileResult ParseVmFileByContent(EVmType vmType, string fileContent, string fileType);
        IEnumerable<ElaTokensGrantModel> GetPendingElaTokensByUser(EVmType vmType, long supportAccountId, long userId, long entityId);
        Task<CnAllocateAuthCodeTokenDetails> GetCnAuthCodeTokenDetails(long supportAccountId);
        Task<List<CnAvailablePanorama>> GetCnAvailablePanorama(long supportAccountId);
    }
}
