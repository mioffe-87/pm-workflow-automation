using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.LicenseManagement;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Csp.Api.Logic
{
    public class LicenseManagementLogic : ILicenseManagementLogic
    {        
        private readonly ILicenseManagementDataAccess licenseManagementDataAccess;
        private readonly ILicensingApiRepository licensingApiRepository;
        private readonly ISearchAllRepository searchAllRepository;

        public LicenseManagementLogic(ILicenseManagementDataAccess licenseManagementDataAccess, ILicensingApiRepository licensingApiRepository, ISearchAllRepository searchAllRepository)
        {            
            this.licenseManagementDataAccess = licenseManagementDataAccess;
            this.licensingApiRepository = licensingApiRepository;
            this.searchAllRepository = searchAllRepository;
        }

        public IEnumerable<LicenseManagementActivatedListItem> GetActivatedLicenses(long supportAccountId)
        {
            return licenseManagementDataAccess.GetActivatedLicenses(supportAccountId);
        }

        public LicenseManagementAvailableData GetAvailableLicenses(long supportAccountId)
        {
            Dictionary<string, string> configKeys = searchAllRepository.GetPanSupportWebConfigKeys();
            LicenseManagementAvailableData availableData = licenseManagementDataAccess.GetAvailableLicenses(supportAccountId);
            availableData.AvailableItems?.ToList().ForEach(i =>
            {
                string linkKey = (i?.ProductGroup ?? "Not") + "ActivationLink";
                if (!string.IsNullOrWhiteSpace(i.Hash)
                    && configKeys.ContainsKey(linkKey))
                {
                    i.ActivationLink = configKeys[linkKey] + i.Hash;
                }
            });
            return availableData;
        }
        
        public LicenseDeactivateResponse DeactivateLicenses(LicenseManagementDeactivationRequest request)
        {           
            var licenseDeactivateRequest = new List<LicenseDeactivateRequest>();
            var licenseDeactivateResponse = new LicenseDeactivateResponse();

            licenseDeactivateRequest = request.DeactivationItems.Select(x => new LicenseDeactivateRequest
            {
                AuthCode = x.AuthCode,
                SerialNumber = x.SerialNumber               
            }).ToList();
            try
            {
                var licenseEntitlementResponse = LicenseApis.DeactivateLicenses<LicenseDeactivateRequest[], List<LicenseDeactivateResponse>>(licenseDeactivateRequest.ToArray());

                if (licenseEntitlementResponse != null && licenseEntitlementResponse.Count > 0)
                {
                    licenseDeactivateResponse = licenseEntitlementResponse[0];
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return licenseDeactivateResponse;
        }
        
        public PanServiceRegenerateMagicLinkResponse GetActivationLink(List<string> authCodes, long supportAccountId)
        {
            var panServiceRegenerateMagicLinkResponse = new PanServiceRegenerateMagicLinkResponse();

            try
            {
                var licensingApi = licensingApiRepository.GetLicensingApiKey(ApiConstants.PANW_SUPPORT_ACCOUNT_ID);
                var request = new List<PanServiceRegenerateMagicLinkRequest>();
                request = authCodes.Select(x => new PanServiceRegenerateMagicLinkRequest
                {
                    AuthCode = x
                }).ToList();

                var licenseEntitlementResponse = PanServiceApis.RegenerateMagicLink<List<PanServiceRegenerateMagicLinkRequest>, List<PanServiceRegenerateMagicLinkResponse>>(request, licensingApi.LicensingApiKey);

                if (licenseEntitlementResponse != null && licenseEntitlementResponse.Count > 0)
                {
                    panServiceRegenerateMagicLinkResponse = licenseEntitlementResponse[0];
                }                             
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return panServiceRegenerateMagicLinkResponse;
        }
    }
}