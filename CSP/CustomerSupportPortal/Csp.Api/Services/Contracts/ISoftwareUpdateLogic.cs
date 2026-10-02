using System.Collections.Generic;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Model.UpdatesDownload;

namespace Csp.Api.Services.Contracts {
	public interface ISoftwareUpdateLogic {
        string GetDownloadUrl(SoftwareUpdateFileInfo fileInfo);

	    List<SoftwareUpdateFilters> GetFiltersForSoftwareUpdates(long userAccountId, long supportAccountId, out string groupIds);
	    SoftwareUpdateRequest IsEligibleToViewUpdates(SoftwareUpdateRequest requestedData, out string groupIds);
	    Task<List<SoftwareUpdateListItem>> GetSoftwareUpdates(SoftwareUpdateRequest filters);

        List<SoftwareUpdateFilters> GetFiltersForDynamicUpdates(long userAccountId, long supportAccountId,string contentType ,out string groupIds);
        Task<List<SoftwareUpdateListItem>> GetDynamicUpdates(SoftwareUpdateRequest filters);

        List<ProductModelListItem> GetAllProductModels();
        bool ShowTrapsSoftwareUpdates(long supportAccountId);
        Task<List<PanOs>> GetPanOSVersions();

        Dictionary<string, string> GetModelsSupportedItems();

        Task<ContentDownloadResponseCellularFilters> GetCellularFilters(ContentDownloadRequestBase model);

        Task<ContentDownloadResponseCellularDownloads> GetCellularData(ContentDownloadRequestCellularData model);

        List<SoftwareUpdateFilters> GetSoftwareUpgradeModels();

        List<ContentDownloadResponseVersionUpgrade> GetSoftwareUpgradeVersions();

        List<string> GetSoftwareUpgradeExpired();

        List<ContentDownloadResponseTooltipUpgrade> GetSoftwareUpgradeTooltip();

        Task<bool> GetFileDataFromUpdatesReader(SoftwareUpdateFileInfo fileInfo);
    }
}