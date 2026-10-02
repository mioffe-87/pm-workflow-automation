using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{

    public class PoolEditReq
    {
        [JsonProperty(PropertyName = "tokenId")]
        public long TokenId { get; set; }

        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId { get; set; }
       
        [JsonProperty(PropertyName = "productId")]
        public string ProductId { get; set; }
 
        [JsonProperty(PropertyName = "xrefTransactionId")]
        public string CrossRefTransactionId { get; set; }

        [JsonProperty(PropertyName = "transactionDate")]
        [JsonConverter(typeof(FwFlexDateFormatConverter), "yyyy-MM-ddThh:mm:ss.fffZ")]
        public DateTime TransactionDate { get; set; }

        [JsonProperty(PropertyName = "poolName")]
        public string PoolName { get; set; }
    }

    public class DepositCreditRequest
    {
        [JsonProperty(PropertyName = "supportAccountId")]
        public long SupportAccountId { get; set; }

        [JsonProperty(PropertyName = "tokenId")]
        public long TokenId { get; set; }

        [JsonProperty(PropertyName = "productId")]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "transactionDate")]
        [JsonConverter(typeof(FwFlexDateFormatConverter), "yyyy-MM-ddThh:mm:ss.fffZ")]
        public DateTime TransactionDate { get; set; }

        [JsonProperty(PropertyName = "xrefTransactionId")]
        public string CrossRefTransactionId { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public long Quantity { get; set; }

    }

    public class DepositCreditModel
    {
        public int UserAccountId { get; set; }
        public long SupportAccountId { get; set; }
        public List<CreditAssociation> CreditAssociations { get; set; }
        public DateTime ActivationDate { get; set; }
    }

    public class CreditAssociation
    {
        public long TokenId { get; set; }
        public string ProductId { get; set; }

        public int TotalPurchased { get; set; }
    }
}
