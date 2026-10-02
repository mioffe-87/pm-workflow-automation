using System;
using System.Collections.Generic;
using System.Linq;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using PAN.Common.Core.Persistence;

namespace Csp.Api.Logic
{
    public class BulkRegistrationLogic : BaseLogic, IBulkRegistrationLogic
    {
        private readonly IRepository _repository;
        private readonly IBulkRegistrationRepository _bulkRegistrationRepository;

        public BulkRegistrationLogic(IRepository repository, IBulkRegistrationRepository bulkRegistrationRepository)
        {
            _repository = repository;
            _bulkRegistrationRepository = bulkRegistrationRepository;
        }

        public bool IsValidCustomerAndDeviceCode(long supportAccountId, string customerCode, string deviceCode)
        {
            return _bulkRegistrationRepository.IsValidCustomerAndDeviceCode(supportAccountId, customerCode, deviceCode);
        }

        public List<ClaimedAssetListItem> GetAvailablePurchases(long supportAccountId, string customerCode, string deviceCode)
        {
            return _bulkRegistrationRepository.GetAvailablePurchases(supportAccountId, customerCode, deviceCode);
        }

        public List<ClaimedAssetListItem> GetClaimedPurchases(long supportAccountId)
        {
            return _bulkRegistrationRepository.GetClaimedPurchases(supportAccountId);
        }

        public List<PrepBulkRegistration.SelectableDevice> GetClaimedOrRegisteredDevicesForPrepBulkReg(long userAccountId, long supportAccountId, long? bulkRegistrationId,
            SelectDevicesFilterTypes selectDevicesFilter)
        {
            //First remove any registered/activated licenses from the claimed table
            _bulkRegistrationRepository.RemoveUsedClaims(supportAccountId);

            //Get all claimed/registered devices (based on filter)
            var results = new List<PrepBulkRegistration.SelectableDevice>();

            if (selectDevicesFilter == SelectDevicesFilterTypes.ClaimedPurchases
                || selectDevicesFilter == SelectDevicesFilterTypes.Both)
            {
                results.AddRange(_bulkRegistrationRepository.GetClaimedDevices(supportAccountId));
            }

            if (selectDevicesFilter == SelectDevicesFilterTypes.RegisteredDevices
                || selectDevicesFilter == SelectDevicesFilterTypes.Both)
            {
                var registeredDevices = _bulkRegistrationRepository.GetRegisteredDevices(supportAccountId).ToList();

                if (registeredDevices.Any())
                {
                    var registeredDevIds = registeredDevices.Select(x => x.DeviceId).ToList();
                    var activatedLics = _bulkRegistrationRepository.GetActivatedLicensesForDeviceIds(registeredDevIds);

                    foreach (var dev in registeredDevices)
                    {
                        dev.ActivatedLicenses = activatedLics.Where(lic => lic.DeviceId == dev.DeviceId).ToList();
                    }
                }

                results.AddRange(registeredDevices);
            }

            results = results.OrderBy(x => x.ModelCompatibility).ThenBy(x => x.SerialNumber).ToList();

            // if bulk reg id has value, query repository for bulk reg dev ids, update results SelectableDevice to make the device selection on the first grid
            if (bulkRegistrationId.HasValue)
            {
                var deviceIds = _bulkRegistrationRepository.GetBulkRegistrationDeviceIdsWithModelCompatibility(bulkRegistrationId.Value).Select(x => long.Parse(x.Value)).ToList();

                foreach (var dev in results)
                {
                    dev.IsSelected = deviceIds.Contains(dev.DeviceId);
                }
            }

            return results;
        }

        public List<PrepBulkRegistration.Availablelicense> GetAvailableLicensesForPrepBulkReg(long userAccountId, long supportAccountId, long? bulkRegistrationId, string modelCompatibility)
        {
            if (string.IsNullOrEmpty(modelCompatibility))
            {
                return new List<PrepBulkRegistration.Availablelicense>();
            }
            var results = new List<PrepBulkRegistration.Availablelicense>();

            var strippedModelCompatibility = StripModelCompatibility(modelCompatibility);

            results.AddRange(_bulkRegistrationRepository.GetAvailableLicensesForPrepBulkReg(supportAccountId, userAccountId, bulkRegistrationId, strippedModelCompatibility));

            return results;
        }

        /// <summary>
        /// Removes NFR and LAB designations
        /// </summary>
        /// <param name="modelCompatibility"></param>
        /// <returns></returns>
        private string StripModelCompatibility(string modelCompatibility)
        {
            return modelCompatibility.Replace("NFR-", string.Empty).Replace("LAB-", string.Empty);
        }

