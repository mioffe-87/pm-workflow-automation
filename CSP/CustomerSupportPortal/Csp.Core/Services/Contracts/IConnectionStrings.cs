using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Core.Services.Contracts
{
    public interface IConnectionStrings
    {
        string CspDatabase { get; }
        string PANDatabase { get; }
        string PANLoginDatabase { get; }
    }
}
