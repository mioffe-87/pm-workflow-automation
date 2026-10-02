using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class PortalDeviceListRequest
    {
        [JsonProperty("serial_numbers")]
        public List<string> SerialNumbers { get; set; }
    }
}
