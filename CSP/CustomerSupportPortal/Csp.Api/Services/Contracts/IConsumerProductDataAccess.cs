using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.ConsumerProduct;
using System.Collections.Generic;
using Csp.Api.Model;
using Csp.Api.Model.ExtendedExpertise;

namespace Csp.Api.Services.Contracts
{
    public interface IConsumerProductDataAccess
    {
        ConsumerProductResponse<ConsumerProductOkyoGardeParent> GetOkyoProductData(ConsumerProductRequest request);
    }
}
