using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BotDetect.C5;
using Csp.Web.HelperClasses;

namespace Csp.Web.Extensions
{
    public static class UrlExtensions
    {
        public static string ActionSecure(this UrlHelper url,
            string action,
            string controller,
            object args)
        {
            var routeValues = new RouteValueDictionary(args);
            var items = new ArrayList<string>();

            foreach (var entry in routeValues)
            {
                items.Add(entry.Key + "=" + (entry.Value ?? ""));
            }

            var encryptedQueryString = SecurityHelper.Encrypt(string.Join("&", items));

            return url.Action(action, controller, new { q = encryptedQueryString });
        }
    }
}