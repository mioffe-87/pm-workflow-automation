using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AutoMapper;
using Csp.Api.DataAccess;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Model;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using Csp.Api.Exceptions;
using Csp.Api.Services.Contracts;
using LINQtoCSV;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

using Csp.Api.DeviceAndLicenseService;
using Csp.Api.HelperClasses;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Group = Csp.Api.Model.Group;
using System.Web;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Csp.Api.Model.Csp.Api.Model;

namespace Csp.Api.Logic
{

    public class DeviceLogic : BaseLogic, IDeviceLogic
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IDeviceTransferRepository _deviceTransferRepository;
        private readonly IGroupLogic _groupLogic;
        private readonly ILicenseLogic _licenseLogic;
        private readonly IAscDeviceRegistrationService _ascDeviceRegistrationService;
        private readonly IUserAccountLogic _userAccountLogic;
        private readonly IMembershipLogic _membershipLogic;
        private readonly ICurrentStateService _currentStateService;
        private readonly ISupportAccountDevicesRepository _supportAccountDevicesRepository;
        private readonly ICsspRepository _csspRepository;
        private readonly IConnectionStrings _connectionStrings;
        private readonly IDeviceDataAccess _deviceDataAccess;
        private readonly string _envType = ConfigurationManager.AppSettings["SalesforceInstance"];
        private const long DebugLong = -1337;
        private readonly ISiteLicenseLogic _siteLicenseLogic;
        private readonly ILicensingApiLogic _iLicensingApiLogic;
        private const string ESA4HRPartNumber = "PAN-ENT-PREM-4HR-ESA";
        private const string ESAPartNumber = "PAN-ENT-PREM-ESA";

        public DeviceLogic(IDeviceRepository deviceRepository, ICountryRepository countryRepository, IDeviceTransferRepository deviceTransferRepository, IGroupLogic groupLogic, ILicenseLogic licenseLogic, IAscDeviceRegistrationService ascDeviceRegistrationService, IUserAccountLogic userAccountLogic, IMembershipLogic membershipLogic, ICurrentStateService currentStateService, ISupportAccountDevicesRepository supportAccountDevicesRepository,
            ICsspRepository csspRepository, IConnectionStrings connectionStrings, IDeviceDataAccess deviceDataAccess, ISiteLicenseLogic siteLicenseLogic, ILicensingApiLogic iLicensingApiLogic)
        {
            _deviceRepository = deviceRepository;
            _countryRepository = countryRepository;
            _deviceTransferRepository = deviceTransferRepository;
            _groupLogic = groupLogic;
            _licenseLogic = licenseLogic;
            _ascDeviceRegistrationService = ascDeviceRegistrationService;
            _userAccountLogic = userAccountLogic;
            _membershipLogic = membershipLogic;
            _currentStateService = currentStateService;
            _supportAccountDevicesRepository = supportAccountDevicesRepository;
            _csspRepository = csspRepository;
            _connectionStrings = connectionStrings;
            _deviceDataAccess = deviceDataAccess;
            _siteLicenseLogic = siteLicenseLogic;
            _iLicensingApiLogic = iLicensingApiLogic;
        }

        #region Private Methods

        /// <summary>
        /// This method is used to update Defaultroles List for an Device.
        /// </summary>
        /// <param name="device"></param>
        /// <returns>void</returns>
        private void UpdateDefaultRoles(Device device)
        {

            Dictionary<long, long> databaseDefaultRolesValues = new Dictionary<long, long>();
            Dictionary<long, long> objectDefaultRolesValues = new Dictionary<long, long>();
            ListUpdateHelper updateHelper;
            List<Role> databaseDefaultRoleList = null;
            DeviceDefaultRolesDataAccess deviceDefaultRolesDataAccess = new DeviceDefaultRolesDataAccess();

            databaseDefaultRoleList = deviceDefaultRolesDataAccess.Read(device.Id, new DataParameters());

            foreach (Role role in databaseDefaultRoleList)
            {

                databaseDefaultRolesValues.Add(role.Id, device.Id);
            }

            foreach (Role role in device.DefaultRoles)
            {

                objectDefaultRolesValues.Add(role.Id, device.Id);
            }

            updateHelper = new ListUpdateHelper(databaseDefaultRolesValues, objectDefaultRolesValues, device.LastUpdateByUserAccountId,
                            deviceDefaultRolesDataAccess.Create, deviceDefaultRolesDataAccess.Delete);
            updateHelper.UpdateDatabase();
        }

        private void AddLicenesToDevices(List<DeviceListItem> devices, bool hideExpiredLicenses)
        {

            var licenseDataAccess = new LicenseDataAccess();
            var grouppedLicenses = licenseDataAccess.ReadMultipleDeviceLicenses(devices.Select(x => x.DeviceId).ToArray(), hideExpiredLicenses);

            // Use support account id 245 to get the License API Key for all support accounts.
            long supportAccountId = ApiConstants.PANW_SUPPORT_ACCOUNT_ID;
            string licensingApiKey = _iLicensingApiLogic.GetLicensingApiKey(supportAccountId).LicensingApiKey;

            foreach (var d in devices)
            {
                var grouppedLicense = grouppedLicenses.SingleOrDefault(x => x.DeviceId == d.DeviceId);

                if (grouppedLicense == null)
                {
                    d.Licenses = new LicenseListItem[0];
                }
                else
                {
                    //Add mode for K2 chassis serial numbers
                    if (d.IsChassis)
                    {
                        string mode;
                        var isK2Device = _deviceDataAccess.IsK2Device(d.SerialNumber, out mode);
                        if (isK2Device && !string.IsNullOrEmpty(mode))
                        {
                            grouppedLicense.Licenses.Add(new LicenseListItem { LicenseName = mode, LicenseFeaturesName = "K2 Mode" });
                        }
                    }
                    MagicLinkHelper.SetBundleMagicLinks(grouppedLicense.Licenses, licensingApiKey);
                    d.Licenses = grouppedLicense.Licenses;
                }
            }
        }

        private void AddGroupsToDevices(List<DeviceListItem> devices, long? supportAccountId = null, long? groupId = null)
        {

            //only one of these can/must be set
            if (supportAccountId.HasValue == groupId.HasValue)
            {
                throw new ArgumentException(string.Format("One and only one of these values must be set: supportAccountId ({0}) and groupId ({1})", supportAccountId.HasValue ? supportAccountId.ToString() : string.Empty, groupId.HasValue ? groupId.ToString() : string.Empty));
            }

            if (!supportAccountId.HasValue)
            {
                supportAccountId = _deviceRepository.GetSupportAccountIdByGroupId(groupId.Value);
            }


            var groupNames = _deviceRepository.GetGroups(supportAccountId.Value);

            if (!groupNames.Any()) return;

            var deviceGroups = _deviceRepository.GetDeviceGroups(supportAccountId.Value);

            foreach (var d in devices)
            {

                var groups =
                    from g in groupNames
                    join dg in deviceGroups.Where(x => x.DeviceId == d.DeviceId) on g.GroupId equals dg.GroupId
                    orderby g.GroupName
                    select g.GroupName;

                d.Group = string.Join("|", groups);
            }
        }

        private void AddDeviceTagToDevices(List<DeviceListItem> devices)
        {
            var deviceCategoryIds = devices?.Where(x => x.DeviceCategoryId.HasValue).Select(x => x.DeviceCategoryId.Value).Distinct().ToArray();
            var tags = deviceCategoryIds.Count() > 0 ?_deviceRepository.GetDeviceTagsByIds(deviceCategoryIds): null;            

            foreach (var d in devices)
            {
                d.DeviceTag = string.Empty;

                if (d.DeviceCategoryId.HasValue)
                {
                    var match = tags?.SingleOrDefault(x => x.DeviceCategoryId == d.DeviceCategoryId.Value);
                    if (match != null)
                    {
                        d.DeviceTag = match.Name;
                    }
                }

                else
                {
                    if (d.DeviceCategoryIdList != null)
                    {
                        var tagslist = _deviceRepository.GetDeviceTagsByIds(d.DeviceCategoryIdList?.Split(',').Select(long.Parse).ToArray());

                        if (tagslist != null)
                        {
                            d.DeviceTag = string.Join(",", tagslist.Select(x => x.Name));
                        }
                    }
                }
            }
        }

        private void AddCountryToDevices(List<DeviceListItem> devices)
        {

            var countryIds = devices.Where(x => x.CountryId.HasValue).Select(x => x.CountryId.Value).Distinct().ToArray();
            var countries = _countryRepository.GetCountriesByIds(countryIds);

            foreach (var d in devices)
            {

                var match = countries.SingleOrDefault(x => x.CountryId == d.CountryId);
                if (match != null)
                {
                    d.Country = match.CountryName;
                }
            }
        }

