using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class PingIdResponsePayload
    {
        [JsonProperty("responseBody")]
        public PingIdResponse ResponseBody { get; set; }

        [JsonIgnore]
        public MfaRegistrationStatus Status { get; set; }
    }

    public class PingIdResponse
    {
        [JsonProperty("activationCode")]
        public string ActivationCode { get; set; }

        [JsonProperty("clientData")]
        public string ClientData { get; set; }

        [JsonProperty("errorId")]
        public string ErrorId { get; set; }

        [JsonProperty("errorMsg")]
        public string ErrorMsg { get; set; }

        [JsonProperty("uniqueMsgId")]
        public string UniqueMsgId { get; set; }
    }

    public class GoogleAuthenticatorResponsePayload
    {
        [JsonProperty("encryptedTOTP")]
        public string EncryptedSecret { get; set; }

        [JsonIgnore]
        public bool IsSuccess { get; set; }
    }
}
