using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Csp.Api.Model
{

    public class ActivationLinksLookupItem
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("activationLinkHash")]
        public string ActivationLinkHash { get; set; }

        [JsonIgnore]
        public string SalesOrderNumber { get; set; }

        [JsonIgnore]
        public string MasterOrderNumber { get; set; }

        [JsonProperty("authorizationCode")]
        public string AuthorizationCode { get; set; }

        [JsonProperty("salesOrder")]
        public string SalesOrder
        {
            get
            {
                return string.Format("{0}{1}{2}", SalesOrderNumber,
                    !string.IsNullOrEmpty(SalesOrderNumber) && !string.IsNullOrEmpty(MasterOrderNumber) ? " / " : string.Empty,
                    !string.IsNullOrEmpty(MasterOrderNumber) ? string.Format("[{0}]", MasterOrderNumber) : string.Empty);
            }
        }

        [JsonProperty("salesforceAccountId")]
        public string SalesforceAccountId { get; set; }

        [JsonProperty("supportAccountId")]
        public long? SupportAccountId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("details")]
        public ActivationLinksLookupItemDetails Details { get; set; }

        /// <summary>
        /// Record created on datetime.
        /// </summary>
        [JsonProperty("createdOn")]
        public string CreatedOn { get; set; }

        /// <summary>
        /// Record created by User name.
        /// </summary>
        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }
    }
}
