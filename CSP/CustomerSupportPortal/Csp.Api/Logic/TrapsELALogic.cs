using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Logic
{
    public class TrapsELALogic : ITrapsELALogic
    {

        private readonly ILicenseLogic _licenseLogic;
        private readonly IUserRegistrationRepository _userRegistrationRepository;
        private readonly ISupportAccountRepository _supportAccountRepository;
        private readonly ITrapsELADataAccess _trapsELADataAccess;

        public TrapsELALogic(ITrapsELADataAccess trapsELADataAccess, ILicenseLogic licenseLogic, IUserRegistrationRepository userRegistrationRepository, ISupportAccountRepository supportAccountRepository, IDeviceLogic deviceLogic, IVmRegistrationRepository vmRegistrationRepository)
        {
            _licenseLogic = licenseLogic;
            _userRegistrationRepository = userRegistrationRepository;
            _supportAccountRepository = supportAccountRepository;
            _trapsELADataAccess = trapsELADataAccess;
        }

        public bool IsTrapsELAAuthcode(string authCode)
        {
            //Get details
            var authCodeDetails = _trapsELADataAccess.GetAuthCodeDetails(authCode);

            //Is this code definitely a Traps ELA AUth Code
            return authCodeDetails.PanDbIdentification.ToUpper().StartsWith("TRAPSELA");
        }

        public static class TrapsELAAuthCodeMessages
        {
            public const string Success = "TrapsELA Auth Code registered successfully!";
            public const string EmptyAuthCode = "Auth Code cannot be empty";
            public const string AlreadyInUse = "This Auth Code is already associated with another user";
            public const string AuthCodeNotFound = "The Auth Code could not be found";
            public const string AuthCodeAlreadyAssociated = "This Auth Code is already associated with another user";
            public const string NotATrapsELAAuthCode = "The Auth Code is invalid";
        }

    }
}
