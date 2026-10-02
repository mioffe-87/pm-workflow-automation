using Csp.Api.HelperClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.UpdatesDownload
{
    public class ContentDownloadResponseCellularDownloadItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get { return ContentTypeEnum.CELLULAR.ToString(); } set { } }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("releaseDate")]
        public string ReleaseDate { get; set; }

        [JsonProperty("releaseNotes")]
        public string ReleaseNotes { get; set; }

        [JsonProperty("releaseNotesType")]
        public string ReleaseNotesType { get; set; }

        [JsonProperty("updateType")]
        public string UpdateType { get; set; }

        [JsonProperty("carrier")]
        public string Carrier { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("productFilterId")]
        public string ProductFilterId { get; set; }

        [JsonProperty("carrierId")]
        public string CarrierId { get; set; }


        [JsonProperty("platformModel")]
        public string PlatformModel { get; set; }

        [JsonProperty("displayFileName")]
        public string DisplayFileName { get; set; }

        [JsonProperty("FolderName")]
        public string FolderName { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        private string fileSize;
        [JsonProperty("fileSize")]
        public string FileSize
        {
            get { return ApiCommon.GetSizeSuffix(fileSize); }
            set { fileSize = value; }
        }

        [JsonProperty("mD5Checksum")]
        public string Md5Checksum { get; set; }

        [JsonProperty("shA256Checksum")]
        public string ShA256Checksum { get; set; }

        [JsonProperty("firmwareDetails")]
        public string FirmwareDetails { get; set; }
        [JsonProperty("deprecated")]
        public string Deprecated { get; set; }
    }
}
