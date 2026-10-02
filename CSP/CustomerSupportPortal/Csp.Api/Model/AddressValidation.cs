using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Csp.Api.Model
    {

        public class AddressAPIRequest
        {
            [JsonProperty(PropertyName = "street")]
            public string Street { get; set; }

            [JsonProperty(PropertyName = "city")]
            public string City { get; set; }

            [JsonProperty(PropertyName = "state")]
            public string State { get; set; }

            [JsonProperty(PropertyName = "zip")]
            public string Zip { get; set; }

            [JsonProperty(PropertyName = "country")]
            public string Country { get; set; }

            [JsonProperty(PropertyName = "company_name")]
            public string CompanyName { get; set; }

            [JsonProperty(PropertyName = "domain")]
            public string Domain { get; set; }

            [JsonProperty(PropertyName = "phoneNumber")]
            public string PhoneNumber { get; set; }
        }

        public class AddressAPIResponse
        {
            [JsonProperty(PropertyName = "address1")]
            public string Address1 { get; set; }

            [JsonProperty(PropertyName = "address2")]
            public string Address2 { get; set; }

            [JsonProperty(PropertyName = "city")]
            public string City { get; set; }

            [JsonProperty(PropertyName = "state")]
            public string State { get; set; }

            [JsonProperty(PropertyName = "zip")]
            public string Zip { get; set; }

            [JsonProperty(PropertyName = "country")]
            public string Country { get; set; }

            [JsonProperty(PropertyName = "county")]
            public string County { get; set; }

            [JsonProperty(PropertyName = "geo_coding")]
            public GeoCoding GeoCoding { get; set; }

            [JsonProperty(PropertyName = "process_status")]
            public string ProcessStatus { get; set; }
        }

        public class GeoCoding
        {
            [JsonProperty(PropertyName = "latitude")]
            public double Latitude { get; set; }
            [JsonProperty(PropertyName = "longitude")]
            public double Longitude { get; set; }
        }

    }

}
