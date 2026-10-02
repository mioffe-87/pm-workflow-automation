using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class GenerateOtpModel
    {
        public long SupportAccountId { get; set; }

        public long UserAccountId { get; set; }

        public string UserEmail { get; set; }

        public string SerialNumber { get; set; }

        public string BulkOtpRequest { get; set; }

        public OtpDeviceType DeviceType { get; set; }

        public OtpType OtpType
        {
            get
            {
                return DeviceType.Equals(OtpDeviceType.PanoramaPrismaAccess) ? OtpType.LoggingService : OtpType.PanOS;
            }
        }
    }

    public class GenerateOtpResult
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public bool IsBulkOtp { get; set; }

        public string OneTimePassword { get; set; }

        public string ExpirationDate { get; set; }

        public string SerialNumber { get; set; }
    }

    public class BulkOtpRequest
    {
        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId { get; set; }
        [JsonProperty(PropertyName = "userAccountId")]
        public long UserAccountId { get; set; }
        [JsonProperty(PropertyName = "hashCode")]
        public string HashCode { get; set; }
    }

}
