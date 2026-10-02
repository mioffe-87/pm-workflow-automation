using System.Web;
using Csp.Web.DataTransferObjects;
using System.Web.Mvc;
using System.Web.Security;

namespace Csp.Web.Controllers
{
    public class SupportController : BaseController
    {
        public ActionResult Index(bool checkMembershipStatus = false)
        {
            var model = new DefaultDto();

            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null || checkMembershipStatus)
            {
                return View(model);
            }
            else
            {
                return RedirectToAction("MyAccounts", "SupportAccount");
            }
        }

        public ActionResult LoginHelp()
        {
            return View();
        }
    }
}
