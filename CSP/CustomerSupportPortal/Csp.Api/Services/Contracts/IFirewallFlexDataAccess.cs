using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Flex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IFirewallFlexDataAccess
    {
        IEnumerable<DeploymentProfileForHub> GetDeploymentProfiles(long supportAccountId);
        void UpdateProfileStatusOnDeactivate(long userAccountId, string authCode);
        UseCaseSubscription GetUseCaseSubscriptions(long supportAccountId, DeploymentProfileType type, EDeploymentProfileSubType? subType, string panOs, string productId, string genType);
        IEnumerable<SupportAccountLicense> GetSupportAccountsLicenses(long userAccountId);
        void CheckandAssignCreditAdminRole(long supportAccountId, long userAccountId);
        void LogCreditTransfer(CreditTransferRequest request);
        IEnumerable<DeploymentProfile> GetExpiredDeploymentProfiles(long supportAccountId, long tokenId);
        DeploymentProfileStatus AddUpdateDeploymentProfile(DeploymentProfile deploymentProfile);
        bool IsProfileNameExists(string profileName, long? profileId);
        void UpdateDeleteDeploymentProfileStatus(long userAccountId, string authCode, string statusMessage);
        bool IsUserHasPermissionToDelete(string authCode, long userAccountId, long supportAccountId);

        FlexDevices GetFlexDevices(FlexDeviceRequest flexDeviceRequest, long supportAccountId, int type, long userAccountId);
        DeploymentProfileList GetDeploymentProfiles(long supportAccountId, long tokenId);
        DeploymentProfile GetDeploymentProfile(long deploymentProfileId, string authCode = "");
       
        TokenAllocationDetail GetAllocationDetail(long tokenId, long? deploymentProfileId);
        IEnumerable<TokenTransferHistory> GetTransactionHistory(string tokenId);

        IEnumerable<Hypervisor> GetHyperVisors(string firewallType = "");
        IEnumerable<PanOsVersion> GetPanOsVersions(string panOsVersion);

        void LogAuditHistory(long supportAccountId, FwFlexRequestType requestType, string api, string request, string response, long userAccountId);

        IEnumerable<PanoramaProvisionedDevice> GetProvisionedPanoramas(string authCode);
        IEnumerable<DeploymentProfileInfo> GetNumberofDeployments(long supportAccountId);
        IEnumerable<DeploymentProfileInfo> GetNumberofRenewalDeployments(long supportAccountId, long tokenId);
        IEnumerable<(long tokenId, bool hasSaasCloud, bool HasCloudNgfw)> GetCreditPoolTraits(long[] acpIds);
        List<string> GetSubscriptionsByUseCase(List<int> useCase);
        AscAccountInfo GetAscAccountInfo(string salesforceAccountId);

        int GetFirewallsCreated(string authCode);
        string GetDeploymentProfileStatus(string authCode);
        List<FwSerialAndSubscription> GetFirewalls(long supportAccountId, long userAccountId, string authCode, string type, string subType = null);
        List<string> DeactivateFirewall(List<string> serialNumbers, string userEmailId);
        List<string> GetSubscriptionsByDeploymentProfileId(long? deploymentProfileId);
        IEnumerable<DeploymentProfileAudit> GetDeploymentProfileAudit(string tokenId);
        IEnumerable<DeploymentProfileAuditDetail> GetDeploymentProfileAuditDetail(long auditId);
        List<PanoramaMigration> GetPanoramasForMigration(long supportAccountId, string authCode, string type, string subType = null);
        PanoramaDevices GetPanorams(FlexDeviceRequest flexDeviceRequest, long supportAccountId);
        List<PanoramaMigrationDetail> GetPanoramaMigrationDetail(long deviceId, string authCode);
        bool TransferDeploymentDeploymentProfile(long? deploymentProfileId, long tokenId, double TokensAllocated, long requestedBy);
        DeploymentProfileList GetRenewalDeploymentProfiles(long supportAccountId, long tokenId);
        DeploymentProfile GetDeploymentProfile(string authCode);
        List<string> GetFlexFeatures(int deploymentProfileId);
        bool IsConsumptionReported(string serailNumber, string authCode, string url);
        string GetAuthCodeFromDeploymentProfileId(long id, long supportAccountId);
        string FlattenSubType(string type, string subtype);
        List<WarmPoolDeviceState> GetWarmPoolDeviceState(List<string> serialNumbers);
        void UpdateWarmPoolDeviceState(List<WarmPoolDeviceState> serialNumbers, string userEmail);

        int GetPubSubMessageConfig();
        EFedRampType GetFedRampType(long supportAccountId);
    }
}