using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum ClaimType
    {
        [Description("sub")]
        Name = 0,
        [Description("userAccountId")]
        UserAccountId = 1,
        [Description("samlSessionId")]
        SessionId = 2
    }
}
