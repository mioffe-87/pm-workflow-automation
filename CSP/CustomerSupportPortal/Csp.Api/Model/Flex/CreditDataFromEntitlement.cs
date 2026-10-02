using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class CreditDataFromEntitlement
    {
        [JsonProperty("license")]
        public List<CreditDetailFromEntitlement> Credits { get; set; }

        public CreditData ToCreditDetail()
        {
            return new CreditData
            {
                Credits = Credits?.Select(i => new CreditDetail
                {
                    SalesAccountId = i.SalesAccountId,
                    SalesAccountName = i.SalesAccountName,
                    SupportAccountId = i.SupportAccountId,
                    AscAccountId = i.AscAccountId,
                    SupportType = i.SupportType,
                    TokenId = i.Entitlements?.FirstOrDefault(p => p.ProductType == "Primary")?.TokenId ?? 0L,
                    ProductId = i.Entitlements?.FirstOrDefault(p => p.ProductType == "Primary")?.ProductId ?? string.Empty,
                    CreditName = i.Entitlements?.FirstOrDefault(p => p.ProductType == "Primary")?.CreditName ?? string.Empty,
                    Sku = i.Entitlements?.FirstOrDefault(p => p.ProductType == "Primary")?.Sku ?? string.Empty,
                    IsEval = false, // TODO
                    EffectiveDate = i.Entitlements?.FirstOrDefault(p => p.ProductType == "Primary")?.EffectiveDate ?? string.Empty,
                    ExpirationDate = i.Entitlements?.FirstOrDefault(p => p.ProductType == "Primary")?.ExpirationDate ?? string.Empty,
                    UnitOfMeasure = i.Entitlements?.FirstOrDefault(p => p.ProductType == "Primary")?
                        .LicenseFeatures?.FirstOrDefault(p => p.Keys.Contains("unitOfMeasure")) != null
                        ? i.Entitlements.First(p => p.ProductType == "Primary").LicenseFeatures.Where(p => p.Keys.Contains("unitOfMeasure")).First()["unitOfMeasure"].ToString()
                        : string.Empty,
                    TotalPurchased = i.Entitlements?.FirstOrDefault(p => p.ProductType == "Primary")?
                        .LicenseFeatures?.FirstOrDefault(p => p.Keys.Contains("quantity")) != null
                        ? Convert.ToDouble(i.Entitlements.First(p => p.ProductType == "Primary").LicenseFeatures.Where(p => p.Keys.Contains("quantity")).First()["quantity"].ToString())
                        : 0D,
                    TotalAllocated = -1, // TODO
                    NetBalance = -1, // TODO
                    NetConsumption = -1, // TODO
                    ParentTokenId = -1, // TODO
                    DiscontinuedSubscriptions = "", // TODO
                    CreditPoolRenewalStatus = "", // TODO
                    Status = i.Entitlements?.FirstOrDefault(p => p.ProductType == "Primary")?.Status ?? string.Empty,
                    TransactionType = "", // TODO
                    PoolName = "", // TODO
                    GenType = "", // TODO
                    IsAirgapEligible = false // TODO
                }).ToList()
            };
        }
    }
}