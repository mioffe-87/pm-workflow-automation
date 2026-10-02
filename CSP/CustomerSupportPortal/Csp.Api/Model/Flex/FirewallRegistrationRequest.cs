using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class FirewallRegistrationRequest
    {
        [JsonProperty("user_account_id")]
        public long UserAccountId { get; set; }

        [JsonProperty("support_account_id")]
        public long SupportAccountId { get; set; }

        [JsonProperty("cpu_id")]
        public string Cpuid { get; set; }

        [JsonProperty("uu_id")]
        public string Uuid { get; set; }

        [JsonProperty("auth_code")]
        public string AuthCode { get; set; }

        [JsonProperty("os_version")]
        public string CurrentOSVersion { get; set; }

        [JsonProperty("firewall_type")]
        public string firewallType { get; set; }

        [JsonProperty("v_cpu")]
        public string Vcpu { get; set; }

        [JsonProperty("memory")]
        public string Memory { get; set; }

        [JsonProperty(PropertyName = "fed_ramp_type")]
        public EFedRampType FedRampType { get; set; }
    }
}
