using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Model.Enums;

namespace Csp.Api.Services.Contracts
{
    public interface IMfaLogic
    {
        bool EnableMfaForAccount(long supportAccountId, long enabledByUserId, bool isRequired, string accountName, string updatedByUser);

        bool EnableMfa(UserAccount user, long enabledByUserId, MultiFactorAuthType mfaAuthType = MultiFactorAuthType.Email, string secret = null, bool unEnrollGoogleAuth = false);

        bool DisableMfa(UserAccount user, long supportAccountId, long disabledByUserAccountId, MultiFactorAuthType mfaAuthType = MultiFactorAuthType.Email);

        void ProcessPendingMfaUserRegistrations();

        bool IsMfaRequiredForAnyMembership(long supportAccountId, long userAccountId);

        bool IsMfaRequiredForAccount(long supportAccountId);

        bool IsMfaRequiredForUser(string userEmail);

        string ObfuscateSecret(string secret);

        bool IsGoogleAuthEnrolled(string userEmail);
    }
}
