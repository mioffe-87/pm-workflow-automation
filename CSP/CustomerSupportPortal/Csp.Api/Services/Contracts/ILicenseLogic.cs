using System.Collections.Generic;
using Csp.Api.LicensingService;
using Csp.Api.Model;
using System;
using System.Web;
using Csp.Api.Model.Flex;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface ILicenseLogic
    {
        bool GrantSiteLicenses(long supportAccountId);
        IEnumerable<ReleasedLicenseItem> GetReleasedVMLicenses(DataParameters parameters, long supportAccountId);
        LicensingService.licenseRet[] LicenseIt(string serialNumber, string cpuid = "", string uuid = "",
            string authorizationCode = "");
        LicensingService.licenseRet[] LicenseIt(string serialNumber, string cpuid, string uuid,
            string authorizationCode, string hyperVisor, string panOs);
        string ActivateLicense(string serialNumber, string cpuid = "", string uuid = "", string authorizationCode = "");
        bool TransferIt(string srcSerialNumber, string destSerialNumber, string curUserEmail);
        LicenseAuthorizationBundle GetLicenseAuthorizationBundle(string serialNumber, string authorizationCode);
        IsSiteLicenseResult IsSiteLicense(string authorizationCode);
        bool Is4HourRma(string authorizationCode);
        License GetLicenseByAuthcode(string authorizationCode);
        licenseRet[] ActivateSiteLicense(long supportAccountId, string authorizationCode);

        bool GrantTrial(string serialNumber, string featureName, int evalutionDays = 0, string cpuid = "",
            string uuid = "", string userEmail = "");

        bool GrantNFREval(string serialNumber, string featureName, int evalutionDays, string limiter, bool IsRemove,
            string cpuid = "", string uuid = "", string userEmail = "");

        string GetNFRSVCExpirationFromPandb(string serialNumber);
        bool HasNfrBnd(string serialNumber);

        bool HasNfrSvcOrBndLic(string serialNumber, string partNumber);
        void GetPartNumberAndModel(string serialNumber, out string partNumber, out string model);
        bool IsElligibleForFeatureLicense(string featureName, string featureSku, string serialNumber, string model, string userEmail, long? supportAccountId, bool isNFR, bool hasNfrSvcOrBndLic);
        //bool IsEligibleForDpmLicense(string serialNumber, string userEmail, long? supportAccountId);
        List<object> GetTrialLicenseListBySerialNumber(string serialNumber, string mode);
        LicenseKey GetLicenseKeybyId(long licenseId, long restrictedByUserAccountId);
        List<LicenseListItem> GetDeviceLicenses(long deviceId);
        string IsAuthorizationCodeValid(string authorizationCode, string serialNumber);
        string IsSiteLicenseAuthorizationCodeValid(string authorizationCode);
        bool IsAuthCodeValid(string authCode);
        bool UpdateLicFeaturesForNsxProvisioning(string selectedAuthCode, string newSerialNumber);
        string GetEPWildFireApiKey(long deviceId, long supportAccountId, DateTime expirationDate);
        string GetPartIdByTrialFeature(string feature);
        List<long> GetActivatedTrialLicenseId(string serialNumber, List<string> partIds);

        bool CreateCloudInstanceWithAccountId(string serialNumber, string cpuId, string uuId, string user, bool isAwsDevice, out long deviceId,
            out string cloudAccountId, out string errorMessage);


        bool DeactivateLicense(string encryptedCode);
        string ParseDeactivateLicenseFile(HttpPostedFileBase file);
        bool UpdateSapIdSfdcIdForAwsSupportAccount(long supportAccountId, string authCode, long userAccountId);

        bool CreateAzureInstance(string azureSerialNumber, string cpuId, string uuId, string user,
            out long deviceId,
            out string errorMessage);

        bool ReportCsspEndUserInfo(long[] deviceId, long csspSupportAccountId, string customerAccountId,
            string companyName, string dnbNumber, string address, string country, string region, string city,
            string postalCode, string endUserContactEmail, string phoneNumber, string website, string industry,
            string createdBy);

        bool RollbackUpgrade(string transactionId);
        string IsAuthorizationCodeValidForUpgrade(string authorizationCode, string serialNumber);
        bool ProcessPreLicenseRequestSynch(int csspPrelicenseRequestid, int quantity, string requestedBy);
        bool ProcessPreLicenseRequestASynch(int csspPrelicenseRequestid, int quantity, string requestedBy);
        bool CreateStub(string assetType, string authorizationCode, long userAccountID, long supportAccountID);

        bool Relicenseit(string serialnumber);
        bool IsCloudServiceAuthCode(string authCode);

        bool GeneratePanoramaDeviceMgtLicense(string serialNumber, string cpuId, string uuId,
            string hyperVisorVersion, string panOsVersion);
        bool UpdateDeviceLocationFor4HrRma(string serialNum, string city, string postalCode, string countryCode);

        bool IsPanoramaUpgradeAuthCodeActivation(string serialNumber, string authcode);
        bool isEligibleForCloudLicenseTransfer(string serialNumber, long supportAccountId);
        bool TransferCloudLicenses(string fromSerialNumber, string toSerialNumber);

        ELACoreAPIResponse ActivateTrapsELA(string authCode, string supportAccountId, string userEmail);
        bool UpdateSupportForVMELABundles(string authcode, string serialNumber);
        string GetAuthCodeType(string authcode);
        string GetPanOsVersion(string authcode);
        bool IsIotAuthCodeActivation(string authcode);
        void AllocateTokensToCnPanorama(string serialNumber, int tokensQuantity, string authCode);
        bool IsPae(string authcode);

        // 2021-04-26: JK - CSP-6337
        bool IsSaasInline(string authcode);

        DeploymentProfileResponse GetDeploymentProfile(DeploymentProfileRequest deploymentProfileRequest);
        DeploymentProfileResponse EditDeploymentProfile(DeploymentProfileRequest deploymentProfileRequest);
        DeploymentProfileModel DeleteDeploymentProfile(string authCode);
        DeploymentProfileModel DeleteCnxDeploymentProfile(string authCode);
        bool PrelicenceAirgapDP(string authCode, string vcpuQuantity, int firewallQuantity);
        bool LicenseFetchAirgapDP(FlexDevicesList devices, int numberOfVcpus);
        FirewallRegisterResponse RegisterFirewall(FirewallRegisterRequest firewallRegisterRequest);
        DownloadLicenseResponse DownloadLicenseKey(long licenseId, long userAccountId);
        DeploymentProfileResponse GetCnxDeploymentProfile(CnxDeploymentProfileRequest deploymentRequest);
        DeploymentProfileResponse EditCnxDeploymentProfile(CnxDeploymentProfileRequest deploymentRequest);
        List<RepurposePanoramasResponse> RepurposePanoramas(RepurposePanoramasRequest request);
        TransferDeploymentProfileResponse TransferDeploymentProfiles(TransferDeploymentProfilesLicensingRequest transferDeploymentProfilesRequest);
        bool IsAiOpsAuthCodeActivation(string authcode);
        List<PanoramasDeleteResponse> DeletePanoramas(string serialNumber);
        DeploymentProfileResponse RenewDeploymentProfile(DeploymentProfileRequest deploymentProfileRequest);
    }

    public class IsSiteLicenseResult
    {
        public bool IsSiteLicense { get; set; }
        public bool IsActive { get; set; }
        public bool IsAlreadyApplied { get; set; }
    }
}
