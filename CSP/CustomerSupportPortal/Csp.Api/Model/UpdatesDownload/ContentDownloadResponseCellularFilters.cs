using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.UpdatesDownload
{
    public class ContentDownloadResponseCellularFilters : ContentDownloadResponseBase
    {
        [JsonProperty("products")]
        public IEnumerable<ContentDownloadContentValue> Products { get; set; }

        [JsonProperty("carriers")]
        public IEnumerable<ContentDownloadContentValue> Carriers { get; set; }

        public static ContentDownloadResponseCellularFilters SetError(string message)
        {
            return new ContentDownloadResponseCellularFilters
            {
                Success = false,
                Message = message
            };
        }
    }
}
