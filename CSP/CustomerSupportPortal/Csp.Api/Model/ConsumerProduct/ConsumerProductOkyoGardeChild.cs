using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ExtendedExpertise
{
    public class ConsumerProductOkyoGardeChild
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("authCode")]
        public string AuthCode { get; set; }

        [JsonProperty("partNumber")]
        public string PartNumber { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("quantity")]
        public int? Quantity { get; set; }

        [JsonProperty("activatedQuantity")]
        public int? ActivatedQuantity { get; set; }

        [JsonProperty("availableQuantity")]
        public int? AvailableQuantity { get; set; }

        public string Export()
        {
            return (!string.IsNullOrWhiteSpace(AuthCode) ? AuthCode : string.Empty) + ","
                + (!string.IsNullOrWhiteSpace(PartNumber) ? PartNumber : string.Empty) + ","
                + (!string.IsNullOrWhiteSpace(Description) ? Description : string.Empty) + ","
                + Quantity + "," + ActivatedQuantity + "," + AvailableQuantity;
        }

        public static string ExportEmpty()
        {
            return ",,,,,\r\n";
        }
    }
}
