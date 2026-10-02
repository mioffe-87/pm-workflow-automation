using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CsspDownloadLicense
    {
        public int? PrelicenseRequestId { get; set; }
        public List<long> PrelicenseDeviceIds { get; set; }
        public List<CsspDeviceLicense> Licenses { get; set; }
    }

    public class CsspDeviceLicense
    {
        public long DeviceId { get; set; }
        public string SerialNumber { get; set; }
        public string LicenseKey { get; set; }
        public string LicenseName { get; set; }
    }

}
