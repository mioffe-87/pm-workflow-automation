using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model.Ela2
{
    public enum EElaConversionStatus
    {
        Initialized = 0,
        InProgress = 1,
        Complete = 2,
        Error = 3,
        FatalError = 4
    }
}
