using System.Collections.Generic;
using Csp.Api.Services.Contracts;

namespace Csp.Web.DataTransferObjects
{
    public class AllPendingMembershipsDto : DefaultDto
    {
        public IEnumerable<PendingMembershipModel> PendingMemberships { get; set; }
    }
}