using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Wildfire
{
    public class WildfireActivationLicense
    {
        [JsonProperty("salesAccountId")]
        public string SalesforceAccountId { get; set; }

        [JsonProperty("supportAccountId")]
        public long? SupportAccountId { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("puchaseQuantity")]
        public string PurchaseQuantity { get; set; }

        [JsonProperty("quantity")]
        public dynamic Quantity { get; set; }

        [JsonProperty("supportLicenseType")]
        public string SupportLicenseType { get; set; }

        public WildfireApiActivationDetailsResponse Validate(WildFireApiActivationProductType productType, string serialNumber, string authCode, IWildfireDataAccess wildfireDataAccess, long userAccountId, string graphQlRequest, string payload)
        {
            Func<WildFireApiActivationProductType, bool, string, string, string> paramsToString = (t, v, s, a) =>
            {
                return (v ? "Product Type '" : string.Empty) + t + "' " + (!string.IsNullOrWhiteSpace(s)
                    ? "Serial Number '" + s + "'"
                    : !string.IsNullOrWhiteSpace(a)
                        ? "Authorisation Code '" + a + "'"
                        : string.Empty);
            };

            if (string.IsNullOrWhiteSpace(SalesforceAccountId))
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("The WF API Activation> The " + paramsToString(productType, true, serialNumber, authCode) + ", User ID '" + userAccountId + "' the License details is missing Salesforce Account ID. GraphQL request payload: '" + graphQlRequest + "'. Payload: '" + payload + "'."));
                return WildfireApiActivationDetailsResponse.SetError("The " + paramsToString(productType, false, serialNumber, authCode) + " is missing Salesforce Account ID.");
            }

            if (SupportAccountId.HasValue)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("The WF API Activation> The " + paramsToString(productType, true, serialNumber, authCode) + ", User ID '" + userAccountId + "' has been already activated in Support Account ID '" + SupportAccountId + "'. GraphQL request payload: '" + graphQlRequest + "'. Payload: '" + payload + "'."));
                return WildfireApiActivationDetailsResponse.SetError("The " + paramsToString(productType, false, serialNumber, authCode) + " has been already activated.");
            }

            IEnumerable<KeyValuePair<long, string>> cspIds = wildfireDataAccess.GetSupportAccountIdsByUserId(userAccountId, SalesforceAccountId);
            if (!cspIds.Any()) // For security reason, the user must have access to the Saleseforce Account ID of the SN/License to kick off the activation process.
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("The WF API Activation> The " + paramsToString(productType, true, serialNumber, authCode) + ", User ID " + userAccountId + " has no access to Support Account(s) '" + string.Join(", ", cspIds.Select(i => i.Key).ToArray()) + "'. GraphQL request payload: '" + graphQlRequest + "'. Payload: '" + payload + "'."));
                return WildfireApiActivationDetailsResponse.SetError("You do not have access to the Salesforce Account of the Product you are trying to activate.");
            }

            return new WildfireApiActivationDetailsResponse
            {
                Success = true,
                ProductType = productType,
                AuthCode = authCode,
                SerialNumber = serialNumber,
                License = this,
                CspIds = cspIds.Select(i => new { value = i.Key.ToString(), label = i.Value }).ToArray()
            };
        }
    }
}
