using Csp.Api.Services.Contracts;
using System.Web.Http;

namespace Csp.Web.Controllers
{
    public class UserAccountApiController : ApiController
    {
        private readonly IUserAccountLogic _userAccountLogic;
        private readonly ILogLogic _logLogic;
        private readonly IUserAccountApiLogic _userAccountApiLogic;
        private readonly ILicenseLogic _licenseLogic;
        private readonly ILoginServiceLogic _loginServiceLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly ILookupLogic _lookupLogic;
        private readonly IMembershipLogic _membershipLogic;
        private readonly ISupportAccountLogic _supportAccountLogic;

        public UserAccountApiController(IUserAccountLogic userAccountLogic, ILoginServiceLogic loginServiceLogic, ILogLogic logLogic, IUserAccountApiLogic userAccountApiLogic,
             ILicenseLogic licenseLogic, IDeviceLogic deviceLogic, ILookupLogic lookupLogic, IMembershipLogic membershipLogic, ISupportAccountLogic supportAccountLogic)
        {
            _userAccountLogic = userAccountLogic;
            _loginServiceLogic = loginServiceLogic;
            _logLogic = logLogic;
            _userAccountApiLogic = userAccountApiLogic;
            _licenseLogic = licenseLogic;
            _deviceLogic = deviceLogic;
            _lookupLogic = lookupLogic;
            _membershipLogic = membershipLogic;
            _supportAccountLogic = supportAccountLogic;
        }

      
        [HttpGet]
        public IHttpActionResult GetCloudServices()
        {
            var result = _userAccountApiLogic.GetAllCloudServices();
            return this.Json(result);
        }
    }
}