        private void AddTransferStatusToDevices(List<DeviceListItem> devices)
        {

            var deviceIds = (devices != null) ? devices.Select(x => x.DeviceId).Distinct().ToArray() : new long[0];

            var latestTransferStates =
                _deviceTransferRepository.GetLatestDeviceTransferStatuses(deviceIds);

            foreach (var d in devices)
            {

                d.DeviceTransferMessage = "";

                var latestState = latestTransferStates.SingleOrDefault(x => x.DeviceId == d.DeviceId);

                if (latestState == null) continue;

                switch (latestState.State)
                {
                    case DeviceTransferStates.Accepted:
                        //Do nothing
                        break;

                    case DeviceTransferStates.Cancelled:
                        d.DeviceTransferMessage = "<div class='TransferCancelled'>Transfer Cancelled</div>";
                        break;

                    case DeviceTransferStates.Pending:
                        d.DeviceTransferMessage = "<div class='TransferPending'><a href='javascript:void(0);' class='pending-device-transfer'>Pending Transfer</a></div>";
                        break;

                    case DeviceTransferStates.Rejected:
                        d.DeviceTransferMessage = "<div class='TransferRejected'>Transfer Rejected</div>";
                        break;
                }
            }
        }


        #endregion

        #region Public Methods


        public IEnumerable<PanOs> GetPanOsVersions()
        {
            return _deviceDataAccess.GetPanOs().Select(os => new PanOs() { PanOsVersion = os.Value });
        }

        public IEnumerable<HyperVisor> GetHyperVisorVersions(string filter = null)
        {
            return _deviceRepository.GetHyperVisors(filter);
        }

        /// <summary>
        /// This method is used to Update Device Details
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="restrictedByUserAccountId"></param>
        /// <returns>Device</returns>
        public Device UpdateDevice(Device device, long restrictedByUserAccountId)
        {

            Device resultDevice = null;
            var deviceDataAccess = new DeviceDataAccess();

            using (var transactionScope = new TransactionScope())
            {

                // Update Device Default role
                UpdateDefaultRoles(device);

                // Update Device Details
                resultDevice = deviceDataAccess.Update(device, restrictedByUserAccountId);
                transactionScope.Complete();
                /*if (!String.IsNullOrEmpty(device.PanOsVersion)) {
                    _deviceRepository.UpdateDevice(device);
                }*/
            }

            if (resultDevice != null && resultDevice.IsVirtualMachine)
            {
                // Check if its a Cloud Model. Cloud model does not require location information.
                int? cloudType = null;
                resultDevice.IsCloudDevice = _deviceRepository.IsCloudDevice(resultDevice.Id, out cloudType);
            }
            return resultDevice;
        }

        /// <summary>
        /// This method is used to retrieve the Device details based on Device ID
        /// </summary>
        /// <param name="deviceId">deviceId</param>
        /// <param name="restrictedByUserAccountId"></param>
        /// <returns>Device</returns>
        public Device GetDevice(long deviceId, long restrictedByUserAccountId)
        {

            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();

            var deviceRecord = deviceDataAccess.Read(deviceId, restrictedByUserAccountId);

            if (deviceRecord != null && deviceRecord.IsVirtualMachine)
            {
                // Check if its a Cloud Model. Cloud model does not require location information.
                int? cloudType = null;
                bool isCloudDevice = _deviceRepository.IsCloudDevice(deviceId, out cloudType);
                if (isCloudDevice && cloudType.HasValue)
                {
                    deviceRecord.IsCloudDevice = true;
                    switch (cloudType)
                    {
                        case 1:
                            deviceRecord.IsAwsDevice = true;
                            break;
                        case 2:
                            deviceRecord.IsAzureDevice = true;
                            break;
                        case 3:
                            deviceRecord.IsCsspProvisionedDevice = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            return deviceRecord;
        }

        public IEnumerable<ChassisComponent> GetUnassignedComponentQuantitiesBySalesOrder(string salesOrderNumber)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            var components = deviceDataAccess.GetUnassignedComponentQuantitiesBySalesOrder(salesOrderNumber);
            return components;
        }

        public string IsSalesOrderComponentsRegistered(string salesOrderNumber)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            return deviceDataAccess.IsSalesOrderComponentsRegistered(salesOrderNumber);
        }

        public IEnumerable<ChassisComponent> GetChassisUnassignedComponentQuantities(string chassisSerialNumber)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            return deviceDataAccess.GetChassisUnassignedComponentQuantities(chassisSerialNumber);
        }

        public IEnumerable<ChassisComponent> GetComponentsBySupportAccount(long supportAccountId, DataParameters parameters = null)
        {

            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            var lineCards = deviceDataAccess.GetComponentsBySupportAccount(supportAccountId);

            if (lineCards != null && parameters != null && !string.IsNullOrEmpty(parameters.SearchField) && !string.IsNullOrEmpty(parameters.SearchValue))
            {
                if (parameters.SearchField == "City")
                    lineCards = lineCards.Where(l => l.City == parameters.SearchValue).ToList();
                if (parameters.SearchField == "PostalCode")
                    lineCards = lineCards.Where(l => l.PostalCode == parameters.SearchValue).ToList();
                if (parameters.SearchField == "Region")
                    lineCards = lineCards.Where(l => l.Region == parameters.SearchValue).ToList();
                if (parameters.SearchField == "SerialNumber")
                    lineCards = lineCards.Where(l => l.ChassisSerialNumber == parameters.SearchValue).ToList();
                if (parameters.SearchField == "PartNumber")
                    lineCards = lineCards.Where(l => l.PartNumber == parameters.SearchValue).ToList();
                if (parameters.SearchField == "CountryId")
                    lineCards = lineCards.Where(l => l.CountryId == Convert.ToInt32(parameters.SearchValue)).ToList();
            }
            if (parameters != null)
            {
                var pageOffset = Convert.ToInt32(parameters.PaginationOffset);
                parameters.TotalRecordCount = lineCards.Count();
                lineCards = lineCards.Skip(pageOffset).Take(parameters.PaginationCount);
            }
            return lineCards;
        }

