using System;
using System.Linq;
using Csp.Api.DataAccess;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;

namespace Csp.Infrastructure.Data.Persistence
{
    public class SelfServiceRegistrationCodeRepository : ISelfServiceRegistrationCodeRepository
    {
        private readonly IConfigValues _configValues;

        public SelfServiceRegistrationCodeRepository(IConfigValues configValues)
        {
            _configValues = configValues;
        }

        public RegistrationCodeStatus GetCurrentRegistrationCodeStatus(long supportAccountId)
        {
            var selfRegistrationCodeDataAccess = new SelfRegistrationCodeDataAccess();

            var code = selfRegistrationCodeDataAccess.GetSelfRegistrationCode(supportAccountId);

            if (String.IsNullOrEmpty(code))
            {
                return new RegistrationCodeStatus
                {
                    RegistrationCodeIsActive = false,
                    SelfServiceRegistrationCodeUrl = string.Empty,
                };
            }

            var appUrl = _configValues.GetAppSetting(AppSettings.ApplicationUrl);

            return new RegistrationCodeStatus
            {
                RegistrationCodeIsActive = true,
                SelfServiceRegistrationCodeUrl = string.Format("{0}/UserAccount/PreRegister?code={1}", appUrl, code),
            };
        }

        public void DisableRegistrationCode(long supportAccountId)
        {
            var selfRegistrationCodeDataAccess = new SelfRegistrationCodeDataAccess();

            selfRegistrationCodeDataAccess.DisableRegistrationCode(supportAccountId);
        }

        public RegistrationCodeStatus GenerateRegistrationCode(long supportAccountId, DateTime? expirationDate, long createdByUserAccountId, string newCode)
        {
            DisableRegistrationCode(supportAccountId);

            var selfRegistrationCodeDataAccess = new SelfRegistrationCodeDataAccess();

            var codeAlreadyExists = selfRegistrationCodeDataAccess.CodeExists(newCode);
            if (codeAlreadyExists)
            {
                //This code should never be called - a duplicate GUID is *nearly* impossible to generate so this would be an exceptional circumstance
                throw new Exception("This code is already in use and cannot be re-created");
            }

            selfRegistrationCodeDataAccess.AddNewSelfRegistration(supportAccountId, newCode, expirationDate, createdByUserAccountId);
            var appUrl = _configValues.GetAppSetting(AppSettings.ApplicationUrl);

            return new RegistrationCodeStatus
            {
                RegistrationCodeIsActive = true,
                SelfServiceRegistrationCodeUrl = string.Format("{0}/UserAccount/PreRegister?code={1}", appUrl, newCode),
            };
        }
    }
}
