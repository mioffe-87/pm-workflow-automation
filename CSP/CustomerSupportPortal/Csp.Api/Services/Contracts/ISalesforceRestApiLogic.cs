using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface ISalesforceRestApiLogic {
        Task<AscContactInfo> GetAscContactInfo(string accountId);
        Task<string> UpdateAscContactInfo(AscContactInfo ascContactInfo);

        Task<AscAccountCaseNotification> GetAscAccountCaseNotificationInfo(string accountId);
        Task<string> AddUpdateAscAccountCaseNotificationInfo(AscAccountCaseNotification ascAccountCaseNotificationInfo);
        Task<List<SupportCaseInfo>> GetCaseAlerts(string accountId, DateTime lastLoginDateTime);
        Task<List<SupportCaseInfo>> GetCaseRecentActivity(string accountId, string contactId, DateTime lastLoginDateTime);

        Task<DateTime> GetUserLastLoginDateTime(long userAccountId, long accountId);
        Task<bool> IsSupportOnlyAccount(string accountId);
        Task<List<AccountTeamInfo>> GetAccountTeamMembers(List<string> salesforceAccountList);
    }
}
