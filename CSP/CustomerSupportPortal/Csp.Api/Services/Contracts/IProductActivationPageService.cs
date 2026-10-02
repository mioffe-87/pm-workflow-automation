using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface IProductActivationPageService
    {
        GroupedData GetActivationPageData(long supportAccountId);

        dynamic GetProductActivationPageData(long supportAccountId);
    }
}
