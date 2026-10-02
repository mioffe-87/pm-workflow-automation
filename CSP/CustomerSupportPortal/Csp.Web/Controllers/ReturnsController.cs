using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Web.Attributes;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using AutoMapper;
using Csp.Api.Model.Enums;
using Csp.Api.HelperClasses;

namespace Csp.Web.Controllers
{
    [Authorize]
    public class ReturnsController : BaseController
    {
        private readonly IReturnsLogic _returnsLogic;
        private readonly ILookupLogic _lookupLogic;

        public ReturnsController(IReturnsLogic returnsLogic, ILookupLogic lookupLogic)
        {
            _returnsLogic = returnsLogic;
            _lookupLogic = lookupLogic;
        }

        public ActionResult Index()
        {
            try
            {
                var model = new DefaultDto();
                var supportAccountId = CurrentState.GetSupportAccount().Id;
                var userAccountId = User.UserAccount.Id;

                var hasReturnsPortalAccess = _returnsLogic.ShowReturnsPortalLink(supportAccountId, userAccountId);

                if (!hasReturnsPortalAccess)
                    return RedirectToAction("MyAccounts", "SupportAccount");

                return View(model);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return View("../Shared/_Error");
            }
        }

        public ActionResult RequestReturn()
        {
            try
            {
                var model = new RequestReturnDto();
                var devices = new List<RmaDeviceDto>();
                var rmaDevices = _returnsLogic.GetDeviceList(CurrentState.GetSupportAccount().Id, User.UserAccount.Id);
                Mapper.Map(rmaDevices, devices);

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

                countries.Insert(0, new CountryDto()
                {
                    Name = Constants.DropDownCountrySelect
                });

                regions.Insert(0, new RegionDto()
                {
                    Name = Constants.DropDownRegionSelect
                });

                foreach (var d in devices)
                {
                    if (!string.IsNullOrEmpty(d.ShippingCountry))
                    {
                        var country = countries.FirstOrDefault(c => c.Code == d.ShippingCountry);

                        if (country != null)
                        {
                            d.Country = country;
                        }
                    }

                    if (!string.IsNullOrEmpty(d.ShippingRegion))
                    {
                        var region = regions.FirstOrDefault(c => c.Code == d.ShippingRegion);

                        if (region != null)
                        {
                            d.Region = region;
                        }
                    }

                    d.OtherRegion = d.ShippingRegion;
                }

                model = new RequestReturnDto
                {
                    Devices = devices,
                    Countries = countries,
                    Regions = regions
                };

                return PartialView("../Partial/_RequestReturn", model);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return PartialView("../Shared/_Error");
            }
        }

        [HttpPost]
        public ActionResult SaveFile(RmaDeviceDto deviceInfo)
        {
            var message = string.Empty;
            var techSupportFile = deviceInfo.TechSupportFile;

            try
            {
                if (techSupportFile != null && deviceInfo != null)
                {
                    var rmaDevice = new RmaDeviceDetails();

                    //Upload File to server
                    var fileName = Path.GetFileName(techSupportFile.FileName);
                    _returnsLogic.CopyTechSupportFile(techSupportFile.InputStream, fileName);
                    deviceInfo.TechSupportFileName = fileName;

                    //Add TechSupport file to RMA Job Queue for processing
                    var supportAccountId = CurrentState.GetSupportAccount().Id;
                    var userAccountId = User.UserAccount.Id;
                    
                    Mapper.Map(deviceInfo, rmaDevice);
                    rmaDevice.TechSupportFileName = fileName;

                    var isQueuedForProcessing = _returnsLogic.QueueTechSuppportFileForProcessing(fileName, rmaDevice, supportAccountId, userAccountId);

                    if (!isQueuedForProcessing)
                    {
                        return new JsonResult
                        {
                            Data = new
                            {
                                Success = false,
                                Message =
                                string.Format("Unable to upload Tech Support file for {0}", deviceInfo.SerialNumber)
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);

                return new JsonResult
                {
                    Data = new
                    {
                        Success = false,
                        Message = string.Format("Unable to upload Tech Support file for {0}", deviceInfo.SerialNumber)
                    }
                };
            }

            return new JsonResult
            {
                Data = new
                {
                    Success = true,
                    Message = string.Format("The Serial Number has been submitted for processing. This process make take some time, please check later.")
                }
            };
        }
        
    }
}
