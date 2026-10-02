using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Model.ConsumerProduct;
using Csp.Api.Model.Enums;
using Csp.Api.Model.ExtendedExpertise;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic
{
    public class ConsumerProductLogic : IConsumerProductLogic
    {
        private readonly IConsumerProductDataAccess _consumerProductDataAccess;

        public ConsumerProductLogic(IConsumerProductDataAccess consumerProductDataAccess)
        {
            _consumerProductDataAccess = consumerProductDataAccess;
        }

        public ConsumerProductResponse<ConsumerProductOkyoGardeParent> GetOkyoGarde(ConsumerProductRequest request)
        {
            ConsumerProductResponse<ConsumerProductOkyoGardeParent> returnData = new ConsumerProductResponse<ConsumerProductOkyoGardeParent>();
            

            returnData = _consumerProductDataAccess.GetOkyoProductData(request);

            return returnData;
        }
    }
}
