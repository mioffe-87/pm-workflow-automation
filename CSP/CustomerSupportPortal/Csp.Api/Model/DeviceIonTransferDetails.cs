using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Api.Model
{
    public class DeviceIonTransferDetails
    {
        [JsonProperty("DeviceId", Required = Required.Always)]
        public long DeviceId { get; set; }

        [JsonProperty("SerialNumber", Required = Required.AllowNull)]
        public string SerialNumber { get; set; }

        [JsonProperty("Model", Required = Required.AllowNull)]
        public string Model { get; set; }

        [JsonProperty("TenantId", Required = Required.AllowNull)]
        public string TenantId { get; set; }

        [JsonProperty("TransferEligibleDevices", Required = Required.AllowNull)]
        public IEnumerable<dynamic> TransferEligibleDevices { get; set; }
    }
}