using System;
using System.Collections.Generic;
using Csp.Api.Model;
using Newtonsoft.Json;
using Csp.Api.Model.UpdatesDownload;

namespace Csp.Api.Services.Contracts {
	public interface IUpdatesRepository {
		bool ShowTrapsSoftwareUpdates(long supportAccountId);
       
	    List<SoftwareUpdateFilters> GetFiltersForSoftwareUpdates(long userAccountId, long supportAccountId, out string groupIds);
        List<SoftwareUpdateFilters> GetFiltersForDynamicUpdates(long userAccountId, long supportAccountId, string contentType ,out string groupIds);
        List<ProductModelListItem> GetAllProductModels();

        Dictionary<string, string> GetModelsSupportedItems();

        ContentDownloadResponseCellularFilters GetCellularFilters(ContentDownloadRequestBase model);

        ContentDownloadRequestCellularDbRecord GetCellularData(ContentDownloadRequestCellularData model);

        List<SoftwareUpdateFilters> GetSoftwareUpgradeModels();

        List<ContentDownloadResponseVersionUpgrade> GetSoftwareUpgradeVersions();

        List<string> GetSoftwareUpgradeExpired();

        ContentDownloadResponseVersionUpgrade GetSoftwareUpgradeVersion(SoftwareUpdateRequest request);

        List<ContentDownloadResponseTooltipUpgrade> GetSoftwareUpgradeTooltip();
    }

    public class SoftwareUpdateModel : DynamicUpdateModel
    {
        [JsonProperty("uniqueId")]
        public string SoftwareUpdateId { get; set; }
        public string ModelsSupported { get; set; }
        [JsonProperty("displayName")]
        public string FileDisplayName { get; set; }
        public string GroupDisplayName { get; set; }
        public int GroupDisplayOrder { get; set; }
        [JsonProperty("policyFile")]
        public string PolicyFile { get; set; }

        [JsonProperty("releaseType")]
        public string ReleaseType { get; set; }

        [JsonProperty("sbomFile")]
        public string SbomFile { get; set; }

        [JsonProperty("sbomDisplayName")]
        public string SbomDisplayName { get; set; }

        [JsonProperty("mD5ChecksumSbom")]
        public string Md5ChecksumSbom { get; set; }

        [JsonProperty("shA256ChecksumSbom")]
        public string Sha256ChecksumSbom { get; set; }

        [JsonProperty("fileSizeSbom")]
        public float FileSizeSbom { get; set; }

        [JsonProperty("carrier")]
        public string Carrier { get; set; }

        [JsonProperty("platformModel")]
        public string PlatformModel { get; set; }
    }

    public class DynamicUpdateModel : DownloadUrlModel {
        [JsonProperty("versionNumber")]
		public string VersionNumber { get; set; }
        [JsonProperty("releaseDate")]
		public DateTime ReleaseDate { get; set; }
        [JsonProperty("releaseNotes")]
		public string ReleaseNotes { get; set; }
        [JsonProperty("releaseNotesType")]
        public string ReleaseNotesType { get; set; }
        public string FolderName { get; set; }
        [JsonProperty("fileName")]
		public string FileName { get; set; }
		public long? FileSize { get; set; }
        [JsonProperty("mD5Checksum")]
		public string Md5Cks { get; set; }
        [JsonProperty("shA256Checksum")]
		public string Sha256Cks { get; set; }

		public long? GroupId { get; set; }
		public string MinVerRequired { get; set; }
		public string Product { get; set; }
		public string Platform { get; set; }

        [JsonProperty("incremental")]
        public int Incremental { get; set; }

        [JsonProperty("firmwareDetails")]
        public string FirmwareDetails { get; set; }
        [JsonProperty("deprecated")]
        public string Deprecated { get; set; }
    }

	public class DownloadUrlModel {
		public string DownloadUrl { get; set; }
		public string ReleaseNotesUrl { get; set; }
        public string PolicyFileUrl { get; set; }
	}

    public enum ReleaseNotesType
    {
        File,
        HyperLink
    }
}
