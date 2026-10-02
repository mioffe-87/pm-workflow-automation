using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class MasterContractRequest
    {
        [JsonProperty("QueryHeader")]
        public List<MasterContractRequestHeader> QueryHeader { get; set; }
    }

    public class MasterContractRequestHeader
    {
        [JsonProperty("AdminEmail")]
        public string AdminEmail { get; set; }

        [JsonProperty("SAPSoldTo")]
        public string SapSoldTo { get; set; }

        [JsonProperty("SAPEndUser")]
        public string SapEndUser { get; set; }

        [JsonProperty("ParentOrder")]
        public string MasterOrderNum { get; set; }

        [JsonProperty("QueryType")]
        public string QueryType { get; set; }
    }

    public enum MasterContractQueryType
    {
        List,
        Detail
    }
}
