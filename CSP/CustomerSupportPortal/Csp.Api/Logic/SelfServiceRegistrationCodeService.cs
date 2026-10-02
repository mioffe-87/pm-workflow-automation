using System;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic
{
    public class SelfServiceRegistrationCodeService : ISelfServiceRegistrationCodeService
    {
        private readonly ISelfServiceRegistrationCodeRepository _selfServiceRegistrationCodeRepository;

        public SelfServiceRegistrationCodeService(ISelfServiceRegistrationCodeRepository selfServiceRegistrationCodeRepository)
        {
            _selfServiceRegistrationCodeRepository = selfServiceRegistrationCodeRepository;
        }

        public RegistrationCodeStatus GetCurrentRegistrationCodeStatus(long supportAccountId)
        {
            return _selfServiceRegistrationCodeRepository.GetCurrentRegistrationCodeStatus(supportAccountId);
        }

        public void DisableRegistrationCode(long supportAccountId)
        {
            _selfServiceRegistrationCodeRepository.DisableRegistrationCode(supportAccountId);
        }

        public RegistrationCodeStatus GenerateRegistrationCode(long supportAccountId, long createdByUserAccountId)
        {
            var newCode = Guid.NewGuid().ToString();

            return _selfServiceRegistrationCodeRepository.GenerateRegistrationCode(supportAccountId, null, createdByUserAccountId, newCode);
        }
    }
}
