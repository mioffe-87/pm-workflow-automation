using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.Ela2
{
    public class ErrorDetails
    {
        [JsonProperty("error_code")]
        public int Error_code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
