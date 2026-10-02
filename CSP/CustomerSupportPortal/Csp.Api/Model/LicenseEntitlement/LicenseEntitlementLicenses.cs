using System;
using Csp.Api.Model.Attributes;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model {
    public class LicenseEntitlementLicenses
    {
        [JsonProperty("auth_code")]
        public string AuthCode { set; get; }

        [JsonProperty("sku")]
        public string Sku { set; get; }

        [JsonProperty("expiration_date")]
        public DateTime ExpirationDate { set; get; }

        [JsonProperty("serial_number")]
        public string SerialNumber { set; get; }

        [JsonProperty("platform_sku")]
        public string PlatformSku { set; get; }
    }
}
