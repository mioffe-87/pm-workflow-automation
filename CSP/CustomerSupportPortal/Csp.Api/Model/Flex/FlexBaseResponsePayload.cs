using Newtonsoft.Json;
using System.Collections.Generic;
namespace Csp.Api.Model.Flex
{
    public class FlexBaseResponsePayload<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("errors")]
        public List<ResponseError> Errors { get; set; }
    }

    public class ResponseError
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("locations")]
        public List<ErrorLocation> Locations { get; set; }

        [JsonProperty("path")]
        public List<string> Path { get; set; }
    }

    public class ErrorLocation
    {
        [JsonProperty("line")]
        public long Line { get; set; }

        [JsonProperty("column")]
        public long Column { get; set; }
    }
}