        /*
        public List<PrepBulkRegistration.DeviceAndLicenses> GetAppliedLicensesForPrepBulkReg(long userAccountId, long supportAccountId, long? bulkRegistrationId, long[] deviceIds)
        {
            var results = new List<PrepBulkRegistration.DeviceAndLicenses>();

            if (deviceIds == null || !deviceIds.Any())
            {
                return results;
            }

            results = _bulkRegistrationRepository.GetAppliedLicensesForPrepBulkReg(userAccountId, supportAccountId, bulkRegistrationId,
                deviceIds);

            return results;
        }*/

        public PrepBulkRegistration GetPrepBulkRegistrationDetails(long supportAccountId, long bulkRegistrationId)
        {
            return _bulkRegistrationRepository.GetPrepBulkRegistrationDetails(supportAccountId, bulkRegistrationId);
        }

        public List<BulkRegistrationHistoryListItem> GetBulkRegistrationHistory(long supportAccountId)
        {
            return _bulkRegistrationRepository.GetBulkRegistrationHistory(supportAccountId);
        }

        public void ClaimPurchases(long userAccountId, long supportAccountId, List<ClaimedAssetListItem> purchases)
        {
            _bulkRegistrationRepository.ClaimPurchases(userAccountId, supportAccountId, purchases);
        }

        public void UnclaimPurchases(long userAccountId, long supportAccountId, List<ClaimedAssetListItem> purchases)
        {
            _bulkRegistrationRepository.UnclaimPurchases(userAccountId, supportAccountId, purchases);
        }

        public List<BulkRegistrationDeviceModel> GetBulkRegistrationDevicesWithLicenses(long bulkRegistrationId, long supportAccountId)
        {
            return _bulkRegistrationRepository.GetBulkRegistrationsWithDevicesWithLicenses(bulkRegistrationId, supportAccountId);
        }

        public void UpdateBulkRegistrationDevicesAndLicenses(long bulkRegistrationId, List<BulkRegistrationDeviceModel> devices)
        {
            _bulkRegistrationRepository.UpdateBulkRegistrationDevicesAndLicenses(bulkRegistrationId, devices);
        }

        public void UpdateBulkRegistrationState(long bulkRegistrationId, BulkRegistrationStates state)
        {
            _bulkRegistrationRepository.UpdateBulkRegistrationState(bulkRegistrationId, state);
        }

        public void DiscardDraft(long supportAccountId, long userAccountId, long bulkRegistrationId)
        {
            _bulkRegistrationRepository.UpdateBulkRegistrationState(bulkRegistrationId, BulkRegistrationStates.Discarded);
        }

        public long CreateUpdateBulkRegistration(long? currentBulkRegistrationId, PrepBulkRegistration draft, long userAccountId, long supportAccountId)
        {
            // for BNDs just save the license once
            if (currentBulkRegistrationId.HasValue)
            {
                return _bulkRegistrationRepository.UpdateBulkRegistration(draft, currentBulkRegistrationId.Value, userAccountId, supportAccountId);
            }
            else
            {
                return _bulkRegistrationRepository.CreateBulkRegistration(draft, userAccountId, supportAccountId);
            }
        }

        public List<NameValue> GetBulkRegistrationDeviceIdsWithModelCompatibility(long bulkRegistrationId)
        {
            return _bulkRegistrationRepository.GetBulkRegistrationDeviceIdsWithModelCompatibility(bulkRegistrationId);
        }

        public List<BulkRegistrationModel> GetAllPendingBulkRegistrations()
        {
            return _bulkRegistrationRepository.GetAllPendingBulkRegistrations();
        }

        public void UpdateBulkRegistrationDeviceStatus(long bulkRegistrationDeviceId, bool isSuccess, string registrationNotes, string registrationErrors)
        {
            _bulkRegistrationRepository.UpdateBulkRegistrationDeviceStatus(bulkRegistrationDeviceId, isSuccess,registrationNotes, registrationErrors);
        }

        public void UpdateBulkRegistrationLicenseStatus(long bulkRegistrationLicenseId, bool isSuccess, string activationNotes, string activationErrors)
        {
            _bulkRegistrationRepository.UpdateBulkRegistrationLicenseStatus(bulkRegistrationLicenseId, isSuccess, activationNotes, activationErrors);
        }

        public void UnclaimAllPurchases(long userAccountId, long supportAccountId)
        {
            _bulkRegistrationRepository.UnclaimAllPurchases(userAccountId, supportAccountId);
        }

        public long? GetCurrentBulkRegistrationId(long supportAccountId, long userAccountId)
        {
            return _bulkRegistrationRepository.GetCurrentBulkRegistrationId(supportAccountId, userAccountId);
        }

        public bool HasBetaAccess(long supportAccountId)
        {
            return _bulkRegistrationRepository.HasBetaAccess(supportAccountId);
        }
    }
}
