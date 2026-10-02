using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum AccountType
    {
        [Description("CUSTOMER")]
        Customer = 1,
        [Description("ASC")]
        Asc = 2,
        [Description("PANEMPLOYEES")]
        PanEmployee = 3,
        [Description("EVAL")]
        Eval = 4,
        [Description("MARKETPLACE")]
        Marketplace = 5
    }

    public enum ConsentAction
    {
        [Description("consent-request")]
        Create,

        [Description("consent-request")]
        Update ,

        [Description("get-consent-requests-details")]
        Get , // pass id

        [Description("get-linked-accounts")]
        GetLinkedAccounts, //cspid

        [Description("my-approvals")]
        MyApprovals,

        [Description("my-requests")] 
        MyRequests ,

        [Description("update-consent-request-roles")]
        UpdateRoles,

        [Description("eligible-support-accounts")]
        SupportAccounts

    }

    public enum ConsentStatus
    {
        [Description("REQUESTED")]
        Requested = 1,

        [Description("APPROVED")]
        Approved = 2,

        [Description("REJECTED")]
        Rejected = 3,

        [Description("REVOKED")]
        Revoked = 4,

        [Description("CANCELED")]
        Cancelled = 5,

        [Description("ROLES UPDATED")]
        RolesUpdated =6
    }

    public enum ConsentErrorCode
    {
        Conflict
    }

    public enum ImpersonateStatus
    {
        Deny = 0,
        Allow = 1,
        RequireConsent =2
    }
}
