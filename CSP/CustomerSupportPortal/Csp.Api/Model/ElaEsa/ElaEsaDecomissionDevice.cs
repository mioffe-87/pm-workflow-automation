using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaEsaDecomissionDevice
    {
        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("material")]
        public string Material { get; set; }

        [JsonProperty("decommissionCategory")]
        public int DecommissionCategory { get; set; }

        [JsonProperty("subCategory")]
        public int SubCategory { get; set; }

        [JsonProperty("docHeaderText")]
        public string DocHeaderText { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("countryId")]
        public long CountryId { get; set; }
        public string Country;


        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("zipCode")]
        public string ZipCode { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("deviceReturn")]
        public bool RecycleDevice { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
        


        [JsonProperty("ea")]
        public IEnumerable<ElaEsaDecomissionDeviceDetail> ElaEsas { get; set; }

        public string[] Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(SerialNumber))
            {
                errors.Add("Serial number is required.");
            }

            if (DecommissionCategory <= 0)
            {
                errors.Add(string.Format("Serial Number {0}: Decommission Category is required.", SerialNumber));
            }

            if (SubCategory <= 0)
            {
                errors.Add(string.Format("Serial Number {0}: Sub Category is required.", SerialNumber));
            }

            if (string.IsNullOrWhiteSpace(DocHeaderText))
            {
                errors.Add(string.Format("Serial Number {0}: Doc Header Text is required.", SerialNumber));
            }

            if (ElaEsas != null)
            {
                errors.AddRange(ElaEsas.SelectMany(i => i.Validate()).ToArray());
            }

            return errors.Any() ? errors.ToArray() : null;
        }
    }
}
