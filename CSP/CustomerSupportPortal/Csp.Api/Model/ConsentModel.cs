using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
//using Google.Type;
using Newtonsoft.Json;

namespace Csp.Api.Model
{
    public class ConsentModel
    {

    }

    /// <summary>
    /// Create Request
    /// </summary>
    public class ConsentRequest
    {
        public string Action { get; set; }

        //create
        [JsonProperty("sourceUserId")]
        public long RequestorUserAccountId { get; set; }

        [JsonProperty("sourceAccountId")]
        public long RequestorSupportAccountId { get; set; }

        [JsonProperty("targetAccountId")]
        public long DestinationSupportAccountId { get; set; }

        [JsonProperty("targetUserId")]
        public long DestinationUserAccountId { get; set; }

        [JsonProperty("requestId")]
        public string ConsentRequestId { get; set; }

        [JsonProperty("sourceUserEmail")]
        public string RequestorEmail { get; set; }

        [JsonProperty("consentStatus")]
        public int ConsentStatus { get; set; }

        [JsonProperty("userId")]
        public long UpdatedUserAccountId { set; get; }

        [JsonProperty("accountId")]
        public long UpdatedSupportAccountId { set; get; }

        [JsonProperty("targetUserEmail")]
        public string DestinationUserEmail { get; set; }

        [JsonProperty("consentList")]
        public List<string> ConsentList { get; set; }

        [JsonProperty("roles")]
        public List<int> Roles { get; set; }

        [JsonProperty("supportAccountIdList")]
        public List<long> SupportAccountIdList { get; set; }

        [JsonProperty("userEmail")]
        public string UpdatedUserEmail { set; get; }
    }

    public class ConsentRequestCreate
    {
        [JsonProperty("sourceUserAccountId")]
        public long RequestorUserAccountId { get; set; }

        [JsonProperty("sourceSupportAccountId")]
        public long RequestorSupportAccountId { get; set; }

        [JsonProperty("targetSupportAccountId")]
        public long DestinationSupportAccountId { get; set; }

        [JsonProperty("targetUserAccountId")]
        public long DestinationUserAccountId { get; set; }

    }

    public class ConsentRequestGetDetails
    {
        [JsonProperty("consentList")]
        public List<string> ConsentList { set; get; }
    }

    public class ConsentRequestUpdate
    {
        [JsonProperty("sourceUserAccountId")]
        public long RequestorUserAccountId { get; set; }

        [JsonProperty("sourceSupportAccountId")]
        public long RequestorSupportAccountId { get; set; }

        [JsonProperty("consentRequestId")]
        public string ConsentRequestId { get; set; }

        [JsonProperty("toConsentStatus")]
        public int ConsentStatus { get; set; }

        [JsonProperty("roles")]
        public List<long> Roles { get; set; }

    }

    public class ConsentRequestGetLinkedAccounts
    {

        [JsonProperty("supportAccountIdList")]
        public List<long> SupportAccountIdList { get; set; }
    }

    /// <summary>
    /// Create Response
    /// </summary>
    public class ConsentRequestResponse
    {
        [JsonProperty("data")]
        public ConsentRequestResponseModel Data { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }

    public class ConsentRequestResponseModel
    {
        [JsonProperty("consentRequestId")]
        public string consentRequestId { get; set; }

      
    }

    /// <summary>
    /// Get Consent Details Response
    /// </summary>

    public class ConsentDetailResponse
    {
        [JsonProperty("data")]
        public ConsentDetail Details { get; set; }
    }

    public class ConsentListResponse
    {
        [JsonProperty("data")]
        public ConsentDetailList ConsentRequestsList { get; set; }
    }

    public class ConsentListResponseModel
    {
        [JsonProperty("data")]
        public ConsentDetailListModel ConsentRequestsList { get; set; }
    }
    public class ConsentDetailList
    {
        [JsonProperty("requestsList")]
        public List<ConsentDetail> RequestsList { get; set; }

        [JsonProperty("requestDetails")]
        public List<ConsentDetail> RequestsDetailsList { get; set; }
    }

    public class ConsentDetailListModel
    {
        [JsonProperty("requestsList")]
        public List<ConsentDetailModel> RequestsList { get; set; }

        [JsonProperty("requestDetails")]
        public List<ConsentDetailModel> RequestsDetailsList { get; set; }
    }


    public class ConsentDetail
    {
        [JsonProperty("requestId")]
        public string ConsentId { get; set; }

        [JsonProperty("updatedBy")]
        public long? UpdatedByUserAccountId { get; set; }

        [JsonProperty("targetAccountId")]
        public long TargetAccountId { get; set; }

        [JsonProperty("targetUserId")]
        public long TargetUserAccountId { get; set; }

        [JsonProperty("sourceAccountId")]
        public long SourceAccountId { get; set; }

        [JsonProperty("sourceUserId")]
        public long SourceUserAccountId { get; set; }

        [JsonProperty("consentStatus")]
        public int Status { get; set; }

        [JsonProperty("sourceUserEmail")]
        public string SourceUserEmail { get; set; }

        [JsonProperty("targetAccountName")]
        public string TargetAccountName { get; set; }

        [JsonProperty("updatedByEmail")]
        public string UpdatedByUserEmail { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty("targetUserEmail")]
        public string TargetUserEmail { get; set; }

        [JsonProperty("sourceAccountName")]
        public string SourceAccountName { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("statusLogs")]
        public List<StatusLog> StatusLogs { get; set; }

