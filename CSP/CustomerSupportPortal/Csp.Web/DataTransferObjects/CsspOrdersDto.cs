using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class CsspOrdersDto : DefaultDto
    {
        public long SupportAccountId { get; set; }
        public string OrderNumber { get; set; }
        public IEnumerable<CsspOrderHistoryItems> OrderItems { get; set; }
        public IEnumerable<CsspOrderHistory> OrdersList { get; set; }
    }
}