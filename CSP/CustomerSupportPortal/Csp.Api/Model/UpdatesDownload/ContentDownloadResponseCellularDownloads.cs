using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.UpdatesDownload
{
    public class ContentDownloadResponseCellularDownloads : ContentDownloadResponseBase
    {
        [JsonProperty("downloads")]
        public IEnumerable<ContentDownloadResponseCellularDownloadItem> Downloads { get; set; }

        [JsonProperty("pagination")]
        public ContentDownloadPagination Pagination { get; set; }

        public static ContentDownloadResponseCellularDownloads SetError(string message)
        {
            return new ContentDownloadResponseCellularDownloads
            {
                Success = false,
                Message = message
            };
        }
    }
}
