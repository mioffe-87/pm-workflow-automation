using Csp.Api.Security;
using Csp.Web.Filters;
using Csp.Web.Security.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;

namespace Csp.Web.Controllers
{
    [Authorize]
    public class WildfireController : BaseController
    {
        [HttpGet, CspAuthorize(Permission.EditWildfireUsers)]
        public ActionResult Activate(string id)
        {
            return View("../Wildfire/Activate");
        }
    }
}
