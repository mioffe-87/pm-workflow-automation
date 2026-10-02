using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Csp.Api.Model;
using Csp.Infrastructure.Data.DBs.CSP;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;

namespace Csp.Infrastructure.Data.Persistence {

    public class DeviceRepository : IDeviceRepository {

        const string AscDeviceTransfer = "ASC_TRANSFER";


        #region Exists in DayOne Config
        public IEnumerable<PanOs> GetPanOs() {

            return SessionDbContext.CspObjectContext.panos
                .OrderByDescending(record => record.panosversion)
                .Select(record => new PanOs() { PanOsVersion = record.panosversion }).ToList();
        }

        public IEnumerable<HyperVisor> GetHyperVisors(string filter = null) {
            if (string.IsNullOrEmpty(filter)) 
            {
                return SessionDbContext.CspObjectContext.hypervisors
                    .Select(record => new HyperVisor()
                    {
                        HyperVisorVersion = record.hypervisorversion,
                        DisplayValue = record.displayvalue
                    })
                    .ToList();
            }

            string sqlQuery = (filter.Trim().ToUpper() == "VM") ? string.Format("select * from hypervisor where hypervisorversion in ('VM-AWS', 'VM-ESX', 'VM-AZR', 'VM-GCP', 'VM-HPV', 'VM-KVM')  ORDER BY displayvalue") : "select * from hypervisor ORDER BY displayvalue";
           
            
            var result = SessionDbContext.CspObjectContext.hypervisors.SqlQuery(sqlQuery)
                .Select(record => new HyperVisor() {
                    HyperVisorVersion = record.hypervisorversion,
                    DisplayValue = record.displayvalue
                }).ToList();

            return result;
        }
        #endregion

        public long ReadSupportAccountIdByDeviceBySerialNumber(string serialNumber) {
            long supportAccountId = 0;
            try
            {
                using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
                {
                    var db = SessionDbContext.CspObjectContext;
                    supportAccountId = db.devices.FirstOrDefault(device => device.SerialNumber == serialNumber).SupportAccountId.Value;
                    trans.Complete();
                    return supportAccountId;
                }
            }
            catch
            {
                return supportAccountId;
            }
        }


