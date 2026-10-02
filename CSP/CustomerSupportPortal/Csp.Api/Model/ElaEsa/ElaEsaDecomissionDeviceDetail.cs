using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public class ElaEsaDecomissionDeviceDetail
    {
        [JsonProperty("authCode")]
        public string AuthCode { get; set; }

        [JsonProperty("devicePrice")]
        public double Price { get; set; }

        [JsonProperty("type")]
        public EElaEsaAgreementType AgreementType { get; set; }

        public string[] Validate()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(AuthCode))
            {
                errors.Add("AuthCode is required.");
            }

            if (AgreementType <= EElaEsaAgreementType.None)
            {
                errors.Add("Contract Agreement Type is required.");
            }

            return errors.ToArray();
        }
    }
}
