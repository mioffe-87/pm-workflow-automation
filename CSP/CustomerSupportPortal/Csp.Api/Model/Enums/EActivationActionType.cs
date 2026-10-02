using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Enums
{
    public enum EActivationActionType
    {
        Undefined = 0,
        CanActivate = 1,
        CannotActivate = 2,
        HasActivationLink = 3
    }
}
