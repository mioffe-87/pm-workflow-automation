using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using PAN.Common.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Logic
{
    public class AssetHistoryLogic : IAssetHistoryLogic
    {
        private readonly IRepository _repository;
        private readonly IAssetHistoryRepository _assetHistoryRepository;

        public AssetHistoryLogic(IRepository repository, IAssetHistoryRepository assetHistoryRepository)
        {
            _repository = repository;
            _assetHistoryRepository = assetHistoryRepository;
        }

        public List<AssetHistoryListItem> GetAssetHistory(string code, DateTime? startDate, DateTime? endDate, long supportAccountId, long restrictedByUserAccountId, DataParameters parameters, List<RoleSecurity> roleSecurities, bool isExportReport = false)
        {
            var assetHistoryList = _assetHistoryRepository.GetAssetHistory(code, startDate, endDate, supportAccountId, restrictedByUserAccountId, parameters, isExportReport);

            AddDeviceDetails(assetHistoryList, supportAccountId, restrictedByUserAccountId, code, startDate, endDate, isExportReport);
            
            // For ASC Users, check if the user has a membership
            if (roleSecurities.IsAscUser(supportAccountId))
            {
                foreach (var asset in assetHistoryList)
                {
                    var hasMembership = roleSecurities.Any(x => x.SupportAccountId == asset.SupportAccountId);
                    
                    asset.HasMembership = (asset.SupportAccountId != supportAccountId) ? hasMembership : false;
                }
            }

            return assetHistoryList;
        }

        private void AddDeviceDetails(List<AssetHistoryListItem> assets, long supportAccountId, long restrictedByUserAccountId, string code, DateTime? startDate, DateTime? endDate, bool isExportReport)
        {
            var devices = assets.Where(x => x.AssetType == AssetTypeEnum.Device);
            var deviceIds = devices.Select(x => x.Id).ToArray();

            if (!isExportReport)
            {
                code = string.Empty;
                startDate = null;
                endDate = null;
            }
            else
            {
                deviceIds = null;
            }

            //Commenting as a quick solution to avoid override from Backup table from Pandb to show only current registration infromation
            //Commented by Manish Misra
            //var deviceRegDetails = _assetHistoryRepository.GetDeviceRegDetailsForAssetHistory(deviceIds, supportAccountId, restrictedByUserAccountId, code, startDate, endDate);
            var spareLicenseDetails = _assetHistoryRepository.GetSpareLicenseTransferDetailsForAssetHistory(deviceIds, supportAccountId, restrictedByUserAccountId, code, startDate, endDate);

            foreach (var d in devices)
            {
                //Commenting as a quick solution to avoid override from Backup table from Pandb to show only current registration infromation
                //Commented by Manish Misra
                /*var registrationDetails = deviceRegDetails.Where(x => x.SerialNumber == d.Code).OrderByDescending(x => x.LastUpdateDate).FirstOrDefault();

                if (registrationDetails != null)
                {
                    d.RegisteredByUserId = registrationDetails.RegisteredByUserId;
                    d.RegisteredByEmail = registrationDetails.RegisteredByEmail;
                    d.RegisteredByName = registrationDetails.RegisteredByName;
                }*/

                var spareLicenseTransferDetails = spareLicenseDetails.Where(x => x.SourceSerialNumber == d.Code || x.DestinationSerialNumber == d.Code).ToList();

                foreach (var detail in spareLicenseTransferDetails)
                {
                    detail.DeviceId = d.Id;
                }

                d.SpareLicenseTransferDetails = spareLicenseTransferDetails;
            }
        }
    }
}
