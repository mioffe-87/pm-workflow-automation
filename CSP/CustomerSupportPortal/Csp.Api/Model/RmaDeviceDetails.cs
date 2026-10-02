using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class RmaDeviceDetails
    {
        [JsonProperty("ts_filename")]
        public string TechSupportFileName { get; set; }

        [JsonProperty("serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("contact_id")]
        public string ContactId { get; set; }

        [JsonProperty("product_code")]
        public string ProductCode { get; set; }

        [JsonProperty("distributor_id")]
        public string DistributorId { get; set; }

        [JsonProperty("end_customer_id")]
        public string EndCustomerId { get; set; }

        [JsonProperty("shipping_address")]
        public string ShippingAddress { get; set; }

        [JsonProperty("shipping_city")]
        public string ShippingCity { get; set; }

        [JsonProperty("shipping_state")]
        public string ShippingState { get; set; }

        [JsonProperty("shipping_country")]
        public string ShippingCountry { get; set; }

        [JsonProperty("shipping_postal_code")]
        public string ShippingPostalCode { get; set; }

        [JsonProperty("site_contact")]
        public string SiteContact { get; set; }

        [JsonProperty("site_email")]
        public string SiteEmail { get; set; }

        [JsonProperty("site_phone")]
        public string SitePhone { get; set; }
    }
}
