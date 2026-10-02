using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using MySql.Data.MySqlClient;
using NHibernate.Transform;

namespace Csp.Infrastructure.Data.Persistence
{
    public class BulkRegistrationRepository : IBulkRegistrationRepository
    {
        private const string BundleLicenseType = "BND";
        private const string EvalPanDbIdentification = "EVAL";
        private const string BulkRegistrationFeature = "BULK_REGISTRATION";

        public bool IsValidCustomerAndDeviceCode(long supportAccountId, string customerCode, string deviceCode)
        {
            long customerCodeNum;
            long.TryParse(customerCode, out customerCodeNum);
            var serialNumber = deviceCode.PadLeft(10, '0');
            var sapSalesOrderNumber = customerCode.PadLeft(10, '0');
            var db = SessionDbContext.CspObjectContext;

            // Comments By LK On 05/01
            // Using Store Proc to optimize the query performance
            /*
            using (var db = new cspEntities())
            {
                var devices =
                    from dev in db.devices
                    join soitems in db.salesorderitems on dev.DeviceId equals soitems.DeviceId
                    join so in db.salesorders on soitems.SalesOrderId equals so.SalesOrderId
                    join map in db.sapsupportaccountmappings on so.SAPCustomerId equals map.SAPId
                    where
                        map.SupportAccountId == supportAccountId && dev.SerialNumber == serialNumber &&
                        (so.SAPCustomerId == customerCodeNum || so.SAPSalesOrderNumber == customerCode)
                    select dev.SerialNumber;

                var licenses =
                    from lic in db.licenses
                    join soitems in db.salesorderitems on lic.LicenseId equals soitems.LicenseId
                    join so in db.salesorders on soitems.SalesOrderId equals so.SalesOrderId
                    join map in db.sapsupportaccountmappings on so.SAPCustomerId equals map.SAPId
                    where
                        map.SupportAccountId == supportAccountId && lic.AuthorizationCode == deviceCode &&
                        (so.SAPCustomerId == customerCodeNum || so.SAPSalesOrderNumber == customerCode)
                    select lic.AuthorizationCode;

                return (devices.Any() || licenses.Any());
            }*/

            var data =
                    db.Database.SqlQuery<bool>(
                        "CALL ValidateCustomerAndDeviceCodeForBulkReg(@p_SupportAccountId, @p_SAPSalesOrderNumber, @p_SAPCustomerId, @p_DeviceCode);",
                        new MySqlParameter("p_SupportAccountId", supportAccountId),
                        new MySqlParameter("p_SAPSalesOrderNumber", sapSalesOrderNumber),
                        new MySqlParameter("p_SAPCustomerId", customerCodeNum),
                        new MySqlParameter("p_DeviceCode", deviceCode));

            return data.FirstOrDefault();
        }

        public List<ClaimedAssetListItem> GetAvailablePurchases(long supportAccountId, string customerCode, string deviceCode)
        {
            long customerCodeNum;
            long.TryParse(customerCode, out customerCodeNum);
            var serialNumber = deviceCode.PadLeft(10, '0');
            var sapSalesOrderNumber = customerCode.PadLeft(10, '0');
            var claimedState = ClaimedPurchaseStates.Claimed.ToString();
            var db = SessionDbContext.CspObjectContext;

            // Comments By LK On 05/01
            // Using Store Proc to optimize the query performance
            /*
            var claimedStateId =
                    db.claimedpurchasestates.Where(cps => cps.Name == claimedState)
                        .Select(x => x.ClaimedPurchaseStateId)
                        .Single();

            var partTypeId = db.parttypes.Where(x => x.PanDbIdentification == SearchAllConts.DevPandDbId).Select(p => p.PartTypeId).Single();

            var activeClaims = db.claimedpurchases.Where(x => x.ClaimedPurchaseStateId == claimedStateId);

            // Update  the where clause whe
            var devices =
                from soitems in db.salesorderitems
                join so in db.salesorders on soitems.SalesOrderId equals so.SalesOrderId

                join map in db.sapsupportaccountmappings on so.SAPCustomerId equals map.SAPId

                join dev in db.devices on soitems.DeviceId equals dev.DeviceId into tempDev
                from leftDev in tempDev.DefaultIfEmpty()

                join cp in activeClaims on soitems.DeviceId equals cp.DeviceId into tempCp
                from leftCp in tempCp.DefaultIfEmpty()

                join pm in db.partmasters on leftDev.PartMasterId equals pm.PartMasterId into tempPm
                from leftPm in tempPm.DefaultIfEmpty()
                let isPanorama = (leftPm.PartNumber.Contains("-PRA-") || leftPm.PartNumber.EndsWith("-PRA"))

                where
                    map.SupportAccountId == supportAccountId && 
                    ((so.SAPSalesOrderNumber == customerCode || so.SAPCustomerId == customerCodeNum) &&
                    (!isPanorama) && (leftPm.PartTypeId == partTypeId) &&
                     leftCp == null && leftDev.RegistrationDate == null)

                select new ClaimedAssetListItem()
                {
                    Id = leftDev.DeviceId,
                    AssetType = ClaimedPurchaseAssetType.Device.ToString(),
                    Code = leftDev.SerialNumber,
                    PartNumber = leftPm.PartNumber,
                    ShipDate = leftDev.ShipDate
                };

            var licenses =
                from soitems in db.salesorderitems

                join so in db.salesorders on soitems.SalesOrderId equals so.SalesOrderId

                join map in db.sapsupportaccountmappings on so.SAPCustomerId equals map.SAPId

                join lic in db.licenses on soitems.LicenseId equals lic.LicenseId into tempLic
                from leftLic in tempLic.DefaultIfEmpty()

                join cp in activeClaims on soitems.LicenseId equals cp.LicenseId into tempCp
                from leftCp in tempCp.DefaultIfEmpty()

                join pm in db.partmasters on leftLic.PartMasterId equals pm.PartMasterId into tempPm
                from leftPm in tempPm.DefaultIfEmpty()
                let isSvc = (leftPm.PartNumber.Contains("-SVC-") || leftPm.PartNumber.EndsWith("-SVC"))
                let isTp = (leftPm.PartNumber.Contains("-TP-") || leftPm.PartNumber.EndsWith("-TP"))
                let isUrl2 = (leftPm.PartNumber.Contains("-URL2-") || leftPm.PartNumber.EndsWith("-URL2"))
                let isUrl4 = (leftPm.PartNumber.Contains("-URL4-") || leftPm.PartNumber.EndsWith("-URL4"))
                let isPortal = (leftPm.PartNumber.Contains("-PORTAL-") || leftPm.PartNumber.EndsWith("-PORTAL"))
                let isGp = (leftPm.PartNumber.Contains("-GP-") || leftPm.PartNumber.EndsWith("-GP"))
                let isWf = (leftPm.PartNumber.Contains("-WF-") || leftPm.PartNumber.EndsWith("-WF"))
                let isBnd = (leftPm.PartNumber.Contains("BND") && !leftPm.PartNumber.Contains("VM") && !leftPm.PartNumber.Contains("IGNITE") && !leftPm.PartNumber.Contains("TRAPS"))

                where
                    map.SupportAccountId == supportAccountId && 
                    ((so.SAPSalesOrderNumber == customerCode || so.SAPCustomerId == customerCodeNum) &&
                    (isSvc || isTp || isUrl2 || isUrl4 || isPortal || isGp || isWf || isBnd) &&
                     leftCp == null && leftLic.RegistrationDate == null)

                select new ClaimedAssetListItem()
                {
                    Id = leftLic.LicenseId,
                    AssetType = ClaimedPurchaseAssetType.License.ToString(),
                    Code = leftLic.AuthorizationCode,
                    PartNumber = leftPm.PartNumber,
                    ShipDate = leftLic.ShipDate
                };

            return devices.Union(licenses).Distinct().ToList();
            */

            var data =
                    db.Database.SqlQuery<ClaimedAssetListItem>(
                        "CALL GetAvailablePurchasesForBulkReg(@p_SupportAccountId, @p_SAPSalesOrderNumber, @p_SAPCustomerId);",
                        new MySqlParameter("p_SupportAccountId", supportAccountId),
                        new MySqlParameter("p_SAPSalesOrderNumber", sapSalesOrderNumber),
                        new MySqlParameter("p_SAPCustomerId", customerCodeNum));

            return data.ToList();
        }

        public List<ClaimedAssetListItem> GetClaimedPurchases(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            /*
            var claimedState = ClaimedPurchaseStates.Claimed.ToString();

            var claimedPurchases =
                from cp in db.claimedpurchases

                join dev in db.devices on cp.DeviceId equals dev.DeviceId into tempDev
                from leftDev in tempDev.DefaultIfEmpty()

                join lic in db.licenses on cp.LicenseId equals lic.LicenseId into tempLic
                from leftLic in tempLic.DefaultIfEmpty()

                join pmDev in db.partmasters on leftDev.PartMasterId equals pmDev.PartMasterId into tempPmDev
                from leftPmDev in tempPmDev.DefaultIfEmpty()

                join pmLic in db.partmasters on leftLic.PartMasterId equals pmLic.PartMasterId into tempPmLic
                from leftPmLic in tempPmLic.DefaultIfEmpty()

                where 
                    cp.SupportAccountId == supportAccountId
                    && cp.claimedpurchasestate.Name == claimedState
                    && leftDev.RegistrationDate == null
                    && leftLic.RegistrationDate == null

                select new ClaimedAssetListItem()
                {
                    ClaimedPurchaseId = cp.ClaimedPurchaseId,
                    Id = (long) (cp.LicenseId ?? cp.DeviceId),
                    AssetType = cp.LicenseId.HasValue ? ClaimedPurchaseAssetType.License.ToString() : ClaimedPurchaseAssetType.Device.ToString(),
                    Code = leftLic.AuthorizationCode ?? leftDev.SerialNumber,
                    PartNumber = (leftPmLic.PartNumber != null) ? leftPmLic.PartNumber : leftPmDev.PartNumber,
                    ShipDate = (leftLic.ShipDate != null) ? leftLic.ShipDate : leftDev.ShipDate,
                };

            return claimedPurchases.Distinct().ToList();
            */

            var data =
                    db.Database.SqlQuery<ClaimedAssetListItem>(
                        "CALL GetClaimedPurchasesForBulkReg(@p_SupportAccountId);",
                        new MySqlParameter("p_SupportAccountId", supportAccountId));

            return data.ToList();
        }

        public void RemoveUsedClaims(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var claimedState = ClaimedPurchaseStates.Claimed.ToString();
            
            var usedClaims =
                from cp in db.claimedpurchases
                
                join dev in db.devices on cp.DeviceId equals dev.DeviceId into tempDev
                from leftDev in tempDev.DefaultIfEmpty()

                join lic in db.licenses on cp.LicenseId equals lic.LicenseId into tempLic
                from leftLic in tempLic.DefaultIfEmpty()

                where
                    cp.SupportAccountId == supportAccountId
                    && cp.claimedpurchasestate.Name == claimedState
   

                    && (//Dev/Lic was removed (like an RMA)
                    (
                        leftDev == null
                        && leftLic == null
                    )
                    || (
                        leftDev != null
                        && leftDev.RegistrationDate != null
                    )
                    || (
                        leftLic != null
                        && leftLic.RegistrationDate != null
                    )
                    )

                select cp;

            if (usedClaims.Any())
            {
                var unClaimedState = ClaimedPurchaseStates.Unclaimed.ToString();

                var unclaimedStateId =
                    db.claimedpurchasestates.Where(cps => cps.Name == unClaimedState)
                        .Select(x => x.ClaimedPurchaseStateId)
                        .Single();

                foreach (var c in usedClaims)
                {
                    c.ClaimedPurchaseStateId = unclaimedStateId;
                }

                db.SaveChanges();
            }
        }

