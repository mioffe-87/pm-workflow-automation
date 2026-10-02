using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Services.Contracts;
using Csp.Api.Model;

namespace Csp.Api.Logic
{
    public class CloudDeviceLogic : ICloudDeviceLogic
    {
        private readonly ICloudDeviceRepository _iCloudDeviceRepository;
        
        public CloudDeviceLogic(ICloudDeviceRepository cloudRepository)
        {
            _iCloudDeviceRepository = cloudRepository;
        }

        public IEnumerable<CloudRegion> GetCloudRegions(int cloudTypeId)
        {
            return _iCloudDeviceRepository.GetCloudRegions(cloudTypeId);
        }

        public IEnumerable<CloudProductType> GetCloudProducts(int cloudTypeId)
        {
            return _iCloudDeviceRepository.GetCloudProducts(cloudTypeId);
        }

        public IEnumerable<CloudType> GetCloudTypes()
        {
            return _iCloudDeviceRepository.GetCloudTypes();
        }
    }
}
