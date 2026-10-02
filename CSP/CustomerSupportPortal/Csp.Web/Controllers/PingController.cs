using System.Web.Mvc;

namespace Csp.Web.Controllers
{
    [AllowAnonymous]
    public class PingController : Controller
    {
        [System.Web.Http.HttpGet]
        public JsonResult Index()
        {
            var resp = new ResponseModel { Message = "CSP application is up and running" };
            return Json(resp, JsonRequestBehavior.AllowGet);
        }
    }
    internal class ResponseModel
    {
        public string Message { get; set; }
    }
}
