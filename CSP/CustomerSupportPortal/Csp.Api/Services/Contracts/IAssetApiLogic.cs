using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IAssetApiLogic
    {
        CommonApiResponse<dynamic> GetAddOnsAndSubscriptions(long userAccountId, long supportAccountId, string tenantId);

        CommonApiResponse<dynamic> RegisterAsset(BaseRequest request);

        CommonApiResponse<dynamic> BeginTransfer(DeviceTransferPayload deviceTransfer);

        CommonApiResponse<dynamic> ReviewTransfer(DeviceTransferReviewRequest deviceTransferReview);

        CommonApiResponse<dynamic> InitiateBulkTransfer(DeviceTransferBulkRequest bulkDeviceTransferRequest);

        CommonApiResponse<dynamic> DownloadAssetData(AssetRequest assetRequest);

        CommonApiResponse<dynamic> DownloadAscAssetData(long supportAccountId, long userAccountId);

        CommonApiResponse<dynamic> UpdateDeviceName(AssetNameUpdateRequest request);
        CommonApiResponse<dynamic> UpdatePanOsVersion(PanOsVersionUpdateRequest request);
        CommonApiResponse<dynamic> UpdateRemarks(RemarksUpdateRequest request);
        CommonApiResponse<dynamic> UpdateLocation(LocationUpdateRequest request);
        CommonApiResponse<dynamic> RegisterTenant(BaseRequest request);
        RegistrationResult RegisterTenantWithValidation(BaseRequest request);
        CommonApiResponse<dynamic> UpdateAssetTags(AssetTagsUpdateRequest request);
        CommonApiResponse<dynamic> ChangetoSpare(SKUChangetoSpareRequest request);
        CommonApiResponse<dynamic> GetAssetMetaData(long userAccountId, long supportAccountId, string serialNumber);
        CommonApiResponse<dynamic> AssetDataInitialLoad(long supportAccountId);
    }
}
