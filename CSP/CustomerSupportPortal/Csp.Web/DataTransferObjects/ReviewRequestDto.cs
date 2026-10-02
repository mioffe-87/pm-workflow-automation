using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Logic;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class ReviewRequestDto : DefaultDto
    {
        public long EntityId { get; set; }
        public string Category { get; set; }
        public IEnumerable<ReviewDeviceTransferModel> DeviceTransferRequest { get; set; }
        public IEnumerable<ElaTokensGrantModel> VmElaTokenRequest { get; set; }
        public EulaModel Eula { get; set; }
				public string NotificationId { get; set; }
    }
}