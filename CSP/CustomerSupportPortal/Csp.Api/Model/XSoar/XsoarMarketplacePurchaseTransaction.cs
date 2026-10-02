using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Xsoar
{
    public class XsoarMarketplacePurchaseTransaction
    {
        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("purchaseDate")]
        public DateTime PurchaseDate { get; set; }

        [JsonProperty("quantityPurchased")]
        public double QuantityPurchased { get; set; }
    }

}