        public long ReadDeviceIdBySerialNumber(string serialNumber) {

            using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted })) {
                var db = SessionDbContext.CspObjectContext;

                var deviceId = db.devices.Single(device => device.SerialNumber == serialNumber).DeviceId;

                trans.Complete();

                return deviceId;
            }
        }

        public DeviceTagMatches[] GetDeviceTagsByIds(long[] deviceCategoryIds) {

            using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted })) {

                var db = SessionDbContext.CspObjectContext;

                var result =
                    db.devicecategories
                                         .Where(x => deviceCategoryIds.Contains(x.DeviceCategoryId))
                                         .Select(x => new DeviceTagMatches {
                                             DeviceCategoryId = x.DeviceCategoryId,
                                             Name = x.DeviceCategoryName,
                                         }).ToArray();

                trans.Complete();

                return result;
            }
        }

        public string[] GetGroupNamesByDeviceId(long deviceId) {

            using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted })) {

                var db = SessionDbContext.CspObjectContext;

                var groups = (
                    from gd in db.groupdevices
                    where gd.DeviceId == deviceId
                    select gd.supportaccountgroup.Name
                    ).ToArray();

                trans.Complete();

                return groups;
            }
        }

        public void UpdateSupportAccountIdBySerialNumber(string serialNumber, long supportAccountId) {

            {

                var db = SessionDbContext.CspObjectContext;

                var dev = db.devices.Single(x => x.SerialNumber == serialNumber);
                dev.SupportAccountId = supportAccountId;
                dev.RegistrationDate = System.DateTime.Now;
                db.SaveChanges();

            }
        }

        public void UpdateSupportAccountIdByDeviceId(long deviceId, long supportAccountId) {

            {
                var db = SessionDbContext.CspObjectContext;

                var dev = db.devices.Single(x => x.DeviceId == deviceId);
                dev.SupportAccountId = supportAccountId;
                db.SaveChanges();

            }
        }

        public long GetSupportAccountIdByDeviceId(long deviceId) {

            {

                var db = SessionDbContext.CspObjectContext;

                var dev = db.devices.Single(x => x.DeviceId == deviceId);
                return dev.SupportAccountId.Value;
            }
        }

        public bool IsSerialNumberValid(string serialNumber) {
            {
                var db = SessionDbContext.CspObjectContext;

                return db.devices.Any(x => x.SerialNumber == serialNumber);
            }
        }

        public GroupNameId[] GetGroups(long supportAccountId) {
            {

                var db = SessionDbContext.CspObjectContext;

                return (
                                 from sag in db.supportaccountgroups
                                 where sag.SupportAccountId == supportAccountId
                                 select new GroupNameId {
                                     GroupId = sag.GroupId,
                                     GroupName = sag.Name,
                                 }).ToArray();
            }
        }

        public DeviceGroup[] GetDeviceGroups(long supportAccountId) {
            {

                var db = SessionDbContext.CspObjectContext;

                return (
                                 from gd in db.groupdevices
                                 join sag in db.supportaccountgroups on gd.GroupId equals sag.GroupId
                                 where sag.SupportAccountId == supportAccountId
                                 select new DeviceGroup {
                                     DeviceId = gd.DeviceId,
                                     GroupId = gd.GroupId,
                                 }).ToArray();
            }
        }

        public GroupNameId[] GetGroups(long[] supportAccountIds) {
            {
                var db = SessionDbContext.CspObjectContext;

                return (
                                 from sag in db.supportaccountgroups
                                 where supportAccountIds.Contains(sag.SupportAccountId)
                                 select new GroupNameId {
                                     GroupId = sag.GroupId,
                                     GroupName = sag.Name,
                                 }).ToArray();
            }
        }

        public DeviceGroup[] GetDeviceGroups(long[] supportAccountIds) {
            {
                var db = SessionDbContext.CspObjectContext;

                return (
                                 from gd in db.groupdevices
                                 join sag in db.supportaccountgroups on gd.GroupId equals sag.GroupId
                                 where supportAccountIds.Contains(sag.SupportAccountId)
                                 select new DeviceGroup {
                                     DeviceId = gd.DeviceId,
                                     GroupId = gd.GroupId,
                                 }).ToArray();
            }
        }

        public long GetSupportAccountIdByGroupId(long groupId) {
            {
                var db = SessionDbContext.CspObjectContext;

                return db.supportaccountgroups.Where(x => x.GroupId == groupId).Select(x => x.SupportAccountId).Single();
            }
        }

        public List<DeviceWithPartMaster> ReadDevices(long[] deviceIdArray) {

            using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                var db = SessionDbContext.CspObjectContext;

                if (deviceIdArray.Any()) {
                    var query =
                        from dev in db.devices
                        join pm in db.partmasters on dev.PartMasterId equals pm.PartMasterId
                        join dc in db.devicecategories on dev.DeviceCategoryId equals dc.DeviceCategoryId into tempdc
                        from leftdc in tempdc.DefaultIfEmpty()
                        where deviceIdArray.Contains(dev.DeviceId)
                        select new DeviceWithPartMaster {
                            Id = dev.DeviceId,
                            SerialNumber = dev.SerialNumber,
                            //ModelName = dev.
                            //Licenses = 
                            //SupportProvider = 
                            //Customer = 
                            //UserAccount = 
                            //DefaultRoles = 
                            //Members = 
                            RegistrationDate = dev.RegistrationDate,
                            ShipDate = dev.ShipDate,
                            IsActive = dev.IsActive,
                            ShipmentNumber = dev.ShipmentNumber ?? 0,
                            DeviceName = dev.DeviceName,
                            Comments = dev.Comments,
                            IsRMAReturn = dev.IsRMAReturn ?? false,
                            IsRMAReplacement = dev.IsRMAReplacement ?? false,
                            SalesForceAccountId = dev.SalesForceAccountId,
                            CustomerServiceNotes = dev.CustomerServiceNotes,
                            IsSAPSynched = dev.IsSAPSynched ?? false,
                            VirtualSystemsAvailable = dev.VirtualSystemsAvailable ?? 0,
                            BillToCountryId = dev.BillToCountryId ?? 0,
                            City = dev.City,
                            PostalCode = dev.PostalCode,
                            CountryId = dev.CountryId,
                            Region = dev.Region,
                            IsVirtualMachine = dev.IsVirtualMachine ?? false,
                            CpuId = dev.CpuId,
                            UuId = dev.UuId,
                            MacAddress = dev.MacAddress,
                            DeviceCategoryId = dev.DeviceCategoryId,
                            CountryName = dev.country.Name,
                            DeviceCategoryName = leftdc == null ? "" : leftdc.DeviceCategoryName,
                            PartNumber = pm.PartNumber,
                            ModelCompatibility = pm.ModelCompatibility,
                            SupportAccountId = dev.SupportAccountId,
                        };

                    var results = query.ToList();
                    trans.Complete();

                    return results;
                }
            }

            return new List<DeviceWithPartMaster>();
        }

        public List<WildfireDeviceItem> GetWildfireDevices(long userAccountId, long supportAccountId, DataParameters parameters) {

            using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                var db = SessionDbContext.CspObjectContext;

                var wildfireDeviceIds = (from wd in db.wildfiredevices
                                         where
                                             wd.supportaccount.SupportAccountId == supportAccountId &&
                                             wd.WildfireUserAccountId == userAccountId
                                         select wd.DeviceId).Distinct().ToList();
                var wildfireDevices = (from device in db.devices
                                       where
                                           device.SupportAccountId == supportAccountId &&
                                           device.IsActive == true
                                       select new WildfireDeviceItem {
                                           DeviceId = device.DeviceId,
                                           SerialNumber = device.SerialNumber,
                                           IsSelected = wildfireDeviceIds.Contains(device.DeviceId)
                                       }).ToList();

                trans.Complete();

                if (!String.IsNullOrEmpty(parameters.SearchValue))
                {
                    wildfireDevices = wildfireDevices.Where(d => d.SerialNumber.ToLower().Contains(parameters.SearchValue.ToLower())).ToList();
                }

                parameters.TotalRecordCount = wildfireDevices.Count;
                return wildfireDevices.Skip(Convert.ToInt32(parameters.PaginationOffset)).Take(parameters.PaginationCount).ToList(); 
            }
        }



        public bool IsAwsAccountSupportAccountMappingExists(long supportAccountId, string awsAccountId)
        {
            using (var db = new cspEntities())
            {
                return db.awsaccountsupportaccountmappings.Any(x => x.SupportAccountId == supportAccountId && x.AwsAccountId == awsAccountId);
            }
        }


        public bool CreateAwsAccountSupportAccountMapping(long supportAccountId, string awsAccountId)
        {
            //NULL awsAccountId: do not add to the table, it is meaningless.
            if (string.IsNullOrEmpty(awsAccountId))
                return false;
            using (var db = new cspEntities())
            {
                var awsSupportMapping = new awsaccountsupportaccountmapping()
                    {
                        SupportAccountId = supportAccountId,
                        AwsAccountId = awsAccountId
                    };
                db.awsaccountsupportaccountmappings.Add(awsSupportMapping);
                return db.SaveChanges() > 0;
            }
        }


        public bool RegisterCloudDevices(long deviceId, long supportAccountId, long userAccountId, string cloudAccountId)
        {
            //cloudAccountId is NULL, register this device only
            if (string.IsNullOrEmpty(cloudAccountId))
            {
                return RegisterCloudDevice(deviceId, supportAccountId, userAccountId);
            }

            using (var db = new cspEntities())
            {
                var devices = (from d in db.devices
                               join ad in db.clouddevices on d.DeviceId equals ad.DeviceId
                               where ad.CloudAccountId == cloudAccountId && d.SupportAccountId == null && ad.IsActive == true
                               select d).ToList();

                devices.ForEach(dev =>
                    {
                        dev.SupportAccountId = supportAccountId;
                        dev.UserAccountId = userAccountId;
                        dev.RegistrationDate = System.DateTime.Now;
                    });

                return db.SaveChanges() > 0;
            }
        }


        public bool RegisterCloudDevice(long deviceId, long supportAccountId, long userAccountId)
        {
            using (var db = new cspEntities())
            {
                var devices = (from d in db.devices
                               join ad in db.clouddevices on d.DeviceId equals ad.DeviceId
                               where ad.DeviceId == deviceId && ad.IsActive == true
                               select d).ToList();

                devices.ForEach(dev =>
                {
                    dev.SupportAccountId = supportAccountId;
                    dev.UserAccountId = userAccountId;
                    dev.RegistrationDate = System.DateTime.Now;
                });

                return db.SaveChanges() > 0;
            }
        }

        //get sfdc account and sapid from the device record.
        // update aws support account with the device sfdc account and sapid.
        public bool UpdateSapIdAndSfdcIdForAwsAccount(long deviceId, long supportAccountId, long userAccountId, out string currentSfdcAccountId, out string newSfdcAccountId)
        {
            int? sapId = null;
            bool hasChanges = false;
            currentSfdcAccountId = string.Empty;
            newSfdcAccountId = string.Empty;
            using (var db = new cspEntities()) {

                var salesOrderExist = (from d in db.devices
                                       join soi in db.salesorderitems on d.DeviceId equals soi.DeviceId
                                       where d.DeviceId == deviceId
                                       select d.DeviceId).Any();
                if (salesOrderExist)
                {
                    var supportaccount =
                        (from dev in db.devices
                         join soitems in db.salesorderitems on dev.DeviceId equals soitems.DeviceId
                         join so in db.salesorders on soitems.SalesOrderId equals so.SalesOrderId
                         join map in db.sapsupportaccountmappings on so.SAPCustomerId equals map.SAPId
                         join sa in db.supportaccounts on map.SupportAccountId equals sa.SupportAccountId
                         where dev.DeviceId == deviceId
                         select new { sa.SalesforceAccountId, map.SAPId }).FirstOrDefault();

                    if (supportaccount != null)
                    {
                        newSfdcAccountId = supportaccount.SalesforceAccountId.Trim();
                        sapId = supportaccount.SAPId;

                        if (sapId.HasValue)
                        {
                            db.sapsupportaccountmappings.Add(new sapsupportaccountmapping()
                                {
                                    CreatedByUserAccountId = userAccountId,
                                    CreatedOn = System.DateTime.Now,
                                    SAPId = sapId.Value,
                                    SupportAccountId = supportAccountId,
                                    IsPrimary = true
                                });
                            hasChanges = true;
                        }
                    }

                    var awsSupportAccount = (from sa in db.supportaccounts
                                             where sa.SupportAccountId == supportAccountId
                                             select sa).FirstOrDefault();

                    if (awsSupportAccount != null && string.IsNullOrEmpty(newSfdcAccountId) == false)
                    {
                        currentSfdcAccountId = awsSupportAccount.SalesforceAccountId;
                        hasChanges = true;
                        awsSupportAccount.SalesforceAccountId = newSfdcAccountId;
                        awsSupportAccount.LastUpdate = System.DateTime.Now;
                    }

                    if (hasChanges)
                    {
                        return db.SaveChanges() > 0;
                    }

                }
            }
            return false;
        }

        public bool IsAwsAccountAndNotAssociatedWithSapIdOrSfdcId(long supportAccountId, string awsDefaultAccountId)
        {
            bool result = false;
            using (var db = new cspEntities())
            {
								string sfdcAccountId = "";

								// Comments By LK On 02/07/2020
								// Check if the Salesforce Account is an AWS Default Account or not
								var supportAccount = (from sa in db.supportaccounts
																			where sa.SupportAccountId == supportAccountId
																			select sa).FirstOrDefault();

								if (supportAccount != null && !string.IsNullOrEmpty(supportAccount.SalesforceAccountId)) {
									sfdcAccountId = supportAccount.SalesforceAccountId;
								}

								if (sfdcAccountId != awsDefaultAccountId) {
									return false;
								}

                sfdcAccountId = (from asam in db.awsaccountsupportaccountmappings
                                     join sa in db.supportaccounts
                                         on asam.SupportAccountId equals sa.SupportAccountId
                                     where asam.SupportAccountId == supportAccountId
                                     select sa.SalesforceAccountId).FirstOrDefault();

                var hasSapId = db.sapsupportaccountmappings.Any(x => x.SupportAccountId == supportAccountId);
                if ((String.IsNullOrEmpty(sfdcAccountId) || sfdcAccountId == awsDefaultAccountId) && !hasSapId)
                {
                    result = true;
                }
            }
            return result;
        }


        #region IDeviceRepository Members


        public bool IsDeviceCodeCirroSecure(string deviceCode, out long devId) {
            string model_compat = "CIRRO";
            // The device code can be authcode or serial number
            // Lets validate if the devicecode is authcode and is a Cirro
            using (var db = new cspEntities()) {
                devId = (from x in db.licenses
                         join y in db.partmasters on x.PartMasterId equals y.PartMasterId
                         join z in db.devices on x.DeviceId equals z.DeviceId
                         where x.AuthorizationCode == deviceCode && y.ModelCompatibility.Contains(model_compat)
                         select z.DeviceId).FirstOrDefault();
                if (devId != 0) {
                    return true;
                } else {
                    devId = (from x in db.devices
                             join y in db.partmasters on x.PartMasterId equals y.PartMasterId
                             where x.SerialNumber == deviceCode && y.ModelCompatibility.Contains(model_compat)
                             select x.DeviceId).FirstOrDefault();
                    if (devId != 0) {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region IDeviceRepository Members


        public bool RegisterCirroSecureDevice(long deviceId, long supportAccountId, long userAccountId) {
            var db = SessionDbContext.CspObjectContext;
            var dev = db.devices.Single(x => x.DeviceId == deviceId);
            dev.SupportAccountId = supportAccountId;
            dev.UserAccountId = userAccountId;
            db.SaveChanges();
            return true;
        }

        #endregion


        public bool IsInstanceAlreadyRegistered(string serialNumber)
        {
            using (var db = new cspEntities())
            {
                var supportAccountId = (from d in db.devices
                                        join ct in db.clouddevices on d.DeviceId equals ct.DeviceId
                                        where d.SerialNumber == serialNumber
                                        select d.SupportAccountId).FirstOrDefault();

                if (supportAccountId.HasValue)
                {
                    return true;
                }
            }
            return false;
        }


        public bool RegisterAzureDevice(long deviceId, long supportAccountId, long userAccountId)
        {
            using (var db = new cspEntities())
            {
                var azureDevice = (from d in db.devices
                                   where d.DeviceId == deviceId
                                   select d).FirstOrDefault();

                if (azureDevice != null)
                {
                    azureDevice.SupportAccountId = supportAccountId;
                    azureDevice.UserAccountId = userAccountId;
                    azureDevice.RegistrationDate = System.DateTime.Now;
                    return db.SaveChanges() > 0;
                }
            }
            return false;
        }

        public bool HasAscTransferBetaAccess(long? supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            return
                db.betaaccesses.Any(
                    x => x.FeatureName == AscDeviceTransfer && x.SupportAccountId == supportAccountId);
        }

        public List<DeviceCategoryListItem> GetDeviceCategoriesWithDeviceCount(long supportAccountId, long userAccountId, long groupId, string deviceType)
        {
            var db = SessionDbContext.CspObjectContext;

            var data =
                    db.Database.SqlQuery<DeviceCategoryListItem>(
                        "CALL GetDeviceCategoriesWithDeviceCount(@p_SupportAccountId, @p_UserAccountId, @p_GroupId, @p_DeviceType);",
                        new MySqlParameter("p_SupportAccountId", supportAccountId),
                        new MySqlParameter("p_UserAccountId", userAccountId),
                        new MySqlParameter("p_GroupId", groupId),
                        new MySqlParameter("p_DeviceType", deviceType));

            return data.ToList();
        }

        public bool IsCloudDevice(long deviceId, out int? cloudType)
        {
            bool isCloudDevice = false;
            cloudType = null;
            var db = SessionDbContext.CspObjectContext;
            var cloudTypeId = (from x in db.clouddevices
                               where x.DeviceId == deviceId
                               select x.CloudTypeId).FirstOrDefault();

            if (cloudTypeId.HasValue)
            {
                isCloudDevice = true;
                cloudType = cloudTypeId.Value;
            }
            else
            {
                // Check if its a CSSP Provisioned Device?

                var csspDevice = (from x in db.licenses
                                  join y in db.partmasters on x.PartMasterId equals y.PartMasterId
                                  where x.DeviceId == deviceId && x.IsActive && y.PartNumber.Contains("-SP")
                                  select y.PartNumber).FirstOrDefault();

                if (csspDevice != null)
                {
                    isCloudDevice = true;
                    cloudType = 3;
                }
            }
            return isCloudDevice;
        }

        public bool AddDeviceToEsaExclusionList(string serialNumber, long supportAccountId, string user)
        {
            var db = SessionDbContext.CspObjectContext;

            var eaReferenceId = (from info in db.eaquoteinfoes
                                 join eaSupAcct in db.easupportaccounts on info.Id equals eaSupAcct.EAQuoteReferenceId
                                 where eaSupAcct.SupportAccountId == supportAccountId 
                                     && (info.EAType == "ESA" || info.EAType == "4hr-ESA" || info.EAType == "ESA-SCM") 
                                     && info.ExpirationDate != null
                                 orderby info.ExpirationDate descending
                                 select info.Id).FirstOrDefault();

            if (eaReferenceId > 0)
            {
                db.eaexcludeddevices.Add(new eaexcludeddevice()
                {
                    SerialNumber = serialNumber,
                    CreatedBy = user,
                    LastModified = DateTime.Now,
                    EAQuoteReferenceId = eaReferenceId
                });
                return db.SaveChanges() > 0;
            }
            return false;
        }

        //Moved from DpmRepository.GetDevicePartNumber
        public string GetDevicePartNumber(string serialNumber)
        {

            using (var db = new cspEntities())
            {
                var devices = db.devices.Where(d => d.SerialNumber == serialNumber);
                if (devices.Any())
                {
                    var partNumber = devices.Select(d => d.partmaster.PartNumber).Single();
                    return partNumber;
                }

                return string.Empty;
            }
        }

        public void CheckGovernmentCloud(List<DeviceListItem> deviceItems)
        {
            if (deviceItems == null || !deviceItems.Any())
                return;
            var query = new StringBuilder();
            query.Append(@"SELECT DeviceId FROM clouddevices WHERE RegionCode LIKE '%-gov-%' AND DeviceId IN (");
            for (int i = 0; i < deviceItems.Count; i++)
            {
                if (i == 0)
                    query.AppendFormat("{0}", deviceItems[i].DeviceId);
                else
                {
                    query.AppendFormat(",{0}", deviceItems[i].DeviceId);
                }
            }
            query.Append(");");
            var queryStr = query.ToString();

            using (var db = new cspEntities())
            {
                var result = db.clouddevices.SqlQuery(queryStr).ToList();
                foreach (var d in deviceItems)
                {
                    if (result.Exists(r => r.DeviceId == d.DeviceId))
                        d.IsAwsGovCloud = true;
                }
            }
        }
    }
}
