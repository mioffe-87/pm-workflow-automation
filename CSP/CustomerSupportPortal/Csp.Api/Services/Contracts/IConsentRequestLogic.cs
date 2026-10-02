using System.Collections.Generic;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IConsentRequestLogic
    {
        Task<dynamic> ConsentRequestProxy(ConsentRequest consentRequest);
        Task<List<NameValueOnly>> GetEligibleAccounts(ConsentRequest consentRequest);
    }
}
