using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class SupportAccountLicense
    {
        public long SupportAccountId { get; set; }
        public string SupportAccountName { get; set; }
        public int DLPLicenseCount { get; set; }
        public int PanoramaDeviceCount { get; set; }
        public int IoTLicenseCount { get; set; }
    }
}
