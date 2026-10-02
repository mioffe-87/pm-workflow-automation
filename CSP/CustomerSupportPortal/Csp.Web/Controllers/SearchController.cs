using Csp.Web.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Csp.Web.Controllers
{
    public class SearchController : BaseController
    {
        public ActionResult Index(long ?id)
        {
            var model = new DefaultDto();

            return View(model);
        }
    }
}
