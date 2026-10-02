using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class CreditDataModel
    {
        public long TokenId { get; set; }

        public string SalesAccountId { get; set; }

        public string SalesAccountName { get; set; }

        public long? SupportAccountId { get; set; }

        public string SupportType { get; set; }

        public string ProductId { get; set; }

        public string CreditName { get; set; }

        public string CreditType { get; set; }

        public string Sku { get; set; }

        public bool IsEval { get; set; }

        public string AscAccountId { get; set; }

        public string EffectiveDate { get; set; }

        public string ExpirationDate { get; set; }

        public bool IsExpired { get; set; }

        public string UnitOfMeasure { get; set; }

        public double TotalPurchased { get; set; }

        public double TotalAllocated { get; set; }

        public double NetBalance { get; set; }

        public double NetConsumption { get; set; }

        public int NumberOfDeployments { get; set; }

        [JsonIgnore]
        public DateTime? Expiration { get; set; }

        public string CreditPoolRenewalStatus { get; set; }

        public string ParentTokenId { get; set; }

        public string Status { get; set; }

        public bool IsRenewalConfirmed { get; set; }

        public string TransactionType { get; set; }

        public List<PoolMessage> PoolMessages { get; set; }

        public string PoolName { get; set; }

        public string GenType { get; set; }

        public bool IsAirgapEligible { get; set; }

        public EFedRampType AccountFedRampType { get; set; }
        
        [JsonProperty("isGracePeriodDp")]
        public bool IsGracePeriodDp { get; set; }

        public CreditDataTraitsModel Traits { get; set; }
    }

    public class PoolMessage
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public List<RampUpData> Data { get; set; }
    }

    public class RampUpData
    {
        public int RampId { get; set; }
        public string TransactionType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public double Credits { get; set; }
        public string Status { get; set; }
    }

    public class CreditDataModelDto
    {
        [JsonProperty(PropertyName = "tokenId")]
        public long TokenId { get; set; }

        [JsonProperty(PropertyName = "salesAccountId")]
        public string SalesAccountId { get; set; }

        [JsonProperty(PropertyName = "salesAccountName")]
        public string SalesAccountName { get; set; }

        [JsonProperty(PropertyName = "supportAccountId")]
        public long? SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "supportType")]
        public string SupportType { get; set; }

        [JsonProperty(PropertyName = "productId")]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "creditName")]
        public string CreditName { get; set; }

        [JsonProperty(PropertyName = "creditType")]
        public string CreditType { get; set; }

        [JsonProperty(PropertyName = "sku")]
        public string Sku { get; set; }

        [JsonProperty(PropertyName = "isEval")]
        public bool IsEval { get; set; }

        [JsonProperty(PropertyName = "ascAccountId")]
        public string AscAccountId { get; set; }

        [JsonProperty(PropertyName = "effectiveDate")]
        public string EffectiveDate { get; set; }

        [JsonProperty(PropertyName = "expirationDate")]
        public string ExpirationDate { get; set; }

        [JsonProperty(PropertyName = "isExpired")]
        public bool IsExpired { get; set; }

        [JsonProperty(PropertyName = "unitOfMeasure")]
        public string UnitOfMeasure { get; set; }

        [JsonProperty(PropertyName = "totalPurchased")]
        public double TotalPurchased { get; set; }

        [JsonProperty(PropertyName = "totalAllocated")]
        public double TotalAllocated { get; set; }

        [JsonProperty(PropertyName = "netBalance")]
        public double NetBalance { get; set; }

        [JsonProperty(PropertyName = "netConsumption")]
        public double NetConsumption { get; set; }

        [JsonProperty(PropertyName = "numberOfDeployments")]
        public int NumberOfDeployments { get; set; }

        [JsonProperty(PropertyName = "poolMessages")]
        public List<PoolMessage> PoolMessages { get; set; }

    }
    
    public class TokenDto
    {
        public IList<TokenInfo> data;
    }

    public class TokenInfo
    {
        [JsonProperty(PropertyName = "creditPoolId")]
        public long TokenId { get; set; }

        [JsonProperty(PropertyName = "poolName")]
        public string CreditName { get; set; }

        [JsonProperty(PropertyName = "supportType")]
        public string SupportType { get; set; }

        [JsonProperty(PropertyName = "expirationDate")]
        public string ExpirationDate { get; set; }

        [JsonProperty(PropertyName = "totalCredits")]
        public double TotalPurchased { get; set; }

        [JsonProperty(PropertyName = "creditsAllocated")]
        public double TotalAllocated { get; set; }

        [JsonProperty(PropertyName = "creditsConsumed")]
        public double NetConsumption { get; set; }

        [JsonProperty(PropertyName = "creditsAvailable")]
        public double NetBalance { get; set; }
 
    }
}
