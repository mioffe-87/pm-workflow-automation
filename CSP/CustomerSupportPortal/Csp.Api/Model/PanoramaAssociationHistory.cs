using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class PanoramaAssociationHistory
    {
        public string PrimarySerialNumber { get; set; }
        public string SecondarySerialNumber { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceName { get; set; }
        public string Status { get; set; }
        public string Requestor { get; set; }
        public string Requested { get; set; }
    }   
}
