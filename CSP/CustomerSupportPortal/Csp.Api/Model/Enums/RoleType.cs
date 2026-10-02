using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum RoleType
    {
        [Description("Standard User")]
        StandardUser = 1,

        [Description("Super User")]
        SuperUser = 2,
       
        [Description("ASC User")]
        ASCUser = 3,

        [Description("TAC User")]
        TACUser = 4,

        [Description("Group Standard User")]
        GroupStandard = 5,

        [Description("TAC Super User")]
        TACSuperUser = 6,

        [Description("ASC Full Service User")]
        ASCFullServiceUser = 7,

        [Description("Impersonate User")]
        ImpersonateUser = 8,

        [Description("Bulk Registration User")]
        BulkRegistration = 9,

        [Description("Group Super User")]
        GroupSuperUser = 10,

        [Description("Limited User")]
        LimitedUser = 11,

        [Description("Group Limited User")]
        GroupLimited = 12,

        [Description("Threat Researcher")]
        ThreatResearcher = 13,

        [Description("ASC Program User")]
        ASCProgramUser = 14,

        [Description("CSSP User")]
        CSSPUser = 15,

        [Description("Group Threat Researcher")]
        GroupThreatResearcher = 16,

        [Description("AutoFocus Trial")]
        AutoFocusTrial = 17,

        [Description("Group AutoFocus Trial")]
        GroupAutoFocusTrial = 18,

        [Description("Partner Program User")]
        PartnerProgramUser = 19,

        [Description("ATC Manager")]
        ATCManager = 20,

        [Description("ATC Instructor")]
        ATCInstructor = 21,


        [Description("TACAdmin")]
        TACAdmin = 38,

        [Description("Domain Administrator")]
        DomainAdministrator = 37,

        [Description("BPA User")]
        BPAUser = 33,

        [Description("ELA Administrator")]
        ELAAdministrator = 36,

        [Description("CPSP Admin")]
        CPSPAdmin = 51
    }
}
