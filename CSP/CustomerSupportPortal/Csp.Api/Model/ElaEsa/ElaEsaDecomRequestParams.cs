using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaEsaDecomRequestParams
    {
        [JsonProperty("userAccountId")]
        public long UserAccountId { get; set; }

        [JsonProperty("supportAccountId")]
        public long SupportAccountId { get; set; }
    }
}
