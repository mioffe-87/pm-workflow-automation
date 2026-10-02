using Csp.Api.Model;
using Csp.Api.Model.Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface IAddressValidationService
    {
        IEnumerable<AddressAPIResponse> ValidateAddress(AddressAPIRequest addressAPIRequest);
        bool IsAddressValidationRequiredForCountry(int countryId);
    }
}
