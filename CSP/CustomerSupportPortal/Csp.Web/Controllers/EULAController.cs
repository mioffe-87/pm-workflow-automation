using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Services.Contracts;
using Csp.Web.DataTransferObjects;

namespace Csp.Web.Controllers
{
    public class EULAController : BaseController
    {
        private readonly IEulaLogic _eulaLogic;

        public EULAController(IEulaLogic eulaLogic)
        {
            _eulaLogic = eulaLogic;
        }

        [HttpGet]
        public ActionResult GetEulaText(int languageId)
        {
            int eulaId;
            string eulaFileName;
            string eulaFileDownloadUrl;
            var eulaText = _eulaLogic.GetEulaText(languageId, out eulaId, out eulaFileName, out eulaFileDownloadUrl);
            var model = new EulaDto()
                {
                    Id = eulaId,
                    LanguageId = languageId,
                    Eula = eulaText,
                    EulaFileName = eulaFileName,
                    EulaDownloadUrl = eulaFileDownloadUrl
                };
            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}
