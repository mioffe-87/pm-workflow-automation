using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Xsoar
{
    public class XsoarMarketplacePurchaseDataModel
    {
        public int Id { get; set; }

        public string ModelName { get; set; }

        public string TotalPointPurchased { get; set; }

        public string PurchaseDate { get; set; }
    }
}