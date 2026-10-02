using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class ImpersonateRequest
    {
        [JsonProperty("requestorEmailAddress")]
        public string RequestorEmailAddress { get; set; }

        [JsonProperty("targetSupportAccountId")]
        public long TargetSupportAccountId { get; set; }

        [JsonProperty("targetUserAccountId")]
        public long TargetUserAccountId { get; set; }

        [JsonProperty("targetUserEmailAddress")]
        public string TargetEmailAddress { get; set; }

        [JsonProperty("approverEmailAddresses")]
        public string ApproverEmailAddresses { get; set; }

        [JsonProperty("requestorUserAccountId")]
        public long RequestorUserAccountId { get; set; }

        [JsonProperty("impersonateAction")]
        public string ImpersonateAction { get; set; }

        [JsonProperty("requestId")]
        public string ImpersonateRequestId { get; set; }

        [JsonProperty("requestedBy")]
        public string RequestedBy { get; set; }

        [JsonProperty("requestedStatus")]
        public string RequestedStatus { get; set; }

        [JsonProperty("updatedBy")]
        public long ReviewedByUserAccountId { get; set; }

        [JsonProperty("additionalApprovers")]
        public string AdditionalApprovers { get; set; }
    }

    public class ImpersonateCreateResponse
    {
        [JsonProperty("requestId")]
        public string ResultId { get; set; }
    }

    public class AccessibleAccounts
    {
        [JsonProperty("accounts")]
        public List<long> Accounts { get; set; }
    }

    public class CommonApiResponse<T> 
    {
        [JsonProperty("data")]
        public T ResponseObject { get; set; }
    }

    public class AutoApproveRequest
    {

        [JsonProperty("caseOwnerEmailAddress")]
        public string CaseOwnerEmailAddress { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("impersonateAction")]
        public string ImpersonateAction { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("salesforceCaseNumber")]
        public string SalesforceCaseNumber { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("targetUserEmailAddress")]
        public string TargetUserEmailAddress { get; set; }

        [JsonProperty("targetCspSupportAccountId")]
        public string TargetCspSupportAccountId { get; set; }

    }

    public class AutoApproveFilteredRequest
    {

        [JsonProperty("caseOwnerEmailAddress")]
        public string CaseOwnerEmailAddress { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("salesforceCaseNumber")]
        public string SalesforceCaseNumber { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
        [JsonProperty("targetUserEmailAddress")]
        public string TargetUserEmailAddress { get; set; }

        [JsonProperty("targetCspSupportAccountId")]
        public string TargetCspSupportAccountId { get; set; }

    }

    //impersonateResponseDtoList
    public class ActiveRequests
    {
        public string requestId { get; set; }
        public long targetSupportAccountId { get; set; }
        public string targetUserAccount { get; set; }
        public string requestedBy { get; set; }
        public DateTime expirationDate { get; set; }
        public int impersonateStatus { get; set; }
    }

    public class ActiveRequestList
    {
        [JsonProperty("impersonateResponseDtoList")]
        public List<ActiveRequests> activeRequests { get; set; }
    }

    public class credentials
    {
        public string userName { get; set; }
    }

    public class updateOktaUser
    {
        public credentials credentials { get; set; }
    }
}
