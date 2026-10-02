using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class PanoramaProvisionModel
    {
        public long SupportAccountId { get; set; }

        public long UserAccountId { get; set; }

        public string AuthCode { get; set; }

        public int Quantity { get; set; }

        public string ProductId { get; set; }

        public string Type { get; set; }

        public long TokenId { get; set; }

        public string PanOs { get; set; }

        public string SubType { get; set; }

        public string[] Panorama { get; set; }

        [JsonProperty(PropertyName = "fed_ramp_type")]
        public EFedRampType FedRampType { get; set; }
    }

    public class PanoramasDeleteRequest
    {
        public long SupportAccountId { get; set; }

        public long UserAccountId { get; set; }

        public string AuthCode { get; set; }

        public string ProductId { get; set; }

        public string Type { get; set; }

        public long TokenId { get; set; }
        public List<SerialNumbers> serialNumbers { get; set; }

        public string PanOs { get; set; }

        [JsonProperty(PropertyName = "fed_ramp_type")]
        public EFedRampType FedRampType { get; set; }
    }

    public class PanoramasDeleteResponse
    {
        public string serialNumber { get; set; }

        public bool success { get; set; }

        public string message { get; set; }

        [JsonIgnore]
        public string Endpoint { get; set; }
    }

    public class SerialNumbers
    {
        public string SerialNumber { get; set; }

      
    }
}
