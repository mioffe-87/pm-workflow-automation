using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Services.Contracts;
using Csp.Api.Model;

namespace Csp.Infrastructure.Data.Persistence
{
    public class CloudDeviceRepository : ICloudDeviceRepository
    {
        public IEnumerable<CloudRegion> GetCloudRegions(int cloudTypeId)
        {
            var db = SessionDbContext.CspObjectContext;
            var cloudRegions = db.cloudregions.Where(ct => ct.CloudTypeId == cloudTypeId).Select(cr => new CloudRegion()
            {
                CloudRegionId = cr.CloudRegionId,
                CloudTypeId = cloudTypeId,
                DisplayName = cr.DisplayName,
                RegionCode = cr.RegionCode
            }).ToArray();
            return cloudRegions;
        }

        public IEnumerable<CloudProductType> GetCloudProducts(int cloudTypeId)
        {
            var db = SessionDbContext.CspObjectContext;
            var cloudProducts = db.cloudproductlookups.Where(ct => ct.CloudTypeId == cloudTypeId && ct.IsActive).Select(cr => new CloudProductType()
            {
                CloudTypeId = cloudTypeId,
                DisplayName = cr.DisplayName,
                ProductCode = cr.ProductCode,
                CloudProductLookupId = cr.CloudProductLookupId,
                ProductId = cr.ProductId
            }).ToArray();
            return cloudProducts;
        }

        // Get a list of cloud types.
        public IEnumerable<CloudType> GetCloudTypes()
        {
            var db = SessionDbContext.CspObjectContext;
            var cloudTypes = db.cloudtypes.Select(ct=> new CloudType()
            {
                CloudTypeId = ct.CloudTypeId,
                Name = ct.Name
            }).ToArray();
            return cloudTypes;
        }
    }
}