        public List<BulkRegistrationHistoryListItem> GetBulkRegistrationHistory(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var devices =
                from reg in db.bulkregistrations

                join dev in db.bulkregistrationdevices on reg.BulkRegistrationId equals dev.BulkRegistrationId into tempDev
                from leftDev in tempDev.DefaultIfEmpty()

                join usr in db.useraccounts on reg.CreatedBy equals usr.UserAccountId into tempUsr
                from leftUsr in tempUsr.DefaultIfEmpty()

                where
                    reg.SupportAccountId == supportAccountId

                group leftDev
                    by new { reg.BulkRegistrationId, reg.SubmittedOn, reg.CreatedOn, leftUsr.FirstName, leftUsr.LastName } into grp

                select new BulkRegistrationHistoryListItem()
                {
                    BulkRegistrationId = grp.Key.BulkRegistrationId,
                    ActivationDate = grp.Key.SubmittedOn,
                    Username = grp.Key.FirstName + " " + grp.Key.LastName,
                    NoOfDevicesRegistered = grp.Count(),
                    NoOfLicensesApplied = 0,
                    CreatedOn = grp.Key.CreatedOn
                };

            var licenses =
                from reg in db.bulkregistrations

                join dev in db.bulkregistrationdevices on reg.BulkRegistrationId equals dev.BulkRegistrationId into tempDev
                from leftDev in tempDev.DefaultIfEmpty()

                join lic in db.bulkregistrationlicenses on leftDev.BulkRegistrationDeviceId equals lic.BulkRegistrationDeviceId into tempLic
                from leftLic in tempLic.DefaultIfEmpty()

                join usr in db.useraccounts on reg.CreatedBy equals usr.UserAccountId into tempUsr
                from leftUsr in tempUsr.DefaultIfEmpty()

                where
                    reg.SupportAccountId == supportAccountId

                group leftLic
                    by new { reg.BulkRegistrationId, reg.SubmittedOn, reg.CreatedOn, leftUsr.FirstName, leftUsr.LastName } into grp

                select new BulkRegistrationHistoryListItem()
                {
                    BulkRegistrationId = grp.Key.BulkRegistrationId,
                    ActivationDate = grp.Key.SubmittedOn,
                    Username = grp.Key.FirstName + " " + grp.Key.LastName,
                    NoOfDevicesRegistered = 0,
                    NoOfLicensesApplied = grp.Count(),
                    CreatedOn = grp.Key.CreatedOn
                };

            var historyList =
                from d in devices

                join l in licenses on d.BulkRegistrationId equals l.BulkRegistrationId

                select new BulkRegistrationHistoryListItem()
                {
                    BulkRegistrationId = d.BulkRegistrationId,
                    ActivationDate = d.ActivationDate,
                    Username = d.Username,
                    NoOfDevicesRegistered = d.NoOfDevicesRegistered,
                    NoOfLicensesApplied = l.NoOfLicensesApplied,
                    CreatedOn = d.CreatedOn.HasValue ? d.CreatedOn : l.CreatedOn
                };

            return historyList.Distinct().OrderByDescending(x => x.CreatedOn).ToList();
        }

        public void ClaimPurchases(long userAccountId, long supportAccountId, List<ClaimedAssetListItem> purchases)
        {
            var db = SessionDbContext.CspObjectContext;

            var claimedState = ClaimedPurchaseStates.Claimed.ToString();
            var claimedStateId =
                    db.claimedpurchasestates.Where(cps => cps.Name == claimedState)
                        .Select(x => x.ClaimedPurchaseStateId)
                        .Single();

            if (purchases.Any())
            {
                foreach (var item in purchases)
                {
                    var purchase = new claimedpurchas()
                    {
                       SupportAccountId = supportAccountId,
                       CreatedOn = DateTime.Now,
                       CreatedBy = userAccountId,
                       ClaimedPurchaseStateId = claimedStateId,
                       UpdatedOn = DateTime.Now,
                       UpdatedBy = userAccountId
                    };

                    if (item.AssetType == ClaimedPurchaseAssetType.License.ToString())
                    {
                        purchase.LicenseId = item.Id;
                    }
                    else
                    {
                        purchase.DeviceId = item.Id;
                    }

                    db.claimedpurchases.Add(purchase);
                }

                db.SaveChanges();
            }
        }

        public void UnclaimPurchases(long userAccountId, long supportAccountId, List<ClaimedAssetListItem> purchases)
        {
            var db = SessionDbContext.CspObjectContext;

            var unclaimedState = ClaimedPurchaseStates.Unclaimed.ToString();
            var unclaimedStateId =
                    db.claimedpurchasestates.Where(cps => cps.Name == unclaimedState)
                        .Select(x => x.ClaimedPurchaseStateId)
                        .Single();

            if (purchases.Any())
            {
                var claimedPurchaseIds = purchases.Select(p => p.ClaimedPurchaseId).ToList();
                var claimedPurchases =
                    from cp in db.claimedpurchases

                    where
                        cp.SupportAccountId == supportAccountId
                        && claimedPurchaseIds.Contains(cp.ClaimedPurchaseId)

                    select cp;

                if (claimedPurchases.Any())
                {
                    foreach (var item in claimedPurchases)
                    {
                        item.ClaimedPurchaseStateId = unclaimedStateId;
                        item.UpdatedOn = DateTime.Now;
                        item.UpdatedBy = userAccountId;
                    }

                    db.SaveChanges();
                }
            }
        }

        public List<BulkRegistrationDeviceModel> GetBulkRegistrationsWithDevicesWithLicenses(long bulkRegistrationId, long supportAccountId)
        {
            var result = new List<BulkRegistrationDeviceModel>();
            var db = SessionDbContext.CspObjectContext;

            var devices =
                (from bulkReg in db.bulkregistrations

                join regDev in db.bulkregistrationdevices on bulkReg.BulkRegistrationId equals regDev.BulkRegistrationId 

                join dev in db.devices on regDev.DeviceId equals dev.DeviceId

                join pm in db.partmasters on dev.PartMasterId equals pm.PartMasterId

                join dc in db.devicecategories on regDev.CustomValueDeviceTagId equals dc.DeviceCategoryId into tempDc
                from leftDc in tempDc.DefaultIfEmpty()

                join con in db.countries on regDev.CustomValueCountryId equals con.CountryId into tempCon
                from leftCon in tempCon.DefaultIfEmpty()

                join reg in db.regions on regDev.CustomValueRegionId equals reg.RegionId into tempReg
                from leftReg in tempReg.DefaultIfEmpty()

                where
                    bulkReg.BulkRegistrationId == bulkRegistrationId
                    && bulkReg.SupportAccountId == supportAccountId

                select new BulkRegistrationDeviceModel()
                {
                    Id = regDev.BulkRegistrationDeviceId,
                    BulkRegistrationId = regDev.BulkRegistrationId,
                    DeviceId = dev.DeviceId,
                    Model = pm.PartNumber,
                    SerialNumber = dev.SerialNumber,
                    VmSeriesLicenseId = regDev.VmSeriesLicenseId,
                    IsAlreadyRegistered = regDev.IsAlreadyRegistered,
                    CustomValueDeviceName = regDev.CustomValueDeviceName,
                    CustomValueDeviceTag = (leftDc != null) ? new NameValue() { Value = leftDc.DeviceCategoryId.ToString(), Name = leftDc.DeviceCategoryName, ValueLong = leftDc.DeviceCategoryId } : new NameValue() { Value = "0", Name = string.Empty, ValueLong = 0 },
                    CustomValueAddress1 = regDev.CustomValueAddress1,
                    CustomValueAddress2 = regDev.CustomValueAddress2,
                    CustomValueCity = regDev.CustomValueCity,
                    CustomValueCountry = (leftCon != null) ? new Country() { Id = leftCon.CountryId, Name = leftCon.Name, Code = leftCon.Code } : new Country() { Id = 0, Name = string.Empty, Code = string.Empty },
                    CustomValueRegion = (leftReg != null) ? new Region() { Id = leftReg.RegionId, Name = leftReg.Name } : new Region() { Id = 0, Name = string.Empty },
                    CustomValueOtherRegion = regDev.CustomValueOtherRegion,
                    CustomValuePostalCode = regDev.CustomValuePostalCode,
                    CustomValueRemarks = regDev.CustomValueRemarks,
                    CreatedOn = regDev.CreatedOn,
                    IsRegistrationSuccessful = regDev.IsRegistrationSuccessful,
                    RegistrationNotes = regDev.RegistrationNotes,
                    RegisteredOn = regDev.RegisteredOn,
                    ShipDate = dev.ShipDate
                }).ToList();

            var licenses =
                (from reg in db.bulkregistrations

                join regDev in db.bulkregistrationdevices on reg.BulkRegistrationId equals regDev.BulkRegistrationId

                join regLic in db.bulkregistrationlicenses on regDev.BulkRegistrationDeviceId equals regLic.BulkRegistrationDeviceId

                join lic in db.licenses on regLic.LicenseId equals lic.LicenseId

                join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId

                join soi in db.salesorderitems on lic.LicenseId equals soi.LicenseId into tempSoi
                from leftSoi in tempSoi.DefaultIfEmpty()

                join so in db.salesorders on leftSoi.SalesOrderId equals so.SalesOrderId into tempSo
                from leftSo in tempSo.DefaultIfEmpty()

                where
                    reg.BulkRegistrationId == bulkRegistrationId
                    && reg.SupportAccountId == supportAccountId

                select new BulkRegistrationLicenseModel()
                {
                    Id = regLic.BulkRegistrationLicenseId,
                    BulkRegistrationDeviceId = regLic.BulkRegistrationDeviceId,
                    LicenseId = lic.LicenseId,
                    AuthorizationCode = lic.AuthorizationCode,
                    PartMasterId = pm.PartMasterId,
                    Type = pm.PartNumber,
                    ExpectedExpirationDate = lic.ExpirationDate,
                    SalesOrderNumber = leftSo.SAPSalesOrderNumber ?? string.Empty,
                }).ToList();

            var bundleLicPartMasterIds = licenses.Where(x => x.Type.Contains(BundleLicenseType)).Select(x => x.PartMasterId).Distinct().ToList();
            var featurePartNumbers =
                (from pm in db.partmasters
                 join bnd in db.bundles on pm.PartMasterId equals bnd.FeaturePartMasterId

                 where
                     bundleLicPartMasterIds.Contains(bnd.BundlePartMasterId.Value)

                 select new PrepBulkRegistration.BundleModel()
                 {
                     PartMasterId = pm.PartMasterId,
                     PartNumber = pm.PartNumber,
                     BundlePartMasterId = bnd.BundlePartMasterId.Value
                 }).ToList();

            foreach (var lic in licenses)
            {
                var partMasterId = lic.PartMasterId;
                var bundles = (lic.Type.Contains(BundleLicenseType))
                    ? featurePartNumbers.Where(x => x.BundlePartMasterId == partMasterId).ToList()
                    : new List<PrepBulkRegistration.BundleModel>();

                lic.Bundles = bundles;
            }

            foreach (var dev in devices)
            {
                dev.BulkRegistrationLicenses = licenses.Where(lic => lic.BulkRegistrationDeviceId == dev.Id).ToList();
                result.Add(dev);
            }

            return result;
        }

