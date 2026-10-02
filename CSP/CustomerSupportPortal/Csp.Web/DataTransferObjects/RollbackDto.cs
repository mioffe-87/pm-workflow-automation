using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class RollbackDto : DefaultDto
    {
        public bool IsSuccess { get; set; }
        public string OriginalVm { get; set; }
    }
}