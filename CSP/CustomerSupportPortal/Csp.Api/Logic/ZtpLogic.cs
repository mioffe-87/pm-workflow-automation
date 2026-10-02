using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Logic
{
    public class ZtpLogic : IZtpLogic
    {
        private readonly int _tenantRetryLimit = Convert.ToInt32(ConfigurationManager.AppSettings["TenantRetryLimit"]);
        private readonly IZtpDataAccess _ztpDataAccess;
        private const string ZTP_APP = "ztp";
        public ZtpLogic(IZtpDataAccess ztpDataAccess)
        {
            _ztpDataAccess = ztpDataAccess;
        }

        public bool AddOrUpdatePanoramaAssociation(PanoramaAssociationRequest requestData)
        {
            long? tenantId = GetPanoramaTenantId(requestData.SupportAccountId);

            if (tenantId == null)
            {
                tenantId = GenerateTenantId(_tenantRetryLimit);
                return _ztpDataAccess.AddPanoramaAssociation(requestData, tenantId, ZTP_APP);
            }
            else
            {
                return _ztpDataAccess.UpdatePanoramaAssociation(requestData, tenantId, ZTP_APP);
            }
        }

        public IEnumerable<PanoramaAssociationResult> GetPanoramaAssociations(long supportAccountId)
        {
            return _ztpDataAccess.GetPanoramaAssociations(supportAccountId, ZTP_APP);
        }

        public List<NameValueOnly> GetDevicesForZtp(long supportAccountId)
        {
            return _ztpDataAccess.GetDevicesForZtp(supportAccountId);
        }

        public IEnumerable<PanoramaAssociationHistory> GetPanoramaAssociationHistory(long supportAccountId)
        {
            return _ztpDataAccess.GetPanoramaAssociationHistory(supportAccountId, ZTP_APP);
        }

        public bool IsPanormaDeviceActive(long supportAccountId, string serialNumber)
        {
            return _ztpDataAccess.IsPanormaDeviceActive(supportAccountId, serialNumber);
        }

        private long? GetPanoramaTenantId(long supId)
        {
            long? tenantId = _ztpDataAccess.GetPanoramaZtpTenant(supId, AppPortalAppType.ZtpService);
            return tenantId;
        }

        private long GenerateTenantId(int retryLimit)
        {
            var tenantId = _ztpDataAccess.GenerateTenantId(retryLimit);
            return tenantId;
        }
    }
}
