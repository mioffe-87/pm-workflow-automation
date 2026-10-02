using System;
using System.Net;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model.Enums;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using Csp.Api.Model;
using Csp.Api.Logic;
using Csp.Api.Security;
using Csp.Api.HelperClasses;
using Csp.Api.Services.Contracts;
using Csp.Web.Security.Attributes;
using Csp.Web.HelperClasses;
using Csp.Web.DataTransferObjects;
using Csp.Web.Attributes;
using Csp.Api.Exceptions;
using AutoMapper;
using System.Configuration;
using Csp.Api.DataAccess;
using System.Data;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using Microsoft.Security.Application;
using Csp.Web.Filters;
using Newtonsoft.Json;

namespace Csp.Web.Controllers {

    internal static class BulkDeviceRegistrationDtoExtension {
        /// <summary>
        /// Helper method used to initialize a BulkDeviceRegistrationDto with values needed for processing and usage in a view
        /// </summary>
        /// <param name="bulkRegistrationDto">The bulk device registration data transfer object that is being extended</param>
        /// <param name="salesOrderLogic">A sales order logic interface used to get the sales orders</param>
        /// <returns></returns>
        internal static BulkDeviceRegistrationDto Initialize(this BulkDeviceRegistrationDto bulkRegistrationDto, ISalesOrderLogic salesOrderLogic) {
            var model = bulkRegistrationDto;

            //use the previously selected sales order ids to get the sales order in a list
            var salesOrders = salesOrderLogic.GetSalesOrders(bulkRegistrationDto.SelectedSalesOrders);

            //Create the action helper as this is not persisted in the model state
            model.ActionHelper = new ActionHelper(ControllerFields.DeviceController, model.SupportAccountId);

            //Get a new licensesDto, the data parametes are not persisted inthe model state and the data will be refreshed in the action methods
            model.AuthorizationCodes = new LicensesDto {
                DataParameters = new DataParameters { TotalRecordCount = 0, PaginationCount = 10 }
            };

            //Initialize the data parameters as this is not persisted in the model state
            model.DataParameters = new DataParameters { TotalRecordCount = 0, PaginationCount = 10 };

            //Map the list of sales order entities over to the data transfer object with objects that are meant to be used by the view 
            model.SalesOrders = Mapper.Map<List<SalesOrder>>(salesOrders);

            //Get the devices and licenses that are for the selected sales orders and filter by the default values of the DataParameters    
            var devices = salesOrderLogic.GetSalesOrderDevices(model.SelectedSalesOrders, model.DataParameters);
            var licenses = salesOrderLogic.GetSalesOrderLicenses(model.SelectedSalesOrders, model.AuthorizationCodes.DataParameters);

            //Map the license and devices over the objects used by the view.
            model.Devices = Mapper.Map<List<DeviceListItem>>(devices);
            model.AuthorizationCodes.Licenses = Mapper.Map<List<LicenseListItem>>(licenses);
            return model;
        }
    }

    public class DeviceController : BaseController {

        private readonly IDeviceTransferLogic _deviceTransferLogic;
        private readonly IBulkRegistrationThreadLogic _bulkRegistrationThreadLogic;
        private readonly ILicenseTransferLogic _licenseTransferLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly ISalesOrderLogic _salesOrderLogic;
        private readonly IGroupLogic _groupLogic;
        private readonly IAscDeviceRegistrationService _ascDeviceRegistrationService;
        private readonly IWildfireWipeoutService _wildfireWipeoutService;
        private readonly IEulaLogic _eulaLogic;
        private readonly IAwsLogic _awsLogic;
        private readonly ILookupLogic _lookupLogic;
        private readonly IBootstrapContainerLogic _bootstrapContainerLogic;
        private readonly ICloudDeviceLogic _cloudDeviceLogic;
        private readonly ISiteLicenseLogic _siteLicenseLogic;
        private readonly IIonLogic ionLogic = null;
        private readonly INetworkSecurityApiLogic _networkSecurityApiLogic;

        /// <summary>
        /// Creates a new BulkRegistrationDto with all of the values needed for proper processing 
        /// </summary>
        /// <param name="supportAccountId">The support account id that is not persisted</param>
        /// <returns>The newly created and initialized bulk registration dto</returns>
        private BulkDeviceRegistrationDto InstantiateBulkRegistrationDto(long supportAccountId) {
            return new BulkDeviceRegistrationDto {
                SupportAccountId = supportAccountId,
                Device = new Device(),
                SalesOrders = new List<SalesOrder>(),
                Devices = new List<DeviceListItem>(),
                DataParameters = new DataParameters { TotalRecordCount = 0, PaginationCount = 10 },
                ActionHelper = new ActionHelper(ControllerFields.DeviceController, supportAccountId),
                AuthorizationCodes = new LicensesDto {
                    DataParameters = new DataParameters { TotalRecordCount = 0, PaginationCount = 10 },
                    Licenses = new List<LicenseListItem>()
                }
            };
        }

        /// <summary>
        /// The bulk registration view has two different posts. This is one of the actions taken for the post.
        /// This gets the devices and serial numbers for the selected sales order and returns a list of the sales order
        /// to the view for the user to make a selection of what to register. 
        /// </summary>
        /// <param name="bulkRegistrationDto">This is the data transfer object from the view</param>
        /// <returns>This returns the action result with the updated data from the system</returns>
        private ActionResult AddSalesOrderForRegistration(BulkDeviceRegistrationDto bulkRegistrationDto) {
            var salesOrder = new SalesOrder();

            // this method initializes the bulk registration DTO with data that is needed to be sent to the view. 
            var model = bulkRegistrationDto.Initialize(_salesOrderLogic);

            //Clear the model state as all values (hidden fields) must be cleared to be populated from the post
            ModelState.Clear();

            //This returns a sales order for the search value and maps it to a object that the view can read
            if (!String.IsNullOrEmpty(model.SalesOrderNumberSearchValue)) {
                var salesOrderNumber = model.SalesOrderNumberSearchValue;

                if (salesOrderNumber.Length < 10) {
                    salesOrderNumber = salesOrderNumber.PadLeft(10, '0');
                }

                var salesOrderEntity = _salesOrderLogic.GetSalesOrder(salesOrderNumber);
                salesOrder = Mapper.Map<SalesOrder>(salesOrderEntity);
            }

            //If there is a sales order then try to add it to the model
            if (salesOrder.Id > 0) {

                //Make sure the model does not already have the sales order in the list
                if (model.SalesOrders.Select(item => item.Id).Where(id => id == salesOrder.Id).Count() == 0) {

                    //Add the sales order to the model
                    model.SalesOrders.Add(salesOrder);

                    //Add the sales order ids to a comma delimted list for the selection
                    model.SelectedSalesOrders = String.Join(",", model.SalesOrders.Select(item => item.Id.ToString()).ToArray());

                    //Get all of the devices and licenses for the selected sales orders, filtered by the values in the data parameters
                    var devices = _salesOrderLogic.GetSalesOrderDevices(model.SelectedSalesOrders, model.DataParameters);
                    var licenses = _salesOrderLogic.GetSalesOrderLicenses(model.SelectedSalesOrders, model.AuthorizationCodes.DataParameters);

                    //Perform mapping from the resulting entities to objects used in the data transfer object for the view. 
                    model.Devices = Mapper.Map<List<DeviceListItem>>(devices);
                    model.AuthorizationCodes.Licenses = Mapper.Map<List<LicenseListItem>>(licenses);
                }
            } else {
                //If there is not a sales order found for the SAP sales order number entered, then notify the user
                model.Notifications.Add(new ResponseNotification(String.Format("There is not a sales order with the sales order number '{0}'. Please try a different sales order number.",
                        bulkRegistrationDto.SalesOrderNumberSearchValue), ResponseState.Information));
            }

            //Return the model to the user so that the user may make a selection of the devices and licenses for bulk registration or to add another sales order
            return PartialView("../Partial/_BulkDeviceRegistration", model);
        }


        /// <summary>
        /// The bulk registration view has two different posts. This is one of the actions taken for the post.
        /// This performs the bulk registration for all of the selected devices and licenses on the form. 
        /// </summary>
        /// <param name="bulkRegistrationDto">The data transfer object with the data needed to perform the bulk registration</param>
        /// <returns>Returns the data, clean and fresh for the user to start performing a registration.</returns>
        private ActionResult BulkRegister(BulkDeviceRegistrationDto bulkRegistrationDto) {
            //Initialize the model with base information needed 
            var model = bulkRegistrationDto.Initialize(_salesOrderLogic);

            //Removes the validation for serial number from the modelstate. This is not appropriate here as multiple devices are registered. 
            ModelState.Remove("Device.SerialNumber");

            /*This is critical for this call, as device information and selected devices are only validated for one of the two posts. This server side validation is needed 
            as it can not be validated on the view*/
            if (!ModelState.IsValid)
                return PartialView("../Partial/_BulkDeviceRegistration", model);

            //This converts the comma delimted list of device and license Ids to an array for proper selection
            var deviceArray = (bulkRegistrationDto.SelectedDevices != null)
                                                        ? bulkRegistrationDto.SelectedDevices.Split(',')
                                                                                                 .Select(x => long.Parse(x))
                                                                                                 .ToArray()
                                                        : new long[0];
            var licenseArray = (bulkRegistrationDto.SelectedAuthorizationCodes != null)
                                                         ? bulkRegistrationDto.SelectedAuthorizationCodes.Split(',')
                                                                                                    .Select(x => long.Parse(x))
                                                                                                    .ToArray()
                                                         : new long[0];

            //This sets up basic information in the device for registration, some of which is only available in the controller like the email, for this reason we set it all up here. 
            bulkRegistrationDto.Device.Customer.Id = bulkRegistrationDto.SupportAccountId;
            bulkRegistrationDto.Device.LastUpdateByEmail = User.UserAccount.Email;

            /*This call spins up a separate thread after initialization to perform the device and license registration. This is 
                     * done in a separate thread as the calls are made on a web service. With a large list of devices and/or licenses selected,
                     * the registration could take a while to process. An email is sent to the user after the registration completes.
                     */
            _bulkRegistrationThreadLogic.StartBulkRegistration(deviceArray, licenseArray, bulkRegistrationDto.Device,
                                                                                                                 User.UserAccount.Id);

            //Clear the model state so that all hidden fields are empty and populated with values from a fresh DTO
            ModelState.Clear();

            //This instantiates a bulk registration data transfer object for the user to start over if other selects are to be made. The SupportAccountID must be passed as it was cleared with the modelstate
            model = InstantiateBulkRegistrationDto(bulkRegistrationDto.SupportAccountId);

            //Notify the user
            model.Notifications.Add(
                    new ResponseNotification("You will recieve an email notification when registration is complete.",
                                                                     ResponseState.Success));
            return PartialView("../Partial/_BulkDeviceRegistration", model);
        }


