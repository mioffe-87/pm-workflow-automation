using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class Nova
    {
        
    }
    public class NovaSfdcToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("instance_url")]
        public string InstanceUrl { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("issued_at")]
        public string IssuedAt { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }

    public class NovaAscAccount
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }
    } 


    public class NovaEntitlement
    {
        [JsonProperty("supportAccount")]
        public NovaSupportAccount SupportAccount { get; set; }

        [JsonProperty("tenantId")]
        public long TenantId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("sfdcEntitlementId")]
        public string SfdcEntitlementId { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("product")]
        public NovaProduct Product { get; set; }

        [JsonProperty("ascAccount")]
        public NovaAscAccount AscAccount { get; set; } 

        [JsonProperty("partner")]
        public NovaPartner Partner { get; set; }

        [JsonProperty("isTrial")]
        public bool IsTrial { get; set; }

        [JsonProperty("instanceUrl")]
        public NovaLink InstanceUrl { get; set; }

        [JsonProperty("entitlementId")]
        public string EntitlementId { get; set; }

        [JsonProperty("entitlementGroupId")]
        public string EntitlementGroupId { get; set; }

        [JsonProperty("entitlementContacts")]
        public IEnumerable<NovaEntitlementContact> EntitlementContacts { get; set; }

        [JsonProperty("effectivePeriod")]
        public NovaEffectivePeriod EffectivePeriod { get; set; }

        [JsonProperty("channelPlatform")]
        public string ChannelPlatform { get; set; }

        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("salesAccount")]
        public NovaAccount SalesAccount { get; set; }

        [JsonProperty("order")]
        public NovaOrder Order { get; set; }

        [JsonProperty("errors")]
        public IEnumerable<NovaResponseError> Errors { get; set; }
    }
    public class NovaSupportAccount
    {
        [JsonProperty("supportAccountId")]
        public string SupportAccountId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public class NovaProduct
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

        [JsonProperty("appId")]
        public string AppId { get; set; }

        [JsonProperty("appName")]
        public string AppName { get; set; }

        [JsonProperty("licenseFeatures")]
        public IEnumerable<NovaProductLicenseFeature> LicensingFeatures { get; set; }

        [JsonProperty("configuration")]
        public IEnumerable<NovaProductConfiguration> Configuration { get; set; }

        [JsonProperty("bundleComponents")]
        public IEnumerable<NovaProductBundleComponent> BundleComponents { get; set; }

        [JsonProperty("links")]
        public IEnumerable<NovaLink> Links { get; set; }
    }
    public class NovaPartner
    {
        [JsonProperty("account")]
        public NovaAccount Account { get; set; }

        [JsonProperty("contact")]
        public NovaEntitlementContact Contact { get; set; }
    }
    public class NovaLink
    {
        [JsonProperty("serviceType")]
        public string ServiceType { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class NovaEntitlementContact
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("contactId")]
        public string ContactId { get; set; }

        [JsonProperty("culture")]
        public string Culture { get; set; }
    }
    public class NovaEffectivePeriod
    {
        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }
    }
    public class NovaAccount
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("culture")]
        public string Culture { get; set; }
    }
    public class NovaOrder
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("orderDate")]
        public string OrderDate { get; set; }
    }
    public class NovaResponseError
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
    public class NovaProductLicenseFeature
    {
        [JsonProperty("unit_of_measure")]
        public string UnitOfMeasure { get; set; }

        [JsonProperty("quantity")]
        public string Quantity { get; set; }
    }
    public class NovaProductConfiguration
    {
        [JsonProperty("region")]
        public string Region { get; set; }
    }
    public class NovaProductBundleComponent
    {
    }
}
