using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface ICsspDataAccess
    {
        IEnumerable<CsspProvisionedDevices> GetCsspProvisionedDevices(long supportAccountId, string serialNumber = "",
            string authorizationCode = "", string endUserAccountId = "", string endUserCompany = "",
            string contactEmail = "", DataParameters parameters = null);
    }
}
