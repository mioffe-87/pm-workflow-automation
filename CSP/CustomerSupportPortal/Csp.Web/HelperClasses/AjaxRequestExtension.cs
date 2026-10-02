using System;
using System.Web;

namespace Csp.Web.HelperClasses {

	public static class AjaxRequestExtensions {
		/// <summary>
		/// Checks to see if the request is an Ajax request as an extension method to simplify code
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static bool IsAjaxRequest(this HttpRequest request) {

			if (request == null) {
				throw new ArgumentNullException("request");
			}

			return (request["X-Requested-With"] == "XMLHttpRequest") || ((request.Headers != null) && (request.Headers["X-Requested-With"] == "XMLHttpRequest"));
		}
	}
}
