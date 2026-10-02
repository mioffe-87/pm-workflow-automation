using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Flex
{
    public class FlexBaseRequestPayload<T>
    {
        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty("data")]
        public List<T> Data { get; set; }
    }
}
