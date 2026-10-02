using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class DeploymentProfileAttributeValue
    {
        [JsonProperty("unit_of_measure")]
        public string UnitOfMeasure { get; set; }

        [JsonProperty("quantity")]
        public string Value { get; set; }

        public static DeploymentProfileAttributeValue[] ToAttributesArray(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<DeploymentProfileAttributeValue>();
            }
            try
            {
                return JsonConvert.DeserializeObject<DeploymentProfileAttributeValue[]>(value);
            }
            catch // swallow the exception because it isn't critical, considering that this utility method is used only for retreival devices
            {
                return Array.Empty<DeploymentProfileAttributeValue>();
            }
        }
    }
}
