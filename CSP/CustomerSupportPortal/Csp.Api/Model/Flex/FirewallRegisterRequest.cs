using Newtonsoft.Json;
using Csp.Api.LicensingService;
using Csp.Api.Model.Enums;

namespace Csp.Api.Model.Flex
{
    public class FirewallRegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        [JsonIgnore]
        public licenseRet[] ServiceResponse { get; set; }
        [JsonIgnore]
        public string Endpoint { get; set; }
    }

    public class DownloadLicenseResponse
    {
        public bool Success { get; set; }
        public string LicenseKey { get; set; }
        public string LicenseName { get; set; }
    }

    public class FirewallRegisterRequest
    {
        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("cpuid")]
        public string Cpuid { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("authCode")]
        public string AuthCode { get; set; }

        [JsonProperty("osVersion")]
        public string CurrentOSVersion { get; set; }

        [JsonProperty("firewallType")]
        public string firewallType { get; set; }

        [JsonProperty("vcpu")]
        public string Vcpu { get; set; }

        [JsonProperty("memory")]
        public string Memory { get; set; }

        [JsonProperty(PropertyName = "fed_ramp_type")]
        public EFedRampType FedRampType { get; set; }
    }
}
