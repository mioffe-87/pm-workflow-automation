using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.ElaEsa
{
    public enum EElaEsaActionStatus
    {
        Initialized = 0,
        InProgress = 1,
        Complete = 2,
        Warning = 3,
        Error = 4,
        FatalError = 5
    }
}