        public void UpdateBulkRegistrationDevicesAndLicenses(long bulkRegistrationId, List<BulkRegistrationDeviceModel> devices)
        {
            var db = SessionDbContext.CspObjectContext;

            var mapBulkRegistrationLicenses = new Dictionary<long, long>();

            foreach (var lic in devices.Where(dev => dev != null && dev.BulkRegistrationLicenses != null).SelectMany(dev => dev.BulkRegistrationLicenses))
            {
                if (lic != null && !mapBulkRegistrationLicenses.ContainsKey(lic.Id))
                {
                    mapBulkRegistrationLicenses.Add(lic.Id, lic.BulkRegistrationDeviceId);
                }
            }

            var deviceIds = devices.Select(x => x.Id).ToList();
            var bulkRegDevices =
                db.bulkregistrationdevices.Where(dev => dev.BulkRegistrationId == bulkRegistrationId)
                    // .Where(dev => deviceIds.Contains(dev.BulkRegistrationDeviceId))
                    .Select(dev => dev)
                    .Include(dev => dev.bulkregistrationlicenses)
                    .ToList();

            foreach (var item in bulkRegDevices)
            {
                var dev = devices.SingleOrDefault(x => x.Id == item.BulkRegistrationDeviceId);

                if (dev != null)
                {
                    item.CustomValueDeviceName = dev.CustomValueDeviceName;

                    if (dev.CustomValueDeviceTag != null)
                    {
                        int deviceTagId;

                        int.TryParse(dev.CustomValueDeviceTag.Value, out deviceTagId);
                        item.CustomValueDeviceTagId = deviceTagId;
                    }

                    item.CustomValueAddress1 = dev.CustomValueAddress1;
                    item.CustomValueAddress2 = dev.CustomValueAddress2;
                    item.CustomValueCity = dev.CustomValueCity;

                    if (dev.CustomValueCountry != null && dev.CustomValueCountry.Id != 0)
                    {
                        item.CustomValueCountryId = dev.CustomValueCountry.Id;
                    }

                    if (dev.CustomValueRegion != null && dev.CustomValueRegion.Id != 0)
                    {
                        item.CustomValueRegionId = dev.CustomValueRegion.Id;
                    }
                    else
                    {
                        item.CustomValueOtherRegion = dev.CustomValueOtherRegion;
                    }

                    item.CustomValuePostalCode = dev.CustomValuePostalCode;
                    item.CustomValueRemarks = dev.CustomValueRemarks ?? string.Empty;
                    item.RegistrationNotes = dev.RegistrationNotes ?? string.Empty;

                    var licsToRemove = new List<bulkregistrationlicens>();

                    foreach (var lic in item.bulkregistrationlicenses)
                    {
                        if (!mapBulkRegistrationLicenses.ContainsKey(lic.BulkRegistrationLicenseId))
                        {
                            licsToRemove.Add(lic);
                        }
                        else
                        {
                            long bulkRegistrationDeviceId;

                            mapBulkRegistrationLicenses.TryGetValue(lic.BulkRegistrationLicenseId, out bulkRegistrationDeviceId);

                            if (bulkRegistrationDeviceId > 0 && lic.BulkRegistrationDeviceId != bulkRegistrationDeviceId)
                            {
                                lic.BulkRegistrationDeviceId = bulkRegistrationDeviceId;
                            }
                        }
                    }

                    if (licsToRemove.Any())
                    {
                        foreach (var lic in licsToRemove)
                        {
                            //item.bulkregistrationlicenses.Remove(lic);
                            db.Entry(lic).State = EntityState.Deleted;
                        }
                    }
                }
            }

            db.SaveChanges();
        }

        public void UpdateBulkRegistrationState(long bulkRegistrationId, BulkRegistrationStates state)
        {
            var db = SessionDbContext.CspObjectContext;

            var bulkRegistrationState = state.ToString();
            var bulkRegistrationStateId =
                    db.bulkregistrationstates.Where(cps => cps.Name == bulkRegistrationState)
                        .Select(x => x.BulkRegistrationStateId)
                        .Single();

            var bulkRegistration =
                db.bulkregistrations.SingleOrDefault(reg => reg.BulkRegistrationId == bulkRegistrationId);

            if (bulkRegistration != null)
            {
                bulkRegistration.BulkRegistrationStateId = bulkRegistrationStateId;

                if (state == BulkRegistrationStates.Pending)
                {
                    bulkRegistration.SubmittedOn = DateTime.Now;
                }

                db.SaveChanges();
            }
        }

        public IEnumerable<PrepBulkRegistration.Availablelicense> GetAvailableLicensesForPrepBulkReg(long supportAccountId, long userAccountId, long? bulkRegistrationId, string strippedModelCompatibility)
        {
            var db = SessionDbContext.CspObjectContext;

            var claimedStateId =
                db.claimedpurchasestates.Single(x => x.Name == ClaimedPurchaseStates.Claimed.ToString())
                    .ClaimedPurchaseStateId;

            var bulkRegDraftStateId = db.bulkregistrationstates.Single(x => x.Name == BulkRegistrationStates.Draft.ToString()).BulkRegistrationStateId;

            //var usedLicIds =
            //    from br in db.bulkregistrations
            //    join brd in db.bulkregistrationdevices on br.BulkRegistrationId equals brd.BulkRegistrationId
            //    join brl in db.bulkregistrationlicenses on brd.BulkRegistrationDeviceId equals
            //        brl.BulkRegistrationDeviceId
            //    where
            //        br.SupportAccountId == supportAccountId
            //        && br.CreatedBy == userAccountId
            //        && br.BulkRegistrationStateId == bulkRegDraftStateId
            //    select new {brl.LicenseId};

						var data = db.Database.SqlQuery<PrepBulkRegistration.Availablelicenses>(
							"CALL GetClaimedLicensesForBulkReg(@p_SupportAccountId,@p_Model);",
			        new MySqlParameter("p_SupportAccountId", supportAccountId),
							new MySqlParameter("p_Model", strippedModelCompatibility)
		        );

	        var lics = data.ToList();
	      

//						var lics =
//								(from c in db.claimedpurchases
                
//								join l in db.licenses on c.LicenseId equals l.LicenseId

//								join pm in db.partmasters on l.PartMasterId equals pm.PartMasterId

//								join brDraft in usedLicIds on c.LicenseId equals brDraft.LicenseId into tempBrDraft
//								from leftBrDraft in tempBrDraft.DefaultIfEmpty()

//								join soi in db.salesorderitems on c.LicenseId equals soi.LicenseId into tempSoi
//								from leftSoi in tempSoi.DefaultIfEmpty()

//								join so in db.salesorders on leftSoi.SalesOrderId equals so.SalesOrderId into tempSo
//								from leftSo in tempSo.DefaultIfEmpty()

//								where
//										c.SupportAccountId == supportAccountId
//// Comments By LK On 05/05 - Using EndsWith To support bundles
////                    && pm.ModelCompatibility.StartsWith(strippedModelCompatibility)
//										&& pm.ModelCompatibility.EndsWith(strippedModelCompatibility)
//										&& c.ClaimedPurchaseStateId == claimedStateId
//										&& l.RegistrationDate == null
//// Comments By LK On 04/21 - Need to display all lics as unchecking a device should return the quantities back to the available lics grid
////                    && leftBrDraft == null

//								select new
//								{
//										l.LicenseId,
//										pm.PartMasterId,
//										pm.PartNumber,
//										pm.Description,
//										l.ShipDate,
//										SalesOrderNumber = leftSo.SAPSalesOrderNumber ?? string.Empty,
//										Term = l.LicenseTermMonths > 0 ?  l.LicenseTermMonths + " Months" : "",
//								}).ToList();

            var bundleLicPartMasterIds = lics.Where(x => x.PartNumber.Contains(BundleLicenseType)).Select(x => x.PartMasterId).Distinct().ToList();

            var featurePartNumbers =
                (from pm in db.partmasters
                    join bnd in db.bundles on pm.PartMasterId equals bnd.FeaturePartMasterId

                    where
                        bundleLicPartMasterIds.Contains(bnd.BundlePartMasterId.Value)

                    select new PrepBulkRegistration.BundleModel()
                    {
                        PartMasterId = pm.PartMasterId,
                        PartNumber = pm.PartNumber,
                        BundlePartMasterId = bnd.BundlePartMasterId.Value
                    }).ToList();

            var groupedLics = new List<PrepBulkRegistration.Availablelicense>();
            var appliedLicIds = new List<long>();

            if (bulkRegistrationId.HasValue)
            {
                appliedLicIds =
                    (from bulkRegLics in db.bulkregistrationlicenses
                        join bulkRegDevs in db.bulkregistrationdevices on bulkRegLics.BulkRegistrationDeviceId equals
                            bulkRegDevs.BulkRegistrationDeviceId
                        join bulkReg in db.bulkregistrations on bulkRegDevs.BulkRegistrationId equals
                            bulkReg.BulkRegistrationId
                        where
                            bulkReg.BulkRegistrationId == bulkRegistrationId.Value

                        select bulkRegLics.LicenseId).ToList();
            }

            foreach (var l in lics)
            {
                var match =
                    groupedLics.SingleOrDefault(
                        x => x.SalesOrderNumber == l.SalesOrderNumber && x.PartNumber == l.PartNumber);
                var partMasterId = l.PartMasterId;
                var bundles = (l.PartNumber.Contains(BundleLicenseType))
                    ? featurePartNumbers.Where(x => x.BundlePartMasterId == partMasterId).ToList()
                    : new List<PrepBulkRegistration.BundleModel>();

                if (match != null)
                {
                    // If l.LicId in applied licIds continue
                    if (!appliedLicIds.Contains(l.LicenseId))
                    {
                        match.Quantity++;
                        match.LicenseIds.Add(l.LicenseId);
                    }
                }
                else
                {
                    // If l.LicId in applied licIds it is already used set qty to zero and set LicIds to empty list
                    groupedLics.Add(new PrepBulkRegistration.Availablelicense
                    {
                        PartNumber = l.PartNumber,
                        Quantity = (appliedLicIds.Contains(l.LicenseId)) ? 0 : 1,
                        SalesOrderNumber = l.SalesOrderNumber,
                        ShipDate = l.ShipDate,
                        Term = l.Term,
                        LicenseIds = (appliedLicIds.Contains(l.LicenseId)) ? new List<long>() : new List<long> { l.LicenseId },
                        Bundles = bundles
                    });
                }
            }

            return groupedLics.OrderByDescending(x => x.ShipDate).ThenBy(x => x.SalesOrderNumber).ThenBy(x => x.PartNumber).ToArray();
        }

