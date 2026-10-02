using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class ClaimedAssetListItem
    {
        public long? ClaimedPurchaseId { get; set; }
        public long Id { get; set; }
        public string AssetType { get; set; }
        public string Code { get; set; }
        public string PartNumber { get; set; }
        public DateTime ShipDate { get; set; }
        public string SalesOrderNumber { get; set; }

        public string UniqueId
        {
            get { return string.Format("{0}_{1}", AssetType, Id); }
        }
    }
}
