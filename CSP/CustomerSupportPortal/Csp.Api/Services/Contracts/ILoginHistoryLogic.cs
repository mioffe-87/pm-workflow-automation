using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface ILoginHistoryLogic
    {
        void AddLoginHistory(LoginHistoryModel loginHistory);

        void UpdateLoginHistory(LoginHistoryModel loginHistory);
        LoginHistoryModel GetLoginHistory(string sessionId);

        void AddLoginUserHistory(LoginHistoryModel loginHistory);

        void UpdateLoginUserHistory(LoginHistoryModel loginHistory);

        LoginHistoryModel GetLoginUserHistory(string sessionId);
    }
}
