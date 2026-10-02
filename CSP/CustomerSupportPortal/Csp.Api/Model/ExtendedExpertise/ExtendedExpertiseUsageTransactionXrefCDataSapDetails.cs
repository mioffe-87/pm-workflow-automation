using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ExtendedExpertise
{
    public class ExtendedExpertiseUsageTransactionXrefCDataSapDetails
    {
        [JsonProperty("sapBill")]
        public string SapBill { get; set; }

        [JsonProperty("sapSalesOrderLine")]
        public string SapSalesOrderLine { get; set; }

        [JsonProperty("sapSalesOrder")]
        public string SapSalesOrder { get; set; }

        [JsonProperty("sfdcOppurtunityLineItemId")]
        public string SfdcOpportunityLineItemId { get; set; }
    }
}