        [JsonProperty("rolesLogs")]
        public List<RolesLog> RolesLogs { get; set; }

        [JsonProperty("historyList")]
        public List<ConsentHistory> HistoryList { get; set; }

        [JsonProperty("sourceUserName")]
        public string SourceUserName { get; set; }

        [JsonProperty("targetUserName")]
        public string TargetUserName { get; set; }

        [JsonProperty("roles")]
        public List<int> Roles { get; set; }
    }

    public class ConsentDetailModel
    {
        [JsonProperty("consentRequestId")]
        public string ConsentId { get; set; }

        [JsonProperty("updatedBy")]
        public long? UpdatedByUserAccountId { get; set; }

        [JsonProperty("targetSupportAccountId")]
        public long TargetAccountId { get; set; }

        [JsonProperty("targetUserAccountId")]
        public long TargetUserAccountId { get; set; }

        [JsonProperty("sourceSupportAccountId")]
        public long SourceAccountId { get; set; }

        [JsonProperty("sourceUserAccountId")]
        public long SourceUserAccountId { get; set; }

        [JsonProperty("consentStatus")]
        public int Status { get; set; }

        [JsonProperty("sourceUserEmail")]
        public string SourceUserEmail { get; set; }

        [JsonProperty("targetAccountName")]
        public string TargetAccountName { get; set; }

        [JsonProperty("updatedByEmail")]
        public string UpdatedByUserEmail { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty("targetUserEmail")]
        public string TargetUserEmail { get; set; }

        [JsonProperty("sourceAccountName")]
        public string SourceAccountName { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("statusLogs")]
        public List<StatusLogModel> StatusLogs { get; set; }

        [JsonProperty("rolesLogs")]
        public List<RolesLogModel> RolesLogs { get; set; }

        [JsonProperty("historyList")]
        public List<ConsentHistory> HistoryList { get; set; }

        [JsonProperty("sourceUserName")]
        public string SourceUserName { get; set; }

        [JsonProperty("targetUserName")]
        public string TargetUserName { get; set; }

        [JsonProperty("roles")]
        public List<long> Roles { get; set; }
    }


    public class EligiblesupportAccountsResposne
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("eligible")]
        public bool Eligible { get; set; }

    }

    public class StatusLog
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("updatedBy")]
        public long UpdatedByUserAccountId { get; set; }

        [JsonProperty("consentStatus")]
        public int ConsentStatus { get; set; }

        [JsonProperty("updatedDate")]
        public DateTime UpdatedDate { get; set; }
    }
    public class StatusLogModel
    {
     
        [JsonProperty("consentRequestId")]
        public string RequestId { get; set; }
        [JsonProperty("updatedBy")]
        public long UpdatedBy { get; set; }

        [JsonProperty("consentStatus")]
        public int ConsentStatus { get; set; }

        [JsonProperty("updatedDate")]
        public DateTime UpdatedDate { get; set; }
    }

    public class RolesLog
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("updatedBy")]
        public long UpdatedByUserAccountId { get; set; }

        [JsonProperty("roles")]
        public List<int> Roles { get; set; }

        [JsonProperty("updatedDate")]
        public DateTime UpdatedDate { get; set; }
    }

    public class RolesLogModel
    {
        [JsonProperty("consentRequestId")]
        public string RequestId { get; set; }

        [JsonProperty("updatedBy")]
        public long UpdatedBy { get; set; }

        [JsonProperty("roles")]
        public List<long> Roles { get; set; }

        [JsonProperty("updatedDate")]
        public DateTime UpdatedDate { get; set; }
    }

    /// <summary>
    /// Linked Accounts Response
    /// </summary>
    //public class ConsentLinkedAccount
    //{
    //    [JsonProperty("consentId")]
    //    public string ConsentId { get; set; }

    //    [JsonProperty("sourceAccountId")]
    //    public long SourceAccountId { get; set; }

    //    [JsonProperty("targetAccountId")]
    //    public long TargetAccountId { get; set; }

    //    [JsonProperty("consentStatus")]
    //    public long ConsentStatus { get; set; }

    //    [JsonProperty("targetUserId")]
    //    public long TargetUserAccountId { get; set; }

    //    [JsonProperty("sourceUserId")]
    //    public long SourceUserAccountId { get; set; }
    //}

    public class ErrorResponse
    {
        [JsonProperty("timestamp")]
        public string CreatedOn { get; set; }

        [JsonProperty("status")]
        public string ErrorCode { get; set; }

        [JsonProperty("header")]
        public string ErrorHeader { get; set; }

        [JsonProperty("message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("error")]
        public string ErrorDesc { get; set; }   

        [JsonProperty("error_message")]
        public string Message { get; set; }
    }

    public class ConsentHistory
    {
        [JsonProperty("name")]
        public string RequestorName { get; set; }

        [JsonProperty("email")]
        public string RequestorEmail { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("logDate")]
        public DateTime LogDate { get; set; }

        [JsonProperty("roles")]
        public string Roles { get; set; }
    }

    public class CBHReport
    {
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int RecentCount { get; set; }
        public List<CBHReportDetail> RecentRequests { get; set; }

    }

    public class CBHReportDetail
    {
        public string SourceAccountName { get; set; }
        public string TargetAccountName { get; set; }
        public string CreatedOn { get; set; }
    }
}
