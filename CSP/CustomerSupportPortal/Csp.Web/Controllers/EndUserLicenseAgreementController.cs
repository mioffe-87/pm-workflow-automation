using System;
using System.Web.Http;
using System.Collections;
using System.IO;
using System.Web;
using System.Text;
using System.Configuration;
using opentoken;
using Csp.Web.Filters;
using Csp.Api.Services.Contracts;
using Csp.Api.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Csp.Api.HelperClasses;
using Csp.Api.Security;
using Csp.Web.HelperClasses;
using System.Net;
using Csp.Api.Model.Enums;

namespace Csp.Web.Controllers
{
    public class EndUserLicenseAgreementController : ApiController
    {
        private readonly IEulaLogic _eulaLogic;

        public EndUserLicenseAgreementController(IEulaLogic eulaLogic)
        {
            _eulaLogic = eulaLogic;
        }

        // GET: api/EULA/EulaText
        [HttpGet]
        [JwtTokenAuthentication]
        public IEnumerable<SimpleEula> EulaText()
        {
            IEnumerable<SimpleEula> returnData = null;
            try
            {
                returnData = _eulaLogic.GetEula();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return returnData;
        }


        // POST: api/EULA
        [HttpPost, HttpOptions]
        [JwtTokenAuthentication]
        public bool Post(EulaAgreements agreements)
        {
            bool returnData = false;
            try
            {
                returnData = _eulaLogic.EndUserAgreements(agreements);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return returnData;
        }
    }
}
