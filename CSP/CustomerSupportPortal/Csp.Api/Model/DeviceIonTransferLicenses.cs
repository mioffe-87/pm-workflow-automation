using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Api.Model
{
    public class DeviceIonTransferLicenses
    {
        [JsonProperty("SourceDeviceId", Required = Required.Always)]
        public long SourceDeviceId { get; set; }

        [JsonProperty("SourceSn", Required = Required.Always)]
        public string SourceSn { get; set; }

        [JsonProperty("DestinationDeviceId", Required = Required.Always)]
        public long DestinationDeviceId { get; set; }

        [JsonProperty("DestinationSn", Required = Required.Always)]
        public string DestinationSn { get; set; }
    }
}