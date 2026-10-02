using System;
using Csp.Api.Model.Attributes;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Csp.Api.Model { 
    public class LicenseEntitlementTenant
    {
        [JsonProperty("tenant_id")]
        public string TenantId { set; get; }

        [JsonProperty("tenant_type")]
        public string TenantType { set; get; }

        [JsonProperty("tenant_already_existed")]
        public bool TenantExistStatus{ set; get; }        
    }    
}
