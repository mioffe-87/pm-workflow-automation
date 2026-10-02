using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Csp.Web.DataTransferObjects
{
    public class CaptchaResponse
    {
           
            public bool Success
            {
                get;
                set;
            }
            
            public List<string> ErrorMessage
            {
                get;
                set;
            }
          
    }
}