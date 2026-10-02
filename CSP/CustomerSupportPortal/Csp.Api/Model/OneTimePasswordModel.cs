using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class OneTimePasswordModel
    {
        public long Id { get; set; }

        public string SerialNumber { get; set; }

        public long SupportAccountId { get; set; }

        public string LookUp { get; set; }

        public string Otp { get; set; }

        public string ExpirationDate { get; set; }

        public OtpType OtpType { get; set; }

        public string UpdatedBy { get; set; }

        public string ProductCategory { get; set; }

        public bool IsBulkOtp { get; set; }

        public OtpDeviceType DeviceType { get; set; }
    }

    public class SingleOtpRequest
    {
        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty(PropertyName = "support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "user_account_id")]
        public string UpdatedBy { get; set; }

        [JsonProperty(PropertyName = "otp_type")]
        public string OtpType { get; set; }

        [JsonProperty(PropertyName = "device_type")]
        public string DeviceType { get; set; }        
       
    }
}
