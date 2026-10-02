using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.Security.Attributes;
using Csp.Api.Model.Enums;
using System.ComponentModel.DataAnnotations;
using Csp.Api.DataAccess.HelperClasses.Literals;
using NHibernate.Cfg.MappingSchema;
using Microsoft.Security.Application;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Kendo.Mvc;

namespace Csp.Web.Controllers
{
    [BulkRegistrationAuthorize]
    public class BulkRegistrationController : BaseController
    {
        private readonly IBulkRegistrationLogic _bulkRegistrationLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly ILookupLogic _lookupLogic;
        private readonly IEulaLogic _eulaLogic;
        private readonly ISiteLicenseLogic _siteLicenseLogic;

        public BulkRegistrationController(IBulkRegistrationLogic bulkRegistrationLogic, IDeviceLogic deviceLogic, ILookupLogic lookupLogic, IEulaLogic eulaLogic, ISiteLicenseLogic siteLicenseLogic)
        {
            _bulkRegistrationLogic = bulkRegistrationLogic;
            _deviceLogic = deviceLogic;
            _lookupLogic = lookupLogic;
            _eulaLogic = eulaLogic;
            _siteLicenseLogic = siteLicenseLogic;
        }

        private bool IsAuthorizedForBulkRegistration(long? bulkRegistrationId, long supportAccountId)
        {
            if (!bulkRegistrationId.HasValue)
            {
                return false;
            }
            
            var currentBulkRegistrationId = _bulkRegistrationLogic.GetCurrentBulkRegistrationId(supportAccountId, User.UserAccount.Id);
            return currentBulkRegistrationId.HasValue && currentBulkRegistrationId.Value == bulkRegistrationId.Value;
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult Tabs(long id, GroupDestination selectedTab = GroupDestination.ClaimedAssets)
        {
            var currentBulkRegistrationId = _bulkRegistrationLogic.GetCurrentBulkRegistrationId(id, User.UserAccount.Id);

            var model = new BulkRegistrationTabsDto
            {
                ActionHelper = new ActionHelper("BulkRegistration", id),
                SelectedTab = selectedTab,
                BulkRegistrationId = currentBulkRegistrationId,
            };
            return PartialView("../Partial/_BulkRegistrationTabs", model);
        }

        public ActionResult AddAssets(long id, long? bulkRegistrationId)
        {
            var model = new AddAssetsDto()
            {
                ActionHelper = new ActionHelper("BulkRegistration", id),
                BulkRegistrationId = bulkRegistrationId,
                AvailablePurchases = new List<ClaimedAssetListItem>()
            };

            return PartialView("../Partial/_AddAssets", model);
        }

        public ActionResult ClaimedAssets(long id, long? bulkRegistrationId)
        {
            var hasEaLicense = _siteLicenseLogic.HasEaLicense(id);
            var model = new ClaimedAssetsDto()
            {
                ActionHelper = new ActionHelper("BulkRegistration", id),
                BulkRegistrationId = bulkRegistrationId,
                AvailablePurchases = new List<ClaimedAssetListItem>(),
                HasEaLicense = hasEaLicense
            };

            return PartialView("../Partial/_ClaimedAssets", model);
        }

        public ActionResult GetAvailablePurchases([DataSourceRequest] DataSourceRequest request, string customerCode, string deviceCode)
        {
            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
            customerCode = Sanitizer.GetSafeHtmlFragment(customerCode);

            if (string.IsNullOrEmpty(customerCode) || string.IsNullOrEmpty(deviceCode))
            {
                var errorMessage = string.IsNullOrEmpty(customerCode)
                    ? "Customer Code is required"
                    : "Device Code is required";

                return Json(new JsonResult()
                {
                    Data = new
                    {
                        Success = false,
                        Errors = errorMessage,
                        Data = new List<ClaimedAssetListItem>(),
                        Total = 0
                    }
                }, JsonRequestBehavior.AllowGet);

                /*return Json(new DataSourceResult
                {
                    Errors = errorMessage
                });*/
            }

            if (!_bulkRegistrationLogic.IsValidCustomerAndDeviceCode(supportAccountId, customerCode, deviceCode))
            {
                return Json(new JsonResult()
                {
                    Data = new
                    {
                        Success = false,
                        Errors = "Invalid customer code or device code",
                        Data = new List<ClaimedAssetListItem>(),
                        Total = 0
                    }
                }, JsonRequestBehavior.AllowGet);

                /*return Json(new DataSourceResult
                {
                    Errors = "Invalid customer code or device code"
                });*/
            }

            var results = _bulkRegistrationLogic.GetAvailablePurchases(supportAccountId, customerCode, deviceCode);
            var assetTypes = results.Where(x => !string.IsNullOrEmpty(x.AssetType)).OrderByDescending(x => x.AssetType).Select(x => x.AssetType).Distinct().ToList();
            var assetCodes = results.Where(x => !string.IsNullOrEmpty(x.Code)).OrderByDescending(x => x.Code).Select(x => x.Code).Distinct().ToList();
            var partNumbers = results.Where(x => !string.IsNullOrEmpty(x.PartNumber)).OrderByDescending(x => x.PartNumber).Select(x => x.PartNumber).Distinct().ToList();
            var salesOrderNumbers = results.Where(x => !string.IsNullOrEmpty(x.SalesOrderNumber)).OrderByDescending(x => x.SalesOrderNumber).Select(x => x.SalesOrderNumber).Distinct().ToList();

            var jsonResult = new JsonResult
            {
                Data = new
                {
                    Success = true,
                    Data = results,
                    Total = results.Count(),
                    AssetTypes = assetTypes,
                    AssetCodes = assetCodes,
                    PartNumbers = partNumbers,
                    SalesOrderNumbers = salesOrderNumbers
                }
            };

            return Json(jsonResult, JsonRequestBehavior.AllowGet);
            /*var results = _bulkRegistrationLogic.GetAvailablePurchases(supportAccountId, customerCode, deviceCode);

            return Json(results.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);*/
        }

        public ActionResult GetClaimedPurchases([DataSourceRequest] DataSourceRequest request)
        {
            var userAccountId = User.UserAccount.Id;
            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;

            var results = _bulkRegistrationLogic.GetClaimedPurchases(supportAccountId);

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

            return Json(results.ToDataSourceResult(sanitizedRequest), JsonRequestBehavior.AllowGet);

        }

        [HttpPost, ValidateJsonAntiForgeryToken]
        public ActionResult ClaimPurchases(List<ClaimedAssetListItem> purchases)
        {
            var userAccountId = User.UserAccount.Id;
            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;

            if (purchases == null || purchases.Count == 0)
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Success = false,
                        Message = "Please select the assets you want to claim"
                    }
                };
            }