        public DeviceController(IDeviceTransferLogic deviceTransferLogic,
                                                        ISalesOrderLogic salesOrderLogic,
                                                        IBulkRegistrationThreadLogic bulkRegistrationThreadLogic,
                                                        ILicenseTransferLogic licenseTransferLogic,
                                                        IDeviceLogic deviceLogic, IGroupLogic groupLogic, IAscDeviceRegistrationService ascDeviceRegistrationService, IWildfireWipeoutService wildfireWipeoutService, IEulaLogic eulaLogic, IAwsLogic awsLogic, ILookupLogic ilookuplogic, IBootstrapContainerLogic bootstrapContainerLogic, ICloudDeviceLogic cloudDeviceLogic, ISiteLicenseLogic siteLicenseLogic, IIonLogic ionLogic, INetworkSecurityApiLogic networkSecurityApiLogic)
        {
            _deviceTransferLogic = deviceTransferLogic;
            _bulkRegistrationThreadLogic = bulkRegistrationThreadLogic;
            _licenseTransferLogic = licenseTransferLogic;
            _deviceLogic = deviceLogic;
            _groupLogic = groupLogic;
            _ascDeviceRegistrationService = ascDeviceRegistrationService;
            _wildfireWipeoutService = wildfireWipeoutService;
            _salesOrderLogic = salesOrderLogic;
            _eulaLogic = eulaLogic;
            _awsLogic = awsLogic;
            _lookupLogic = ilookuplogic;
            _bootstrapContainerLogic = bootstrapContainerLogic;
            _cloudDeviceLogic = cloudDeviceLogic;
            _siteLicenseLogic = siteLicenseLogic;
            this.ionLogic = ionLogic;
            _networkSecurityApiLogic = networkSecurityApiLogic;
        }

        private FeatureAccess _featureAccess = new FeatureAccess();

        [HttpGet, CspAuthorize(Permission.ViewDevice)]
        public ActionResult Details(long deviceId) {
            var model = new DeviceDto {
                Device = _deviceLogic.GetDevice(deviceId, User.UserAccount.Id),
                ActionHelper = new ActionHelper(ControllerFields.DeviceController, deviceId)
            };

            model.Device.Customer = CurrentState.GetSupportAccount();


            CurrentState.SetDevice(model.Device);
            return PartialView("../Partial/_DeviceDetails", model);
        }

