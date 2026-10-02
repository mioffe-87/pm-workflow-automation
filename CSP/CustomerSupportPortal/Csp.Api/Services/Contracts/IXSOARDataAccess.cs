using Csp.Api.Model.Xsoar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IXSOARDataAccess
    {
        string GetSalesForceAccountId(long supportAccountId);
    }
}