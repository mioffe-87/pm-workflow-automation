using System;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class LogApiCall
    {
        public long LogApiCallId { get; set; }

        [JsonProperty("application")]
        public string Application { get; set; }

        [JsonProperty("apiCalled")]
        public string ApiCalled { get; set; }

        [JsonProperty("apiMethod")]
        public string ApiMethod { get; set; }

        [JsonProperty("requestPayload")]
        public string RequestPayload { get; set; }

        [JsonProperty("responseStatus")]
        public string ResponseStatus { get; set; }

        [JsonProperty("responsePayload")]
        public string ResponsePayload { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; }
    }
}