        /*
        public List<PrepBulkRegistration.DeviceAndLicenses> GetAppliedLicensesForPrepBulkReg(long userAccountId, long supportAccountId, long? bulkRegistrationId, long[] deviceIds)
        {
            var db = SessionDbContext.CspObjectContext;

            var bulkRegDevsAndLics = new List<BulkRegistrationDeviceModel>();

            if (bulkRegistrationId.HasValue)
            {
                bulkRegDevsAndLics = GetBulkRegistrationsWithDevicesWithLicenses(bulkRegistrationId.Value,
                    supportAccountId);
            }

            var devs =
                (from d in db.devices
                join pm in db.partmasters on d.PartMasterId equals pm.PartMasterId
                where deviceIds.Contains(d.DeviceId)
                select new PrepBulkRegistration.DeviceAndLicenses
                {
                    DeviceId = d.DeviceId,
                    SerialNumber = d.SerialNumber,
                    AssetPartNumber = pm.PartNumber,
                }).ToList();

            // query the bulk reg dev and lic tables, for each dev serial number we need to get the applied licIds and lic Part numbers
            foreach (var dev in devs)
            {
                var bulkRegDev = bulkRegDevsAndLics.Single(x => x.DeviceId == dev.DeviceId);
                var bulkRegLics =
                    bulkRegDevsAndLics.Single(x => x.DeviceId == dev.DeviceId).BulkRegistrationLicenses;

                if (bulkRegDev != null)
                {
                    dev.BulkRegistrationDeviceId = bulkRegDev.Id;
                }

                foreach (var lic in bulkRegLics)
                {
                    // Check if PartNumber is of type SVC
                    if (lic.Type.Contains("-SVC-") || lic.Type.EndsWith("-SVC"))
                    {
                        dev.StandardSupportLicenseId = lic.LicenseId;
                        dev.StandardSupportAuthCode = lic.AuthorizationCode;
                        dev.StandardSupportExpiration = lic.ExpectedExpirationDate;
                        dev.StandardSupportDisplayName = lic.Type;
                    }
                    else if (lic.Type.Contains("-TP-") || lic.Type.EndsWith("-TP"))
                    {
                        dev.ThreatProtectionLicenseId = lic.LicenseId;
                        dev.ThreatProtectionAuthCode = lic.AuthorizationCode;
                        dev.ThreatProtectionExpiration = lic.ExpectedExpirationDate;
                        dev.ThreatProtectionDisplayName = lic.Type;
                    }
                    else if (lic.Type.Contains("-URL2-") || lic.Type.EndsWith("-URL2"))
                    {
                        dev.UrlLicenseId = lic.LicenseId;
                        dev.UrlAuthCode = lic.AuthorizationCode;
                        dev.UrlExpiration = lic.ExpectedExpirationDate;
                        dev.UrlDisplayName = lic.Type;
                    }
                    else if (lic.Type.Contains("-PORTAL-") || lic.Type.EndsWith("-PORTAL"))
                    {
                        dev.GpPortalLicenseId = lic.LicenseId;
                        dev.GpPortalAuthCode = lic.AuthorizationCode;
                        dev.GpPortalExpiration = lic.ExpectedExpirationDate;
                        dev.GpPortalDisplayName = lic.Type;
                    }
                    else if (lic.Type.Contains("-GP-") || lic.Type.EndsWith("-GP"))
                    {
                        dev.GpGatewayLicenseId = lic.LicenseId;
                        dev.GpGatewayAuthCode = lic.AuthorizationCode;
                        dev.GpGatewayExpiration = lic.ExpectedExpirationDate;
                        dev.GpGatewayDisplayName = lic.Type;
                    }
                    else if (lic.Type.Contains("-WF-") || lic.Type.EndsWith("-WF"))
                    {
                        dev.WildfireLicenseId = lic.LicenseId;
                        dev.WildfireAuthCode = lic.AuthorizationCode;
                        dev.WildfireExpiration = lic.ExpectedExpirationDate;
                        dev.WildfireDisplayName = lic.Type;
                    }
                    // Duplicate logic for BND type
                    //... Duplicate the logic for all other 5 types
                    //For BND type loop thru all the applicable lic types and set the std lic Id and part number cols for each of lic 
                }
            }

            return devs;
        }*/

