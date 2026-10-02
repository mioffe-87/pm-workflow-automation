using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireApiKeysResponse
    {
        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("queryMaxLimit")]
        public int QueryMaxLimit { get; set; }

        [JsonProperty("uploadMaxLimit")]
        public int UploadMaxLimit { get; set; }

        public string EndDateString { get; set; }
    }

    public class WildfireApiKeysResponseDetails
    {
        [JsonProperty("wildfireAPIkeys")]
        public List<WildfireApiKeysResponse> WildfireAPIkeys { get; set; }
    }

    public class WildfireApiKeyList
    {
        [JsonProperty("data")]
        public List<WildfireApiKeysResponse> WildfireAPIkeys { get; set; }

        public int Total { get; set; }
    }
}
