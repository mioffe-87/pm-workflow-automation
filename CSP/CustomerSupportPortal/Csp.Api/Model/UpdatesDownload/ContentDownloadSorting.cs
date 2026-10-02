using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.UpdatesDownload
{
    public class ContentDownloadSorting
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }
    }
}