            _bulkRegistrationLogic.ClaimPurchases(userAccountId, supportAccountId, purchases);

            return new JsonResult
            {
                Data = new
                {
                    Success = true,
                    Message = "Your selected purchases have been claimed successfully"
                }
            };
        }

        public ActionResult UnClaimPurchases(List<ClaimedAssetListItem> purchases)
        {
            var userAccountId = User.UserAccount.Id;
            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;

            if (purchases == null || purchases.Count == 0)
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Success = false,
                        Message = "Please select the assets you want to unclaim"
                    }
                };
            }

            _bulkRegistrationLogic.UnclaimPurchases(userAccountId, supportAccountId, purchases);

            return new JsonResult
            {
                Data = new
                {
                    Success = true,
                    Message = "Your selected purchases have been unclaimed successfully"
                }
            };
        }

        public ActionResult UnClaimAllPurchases()
        {
            var userAccountId = User.UserAccount.Id;
            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;

            _bulkRegistrationLogic.UnclaimAllPurchases(userAccountId, supportAccountId);

            return new JsonResult
            {
                Data = new
                {
                    Success = true,
                    Message = "Your selected purchases have been unclaimed successfully"
                }
            };
        }


        [HttpGet]
        public ActionResult PrepBulkRegistration(long id, long? bulkRegistrationId)
        {
            var userAccountId = User.UserAccount.Id;
            var model = new PrepBulkRegistrationDto
            {
                ActionHelper = new ActionHelper("BulkRegistration", id),
                BulkRegistrationId = bulkRegistrationId,
                SelectDevicesFilter = SelectDevicesFilterTypes.ClaimedPurchases,
                DeviceCategories = _deviceLogic.GetAllDeviceCategories(CurrentState.GetCurrentAccountId(), User.UserAccount.Id),
            };

            if (bulkRegistrationId.HasValue)
            {
                var devices =
                    _bulkRegistrationLogic.GetBulkRegistrationDeviceIdsWithModelCompatibility(bulkRegistrationId.Value);
                var deviceIds = devices.Select(dev => long.Parse(dev.Value)).ToArray();

                model.ModelCompatibility = (devices.Any()) ? devices[0].Name : string.Empty;
                var prepBulkRegDetails = _bulkRegistrationLogic.GetPrepBulkRegistrationDetails(id, bulkRegistrationId.Value);

                if (prepBulkRegDetails != null)
                {
                    model.DefaultDeviceName = prepBulkRegDetails.DefaultDeviceName;
                    model.DefaultDeviceCategoryId = prepBulkRegDetails.DefaultDeviceCategoryId;
                    model.DefaultDeviceAddress1 = prepBulkRegDetails.DefaultDeviceAddress1;
                    model.DefaultDeviceAddress2 = prepBulkRegDetails.DefaultDeviceAddress2;
                    model.DefaultDeviceCity = prepBulkRegDetails.DefaultDeviceCity;
                    model.DefaultDeviceCountryId = prepBulkRegDetails.DefaultDeviceCountryId;
                    model.DefaultDeviceRegionId = prepBulkRegDetails.DefaultDeviceRegionId;
                    model.DefaultDeviceOtherRegion = prepBulkRegDetails.DefaultDeviceOtherRegion;
                    model.DefaultDevicePostalCode = prepBulkRegDetails.DefaultDevicePostalCode;
                    model.DefaultDeviceComments = prepBulkRegDetails.DefaultDeviceComments;

                    model.SelectDevicesFilter = prepBulkRegDetails.SelectDevicesFilter;
                    model.DevsAndLics = prepBulkRegDetails.DevsAndLics;
                }
            }

            return PartialView("../Partial/_PrepBulkRegistration", model);
        }

        [CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult GetClaimedOrRegisteredDevicesForPrepBulkReg([DataSourceRequest] DataSourceRequest request, long id, long? bulkRegistrationId, SelectDevicesFilterTypes selectDevicesFilter, bool reload = false)
        {
            var userAccountId = User.UserAccount.Id;
            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;

            if (reload)
            {
                bulkRegistrationId = null;
            }
            
            var results = _bulkRegistrationLogic.GetClaimedOrRegisteredDevicesForPrepBulkReg(userAccountId, supportAccountId, bulkRegistrationId,
                selectDevicesFilter);

            var jsonResult = Json(results.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        [AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult GetAvailableLicensesForPrepBulkReg([DataSourceRequest] DataSourceRequest request, long id, long? bulkRegistrationId, string modelCompatibility)
        {
            var results = new List<PrepBulkRegistration.Availablelicense>();
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

            if (string.IsNullOrEmpty(modelCompatibility))
            {
                return Json(results.ToDataSourceResult(sanitizedRequest), JsonRequestBehavior.AllowGet);
            }

            var userAccountId = User.UserAccount.Id;

            results = _bulkRegistrationLogic.GetAvailableLicensesForPrepBulkReg(userAccountId, id, bulkRegistrationId,
                modelCompatibility);

            return Json(results.ToDataSourceResult(sanitizedRequest), JsonRequestBehavior.AllowGet);
        }

        /*
        [AjaxOnly, ValidateAntiForgeryToken,  CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult GetAppliedLicensesForPrepBulkReg(long id, long? bulkRegistrationId, long[] deviceIds)
        {
            var userAccountId = User.UserAccount.Id;

            var results = _bulkRegistrationLogic.GetAppliedLicensesForPrepBulkReg(userAccountId, id, bulkRegistrationId, deviceIds).ToList();

            var jsonResult = Json(results);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }*/

        [AjaxOnly, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult PrepBulkRegSaveDraft(long id, PrepBulkRegistration draft)
        {
            var currentBulkRegistrationId = _bulkRegistrationLogic.GetCurrentBulkRegistrationId(id, User.UserAccount.Id);
            var userAccountId = User.UserAccount.Id;
            var countryId = draft.DefaultDeviceCountryId.HasValue ? long.Parse(draft.DefaultDeviceCountryId.Value.ToString()) : 0;
            var countryCode = _lookupLogic.GetCountryCodeById(countryId);

            if (countryCode == CountryCode.US.ToDescription() ||
                countryCode == CountryCode.CA.ToDescription())
            {
                draft.DefaultDeviceOtherRegion = string.Empty;
            }
            else
            {
                draft.DefaultDeviceRegionId = null;
            }

            if (draft.DevsAndLics == null || draft.DevsAndLics.Count == 0)
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Success = false,
                        Message = "No devices or licenses selected"
                    }
                };
            }

            var resultingBulkRegId = _bulkRegistrationLogic.CreateUpdateBulkRegistration(currentBulkRegistrationId, draft, userAccountId, id);

            return new JsonResult
            {
                Data = new
                {
                    Success = true,
                    BulkRegistrationId = resultingBulkRegId,
                    Message = "Your bulk registration has been saved successfully"
                }
            };
        }

        [AjaxOnly, ValidateAntiForgeryToken, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult PrepBulkRegDiscardDraft(long id)
        {
            var currentBulkRegistrationId = _bulkRegistrationLogic.GetCurrentBulkRegistrationId(id, User.UserAccount.Id);
            if (currentBulkRegistrationId.HasValue)
            {
                _bulkRegistrationLogic.DiscardDraft(id, User.UserAccount.Id, currentBulkRegistrationId.Value);
            }
            else
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Success = false,
                        Message = "Invalid bulk registration id"
                    }
                };
            }

            return new JsonResult
            {
                Data = new
                {
                    Success = true,
                    Message = "Your bulk registration has been discarded successfully"
                }
            };
        }

        public ActionResult Customize(long id, long? bulkRegistrationId)
        {
            if (!bulkRegistrationId.HasValue)
            {
                return RedirectToAction("Tabs", new {id = id, selectedTab = GroupDestination.PrepBulkRegistration});
            }

            var countries = _lookupLogic.GetCountries().Select(c => new CountryDto()
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name
            }).ToList();

            var regions = _lookupLogic.GetAllRegions().Select(c => new RegionDto()
            {
                Id = c.Id,
                CountryId = c.CountryId,
                Code = c.Code,
                Name = c.Name
            }).ToList();

            var deviceCategories = _deviceLogic.GetAllDeviceCategories(CurrentState.GetCurrentAccountId(),
                User.UserAccount.Id).Select(c => new NameValue()
                {
                    Name = c.DeviceCategoryName,
                    Value = c.DeviceCategoryId.ToString()
                }).ToList();

            countries.Insert(0, new CountryDto()
            {
                Name = Constants.DropDownCountrySelect
            });

            regions.Insert(0, new RegionDto()
            {
                Name = Constants.DropDownRegionSelect
            });

            deviceCategories.Insert(0, new NameValue()
            {
                Name = Constants.DropDownPleaseSelect,
                Value = "0"
            });

            var model = new CustomizeBulkRegistrationDto()
            {
                ActionHelper = new ActionHelper("BulkRegistration", id),
                BulkRegistrationId = bulkRegistrationId,
                SupportAccountId = id,
                Countries = countries,
                Regions = regions,
                DeviceCategories = deviceCategories
            };

            return PartialView("../Partial/_CustomizeBulkRegistration", model);
        }

        [HttpPost, AjaxOnly, ValidateJsonAntiForgeryToken]
        public ActionResult SaveCutomizations(CustomizeBulkRegistrationDto model)
        {
            if (model == null || model.BulkRegistrationDevices == null || !model.BulkRegistrationDevices.Any())
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Success = false,
                        Message = "There are no devices to save"
                    }
                };
            }

            if (model.BulkRegistrationId.HasValue)
            {
                var currentSupportAccountId = CurrentState.GetSupportAccountId() ?? 0;
                
                if (!IsAuthorizedForBulkRegistration(model.BulkRegistrationId, currentSupportAccountId))
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
                
                _bulkRegistrationLogic.UpdateBulkRegistrationDevicesAndLicenses(model.BulkRegistrationId.Value, model.BulkRegistrationDevices.ToList());
            }

            return new JsonResult
            {
                Data = new
                {
                    Success = true,
                    Message = "Your customizations have been saved successfully"
                }
            };
        }

        [HttpPost, AjaxOnly, ValidateJsonAntiForgeryToken]
        public ActionResult FinalizeCustomizations(CustomizeBulkRegistrationDto model)
        {
            var errors = new List<string>();

            if (model == null || model.BulkRegistrationDevices == null || !model.BulkRegistrationDevices.Any())
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Success = false,
                        Message = "There are no devices to save"
                    }
                };
            }

            var currentSupportAccountId = CurrentState.GetSupportAccountId() ?? 0;
            
            if (model.BulkRegistrationId.HasValue && !IsAuthorizedForBulkRegistration(model.BulkRegistrationId, currentSupportAccountId))
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
            
            foreach (var dev in model.BulkRegistrationDevices)
            {
                var validator = new BulkRegistrationDeviceModelValidator();
                var results = validator.Validate(dev);

                if (!results.IsValid)
                {
                    errors.Add(string.Format("{0}_{1}: {2}", dev.SerialNumber, dev.Model, string.Join(", ", results.Errors.Select(error => error.ErrorMessage))));
                }
            }

            if (errors.Count > 0)
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Success = false,
                        Message = "Please fix the following validation errors",
                        Errors = errors.ToArray()
                    }
                };
            }

            var defaultEula = _eulaLogic.GetEula(EulaLanguage.English);
            var ea = new EulaAgreements()
            {
                BulkRegistrationId = model.BulkRegistrationId,
                EulaContextId = EulaContext.BulkRegistration,
                EulaId = defaultEula.Id,
                SupportAccountId = currentSupportAccountId,
                SignedBy = User.UserAccount.Email
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
                            string.Format("Could not add EULA Agreements for Bulk Registration {0}. Eula Id Selected is - {1}",
                                model.BulkRegistrationId, model.Eula.DefaultEula.Id)));
            }

            if (model.BulkRegistrationId.HasValue)
            {
                _bulkRegistrationLogic.UpdateBulkRegistrationState(model.BulkRegistrationId.Value,
                    BulkRegistrationStates.Pending);
            }

            return new JsonResult
            {
                Data = new
                {
                    Success = true,
                    Message = "Your customizations have been saved successfully"
                }
            };
        }

        public ActionResult GetBulkRegistrationDevicesWithLicenses([DataSourceRequest] DataSourceRequest request, long? bulkRegistrationId, long supportAccountId)
        {
            var results = new List<BulkRegistrationDeviceModel>();

            if (bulkRegistrationId.HasValue)
            {
                results = _bulkRegistrationLogic.GetBulkRegistrationDevicesWithLicenses(bulkRegistrationId.Value, supportAccountId);
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Review(long id, long? bulkRegistrationId)
        {
            if (!bulkRegistrationId.HasValue)
            {
                return RedirectToAction("Tabs", new { id = id, selectedTab = GroupDestination.PrepBulkRegistration });
            }

            var countries = _lookupLogic.GetCountries().Select(c => new CountryDto()
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name
            }).ToList();

            var regions = _lookupLogic.GetAllRegions().Select(c => new RegionDto()
            {
                Id = c.Id,
                CountryId = c.CountryId,
                Code = c.Code,
                Name = c.Name
            }).ToList();

            var deviceCategories = _deviceLogic.GetAllDeviceCategories(CurrentState.GetCurrentAccountId(),
                User.UserAccount.Id).Select(c => new NameValue()
                {
                    Name = c.DeviceCategoryName,
                    Value = c.DeviceCategoryId.ToString()
                }).ToList();

            var model = new CustomizeBulkRegistrationDto()
            {
                ActionHelper = new ActionHelper("BulkRegistration", id),
                BulkRegistrationId = bulkRegistrationId,
                SupportAccountId = id,
                IsReviewMode = true,
                Countries = countries,
                Regions = regions,
                DeviceCategories = deviceCategories
            };

            model.Eula = new EulaModel()
            {
                Languages = _eulaLogic.GetEulaLanguages(),
                Eulas = _eulaLogic.GetEula(),
                DefaultEula = _eulaLogic.GetEula(EulaLanguage.English),
                SelectedLanguage = EulaLanguage.English
            };
            model.Eula.IsEulaInMultipleLanguages = model.Eula.Eulas.Count() > 1;
            model.Eula.SelectedEulaId = model.Eula.DefaultEula.Id;
            model.Eula.Context = EulaContext.BulkRegistration;

            return PartialView("../Partial/_BulkRegistrationReview", model);
        }

        public ActionResult History(long id)
        {
            var userAccountId = User.UserAccount.Id;
            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;

            var results = _bulkRegistrationLogic.GetBulkRegistrationHistory(supportAccountId);
            
            var model = new BulkRegistrationHistoryDto()
            {
                SupportAccountId = supportAccountId,
                BulkRegistrations = results
            };

            return View(model);
        }

        public ActionResult ViewHistory(long id, long bulkRegistrationId)
        {
            var model = new CustomizeBulkRegistrationDto()
            {
                ActionHelper = new ActionHelper("BulkRegistration", id),
                BulkRegistrationId = bulkRegistrationId,
                SupportAccountId = id,
                IsReviewMode = true,
                Countries = new List<CountryDto>(),
                Regions = new List<RegionDto>(),
                DeviceCategories = new List<NameValue>()
            };

            return View(model);
        }
    }

    public class BulkRegistrationTabsDto : DefaultDto
    {
        /// <summary>
        /// Don't show customize/review links unless there is at least one device to register
        /// </summary>
        public long? BulkRegistrationId { get; set; }
        public GroupDestination SelectedTab { get; set; }
    }

    public class PrepBulkRegistrationDto : DefaultDto
    {
        public long? BulkRegistrationId { get; set; }
        public string ModelCompatibility { get; set; }
        public SelectDevicesFilterTypes SelectDevicesFilter { get; set; }
        public List<long> SelectedDevices { get; set; }

        public IEnumerable<DeviceCategory> DeviceCategories { get; set; }
        public string DefaultDeviceName { get; set; }
        public long? DefaultDeviceCategoryId { get; set; }
        public string DefaultDeviceAddress1 { get; set; }
        public string DefaultDeviceAddress2 { get; set; }
        public string DefaultDeviceCity { get; set; }
        public long? DefaultDeviceCountryId { get; set; }
        public long? DefaultDeviceRegionId { get; set; }
        public string DefaultDeviceOtherRegion { get; set; }
        public string DefaultDevicePostalCode { get; set; }
        public string DefaultDeviceComments { get; set; }
        public bool IsOverrideDefaults { get; set; }

        //public IEnumerable<object> AvailableLicenses { get; set; }
        public List<PrepBulkRegistration.DeviceAndLicenses> DevsAndLics { get; set; }
    }
}
