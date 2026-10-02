using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IOAuthApiLogic
    {
        CommonApiResponse<dynamic> GetOAuthCreds(long userAccountId, long supportAccountId);

        CommonApiResponse<dynamic> CreateRegenerateOAuthCreds(OAuthApiRequest request);

        CommonApiResponse<dynamic> GetCustomerDetails(string  ClientId);
     
    }
}
