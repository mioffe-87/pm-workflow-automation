using Kendo.Mvc.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.HelperClasses
{
    public class KendoJavaScriptSerializer : IJavaScriptSerializer
    {
        public string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value); //use the serializer you want  here
        }
    }
}