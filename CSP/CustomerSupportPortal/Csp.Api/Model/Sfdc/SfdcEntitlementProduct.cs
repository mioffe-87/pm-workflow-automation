using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Csp.Api.Model.Enums;

namespace Csp.Api.Model.Sfdc
{
    public class SfdcEntitlementProduct
    {
        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("primaryProduct")]
        public bool PrimaryProduct { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("licenseFeatures")]
        public IEnumerable<SfdcEntitlementProductLicenseFeature> LicensingFeatures { get; set; }

        [JsonProperty("configuration")]
        public IEnumerable<SfdcEntitlementProductConfiguration> Configuration { get; set; }

        [JsonProperty("bundleComponents")]
        public IEnumerable<SfdcEntitlementProductBundleComponent> BundleComponents { get; set; }

        [JsonProperty("links")]
        public IEnumerable<SfdcEntitlementLink> Links { get; set; }

        [JsonProperty("appId")]
        public string AppId { get; set; }

        [JsonIgnore]
        public SfdcProductFamily ProductFamily
        {
            get
            {
                switch (AppId)
                {
                    case "cortex_xdr":
                        return SfdcProductFamily.Xdr;
                    case "cortex_demisto":
                    case "cortex_xsoar":
                        return SfdcProductFamily.Xsoar;
                    case "prisma_cloud":
                        return SfdcProductFamily.PrismaCloud;
                    case "prisma_twistlock":
                        return SfdcProductFamily.PrismaTwistlock;
                    case "Logging Service":
                    case "logging_service":
                        return SfdcProductFamily.LoggingService;
                    default:
                        return SfdcProductFamily.General;
                }
            }
        }
    }
}
