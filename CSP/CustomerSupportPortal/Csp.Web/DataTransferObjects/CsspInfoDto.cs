using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class CsspInfoDto : DefaultDto
    {
        public bool HasCsspAccess { get; set; }
        public long SupportAccountId { get; set; }
        public string CartItemIds { get; set; }
        public bool IsOrderSubmitted { get; set; }
        public bool HasError { get; set; }
        public CsspInfo Cssp { get; set; }
        public IEnumerable<CsspSku> OrderItems { get; set; }
        public int? CartId { get; set; }
        public string CartNumber { get; set; }
    }
}