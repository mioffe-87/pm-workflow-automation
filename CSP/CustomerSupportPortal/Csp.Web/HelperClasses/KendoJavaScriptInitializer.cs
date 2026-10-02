using Kendo.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.HelperClasses
{
    public class KendoJavaScriptInitializer : JavaScriptInitializer
    {
        public override IJavaScriptSerializer CreateSerializer()
        {
            return new KendoJavaScriptSerializer();
        }
    }
}