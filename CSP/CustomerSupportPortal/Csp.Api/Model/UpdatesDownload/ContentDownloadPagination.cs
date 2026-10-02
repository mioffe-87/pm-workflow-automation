using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.UpdatesDownload
{
    public class ContentDownloadPagination
    {
        [JsonProperty("current")]
        public int Current { get; set; } = 1;

        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 10;

        [JsonProperty("total")]
        public long Total { get; set; }
    }
}
