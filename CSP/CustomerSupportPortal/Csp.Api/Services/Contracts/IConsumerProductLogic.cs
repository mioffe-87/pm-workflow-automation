using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.ConsumerProduct;
using Csp.Api.Model.ExtendedExpertise;

namespace Csp.Api.Services.Contracts
{
    public interface IConsumerProductLogic
    {
        ConsumerProductResponse<ConsumerProductOkyoGardeParent> GetOkyoGarde(ConsumerProductRequest request);
    }
}
