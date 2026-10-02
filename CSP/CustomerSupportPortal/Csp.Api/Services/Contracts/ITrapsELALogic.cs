using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface ITrapsELALogic
    {
        bool IsTrapsELAAuthcode(string authCode);
    }
}
