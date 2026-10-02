using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Csp.Api.Model.Ela2
{
   public class ElaConversionStatusLicenseDataResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error_details")]
        public ErrorDetails errorDetails { get; set; }

        [JsonProperty("RequestId")]
        public int RequestId { get; set; }

        [JsonProperty("HttpStatus")]
        public string HttpStatus { get; set; }

    }
}
