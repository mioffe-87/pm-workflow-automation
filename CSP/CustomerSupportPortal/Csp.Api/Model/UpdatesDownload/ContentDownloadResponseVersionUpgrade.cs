using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.UpdatesDownload
{
    public class ContentDownloadResponseVersionUpgrade
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("currentVersion")]
        public string CurrentVersion { get; set; }

        [JsonProperty("newMinVersion")]
        public string NewMinVersion { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }

        [JsonProperty("whitelist")]
        public string Whitelist { get; set; }

        [JsonProperty("seriesId")]
        public int SeriesId { get; set; }
    }
}