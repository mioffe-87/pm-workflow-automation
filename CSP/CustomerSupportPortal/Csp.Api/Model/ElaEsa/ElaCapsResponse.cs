using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaCapsResponse<T>
    {
        public const string MESSAGE_SUCCESS = "success";
        public const string MESSAGE_ERROR = "error";

        [JsonProperty("assets")]
        public T Data { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = MESSAGE_SUCCESS;

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