        public Int32 GetUnassignedComponentQuantity(string partNumber, Int64 supportAccountId)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            var quantity = deviceDataAccess.GetUnassignedComponentQuantity(partNumber, supportAccountId);
            return quantity;
        }

        public void UpdateComponent(ChassisComponent chassisComponent)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            deviceDataAccess.UpdateComponent(chassisComponent);
        }

        public void DeleteComponent(ChassisComponent component)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            deviceDataAccess.DeleteComponent(component);
        }

        public void UpdateChassisLocation(ChassisComponent component)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            deviceDataAccess.UpdateChassisLocation(component);
        }

        public ChassisComponent GetChassisLocation(string chassisSerialNumber)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            return deviceDataAccess.GetChassisLocation(chassisSerialNumber);
        }

        public void UpdateChassisComponents(List<ChassisComponent> chassisComponents)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            deviceDataAccess.UpdateChassisComponents(chassisComponents);
        }
        /// <summary>
        /// This method is used to retrieve the Suppport Account by a Device serial number
        /// </summary>
        public long GetSupportAccountIdByDeviceBySerialNumber(string serialNumber)
        {

            return _deviceRepository.ReadSupportAccountIdByDeviceBySerialNumber(serialNumber);

        }


        /// <summary>
        /// This method is used to retrive list of Device based on Support Account ID
        /// </summary>
        /// <param name="supportAccountId">Support Account Id</param>
        /// <param name="restrictedByUserAccountId"></param>
        /// <param name="parameters">Pagiation Parameter</param>
        /// <returns>List<DeviceListItem/></returns>
        public List<DeviceListItem> GetSupportAccountDevices(long supportAccountId, long restrictedByUserAccountId, DataParameters parameters, bool hideExpiredLicenses, bool isDebug = false, StringBuilder sb = null)
        {

            var swReadSupportAccountDevicesOld = new Stopwatch();
            var swReadSupportAccountDevicesNew = new Stopwatch();
            var swAddLicenesToDevices = new Stopwatch();
            var swAddGroupsToDevices = new Stopwatch();
            //var swAddDeviceTagToDevices = new Stopwatch();
            var swAddTransferStatusToDevices = new Stopwatch();


            swReadSupportAccountDevicesNew.Start();
            var devices =
                _supportAccountDevicesRepository.ReadSupportAccountDevices(supportAccountId, SupportAccountDeviceType.Prod, parameters,
                                                                                                        _currentStateService.GetCurrentUser().RoleSecurities, restrictedByUserAccountId)
                                                 .Select(x => (DeviceListItem)x)
                                                 .ToList();
            swReadSupportAccountDevicesNew.Stop();
            Debug.WriteLine("LessJoins: " + swReadSupportAccountDevicesNew.ElapsedMilliseconds);
            if (sb != null)
                sb.AppendLine("LessJoins: " + swReadSupportAccountDevicesNew.ElapsedMilliseconds);



            swAddLicenesToDevices.Start();
            AddLicenesToDevices(devices, hideExpiredLicenses);
            swAddLicenesToDevices.Stop();

            swAddGroupsToDevices.Start();
            AddGroupsToDevices(devices, supportAccountId: supportAccountId);
            swAddGroupsToDevices.Stop();

            //swAddDeviceTagToDevices.Start();
            ////AddDeviceTagToDevices(devices); this is now populated by the main ReadSupportAccountDevices() query
            //swAddDeviceTagToDevices.Stop();

            swAddTransferStatusToDevices.Start();
            AddTransferStatusToDevices(devices);
            swAddTransferStatusToDevices.Stop();

            //distinguish AWS and AWS GOV for Market Place Cloud
            CheckAwsGovernmentCloudForMarketPlace(devices);

            if (sb != null)
            {
                sb.AppendLine();
            }

            return devices;
        }

        private void CheckAwsGovernmentCloudForMarketPlace(List<DeviceListItem> devices)
        {
            //_deviceRepository.CheckGovernmentCloud(devices);
            //if (devices != null && devices.Any())
            //{
            //    foreach (var d in devices)
            //    {
            //        if (d.IsAwsGovCloud)
            //        {
            //            d.HyperVisorVersion = "Amazon AWS Gov Cloud";
            //        }
            //    }
            //}

            _deviceDataAccess.CheckGovernmentCloud(devices);
        }

        public byte[] GetSupportAccountDevicesCsv(long supportAccountId, long restrictedByUserAccountId, string searchField,
                                                                                            string searchValue, bool hideExpiredLicenses)
        {

            var dataParameters = new DataParameters { SearchField = searchField, SearchValue = searchValue };

            var data = GetSupportAccountDevices(supportAccountId, restrictedByUserAccountId, dataParameters, hideExpiredLicenses);

            var cc = new CsvContext();
            var outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                QuoteAllFields = true,
                EnforceCsvColumnAttribute = true,
            };

            using (var ms = new MemoryStream())
            using (TextWriter tw = new StreamWriter(ms))
            {

                var productionAssetsCsv =
                    from d in data
                    from l in d.Licenses.DefaultIfEmpty()
                    select new DeviceListItemCsv
                    {
                        SerialNumber = d.SerialNumber,
                        ModelName = d.ModelName,
                        DeviceName = d.DeviceName,
                        Group = d.Group,
                        License = l != null ? l.LicenseNameDisplay : string.Empty,
                        AuthorizationCode = l != null ? l.AuthorizationCode : string.Empty,
                        ExpirationDate = l != null ? (l.ExpirationDate.HasValue ? l.ExpirationDate.Value.ToShortDateString() : string.Empty) : string.Empty,
                        AuthorizedSupportCenter = d.ASCName,
                        OSVersion=d.PanOsVersion,
                        DeviceTag = d.DeviceTag,
                        StreetAddress=!string.IsNullOrEmpty (d.Address1 )? d.Address1 +" "+ d.Address2 : d.Address2,
                        City = d.City,
                        RegionState = d.RegionState,
                        PostalCode = d.PostalCode,
                        //Country = d.Country,
                        //Remarks = d.Comments,
                        IsPerpetual = l != null && l.IsPerpetual,
                    };

                cc.Write(productionAssetsCsv, tw, outputFileDescription);

                tw.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }


        /// <summary>
        /// This method is used to retrive list of Spare Device based on Support Account ID
        /// </summary>
        /// <param name="supportAccountId">Support Account Id</param>
        /// <param name="parameters">Pagiation Parameter</param>
        /// <returns>List of DeviceListItems</returns>
        public List<DeviceListItem> GetSupportAccountSpareDevices(long supportAccountId, long restrictedByUserAccountId, DataParameters parameters, bool isDebug = false, StringBuilder sb = null)
        {

            var swReadSupportAccountSparesOld = new Stopwatch();
            var swReadSupportAccountSparesNew = new Stopwatch();

            var oldCount = DebugLong;
            swReadSupportAccountSparesOld.Start();
            if (isDebug)
            {
                var deviceDataAccess = new DeviceDataAccess();
                var devicesOld = deviceDataAccess.ReadSupportAccountSpareDevices(supportAccountId, restrictedByUserAccountId, parameters).Select(x => (DeviceListItem)x).ToList();
                oldCount = parameters.TotalRecordCount;
            }
            swReadSupportAccountSparesOld.Stop();

            swReadSupportAccountSparesNew.Start();
            var devices =
                _supportAccountDevicesRepository.ReadSupportAccountDevices(supportAccountId, SupportAccountDeviceType.Spare, parameters,
                                                                                                        _currentStateService.GetCurrentUser().RoleSecurities, restrictedByUserAccountId)
                                                 .Select(x => (DeviceListItem)x)
                                                 .ToList();
            swReadSupportAccountSparesNew.Stop();

            AddLicenesToDevices(devices, false);

            AddGroupsToDevices(devices, supportAccountId: supportAccountId);

            AddDeviceTagToDevices(devices);

            AddCountryToDevices(devices);

            AddTransferStatusToDevices(devices);

            if (isDebug)
            {
                var improvement = swReadSupportAccountSparesOld.ElapsedMilliseconds - swReadSupportAccountSparesNew.ElapsedMilliseconds;
                var sameCount = oldCount == parameters.TotalRecordCount;
                var countText = sameCount ? parameters.TotalRecordCount.ToString() : "DIFF (" + oldCount + " VS " + parameters.TotalRecordCount + ")";
                Debug.WriteLine("Improvement = " + improvement + " COUNT = " + countText);
            }

            return devices;
        }

        public byte[] GetSupportAccountSpareDevicesCsv(long supportAccountId, long restrictedByUserAccountId, string searchField,
                                                                                                     string searchValue)
        {

            var dataParameters = new DataParameters { SearchField = searchField, SearchValue = searchValue };

            var data = GetSupportAccountSpareDevices(supportAccountId, restrictedByUserAccountId, dataParameters);

            var cc = new CsvContext();
            var outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                QuoteAllFields = true,
            };

            using (var ms = new MemoryStream())
            using (TextWriter tw = new StreamWriter(ms))
            {

                var sparesCsv = Mapper.Map<DeviceListItemSparesCsv[]>(data);
                cc.Write(sparesCsv, tw, outputFileDescription);

                tw.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }


        /// <summary>
        /// This method is used to retrive list of device in a prticular group. 
        /// </summary>
        /// <param name="groupId">Group Id</param>
        /// <param name="parameters">Paination Parameter</param>
        /// <returns>List<DeviceListItem></returns>
        public List<DeviceListItem> GetGroupDevices(long groupId, long restrictedByUserAccountId, long supportAccountId, DataParameters parameters)
        {

            var deviceDataAccess = new DeviceDataAccess();

            var devices = deviceDataAccess.ReadGroupDevices(groupId, restrictedByUserAccountId, parameters);

            AddLicenesToDevices(devices, false);

            AddGroupsToDevices(devices, groupId: groupId);

            AddDeviceTagToDevices(devices);

            AddTransferStatusToDevices(devices);

            return devices;
        }

        public byte[] GetGroupDevicesCsv(long groupId, long restrictedByUserAccountId, long supportAccountId, string searchField,
                                                                         string searchValue)
        {

            var dataParameters = new DataParameters { SearchField = searchField, SearchValue = searchValue };

            var data = GetGroupDevices(groupId, restrictedByUserAccountId, supportAccountId, dataParameters);

            var cc = new CsvContext();
            var outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                QuoteAllFields = true,
                EnforceCsvColumnAttribute = true,
            };

            using (var ms = new MemoryStream())
            using (TextWriter tw = new StreamWriter(ms))
            {

                var groupDevicesCsv =
                    from d in data
                    from l in d.Licenses.DefaultIfEmpty()
                    select new DeviceListItemCsv
                    {
                        SerialNumber = d.SerialNumber,
                        ModelName = d.ModelName,
                        DeviceName = d.DeviceName,
                        Group = d.Group,
                        License = l != null ? l.LicenseNameDisplay : string.Empty,
                        AuthorizationCode = l != null ? l.AuthorizationCode : string.Empty,
                        ExpirationDate = l != null ? (l.ExpirationDate.HasValue ? l.ExpirationDate.Value.ToShortDateString() : string.Empty) : string.Empty,
                        AuthorizedSupportCenter = d.ASCName,
                        DeviceTag = d.DeviceTag,
                        City = d.City,
                        RegionState = d.RegionState,
                        PostalCode = d.PostalCode,
                        //Country = d.Country,
                        //Remarks = d.Comments,
                        IsPerpetual = l != null? l.IsPerpetual:false,
                    };
                cc.Write(groupDevicesCsv, tw, outputFileDescription);

                tw.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }


        /// <summary>
        /// This method is used to retrive list of Spare device in a prticular group. 
        /// </summary>
        /// <param name="groupId">Group Id</param>
        /// <param name="parameters">Paination Parameter</param>
        /// <returns>List<DeviceListItem></returns>
        public List<DeviceListItem> GetGroupSpareDevices(long groupId, long restrictedByUserAccountId, long supportAccountId, DataParameters parameters)
        {

            var deviceDataAccess = new DeviceDataAccess();
            var devices = deviceDataAccess.ReadGroupSpareDevices(groupId, restrictedByUserAccountId, parameters);

            AddLicenesToDevices(devices, false);

            AddGroupsToDevices(devices, groupId: groupId);

            AddDeviceTagToDevices(devices);

            AddCountryToDevices(devices);

            AddTransferStatusToDevices(devices);

            return devices;
        }

        public byte[] GetGroupSpareDevicesCsv(long groupId, long restrictedByUserAccountId, long supportAccountId, string searchField,
                                                                                    string searchValue)
        {

            var dataParameters = new DataParameters { SearchField = searchField, SearchValue = searchValue };

            var data = GetGroupSpareDevices(groupId, restrictedByUserAccountId, supportAccountId, dataParameters);

            var cc = new CsvContext();
            var outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                QuoteAllFields = true,
            };

            using (var ms = new MemoryStream())
            using (TextWriter tw = new StreamWriter(ms))
            {

                var groupSparesCsv = Mapper.Map<DeviceListItemSparesCsv[]>(data);
                cc.Write(groupSparesCsv, tw, outputFileDescription);

                tw.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }

        public List<SoftwareKey> GetWF500SoftwareKeys(string serialNumber)
        {
            var deviceDataAccess = new DeviceDataAccess();

            return deviceDataAccess.ReadWF500SoftwareKeys(serialNumber);

        }

        public List<WildfireDeviceItem> GetWildfireDevices(long userAccountId, long supportAccountId, DataParameters parameters)
        {
            return _deviceRepository.GetWildfireDevices(userAccountId, supportAccountId, parameters);
        }

        public static bool IsModelNameSpare(string modelName)
        {
            return modelName.Contains("-SPR") || modelName.Contains("-OSS");
        }


        public List<DeviceListItem> GetAvailableSerialNumber(long groupId, long restrictedByUserAccountId, bool isProd, DataParameters parameters)
        {

            var deviceDataAccess = new DeviceDataAccess();

            return
                isProd
                ? deviceDataAccess.ReadAvailableSerialNumber(groupId, restrictedByUserAccountId, parameters)
                : deviceDataAccess.ReadAvailableSpareSerialNumber(groupId, restrictedByUserAccountId, parameters);
        }

        public string CheckIsSerialNumberPanorama(string serialNumber)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            return deviceDataAccess.CheckIsSerialNumberPanorama(serialNumber);
        }

        public string GetLegacyLightCyberDeviceName(string serialNumber)
        {
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand
                {
                    CommandType = CommandType.Text,
                    Connection = connection
                })
                {
                    command.CommandText = "SELECT DeviceName FROM LightCyberDevice WHERE SerialNumber = ?p_SerialNumber";
                    command.Parameters.AddWithValue("?p_SerialNumber", serialNumber);

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        connection.Close();
                        return HttpUtility.HtmlEncode(result.ToString());
                    }
                }
                connection.Close();
            }

            return string.Empty;
        }

        /// <summary>
        /// This method is to Check Is SerialNumber Valid and Not Registerd
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>List<Country></returns>
        public string CheckIsSerialNumberValidandNotRegisterd(string serialNumber)
        {

            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            int isValid = deviceDataAccess.CheckIsSerialNumberValidandNotRegisterd(serialNumber);
            var message = isValid == 1 ? ApiConstants.SerialNumberNotValid_Message : (isValid == 2 ? ApiConstants.SerialNumberRegsitered_Message : string.Empty);
            return message;
        }

        /// <summary>
        /// This method is to Check Is SerialNumber is eligible to be registered from device registration page
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>IsEligible</returns>
        public string CheckIsSerialNumberEligible(string serialNumber)
        {

            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            bool isValid = deviceDataAccess.CheckIsSerialNumberEligible(serialNumber);
            var message = isValid ? string.Empty : ApiConstants.SerialNumberNotEligible_Message;
            return message;
        }

        /// <summary>
        /// This method is to Check Is SerialNumber Valid and Registerd
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>List<Country></returns>
        public string CheckIsSerialNumberValidandAndRegisterd(string serialNumber)
        {

            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();

            return deviceDataAccess.CheckIsSerialNumberValidandRegisterd(serialNumber);
        }
        /// <summary>
        /// This method is to Check Is SerialNumber Valid and Not Registerd
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>List<Country></returns>
        public bool CheckIsSerialNumberValid(string serialNumber)
        {

            return _deviceRepository.IsSerialNumberValid(serialNumber);
        }


        public Device RegisterDevice(Device device, string countryName, long lastUpdateByUserAccountId, out bool isSiteLicenseGranted, Group currentGroup = null, string ascCustBklnSupportAuthCode = null, bool isCalledFromSelfServiceRegistration = false)
        {
            isSiteLicenseGranted = false;
            var groupId = currentGroup == null ? 0 : (currentGroup.Id > 0 ? currentGroup.Id : 0); 
            var supportAccountId =
                groupId == 0
                    ? device.Customer.Id
                    : currentGroup.SupportAccountId;

            _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, device.SerialNumber, "DeviceLogic---RegisterDevice", "Register Device Started", "Info", lastUpdateByUserAccountId);

            var email = _userAccountLogic.GetUserAccount(lastUpdateByUserAccountId, lastUpdateByUserAccountId).Email;
            _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, device.SerialNumber, "DeviceLogic---RegisterDevice", "Register Device Step 1 Recieved Email Id : " + email, "Info", lastUpdateByUserAccountId);

            var isAscForCustRegistration = !string.IsNullOrEmpty(ascCustBklnSupportAuthCode);
            if (isAscForCustRegistration)
            {

                //Set the customer's support account id and clear any group (you can't cross register to a group)
                var customerSupportAccountIds = _ascDeviceRegistrationService.GetDeviceSalesOrderSupportAccountId(device.SerialNumber);
                var ascSupportAccountId = supportAccountId;
                supportAccountId = customerSupportAccountIds.Single();
                currentGroup = null;

                //Verify they can do this
                var isAscMember = _ascDeviceRegistrationService.IsMemberOfAsc(lastUpdateByUserAccountId, ascSupportAccountId);

                var deviceSupportAccountIds = _ascDeviceRegistrationService.GetDeviceSalesOrderSupportAccountId(device.SerialNumber);
                var belongsToAnotherSupportAccount = deviceSupportAccountIds.Any(saId => saId != ascSupportAccountId);

                var isBklnLicense = _ascDeviceRegistrationService.IsBklnAuthCode(ascCustBklnSupportAuthCode);
                var bklnSupportCenterSupportAccountId =
                        _ascDeviceRegistrationService.GetBklnSupportCenterSupportAccountId(ascCustBklnSupportAuthCode);
                var isCorrectBklnForAsc = bklnSupportCenterSupportAccountId == ascSupportAccountId;

                if (!isAscMember || !belongsToAnotherSupportAccount || deviceSupportAccountIds.Count() > 1 || !isBklnLicense || !isCorrectBklnForAsc)
                {
                    _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, device.SerialNumber, "DeviceLogic---RegisterDevice", "ASC not authorized to register device for customer. Asc Support Account Id: " + ascSupportAccountId + " SN: " + device.SerialNumber + " Authcode: " + ascCustBklnSupportAuthCode , "Info", lastUpdateByUserAccountId);
                    
                    throw new Exception(string.Format("ASC not authorized to register device for customer. Asc Support Account Id: {0} SN: {1} Authcode: {2}",
                            ascSupportAccountId, device.SerialNumber, ascCustBklnSupportAuthCode));
                }

                //Check if any memberships exist in the customer's account (active or not)
                var hasMembers = _ascDeviceRegistrationService.HasAnyMemberships(supportAccountId);
                //If not, then add the current user as the super user
                if (!hasMembers)
                {


                    var membership = new Membership()
                    {
                        UserAccount = new UserAccount() { Id = lastUpdateByUserAccountId },
                        SupportAccountId = supportAccountId,
                        ActivationDate = DateTime.Now,
                        Description = "New user created",
                        CreatedOn = DateTime.Now,
                        CreatedByEmail = email,
                        CreatedByUserAccountId = lastUpdateByUserAccountId
                    };

                    membership = _membershipLogic.CreateMembership(membership);
                    var membershipId = membership.Id;

                    //Will automatically make them Super
                    _membershipLogic.AssignMembershipRoles(membershipId, lastUpdateByUserAccountId, supportAccountId,
                                                                                                lastUpdateByUserAccountId);
                }
            }
            _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, device.SerialNumber, "DeviceLogic---RegisterDevice", "Register Device Step 2 Cloude Device ", "Info", lastUpdateByUserAccountId);

            var deviceDataAccess = new DeviceDataAccess();
            long deviceId = 0;
            if (device.IsCloudDevice == false)
            {
                _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, device.SerialNumber, "DeviceLogic---RegisterDevice", "Register Device Step 3 UpdatePANDBDeviceMaster ", "Info", lastUpdateByUserAccountId);

                deviceDataAccess.UpdatePANDBDeviceMaster(device, countryName);
                if (isAscForCustRegistration)
                {
                    _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, device.SerialNumber, "DeviceLogic---RegisterDevice", "Register Device Step 4 UpdateForDeviceRegistrationWithoutMembershipCheck ", "Info", lastUpdateByUserAccountId);

                    _ascDeviceRegistrationService.UpdateForDeviceRegistrationWithoutMembershipCheck(device.SerialNumber,
                                                                                                    device.Region,
                                                                                                    device.DeviceCategoryId,
                                                                                                    lastUpdateByUserAccountId,
                                                                                                    supportAccountId);
                }
                else
                {
                    _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, device.SerialNumber, "DeviceLogic---RegisterDevice", "Register Device Step 5 UpdateForDeviceRegistration", "Info", lastUpdateByUserAccountId);

                    deviceDataAccess.UpdateForDeviceRegistration(device, lastUpdateByUserAccountId, supportAccountId);
                }
                _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, device.SerialNumber, "DeviceLogic---RegisterDevice", "Register Device Step 6 UpdateForDeviceRegistration Done", "Info", lastUpdateByUserAccountId);

                /*  if (!String.IsNullOrEmpty(device.PanOsVersion)) {
                      _deviceRepository.UpdateDevice(device);
                  }*/
                device.Customer.Id = supportAccountId;
                // Exclude Device ITAP-322
                // ApplyEsaLicenseToDevice: null - ESA not expected, 1 - ESA/4HR ESA eligible but not selected, 2: - ESA, 3 - 4HR ESA
                if (device.ApplyEsaLicenseToDevice.HasValue) // ESA/4HR ESA eligeable device
                {
                    if (device.ApplyEsaLicenseToDevice.Value == 1) // ESA/4HR ESA is not selected - add the SN to csp.eaexcludeddevices table
                    {
                        bool addedToExclusionListSuccessfully = _deviceRepository.AddDeviceToEsaExclusionList(device.SerialNumber, supportAccountId, email);
                        if (!addedToExclusionListSuccessfully)
                        {
                            string errMsg =
                                string.Format(
                                    "Could not add device {0} to ESA Exclusion List! Reason: Could not find a matching EaQuoteInfoId for SupportAccountId {1} ",
                                    device.SerialNumber, supportAccountId);
                            Elmah.ErrorSignal.FromCurrentContext().Raise(new ExceptionHandlingException(errMsg));
                            throw new CspNotificationException(errMsg);
                        }
                    }
                    // CSP 4 Hour and Backline new asset reg
                    else if (device.ApplyEsaLicenseToDevice.Value == 2 // ESA is selected 
                        || device.ApplyEsaLicenseToDevice.Value == 3)  // 4hr ESA is selected
                    {
                        string partId = device.ApplyEsaLicenseToDevice.Value == 3 ? ESA4HRPartNumber : ESAPartNumber;
                        _deviceDataAccess.EaEsaDeviceInsert(supportAccountId, partId, device.SerialNumber, lastUpdateByUserAccountId.ToString());
                    }
                }
                _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, device.SerialNumber, "DeviceLogic---RegisterDevice", "Register Device Step 6 ActivateLicense Start", "Info", lastUpdateByUserAccountId);

                var errorMessage = _licenseLogic.ActivateLicense(device.SerialNumber);
                _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, device.SerialNumber, "DeviceLogic---RegisterDevice", "Register Device Step 7 ActivateLicense Done And Message : " + errorMessage, "Info", lastUpdateByUserAccountId);

                if (errorMessage != "")
                {
                    //  Elmah.ErrorSignal.FromCurrentContext().Raise(new ExceptionHandlingException(errorMessage));
                    //  throw new CspNotificationException(errorMessage);
                }
                _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, device.SerialNumber, "DeviceLogic---RegisterDevice", "Register Device Step 8 GrantSiteLicenses Start ", "Info", lastUpdateByUserAccountId);

                isSiteLicenseGranted = _licenseLogic.GrantSiteLicenses(supportAccountId); // This was changed so that ESA license can be applied.
                _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, device.SerialNumber, "DeviceLogic---RegisterDevice", "Register Device Step 9 GrantSiteLicenses Done And Status  : " + (isSiteLicenseGranted ? "Success" : "Failure"), "Info", lastUpdateByUserAccountId);

                deviceId = deviceDataAccess.GetDeviceIdBySerialNumber(device.SerialNumber);
            }
            else
            {
                if (device.IsAwsDevice || device.IsGoogleCloudDevice)
                {
                    // Call Licensing Service to Create and activate Aws Instance.
                    string cloudAccountId, errorMessage;
                    string serialNumber = device.AwsSerialNumber, cpuid = device.AwsCpuid, uuid = device.AwsUuid;
                    if (device.IsGoogleCloudDevice)
                    {
                        serialNumber = device.GoogleCloudSerialNumber;
                        cpuid = device.GoogleCloudCpuid;
                        uuid = device.GoogleCloudUuid;
                    }


                    bool isSuccess = _licenseLogic.CreateCloudInstanceWithAccountId(serialNumber, cpuid, uuid,
                            email, device.IsAwsDevice, out deviceId, out cloudAccountId, out errorMessage); ;

                    if (isSuccess && string.IsNullOrEmpty(errorMessage))
                    {
                        if (IsInstanceAlreadyRegistered(serialNumber))
                        {
                            errorMessage = string.Format("Validation failed: {0} Instance({1}) already registered", device.IsAwsDevice ? "AWS" : "Google Cloud", serialNumber);
                        }
                        else
                        {
                            isSiteLicenseGranted = _licenseLogic.GrantSiteLicenses(supportAccountId);
                            //cloudAccountId is NULL for AWS GOV cloud before License API validation is done. 
                            if (device.IsAwsDevice && !string.IsNullOrWhiteSpace(cloudAccountId))
                            {
                                if (!_deviceRepository.IsAwsAccountSupportAccountMappingExists(supportAccountId, cloudAccountId))
                                {
                                    _deviceRepository.CreateAwsAccountSupportAccountMapping(supportAccountId, cloudAccountId);
                                }
                            }
                            _deviceRepository.RegisterCloudDevice(deviceId, supportAccountId, lastUpdateByUserAccountId);
                            device.LastUpdateByEmail = email;
                            device.Id = deviceId;
                            deviceDataAccess.UpdateDeviceMetadata(device, countryName); // Update device metadata  
                        }
                    }

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new ExceptionHandlingException(errorMessage));
                        throw new CspNotificationException(errorMessage);
                    }
                }
                else if (device.IsAzureDevice)
                {
                    // Call Licensing Service to Create and activate Aws Instance.
                    string errorMessage;
                    bool isSuccess = _licenseLogic.CreateAzureInstance(device.AzureSerialNumber, device.AzureCpuid,
                        device.AzureUuid,
                        email, out deviceId, out errorMessage);
                    if (isSuccess && string.IsNullOrEmpty(errorMessage))
                    {
                        if (IsInstanceAlreadyRegistered(device.AzureSerialNumber))
                        {
                            errorMessage = "Validation failed: Azure Instance already registered";
                        }
                        else
                        {
                            isSiteLicenseGranted = _licenseLogic.GrantSiteLicenses(supportAccountId);
                            _deviceRepository.RegisterAzureDevice(deviceId, supportAccountId, lastUpdateByUserAccountId);
                            device.LastUpdateByEmail = email;
                            device.Id = deviceId;
                            deviceDataAccess.UpdateDeviceMetadata(device, countryName); // Update device metadata
                        }
                    }

                    if (errorMessage != "")
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new ExceptionHandlingException(errorMessage));
                        throw new CspNotificationException(errorMessage);
                    }
                }
                else
                {
                    try
                    {
                        //1) Backup in csp.logdevice
                        var logDeviceDataAccess = new LogDeviceDataAccess();
                        deviceId = deviceDataAccess.GetDeviceIdBySerialNumber(device.CsspSerialNumber);
                        logDeviceDataAccess.Log(deviceId, CsspConstants.CsspDeviceTransferLog, lastUpdateByUserAccountId);
                        //2) Update device metadata
                        device.Id = deviceId;
                        deviceDataAccess.UpdateDeviceMetadata(device, countryName); // Update device metadata
                        //3) Set new supportaccount id
                        device.IsCsspEndUserDeviceRegistrationSuccess = _csspRepository.UpdateSupportAccountForProvisionedDevice(deviceId, lastUpdateByUserAccountId,
                            supportAccountId);
                        //4) Update Pan accounts for deviceid
                        if (device.IsCsspEndUserDeviceRegistrationSuccess)
                        {
                            long? endUserId;
                            bool isCspAccountUpdated;
                            bool isSfdcAccountUpdated;
                            if (_csspRepository.UpdatePanAccountForEndUser(deviceId, supportAccountId, out endUserId, out isCspAccountUpdated, out isSfdcAccountUpdated))
                            {
                                if (endUserId.HasValue)
                                {
                                    string message = string.Empty;

                                    if (isCspAccountUpdated && isSfdcAccountUpdated)
                                    {
                                        message =
                                            "End User PAN Accounts (CSP, SFDC) SUCCESSFULLY UPDATED FROM CSP DURING ENDUSER DEVICE REGISTRATION!";
                                    }
                                    else if (isCspAccountUpdated)
                                    {
                                        message =
                                            "End User CSP ACCOUNT SUCCESSFULLY UPDATED FROM CSP DURING ENDUSER DEVICE REGISTRATION!";
                                    }
                                    else if (isSfdcAccountUpdated)
                                    {
                                        message =
                                            "End User SFDC Account SUCCESSFULLY UPDATED FROM CSP DURING ENDUSER DEVICE REGISTRATION!";
                                    }
                                    else
                                    {
                                        message =
                                            "End User accounts not updated during CSP device registration!";
                                    }

                                    _csspRepository.LogCsspPanAccountCreationDetails(endUserId.Value, "CSP,SFDC", "INFO", message, "CSP User");
                                }
                            }
                        }
                    }
                    catch (Exception oex)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new ExceptionHandlingException(oex.ToString()));
                        throw new CspNotificationException("CSSP Device Registration Error.");
                    }
                }
            }
            if (currentGroup != null)
            {
                _groupLogic.CreateGroupDevice(currentGroup.Id, deviceId, lastUpdateByUserAccountId);
            }

            device.Id = deviceId;

            if (isAscForCustRegistration)
            {
                //Apply BKLN support license

                var licenseResult = _licenseLogic.ActivateLicense(device.SerialNumber, authorizationCode: ascCustBklnSupportAuthCode);
                if (!string.IsNullOrEmpty(licenseResult))
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new ExceptionHandlingException(licenseResult));
                    throw new CspNotificationException(licenseResult);
                }
            }
            return device;
        }
        public List<DeviceCategory> GetAllDeviceCategories(long supportAccountId, long userAccountId)
        {
            var deviceDataAccess = new DeviceDataAccess();
            var deviceCategoryListItems = deviceDataAccess.ReadDeviceCategories(supportAccountId, userAccountId, new DataParameters());

            return deviceCategoryListItems.Select(
                                        d =>
                                        new DeviceCategory()
                                        {
                                            DeviceCategoryId = d.DeviceCategoryId,
                                            DeviceCategoryName = d.DeviceCategoryName,
                                        }).ToList(); ;
        }

        public List<DeviceCategoryListItem> GetDeviceCategories(long supportAccountId, long userAccountId, DataParameters dataParameters)
        {
            var deviceDataAccess = new DeviceDataAccess();
            var deviceCategoryListItems = deviceDataAccess.ReadDeviceCategories(supportAccountId, userAccountId, dataParameters);

            return deviceCategoryListItems;
        }

        public DeviceCategory CreateDeviceCategory(DeviceCategory deviceCategory)
        {
            DeviceCategory resultDeviceCategory = null;
            var deviceDataAccess = new DeviceDataAccess();

            using (var transactionScope = new TransactionScope())
            {
                resultDeviceCategory = deviceDataAccess.CreateDeviceCategory(deviceCategory);

                transactionScope.Complete();
            }

            return resultDeviceCategory;
        }

        public DeviceCategory UpdateDeviceCategory(DeviceCategory deviceCategory)
        {
            DeviceCategory resultDeviceCategory = null;
            var deviceDataAccess = new DeviceDataAccess();

            using (var transactionScope = new TransactionScope())
            {
                resultDeviceCategory = deviceDataAccess.UpdateDeviceCategory(deviceCategory);

                transactionScope.Complete();
            }

            return resultDeviceCategory;
        }

        public DeviceCategory GetDeviceCategory(long deviceCategoryId)
        {
            var deviceDataAccess = new DeviceDataAccess();

            return deviceDataAccess.ReadDeviceCategory(deviceCategoryId);
        }

        public void DeleteDeviceCategory(DeviceCategory deviceCategory)
        {
            var deviceDataAccess = new DeviceDataAccess();

            using (var transactionScope = new TransactionScope())
            {
                deviceDataAccess.DeleteDeviceCategory(deviceCategory);

                transactionScope.Complete();
            }
        }

        public bool IsDeviceCategoryExists(string deviceCategoryName, long supportAccountId, long deviceCategoryId)
        {
            var deviceDataAccess = new DeviceDataAccess();
            return deviceDataAccess.IsDeviceCategoryExists(deviceCategoryName, supportAccountId, deviceCategoryId);
        }

        #endregion

    
        public bool PushToDeviceMaster(string serialNumber, int? userAccountId, string partId, System.DateTime? shipDate = null)
        {
            try
            {
                if (shipDate == null)
                {
                    shipDate = System.DateTime.Now;
                }
                var devMaster = new DeviceMaster() { SerialNumber = serialNumber, PartId = partId, ShipDate = shipDate, IsActive = true, ShipNumber = "0", RegisterDate = DateTime.Now, AccountId = userAccountId };
                var licResponse = GetDeviceAndLicenseServiceInstance().PushToDeviceMaster(devMaster);
                var result = licResponse.Result;
                if (result.HasValue)
                {
                    return result.Value;
                }
                return false;
            }
            catch (Exception oex)
            {
                var ex = oex;
                throw;
            }
        }


        public long GetDeviceIdBySerialNumber(string serialNumber)
        {
            return _deviceRepository.ReadDeviceIdBySerialNumber(serialNumber);
        }



        public bool CreateAwsInstance(string serialNumber, string cpuId, string uuId, string user, out long deviceId, out string awsAccountId, out string errorMessage)
        {
            return _licenseLogic.CreateCloudInstanceWithAccountId(serialNumber, cpuId, uuId, user, true, out deviceId, out awsAccountId,
                                                   out errorMessage);
        }


        public bool RegisterAWSDevices(long supportAccountId, string awsAccountId, long deviceId, long userAccountId)
        {
            //cloudAccountId is NULL is meaningless
            if (!string.IsNullOrEmpty(awsAccountId) && !_deviceRepository.IsAwsAccountSupportAccountMappingExists(supportAccountId, awsAccountId))
            {
                _deviceRepository.CreateAwsAccountSupportAccountMapping(supportAccountId, awsAccountId);
            }
            return _deviceRepository.RegisterCloudDevices(deviceId, supportAccountId, userAccountId, awsAccountId);
        }

        public bool UpdateSapIdSfdcIdForAwsSupportAccount(long supportAccountId, long deviceId, long userAccountId)
        {
            try
            {
                string awsSfAccountId = ConfigurationManager.AppSettings["SalesforceDefaultAWSAccountId"].ToString();
                bool canProceed = _deviceRepository.IsAwsAccountAndNotAssociatedWithSapIdOrSfdcId(supportAccountId, awsSfAccountId);
                if (canProceed)
                {
                    // Using the deviceId (non aws) we get the sapid, sfdcid. 
                    string currentSfdcAccountId;
                    string newSfdcAccountId;

                    bool isSuccess = _deviceRepository.UpdateSapIdAndSfdcIdForAwsAccount(deviceId, supportAccountId, userAccountId, out currentSfdcAccountId, out newSfdcAccountId);

                    if (isSuccess && !string.IsNullOrEmpty(currentSfdcAccountId) && !string.IsNullOrEmpty(newSfdcAccountId))
                    {
                        // get list of users in current support account
                        // get their contact record in salesforce
                        // update their salesforce account 

                        var useraccounts = _userAccountLogic.GetAwsSupportAccountUsers(supportAccountId);
                        foreach (var useraccount in useraccounts)
                        {
                            _userAccountLogic.UpdateSalesforceContactsAccount(useraccount, supportAccountId, currentSfdcAccountId,
                                newSfdcAccountId);
                        }
                    }

                }
                return false;
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new ExceptionHandlingException(oex.ToString()));
                return false;
            }
        }

        public bool CreateAzureInstance(string serialNumber, string cpuId, string uuId, string user, out long deviceId, out string errorMessage)
        {
            return _licenseLogic.CreateAzureInstance(serialNumber, cpuId, uuId, user, out deviceId, out errorMessage);
        }


        public bool IsInstanceAlreadyRegistered(string azureSerialNumber)
        {
            return _deviceRepository.IsInstanceAlreadyRegistered(azureSerialNumber);
        }


        public bool ValidateEndUserSerialNumber(string serialNumber, out string eMessage)
        {
            return _csspRepository.ValidateEndUserSerialNumber(serialNumber, out eMessage);
        }

        public bool ValidateEndUserCustomerAccountId(string customerAccountId, out string eMessage)
        {
            return _csspRepository.ValidateEndUserCustomerAccountId(customerAccountId, out eMessage);
        }

        public bool ValidateCsspEndUserInputCombination(string serialNumber, string customerAccountId, out string eMessage)
        {
            return _csspRepository.ValidateCsspEndUserInputCombination(serialNumber, customerAccountId, out eMessage);
        }

        public List<DeviceCategoryListItem> GetDeviceCategoriesWithDeviceCount(long supportAccountId, long userAccountId, long groupId, string deviceType)
        {
            return _deviceRepository.GetDeviceCategoriesWithDeviceCount(supportAccountId, userAccountId, groupId, deviceType);
        }

        public PendingDeviceTransferDetails GetPendingDeviceTransferDetails(long deviceId)
        {
            return _deviceTransferRepository.GetPendingDeviceTransferDetails(deviceId);
        }

        public bool Has4HRSupport(long deviceId)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            return deviceDataAccess.Has4HRSupport(deviceId);
        }

        public string GetAuthcodeInfo(long licenseId)
        {
            if (licenseId < 1)
                return string.Empty;
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand
                {
                    CommandType = CommandType.Text,
                    Connection = connection
                })
                {
                    command.CommandText = "SELECT l.AuthorizationCode FROM license l WHERE l.LicenseId=?p_LicenseId;";
                    command.Parameters.AddWithValue("?p_LicenseId", licenseId);

                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        connection.Close();
                        return result.ToString();
                    }
                }
                connection.Close();
            }

            return string.Empty;
        }


        /// <summary>
        /// This method checks if Bulk Device Transfer is eligible
        /// Eligibility :
        ///    1. User is super user in atleast one account other than the logged in account
        ///    2. Logged in account has eligible devices for transfer
        /// </summary>
        /// <param name="supportAccountId">long</param>
        /// <param name="userEmail">Email</param>
        /// <param name="userAccountId">long</param>
        /// <returns></returns>
        public bool ShowBulkDeviceTransfer(long supportAccountId, long userAccountId)
        {
            var accounts = _deviceDataAccess.GetDestinationAccounts(userAccountId, supportAccountId);
            //if user is super user in more than one account
            if (accounts.Count >= 1)
            {
                //check if account is has eligible devices for transfer
                var result = _deviceDataAccess.GetEligibleDevicesForTransfer(supportAccountId, userAccountId);
                return result.Count > 0;
            }
            return false;
        }



        public Address GetDeviceAddressBySerialNumber(string serialNumber)
        {
            var device = _deviceDataAccess.GetDeviceDetailsBySerialNumber(serialNumber);

            var deviceAddress = new Address
            {
                Address1 = device.Address1,
                Address2 = device.Address2,
                City = device.City,
                Region = device.Region,
                Country = device.CountryName,
                PostalCode = device.PostalCode
            };

            return deviceAddress;
        }

        public bool GetUserPreferences(long userAccountId, long supportAccountId)
        {

            try
            {
                DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
                var result = deviceDataAccess.GetUserPreferences(userAccountId, supportAccountId);

                if (result.Any())
                {
                    var preferences = JsonConvert.DeserializeObject<Preferences>(result);
                    if (preferences != null)
                    {
                        var hideExpiredlicenses = preferences.HideExpiredLicenseses.SingleOrDefault(p => p.SupportAccountId == supportAccountId);
                        return hideExpiredlicenses != null && hideExpiredlicenses.Flag;

                    }
                }

                return false;
            }
            catch (Exception ex)
            {

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
        }

        public void UpdateHideExpiredLicensesFlag(long userAccountId, long supportAccountId, bool hideExpiredLicenses)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            var result = deviceDataAccess.GetUserPreferences(userAccountId, supportAccountId);

            var currentPreferences = JsonConvert.DeserializeObject<Preferences>(result);

            if (currentPreferences != null)  // preference already exists for user
            {
                if (currentPreferences.HideExpiredLicenseses != null)  //hideExpiredlicenses preference exists
                {
                    var isExists = currentPreferences.HideExpiredLicenseses.SingleOrDefault(p => p.SupportAccountId == supportAccountId);
                    if (isExists == null)  //hideExpiredLicenses preference doesn't exist for supportaccountid
                    {
                        var newhideExpiredLicenses = new HideExpiredLicenses()
                        {
                            Flag = hideExpiredLicenses,
                            SupportAccountId = supportAccountId
                        };
                        currentPreferences.HideExpiredLicenseses.Add(newhideExpiredLicenses);
                    }
                    else
                    {
                        foreach (HideExpiredLicenses item in currentPreferences.HideExpiredLicenseses)
                        {
                            if (item.SupportAccountId == supportAccountId)
                                item.Flag = hideExpiredLicenses;
                        }
                    }
                }
                else
                {
                    //create new hideExpiredLicenses and add to list
                    var hideExpiredlicenses = new HideExpiredLicenses()
                    {
                        Flag = hideExpiredLicenses,
                        SupportAccountId = supportAccountId
                    };

                    currentPreferences.HideExpiredLicenseses = new List<HideExpiredLicenses>() { hideExpiredlicenses };
                }

            }
            else
            {
                var hideExpiredlicenses = new HideExpiredLicenses()
                {
                    Flag = hideExpiredLicenses,
                    SupportAccountId = supportAccountId
                };

                //create preferences instance
                currentPreferences = new Preferences()
                {
                    HideExpiredLicenseses = new List<HideExpiredLicenses>() { hideExpiredlicenses }
                };
            }


            //call dataccess method to update json
            deviceDataAccess.UpdateHideExpiredLicensesFlag(userAccountId, supportAccountId,
                JsonConvert.SerializeObject(currentPreferences));

        }

        public bool IsProcessingCard(string serialNumber)
        {
            try
            {
                return _deviceDataAccess.IsProcessingCard(serialNumber);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new ExceptionHandlingException(ex.ToString()));
                return false;
            }
        }


        public bool IsK2Device(string serialNumber, out string mode)
        {
            try
            {
                return _deviceDataAccess.IsK2Device(serialNumber, out mode);
            }
            catch (Exception ex)
            {
                mode = string.Empty;
                Elmah.ErrorSignal.FromCurrentContext().Raise(new ExceptionHandlingException(ex.ToString()));
                return false;
            }
        }

        public string IsValidSerial(string serialNumber, long supportAccountId)
        {
            string message = CheckIsSerialNumberValidandNotRegisterd(serialNumber);

            if (!string.IsNullOrEmpty(message))
            {
                return message;
            }

            //Check if the device being registered is not a processing card
            var isProcessingCard = IsProcessingCard(serialNumber);
            if (isProcessingCard)
            {
                return "Please go to Line Cards/Optics/FRUs page to register processing cards.";
            }

            var isEvalDevice = _userAccountLogic.IsEvalDeviceCode(serialNumber);
            var isEvalSupportAccount = _userAccountLogic.IsEvalSupportAccount(supportAccountId);

            if (isEvalSupportAccount && !isEvalDevice)
            {
                return
                    "To register a new purchased asset, the Eval account must be converted to an active CSP account.  Please go to the Company Account page to activate your account.";
            }

            return string.Empty;
        }

        public string IsValidSerialNumber(string serialNumber, long supportAccountId, out bool isRegistered)
        {
            int isValid = CheckIfSerialNumberValidandNotRegisterd(serialNumber);

            string message = isValid == 1 ? ApiConstants.SerialNumberNotValid_Message : (isValid == 2 ? ApiConstants.SerialNumberRegsitered_Message : CheckIsSerialNumberEligible(serialNumber));
            isRegistered = isValid == 2;

            if (!string.IsNullOrEmpty(message))
            {
                return message;
            }

            //Check if the device being registered is not a processing card
            var isProcessingCard = IsProcessingCard(serialNumber);
            if (isProcessingCard)
            {
                return "Please go to Line Cards/Optics/FRUs page to register processing cards.";
            }

            var isEvalDevice = _userAccountLogic.IsEvalDeviceCode(serialNumber);
            var isEvalSupportAccount = _userAccountLogic.IsEvalSupportAccount(supportAccountId);

            if (isEvalSupportAccount && !isEvalDevice)
            {
                return
                    "To register a new purchased asset, the Eval account must be converted to an active CSP account.  Please go to the Company Account page to activate your account.";
            }

            return string.Empty;
        }

        public bool IsChassisSerialNumber(string serialNumber)
        {
            return _deviceDataAccess.IsChassisSerialNumber(serialNumber);
        }

        public string GetCountryCodeById(long countryId)
        {
            return _deviceDataAccess.GetCountryCodeById(countryId);
        }

        private int CheckIfSerialNumberValidandNotRegisterd(string serialNumber)
        {
            DeviceDataAccess deviceDataAccess = new DeviceDataAccess();
            return deviceDataAccess.CheckIsSerialNumberValidandNotRegisterd(serialNumber);
        }

        public bool IsValidDeviceForOtp(string serialNumber, long supportAccountId)
        {
            return _deviceDataAccess.IsValidDeviceForOtp(serialNumber, supportAccountId);
        }

        public IEnumerable<ActivationLinkModel> GetTrialLicenseActivationActiveLinksBySupportAccountId(long supportAccountId, string types, string serialNumber)
        {
            string sku = string.Empty;
            string productFamily = string.Empty;
            int grace = 0;
            int maxAllowed = 0;
            int firstIOTMaxAllowed = 0;
            string[] typeList = types.Split(',');
            ActivationLinkLicenseRequest firstIOTActivationlink = null;

            foreach (string type in typeList)
            {
                switch (type)
                {
                    case "IOTSecurity":
                        sku = "PAN-IOT-DRDL-TRIAL";
                        grace = 60;
                        productFamily = "IOT";
                        maxAllowed = 5;
                        break;
                    case "IOTSecurityDataLake":
                        sku = "PAN-IOT-TRIAL";
                        grace = 60;
                        productFamily = "IOT";
                        maxAllowed = 5;
                        break;
                    case "IOTSecurity3rd":
                        sku = "PAN-IOT-3P-ADDON-ADV-TRIAL";
                        grace = 60;
                        productFamily = "IOT";
                        //1000 indicates no maxAllowed is specified
                        maxAllowed = 1000;
                        break;
                    case "SAASINLINE":
                        sku = "PAN-SAAS-INLINE-TRIAL";
                        grace = 60;
                        productFamily = "SAAS";
                        maxAllowed = 5;
                        break;
                    case "AIOPS":
                        sku = "PAN-AIOPS-TRIAL";
                        grace = 90;
                        productFamily = null;
                        maxAllowed = 5;
                        break;
                    default:
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Invalid Type: {0}", type)));
                        break;
                }

                // If the parameter types has more than one string separated by a comma that means there are two IOT packages in the list
                if (typeList.Length > 1 && firstIOTActivationlink == null)
                {
                    firstIOTMaxAllowed = maxAllowed;
                    firstIOTActivationlink = new ActivationLinkLicenseRequest
                    {
                        Sku = sku,
                        Grace = grace,
                        Term = 0,
                        SaasQuantity = 0,
                        UseCount = 2
                    };
                }
            }

            //Reset IOT types to their original values which were modified for the SKU-s above.
            if (types.Contains("IOT"))
            {
                for (int i = 0; i < typeList.Length; i++)
                {
                    if (typeList[i].Contains("IOT"))
                    {
                        typeList[i] = "IOT";
                    }
                }
            }

            //Assign the license type parameter(lType) as the 1st item in the typeList array
            string lType = string.Empty;
            if (typeList != null & typeList.Length > 0)
            {
                lType = typeList[0];
            }

            string sfAccount = null;
            string email = HttpContext.Current.User.Identity.Name;

            // List of all unexpired requested trial links
            IEnumerable<ActivationLinkModel> allrequestedLicenses = _deviceDataAccess.GetTrialLicenseActivationActiveLinksBySupportAccountId(supportAccountId, lType, productFamily, grace, out sfAccount);
            // NOTE: if license was activated but already expired, the activation link is considered invalid.
            List<ActivationLinkModel> availableLicenses = allrequestedLicenses.Where(a => !a.LicenseExpiration.HasValue).ToList();

            // 1. # of unexpired and unactivated Activation links is 0; first time request for Active Link - generate Active Link and mail it
            // 2. # of unexpired and unactivated Activation links is less then 5; at least one Active Link was generated prior to this request - generate Active Link and mail it as well as all priors
            try
            {
                ActivationLinkRequest req = null;
                ActivationLinkLicenseRequest activationLink = new ActivationLinkLicenseRequest
                {
                    Sku = sku,
                    Grace = grace,
                    Term = 0,
                    SaasQuantity = 0,
                    UseCount = 2
                };

                //IOT requests handling
                if (activationLink.Sku.Contains("PAN-IOT"))
                {
                    // One magic link request for 2 IOT Packages.
                    if (firstIOTActivationlink != null)
                    {
                        req = new ActivationLinkRequest
                        {
                            SfAccountId = sfAccount,
                            SupportAccountId = supportAccountId,
                            SerialNumber = serialNumber,
                            UserEmail = email,
                            OrderType = "trial",
                            Licenses = new ActivationLinkLicenseRequest[] { firstIOTActivationlink, activationLink }
                        };
                        firstIOTActivationlink = null;
                    }
                    else
                    {
                        // One magic link request per IOT package.
                        req = new ActivationLinkRequest
                        {
                            SfAccountId = sfAccount,
                            SupportAccountId = supportAccountId,
                            SerialNumber = serialNumber,
                            UserEmail = email,
                            OrderType = "trial",
                            Licenses = new ActivationLinkLicenseRequest[] { activationLink }
                        };
                    }
                }
                //SaaS requests handling
                else if (activationLink.Sku.Contains("PAN-SAAS"))
                {
                    req = new ActivationLinkRequest
                    {
                        SfAccountId = sfAccount,
                        SupportAccountId = supportAccountId,
                        SerialNumber = serialNumber,
                        UserEmail = email,
                        OrderType = "trial",
                        Licenses = new ActivationLinkLicenseRequest[] { activationLink }
                    };
                }
                //request for those packages which don't require opportunity creation
                else
                {
                    req = new ActivationLinkRequest
                    {
                        SfAccountId = sfAccount,
                        OrderType = "trial",
                        Licenses = new ActivationLinkLicenseRequest[] { activationLink }
                    };
                }

                var newActiveLink = GetActivateTrialAuthCode<ActivationLinkRequest, ActivationLinkResponse>(req);
                if (!newActiveLink.IsValid())
                {
                    return null;
                }

                availableLicenses.InsertRange(0, newActiveLink.Licenses.Select(i => new ActivationLinkModel
                {
                    ActivationLink = i.ActivationLink,
                    ActivationLinkExpiration = string.IsNullOrWhiteSpace(i.ExpirationDate) ? DateTime.Now.AddDays(grace) : Convert.ToDateTime(i.ExpirationDate),
                    AuthorizationCode = i.AuthorizationCode,
                    LicenseExpiration = null,
                    LicenseId = 0,
                    LinkType = i.ProductSubGroup
                }).ToList());

                // Only return unique magic links for IOT(LDB return 2 identical links for some IOT requests)
                if (types.Contains("IOT") && availableLicenses.Count() > 1)
                    availableLicenses = availableLicenses.GroupBy(x => x.ActivationLink).Select(x => x.First()).ToList();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return null;
            }

            return availableLicenses;
        }

        private R GetActivateTrialAuthCode<T, R>(T requestPayload)
            where T : class
            where R : class
        {
            string requestData = JsonConvert.SerializeObject(requestPayload);
            HttpWebRequest request = null;

            // Use support account id 245 to get the License API Key for all support accounts.
            long supportAccountId = ApiConstants.PANW_SUPPORT_ACCOUNT_ID;
            string panServicesUserName = ConfigurationManager.AppSettings["PanServicesUserName"];
            string panServicesUserKey = ConfigurationManager.AppSettings["PanServicesUserKey"];
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(panServicesUserName + ":" + panServicesUserKey));

            //IOT trial request endpoint
            if (!string.IsNullOrEmpty(requestData) && (requestData.Contains("PAN-IOT") || requestData.Contains("PAN-SAAS")))
            {
                var panServiceCertThumbprint = ConfigurationManager.AppSettings["PanServicesCertThumbprint"];

                // If there's no PanServicesRestUrl key in the secrets file then use PanServicesUrl key
                string restUrl = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["PanServicesRestUrl"]) ? ConfigurationManager.AppSettings["PanServicesRestUrl"] : ConfigurationManager.AppSettings["PanServicesUrl"];
                string url = restUrl + "/api/EasyOnboarding/GenerateTrial";
                request = WebCommon.GetWebRequestWithClientCertificate(url, "POST", panServiceCertThumbprint, requestData);
                request.Headers["LicenseAPIKey"] = _iLicensingApiLogic.GetLicensingApiKey(supportAccountId).LicensingApiKey;
            }
            //Non-IOT trial request endpoint
            else
            {
                request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["PanServicesUrl"] + "/api/EasyOnboarding/GenerateOrder");
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = !string.IsNullOrEmpty(requestData) ? requestData.Length : 0;
                request.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                request.Headers["LicenseAPIKey"] = _iLicensingApiLogic.GetLicensingApiKey(supportAccountId).LicensingApiKey;
                using (StreamWriter requestWriter = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII))
                {
                    requestWriter.Write(requestData);
                    requestWriter.Close();
                }
            }

            using (WebResponse webResponse = request.GetResponse())
            {
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    using (StreamReader responseReader = new StreamReader(webStream))
                    {
                        return JsonConvert.DeserializeObject<R>(responseReader.ReadToEnd(), new JsonSerializerSettings()
                        {
                            MaxDepth = 32,
                            TypeNameHandling = TypeNameHandling.None
                        });
                    }
                }
            }
        }

        public bool IsNgfwDlpDevice(long deviceId)
        {
            return _deviceDataAccess.IsNgfwDlpDevice(deviceId);
        }

        public bool IsNgfwDlpDevice(string serialNumber)
        {
            return _deviceDataAccess.IsNgfwDlpDevice(serialNumber);
        }

        public string GetCountryNameByCode(string countryCode)
        {
            return _deviceDataAccess.GetCountryNameByCode(countryCode);
        }

        public bool IsSpareDevice(string serialNumber)
        {
            var modelName = _deviceDataAccess.GetModelNameBySerialNumber(serialNumber);
            return !string.IsNullOrEmpty(modelName) && IsModelNameSpare(modelName);
        }

    }
}
