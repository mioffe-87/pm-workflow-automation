using Newtonsoft.Json;
using System;

namespace Csp.Api.Model.UpdatesDownload
{
    public class ContentDownloadResponseTooltipUpgrade
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("currentVersion")]
        public string CurrentVersion { get; set; }

        [JsonProperty("seriesName")]
        public string SeriesName { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }

        [JsonProperty("specifics")]
        public string Specifics { get; set; }
    }
}
