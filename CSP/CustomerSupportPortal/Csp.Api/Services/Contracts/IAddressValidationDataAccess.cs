using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IAddressValidationDataAccess
    {
        IList<Model.Csp.Api.Model.AddressAPIResponse> ValidateRegioncode(IList<Model.Csp.Api.Model.AddressAPIResponse> addressAPIResposne);
        bool ValidateCountryforAddressAPI(int countrID);
    }
}
