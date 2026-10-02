using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;
using Csp.Api.Model.Ela2;

namespace Csp.Web.DataTransferObjects
{
    public class EnterpriseAgreementsDto : DefaultDto
    {
        public EnterpriseAgreementsDto()
        {
            EnterpriseAgreements = new List<EnterpriseAgreementItem>();
        }

        public List<EnterpriseAgreementItem> EnterpriseAgreements { get; set; }
        public long SupportAccountId { get; set; }
        public string SelectedTab { get; set; }
        public bool HasTokenGrantRequest { get; set; }
        public Ela2ButtonLinks Ela2ButtonLinkAccess { get; set; }
        public string ConvertStatus { get; set; }
        public string ConvertButtonEnabled { get; set; }
        public bool CheckForConversionStatus { get; set; }
        public bool HasEla2ConvertADVURL2Roles { get; set; }
        public bool HasUrl4SiteLicense { get; set; }
        public string UserEmail { get; set; }
    }
}