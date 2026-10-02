using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IMarketplaceAccountLogic
    {
        bool IsAccountExists(string email);
        void CreateAccount(MarketplaceAccount account);
    }
}
