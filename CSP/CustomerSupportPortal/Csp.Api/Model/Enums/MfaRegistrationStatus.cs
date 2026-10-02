using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum MfaRegistrationStatus
    {
        None = 0,
        Pending = 1,
        Processing = 2,
        Success = 3,
        Failure = 4
    }
}
