using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.UpdatesDownload
{
    public class ContentDownloadRequestCellularData : ContentDownloadRequestBase
    {
        [JsonProperty("product")]
        public string ProductFilterId { get; set; }

        [JsonProperty("Carrier")]
        public string CarrierId { get; set; }

        [JsonProperty("search")]
        public string Search { get; set; }

        [JsonProperty("pagination")]
        public ContentDownloadPagination Pagination { get; set; }

        [JsonProperty("sorting")]
        public ContentDownloadSorting Sorting { get; set; }
    }
}