        public PrepBulkRegistration GetPrepBulkRegistrationDetails(long supportAccountId, long bulkRegistrationId)
        {
            var result = new PrepBulkRegistration()
            {
                DevsAndLics = new List<PrepBulkRegistration.DeviceAndLicenses>()
            };

            var db = SessionDbContext.CspObjectContext;

            var bulkReg =
                (from b in db.bulkregistrations
                join ft in db.bulkregistrationfiltertypes on b.BulkRegistrationFilterTypeId equals ft.BulkRegistrationFilterTypeId
                where b.BulkRegistrationId == bulkRegistrationId
                select new 
                {
                    ft.Name,
                    b.DefaultDeviceName,
                    b.DefaultDeviceTagId,
                    b.DefaultAddress1,
                    b.DefaultAddress2,
                    b.DefaultCity,
                    b.DefaultCountryId,
                    b.DefaultRegionId,
                    b.DefaultOtherRegion,
                    b.DefaultPostalCode,
                    b.DefaultRemarks
                }).SingleOrDefault();

            if (bulkReg != null)
            {
                result.DefaultDeviceName = bulkReg.DefaultDeviceName;
                result.DefaultDeviceCategoryId = bulkReg.DefaultDeviceTagId;
                result.DefaultDeviceAddress1 = bulkReg.DefaultAddress1;
                result.DefaultDeviceAddress2 = bulkReg.DefaultAddress2;
                result.DefaultDeviceCity = bulkReg.DefaultCity;
                result.DefaultDeviceCountryId = bulkReg.DefaultCountryId;
                result.DefaultDeviceRegionId = bulkReg.DefaultRegionId;
                result.DefaultDeviceOtherRegion = bulkReg.DefaultOtherRegion;
                result.DefaultDevicePostalCode = bulkReg.DefaultPostalCode;
                result.DefaultDeviceComments = bulkReg.DefaultRemarks;

                if (bulkReg.Name == SelectDevicesFilterTypes.ClaimedPurchases.ToString())
                {
                    result.SelectDevicesFilter = SelectDevicesFilterTypes.ClaimedPurchases;
                }
                else if (bulkReg.Name == SelectDevicesFilterTypes.RegisteredDevices.ToString())
                {
                    result.SelectDevicesFilter = SelectDevicesFilterTypes.RegisteredDevices;
                }
                else
                {
                    result.SelectDevicesFilter = SelectDevicesFilterTypes.Both;
                }
            }

            var bulkRegDevsAndLics = GetBulkRegistrationsWithDevicesWithLicenses(bulkRegistrationId,
                    supportAccountId);

            var lics = bulkRegDevsAndLics.SelectMany(x => x.BulkRegistrationLicenses).ToList();

            var bundleLicPartMasterIds = lics.Where(x => x.Type.Contains(BundleLicenseType)).Select(x => x.PartMasterId).Distinct().ToList();
            var featurePartNumbers =
                (from pm in db.partmasters
                 join bnd in db.bundles on pm.PartMasterId equals bnd.FeaturePartMasterId

                 where
                     bundleLicPartMasterIds.Contains(bnd.BundlePartMasterId.Value)

                 select new PrepBulkRegistration.BundleModel()
                 {
                     PartMasterId = pm.PartMasterId,
                     PartNumber = pm.PartNumber,
                     BundlePartMasterId = bnd.BundlePartMasterId.Value
                 }).ToList();

            var devs = bulkRegDevsAndLics.Select(dev => new PrepBulkRegistration.DeviceAndLicenses()
            {
                BulkRegistrationDeviceId = dev.Id,
                DeviceId = dev.DeviceId,
                SerialNumber = dev.SerialNumber,
                AssetPartNumber = dev.Model
            }).ToList();

            // query the bulk reg dev and lic tables, for each dev serial number we need to get the applied licIds and lic Part numbers
            foreach (var dev in devs)
            {
                var bulkRegDev = bulkRegDevsAndLics.Single(x => x.DeviceId == dev.DeviceId);
                var bulkRegLics =
                    bulkRegDevsAndLics.Single(x => x.DeviceId == dev.DeviceId).BulkRegistrationLicenses;

                if (bulkRegDev != null)
                {
                    dev.BulkRegistrationDeviceId = bulkRegDev.Id;
                }

                foreach (var lic in bulkRegLics)
                {
                    string licType = lic.Type;
                    // Check if PartNumber is of type SVC
                    if (licType.Contains("-SVC-") || licType.EndsWith("-SVC"))
                    {
                        dev.StandardSupportLicenseId = lic.LicenseId;
                        dev.StandardSupportAuthCode = lic.AuthorizationCode;
                        dev.StandardSupportExpiration = lic.ExpectedExpirationDate;
                        dev.StandardSupportDisplayName = licType;
                    }
                    else if (licType.Contains("-TP-") || licType.EndsWith("-TP") || licType.Contains("-ATP-") || licType.EndsWith("-ATP"))
                    {
                        dev.ThreatProtectionLicenseId = lic.LicenseId;
                        dev.ThreatProtectionAuthCode = lic.AuthorizationCode;
                        dev.ThreatProtectionExpiration = lic.ExpectedExpirationDate;
                        dev.ThreatProtectionDisplayName = licType;
                    }
                    else if (licType.Contains("-URL2-") || licType.EndsWith("-URL2") || licType.Contains("-URL4-") || licType.EndsWith("-URL4") 
                        || licType.Contains("-ADVURL-") || licType.EndsWith("-ADVURL"))
                    {
                        dev.UrlLicenseId = lic.LicenseId;
                        dev.UrlAuthCode = lic.AuthorizationCode;
                        dev.UrlExpiration = lic.ExpectedExpirationDate;
                        dev.UrlDisplayName = licType;
                    }
                    else if (licType.Contains("-PORTAL-") || licType.EndsWith("-PORTAL"))
                    {
                        dev.GpPortalLicenseId = lic.LicenseId;
                        dev.GpPortalAuthCode = lic.AuthorizationCode;
                        dev.GpPortalExpiration = lic.ExpectedExpirationDate;
                        dev.GpPortalDisplayName = licType;
                    }
                    else if (licType.Contains("-GP-") || licType.EndsWith("-GP"))
                    {
                        dev.GpGatewayLicenseId = lic.LicenseId;
                        dev.GpGatewayAuthCode = lic.AuthorizationCode;
                        dev.GpGatewayExpiration = lic.ExpectedExpirationDate;
                        dev.GpGatewayDisplayName = licType;
                    }
                    else if (licType.Contains("-WF-") || licType.EndsWith("-WF") || licType.Contains("-AWF-") || licType.EndsWith("-AWF"))
                    {
                        dev.WildfireLicenseId = lic.LicenseId;
                        dev.WildfireAuthCode = lic.AuthorizationCode;
                        dev.WildfireExpiration = lic.ExpectedExpirationDate;
                        dev.WildfireDisplayName = licType;
                    }
                    else if (licType.Contains("-DNS-") || licType.EndsWith("-DNS") || licType.Contains("-ADNS-") || licType.EndsWith("-ADNS"))
                    {
                        dev.DnsLicenseId = lic.LicenseId;
                        dev.DnsAuthCode = lic.AuthorizationCode;
                        dev.DnsExpiration = lic.ExpectedExpirationDate;
                        dev.DnsDisplayName = licType;
                    }
                    else if (licType.Contains("-SDWAN-") || licType.EndsWith("-SDWAN"))
                    {
                        dev.SdwanLicenseId = lic.LicenseId;
                        dev.SdwanAuthCode = lic.AuthorizationCode;
                        dev.SdwanExpiration = lic.ExpectedExpirationDate;
                        dev.SdwanDisplayName = licType;
                    }
                    else if (licType.Contains("-PAA-") || licType.EndsWith("-PAA"))
                    {
                        dev.PaaLicenseId = lic.LicenseId;
                        dev.PaaAuthCode = lic.AuthorizationCode;
                        dev.PaaExpiration = lic.ExpectedExpirationDate;
                        dev.PaaDisplayName = licType;
                    }
                    else if (!licType.Contains("-SVC-") && System.Text.RegularExpressions.Regex.IsMatch(licType, @"-M(\d+)?-P-\d+", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        dev.PanoramaManagementLicenseId = lic.LicenseId;
                        dev.PanoramaManagementAuthCode = lic.AuthorizationCode;
                        dev.PanoramaManagementExpiration = lic.ExpectedExpirationDate;
                        dev.PanoramaManagementDisplayName = licType;
                    }
                    else if (licType.Contains(BundleLicenseType))
                    {
                        // Duplicate logic for BND type
                        //... Duplicate the logic for all other 5 types
                        //For BND type loop thru all the applicable lic types and set the std lic Id and part number cols for each of lic 
                        var bundles = (licType.Contains(BundleLicenseType))
                            ? featurePartNumbers.Where(x => x.BundlePartMasterId == lic.PartMasterId).ToList()
                            : new List<PrepBulkRegistration.BundleModel>();

                        dev.BundleLicenseId = lic.LicenseId;
                        dev.BundleAuthCode = lic.AuthorizationCode;
                        dev.BundleExpiration = lic.ExpectedExpirationDate;
                        dev.BundleDisplayName = licType;

                        foreach (var featureLic in bundles)
                        {
                            string featureType = featureLic.PartNumber;
                            if (featureType.Contains("-SVC-") || featureType.EndsWith("-SVC"))
                            {
                                dev.StandardSupportLicenseId = lic.LicenseId;
                                dev.StandardSupportAuthCode = lic.AuthorizationCode;
                                dev.StandardSupportExpiration = lic.ExpectedExpirationDate;
                                dev.StandardSupportDisplayName = licType;
                            }
                            else if (featureType.Contains("-TP-") || featureType.EndsWith("-TP") || featureType.Contains("-ATP-") || featureType.EndsWith("-ATP"))
                            {
                                dev.ThreatProtectionLicenseId = lic.LicenseId;
                                dev.ThreatProtectionAuthCode = lic.AuthorizationCode;
                                dev.ThreatProtectionExpiration = lic.ExpectedExpirationDate;
                                dev.ThreatProtectionDisplayName = licType;
                            }
                            else if (featureType.Contains("-URL2-") || featureType.EndsWith("-URL2") || featureType.Contains("-URL4-") || featureType.EndsWith("-URL4") 
                                || featureType.Contains("-ADVURL-") || featureType.EndsWith("-ADVURL"))
                            {
                                dev.UrlLicenseId = lic.LicenseId;
                                dev.UrlAuthCode = lic.AuthorizationCode;
                                dev.UrlExpiration = lic.ExpectedExpirationDate;
                                dev.UrlDisplayName = licType;
                            }
                            else if (featureType.Contains("-PORTAL-") || featureType.EndsWith("-PORTAL"))
                            {
                                dev.GpPortalLicenseId = lic.LicenseId;
                                dev.GpPortalAuthCode = lic.AuthorizationCode;
                                dev.GpPortalExpiration = lic.ExpectedExpirationDate;
                                dev.GpPortalDisplayName = licType;
                            }
                            else if (featureType.Contains("-GP-") || featureType.EndsWith("-GP"))
                            {
                                dev.GpGatewayLicenseId = lic.LicenseId;
                                dev.GpGatewayAuthCode = lic.AuthorizationCode;
                                dev.GpGatewayExpiration = lic.ExpectedExpirationDate;
                                dev.GpGatewayDisplayName = licType;
                            }
                            else if (featureType.Contains("-WF-") || featureType.EndsWith("-WF") || featureType.Contains("-AWF-") || featureType.EndsWith("-AWF"))
                            {
                                dev.WildfireLicenseId = lic.LicenseId;
                                dev.WildfireAuthCode = lic.AuthorizationCode;
                                dev.WildfireExpiration = lic.ExpectedExpirationDate;
                                dev.WildfireDisplayName = licType;
                            }
                            else if (featureType.Contains("-DNS-") || featureType.EndsWith("-DNS") || licType.Contains("-ADNS-") || licType.EndsWith("-ADNS"))
                            {
                                dev.DnsLicenseId = lic.LicenseId;
                                dev.DnsAuthCode = lic.AuthorizationCode;
                                dev.DnsExpiration = lic.ExpectedExpirationDate;
                                dev.DnsDisplayName = licType;
                            }
                            else if (featureType.Contains("-SDWAN-") || featureType.EndsWith("-SDWAN"))
                            {
                                dev.SdwanLicenseId = lic.LicenseId;
                                dev.SdwanAuthCode = lic.AuthorizationCode;
                                dev.SdwanExpiration = lic.ExpectedExpirationDate;
                                dev.SdwanDisplayName = licType;
                            }
                            else if (featureType.Contains("-PAA-") || featureType.EndsWith("-PAA"))
                            {
                                dev.PaaLicenseId = lic.LicenseId;
                                dev.PaaAuthCode = lic.AuthorizationCode;
                                dev.PaaExpiration = lic.ExpectedExpirationDate;
                                dev.PaaDisplayName = licType;
                            }
                            else if (!featureType.Contains("-SVC-")&& System.Text.RegularExpressions.Regex.IsMatch(featureType, @"-M(\d+)?-P-\d+", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            {
                                dev.PanoramaManagementLicenseId = lic.LicenseId;
                                dev.PanoramaManagementAuthCode = lic.AuthorizationCode;
                                dev.PanoramaManagementExpiration = lic.ExpectedExpirationDate;
                                dev.PanoramaManagementDisplayName = licType;
                            }
                        }
                    }
                }
            }

            if (result.SelectDevicesFilter == SelectDevicesFilterTypes.RegisteredDevices)
            {
                if (devs.Any())
                {
                    var registeredDevIds = devs.Select(x => x.DeviceId).ToList();
                    var activatedLics = GetActivatedLicensesForDeviceIds(registeredDevIds);

                    foreach (var dev in devs)
                    {
                        dev.ActivatedLicenses = activatedLics.Where(lic => lic.DeviceId == dev.DeviceId).ToList();
                    }
                }
            }

            result.DevsAndLics = devs;

            return result;
        }

        public long CreateBulkRegistration(PrepBulkRegistration draft, long userAccountId, long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var filterTypeId =
                db.bulkregistrationfiltertypes.Single(x => x.Name == draft.SelectDevicesFilter.ToString())
                    .BulkRegistrationFilterTypeId;

            var draftStateName = BulkRegistrationStates.Draft.ToString();
            var draftStateId = db.bulkregistrationstates.Single(x => x.Name == draftStateName).BulkRegistrationStateId;

            var model = new bulkregistration
            {
                SupportAccountId = supportAccountId,
                BulkRegistrationFilterTypeId = filterTypeId,
                DefaultDeviceName = draft.DefaultDeviceName,
                DefaultDeviceTagId = draft.DefaultDeviceCategoryId,
                DefaultAddress1 = draft.DefaultDeviceAddress1,
                DefaultAddress2 = draft.DefaultDeviceAddress2,
                DefaultCity = draft.DefaultDeviceCity,
                DefaultCountryId = draft.DefaultDeviceCountryId,
                DefaultRegionId = draft.DefaultDeviceRegionId,
                DefaultOtherRegion = draft.DefaultDeviceOtherRegion,
                DefaultPostalCode = draft.DefaultDevicePostalCode,
                DefaultRemarks = draft.DefaultDeviceComments,
                BulkRegistrationStateId = draftStateId,
                CreatedBy = userAccountId,
                CreatedOn = DateTime.UtcNow,
            };

            db.bulkregistrations.Add(model);

            db.SaveChanges();

            foreach (var devAndLics in draft.DevsAndLics)
            {
                var devDetails = db.devices.Single(x => x.SerialNumber == devAndLics.SerialNumber);

                var devModel = new bulkregistrationdevice
                {
                    BulkRegistrationId = model.BulkRegistrationId,
                    DeviceId = devDetails.DeviceId,
                    SerialNumber = devAndLics.SerialNumber,
                    IsAlreadyRegistered = devDetails.RegistrationDate.HasValue,
                    CustomValueDeviceName = draft.DefaultDeviceName,
                    CustomValueDeviceTagId = draft.DefaultDeviceCategoryId,
                    CustomValueAddress1 = draft.DefaultDeviceAddress1,
                    CustomValueAddress2 = draft.DefaultDeviceAddress2,
                    CustomValueCity = draft.DefaultDeviceCity,
                    CustomValueCountryId = draft.DefaultDeviceCountryId,
                    CustomValueRegionId = draft.DefaultDeviceRegionId,
                    CustomValueOtherRegion = draft.DefaultDeviceOtherRegion,
                    CustomValuePostalCode = draft.DefaultDevicePostalCode,
                    CustomValueRemarks = draft.DefaultDeviceComments,
                    CreatedOn = DateTime.UtcNow,
                    IsRegistrationSuccessful = false,
                    RegistrationNotes = string.Empty,
                };

                db.bulkregistrationdevices.Add(devModel);
                db.SaveChanges();

                if (!IsBundleLicense(devAndLics.StandardSupportDisplayName))
                {
                    AddLicense(db, devAndLics.StandardSupportLicenseId, devModel.BulkRegistrationDeviceId);
                }

                if (!IsBundleLicense(devAndLics.ThreatProtectionDisplayName))
                {
                    AddLicense(db, devAndLics.ThreatProtectionLicenseId, devModel.BulkRegistrationDeviceId);
                }

                if (!IsBundleLicense(devAndLics.GpGatewayDisplayName))
                {
                    AddLicense(db, devAndLics.GpGatewayLicenseId, devModel.BulkRegistrationDeviceId);
                }

                if (!IsBundleLicense(devAndLics.GpPortalDisplayName))
                {
                    AddLicense(db, devAndLics.GpPortalLicenseId, devModel.BulkRegistrationDeviceId);
                }

                if (!IsBundleLicense(devAndLics.UrlDisplayName))
                {
                    AddLicense(db, devAndLics.UrlLicenseId, devModel.BulkRegistrationDeviceId);
                }

                if (!IsBundleLicense(devAndLics.WildfireDisplayName))
                {
                    AddLicense(db, devAndLics.WildfireLicenseId, devModel.BulkRegistrationDeviceId);
                }

                if (!IsBundleLicense(devAndLics.DnsDisplayName))
                {
                    AddLicense(db, devAndLics.DnsLicenseId, devModel.BulkRegistrationDeviceId);
                }

                if (!IsBundleLicense(devAndLics.SdwanDisplayName))
                {
                    AddLicense(db, devAndLics.SdwanLicenseId, devModel.BulkRegistrationDeviceId);
                }

                if (!IsBundleLicense(devAndLics.PaaDisplayName))
                {
                    AddLicense(db, devAndLics.PaaLicenseId, devModel.BulkRegistrationDeviceId);
                }

                if (!IsBundleLicense(devAndLics.PanoramaManagementDisplayName))
                {
                    AddLicense(db, devAndLics.PanoramaManagementLicenseId, devModel.BulkRegistrationDeviceId);
                }

                AddLicense(db, devAndLics.BundleLicenseId, devModel.BulkRegistrationDeviceId);

                db.SaveChanges();
            }

            return model.BulkRegistrationId;
        }

        public List<NameValue> GetBulkRegistrationDeviceIdsWithModelCompatibility(long bulkRegistrationId)
        {
            var db = SessionDbContext.CspObjectContext;

            var deviceIds =
                (from brdev in db.bulkregistrationdevices
                    join dev in db.devices on brdev.DeviceId equals dev.DeviceId
                    join pm in db.partmasters on dev.PartMasterId equals pm.PartMasterId
                    where
                        brdev.BulkRegistrationId == bulkRegistrationId

                    select new NameValue()
                    {
                        Value = brdev.DeviceId.ToString(),
                        Name = pm.ModelCompatibility
                    }).ToList();

            return deviceIds;
        }

        public long UpdateBulkRegistration(PrepBulkRegistration draft, long bulkRegistrationId, long userAccountId, long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var filterTypeId =
                db.bulkregistrationfiltertypes.Single(x => x.Name == draft.SelectDevicesFilter.ToString())
                    .BulkRegistrationFilterTypeId;

            var draftStateName = BulkRegistrationStates.Draft.ToString();
            var draftStateId = db.bulkregistrationstates.Single(x => x.Name == draftStateName).BulkRegistrationStateId;

            var bulkReg = db.bulkregistrations.Single(x => x.BulkRegistrationId == bulkRegistrationId);

            if (bulkReg != null)
            {
                bulkReg.BulkRegistrationFilterTypeId = filterTypeId;

                if (draft.IsOverrideDefaults)
                {
                    bulkReg.DefaultDeviceName = draft.DefaultDeviceName;
                    bulkReg.DefaultDeviceTagId = draft.DefaultDeviceCategoryId;
                    bulkReg.DefaultAddress1 = draft.DefaultDeviceAddress1;
                    bulkReg.DefaultAddress2 = draft.DefaultDeviceAddress2;
                    bulkReg.DefaultCity = draft.DefaultDeviceCity;
                    bulkReg.DefaultCountryId = draft.DefaultDeviceCountryId;
                    bulkReg.DefaultRegionId = draft.DefaultDeviceRegionId;
                    bulkReg.DefaultOtherRegion = draft.DefaultDeviceOtherRegion;
                    bulkReg.DefaultPostalCode = draft.DefaultDevicePostalCode;
                    bulkReg.DefaultRemarks = draft.DefaultDeviceComments;
                }

                db.SaveChanges();

                // Get Devs and Lics for Update/Delete
                var bulkRegDeviceIds = draft.DevsAndLics.Where(x => x.BulkRegistrationDeviceId != null).Select(x => x.BulkRegistrationDeviceId).ToList();

                var devsAndLics =
                    db.bulkregistrationdevices.Where(dev => dev.BulkRegistrationId == bulkRegistrationId)
                        .Select(dev => dev)
                        .Include(dev => dev.bulkregistrationlicenses);

                var mapExistingBulkRegistrationLicenses = new Dictionary<long, long>();

                foreach (var devAndLic in devsAndLics)
                {
                    foreach (var lic in devAndLic.bulkregistrationlicenses)
                    {
                        if (!mapExistingBulkRegistrationLicenses.ContainsKey(lic.LicenseId))
                        {
                            mapExistingBulkRegistrationLicenses.Add(lic.LicenseId, lic.BulkRegistrationDeviceId);
                        }
                    }
                }
                
                // Update Devs and Lics
                
                var mapBulkRegistrationLicenses = new Dictionary<long, long>();
                var updateDevsAndLics = draft.DevsAndLics.Where(x => x.BulkRegistrationDeviceId != null).ToList();

                if (updateDevsAndLics.Any())
                {
                    foreach (var d in updateDevsAndLics)
                    {
                        var bulkRegDeviceId = d.BulkRegistrationDeviceId ?? 0;

                        if (!IsBundleLicense(d.StandardSupportDisplayName) && d.StandardSupportLicenseId.HasValue &&
                            !mapBulkRegistrationLicenses.ContainsKey(d.StandardSupportLicenseId.Value))
                        {
                            mapBulkRegistrationLicenses.Add(d.StandardSupportLicenseId.Value,
                                bulkRegDeviceId);
                        }

                        if (!IsBundleLicense(d.ThreatProtectionDisplayName) && d.ThreatProtectionLicenseId.HasValue &&
                                 !mapBulkRegistrationLicenses.ContainsKey(d.ThreatProtectionLicenseId.Value))
                        {
                            mapBulkRegistrationLicenses.Add(d.ThreatProtectionLicenseId.Value,
                                bulkRegDeviceId);
                        }

                        if (!IsBundleLicense(d.UrlDisplayName) && d.UrlLicenseId.HasValue &&
                                 !mapBulkRegistrationLicenses.ContainsKey(d.UrlLicenseId.Value))
                        {
                            mapBulkRegistrationLicenses.Add(d.UrlLicenseId.Value,
                                bulkRegDeviceId);
                        }

                        if (!IsBundleLicense(d.GpPortalDisplayName) && d.GpPortalLicenseId.HasValue &&
                                 !mapBulkRegistrationLicenses.ContainsKey(d.GpPortalLicenseId.Value))
                        {
                            mapBulkRegistrationLicenses.Add(d.GpPortalLicenseId.Value,
                                bulkRegDeviceId);
                        }

                        if (!IsBundleLicense(d.GpGatewayDisplayName) && d.GpGatewayLicenseId.HasValue &&
                                 !mapBulkRegistrationLicenses.ContainsKey(d.GpGatewayLicenseId.Value))
                        {
                            mapBulkRegistrationLicenses.Add(d.GpGatewayLicenseId.Value,
                                bulkRegDeviceId);
                        }

                        if (!IsBundleLicense(d.WildfireDisplayName) && d.WildfireLicenseId.HasValue &&
                                 !mapBulkRegistrationLicenses.ContainsKey(d.WildfireLicenseId.Value))
                        {
                            mapBulkRegistrationLicenses.Add(d.WildfireLicenseId.Value,
                                bulkRegDeviceId);
                        }

                        if (!IsBundleLicense(d.DnsDisplayName) && d.DnsLicenseId.HasValue &&
                            !mapBulkRegistrationLicenses.ContainsKey(d.DnsLicenseId.Value))
                        {
                            mapBulkRegistrationLicenses.Add(d.DnsLicenseId.Value,
                                bulkRegDeviceId);
                        }

                        if (!IsBundleLicense(d.SdwanDisplayName) && d.SdwanLicenseId.HasValue &&
                            !mapBulkRegistrationLicenses.ContainsKey(d.SdwanLicenseId.Value))
                        {
                            mapBulkRegistrationLicenses.Add(d.SdwanLicenseId.Value,
                                bulkRegDeviceId);
                        }

                        if (!IsBundleLicense(d.PaaDisplayName) && d.PaaLicenseId.HasValue &&
                            !mapBulkRegistrationLicenses.ContainsKey(d.PaaLicenseId.Value))
                        {
                            mapBulkRegistrationLicenses.Add(d.PaaLicenseId.Value,
                                bulkRegDeviceId);
                        }

                        if (!IsBundleLicense(d.PanoramaManagementDisplayName) && d.PanoramaManagementLicenseId.HasValue &&
                            !mapBulkRegistrationLicenses.ContainsKey(d.PanoramaManagementLicenseId.Value))
                        {
                            mapBulkRegistrationLicenses.Add(d.PanoramaManagementLicenseId.Value,
                                bulkRegDeviceId);
                        }

                        if (d.BundleLicenseId.HasValue &&
                                 !mapBulkRegistrationLicenses.ContainsKey(d.BundleLicenseId.Value))
                        {
                            mapBulkRegistrationLicenses.Add(d.BundleLicenseId.Value,
                                bulkRegDeviceId);
                        }
                    }

                    foreach (var item in devsAndLics)
                    {
                        if (draft.IsOverrideDefaults)
                        {
                            item.CustomValueDeviceName = draft.DefaultDeviceName;
                            item.CustomValueDeviceTagId = draft.DefaultDeviceCategoryId;
                            item.CustomValueAddress1 = draft.DefaultDeviceAddress1;
                            item.CustomValueAddress2 = draft.DefaultDeviceAddress2;
                            item.CustomValueCity = draft.DefaultDeviceCity;
                            item.CustomValueCountryId = draft.DefaultDeviceCountryId;
                            item.CustomValueRegionId = draft.DefaultDeviceRegionId;
                            item.CustomValueOtherRegion = draft.DefaultDeviceOtherRegion;
                            item.CustomValuePostalCode = draft.DefaultDevicePostalCode;
                            item.CustomValueRemarks = draft.DefaultDeviceComments;
                        }

                        if (item.bulkregistrationlicenses.Any())
                        {
                            var licsToRemove = new List<bulkregistrationlicens>();

                            foreach (var lic in item.bulkregistrationlicenses)
                            {
                                if (!mapBulkRegistrationLicenses.ContainsKey(lic.LicenseId))
                                {
                                    licsToRemove.Add(lic);
                                }
                                else
                                {
                                    long bulkRegistrationDeviceId;

                                    mapBulkRegistrationLicenses.TryGetValue(lic.LicenseId,
                                        out bulkRegistrationDeviceId);

                                    if (bulkRegistrationDeviceId > 0 &&
                                        lic.BulkRegistrationDeviceId != bulkRegistrationDeviceId)
                                    {
                                        lic.BulkRegistrationDeviceId = bulkRegistrationDeviceId;
                                    }
                                }
                            }

                            if (licsToRemove.Any())
                            {
                                foreach (var lic in licsToRemove)
                                {
                                    db.Entry(lic).State = EntityState.Deleted;
                                }
                            }
                        }
                        else
                        {
                            var devAndLics =
                                draft.DevsAndLics
                                    .SingleOrDefault(x => x.BulkRegistrationDeviceId == item.BulkRegistrationDeviceId);

                            if (devAndLics != null)
                            {
                                if (!IsBundleLicense(devAndLics.StandardSupportDisplayName) && devAndLics.StandardSupportLicenseId.HasValue &&
                                    (!mapBulkRegistrationLicenses.ContainsKey(devAndLics.StandardSupportLicenseId.Value) ||
                                     !mapExistingBulkRegistrationLicenses.ContainsKey(
                                         devAndLics.StandardSupportLicenseId.Value)))
                                {
                                    AddLicense(db, devAndLics.StandardSupportLicenseId, item.BulkRegistrationDeviceId);
                                }

                                if (!IsBundleLicense(devAndLics.ThreatProtectionDisplayName) && devAndLics.ThreatProtectionLicenseId.HasValue &&
                                    (!mapBulkRegistrationLicenses.ContainsKey(devAndLics.ThreatProtectionLicenseId.Value) ||
                                     !mapExistingBulkRegistrationLicenses.ContainsKey(
                                         devAndLics.ThreatProtectionLicenseId.Value)))
                                {
                                    AddLicense(db, devAndLics.ThreatProtectionLicenseId, item.BulkRegistrationDeviceId);
                                }

                                if (!IsBundleLicense(devAndLics.GpGatewayDisplayName) && devAndLics.GpGatewayLicenseId.HasValue &&
                                    (!mapBulkRegistrationLicenses.ContainsKey(devAndLics.GpGatewayLicenseId.Value) ||
                                     !mapExistingBulkRegistrationLicenses.ContainsKey(
                                         devAndLics.GpGatewayLicenseId.Value)))
                                {
                                    AddLicense(db, devAndLics.GpGatewayLicenseId, item.BulkRegistrationDeviceId);
                                }

                                if (!IsBundleLicense(devAndLics.GpPortalDisplayName) && devAndLics.GpPortalLicenseId.HasValue &&
                                    (!mapBulkRegistrationLicenses.ContainsKey(devAndLics.GpPortalLicenseId.Value) ||
                                     !mapExistingBulkRegistrationLicenses.ContainsKey(
                                         devAndLics.GpPortalLicenseId.Value)))
                                {
                                    AddLicense(db, devAndLics.GpPortalLicenseId, item.BulkRegistrationDeviceId);
                                }

                                if (!IsBundleLicense(devAndLics.UrlDisplayName) && devAndLics.UrlLicenseId.HasValue &&
                                    (!mapBulkRegistrationLicenses.ContainsKey(devAndLics.UrlLicenseId.Value) ||
                                     !mapExistingBulkRegistrationLicenses.ContainsKey(
                                         devAndLics.UrlLicenseId.Value)))
                                {
                                    AddLicense(db, devAndLics.UrlLicenseId, item.BulkRegistrationDeviceId);
                                }

                                if (!IsBundleLicense(devAndLics.WildfireDisplayName) && devAndLics.WildfireLicenseId.HasValue &&
                                    (!mapBulkRegistrationLicenses.ContainsKey(devAndLics.WildfireLicenseId.Value) ||
                                     !mapExistingBulkRegistrationLicenses.ContainsKey(
                                         devAndLics.WildfireLicenseId.Value)))
                                {
                                    AddLicense(db, devAndLics.WildfireLicenseId, item.BulkRegistrationDeviceId);
                                }

                                if (!IsBundleLicense(devAndLics.DnsDisplayName) && devAndLics.DnsLicenseId.HasValue &&
                                    (!mapBulkRegistrationLicenses.ContainsKey(devAndLics.DnsLicenseId.Value) ||
                                     !mapExistingBulkRegistrationLicenses.ContainsKey(
                                         devAndLics.DnsLicenseId.Value)))
                                {
                                    AddLicense(db, devAndLics.DnsLicenseId, item.BulkRegistrationDeviceId);
                                }

                                if (!IsBundleLicense(devAndLics.SdwanDisplayName) && devAndLics.SdwanLicenseId.HasValue &&
                                    (!mapBulkRegistrationLicenses.ContainsKey(devAndLics.SdwanLicenseId.Value) ||
                                     !mapExistingBulkRegistrationLicenses.ContainsKey(
                                         devAndLics.SdwanLicenseId.Value)))
                                {
                                    AddLicense(db, devAndLics.SdwanLicenseId, item.BulkRegistrationDeviceId);
                                }

                                if (!IsBundleLicense(devAndLics.PaaDisplayName) && devAndLics.PaaLicenseId.HasValue &&
                                    (!mapBulkRegistrationLicenses.ContainsKey(devAndLics.PaaLicenseId.Value) ||
                                     !mapExistingBulkRegistrationLicenses.ContainsKey(
                                         devAndLics.PaaLicenseId.Value)))
                                {
                                    AddLicense(db, devAndLics.PaaLicenseId, item.BulkRegistrationDeviceId);
                                }

                                if (!IsBundleLicense(devAndLics.PanoramaManagementDisplayName) && devAndLics.PanoramaManagementLicenseId.HasValue &&
                                    (!mapBulkRegistrationLicenses.ContainsKey(devAndLics.PanoramaManagementLicenseId.Value) ||
                                     !mapExistingBulkRegistrationLicenses.ContainsKey(
                                         devAndLics.PanoramaManagementLicenseId.Value)))
                                {
                                    AddLicense(db, devAndLics.PanoramaManagementLicenseId, item.BulkRegistrationDeviceId);
                                }

                                if (devAndLics.BundleLicenseId.HasValue &&
                                    (!mapBulkRegistrationLicenses.ContainsKey(devAndLics.BundleLicenseId.Value) ||
                                     !mapExistingBulkRegistrationLicenses.ContainsKey(
                                         devAndLics.BundleLicenseId.Value)))
                                {
                                    AddLicense(db, devAndLics.BundleLicenseId, item.BulkRegistrationDeviceId);
                                }
                            }
                        }
                    }

                    db.SaveChanges();
                }

                // Delete Devs and Lics
                var deleteDevsAndLics =
                    devsAndLics.Where(dev => !bulkRegDeviceIds.Contains(dev.BulkRegistrationDeviceId))
                        .Select(dev => dev)
                        .Include(dev => dev.bulkregistrationlicenses)
                        .ToList();

                if (deleteDevsAndLics.Any())
                {
                    var licsToRemove = new List<bulkregistrationlicens>();
                    var devsToRemove = new List<bulkregistrationdevice>();

                    foreach (var devAndLics in deleteDevsAndLics)
                    {
                        foreach (var lic in devAndLics.bulkregistrationlicenses)
                        {
                            if (!mapBulkRegistrationLicenses.ContainsKey(lic.LicenseId))
                            {
                                licsToRemove.Add(lic);
                            }
                        }

                        devsToRemove.Add(devAndLics);
                    }

                    if (licsToRemove.Any())
                    {
                        foreach (var lic in licsToRemove)
                        {
                            db.Entry(lic).State = EntityState.Deleted;
                        }
                    }

                    if (devsToRemove.Any())
                    {
                        foreach (var dev in devsToRemove)
                        {
                            db.Entry(dev).State = EntityState.Deleted;
                        }
                    }

                    db.SaveChanges();
                }

                // Create Devs and Lics
                var createDevsAndLics = draft.DevsAndLics.Where(x => x.BulkRegistrationDeviceId == null).ToList();

                if (createDevsAndLics.Any())
                {
                    foreach (var devAndLics in createDevsAndLics)
                    {
                        var devDetails = db.devices.Single(x => x.SerialNumber == devAndLics.SerialNumber);

                        var devModel = new bulkregistrationdevice
                        {
                            BulkRegistrationId = bulkReg.BulkRegistrationId,
                            DeviceId = devDetails.DeviceId,
                            SerialNumber = devAndLics.SerialNumber,
                            IsAlreadyRegistered = devDetails.RegistrationDate.HasValue,
                            CustomValueDeviceName = draft.DefaultDeviceName,
                            CustomValueDeviceTagId = draft.DefaultDeviceCategoryId,
                            CustomValueAddress1 = draft.DefaultDeviceAddress1,
                            CustomValueAddress2 = draft.DefaultDeviceAddress2,
                            CustomValueCity = draft.DefaultDeviceCity,
                            CustomValueCountryId = draft.DefaultDeviceCountryId,
                            CustomValueRegionId = draft.DefaultDeviceRegionId,
                            CustomValueOtherRegion = draft.DefaultDeviceOtherRegion,
                            CustomValuePostalCode = draft.DefaultDevicePostalCode,
                            CustomValueRemarks = draft.DefaultDeviceComments,
                            CreatedOn = DateTime.UtcNow,
                            IsRegistrationSuccessful = false,
                            RegistrationNotes = string.Empty,
                        };

                        db.bulkregistrationdevices.Add(devModel);
                        db.SaveChanges();

                        if (!IsBundleLicense(devAndLics.StandardSupportDisplayName))
                        {
                            AddLicense(db, devAndLics.StandardSupportLicenseId, devModel.BulkRegistrationDeviceId);
                        }

                        if (!IsBundleLicense(devAndLics.ThreatProtectionDisplayName))
                        {
                            AddLicense(db, devAndLics.ThreatProtectionLicenseId, devModel.BulkRegistrationDeviceId);
                        }

                        if (!IsBundleLicense(devAndLics.GpGatewayDisplayName))
                        {
                            AddLicense(db, devAndLics.GpGatewayLicenseId, devModel.BulkRegistrationDeviceId);
                        }

                        if (!IsBundleLicense(devAndLics.GpPortalDisplayName))
                        {
                            AddLicense(db, devAndLics.GpPortalLicenseId, devModel.BulkRegistrationDeviceId);
                        }

                        if (!IsBundleLicense(devAndLics.UrlDisplayName))
                        {
                            AddLicense(db, devAndLics.UrlLicenseId, devModel.BulkRegistrationDeviceId);
                        }

                        if (!IsBundleLicense(devAndLics.WildfireDisplayName))
                        {
                            AddLicense(db, devAndLics.WildfireLicenseId, devModel.BulkRegistrationDeviceId);
                        }

                        if (!IsBundleLicense(devAndLics.DnsDisplayName))
                        {
                            AddLicense(db, devAndLics.DnsLicenseId, devModel.BulkRegistrationDeviceId);
                        }

                        if (!IsBundleLicense(devAndLics.SdwanDisplayName))
                        {
                            AddLicense(db, devAndLics.SdwanLicenseId, devModel.BulkRegistrationDeviceId);
                        }

                        if (!IsBundleLicense(devAndLics.PaaDisplayName))
                        {
                            AddLicense(db, devAndLics.PaaLicenseId, devModel.BulkRegistrationDeviceId);
                        }

                        if (!IsBundleLicense(devAndLics.PanoramaManagementDisplayName))
                        {
                            AddLicense(db, devAndLics.PanoramaManagementLicenseId, devModel.BulkRegistrationDeviceId);
                        }

                        AddLicense(db, devAndLics.BundleLicenseId, devModel.BulkRegistrationDeviceId);

                        db.SaveChanges();
                    }
                }
            }

            return bulkRegistrationId;
        }

        public List<BulkRegistrationModel> GetAllPendingBulkRegistrations()
        {
            var result = new List<BulkRegistrationModel>();
            var db = SessionDbContext.CspObjectContext;
            var pendingStateName = BulkRegistrationStates.Pending.ToString();
            var pendingStateId = db.bulkregistrationstates.Single(x => x.Name == pendingStateName).BulkRegistrationStateId;

            var bulkRegs =
                (from br in db.bulkregistrations

                    join usr in db.useraccounts on br.CreatedBy equals usr.UserAccountId into tempUsr
                    from leftUsr in tempUsr.DefaultIfEmpty()

                    where
                        br.BulkRegistrationStateId == pendingStateId
                    select new BulkRegistrationModel()
                    {
                        Id = br.BulkRegistrationId,
                        SupportAccountId = br.SupportAccountId,
                        BulkRegistrationStateId = pendingStateId,
                        CreatedById = br.CreatedBy,
                        CreatedByEmail = (leftUsr != null) ? leftUsr.Email : string.Empty,
                        CreatedOn = br.CreatedOn,
                        SubmittedOn = br.SubmittedOn
                    }).ToList();

            var devices =
                (from bulkReg in db.bulkregistrations

                join regDev in db.bulkregistrationdevices on bulkReg.BulkRegistrationId equals regDev.BulkRegistrationId 

                join dev in db.devices on regDev.DeviceId equals dev.DeviceId

                join pm in db.partmasters on dev.PartMasterId equals pm.PartMasterId

                join dc in db.devicecategories on regDev.CustomValueDeviceTagId.ToString() equals dc.DeviceCategoryId.ToString() into tempDc
                from leftDc in tempDc.DefaultIfEmpty()

                join con in db.countries on regDev.CustomValueCountryId equals con.CountryId into tempCon
                from leftCon in tempCon.DefaultIfEmpty()

                join reg in db.regions on regDev.CustomValueRegionId equals reg.RegionId into tempReg
                from leftReg in tempReg.DefaultIfEmpty()

                where
                    bulkReg.BulkRegistrationStateId == pendingStateId

                select new BulkRegistrationDeviceModel()
                {
                    Id = regDev.BulkRegistrationDeviceId,
                    BulkRegistrationId = regDev.BulkRegistrationId,
                    DeviceId = dev.DeviceId,
                    Model = pm.PartNumber,
                    SerialNumber = dev.SerialNumber,
                    VmSeriesLicenseId = regDev.VmSeriesLicenseId,
                    IsAlreadyRegistered = regDev.IsAlreadyRegistered,
                    CustomValueDeviceName = regDev.CustomValueDeviceName,
                    CustomValueDeviceTag = (leftDc != null) ? new NameValue() { Value = leftDc.DeviceCategoryId.ToString(), Name = leftDc.DeviceCategoryName, ValueLong = leftDc.DeviceCategoryId } : new NameValue() { Value = "0", Name = string.Empty, ValueLong = 0 },
                    CustomValueAddress1 = regDev.CustomValueAddress1,
                    CustomValueAddress2 = regDev.CustomValueAddress2,
                    CustomValueCity = regDev.CustomValueCity,
                    CustomValueCountry = (leftCon != null) ? new Country() { Id = leftCon.CountryId, Name = leftCon.Name, Code = leftCon.Code } : new Country() { Id = 0, Name = string.Empty, Code = string.Empty },
                    CustomValueRegion = (leftReg != null) ? new Region() { Id = leftReg.RegionId, Name = leftReg.Name } : new Region() { Id = 0, Name = string.Empty },
                    CustomValueOtherRegion = regDev.CustomValueOtherRegion,
                    CustomValuePostalCode = regDev.CustomValuePostalCode,
                    CustomValueRemarks = regDev.CustomValueRemarks,
                    CreatedOn = regDev.CreatedOn,
                    IsRegistrationSuccessful = regDev.IsRegistrationSuccessful,
                    RegistrationNotes = regDev.RegistrationNotes,
                    RegisteredOn = regDev.RegisteredOn
                }).ToList();

            var licenses =
                (from reg in db.bulkregistrations

                join regDev in db.bulkregistrationdevices on reg.BulkRegistrationId equals regDev.BulkRegistrationId

                join regLic in db.bulkregistrationlicenses on regDev.BulkRegistrationDeviceId equals regLic.BulkRegistrationDeviceId

                join lic in db.licenses on regLic.LicenseId equals lic.LicenseId

                join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId

                where
                    reg.BulkRegistrationStateId == pendingStateId

                select new BulkRegistrationLicenseModel()
                {
                    Id = regLic.BulkRegistrationLicenseId,
                    BulkRegistrationDeviceId = regLic.BulkRegistrationDeviceId,
                    LicenseId = lic.LicenseId,
                    AuthorizationCode = lic.AuthorizationCode,
                    Type = pm.PartNumber,
                    ExpectedExpirationDate = lic.ExpirationDate,
                }).ToList();

            foreach (var reg in bulkRegs)
            {
                reg.BulkRegistrationDevices = devices.Where(dev => dev.BulkRegistrationId == reg.Id).ToList();

                foreach (var dev in devices)
                {
                    dev.BulkRegistrationLicenses =
                        licenses.Where(lic => lic.BulkRegistrationDeviceId == dev.Id).ToList();
                }

                result.Add(reg);
            }

            /*result =
                db.bulkregistrations.Where(br => br.BulkRegistrationStateId == pendingStateId)
                    .Select(m => new BulkRegistrationModel()
                    {
                        Id = m.BulkRegistrationId,
                        SupportAccountId = m.SupportAccountId,
                        BulkRegistrationStateId = pendingStateId,
                        CreatedOn = m.CreatedOn,
                        SubmittedOn = m.SubmittedOn,
                        BulkRegistrationDevices =
                            m.bulkregistrationdevices.Select(dev => new BulkRegistrationDeviceModel()
                            {
                                Id = dev.BulkRegistrationDeviceId,
                                BulkRegistrationId = dev.BulkRegistrationId,
                                DeviceId = dev.DeviceId,
                                BulkRegistrationLicenses =
                                    dev.bulkregistrationlicenses.Select(lic => new BulkRegistrationLicenseModel()
                                    {
                                        Id = lic.LicenseId,
                                        AuthorizationCode = string.Empty
                                    })
                            }).ToList()
                    }).ToList();*/
            
            return result;
        }

        public void UpdateBulkRegistrationDeviceStatus(long bulkRegistrationDeviceId, bool isSuccess, string registrationNotes, string registrationErrors)
        {
            var db = SessionDbContext.CspObjectContext;

            var bulkRegistrationDevice =
                db.bulkregistrationdevices.SingleOrDefault(reg => reg.BulkRegistrationDeviceId == bulkRegistrationDeviceId);

            if (bulkRegistrationDevice != null)
            {
                bulkRegistrationDevice.IsRegistrationSuccessful = isSuccess;
                bulkRegistrationDevice.RegistrationNotes = registrationNotes;
                bulkRegistrationDevice.RegistrationErrors = registrationErrors;

                if (isSuccess)
                {
                    bulkRegistrationDevice.RegisteredOn = DateTime.Now;
                }

                db.SaveChanges();
            }
        }

        public void UpdateBulkRegistrationLicenseStatus(long bulkRegistrationLicenseId, bool isSuccess, string activationNotes, string activationErrors)
        {
            var db = SessionDbContext.CspObjectContext;

            var bulkRegistrationLicense =
                db.bulkregistrationlicenses.SingleOrDefault(reg => reg.BulkRegistrationLicenseId == bulkRegistrationLicenseId);

            if (bulkRegistrationLicense != null)
            {
                bulkRegistrationLicense.IsActivationSuccessful = isSuccess;
                bulkRegistrationLicense.ActivationNotes = activationNotes;
                bulkRegistrationLicense.ActivationErrors = activationErrors;

                if (isSuccess)
                {
                    bulkRegistrationLicense.ActivatedOn = DateTime.Now;
                }

                db.SaveChanges();
            }
        }

        public void UnclaimAllPurchases(long userAccountId, long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var unclaimedState = ClaimedPurchaseStates.Unclaimed.ToString();
            var unclaimedStateId =
                db.claimedpurchasestates.Where(cps => cps.Name == unclaimedState)
                    .Select(x => x.ClaimedPurchaseStateId)
                    .Single();

            var claimedPurchases =
                from cp in db.claimedpurchases

                where
                    cp.SupportAccountId == supportAccountId

                select cp;

            if (claimedPurchases.Any())
            {
                foreach (var item in claimedPurchases)
                {
                    item.ClaimedPurchaseStateId = unclaimedStateId;
                    item.UpdatedOn = DateTime.Now;
                    item.UpdatedBy = userAccountId;
                }

                db.SaveChanges();
            }
        }

        public bool HasBetaAccess(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            return
                db.betaaccesses.Any(
                    x => x.FeatureName == BulkRegistrationFeature && x.SupportAccountId == supportAccountId);
        }

        public List<PrepBulkRegistration.ActivatedLicense> GetActivatedLicensesForDeviceIds(List<long> deviceIds)
        {
            var db = SessionDbContext.CspObjectContext;

            var activatedLics =
                from lic in db.licenses

                join pm in db.partmasters on lic.PartMasterId equals pm.PartMasterId
                let isSvc = (pm.PartNumber.Contains("-SVC-") || pm.PartNumber.EndsWith("-SVC"))
                let isTp = (pm.PartNumber.Contains("-TP-") || pm.PartNumber.EndsWith("-TP"))
                let isATp = (pm.PartNumber.Contains("-ATP-") || pm.PartNumber.EndsWith("-ATP"))
                let isAWF = (pm.PartNumber.Contains("-AWF-") || pm.PartNumber.EndsWith("-AWF"))
                let isAdvUrl = (pm.PartNumber.Contains("-ADVURL-") || pm.PartNumber.EndsWith("-ADVURL"))
                let isUrl2 = (pm.PartNumber.Contains("-URL2-") || pm.PartNumber.EndsWith("-URL2"))
                let isUrl4 = (pm.PartNumber.Contains("-URL4-") || pm.PartNumber.EndsWith("-URL4"))
                let isPortal = (pm.PartNumber.Contains("-PORTAL-") || pm.PartNumber.EndsWith("-PORTAL"))
                let isGp = (pm.PartNumber.Contains("-GP-") || pm.PartNumber.EndsWith("-GP"))
                let isWf = (pm.PartNumber.Contains("-WF-") || pm.PartNumber.EndsWith("-WF"))
                let isDns = (pm.PartNumber.Contains("-DNS-") || pm.PartNumber.EndsWith("-DNS"))              
                let isSdwan = (pm.PartNumber.Contains("-SDWAN-") || pm.PartNumber.EndsWith("-SDWAN"))
                let isBnd = (pm.PartNumber.Contains("BND") && !pm.PartNumber.Contains("VM") && !pm.PartNumber.Contains("IGNITE") && !pm.PartNumber.Contains("TRAPS"))

                where
                    lic.DeviceId != null && (deviceIds.Contains(lic.DeviceId.Value) &&
                    (isSvc || isTp || isUrl2 || isUrl4 || isPortal || isGp || isWf || isBnd || isDns || isSdwan || isATp || isAWF || isAdvUrl))

                select new PrepBulkRegistration.ActivatedLicense()
                {
                    DeviceId = lic.DeviceId.Value,
                    LicenseId = lic.LicenseId,
                    PartMasterId = lic.PartMasterId,
                    PartNumber = pm.PartNumber,
                    AuthorizationCode = lic.AuthorizationCode,
                    RegistrationDate = lic.RegistrationDate,
                    ExpirationDate = lic.ExpirationDate
                };

            var bundleLicPartMasterIds =
                activatedLics.Where(x => x.PartNumber.Contains(BundleLicenseType))
                    .Select(x => x.PartMasterId)
                    .Distinct()
                    .ToList();
            var featurePartNumbers =
                (from pm in db.partmasters
                    join bnd in db.bundles on pm.PartMasterId equals bnd.FeaturePartMasterId

                    where
                        bundleLicPartMasterIds.Contains(bnd.BundlePartMasterId.Value)

                    select new PrepBulkRegistration.BundleModel()
                    {
                        PartMasterId = pm.PartMasterId,
                        PartNumber = pm.PartNumber,
                        BundlePartMasterId = bnd.BundlePartMasterId.Value
                    }).ToList();

            foreach (var lic in activatedLics)
            {
                var partMasterId = lic.PartMasterId;
                var bundles = (lic.PartNumber.Contains(BundleLicenseType))
                    ? featurePartNumbers.Where(x => x.BundlePartMasterId == partMasterId).ToList()
                    : new List<PrepBulkRegistration.BundleModel>();

                lic.Bundles = bundles;
            }

            return activatedLics.ToList();
        }

        private void AddLicense(cspEntities db, long? licenseId, long bulkRegistrationDeviceId)
        {
            if (!licenseId.HasValue)
            {
                return;
            }

            var lic = new bulkregistrationlicens
            {
                BulkRegistrationDeviceId = bulkRegistrationDeviceId,
                LicenseId = licenseId.Value,
                CreatedOn = DateTime.Now,
                IsActivationSuccessful = false,
                ActivationNotes = string.Empty
            };

            db.bulkregistrationlicenses.Add(lic);
        }

        public long? GetCurrentBulkRegistrationId(long supportAccountId, long userAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var draftStateName = BulkRegistrationStates.Draft.ToString();
            var draftStateId = db.bulkregistrationstates.Single(x => x.Name == draftStateName).BulkRegistrationStateId;

            var draftId =
                (
                    from br in db.bulkregistrations
                    where
                        br.BulkRegistrationStateId == draftStateId
                        && br.SupportAccountId == supportAccountId
                        && br.CreatedBy == userAccountId
                    orderby br.CreatedOn descending
                    select (long?) br.BulkRegistrationId
                    ).FirstOrDefault();

            return draftId;
        }

        public IEnumerable<PrepBulkRegistration.SelectableDevice> GetClaimedDevices(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var claimedStateName = ClaimedPurchaseStates.Claimed.ToString();
            var claimedStateId = db.claimedpurchasestates.Single(x => x.Name == claimedStateName).ClaimedPurchaseStateId;

            var devs =
                from c in db.claimedpurchases
                join d in db.devices on c.DeviceId equals d.DeviceId
                join pm in db.partmasters on d.PartMasterId equals pm.PartMasterId
                where
                    c.ClaimedPurchaseStateId == claimedStateId
                    && c.SupportAccountId == supportAccountId

                select new PrepBulkRegistration.SelectableDevice
                {
                    ModelCompatibility = pm.ModelCompatibility,
                    ProductFamily = pm.ProductFamily,
                    SerialNumber = d.SerialNumber,
                    DeviceId = d.DeviceId,
                    PartNumber = pm.PartNumber,
                    RegistrationDate = d.RegistrationDate,
                    ShipDate = d.ShipDate,
                };

            return devs.ToArray();
        }

        public IEnumerable<PrepBulkRegistration.SelectableDevice> GetRegisteredDevices(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var devPartTypeId = db.parttypes.Single(x => x.PanDbIdentification == SearchAllConts.DevPandDbId).PartTypeId;

            var devs =
                from d in db.devices
                
                join pm in db.partmasters on d.PartMasterId equals pm.PartMasterId

                join pst in db.partsubtypes on pm.PartSubtypeId equals  pst.PartSubtypeId into tempPst
                from leftPst in tempPst.DefaultIfEmpty()

                where
                    d.SupportAccountId == supportAccountId
                    && pm.PartTypeId == devPartTypeId
                    && pm.PartNumber != SearchAllConts.VmPartNumber
                    && (pm.ProductFamily == null || pm.ProductFamily != SearchAllConts.TrapsProductFamily)
                    && (leftPst == null || (leftPst != null && leftPst.PanDbIdentification != EvalPanDbIdentification))

                select new PrepBulkRegistration.SelectableDevice
                {
                    ModelCompatibility = pm.ModelCompatibility,
                    ProductFamily = pm.ProductFamily,
                    SerialNumber = d.SerialNumber,
                    DeviceId = d.DeviceId,
                    PartNumber = pm.PartNumber,
                    RegistrationDate = d.RegistrationDate,
                    ShipDate = d.ShipDate
                };

            return devs.ToArray();
        }

        private bool IsBundleLicense(string licenseName)
        {
            return (licenseName != null && licenseName.Contains(BundleLicenseType));
        }
    }
}
