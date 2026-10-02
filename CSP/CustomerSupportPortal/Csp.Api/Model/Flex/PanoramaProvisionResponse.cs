using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class PanoramaProvisionResponse
    {

        public bool Status { get; set; }

        public string Message { get; set; }
        [JsonProperty(PropertyName = "serial_numbers")]
        public List<string> SerialNumber;
    }

    public class PanoramaProvisionedDevice
    {
        public long Id { get; set; }
        public string SerialNumber { get; set; }
        public string Resides { get; set; }
        public string ModelName { get; set; }
        public string AuthCode { get; set; }
        public string License { get; set; }
        public string NewLicense { get; set; }
        public string ExpirationDate { get; set; }
        public string NewExpirationDate { get; set; }
        public bool IsFwFlexPanorama { get; set; }

        [JsonProperty(PropertyName = "fed_ramp_type")]
        public EFedRampType FedRampType { get; set; }
    }

    public class RepurposePanoramasRequest
    {
        public long UserAccountId { get; set; }
        public long SupportAccountId { get; set; }
        public string AuthCode { get; set; }
        public string ProductId { get; set; }
        public long TokenId { get; set; }
        public string Type { get; set; }
        public string PanOs { get; set; }
        public List<RepurposeReqInfo> RepurposeReqInfo { get; set; }

        [JsonProperty(PropertyName = "fed_ramp_type")]
        public EFedRampType FedRampType { get; set; }
    }

    public class RepurposeReqInfo
    {
        public string SerialNumber { get; set; }
        public string ToAuthCode { get; set; }
        public string SourceAuthCode { get; set; }
    }

    public class RepurposePanoramasResponse
    {
        public string serialNumber { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
        [JsonIgnore]
        public string Endpoint { get; set; }

    }

    public class PanoramaMigration
    {
        public long Id { get; set; }
        public string SerialNumber { get; set; }
        public string Resides { get; set; }
        public string ModelName { get; set; }
        public string ExpirationDate { get; set; }
        public string NewExpirationDate { get; set; }
        public string SourceAuthcode { get; set; }

        [JsonProperty(PropertyName = "fed_ramp_type")]
        public EFedRampType FedRampType { get; set; }
    }

    public class PanoramaMigrationDetail
    {
        public long Id { get; set; }
        public string AuthCode { get; set; }
        public string ExpirationDate { get; set; }
        public string CurrentLicense { get; set; }
        public string NewLicense { get; set; }
       
    }

}
