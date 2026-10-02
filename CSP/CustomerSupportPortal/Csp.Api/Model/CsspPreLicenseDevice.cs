using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class CsspPreLicenseDevice
    {
        public long DeviceId { get; set; }
        public string SerialNumber { get; set; }
        public string Model { get; set; }
        public string ProvisionedDate { get; set; }
        public string MasterAuthCode { get; set; }
    }
}
