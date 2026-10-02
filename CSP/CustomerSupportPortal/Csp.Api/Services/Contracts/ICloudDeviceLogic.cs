using Csp.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Services.Contracts
{
    public interface ICloudDeviceLogic
    {
        IEnumerable<CloudRegion> GetCloudRegions(int cloudTypeId);
        IEnumerable<CloudProductType> GetCloudProducts(int cloudTypeId);
        IEnumerable<CloudType> GetCloudTypes();
    }
}