        [JwtTokenAuthentication]
        [HttpGet, CspAuthorize(Permission.ViewDevice)]
        public JsonResult GetIonDeviceLightDetails(long id)
        {
            try
            {
                Device device = _deviceLogic.GetDevice(id, User.UserAccount.Id);
                return Json(device != null
                    ? new DeviceIonDetailsLightInfo
                    {
                        DeviceId = device.Id,
                        SerialNumber = device.SerialNumber,
                        Model = device.ModelName,
                        DeviceName = device.DeviceName,
                        TagId = device.DeviceCategoryId,
                        PanOsVersion = device.PanOsVersion,
                        Address1 = device.Address1,
                        Address2 = device.Address2,
                        City = device.City,
                        CountryId = device.CountryId,
                        Country = device.CountryName,
                        Region = device.Region,
                        PostalCode = device.PostalCode,
                        Comments = device.Comments,
                        Tags = _deviceLogic.GetAllDeviceCategories(CurrentState.GetCurrentAccountId(), User.UserAccount.Id).Select(i => new { Id = i.DeviceCategoryId, Name = i.DeviceCategoryName }).ToArray(),
                        ShowLicenseTransfer = GetIonDeviceTransferDetailsInternal(device).TransferEligibleDevices?.Any() ?? false,
                        TenantId = device.TenantId
                    }
                    : null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new System.Web.HttpException("Error of getting IONs.");
            }
        }

        [JwtTokenAuthentication]
        [HttpGet, CspAuthorize(Permission.ViewDevice)]
        public JsonResult GetIonDeviceTransferDetails(long id)
        {
            try
            {
                Device device = _deviceLogic.GetDevice(id, User.UserAccount.Id);
                return Json(device != null ? GetIonDeviceTransferDetailsInternal(device) : null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new System.Web.HttpException("Error of getting IONs.");
            }
        }

        private DeviceIonTransferDetails GetIonDeviceTransferDetailsInternal(Device device)
        {
            return ionLogic.GetIonDeviceTransferDetails(device);
        }

        /// <summary>
        /// This method is initially used to load _groups partial view when first time called
        /// </summary>
        /// <param name="deviceId">Device ID</param>
        /// <returns>Partail View</returns>
        [HttpGet, AjaxOnly, CspAuthorize(Permission.ViewDeviceGroups)]
        public ActionResult Groups(long deviceId) {
            var dataParameters = new DataParameters { PaginationCount = 10 };
            var groupListItems = GroupLogic.GetDeviceGroups(deviceId, User.UserAccount.Id, dataParameters);

            var model = new GroupsListDto {
                DataParameters = dataParameters,
                Groups = groupListItems,
                ActionHelper = new ActionHelper(ControllerFields.DeviceController, deviceId)
            };

            return PartialView("../Partial/_Groups", model);
        }

        /// <summary>
        /// This method is used for sorting and searching implementation.
        /// This method is called by Grid using Ajax method to load result in _groups partial view.
        /// </summary>
        /// <param name="request">Grid DataSourceRequest</param>
        /// <param name="id">Device ID</param>
        /// <returns></returns>
        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewDeviceGroups)]
        public ActionResult Groups(long id, [DataSourceRequest] DataSourceRequest request) {
            var sanitizedRequest = new DataSourceRequest();
            var filters = KendoHelper.SanitizeFilters(request.Filters);
            var sorts = KendoHelper.SanitizeSorts(request.Sorts);
            var groups = KendoHelper.SanitizeGroups(request.Groups);
            var aggregates = KendoHelper.SanitizeAggregates(request.Aggregates);
            sanitizedRequest.Aggregates = aggregates;
            sanitizedRequest.Filters = filters;
            sanitizedRequest.Groups = groups;
            sanitizedRequest.Page = request.Page;
            sanitizedRequest.PageSize = request.PageSize;
            sanitizedRequest.Sorts = sorts;
            var dataParameters = sanitizedRequest.ToDataParameters();
            var groupListItems = GroupLogic.GetDeviceGroups(id, User.UserAccount.Id, dataParameters);


            var result = new DataSourceResult {
                Data = groupListItems, // Process data (paging and sorting applied)
                Total = Convert.ToInt32(dataParameters.TotalRecordCount) // Total number of records
            };

            return Json(result);
        }


        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditDevice)]
        public ActionResult Licenses(long id, [DataSourceRequest] DataSourceRequest request) {
            var licenseListItem = LicenseLogic.GetDeviceLicenses(id);

            var result = new DataSourceResult {
                Data = licenseListItem
            };

            return Json(result);
        }


        /// <summary>
        /// This method is initially used to load _Memberships partial view when first time called
        /// </summary>
        /// <param name="id">Device ID</param>
        /// <returns>Partail View</returns>

        [HttpGet, AjaxOnly, CspAuthorize(Permission.ViewDeviceMemberships)]
        public ActionResult Users(long id) {
            var dataParameters = new DataParameters { PaginationCount = 10 };

            var membershipDto = new MembershipListDto {
                DataParameters = dataParameters,
                Memberships = Mapper.Map<List<SupportAccountMembershipModel>>(MembershipLogic.GetDeviceMembers(id, User.UserAccount.Id, dataParameters)),
                AvailableRoles =
                    SecurityLogic.GetAvailableRoles(RoleCategory.Device, CurrentState.GetCurrentAccountId(), User.UserAccount.Id)
                                             .ToList()
                                             .Select(r => new NameValue() { Name = r.Name, Value = r.Id.ToString() })
                                             .ToList(),
                MembershipTypeId = (int)MembershipType.Device,
                MembershipParentId = id,
                ActionHelper = new ActionHelper(ControllerFields.DeviceController, id)
            };

            return PartialView("../Partial/_Memberships", membershipDto);
        }


        /// <summary>
        /// This method is used for sorting and searching implementation.
        /// This method is called by Grid using Ajax method to load result in _Memberships partial view.
        /// </summary>
        /// <param name="request">Grid DataSourceRequest</param>
        /// <param name="id">Device ID</param>
        /// <returns></returns>
        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewDeviceMemberships)]
        public ActionResult Users(long id, [DataSourceRequest] DataSourceRequest request) {
            var dataParameters = request.ToDataParameters();

            var result = new DataSourceResult {
                Data = MembershipLogic.GetDeviceMembers(id, User.UserAccount.Id, dataParameters), // Process data (paging and sorting applied)
                Total = Convert.ToInt32(dataParameters.TotalRecordCount) // Total number of records
            };

            return Json(result);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        [HttpGet, ChildActionOnly, CspAuthorize(Permission.ViewDevice)]
        public ActionResult View(Device device) {
            var model = new DeviceDto {
                Device = device,
                ActionHelper = new ActionHelper(ControllerFields.DeviceController, device.Id)
            };
            return PartialView("../Partial/_ViewDevice", model);
        }

        private Device SanitizeInputObject(Device rawInput)
        {
            Device sanitizedDevice = new Device()
            {
                ApplyEsaLicenseToDevice = rawInput.ApplyEsaLicenseToDevice,
                BillToCountryId = rawInput.BillToCountryId,
                Contains4HRSupport = rawInput.Contains4HRSupport,
                CountryId = rawInput.CountryId,
                CreatedByUserAccountId = rawInput.CreatedByUserAccountId,
                CreatedOn = rawInput.CreatedOn,
                Customer = rawInput.Customer,
                DefaultRoles = rawInput.DefaultRoles,
                DeviceCategoryId = rawInput.DeviceCategoryId,
                GeneratePanoramaLicense = rawInput.GeneratePanoramaLicense,
                GracePeriodDays = rawInput.GracePeriodDays,
                Id = rawInput.Id,
                IsActive = rawInput.IsActive,
                IsAwsDevice = rawInput.IsAwsDevice,
                IsAzureDevice = rawInput.IsAzureDevice,
                IsCirroSecureDevice = rawInput.IsCirroSecureDevice,
                IsCloudDevice = rawInput.IsCloudDevice,
                IsCsspEndUserDeviceRegistrationSuccess = rawInput.IsCsspEndUserDeviceRegistrationSuccess,
                IsCsspProvisionedDevice = rawInput.IsCsspProvisionedDevice,
                IsEligibleForUpgrade = rawInput.IsEligibleForUpgrade,
                IsGoogleCloudDevice = rawInput.IsGoogleCloudDevice,
                IsPanorama = rawInput.IsPanorama,
                IsRMAReplacement = rawInput.IsRMAReplacement,
                IsRMAReturn = rawInput.IsRMAReturn,
                IsSaasStubDevice =rawInput.IsSaasStubDevice,
                IsSAPSynched = rawInput.IsSAPSynched,
                IsTraps = rawInput.IsTraps,
                IsTrapsManagementService = rawInput.IsTrapsManagementService,
                IsUpgradeComplete = rawInput.IsUpgradeComplete,
                IsUpgradeInProgress = rawInput.IsUpgradeInProgress,
                IsVirtualMachine = rawInput.IsVirtualMachine,
                IsCgx = rawInput.IsCgx,
                LastUpdate = rawInput.LastUpdate,
                LastUpdateByUserAccountId = rawInput.LastUpdateByUserAccountId,
                Licenses = rawInput.Licenses,
                LineCards = rawInput.LineCards,
                Members = rawInput.Members,
                RegistrationDate = rawInput.RegistrationDate,
                ShipDate = rawInput.ShipDate,
                ShipmentNumber = rawInput.ShipmentNumber,
                SupportAccountHasEsa = rawInput.SupportAccountHasEsa,
                SupportProvider = rawInput.SupportProvider,
                UserAccount = rawInput.UserAccount,
                VirtualSystemsAvailable = rawInput.VirtualSystemsAvailable,
                //Inputs that needed Sanitization.
                Address1 = Sanitizer.GetSafeHtmlFragment(rawInput.Address1),
                Address2 = Sanitizer.GetSafeHtmlFragment(rawInput.Address2),
                AwsCpuid = Sanitizer.GetSafeHtmlFragment(rawInput.AwsCpuid),
                AwsSerialNumber = Sanitizer.GetSafeHtmlFragment(rawInput.AwsSerialNumber),
                AwsUuid = Sanitizer.GetSafeHtmlFragment(rawInput.AwsUuid),
                AzureCpuid = Sanitizer.GetSafeHtmlFragment(rawInput.AzureCpuid),
                AzureProductCode = Sanitizer.GetSafeHtmlFragment(rawInput.AzureProductCode),
                AzureRegion = Sanitizer.GetSafeHtmlFragment(rawInput.AzureRegion),
                AzureSerialNumber = Sanitizer.GetSafeHtmlFragment(rawInput.AzureSerialNumber),
                AzureUuid = Sanitizer.GetSafeHtmlFragment(rawInput.AzureUuid),
                City = Sanitizer.GetSafeHtmlFragment(rawInput.City),
                Comments = Sanitizer.GetSafeHtmlFragment(rawInput.Comments),
                CommentsEncoded = Sanitizer.GetSafeHtmlFragment(rawInput.CommentsEncoded),
                CountryName = Sanitizer.GetSafeHtmlFragment(rawInput.CountryName),
                CpuId = Sanitizer.GetSafeHtmlFragment(rawInput.CpuId),
                CreatedByEmail = Sanitizer.GetSafeHtmlFragment(rawInput.CreatedByEmail),
                CsspSerialNumber = Sanitizer.GetSafeHtmlFragment(rawInput.CsspSerialNumber),
                CustomerAccountId = Sanitizer.GetSafeHtmlFragment(rawInput.CustomerAccountId),
                CustomerServiceNotes = Sanitizer.GetSafeHtmlFragment(rawInput.CustomerServiceNotes),
                DeviceCategoryName = Sanitizer.GetSafeHtmlFragment(rawInput.DeviceCategoryName),
                DeviceName = Sanitizer.GetSafeHtmlFragment(rawInput.DeviceName),
                DeviceNameEncoded = Sanitizer.GetSafeHtmlFragment(rawInput.DeviceNameEncoded),
                GoogleCloudCpuid = Sanitizer.GetSafeHtmlFragment(rawInput.GoogleCloudCpuid),
                GoogleCloudSerialNumber = Sanitizer.GetSafeHtmlFragment(rawInput.GoogleCloudSerialNumber),
                GoogleCloudUuid = Sanitizer.GetSafeHtmlFragment(rawInput.GoogleCloudUuid),
                HyperVisorVersion = Sanitizer.GetSafeHtmlFragment(rawInput.HyperVisorVersion),
                InstanceId = Sanitizer.GetSafeHtmlFragment(rawInput.InstanceId),
                LastUpdateByEmail = Sanitizer.GetSafeHtmlFragment(rawInput.LastUpdateByEmail),
                MacAddress = Sanitizer.GetSafeHtmlFragment(rawInput.MacAddress),
                ModelName = Sanitizer.GetSafeHtmlFragment(rawInput.ModelName),
                PanOsVersion = Sanitizer.GetSafeHtmlFragment(rawInput.PanOsVersion),
                PostalCode = Sanitizer.GetSafeHtmlFragment(rawInput.PostalCode),
                Region = Sanitizer.GetSafeHtmlFragment(rawInput.Region),
                ProductCode = Sanitizer.GetSafeHtmlFragment(rawInput.ProductCode),
                RegionText = Sanitizer.GetSafeHtmlFragment(rawInput.RegionText),
                SalesForceAccountId = Sanitizer.GetSafeHtmlFragment(rawInput.SalesForceAccountId),
                SerialNumber = Sanitizer.GetSafeHtmlFragment(rawInput.SerialNumber),
                TransactionId = Sanitizer.GetSafeHtmlFragment(rawInput.TransactionId),
                VmUpgradeAuthCode = Sanitizer.GetSafeHtmlFragment(rawInput.VmUpgradeAuthCode),
                UuId = Sanitizer.GetSafeHtmlFragment(rawInput.UuId),
                VmUpgradeFrom = Sanitizer.GetSafeHtmlFragment(rawInput.VmUpgradeFrom),
                VmUpgradeTo = Sanitizer.GetSafeHtmlFragment(rawInput.VmUpgradeTo),
                ZoneOrRegion = Sanitizer.GetSafeHtmlFragment(rawInput.ZoneOrRegion),
                TenantId = Sanitizer.GetSafeHtmlFragment(rawInput.TenantId)
            };
            
            return sanitizedDevice;
        }

        [HttpGet, ChildActionOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult Edit(Device device) {
            var deviceData = SanitizeInputObject(device);
            var actionHelper = new ActionHelper(ControllerFields.DeviceController, deviceData.Id, ActionHelper.EntityAction.Edit);
            var model = new DeviceDto {
                Device = deviceData,
                ActionHelper = actionHelper,
                DeviceCategories = _deviceLogic.GetAllDeviceCategories(CurrentState.GetCurrentAccountId(), User.UserAccount.Id),
            };
            model.Device.RegionText = model.Device.Region;
            var isSpare = DeviceLogic.IsModelNameSpare(deviceData.ModelName);
            ViewBag.IsValidForTransfer = !deviceData.IsCgx && (isSpare || _licenseTransferLogic.IsValidSourceForTransfer(
                                                                   deviceData.Id, User.UserAccount.Id,
                                                                   CurrentState.GetCurrentAccountId()));
            deviceData.Contains4HRSupport = _deviceLogic.Has4HRSupport(deviceData.Id);

            if (deviceData.IsTrapsManagementService || deviceData.IsCgx)
                ViewBag.ShowBootstrapContainerButton = false;
            else
                ViewBag.ShowBootstrapContainerButton = !deviceData.IsVirtualMachine;

            //Show/Hide Transfer Ownership Button
            ViewBag.ShowBeginDeviceTransferButton = _deviceTransferLogic.ShowBeginDeviceTransferButton(User.UserAccount, CurrentState.GetSupportAccountId().Value, deviceData, ViewBag.ShowBootstrapContainerButton);
           
            return PartialView("../Partial/_Device", model);
        }



        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditDevice)]
        public ActionResult Edit(DeviceDto deviceDto) {
            var actionHelper = new ActionHelper(ControllerFields.DeviceController, deviceDto.Device.Id, ActionHelper.EntityAction.Edit);

            var model = new DeviceDto {
                Device = SanitizeInputObject(deviceDto.Device),
                ActionHelper = actionHelper,
            };

            try {
                // Verify the user has access to edit this device by checking it belongs to their current support account
                var currentSupportAccountId = CurrentState.GetCurrentAccountId();
                var existingDevice = _deviceLogic.GetDevice(deviceDto.Device.Id, User.UserAccount.Id);
                if (existingDevice == null || existingDevice.Id == 0 || existingDevice.Customer?.Id != currentSupportAccountId)
                {
                    model.Notifications.Add(new ResponseNotification("You are not authorized to edit this device.", ResponseState.Error));
                    model.DeviceCategories = _deviceLogic.GetAllDeviceCategories(currentSupportAccountId, User.UserAccount.Id);
                    return PartialView("../Partial/_Device", model);
                }

                ModelState.Remove("Device.CountryId");
                ModelState.Remove("Device.Customer.Name");

                if (ModelState.IsValid) {
                    model.Device.Region = string.IsNullOrEmpty(model.Device.RegionText) ? model.Device.Region : model.Device.RegionText;
                    model.Device.LastUpdateByUserAccountId = User.UserAccount.Id;
                    model.Device = _deviceLogic.UpdateDevice(model.Device, User.UserAccount.Id);

                    var chassisComponent = new ChassisComponent()
                    {
                        ChassisSerialNumber = model.Device.SerialNumber,
                        Address1 = model.Device.Address1,
                        Address2 = model.Device.Address2,
                        City = model.Device.City,
                        PostalCode = model.Device.PostalCode,
                        CountryId = Convert.ToInt64(model.Device.CountryId),
                        Region = model.Device.Region
                    };
                    _deviceLogic.UpdateChassisLocation(chassisComponent);
                    model.Notifications.Add(new ResponseNotification("Device updated sucessfully", ResponseState.Success));
                    ModelState.Clear();
                } else {
                    var errors = ModelState.Select(x => x.Value.Errors)
                                                     .Where(y => y.Count > 0)
                                                     .ToList();
                }

            } catch (CspNotificationException exception) {

                model.Notifications.Add(new ResponseNotification(exception.Message, ResponseState.Error));
            }

            model.DeviceCategories = _deviceLogic.GetAllDeviceCategories(CurrentState.GetCurrentAccountId(), User.UserAccount.Id);
            model.Device.RegionText = model.Device.Region;
            return PartialView("../Partial/_Device", model);
        }

        [JwtTokenAuthentication]
        [HttpPut, CspAuthorize(Permission.EditDevice)]
        public JsonResult SaveIonDeviceLightDetails([System.Web.Http.FromBody] DeviceIonDetailsLightInfo details)
        {
            try
            {
                ionLogic.SaveIonDeviceLightDetails(details, User.UserAccount.Id);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new System.Web.HttpException("Error of getting IONs.");
            }

            return GetIonDeviceLightDetails(details.DeviceId);
        }

        [JwtTokenAuthentication]
        [HttpPut, CspAuthorize(Permission.EditDevice)]
        public EmptyResult TransferIonDeviceLicenses([System.Web.Http.FromBody] DeviceIonTransferLicenses transfer)
        {
            try
            {
                transfer.SourceSn = _deviceLogic.GetDevice(transfer.SourceDeviceId, User.UserAccount.Id).SerialNumber;
                transfer.DestinationSn = _deviceLogic.GetDevice(transfer.DestinationDeviceId, User.UserAccount.Id).SerialNumber;
                ionLogic.TransferIonDeviceLicenses(transfer, User.UserAccount.Id);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new System.Web.HttpException("Error of getting IONs.");
            }

            return new EmptyResult();
        }

        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditGroup)]
        public ActionResult UpdateGroupInline([DataSourceRequest]DataSourceRequest request, GroupListItem model) {
            var group = GroupLogic.GetGroup(model.GroupId, User.UserAccount.Id);

            group.Name = model.SupportAccountGroupName;
            group.Description = model.Description;
            group.LastUpdateByUserAccountId = User.UserAccount.Id;

            GroupLogic.UpdateGroupNameAndDescription(group, User.UserAccount.Id);

            var sanitizedRequest = new DataSourceRequest();
            var filters = KendoHelper.SanitizeFilters(request.Filters);
            var sorts = KendoHelper.SanitizeSorts(request.Sorts);
            var groups = KendoHelper.SanitizeGroups(request.Groups);
            var aggregates = KendoHelper.SanitizeAggregates(request.Aggregates);
            sanitizedRequest.Aggregates = aggregates;
            sanitizedRequest.Filters = filters;
            sanitizedRequest.Groups = groups;
            sanitizedRequest.Page = request.Page;
            sanitizedRequest.PageSize = request.PageSize;
            sanitizedRequest.Sorts = sorts;
            return Json(new[] { model }.ToDataSourceResult(sanitizedRequest, ModelState));
        }


        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditGroup)]
        public ActionResult GetAvailableDeviceSerialNumber(long groupId, bool isProd) {
            var dataParameters = new DataParameters();

            //Commented and added new line below with Select to minimze the size of JSON string
            //var deviceListItem = _deviceLogic.GetAvailableSerialNumber(id, User.UserAccount.Id, isProd, dataParameters);
            var deviceListItem = _deviceLogic.GetAvailableSerialNumber(groupId, User.UserAccount.Id, isProd, dataParameters).Select(item => new { DeviceId = item.DeviceId, SerialNumber = item.SerialNumber });

            return Json(deviceListItem, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditGroup)]
        public ActionResult GetDeviceCategoriesWithDeviceCount(long groupId, bool isProd)
        {
            var userAccountId = User.UserAccount.Id;
            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
            var dataParameters = new DataParameters();
            var deviceType = isProd ? "All" : "Spare";

            var deviceCategoryListItems = _deviceLogic.GetDeviceCategoriesWithDeviceCount(supportAccountId, userAccountId, groupId, deviceType);

            foreach (var item in deviceCategoryListItems)
            {
                item.DeviceCategoryName = string.Format("{0} ({1})", item.DeviceCategoryName, item.DeviceCount);
            }

            return Json(deviceCategoryListItems, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditGroup)]
        public ActionResult AddDeviceToGroup(long groupId, bool isProd, bool showSuccessMessage = false) {
            var model = new AddDeviceToGroupDto {
                ActionHelper = new ActionHelper(ControllerFields.DeviceController, groupId),
                GroupId = groupId,
                IsProduction = isProd,
                AddDeviceToGroupFilter = AddDeviceToGroupFilterTypes.SerialNumber
            };

            if (showSuccessMessage) {
                model.Notifications.Add(new ResponseNotification("Device(s) added to group", ResponseState.Success));
            }

            return PartialView("../Partial/_AddDeviceToGroup", model);
        }


        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditGroup)]
        public ActionResult AddDeviceToGroup(AddDeviceToGroupDto model) {
            ModelState.Remove(model.AddDeviceToGroupFilter == AddDeviceToGroupFilterTypes.SerialNumber
                ? "DeviceCategoryId"
                : "DeviceId");

            if (!ModelState.IsValid)
                return PartialView("../Partial/_AddDeviceToGroup", model);

            var currentSupportAccountId = CurrentState.GetCurrentAccountId();
            
            var group = _groupLogic.GetGroup(model.GroupId, User.UserAccount.Id);
            if (group == null || group.Id == 0 || group.SupportAccountId != currentSupportAccountId)
            {
                model.Notifications.Add(new ResponseNotification("You are not authorized to perform this action", ResponseState.Error));
                return PartialView("../Partial/_AddDeviceToGroup", model);
            }

            if (model.AddDeviceToGroupFilter == AddDeviceToGroupFilterTypes.SerialNumber)
            {
                var device = _deviceLogic.GetDevice(model.DeviceId.Value, User.UserAccount.Id);
                if (device == null || device.Id == 0 || device.Customer?.Id != currentSupportAccountId)
                {
                    model.Notifications.Add(new ResponseNotification("You are not authorized to perform this action", ResponseState.Error));
                    return PartialView("../Partial/_AddDeviceToGroup", model);
                }
                
                _groupLogic.CreateGroupDevice(model.GroupId, model.DeviceId.Value, User.UserAccount.Id);
            }
            else
            {
                var dataParameters = new DataParameters();
                var deviceCategories = _deviceLogic.GetDeviceCategories(currentSupportAccountId, User.UserAccount.Id, dataParameters);
                bool hasAccessToCategory = deviceCategories.Any(c => c.DeviceCategoryId == model.DeviceCategoryId);
                
                if (!hasAccessToCategory)
                {
                    model.Notifications.Add(new ResponseNotification("You are not authorized to perform this action", ResponseState.Error));
                    return PartialView("../Partial/_AddDeviceToGroup", model);
                }
                
                var deviceType = model.IsProduction ? "All" : "Spare";

                _groupLogic.AddDeviceCategoryDevicesToGroup(model.GroupId, model.DeviceCategoryId, User.UserAccount.Id, deviceType);
            }

            //return AddDeviceToGroup(model.GroupId, model.IsProduction, true);
            return RedirectToAction("AddDeviceToGroup", new { groupId = model.GroupId, isProd = model.IsProduction, showSuccessMessage = true });
        }


        [HttpGet, ChildActionOnly, CspAuthorize(Permission.BulkRegisterDevice)]
        public ActionResult BulkDeviceRegistration(long id) {
            var model = InstantiateBulkRegistrationDto(id);
            return PartialView("../Partial/_BulkDeviceRegistration", model);
        }

        [HttpGet, CspAuthorize(Permission.RegisterDevice)]
        public ActionResult BulkRegistration(long id) {
            return View(id);
        }

        /// <summary>
        /// This action method is used for the paging, sorting and searching of the device grid
        /// </summary>
        /// <param name="salesOrderIds">The Ids of the sales orders that hold the devices</param>
        /// <param name="request">The DataSourceRequest from kendo</param>
        /// <returns>Returns a Json result of the data for the kendo grid </returns>
        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.BulkRegisterDevice)]
        public ActionResult BulkRegistrationDevices(string salesOrderIds, [DataSourceRequest] DataSourceRequest request) {
            //Only perform the action if sales orders have been added. 
            if (string.IsNullOrEmpty(salesOrderIds))
                return Json(null);

            //Convert the request to dataparametes to pass the values to the database
            var dataParameters = request.ToDataParameters();

            //This gets the devices for the list of sales orders with the proper result set, sorting and page of data
            var devices = _salesOrderLogic.GetSalesOrderDevices(salesOrderIds, dataParameters);

            //This maps the list of device entities  to DeviceListItems to be displayed on the view
            var deviceListItems = Mapper.Map<IEnumerable<DeviceListItem>>(devices);

            var result = new DataSourceResult {
                Data = deviceListItems,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        /// <summary>
        /// This action method is used for the paging, sorting and searching of the license grid
        /// </summary>
        /// <param name="salesOrderIds">The Ids of the sales orders that hold the licenses</param>
        /// <param name="request">The DataSourceRequest from kendo</param>
        /// <returns>Returns a Json result of the data for the kendo grid</returns>
        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.BulkRegisterDevice)]
        public ActionResult BulkRegistrationLicenses(string salesOrderIds, [DataSourceRequest] DataSourceRequest request) {
            //Only perform the action if sales orders have been added. 
            if (string.IsNullOrEmpty(salesOrderIds))
                return Json(null);

            //Convert the request to dataparametes to pass the values to the database
            var dataParameters = request.ToDataParameters();

            //This gets the licenses for the list of sales orders with the proper result set, sorting and page of data
            var licenses = _salesOrderLogic.GetSalesOrderLicenses(salesOrderIds, dataParameters);

            //This maps the license entities to license list items to be displayed on the view
            var licenseListItems = Mapper.Map<IEnumerable<LicenseListItem>>(licenses);

            var result = new DataSourceResult {
                Data = licenseListItems,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        /// <summary>
        /// This action handles a dual post. If the StartRegistration value is set in the interface then the post
        /// performs the bulk registration. Otherwise the action will add the sales order for a registration.
        /// </summary>
        /// <param name="bulkRegistrationDto">The DTO that is passed from the view</param>
        /// <returns>Returns the page as a ajax post back</returns>
        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.BulkRegisterDevice)]
        public ActionResult BulkDeviceRegistration(BulkDeviceRegistrationDto bulkRegistrationDto) {
            //If the hidden field in the form is set to start the registration by the correct submit button the bulk registration is performed.   
            if (bulkRegistrationDto.StartRegistration) {
                return BulkRegister(bulkRegistrationDto);
            }  else {
                //Otherwise simply add the sales order, devices and licenses to the list. 
                return AddSalesOrderForRegistration(bulkRegistrationDto);
            }
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.RegisterDevice)]
        public ActionResult DeviceType(long id, long groupId = 0) {
            var model = new DeviceTypeDto() {
                SupportAccountId = id,
                DeviceType = (int)DeviceTypes.PanDevice
            };
            model.CloudTypes = _cloudDeviceLogic.GetCloudTypes();
            return PartialView("../Partial/_DeviceType", model);
        }

        [HttpGet, CspAuthorize(Permission.RegisterDevice)]
        public ActionResult RegisterDevice(long id, bool isCloudDevice = false, int cloudType = 0, long groupId = 0) {
            ViewBag.Message = "";
            ViewBag.UserAccountId = User.UserAccount.Id;

            var model = new DeviceRegistrationDto {
                IsAscCustomerRegistration = false,
                Device = new Device() { IsCloudDevice = false },
                DeviceCategories = _deviceLogic.GetAllDeviceCategories(id, User.UserAccount.Id),
                ActionHelper = new ActionHelper(ControllerFields.DeviceController),
                GroupId = groupId
            };
            model.Device.Customer.Id = id;

            #region Bind Eula to model
            model.Eula = new EulaModel() {
                Languages = _eulaLogic.GetEulaLanguages(),
                Eulas = _eulaLogic.GetEula(),
                DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.RegisterDevice;
            #endregion

            if (isCloudDevice)
            {
                model.Device.IsCloudDevice = true;
                model.Device.AzureRegion = null;
                //model.CloudRegions = _cloudDeviceLogic.GetCloudRegions(cloudType).ToList();
                // model.CloudProducts = _cloudDeviceLogic.GetCloudProducts(cloudType).ToList();

                switch (cloudType)
                {
                    case 1:
                        model.Device.IsAwsDevice = true;
                        break;
                    case 2:
                        model.Device.IsAzureDevice = true;
                        break;
                    case 3:
                        model.Device.IsCsspProvisionedDevice = true;
                        break;
                    case 5:
                        model.Device.IsGoogleCloudDevice = true;
                        break;
                }

            }
            else
            {
                model.Device.SupportAccountHasEsa = _siteLicenseLogic.HasEsaLicense(id);
                model.Device.ApplyEsaLicenseToDevice = null;
                model.DeviceEsaOptions = new List<DeviceRegistrationEsaOptions>()
                {
                    new DeviceRegistrationEsaOptions(){EsaOptionId = 1, EsaOptionText = "Yes"},
                    new DeviceRegistrationEsaOptions(){EsaOptionId = 2, EsaOptionText = "No"}
                };
            }
            return PartialView("../Partial/_DeviceRegistration", model);
        }


        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.RegisterDevice)]
        public ActionResult RegisterDevice(DeviceRegistrationDto deviceRegistrationDto) {
            ViewBag.UserAccountId = User.UserAccount.Id;
            var model = new DeviceRegistrationDto {
                ActionHelper = new ActionHelper(ControllerFields.DeviceController),
                Device = deviceRegistrationDto.Device,
                CountryName = deviceRegistrationDto.CountryName,
            };
            model.Device.Region = string.IsNullOrEmpty(model.Device.RegionText) ? model.Device.Region : model.Device.RegionText;
            if (model.Device != null && model.Device.CountryId != null) {
                model.CountryName = _lookupLogic.GetCountryCodeById(model.Device.CountryId.Value);
            }

            if (model.Device.IsAwsDevice || model.Device.IsAzureDevice || model.Device.IsGoogleCloudDevice || model.Device.IsCsspProvisionedDevice)
            {
                model.Device.IsCloudDevice = true;
                ModelState.Remove("Device.SerialNumber");
            } else {
                ModelState.Remove("Device.PanOs");
                ModelState.Remove("Device.AzureSerialNumber");
                ModelState.Remove("Device.AzureCpuid");
                ModelState.Remove("Device.AzureUuid");
                ModelState.Remove("Device.AwsSerialNumber");
                ModelState.Remove("Device.AwsCpuid");
                ModelState.Remove("Device.GoogleCloudSerialNumber");
                ModelState.Remove("Device.GoogleCloudCpuid");
                ModelState.Remove("Device.GoogleCloudUuid");
                ModelState.Remove("Device.CsspSerialNumber");
                ModelState.Remove("Device.CustomerAccountId"); // CSSP Customer Reference Id
            }

            string serialNumber = model.Device.SerialNumber;
            if (model.Device.IsTraps)
            {
                ModelState.Remove("Device.Address1");
                ModelState.Remove("Device.Address2");
                ModelState.Remove("Device.City");
                ModelState.Remove("Device.PostalCode");
                ModelState.Remove("Device.CountryId");
                ModelState.Remove("Device.PanOs");
            }
            // Panorama offline device license management: Location information is not required.
            RegisterVmDto panoramaParams = null;
            if (model.Device.IsPanorama)
            {
                ModelState.Remove("Device.Address1");
                ModelState.Remove("Device.Address2");
                ModelState.Remove("Device.City");
                ModelState.Remove("Device.PostalCode");
                ModelState.Remove("Device.CountryId");
                if (model.Device.GeneratePanoramaLicense)
                {
                    panoramaParams = new RegisterVmDto
                    {
                        UuId = string.IsNullOrEmpty(model.Device.UuId) ? string.Empty : model.Device.UuId,
                        CpuId = string.IsNullOrEmpty(model.Device.CpuId) ? string.Empty : model.Device.CpuId,
                        AuthCode = string.Empty,
                        PanOsVersion = string.IsNullOrEmpty(model.Device.PanOsVersion) ? string.Empty : model.Device.PanOsVersion,
                        HyperVisorVersion = string.IsNullOrEmpty(model.Device.HyperVisorVersion) ? string.Empty : model.Device.HyperVisorVersion
                    };
                }
            }

            if (string.IsNullOrEmpty(deviceRegistrationDto.AscCustBklnSupportAuthCode)) {
                ModelState.Remove("AscCustBklnSupportAuthCode");
            }

            ModelState.Remove("Device.Customer.Name");

            string cloudName = "AWS";
            if (model.Device.IsCloudDevice && model.Device.IsGoogleCloudDevice)
                cloudName = "Google Cloud";
            try {
                if (ModelState.IsValid)
                {
                    bool isSiteLicenseGranted;
                    var group = _groupLogic.GetGroup(deviceRegistrationDto.GroupId,
                        ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);

                    if (model.Device.IsCloudDevice)
                    {
                        if (string.IsNullOrEmpty(model.Device.HyperVisorVersion))
                        {
                            if (model.Device.IsAwsDevice)
                                model.Device.HyperVisorVersion = ApiConstants.HYPERVISOR_VERSION_AWS;
                            else if (model.Device.IsGoogleCloudDevice)
                                model.Device.HyperVisorVersion = ApiConstants.HYPERVISOR_VERSION_GOOGLE;
                            else if (model.Device.IsAzureDevice)
                                model.Device.HyperVisorVersion = ApiConstants.HYPERVISOR_VEERSION_AZURE;
                        }
                        if (model.Device.IsAwsDevice || model.Device.IsGoogleCloudDevice)
                        {
                            model.Device = _deviceLogic.RegisterDevice(model.Device, model.CountryName, User.UserAccount.Id,
                                out isSiteLicenseGranted, group, deviceRegistrationDto.AscCustBklnSupportAuthCode, false);

                            if (model.Device != null && model.Device.Id != 0)
                            {
                                Elmah.ErrorSignal.FromCurrentContext()
                                    .Raise(
                                        new Exception(
                                            string.Format(
                                                "Successfully Registered {0} Instance \"{1}\" to Support Account \"{2}\" by User \"{3}\"",
                                                cloudName, model.Device.SerialNumber, model.Device.Customer.Id, User.UserAccount.Id)));
                            }

                            if (model.Device != null)
                            {
                                if (model.Device.IsAwsDevice)
                                {
                                    model.Device.AwsSerialNumber = "";
                                    model.Device.AwsCpuid = "";
                                    model.Device.AwsUuid = "";
                                    model.Device.DeviceName = "";
                                }
                                else
                                {
                                    model.Device.GoogleCloudSerialNumber = "";
                                    model.Device.GoogleCloudCpuid = "";
                                    model.Device.GoogleCloudUuid = "";
                                    model.Device.DeviceName = "";
                                }
                                ModelState.Clear();
                            }
                        }
                        else if (model.Device.IsAzureDevice)
                        {
                            model.Device = _deviceLogic.RegisterDevice(model.Device, model.CountryName,
                                User.UserAccount.Id, out isSiteLicenseGranted, group,
                                deviceRegistrationDto.AscCustBklnSupportAuthCode, false);

                            if (model.Device != null && model.Device.Id != 0)
                            {
                                Elmah.ErrorSignal.FromCurrentContext()
                                    .Raise(
                                        new Exception(
                                            string.Format(
                                                "Successfully Registered Azure Instance \"{0}\" to Support Account \"{1}\" by User \"{2}\"",
                                                model.Device.InstanceId, model.Device.Customer.Id, User.UserAccount.Id)));
                            }

                            if (model.Device != null)
                            {
                                model.Device.AzureSerialNumber = "";
                                model.Device.AzureCpuid = "";
                                model.Device.AwsUuid = "";
                                model.Device.DeviceName = "";
                                ModelState.Clear();
                            }
                        }
                        else
                        {
                            // CSSP Device Transfer.
                            model.Device.LastUpdateByEmail = User.UserAccount.Email;
                            model.Device.LastUpdateByUserAccountId = User.UserAccount.Id;
                            model.Device = _deviceLogic.RegisterDevice(model.Device, model.CountryName, User.UserAccount.Id,
                                out isSiteLicenseGranted, null, null, false);
                            isSiteLicenseGranted = true; // Actually its not required to apply site lics for End User devices.
                            model.Device.CsspSerialNumber = "";
                            model.Device.DeviceName = "";
                            ModelState.Clear();
                        }

                    }
                    else
                    {
                        model.Device.LastUpdateByEmail = User.UserAccount.Email;
                        model.Device.LastUpdateByUserAccountId = User.UserAccount.Id;
                        model.Device = _deviceLogic.RegisterDevice(model.Device, model.CountryName, User.UserAccount.Id,
                                out isSiteLicenseGranted, group,
                                deviceRegistrationDto.AscCustBklnSupportAuthCode, false);

                        // Check if the current support account is a AWS Support account
                        // If yes, grab the sapid, sfdcid from the current device, salesorderitem, salesorder
                        
												// Comments By LK On 02/08/2020
												// Disable the logic to update SalesforceAccountId programatically
												//_deviceLogic.UpdateSapIdSfdcIdForAwsSupportAccount(model.Device.Customer.Id, model.Device.Id,
                        //    User.UserAccount.Id);

                        model.Device.SerialNumber = "";
                        model.Device.DeviceName = "";
                        ModelState.Clear();
                    } // End of device specific code.

                    if (!isSiteLicenseGranted)
                    {
                        model.Notifications.Add(new ResponseNotification("The system was unable to grant a site license.",
                            ResponseState.Warning));
                        Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(
                                new Exception(string.Format("Grant Site License Failed! SN: {0} UserAccountId: {1}",
                                    model.Device.SerialNumber, model.Device.Customer.Id)));
                    }

                    var deviceRegistrationSuccessMsg = "Device was successfully registered.";
                    if (!string.IsNullOrEmpty(deviceRegistrationDto.AscCustBklnSupportAuthCode))
                    {
                        var customerSupportAccountId = model.Device.Customer.Id;
                        var customerSupportAccountName =
                            _ascDeviceRegistrationService.GetSupportAccountNameBySupportAccountId(customerSupportAccountId);
                        deviceRegistrationSuccessMsg = string.Format("Device was successfully registered to {0} ({1}).",
                            customerSupportAccountName, customerSupportAccountId);
                    }

                    if (panoramaParams != null)
                    {
                        var result = LicenseLogic.GeneratePanoramaDeviceMgtLicense(serialNumber, panoramaParams.CpuId, panoramaParams.UuId, panoramaParams.HyperVisorVersion, panoramaParams.PanOsVersion);
                        if (!result)
                        {
                            deviceRegistrationSuccessMsg = string.Format("{0}, failed to activate Panorama device management license.", deviceRegistrationSuccessMsg);
                        }
                    }

                    // Update agreements
                    #region Store Eula Agreements

                    var ea = new EulaAgreements()
                    {
                        DeviceId = model.Device.Id,
                        EulaContextId = EulaContext.RegisterDevice,
                        EulaId = deviceRegistrationDto.Eula.SelectedEulaId,
                        SupportAccountId = model.Device.Customer.Id,
                        SignedBy = User.UserAccount.Email,
                        Notes = string.Format("SerialNumber entered is {0}", serialNumber)
                    };

                    try
                    {
                        _eulaLogic.EndUserAgreements(ea);
                    }
                    catch (Exception)
                    {
                        Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(
                                new Exception(
                                    string.Format("Could not add EULA Agreements for Device {0}. Eula Id Selected is - {1}",
                                        model.Device.Id, deviceRegistrationDto.Eula.DefaultEula.Id)));
                    }

                    #endregion

                    model.Notifications.Add(new ResponseNotification(deviceRegistrationSuccessMsg, ResponseState.Success));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                }
            } catch (CspNotificationException exception) {
                model.Notifications.Add(new ResponseNotification(exception.Message, ResponseState.Error));
            }

            model.DeviceCategories = _deviceLogic.GetAllDeviceCategories(CurrentState.GetCurrentAccountId(), User.UserAccount.Id);

            #region Bind Eula Model again

            model.Eula = new EulaModel() {
                Languages = _eulaLogic.GetEulaLanguages(),
                Eulas = _eulaLogic.GetEula(),
                DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.RegisterDevice;
            #endregion

            return PartialView("../Partial/_DeviceRegistration", model);
        }

        [Obsolete]
        private bool ExtractAwsCredentialFromSerialNumberAndCpuid(DeviceRegistrationDto model)
        {
            // Get AWS Instance ID, Region and Product from user input.
            // Populate device
            /* 
             * Longer InstanceId
             * Instance Id: i-09f2c8cee499ddf37
             * Cpu ID: AWSMP:09F2C8CEE499DDF37:6KXDW3BBMDEDA3O6I1GGQT4KM:US-EAST-1
             * SerialNumber: 47679FEAF1E36D4
             */
            /*
             * Shorter InstanceID
             * Instance Id: I-E3740D7A
             * Cpu Id: AWSMKT:806J2OF0QY5OSGJJIXQ9GQC6G:US-EAST-1
             * Serial Number: AWSMKTIE3740D7A
             */
            bool isValidAwsInput = true;
            try
            {
                string cCpuId = model.Device.AwsCpuid;
                string cSerialNumber = model.Device.AwsSerialNumber;

                if (cSerialNumber.StartsWith("4") || cSerialNumber.StartsWith("AWSMKT"))
                {
                    if (cCpuId.Contains("AWSMP:")) // Longer Instance ID
                    {
                        var aCpuId = cCpuId.Split(':');
                        if (aCpuId.Length != 4)
                        {
                            isValidAwsInput = false;
                            model.Notifications.Add(new ResponseNotification("Invalid CPU ID", ResponseState.Error));
                        }
                        else
                        {
                            model.Device.InstanceId = string.Concat("i-", aCpuId[1].ToLower());
                            model.Device.ZoneOrRegion = aCpuId[3].ToLower();
                            model.Device.ProductCode = aCpuId[2].ToLower();
                        }
                    }
                    else if (cCpuId.Contains("AWSMKT:"))
                    {
                        var aCpuId = cCpuId.Split(':');
                        if (aCpuId.Length != 3)
                        {
                            isValidAwsInput = false;
                            model.Notifications.Add(new ResponseNotification("Invalid CPU ID", ResponseState.Error));
                        }
                        else
                        {
                            model.Device.InstanceId = cSerialNumber.ToLower().Replace("awsmkti", "i-");
                            model.Device.ZoneOrRegion = aCpuId[2].ToLower();
                            model.Device.ProductCode = aCpuId[1].ToLower();
                        }
                    }
                    else
                    {
                        isValidAwsInput = false;
                        model.Notifications.Add(new ResponseNotification("Invalid CPU ID", ResponseState.Error));
                    }
                }
                else
                {
                    isValidAwsInput = false;
                    model.Notifications.Add(new ResponseNotification("Invalid Serial Number", ResponseState.Error));
                }
            }
            catch (Exception)
            {
                isValidAwsInput = false;
            }

            if (isValidAwsInput &&
                (string.IsNullOrEmpty(model.Device.InstanceId) || string.IsNullOrEmpty(model.Device.ProductCode) ||
                 string.IsNullOrEmpty(model.Device.ZoneOrRegion)) && model.Notifications.Count == 0)
            {
                isValidAwsInput = false;
                model.Notifications.Add(new ResponseNotification("Invalid AWS Input", ResponseState.Error));
            }

            return isValidAwsInput;
        }

        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewDevice)]
        public ActionResult GetDeviceLicenses(long deviceId, [DataSourceRequest] DataSourceRequest request) {
            var dataParameters = request.ToDataParameters();

            var model = LicenseLogic.GetDeviceLicenses(deviceId);

            var result = new DataSourceResult {
                Data = model, // Process data (paging and sorting applied)
                Total = Convert.ToInt32(dataParameters.TotalRecordCount) // Total number of records
            };

            return Json(result);
        }

        /// <summary>
        /// This method is initially used to load DeviceTags partial view when first time called
        /// </summary>
        [HttpGet, CspAuthorize(Permission.EditDeviceTags)]
        public ActionResult DeviceCategories(long id = 0) {
            var userAccountId = User.UserAccount.Id;
            long supportAccountId = CurrentState.GetCurrentAccountId();
            var dataParameters = new DataParameters { PaginationCount = 10 };
            var currentUser = (CspPrincipal)User;
            var isSupportAccountAdmin = currentUser.RoleSecurities.IsSupportAccountAdmin(supportAccountId);

            var deviceCategoryListDto = new DeviceCategoryListDto {
                DataParameters = dataParameters,
                ActionHelper = new ActionHelper(ControllerFields.DeviceController),
                DeviceCategories = _deviceLogic.GetDeviceCategories(supportAccountId, userAccountId, dataParameters),
                IsSupportAccountAdmin = isSupportAccountAdmin
            };

            return View(deviceCategoryListDto);
        }

        /// <summary>
        /// This method is used for sorting and searching implementation.
        /// This method is called by Grid using Ajax method to load results in DeviceCategories partial view.
        /// </summary>
        /// <param name="request">Grid DataSourceRequest</param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.EditDeviceTags)]
        public ActionResult DeviceCategories([DataSourceRequest] DataSourceRequest request) {
           // var dataParameters = new DataParameters { PaginationCount = 10 };
            var userAccountId = User.UserAccount.Id;
            long supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
            var sanitizedRequest = new DataSourceRequest();
            var filters = KendoHelper.SanitizeFilters(request.Filters);
            var sorts = KendoHelper.SanitizeSorts(request.Sorts);
            var groups = KendoHelper.SanitizeGroups(request.Groups);
            var aggregates = KendoHelper.SanitizeAggregates(request.Aggregates);
            sanitizedRequest.Aggregates = aggregates;
            sanitizedRequest.Filters = filters;
            sanitizedRequest.Groups = groups;
            sanitizedRequest.Page = request.Page;
            sanitizedRequest.PageSize = request.PageSize;
            sanitizedRequest.Sorts = sorts;
            var dataParameters = sanitizedRequest.ToDataParameters();
            dataParameters.PaginationCount = 10;
            var deviceCategoryListItems = _deviceLogic.GetDeviceCategories(supportAccountId, userAccountId, dataParameters);

            var result = new DataSourceResult {
                Data = deviceCategoryListItems,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditDeviceTags)]
        public ActionResult AddUpdateDeviceCategory(long? deviceCategoryId) {
            var deviceCategory = new DeviceCategory();
            var currentSupportAccountId = CurrentState.GetCurrentAccountId();
            
            if (deviceCategoryId.HasValue)
            {
                var dataParameters = new DataParameters();
                List<DeviceCategoryListItem> deviceCategoryListItems = _deviceLogic.GetDeviceCategories(currentSupportAccountId, User.UserAccount.Id, dataParameters);
                bool hasAccess = deviceCategoryListItems.Any(c => c.DeviceCategoryId == deviceCategoryId.Value);
                
                if (hasAccess)
                {
                    deviceCategory = _deviceLogic.GetDeviceCategory(deviceCategoryId.Value);
                    
                    if (deviceCategory == null || deviceCategory.SupportAccountId != currentSupportAccountId)
                    {
                        deviceCategory = new DeviceCategory();
                    }
                }
            }
            
            var model = new DeviceCategoryDto {
                ActionHelper = new ActionHelper(ControllerFields.DeviceController),
                DeviceCategory = deviceCategory
            };

            return PartialView("../Partial/_AddUpdateDeviceCategory", model);
        }

        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditDeviceTags)]
        public ActionResult AddUpdateDeviceCategory(DeviceCategoryDto model) {

						// Comments By LK On 08/02
						// Implement fix for XSS attacks
						if (model.DeviceCategory != null)
						{
							model.DeviceCategory.DeviceCategoryName = Sanitizer.GetSafeHtmlFragment(model.DeviceCategory.DeviceCategoryName);
							model.DeviceCategory.Description = Sanitizer.GetSafeHtmlFragment(model.DeviceCategory.Description);
						}

						string message;
            var deviceCategory = model.DeviceCategory;
            var isAddNew = (deviceCategory != null && deviceCategory.DeviceCategoryId == 0);
            var supportAccountId = CurrentState.GetCurrentAccountId();

            try {
                if (ModelState.IsValid && deviceCategory != null) {
                    if (_deviceLogic.IsDeviceCategoryExists(deviceCategory.DeviceCategoryName.Trim(), supportAccountId, deviceCategory.DeviceCategoryId)) {
                        model.Notifications.Add(new ResponseNotification(ApiConstants.DEVICE_CATEGORY + " Name already exists. Please enter a unique name.", ResponseState.Error));
                    } else {
                        deviceCategory.SupportAccountId = supportAccountId;
                        deviceCategory.UserAccountId = User.UserAccount.Id;

                        if (isAddNew) {
                            deviceCategory.IsActive = true;
                            deviceCategory.IsPublic = true;
                            deviceCategory.CreatedOn = DateTime.Now;
                            deviceCategory.CreatedByUserAccountId = User.UserAccount.Id;

                            deviceCategory = _deviceLogic.CreateDeviceCategory(deviceCategory);
                            deviceCategory = new DeviceCategory();
                            message = string.Format(ApiConstants.DEVICE_CATEGORY + " has been {0} successfully.",
                                (isAddNew) ? "created" : "updated");
                            model.Notifications.Add(new ResponseNotification(message, ResponseState.Success));
                        }
                        else
                        {
                            var dataParameters = new DataParameters();
                            List<DeviceCategoryListItem> deviceCategoryListItems = _deviceLogic.GetDeviceCategories(supportAccountId, User.UserAccount.Id, dataParameters);
                            bool hasAccess = deviceCategoryListItems.Any(c => c.DeviceCategoryId == deviceCategory.DeviceCategoryId);

                            if (!hasAccess)
                            {
                                message = "You are not authorized to perform this action";
                                model.Notifications.Add(new ResponseNotification(message, ResponseState.Success));
                            }
                            else
                            {
                                deviceCategory.IsActive = true;
                                deviceCategory.IsPublic = true;
                                deviceCategory.LastUpdate = DateTime.Now;
                                deviceCategory.LastUpdateByUserAccountId = User.UserAccount.Id;

                                deviceCategory = _deviceLogic.UpdateDeviceCategory(deviceCategory);
                                message = string.Format(ApiConstants.DEVICE_CATEGORY + " has been {0} successfully.",
                                    (isAddNew) ? "created" : "updated");
                                model.Notifications.Add(new ResponseNotification(message, ResponseState.Success));
                            }
                        }
                    }
                }
            } catch (Exception exception) {
                Elmah.ErrorSignal.FromCurrentContext().Raise(exception);
                message = string.Format("Failed to {0} " + ApiConstants.DEVICE_CATEGORY + ".", (isAddNew) ? "create" : "update");
                model.Notifications.Add(new ResponseNotification(message, ResponseState.Error));
            }

            ModelState.Clear();
            model.ActionHelper = new ActionHelper(ControllerFields.DeviceController);
            model.DeviceCategory = deviceCategory;

            return PartialView("../Partial/_AddUpdateDeviceCategory", model);
        }

        [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.EditDeviceTags)]
        public ActionResult DeleteDeviceCategory(long deviceCategoryId) {
            string message;
            var dataParameters = new DataParameters();
            var currentSupportAccountId = CurrentState.GetSupportAccountId() ?? 0;
            try {
                List<DeviceCategoryListItem> deviceCategoryListItems = _deviceLogic.GetDeviceCategories(currentSupportAccountId, User.UserAccount.Id, dataParameters);
                bool hasAccess = deviceCategoryListItems.Any(c => c.DeviceCategoryId == deviceCategoryId);

                if (!hasAccess)
                {
                    return new JsonResult
                    {
                        Data = new
                        {
                            Success = false,
                            Message = "You are not authorized to perform this action"
                        }
                    };
                }
                var deviceCategory = _deviceLogic.GetDeviceCategory(deviceCategoryId);
                
                if (deviceCategory == null || deviceCategory.SupportAccountId != currentSupportAccountId)
                {
                    return new JsonResult
                    {
                        Data = new
                        {
                            Success = false,
                            Message = "You are not authorized to perform this action"
                        }
                    };
                }
                
                deviceCategory.IsActive = false;
                deviceCategory.LastUpdate = DateTime.Now;
                deviceCategory.LastUpdateByUserAccountId = User.UserAccount.Id;

                _deviceLogic.DeleteDeviceCategory(deviceCategory);

                message = ApiConstants.DEVICE_CATEGORY + " has been deleted successfully.";
                return new JsonResult {
                    Data = new {
                        Success = true,
                        Message = message
                    }
                };
            } catch (Exception exception) {
                Elmah.ErrorSignal.FromCurrentContext().Raise(exception);
                message = "Failed to delete " + ApiConstants.DEVICE_CATEGORY + ".";
            }

            return new JsonResult {
                Data = new {
                    Success = false,
                    Message = message
                }
            };
        }

        [HttpPost, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult GetWildfireDevices([DataSourceRequest] DataSourceRequest request, long userAccountId) {
            var supportAccountId = CurrentState.GetCurrentAccountId();
            
            var sanitizedRequest = new DataSourceRequest();
            var filters = KendoHelper.SanitizeFilters(request.Filters);
            var sorts = KendoHelper.SanitizeSorts(request.Sorts);
            var groups = KendoHelper.SanitizeGroups(request.Groups);
            var aggregates = KendoHelper.SanitizeAggregates(request.Aggregates);
            sanitizedRequest.Aggregates = aggregates;
            sanitizedRequest.Filters = filters;
            sanitizedRequest.Groups = groups;
            sanitizedRequest.Page = request.Page;
            sanitizedRequest.PageSize = request.PageSize;
            sanitizedRequest.Sorts = sorts;
            var dataParameters = sanitizedRequest.ToDataParameters();
            var wildfireDevices = _deviceLogic.GetWildfireDevices(userAccountId, supportAccountId, dataParameters);

            var result = new DataSourceResult
            {
                Data = wildfireDevices,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };
            return Json(result);
        }


        [HttpPost, AjaxOnly, CspAuthorize(Permission.EditDeviceTags)]
        public ActionResult WipeOutNFRWildfire(string serialnumber){
            return new JsonResult {
                Data = new {
                    Success = _wildfireWipeoutService.WildFireWipeout(serialnumber),
                    Message = String.Empty
                }
            };
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.EditDevice)]
        public ActionResult GetPendingDeviceTransferDetails(long deviceId) {
            var pendingDeviceTransferDetails = _deviceLogic.GetPendingDeviceTransferDetails(deviceId);

            return PartialView("../Partial/_PendingDeviceTransferDetails", pendingDeviceTransferDetails);
        }

        // Anonymous for new CSP User Registration
        [AllowAnonymous]
        [CspAuthorize(Permission.ViewSupportAccount)]
        public JsonResult GetPanOsVersions() {
            // var panOsVersions = _deviceLogic.GetPanOsVersions();
            IEnumerable<PanOs> result = null; 
            var response = _networkSecurityApiLogic.GetPanOSVersions();
            PanOsVersions panOsVersions = JsonConvert.DeserializeObject<PanOsVersions>(JsonConvert.SerializeObject(response.ResponseObject));
            if (panOsVersions != null)            {
                 result = panOsVersions.PanOsVersion.Select(os => new PanOs() { PanOsVersion = os });               
            }
            //panOsVersions = ExcludeBetaVersions(panOsVersions); Removed beta check for PAN OS 9.0.0
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Anonymous for new CSP User Registration
        [AllowAnonymous]
        //[CspAuthorize(Permission.RegisterDevice)]
        public JsonResult GetHyperVisorVersions(string filter = null) {
            var hyperVisors = _deviceLogic.GetHyperVisorVersions(Sanitizer.GetSafeHtmlFragment(filter));
            //var hyperVisorVersions = ExcludeBetaFeatureVMs(hyperVisors);
            return Json(hyperVisors, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<PanOs> ExcludeBetaVersions(IEnumerable<PanOs> panOsVersions)
        {
            if (!panOsVersions.Any())
                return panOsVersions;
            if (!_featureAccess.IsBetaFeature(FeatureAccess.PANOsVersion_Feature, FeatureAccess.PANOsBetaVersion))
                return panOsVersions;

            var supportAccountId = CurrentState.GetSupportAccountId().HasValue ? CurrentState.GetSupportAccountId().Value : 0;
            //user can access this feature
            if (User != null && _featureAccess.CanUserAccessBetaFeature(FeatureAccess.PANOsVersion_Feature, User.UserAccount.Email, supportAccountId))
                return panOsVersions;
            var newResult = panOsVersions.Where(e => e.PanOsVersion != FeatureAccess.PANOsBetaVersion).ToList();
            return newResult;

        }

        /* For Budapest Beta use only
        private IEnumerable<HyperVisor> ExcludeBetaFeatureVMs(IEnumerable<HyperVisor> hyperVisors)
        {
            if (!hyperVisors.Any())
                return hyperVisors;
            if (!_featureAccess.IsBetaFeature(FeatureAccess.VMRegistration_Feature, FeatureAccess.VMGoogleCloudModel))
                return hyperVisors;

            var supportAccountId = CurrentState.GetSupportAccountId().HasValue ? CurrentState.GetSupportAccountId().Value : 0;
            //user can access this feature
            if (User != null && _featureAccess.CanUserAccessBetaFeature(FeatureAccess.VMRegistration_Feature, User.UserAccount.Email, supportAccountId))
                return hyperVisors;
	        var newResult = hyperVisors.Where(e => e.HyperVisorVersion != FeatureAccess.VMGoogleCloudModel).ToList();
	        return newResult;
	    }
          */

        [HttpGet, CspAuthorize(Permission.ViewDevice)]
        public ActionResult GetUnassignedComponentQuantities(string chassisSerialNumber)
        {
            var components = _deviceLogic.GetChassisUnassignedComponentQuantities(chassisSerialNumber);
            return Json(components, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, CspAuthorize(Permission.ViewDevice)]
        public ActionResult GetUnassignedComponentQuantitiesBySalesOrder([DataSourceRequest] DataSourceRequest request, string salesOrderNumber)
        {
            var components = _deviceLogic.GetUnassignedComponentQuantitiesBySalesOrder(salesOrderNumber);
            return Json(components, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ValidateJsonAntiForgeryToken, CspAuthorize(Permission.ViewDevice)]
        public ActionResult RegisterComponents([DataSourceRequest] DataSourceRequest request, List<ChassisComponent> components)
        {
            var sanitizedComponents = new List<ChassisComponent>();
            foreach(var component in components){
                sanitizedComponents.Add(new ChassisComponent
                {
                    ActualQuantity = component.ActualQuantity,
                    Address1 = Sanitizer.GetSafeHtmlFragment(component.Address1),
                    Address2 = Sanitizer.GetSafeHtmlFragment(component.Address2),
                    ChasisPartNumber = Sanitizer.GetSafeHtmlFragment(component.ChasisPartNumber),
                    ChassisSerialNumber = Sanitizer.GetSafeHtmlFragment(component.ChassisSerialNumber),
                    City = Sanitizer.GetSafeHtmlFragment(component.City),
                    CountryId = component.CountryId,
                    CountryName = Sanitizer.GetSafeHtmlFragment(component.CountryName),
                    Description = Sanitizer.GetSafeHtmlFragment(component.Description),
                    DeviceId = component.DeviceId,
                    LineCardId = component.LineCardId,
                    ModelCompatibility = Sanitizer.GetSafeHtmlFragment(component.ModelCompatibility),
                    OldChassisSerialNumber = Sanitizer.GetSafeHtmlFragment(component.OldChassisSerialNumber),
                    PartMasterId = component.PartMasterId,
                    PartNumber = Sanitizer.GetSafeHtmlFragment(component.PartNumber),
                    PostalCode = Sanitizer.GetSafeHtmlFragment(component.PostalCode),
                    ProductFamily = Sanitizer.GetSafeHtmlFragment(component.ProductFamily),
                    Quantity = component.Quantity,
                    Region = Sanitizer.GetSafeHtmlFragment(component.Region),
                    RegisteredQuantity = component.RegisteredQuantity,
                    SalesOrderNumber = Sanitizer.GetSafeHtmlFragment(component.SalesOrderNumber),
                    ShipmentNumber = component.ShipmentNumber,
                    SupportAccountId = component.SupportAccountId,
                    UserAccountId = component.UserAccountId
                });
            }
            _deviceLogic.UpdateChassisComponents(sanitizedComponents);
            return Json(sanitizedComponents, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, CspAuthorize(Permission.ViewDevice)]
        public ActionResult UpdateComponent(string partNumber, string chassisSerialNumber, Int64 quantity, string address1, string address2,
            string city, string region, Int64 countryId, string postalCode, Int64 supportAccountId, Int64 registeredQuantity)
        {
            var linceCardDto = new ChassisComponentDto()
            {
                ChassisComponent = new ChassisComponent()
                {
                    PartNumber = partNumber,
                    ChassisSerialNumber = chassisSerialNumber,
                    OldChassisSerialNumber = chassisSerialNumber,
                    Quantity = quantity,
                    ActualQuantity = registeredQuantity,
                    Address1 = address1,
                    Address2 = address2,
                    City = city,
                    Region = region,
                    CountryId = countryId,
                    PostalCode = postalCode,
                    SupportAccountId = supportAccountId,
                    RegisteredQuantity = registeredQuantity
                }
            };
            return View("~/Views/Partial/_UpdateChassisComponent.cshtml", linceCardDto);
        }

        [HttpPost, CspAuthorize(Permission.ViewDevice)]
        public ActionResult UpdateComponent(ChassisComponentDto model)
        {
            var component = model.ChassisComponent;
            var registerdQuantity = component.RegisteredQuantity;
            var newQuantity = component.Quantity;

            //Chassis serial number is modified and not unassined components  registration 
            if (component.OldChassisSerialNumber != component.ChassisSerialNumber && registerdQuantity != 0)
            {
                var oldChassis = component.OldChassisSerialNumber;
                var newChassis = component.ChassisSerialNumber;

                // Deleting old chassis components
                component.Quantity = registerdQuantity;
                component.ChassisSerialNumber = oldChassis;
                _deviceLogic.DeleteComponent(component);

                //Updating new chassis components
                component.Quantity = newQuantity;
                component.ChassisSerialNumber = newChassis;
                component.UserAccountId = User.UserAccount.Id;
                _deviceLogic.UpdateComponent(component);
            }
            else
            {
                if (newQuantity > registerdQuantity)
                {
                    component.Quantity = newQuantity - registerdQuantity; // calculating additional components quantiy
                    component.UserAccountId = User.UserAccount.Id;
                    _deviceLogic.UpdateComponent(component);
                }
                else if (newQuantity < registerdQuantity)
                {
                    component.Quantity = registerdQuantity - newQuantity;// caluculating to be delete components quantity
                    _deviceLogic.DeleteComponent(component);
                }
            }

            //Update Location
            _deviceLogic.UpdateChassisLocation(component);

            model.Notifications.Add(new ResponseNotification("Component was successfully updated", ResponseState.Success));
            ViewBag.HideButton = true;
            return View("~/Views/Partial/_UpdateChassisComponent.cshtml", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewDevice)]
        public ActionResult GetChassisLocation(string chassisSerialNumber)
        {
            var chassisLocation = _deviceLogic.GetChassisLocation(chassisSerialNumber);
            return Json(chassisLocation, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, CspAuthorize(Permission.ViewDevice)]
        public ActionResult RegisterSalesOrderComponents()
        {
            var model = new ChassisComponentDto()
            {
                ChassisComponent = new ChassisComponent() { SupportAccountId = CurrentState.GetCurrentAccountId() },

            };

            #region Bind Eula Model again

            model.Eula = new EulaModel()
            {
                Languages = _eulaLogic.GetEulaLanguages(),
                Eulas = _eulaLogic.GetEula(),
                DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.RegisterDevice;


            #endregion
            return PartialView("~/Views/Partial/_RegisterComponentBySalesOrder.cshtml", model);
        }

        [HttpPost, CspAuthorize(Permission.ViewDevice)]
        public ActionResult RegisterSalesOrderComponents(ChassisComponentDto component)
        {
            var model = new ChassisComponentDto()
            {
                ChassisComponent = new ChassisComponent(),

            };

            #region Bind Eula Model again

            model.Eula = new EulaModel()
            {
                Languages = _eulaLogic.GetEulaLanguages(),
                Eulas = _eulaLogic.GetEula(),
                DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.RegisterDevice;
            model.Notifications.Add(new ResponseNotification("Components registered sucessfully", ResponseState.Success));

            #endregion
            return PartialView("~/Views/Partial/_RegisterComponentBySalesOrder.cshtml", model);
        }
    }
}
