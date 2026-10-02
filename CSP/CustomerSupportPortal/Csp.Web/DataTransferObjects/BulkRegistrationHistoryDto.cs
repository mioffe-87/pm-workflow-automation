using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class BulkRegistrationHistoryDto : DefaultDto
    {
        public long SupportAccountId { get; set; }

        public IEnumerable<BulkRegistrationHistoryListItem> BulkRegistrations { get; set; }
    }
}