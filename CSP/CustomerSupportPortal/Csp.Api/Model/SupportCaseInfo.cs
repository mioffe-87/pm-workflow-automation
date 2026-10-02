using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model
{
    public class SupportCaseInfo
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string CaseNumber { get; set; }
        public string Status { get; set; }
        public string Subject { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public SupportCaseHistoryInfo CaseHistory { get; set; }
        public bool IsRma { get; set; }
    }

    public class SupportCaseHistoryInfo
    {
        public string Id { get; set; }
        public string CaseId { get; set; }
        public string NewStatus { get; set; }
        public string OldStatus { get; set; }
    }

    public class SupportCaseCommentInfo
    {
        public string Id { get; set; }
        public string CaseId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AccountTeamInfo
    {
        public string AccountId { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
