using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class DeploymentProfileModel
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "authcode")]
        public string Authcode { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }


    public class DeploymentDeleteResponse
    {
        [JsonProperty(PropertyName = "isDeleted")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "dAuthcode")]
        public string Authcode { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }


    public class DeletePanoramasResponse
    {
        public bool Success { get; set; }
        public string SerialNumbers { get; set; }
        public string Message { get; set; }
    }
}
