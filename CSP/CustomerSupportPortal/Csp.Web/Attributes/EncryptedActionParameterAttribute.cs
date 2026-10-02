using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Csp.Web.HelperClasses;

namespace Csp.Web.Attributes
{
    public class EncryptedActionParameterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.Request.QueryString.Get("q") != null)
            {
                var encryptedQueryString = HttpContext.Current.Request.QueryString.Get("q");
                var decryptedString = SecurityHelper.Decrypt(encryptedQueryString);
                var paramsArray = decryptedString.Split('&');

                var actionParams = filterContext.ActionDescriptor.GetParameters();

                foreach (var item in paramsArray)
                {
                    string[] keyValuePair = item.Split('=');

                    // Make sure the action has the parameter of the given name
                    var actionParam = actionParams.FirstOrDefault(o => o.ParameterName == keyValuePair[0]);

                    if (actionParam != null)
                    {
                        var nullType = Nullable.GetUnderlyingType(actionParam.ParameterType);

                        // If a nullable type, make sure to use changetype for that type instead; nullable types are not supported
                        if (nullType != null)
                        {
                            filterContext.ActionParameters[keyValuePair[0]] =
                                Convert.ChangeType(keyValuePair[1], nullType);
                        }
                        //Otherwise, assign and cast the value accordingly
                        else
                        {
                            filterContext.ActionParameters[keyValuePair[0]] =
                                Convert.ChangeType(keyValuePair[1], actionParam.ParameterType);
                        }
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}