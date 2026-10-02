using Newtonsoft.Json;

namespace Csp.Api.Model.Flex
{
    public class TransferDeploymentProfileResponse
    {
        public bool Success { get; set; }
        public string Authcode { get; set; }
        public string Message { get; set; }
        [JsonIgnore]
        public string Endpoint { get; set; }

    }

}