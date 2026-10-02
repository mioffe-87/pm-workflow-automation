using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Xsoar
{
    public class XSOARListItem
    {
        public string AssetEnvironment { get; set; }

        public string SerialNumber { get; set; }

        public string LicenseDescription { get; set; }

        public string TenantURL { get; set; }

        public string LicenseDownloadLink { get; set; }

        public string Status { get; set; }

        public string StartDate { get; set; }

        public string ExpirationDate { get; set; }

        public string DownloadDescription { get; set; }
    }

}