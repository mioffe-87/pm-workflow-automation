using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IMfaDataAccess
    {
        List<string> EnableMfaForAccount(long supportAccountId, long enabledByUserAccountId, bool isRequired);

        bool GetUserAccountMfaAggregate(long userAccountId, long supportAccountId);

        List<UserAccount> GetPendingMfaRegistrations();

        bool UpdateUserRegistration(bool enableMfa, MfaRegistrationModel mfaRegistrationModel);

        void LogMfaApiCall(string email, string api, string request, string response);

        bool IsMfaRequiredforUser(string userEmail);

        void LogMfa(MfaRegistrationModel mfaRegistrationModel);
    }
}
