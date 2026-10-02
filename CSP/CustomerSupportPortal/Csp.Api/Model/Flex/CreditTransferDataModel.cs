using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class CreditTransferDataModel
    {
        public int UserAccountId { get; set; }

        public long SourceSupportAccountId { get; set; }

        public long? DestinationSupportAccountId { get; set; }

        public long SourceTokenId { get; set; }

        public long DestinationTokenId { get; set; }

        public string SourceProductId { get; set; }

        public string DestinationProductId { get; set; }

        [JsonConverter(typeof(FwFlexDateFormatConverter), "yyyy-MM-ddThh:mm:ss.fffZ")]
        public DateTime TransactionDate { get; set; }

        public string CrossRefTransactionId { get; set; }

        public double Quantity { get; set; }

        public string UsageDescription { get; set; }
    }
}
