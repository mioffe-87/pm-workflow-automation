using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
     public interface IConsentRequestDataAccess
    {
        void AddUpdateConsentRequest(string consentRequestId,long sourceAccountId,long targetAccountId,bool isActive);
    }
}
