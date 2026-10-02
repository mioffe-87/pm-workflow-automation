//using Csp.Api.LicensingService;
using Csp.Api.Model;
using Csp.Api.Model.Flex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IFirewallFlexLogic
    {
        IEnumerable<DeploymentProfileForHub> GetDeploymentProfiles(long supportAccountId);
        IEnumerable<SupportAccountLicense> GetSupportAccountsLicenses(long userAccountId);
        IEnumerable<CreditDataModel> GetCreditDetails(string tokenIds, long userAccountId);
        IList<CreditDataModel> GetCreditDetails(long supportAccount, long userAccountId);
        bool DepositCredits(List<DepositCreditRequest> request, long supportAccountId, long userAccountId);
        bool DepositCredits(DepositCreditModel model);

        TokenCalculatorResponse GetEstimatedCost(Csp.Api.Model.TokenCalculatorRequest request);
        bool CreditTransfer(CreditTransferRequest request);
        IEnumerable<DeploymentProfile> GetExpiredDeploymentProfiles(long supportAccountId, long userAccountId, long tokenId);
        DeploymentProfileStatus AddUpdateDeploymentProfile(DeploymentProfile deploymentProfile);

        IEnumerable<CreditDataModel> GetAvailableCreditTypes(long sourceSupportAccountId, long targetSupportAccountId, long tokenId, long userAccountId);
        DeploymentProfileModel DeleteDeploymentProfile(long userAccountId, string authCode, string productId, long tokenId, string type, string panOs = null, string subType = null);

        FlexDevices GetFlexDevices(FlexDeviceRequest flexDeviceRequest, long supportAccountId, long userAccountId, string type, string subType = null);

        DeploymentProfileList GetDeploymentProfiles(long userAccountId, long supportAccountId, long tokenId, string poolExpirationDate);
        bool IsProfileNameExists(string profileName, long? profileId);

        DeploymentProfile GetDeploymentProfile(long deploymentProfileId);

        TokenAllocationDetail GetAllocationDetail(long tokenId, string productId, long userAccountId, long? deploymentProfileId);

        FirewallRegisterResponse RegisterFireWall(FirewallRegisterRequest firewallRegisterRequest);
        PanoramaProvisionResponse PanoramaProvision(PanoramaProvisionModel request);
        IEnumerable<PanoramaProvisionedDevice> GetProvisionedPanoramas(string authCode);
        DownloadLicenseResponse DownloadLicenseKey(long licenseId, long userAccountId);

        List<string> GetSubscriptionsByUseCase(List<int> useCase);
        DownloadLicenseResponse DownloadPanoramaLicense(long licenseId, long userAccountId);
        List<FwSerialAndSubscription> GetFirewalls(long supportAccountId, long userAccountId, string authCode, string type, string subType = null);
        List<string> DeactivateFirewall(FirewallDeactivationRequest firewallDeactivationRequest, string userEmailId);
        CreditDetail GetTokensAllocatedAndConsumed(long supportAccountId, long userAccountId, long tokenId);

        IEnumerable<DeploymentProfileAudit> GetDeploymentProfileAudit(string tokenId);
        IEnumerable<DeploymentProfileAuditDetail> GetDeploymentProfileAuditDetail(long auditId);
        double GetTokensConsumed(long userAccountId, long supportAccountId, string authCode);
        List<PanoramaMigration> GetPanoramasForMigration(long supportAccountId, string authCode, string type, string subType = null);
        List<RepurposePanoramasResponse> MigratePanoramas(RepurposePanoramasRequest panoramas);
        PanoramaDevices GetPanoramas(FlexDeviceRequest flexDeviceRequest, long supportAccountId);
        List<PanoramaMigrationDetail> GetPanoramaMigrationDetail(long deviceId, string authCode);
        IList<CreditPool> CreditPoolsForTransfer(long creditPoolId, long supportAccountId, long userAccountId);
        TransferDeploymentProfileResponse TransferDeploymentProfiles(TransferDeploymentProfilesRequest transferDeploymentProfilesRequest);
        DeploymentProfileList GetRenewalDeploymentProfiles(long userAccountId, long supportAccountId, long tokenId);
        IEnumerable<PanOsVersion> GetPanOsVersions(string panOsVersion,PanOsVersions PanOsVersionsList);
        List<PanoramasDeleteResponse> DeletePanoramas(PanoramasDeleteRequest request);
        bool EditPoolName(PoolEditReq request, long userAccountId);
        UserAccount GetUserAccountFromEmail(string userEmail);
        DeploymentProfile GetDeploymentProfile(string authCode, long userAccountId, long supportAccountId);
       dynamic GetCloudCreditDetails(long supportAccountId, long? creditPoolId = null, int page = 0, int size = 10);
       dynamic GetPanOSVersions(long userAccountId, long supportAccountId, string panOSVersion = "");

        dynamic FirewallRegistration(FirewallRegistrationRequest firewallRegistrationRequest);
    }
}
