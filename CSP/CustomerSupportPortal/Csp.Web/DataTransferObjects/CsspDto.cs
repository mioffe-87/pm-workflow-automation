using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class CsspDto
    {
        public long SupportAccountId { get; set; }
        public string SelectedTab { get; set; }
        public string FirstQueryStringValue { get; set; }
        public bool IsPrelicensingEnabled { get; set; }
    }
}