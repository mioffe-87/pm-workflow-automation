using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Model.Sfdc
{
    public class SfdcEntitlement
    {
        public static explicit operator AvailableSaaSActivations(SfdcEntitlement the)
        {
            return the == null
                ? null
                : new AvailableSaaSActivations
                {
                };
        }

        [JsonProperty("entitlementGroupId")]
        public string EntitlementGroupId { get; set; }

        [JsonProperty("supportAccount")]
        public SfdcEntitlementSupportAccount SupportAccount { get; set; }

        [JsonProperty("ascAccount")]
        public SfdcAscAccount AscAccount { get; set; }

        [JsonProperty("tenantId")]
        public long TenantId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("sfdcEntitlementId")]
        public string SfdcEntitlementId { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("product")]
        public SfdcEntitlementProduct Product { get; set; }

        [JsonProperty("partner")]
        public SfdcEntitlementPartner Partner { get; set; }

        [JsonProperty("isTrial")]
        public bool IsTrial { get; set; }

        [JsonProperty("instanceUrl")]
        public SfdcEntitlementLink InstanceUrl { get; set; }

        [JsonProperty("entitlementId")]
        public string EntitlementId { get; set; }
      

        [JsonProperty("entitlementContacts")]
        public IEnumerable<SfdcEntitlementContact> EntitlementContacts { get; set; }

        [JsonProperty("effectivePeriod")]
        public SfdcEntitlementEffectivePeriod EffectivePeriod { get; set; }

        [JsonProperty("channelPlatform")]
        public string ChannelPlatform { get; set; }

        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("salesAccount")]
        public SfdcEntitlementAccount SalesAccount { get; set; }

        [JsonProperty("order")]
        public SfdcEntitlementOrder Order { get; set; }

        [JsonProperty("errors")]
        public IEnumerable<SfdcEntitlementResponseError> Errors { get; set; }

        public SearchAllModel MapToEntitlementsSearchAllModel(IEnumerable<SfdcEntitlement> entitlements)
        {
            long supportAccountId = 0L;
            return new SearchAllModel
            {
                SupportAccountId = SupportAccount != null && long.TryParse(SupportAccount.SupportAccountId, out supportAccountId) ? supportAccountId : 0L,
                SupportAccountName = SupportAccount != null ? SupportAccount.Name : string.Empty,
                AssetType = AssetTypes.CloudServices,

                Id = 0L,
                DeviceCode = SerialNumber,
                ModelName = Product.Sku.Contains("-LGS-") ? "Cortex Data Lake" : "Cortex XDR",
                DeviceName = string.Empty,
                City = string.Empty,
                PostalCode = string.Empty,
                Region = string.Empty,
                Country = string.Empty,
                DeviceTag = string.Empty,
                AuthorizedSupportCenter = this.AscAccount != null ? this.AscAccount.Name : string.Empty,
                VmDetails = new VmDetail
                {
                    VmUseCount = null,
                    VmTotalCount = null,
                    IsNsx = null,
                    IsPerpetualNsx = null

                },
                BundleLicenseId = null,
                SearchAllDetails = new SearchAllDetail[] { new SearchAllDetail { 
                                                                   // Quantity = entitlements.Any() ? string.Join(" ", entitlements.Where(e => e.SerialNumber == SerialNumber).Select(e => string.Join("", e.Product.LicensingFeatures.Select(l => "<div class=''>" + l.Quantity + "</div>\n").ToArray())).ToArray()) : string.Empty,
                                                                   Quantity = entitlements.Any() ? formattedQuantity(entitlements) : string.Empty,
                                                                    ExpirationDate = EffectivePeriod.EndDate != null ? DateTime.Parse(EffectivePeriod.EndDate).ToShortDateString()  : "",
                                                                    DtExpirationDate = string.IsNullOrWhiteSpace(EffectivePeriod.EndDate)? (DateTime?)null : DateTime.Parse(EffectivePeriod.EndDate),
                                                                    License = entitlements.Any() ? string.Join(" ", entitlements.Where(e => e.SerialNumber == SerialNumber).Select(e => "<div>" + (e.Product !=null ? e.Product.Name : string.Empty) + "</div>").ToArray()) : string.Empty
                                                            } }
            };
        }
      
        [JsonProperty("licenseData")]
        public string LicenseData { get; set; }

        private string formattedQuantity(IEnumerable<SfdcEntitlement> entitlements)
        {
            //Quantity = entitlements.Any() ? (entitlements.Where(e => e.SerialNumber == SerialNumber && e.Product != null).Select(e => e.Product != null ? string.Join("", e.Product.LicensingFeatures.Select(l => "<div class=''>" + l.Quantity + "</div>\n")).ToArray() : string.Empty).ToString())
            //    : string.Empty,
            StringBuilder strQuantity = new StringBuilder();
            if (entitlements.Any())
            {
                List<SfdcEntitlement> matchingEntitlements = entitlements.Where(e => e.SerialNumber == SerialNumber).ToList();
                foreach (var vEntitlement in matchingEntitlements)
                {
                    if (vEntitlement.Product != null)
                    {
                        foreach (var licenseFeature in vEntitlement.Product.LicensingFeatures)
                        {
                            strQuantity.Append("<div class=''>" + licenseFeature.Quantity + "</div>\n");
                        }
                    }
                }
            }


            return strQuantity.ToString();
        }
    }

    #region GraphQL Api Response
    public class SfdcEntitlementsModel {

        [JsonProperty("entitlementGroupId")]
        public string EntitlementGroupId { get; set; }

        [JsonProperty("salesAccountId")]
        public string SalesAccountId { get; set; }

        [JsonProperty("salesAccountName")]
        public string SalesAccountName { get; set; }

        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty("isCSPAssociated")]
        public bool IsCspAssociated { get; set; }

        [JsonProperty("entitlements")]
        public IEnumerable<SfdcEntitlementDetails> Entitlements { get; set; }

        [JsonProperty("ascAccountName")]
        public string AscName { get; set; }

        [JsonProperty("ascAccountId")]
        public string AscSfAccountId { get; set; }
    }

    public class SfdcEntitlementDetails
    {
        [JsonProperty("sfdcEntitlementId")]
        public string SfdcEntitlementId { get; set; }

        [JsonProperty("tenantId")]
        public long TenantId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; }

        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("licenseFeatures")]
        public IEnumerable<SfdcEntitlementProductLicenseFeature> LicensingFeatures { get; set; }

        [JsonProperty("configuration")]
        public IEnumerable<SfdcEntitlementProductConfiguration> Configuration { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }

        //[JsonProperty("primaryProduct")]
        //public bool PrimaryProduct { get; set; }

    }

    public class SfdcEntitlementResponse
    {
        [JsonProperty("license")]
        public IEnumerable<SfdcEntitlementsModel> SfdcEntitlementsModels { get; set; }
    }

    public class AscResponse
    {
        [JsonProperty("license")]
        public List<AscSupportAccount> AscSupportAccountsList { get; set; }
    }

    public class AscSupportAccount
    {
        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }
    }
    #endregion
}
