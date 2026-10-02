using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public abstract class WildfireApiResponseBase
    {

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        public static WildfireApiActivateResponse SetError(string messag)
        {
            return new WildfireApiActivateResponse
            {
                Success = false,
                Message = messag
            };
        }
    }
}
