using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface INetworkSecurityApiLogic
    {
        CommonApiResponse<dynamic> GetFilters(string filter, long userAccountId, long supportAccountId, int? categoryId);
        CommonApiResponse<dynamic> GetAssetData(AssetRequest asssetRequest);
        CommonApiResponse<dynamic> GetAssetAttributes(string serialNumber);
        CommonApiResponse<dynamic> GetAssetMetaData(AssetRequest asssetRequest);
        CommonApiResponse<dynamic> GetAssetDashboardData(AssetRequest asssetRequest);
        CommonApiResponse<dynamic> GetDeviceTag(long supportAccountId, long userAccountId, string serialNumber);
        CommonApiResponse<dynamic> GetDeviceTag(int? page, int? size, string sort, DeviceTagRequest deviceTagRequest);
        CommonApiResponse<dynamic> GetFlexCreditDetails(FlexDetailsRequest flexDetailsRequest);
        CommonApiResponse<dynamic> GetTrialLicenses(long supportAccountId, long userAccountId, string serialNumber);
        CommonApiResponse<TrialLicenseResponseObject> ActivateTrialLicenses(TrialLicenseRequest trialLicenseRequest);
        CommonApiResponse<dynamic> ActivateLicense(LicenseRequest licenseRequest);
        CommonApiResponse<dynamic> GetAssetDetails(AssetDetailsRequest assetDetailsRequest);

        CommonApiResponse<dynamic> GetAuthCodeDetails(long supportAccountId, long userAccountId, string authCode);
        CommonApiResponse<dynamic> GetPanOSVersions();
        CommonApiResponse<dynamic> UpdateDeviceInformation(DeviceInformation deviceInfo);
        CommonApiResponse<dynamic>  DownloadAssetData(long supportAccountId, long userAccountId);
        CommonApiResponse<dynamic> GetLicenseActivationTypes(long userAccountId, long supportAccountId, string serialNumber);
        CommonApiResponse<dynamic> GetVirtualPlatforms();
        CommonApiResponse<dynamic> ActivateOfflineLicense(OfflineLicenseRequest licenseRequest);
        CommonApiResponse<dynamic> GetFeatureLicenses(long userAccountId, long supportAccountId, string serialNumber);
        CommonApiResponse<dynamic> RollbackVmUpgrade(long userAccountId, long supportAccountId, string serialNumber, string transactionId);
        CommonApiResponse<dynamic> GetAscContactInformation(long userAccountId, long supportAccountId, long ascSupportAccountId);

        CommonApiResponse<dynamic> GetDevicesForLicenseTransfer(long userAccountId, long supportAccountId, string serialNumber, bool searchArchive);
        CommonApiResponse<dynamic> TransferAssetLicense(TransferAssetLicenseRequest transferAssetLicenseRequest);
        CommonApiResponse<dynamic> AssociateComponent(AssociateComponentRequest associateComponentRequest);
        CommonApiResponse<dynamic> GetUnRegisteredComponents(ComponentRequest componentRequest);
        CommonApiResponse<dynamic> ActivateSiteLicense(ActivateSiteLicenseRequest activateSiteLicenseRequest);
        CommonApiResponse<dynamic> GenerateMagicLink(MagicLinkRequest magicLinkRequest);
        CommonApiResponse<dynamic> GetAssetLicense(long userAccountId, long supportAccountId, long categoryId, string serialNumber);
        CommonApiResponse<dynamic> DeleteTrialLicenses(DeleteTrialLicenseRequest deleteTrialLicenseRequest);
        CommonApiResponse<dynamic> ChassisComponents(long userAccountId, long supportAccountId, string serialNumber);
    }
}
