using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    class Impersonate
    {
    }

    public enum ImpersonateAction
    {
        [Description("create")]
        Create,

        [Description("update")]
        Update,

        [Description("details")]
        Get,

        [Description("history")]
        History,

        [Description("activerequests")]
        ActiveRequests,

        [Description("addApprovers")]
        AddApprovers,

        [Description("validRequests")]
        ValidRequests,

        [Description("extend")]
        Extend,
        
        [Description("revoke")]
        Revoke,

        [Description("approve")]
        Approve,
    }
}
