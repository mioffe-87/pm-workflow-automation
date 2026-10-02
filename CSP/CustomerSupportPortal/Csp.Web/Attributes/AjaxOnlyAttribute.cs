using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Csp.Web.Attributes {



	[AttributeUsage(AttributeTargets.Method)]
	public class AjaxOnlyAttribute : ActionFilterAttribute {

		#region public members

		public override void OnActionExecuting(ActionExecutingContext filterContext) {

			if (!filterContext.HttpContext.Request.IsAjaxRequest()) {

				filterContext.Result = new HttpNotFoundResult();
			}
			base.OnActionExecuting(filterContext);
		}

		#endregion
	}



}