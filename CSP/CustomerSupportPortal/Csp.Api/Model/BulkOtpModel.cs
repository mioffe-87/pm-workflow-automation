using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class BulkOtpModel
    {
        public long BulkOtpId { get; set; }

        public long SupportAccountId { get; set; }

        public string RequestPayload { get; set; }

        public string PanoramaSerialNumber { get; set; }

        public string DeviceList { get; set; }

        public long RequestedByUserAccountId { get; set; }

        public string ResponsePayload { get; set; }

        public ProcessingStatus ProcessingStatus { get; set; }

        public string StatusMessage { get; set; }
    }

    public class BulkOtpRequestPayload
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("panorama")]
        public string PanoramaSerialNumber { get; set; }

        [JsonProperty("devices")]
        public List<string> Devices { get; set; }
    }

    public class BulkOtpResponsePayload
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("devices")]
        public List<DeviceOtp> DeviceOtp { get; set; }
    }

    public class DeviceOtp
    {
        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty("otp")]
        public string OneTimePassword { get; set; }

        [JsonProperty("error_msg")]
        public string ErrorMessage { get; set; }
    }
}
