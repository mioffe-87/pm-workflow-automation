using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Core.Services.Contracts
{
    public interface IApplicationSettings
    {
        bool MiniProfilerEnabled { get; }
        string StaticResources { get; }
    }
}
