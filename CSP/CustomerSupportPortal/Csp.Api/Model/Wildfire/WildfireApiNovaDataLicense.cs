using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireApiNovaDataLicense
    {
        private const string PRODUCT_TYPE_PRIMARY = "Primary";

        [JsonProperty("salesAccountId")]
        public string SalesforceAccountId { get; set; }

        [JsonProperty("supportAccountId")]
        public long? SupportAccountId { get; set; }

        [JsonProperty("supportType")]
        public string SupportType { get; set; }

        [JsonProperty("entitlements")]
        public IEnumerable<WildfireApiNovaDataEntitlement> Entitlements { get; set; }

        public WildfireAssetItem MapToAssets()
        {
            WildfireApiNovaDataEntitlement wfApiLicense = GetPromaryWfApiLicense();
            if (wfApiLicense == null)
            {
                return null;
            }

            WildfireAssetItem item = wfApiLicense.MapToAssets();
            item.SupportLicenseType = SupportType;
            return item;
        }

        public WildfireActivationLicense MapToActivationLicenses()
        {
            WildfireApiNovaDataEntitlement wfApiLicense = GetPromaryWfApiLicense();
            if (wfApiLicense == null)
            {
                return null;
            }

            WildfireActivationLicense item = wfApiLicense.MapToActivationLicenses();

            item.SalesforceAccountId = SalesforceAccountId;
            item.SupportAccountId = SupportAccountId;
            item.SupportLicenseType = SupportType;
            return item;
        }

        private WildfireApiNovaDataEntitlement GetPromaryWfApiLicense()
        {
            return Entitlements
                .Where(i => i != null && !string.IsNullOrWhiteSpace(i.ModelName) && i.ModelName.Contains("WFAPI"))
                .SingleOrDefault(i => i.ProductType == PRODUCT_TYPE_PRIMARY); // only 1 WF API license is expected in a License.Entitlements
        }
    }
}