using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class ManageAscPreferencesDto : DefaultDto {
        public AscContactInfo ContactInfo { get; set; }
        public AscAccountCaseNotification CaseNotification { get; set; }
    }
}