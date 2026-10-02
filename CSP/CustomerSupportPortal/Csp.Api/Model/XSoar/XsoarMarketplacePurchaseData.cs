using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Xsoar
{
    public class XsoarMarketplacePurchaseData
    {
        [JsonProperty("purchaseTransactions")]
        public List<XsoarMarketplacePurchaseTransaction> PurchaseTransactions { get; set; }
    }

}