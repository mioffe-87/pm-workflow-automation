using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface ICloudDeviceRepository
    {
        IEnumerable<CloudRegion> GetCloudRegions(int cloudTypeId);
        IEnumerable<CloudProductType> GetCloudProducts(int cloudTypeId);
        IEnumerable<CloudType> GetCloudTypes();
    }
}
