using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Kendo.Mvc.UI;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Csp.Web.Controllers
{
    [Authorize]
    public class CsspController : BaseController
    {
        private readonly ICsspLogic _csspLogic;
        private readonly ILookupLogic _lookupLogic;
        private readonly ICsspDataAccess _csspDataAccess;

        public CsspController(ICsspLogic csspLogic, ILookupLogic lookupLogic,ICsspDataAccess csspDataAccess)
        {
            _csspLogic = csspLogic;
            _lookupLogic = lookupLogic;
            _csspDataAccess = csspDataAccess;
        }

        [CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult Index(long id, string prelicenseId = null)
        {
            try
            {
                var csspInfo = _csspLogic.GetCsspInfo(id);
                var shoppingCart = new List<CsspSku>();
                var csspDto = new CsspInfoDto()
                {
                    IsOrderSubmitted = false,
                    Cssp = csspInfo,
                    OrderItems = shoppingCart,
                    SupportAccountId = id
                };
                if (prelicenseId == null)
                {
                    return PartialView("../Partial/_CsspHome", csspDto);
                }
                else
                {
                    var dataParameters = new DataParameters { PaginationCount = 50 };
                    var model = new CsspPreLicenseDeviceDto();
                    model.DataParameters = dataParameters;
                    model.SupportAccountId = id;
                    model.DefaultDeviceCode = prelicenseId;
                    model.DefaultDeviceSearchField = "BatchId";
                    return PartialView("../Partial/_CsspDownloadLicenseKeys", model);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception($"Error loading CSSP Home for SupportAccountId: {id}", ex));
                return PartialView("../Shared/_Error");
            }
        }

        [CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult Cssp(long id, string csspSection)
        {
            var model = new CsspDto();
            model.SupportAccountId = id;
            model.SelectedTab = csspSection;
            model.IsPrelicensingEnabled = _csspLogic.CanGeneratePrelicenseKeys(id);
            return PartialView("../Partial/_Cssp", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult CsspCart(long id)
        {
            var csspInfo = _csspLogic.GetCsspInfo(id);
            var loggedInUser = User.UserAccount.Id;
            var hasCsspAccess = _csspLogic.IsUserCssp(loggedInUser, id);
            var shoppingCart = new List<CsspSku>();
            var csspDto = new CsspInfoDto()
            {
                IsOrderSubmitted = false,
                Cssp = csspInfo,
                OrderItems = shoppingCart,
                SupportAccountId = id,
                HasCsspAccess = hasCsspAccess
            };
            return PartialView("../Partial/_CsspDetails", csspDto);
        }

        [HttpPost, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult SubmitOrder(long SupportAccountId, string CartItemIds, string orderConfirmationEmail = "")
        {
            var loggedInUser = User.UserAccount.Id;
            var loggedInUserEmail = User.UserAccount.Email;
            var hasCsspAccess = _csspLogic.IsUserCssp(loggedInUser, SupportAccountId);
            if (!hasCsspAccess)
            {
                string errMessage =
                            "You do not have access to Submit Order!";
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errMessage));
                return PartialView("../Shared/_Error");
            }
            List<int> productIds = CartItemIds.Split(',').Select(int.Parse).ToList();
            var csspDto = new CsspInfoDto()
            {
                IsOrderSubmitted = false,
                HasError = true
            };
            int? cartId;
            string cartNumber;
            try
            {
                bool isOrderSaved = _csspLogic.SubmitCsspOrder(productIds, SupportAccountId,
                    User.UserAccount.Id, out cartId, out cartNumber);

                if (isOrderSaved && cartId != null)
                {
                    try
                    {
                        _csspLogic.CreateSapOrderAsync(SupportAccountId, cartId.Value, cartNumber, loggedInUserEmail);
                    }
                    catch (Exception)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error occured while creating SAP Order"));
                    }
                    csspDto.IsOrderSubmitted = true;
                    csspDto.CartNumber = cartNumber;
                }
                else
                {
                    string errMessage =
                            "Error occured while saving Cart Items";
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errMessage));
                    return PartialView("../Shared/_Error");
                }
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(oex);
                return PartialView("../Shared/_Error");
            }
            return PartialView("../Partial/_CsspDetails", csspDto);
        }


        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult OrderHistory(long id)
        {
            var ordersDto = new CsspOrdersDto();
            ordersDto.OrdersList = new List<CsspOrderHistory>();
            ordersDto.SupportAccountId = id;

            return PartialView("../Partial/_CsspOrderHistory", ordersDto);
        }

         [OutputCache(NoStore = true, Duration = 0), CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult OrderHistory_Read([DataSourceRequest] DataSourceRequest request, string orderNumber = "")
        {
            var supportAccountId = CurrentState.GetSupportAccountId();
            var dataParameters = request.ToDataParameters();
            if (supportAccountId != null)
            {
                var orderList = _csspLogic.GetOrderHistory(supportAccountId.Value, orderNumber);

                var result = new DataSourceResult
                {
                    Data = orderList,
                    Total = Convert.ToInt32(dataParameters.TotalRecordCount)
                };
                var jsonResult = Json(result);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            else
            {
                var result = new DataSourceResult
                {
                    Data = new List<CsspOrderHistory>(),
                    Total = Convert.ToInt32(dataParameters.TotalRecordCount)
                };
                var jsonResult = Json(result);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        [CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult OrderItems_Read([DataSourceRequest] DataSourceRequest request, int orderId)
        {
            var dataParameters = request.ToDataParameters();
            var orderItemList = _csspLogic.GetOrderHistoryItems(orderId);

            var result = new DataSourceResult
            {
                Data = orderItemList,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };
            var jsonResult = Json(result);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult ShowProductFilter()
        {
            var model = new CsspProductSelectorDto();

            model.CsspModels = _csspLogic.GetCsspModels();
            model.CsspSupportTypes = _csspLogic.GetCsspSupportTypes();
            model.CsspTerms = _csspLogic.GetCsspTerms();

            var dataParameters = new DataParameters { PaginationCount = 50 };
            model.DataParameters = dataParameters;

            return PartialView("../Partial/_CsspProductSelector", model);
        }

        [HttpPost, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult SearchProducts([DataSourceRequest] DataSourceRequest request, int? csspModel, int? csspTerm, int? csspSupportType, int? bundle, string filterProducts = "")
        {
            
            var dataParameters = request.ToDataParameters();
            
            // ReSharper disable once PossibleInvalidOperationException
            var supportAccountId = CurrentState.GetSupportAccountId().Value;
            var searchResults = _csspLogic.SearchCsspSkus(dataParameters, supportAccountId, model: csspModel, term: csspTerm, supportType: csspSupportType, bundle: bundle, filterProducts: filterProducts);

            var result = new DataSourceResult
            {
                Data = searchResults,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };
            var jsonResult = Json(result);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;

        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult GetProductDetails(int skuId)
        {
            var data = _csspLogic.GetSkuDetails(skuId);
            var jsonResult = Json(data, JsonRequestBehavior.AllowGet);
            return jsonResult;

        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult GetProductsDetails(string csvProductIds)
        {
            List<int> productIds = csvProductIds.Split(',').Select(int.Parse).ToList();
            var data = _csspLogic.GetSkuDetails(productIds);
            var jsonResult = Json(data, JsonRequestBehavior.AllowGet);
            return jsonResult;

        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult ViewAvailable(long id)
        {
            var supportAcctId = id;
            var vmViewModel = new CsspVmDto();
            var dataParameters = new DataParameters { PaginationCount = 50 };
            vmViewModel.DataParameters = dataParameters;
            vmViewModel.AvailableVms = _csspLogic.GetAvailableVms(dataParameters, supportAcctId, User.UserAccount.Id, "");
            vmViewModel.SupportAccountId = supportAcctId;
            vmViewModel.HasCsspAccess = _csspLogic.IsUserCssp(User.UserAccount.Id, id);
            vmViewModel.CanGeneratePrelicenseKeys = _csspLogic.CanGeneratePrelicenseKeys(supportAcctId);
            return PartialView("../Partial/_CsspVMView", vmViewModel);
        }

        [HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.VmAuthRegister)]
        public ActionResult SearchAvailable([DataSourceRequest] DataSourceRequest request, string authCode)
        {
            var dataParameters = request.ToDataParameters();

            var availableVms = _csspLogic.GetAvailableVms(dataParameters, CurrentState.GetCurrentAccountId(),User.UserAccount.Id, authCode);

            var result = new DataSourceResult
            {
                Data = availableVms,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult ProvisionedDevices(long id)
        {
            var dataParameters = new DataParameters { PaginationCount = 50 };
            var model = new CsspProvisionedDevicesDto();
            model.HasCsspAccess = _csspLogic.IsUserCssp(User.UserAccount.Id, id);
            model.DataParameters = dataParameters;
            model.SupportAccountId = id;
            model.DefaultDeviceCode = "";
            DateTime now = DateTime.Now;
            model.DownLoadUsageReportMonths = new List<KeyValuePair<string, string>>();

            if (now.Day == 1) // still previous month unavailable
            {
                now = now.AddDays(-1);
            }
            now = now.AddMonths(-7);
            for (int count = 0; count < 6; count++)
            {
                now = now.AddMonths(1);
                model.DownLoadUsageReportMonths.Add(new KeyValuePair<string, string>(now.ToString("MMM-yy-MM-yyyy"), now.ToString("MMMM yyyy")));
            }
            return PartialView("../Partial/_CsspDeviceView", model);
        }

        [HttpPost, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult ProvisionedDevices([DataSourceRequest] DataSourceRequest request, long? supportAccountId, string searchField, string searchValue)
        {
            var dataParameters = request.ToDataParameters();

            if (!supportAccountId.HasValue)
            {
                supportAccountId = CurrentState.GetCurrentAccountId();
            }

            string serialNumber = string.Empty;
            string authorizationCode = string.Empty;
            string endUserAccountId = string.Empty;
            string endUserCompany = string.Empty;
            string contactEmail = string.Empty;

            switch (searchField)
            {
                case "SerialNumber":
                    serialNumber = searchValue;
                    break;
                case "AuthorizationCodes":
                    authorizationCode = searchValue;
                    break;
                case "EndUserAccountId":
                    endUserAccountId = searchValue;
                    break;
                case "EndUserCompany":
                    endUserCompany = searchValue;
                    break;
                case "EndUserContactEmail":
                    contactEmail = searchValue;
                    break;
            }

            //var availableDevices = _csspLogic.GetCsspProvisionedDevices(dataParameters, supportAccountId.Value,
            //    serialNumber, authorizationCode, endUserAccountId, endUserCompany, contactEmail);

            var availableDevices = _csspDataAccess.GetCsspProvisionedDevices(supportAccountId.Value,
                serialNumber, authorizationCode, endUserAccountId, endUserCompany, contactEmail, dataParameters);

            var result = new DataSourceResult
            {
                Data = availableDevices,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };

            return Json(result);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public FileResult ProvisionedDevicesReport(long supportAccountId, string searchField, string searchValue)
        {
            // Verify the user has access to the requested support account
            var currentSupportAccountId = CurrentState.GetSupportAccountId();
            if (!currentSupportAccountId.HasValue || currentSupportAccountId.Value != supportAccountId)
            {
                throw new UnauthorizedAccessException("You are not authorized to access this support account's data.");
            }

            string serialNumber = string.Empty;
            string authorizationCode = string.Empty;
            string endUserAccountId = string.Empty;
            string endUserCompany = string.Empty;
            string contactEmail = string.Empty;

            switch (searchField)
            {
                case "SerialNumber":
                    serialNumber = searchValue;
                    break;
                case "AuthorizationCodes":
                    authorizationCode = searchValue;
                    break;
                case "EndUserAccountId":
                    endUserAccountId = searchValue;
                    break;
                case "EndUserCompany":
                    endUserCompany = searchValue;
                    break;
                case "EndUserContactEmail":
                    contactEmail = searchValue;
                    break;
            }
            var csv = _csspLogic.GetCsspProvisionedDevicesCsv(supportAccountId, serialNumber, authorizationCode, endUserAccountId, endUserCompany, contactEmail);
            var dateStr = DateTime.Now.ToString("yyyyMMdd");
            return File(csv, "text/comma-separated-values", "ProvisionedDevicesReport_" + dateStr + ".csv");
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public FileResult ProvisionedDevicesMonthlyReport(long supportAccountId, string key)
        {
            // Verify the user has access to the requested support account
            var currentSupportAccountId = CurrentState.GetSupportAccountId();
            if (!currentSupportAccountId.HasValue || currentSupportAccountId.Value != supportAccountId)
            {
                throw new UnauthorizedAccessException("You are not authorized to access this support account's data.");
            }

            string[] keys = key.Split('-');
            CsspProvisionedDevicesMonthlyData<byte[]> data = _csspLogic.GetCsspProvisionedDevicesMonthlyCsv(supportAccountId, int.Parse(keys[3]), int.Parse(keys[2]));
            return File(data.Items, "text/comma-separated-values", string.Format("UsageReport_{0}_{1}{2}.csv",
                Regex.Replace(System.Text.RegularExpressions.Regex.Replace(data.SupportAccountName, "[^a-zA-Z0-9]", "."), "\\.+", "."), 
                data.Month, data.Year));
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult RegisterVm(string authCode)
        {
            throw new NotImplementedException("CSSP End Of Service.");
            /*
            var registerVmDto = new RegisterVmDto
            {
                AuthCode = authCode,
            };
            return PartialView("../Partial/_CsspVMRegister", registerVmDto);
            */
        }

        [HttpPost, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult RegisterVm(RegisterVmDto model)
        {
            throw new NotImplementedException("CSSP End Of Service.");
            /*
            RegisterVmResult result;
            try
            {
                if (_csspLogic.IsUserCssp(User.UserAccount.Id, CurrentState.GetCurrentAccountId()))
                {
                    result = _csspLogic.RegisterVm(model.UuId, model.CpuId, model.AuthCode,
                        CurrentState.GetCurrentAccountId(), User.UserAccount.Id);
                }
                else
                {
                    result = new RegisterVmResult
                    {
                        ErrorMessage = "You do not have rights to register a VM",
                    };
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                result = new RegisterVmResult
                {
                    ErrorMessage = "An error occurred while registering the VM",
                };
            }
            ModelState.Remove("NewDeviceId");
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                model.Notifications.Add(new ResponseNotification("Success", ResponseState.Success));
                model.NewDeviceId = result.NewDeviceId;
            }
            else
            {
                model.Notifications.Add(new ResponseNotification(result.ErrorMessage, ResponseState.Error));
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error from LicenseIt: " + result.ErrorMessage));
            }
            return PartialView("../Partial/_CsspVMRegister", model);
            */
        }

        [HttpPost, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult RegisterVmFile(HttpPostedFileBase[] uploadUuIdCpuId)
        {
            throw new NotImplementedException("CSSP End Of Service.");
            /*
            ParseVmFileResult result;
            if (uploadUuIdCpuId.Length == 1)
            {
                result = _csspLogic.ParseVmFile(uploadUuIdCpuId.Single());
            }
            else
            {
                result = new ParseVmFileResult
                {
                    Message = "Upload failed.  File could not be found!"
                };
            }

            return Json(result);
            */
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult EndUserDetails(long supportAccountId, long deviceId)
        {
            var model = new CsspEndUserInfoDto()
            {
                EndUser = new CsspEndUser(),
            };
            model.EndUser.CSSPSupportAccountId = supportAccountId;
            model.EndUser.ProvisionedDeviceId = deviceId;
            return PartialView("../Partial/_CsspEndUserDetails", model);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult EndUsersDetails(long supportAccountId, string csvDeviceIds)
        {
            var model = new CsspEndUserInfoDto()
            {
                EndUser = new CsspEndUser(),
            };
            model.EndUser.CSSPSupportAccountId = supportAccountId;
            model.EndUser.ProvisionedDeviceIds = csvDeviceIds;
            return PartialView("../Partial/_CsspEndUserDetails", model);
        }

       [HttpPost, AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult AddEndUserInformation(CsspEndUserInfoDto model)
        {
            throw new NotImplementedException("CSSP End Of Service.");
            /*
           try
           {

               if (_csspLogic.IsUserCssp(User.UserAccount.Id, CurrentState.GetCurrentAccountId()))
               {
                   if (model.EndUser.CountryId != null)
                       model.EndUser.Country = _lookupLogic.GetCountryCodeById(model.EndUser.CountryId.Value);
                   var eUser = new CsspEndUser()
                   {
                       CreatedBy = User.UserAccount.Email,
                       ProvisionedDeviceId = model.EndUser.ProvisionedDeviceId,
                       ProvisionedDeviceIds = model.EndUser.ProvisionedDeviceIds,
                       Address = model.EndUser.Address,
                       CSSPSupportAccountId = model.EndUser.CSSPSupportAccountId,
                       City = model.EndUser.City,
                       CompanyName = model.EndUser.CompanyName,
                       Country = model.EndUser.Country,
                       CustomerAccountId = model.EndUser.CustomerAccountId,
                       EndUserContactEmail = model.EndUser.EndUserContactEmail,
                       PostalCode = model.EndUser.PostalCode,
                       Region = model.EndUser.Region,
                       PhoneNumber = model.EndUser.PhoneNumber,
                       CompanyWebsite = model.EndUser.CompanyWebsite,
                       Industry = model.EndUser.Industry
                   };
                   string errorMessage;

                   if (string.IsNullOrEmpty(eUser.ProvisionedDeviceIds) && eUser.ProvisionedDeviceId.HasValue == false)
                   {
                       model.Notifications.Add(new ResponseNotification("Please select a device.", ResponseState.Error));
                   }
                   else
                   {
                       if (_csspLogic.AddEndUserInformationForCsspCustomer(eUser, out errorMessage))
                       {
                           model.Notifications.Add(new ResponseNotification(
                               "Successfully updated End User information.", ResponseState.Success));
                       }
                       else
                       {
                           model.Notifications.Add(new ResponseNotification(errorMessage, ResponseState.Error));
                       }
                   }
               }
               else
               {
                   model.Notifications.Add(new ResponseNotification(
                               "You dont have access to Add end user information.", ResponseState.Error));
               }
           }
           catch (Exception ex)
           {
               Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
               model.Notifications.Add(new ResponseNotification("Error occured while adding End User Information",
                   ResponseState.Error));
           }
           return PartialView("../Partial/_CsspEndUserDetails", model);
            */
        }

        [HttpPost, CspAuthorize(Permission.ViewSupportAccount)]
       public ActionResult SearchEndUsers([DataSourceRequest] DataSourceRequest request, long supAccountId, string searchField, string searchValue)
        {
            var dataParameters = request.ToDataParameters();
            var searchResult = _csspLogic.SearchEndUser(supAccountId,searchField, searchValue);
            var result = new DataSourceResult
            {
                Data = searchResult,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };
            var jsonResult = Json(result);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [HttpGet, AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult CreatePrelicenseKeys(string authCode)
        {
            long supId = CurrentState.GetSupportAccountId().Value;
            var prelicenseDto = new GenerateKeysDto
            {
                AuthCode = authCode,
                SupportAccountId = supId
            };
            prelicenseDto.VmUseCount = _csspLogic.GetVmUseCountByAuthCode(authCode);
            return PartialView("../Partial/_CsspCreatePrelicenseKeys", prelicenseDto);
        }

        [HttpPost, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult CreatePrelicenseKeys(GenerateKeysDto model)
        {
            var supportAccountId = CurrentState.GetSupportAccountId();
            var loggedInUser = User.UserAccount.Email;
            if (ModelState.IsValid == false || supportAccountId == null || model.PreLicenseQuantity == null)
            {
                return PartialView("../Partial/_CsspCreatePrelicenseKeys", model);
            }
            var result = _csspLogic.ProcessPrelicenseKeys(supportAccountId.Value, model.AuthCode, model.PreLicenseQuantity.Value,
                        loggedInUser);

            if (!result.HasError)
            {
                model.IsPrelicenseRequestSuccess = true;
                model.PreLicenseRequestId = result.PrelicenseRequestId;
                model.BatchId = result.BatchId;
                model.IsAsync = result.IsAsync;
                model.RequestedBy = loggedInUser;
                model.SupportAccountId = supportAccountId.Value;
                model.PrelicenseDeviceUrl = string.Format("/SupportAccount/CsspInfo/{0}?prelicenseId={1}",
                    supportAccountId, result.BatchId);
            }
            else
            {
                model.Notifications.Add(new ResponseNotification("Error occured while generating Prelicense keys!", ResponseState.Error));
            }
            return PartialView("../Partial/_CsspCreatePrelicenseKeys", model);
        }

        //Device with Prelicense Keys
        [CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult PreLicenseDevices(long supportAccountId, string prelicenseId = null)
        {
            var dataParameters = new DataParameters { PaginationCount = 50 };
            var model = new CsspPreLicenseDeviceDto();
            model.DataParameters = dataParameters;
            model.SupportAccountId = supportAccountId;
            if (string.IsNullOrEmpty(prelicenseId))
            {
                model.DefaultDeviceCode = "";
                model.DefaultDeviceSearchField = "AuthorizationCodes";
            }
            else
            {
                model.DefaultDeviceCode = prelicenseId;
                model.DefaultDeviceSearchField = "BatchId";
            }
            return PartialView("../Partial/_CsspDownloadLicenseKeys", model);
        }

        [HttpPost, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult PreLicenseDevices([DataSourceRequest] DataSourceRequest request, long supAccountId, string searchField, string searchValue)
        {
            var dataParameters = request.ToDataParameters();
            string serialNumber = string.Empty;
            string authorizationCode = string.Empty;
            string batchId = string.Empty;
            switch (searchField)
            {
                case "SerialNumber":
                    serialNumber = searchValue;
                    break;
                case "AuthorizationCodes":
                    authorizationCode = searchValue;
                    break;
                case "BatchId":
                    batchId = searchValue;
                    break;
            }
            var searchResult = _csspLogic.GetCsspPrelicenseDevices(dataParameters, supAccountId, authorizationCode, batchId, serialNumber);
            var result = new DataSourceResult
            {
                Data = searchResult,
                Total = Convert.ToInt32(dataParameters.TotalRecordCount)
            };
            var jsonResult = Json(result);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult DownloadPreLicenseKeysOnTheFly(string csvDeviceIds, long supAccountId, bool selectAll,
            string searchField, string searchValue)
        {
            var deviceIds =  selectAll ? _csspLogic.GetDeviceIdsToDownloadLicenseKeys(supAccountId, searchField, searchValue) : 
                csvDeviceIds.Split(',').Select(long.Parse).ToList();
            if (deviceIds.Count > 0)
            {
                var result = _csspLogic.DownloadLicense(deviceIds);
                using (var baseOutputStream = new MemoryStream())
                {
                    ZipOutputStream zipOutput = new ZipOutputStream(baseOutputStream);
                    zipOutput.IsStreamOwner = false;
                    zipOutput.UseZip64 = UseZip64.Off;
                    zipOutput.SetLevel(3);
                    byte[] buffer = new byte[4096];
                    foreach (var csspDev in result.Licenses)
                    {
                        ZipEntry entry =
                            new ZipEntry(string.Format("{0}/{1}.key", csspDev.SerialNumber, csspDev.LicenseName));
                        entry.DateTime = DateTime.Now;
                        zipOutput.PutNextEntry(entry);
                        StreamUtils.Copy(new MemoryStream(UTF8Encoding.Default.GetBytes(csspDev.LicenseKey)), zipOutput,
                            new byte[4096]);
                        zipOutput.CloseEntry();
                    }
                    // Finalize the zip output.
                    zipOutput.Finish();
                    zipOutput.Flush();
                    zipOutput.Close();

                    baseOutputStream.Position = 0;
                    return File(baseOutputStream.ToArray(), "application/zip", "Pre-License Keys.zip");
                }
            }
            return null;
        }
        

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult GetPrelicenseStatus(int requestId)
        {
            var statusId = _csspLogic.GetPreLicenseRequestStatusId(requestId);
            return Json(statusId, JsonRequestBehavior.AllowGet);
        }
  
    }
}
