using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Xsoar
{
    public class XsoarMarketplacePurchase
    {
        [JsonProperty("data")]
        public XsoarMarketplacePurchaseData Data { get; set; }

        [JsonProperty("errors")]
        public List<XsoarMarketplaceError> Errors { get; set; }

        public List<XsoarMarketplacePurchaseDataModel> MapToXsoarMarketplacePurchaseDataModel()
        {
            int i = 0;
            var purchases = Data.PurchaseTransactions.OrderByDescending(x => x.PurchaseDate).ToList();
            return purchases.Any()
                ? purchases.ConvertAll(a => new XsoarMarketplacePurchaseDataModel
                {
                    Id = i++,
                    ModelName = a.ProductName,
                    PurchaseDate = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(a.PurchaseDate, "Pacific Standard Time").ToString("MM/dd/yyyy"),
                    TotalPointPurchased = Convert.ToInt32(a.QuantityPurchased).ToString()
                })
                : new List<XsoarMarketplacePurchaseDataModel>();
        }
    }
}