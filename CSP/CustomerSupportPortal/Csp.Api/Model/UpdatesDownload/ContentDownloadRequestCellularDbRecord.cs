using System;

namespace Csp.Api.Model.UpdatesDownload
{
    public class ContentDownloadRequestCellularDbRecord : ContentDownloadRequestBase
    {
        public string Carrier { get; set; }

        public string Product { get; set; }

        public string Platform { get; set; }

        public string Model { get; set; }
    }
}
