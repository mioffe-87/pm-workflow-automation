using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;
using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;

namespace Csp.Web.Attributes {
	public class ValidateJsonAntiForgeryTokenAttribute : AuthorizeAttribute {
		public override void OnAuthorization(AuthorizationContext filterContext) {
			var request = filterContext.HttpContext.Request;

			//  Only validate POSTs
			if (request.HttpMethod == WebRequestMethods.Http.Post) {
				//  Validate the AntiForgeryToken token
				if (request.IsAjaxRequest()) {
					var antiForgeryCookie = request.Cookies[AntiForgeryConfig.CookieName];

					var cookieValue = antiForgeryCookie != null
							? antiForgeryCookie.Value
							: null;

					AntiForgery.Validate(cookieValue, request.Headers["__RequestVerificationToken"]);
				} else {
					new ValidateAntiForgeryTokenAttribute()
							.OnAuthorization(filterContext);
				}
			}
		}
	}
}