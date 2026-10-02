using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum MultiFactorAuthType
    {
        [Description("Not Enabled")]
        None = 0,
        [Description("Email")]
        Email = 1,
        [Description("Google Authenticator")]
        GoogleAuthenticator = 2
    }
}
