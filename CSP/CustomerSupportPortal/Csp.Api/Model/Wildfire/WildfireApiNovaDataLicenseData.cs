using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireApiNovaDataLicenseData
    {
        [JsonProperty("queryMaxLimit")]
        public int? Queries { get; set; }

        [JsonProperty("uploadMaxLimit")]
        public int? Submissions { get; set; }

        [JsonProperty("Licensing_Features__c")]
        public string LicensingFeaturesC { get; set; }

        public int? PurchaseQuantity
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(LicensingFeaturesC))
                {
                    try
                    {
                        dynamic c = JsonConvert.DeserializeObject<dynamic>(LicensingFeaturesC);
                        if (c is JArray)
                        {
                            return ((JArray)c).Where(i => i is JObject && ((JObject)i)["quantity"] != null).Sum(i => (int)((JObject)i)["quantity"]);
                        }
                    }
                    catch
                    {
                    }
                }

                return null;
            }
        }
    }
}
