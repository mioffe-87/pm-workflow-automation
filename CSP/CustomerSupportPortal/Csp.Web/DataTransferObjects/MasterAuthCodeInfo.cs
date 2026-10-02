using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class MasterAuthCodeInfo
    {
        public string MasterAuthCode { get; set; }
        public string SerialNumber { get; set; }
        public string ModelName { get; set; }
        public string AuthCode { get; set; }
    }
}