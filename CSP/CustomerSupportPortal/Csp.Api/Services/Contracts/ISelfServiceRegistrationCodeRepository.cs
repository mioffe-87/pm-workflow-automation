using System;

namespace Csp.Api.Services.Contracts
{
    public interface ISelfServiceRegistrationCodeRepository
    {
        RegistrationCodeStatus GetCurrentRegistrationCodeStatus(long supportAccountId);
        void DisableRegistrationCode(long supportAccountId);
        RegistrationCodeStatus GenerateRegistrationCode(long supportAccountId, DateTime? expirationDate, long createdByUserAccountId, string newCode);
    }
}
