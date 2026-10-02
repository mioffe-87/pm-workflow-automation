using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class CreditDetail
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

        [JsonProperty(PropertyName = "productName")]
        public string CreditName { get; set; }

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

        [JsonProperty(PropertyName = "unitOfMeasure")]
        public string UnitOfMeasure { get; set; }

        [JsonProperty(PropertyName = "quantityPurchased")]
        public double TotalPurchased { get; set; }

        [JsonProperty(PropertyName = "quantityNetAllocated")]
        public double TotalAllocated { get; set; }

        [JsonProperty(PropertyName = "quantityNetBalance")]
        public double NetBalance { get; set; }

        [JsonProperty(PropertyName = "quantityNetConsumption")]
        public double NetConsumption { get; set; }

        [JsonProperty("quantityAllocation")]
        public List<QuantityAllocation> QuantityAllocation { get; set; }

        [JsonProperty("creditPoolRenewalStatus")]
        public string CreditPoolRenewalStatus { get; set; }

        [JsonProperty("parentTokenId")]
        public long ParentTokenId { get; set; }

        [JsonProperty("discontinuedSubscriptions")]
        public string DiscontinuedSubscriptions { get; set; }

        [JsonProperty("consumptionRenewalStatus")]
        public string ConsumptionRenewalStatus { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("poolName")]
        public string PoolName { get; set; }

        [JsonProperty(PropertyName = "genType")]
        public string GenType { get; set; }

        [JsonProperty(PropertyName = "isAirgapEligible")]
        public bool IsAirgapEligible { get; set; }
    }
       

    public class CreditPool
    {
        [JsonProperty(PropertyName = "tokenId")]
        public long TokenId { get; set; }

        [JsonProperty(PropertyName = "supportType")]
        public string SupportType { get; set; }

        [JsonProperty(PropertyName = "productName")]
        public string CreditName { get; set; }

        [JsonProperty(PropertyName = "expirationDate")]
        public DateTime? ExpirationDate { get; set; }


        [JsonProperty(PropertyName = "quantityNetBalance")]
        public double NetBalance { get; set; }

        [JsonProperty(PropertyName = "productId")]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "sku")]
        public string Sku { get; set; }

        [JsonProperty(PropertyName = "isAirgapEligible")]
        public bool IsAirgapEligible { get; set; }
    }
}
