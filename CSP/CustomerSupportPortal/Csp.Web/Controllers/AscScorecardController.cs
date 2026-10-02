using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Web.DataTransferObjects;
using AutoMapper;
using Csp.Web.HelperClasses;
using Kendo.Mvc.UI;
using Csp.Web.Security.Attributes;
using Csp.Api.Security;
using FluentValidation.Results;
using System.Globalization;
using System.Web.Caching;
using Kendo.Mvc.Extensions;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Security.Application;

namespace Csp.Web.Controllers
{
		/*
    [AscDashboardAttribute]
    public class AscScorecardController : BaseController
    {
        private const string ASCDATA_FILENAME = "ASCDATA";
        private readonly IAscScoreCardLogic _ascScoreCardLogic;
        private readonly ISupportAccountLogic _supportAccountLogic;
        private readonly IAscDeviceRegistrationService _ascDeviceRegistrationService;

        public AscScorecardController(IAscScoreCardLogic ascScoreCardLogic, ISupportAccountLogic supportAccountLogic, IAscDeviceRegistrationService ascDeviceRegistrationService)
        {
            _ascScoreCardLogic = ascScoreCardLogic;
            _supportAccountLogic = supportAccountLogic;
            _ascDeviceRegistrationService = ascDeviceRegistrationService;
        }


        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                if (CurrentState.GetSupportAccountId() != null &&
                    !String.IsNullOrEmpty(CurrentState.GetSupportAccountName()))
                {
                    //	var ascScoreCardDto = GetLastFiveQuaterData("0017000001DcE2nAAF", true, "");

                    var ascScoreCardDto = new AscScoreCardDto();

                    ascScoreCardDto.IsLastFiveQtr = true;
                    ascScoreCardDto.Theatre = "ALL";
                    if (User.ActivePermissions.Contains(Permission.ViewASCDashboardReport))
                    {
                        ascScoreCardDto.SFDCAccountId = "ALL";
                        ascScoreCardDto.IsInternal = true;
                    }
                    else
                    {
                        var thisUser = (CspPrincipal)User;
                        if (thisUser.SupportAccountIdForPermissionSet != null)
                        {
                            ascScoreCardDto.SFDCAccountId =
                                _supportAccountLogic.GetSalesforceIdBySupportAccountId(
                                    Convert.ToInt64(thisUser.SupportAccountIdForPermissionSet));
                        }
                        ascScoreCardDto.IsInternal = false;
                    }
                    ascScoreCardDto.IsincludeCurrentPartial = "1";
                    ascScoreCardDto = GetLastFiveQuaterData(ascScoreCardDto.SFDCAccountId, ascScoreCardDto.IsInternal,
                    ascScoreCardDto.Theatre, ascScoreCardDto.IsincludeCurrentPartial);

                    ascScoreCardDto.LastRefreshDate = _ascScoreCardLogic.GetLastRefreshDate();

                    return View(ascScoreCardDto);
                }
                return View("../Error/SessionExpired");
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                bool showMaintenanceMessage;
                bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["ShowReportMaintenanceMessage"], out showMaintenanceMessage);
                return showMaintenanceMessage ? View("../Error/ReportMaintenance") : View("../Shared/_Error");
            }
        }


        public JsonResult GetAscUserAccountList(string name, string theatre)
        {
            List<AscPartner> ascUserList;
            if (!string.IsNullOrEmpty(theatre) && theatre.ToUpper().Equals("GLOBAL"))
            {
                ascUserList = (List<AscPartner>)System.Web.HttpContext.Current.Cache.Get("AscScoreCardGlobalAscUserList");
            }
            else
            {
                ascUserList = (List<AscPartner>)System.Web.HttpContext.Current.Cache.Get("AscScoreCardAscUserList");
            }

            if (ascUserList==null || ascUserList.Count == 0)
            {
                if (!string.IsNullOrEmpty(theatre) && theatre.ToUpper().Equals("GLOBAL"))
                {
                    ascUserList = _ascScoreCardLogic.GetGlobalAscPartnerList();
                    System.Web.HttpContext.Current.Cache.Add("AscScoreCardGlobalAscUserList", ascUserList,
                        null, DateTime.Now.AddSeconds(3600), Cache.NoSlidingExpiration, CacheItemPriority.High, null);
                }
                else
                {
                    ascUserList = _ascScoreCardLogic.GetAllAscPartnerList();
                    System.Web.HttpContext.Current.Cache.Add("AscScoreCardAscUserList", ascUserList,
                        null, DateTime.Now.AddSeconds(3600), Cache.NoSlidingExpiration, CacheItemPriority.High, null);
                }
            }
            List<DropDownModel> ascUserFilteredList = null;
            if (!string.IsNullOrEmpty(theatre) && theatre.ToUpper().Equals("GLOBAL"))
            {
                ascUserFilteredList =
                       (from a in ascUserList select new DropDownModel { Id = a.AccountId, Value = a.PartnerName })
                           .OrderBy(a => a.Value).ToList();
            }
            else
            {

                name = (string.IsNullOrEmpty(name) || name.Trim().ToUpper().Equals("ALL ASC") ||
                        name.Trim().ToUpper().Equals("ALL"))
                    ? string.Empty
                    : name.Trim().ToUpper();
                theatre = (string.IsNullOrEmpty(theatre) || theatre.Trim().ToUpper().Equals("ALL"))
                    ? string.Empty
                    : theatre.Trim().ToUpper();

                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(theatre))
                {
                    ascUserFilteredList =
                        (from a in ascUserList select new DropDownModel { Id = a.AccountId, Value = a.PartnerName })
                            .OrderBy(a => a.Value).ToList();
                }
                else if (!string.IsNullOrEmpty(theatre) && string.IsNullOrEmpty(name))
                {
                    ascUserFilteredList = (from a in ascUserList
                                           where a.Theatre.ToUpper().Equals(theatre)
                                           select new DropDownModel { Id = a.AccountId, Value = a.PartnerName }).OrderBy(a => a.Value)
                        .ToList();
                }
                else if (string.IsNullOrEmpty(theatre) && !string.IsNullOrEmpty(name))
                {
                    ascUserFilteredList = (from a in ascUserList
                                           where a.PartnerName.ToUpper().StartsWith(name)
                                           select new DropDownModel { Id = a.AccountId, Value = a.PartnerName }).OrderBy(a => a.Value)
                        .ToList();
                }
                else
                {

                    ascUserFilteredList = (from a in ascUserList
                                           where a.Theatre.ToUpper().Equals(theatre) && a.PartnerName.ToUpper().StartsWith(name)
                                           select new DropDownModel { Id = a.AccountId, Value = a.PartnerName }).OrderBy(a => a.Value)
                        .ToList();
                }
            }
            //if (ascUserFilteredList.Count > 0)
            //{
            //    DropDownModel allAscObj = new DropDownModel {Value = "All ASC", Id = string.Empty};
            //    ascUserFilteredList.Insert(0, allAscObj);
            //}
            return Json(ascUserFilteredList, JsonRequestBehavior.AllowGet);

        }

        private AscScoreCardDto GetLastFiveQuaterData(string sfdcAccountId, bool isInternal, string theatre, string includeCurrentPartial)
        {
           var sfdcAccountIdFormatted=SFDCAccountFormatting(sfdcAccountId);

           AscScoreCard ascScoreCard = _ascScoreCardLogic.GetAscScoreCardQtrData(sfdcAccountIdFormatted, isInternal, theatre, includeCurrentPartial);

            AscScoreCardDto ascScoreCardDto = new AscScoreCardDto();
            ascScoreCardDto.IsLastFiveQtr = true;
            ascScoreCardDto.SFDCAccountId = sfdcAccountId;
            ascScoreCardDto.IsInternal = isInternal;
            ascScoreCardDto.Theatre = theatre;
            ascScoreCardDto.IsincludeCurrentPartial = includeCurrentPartial;

            Mapper.Map(ascScoreCard, ascScoreCardDto);
            Mapper.Map(ascScoreCard.PartnerProfile, ascScoreCardDto.PartnerProfile);

            Mapper.Map(ascScoreCard.AscDeviceTheatrePartnerChart, ascScoreCardDto.AscDeviceTheatrePartnerChart);

            Mapper.Map(ascScoreCard.InternalDeviceInformationChart, ascScoreCardDto.InternalDeviceInformationChart);
            Mapper.Map(ascScoreCard.InternalCustomerInformationChart, ascScoreCardDto.InternalCustomerInformationChart);

            Mapper.Map(ascScoreCard.FrontlineMetricsConsolidated, ascScoreCardDto.FrontlineMetricsConsolidated);
            Mapper.Map(ascScoreCard.ResolutionMetricsConsolidated, ascScoreCardDto.ResolutionMetricsConsolidated);
            Mapper.Map(ascScoreCard.CsatTop3LineChart, ascScoreCardDto.CsatTop3LineChart);
            Mapper.Map(ascScoreCard.CsatResponseLineChart, ascScoreCardDto.CsatResponseLineChart);

            return ascScoreCardDto;
        }

        public ActionResult LoadInternalData(bool isLastFiveMonth, string userAccountId, string theatre, string isIncludeCurrentPartial)
        {
            try
            {
                if (CurrentState.GetSupportAccountId() != null &&
                    !String.IsNullOrEmpty(CurrentState.GetSupportAccountName()))
                {
                    theatre = string.IsNullOrEmpty(theatre) ? "ALL" : theatre;
                    userAccountId = string.IsNullOrEmpty(userAccountId) ? "ALL" : userAccountId;

                    if (isLastFiveMonth)
                    {
                        return LoadLastFiveMonthPartnerData(userAccountId, true, theatre, isIncludeCurrentPartial);
                    }
                    return LoadLastFiveQuaterPartnerData(userAccountId, true, theatre, isIncludeCurrentPartial);
                }
                return PartialView("../Error/SessionExpired");
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return PartialView("../Shared/_Error");
            }
        }

        public ActionResult LoadLastFiveQuaterPartnerData(string sfdcAccountId, bool isInternal, string theatre, string isIncludeCurrentPartial)
        {
            try
            {

                var ascScoreCardDto = GetLastFiveQuaterData(sfdcAccountId, isInternal, theatre, isIncludeCurrentPartial);
                ascScoreCardDto.IsInternal = isInternal;
                ascScoreCardDto.IsLastFiveQtr = true;
                ascScoreCardDto.SFDCAccountId = sfdcAccountId;
                ascScoreCardDto.Theatre = theatre;
                ascScoreCardDto.IsincludeCurrentPartial = isIncludeCurrentPartial;

                return PartialView("../AscScorecard/_AscScorecard", ascScoreCardDto);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return PartialView("../Shared/_Error");
            }
        }

        public ActionResult LoadLastFiveMonthPartnerData(string sfdcAccountId, bool isInternal, string theatre, string isIncludeCurrentPartial)
        {
            try
            {
                //var  ascScoreCard = _ascScoreCardLogic.GetAscScoreCardLastFiveMonthData("0017000001DcE2nAAF", false, "");
                AscScoreCardDto ascScoreCardDto = GetLastFiveMonthData(sfdcAccountId, isInternal, theatre, isIncludeCurrentPartial);

                return PartialView("../AscScorecard/_AscScorecard", ascScoreCardDto);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return PartialView("../Shared/_Error");
            }
        }

        private string SFDCAccountFormatting(string sfdcAccountId)
        {
            if (!string.IsNullOrEmpty(sfdcAccountId))
            {
                string[] sfdcAccountIdList = sfdcAccountId.Split(',');
                if (sfdcAccountIdList.Length > 1)
                {
                    for (int i = 0; i < sfdcAccountIdList.Length; i++)
                    {
                        sfdcAccountIdList[i] = String.Format("''{0}''", sfdcAccountIdList[i]);
                    }
                    sfdcAccountId = string.Join(",", sfdcAccountIdList);
                }
                else
                {
                    sfdcAccountId = String.Format("''{0}''", sfdcAccountId);
                }
            }
            return sfdcAccountId;
        }

        private AscScoreCardDto GetLastFiveMonthData(string sfdcAccountId, bool isInternal, string theatre, string isIncludeCurrentPartial)
        {
           var sfdcAccountIdFormatted= SFDCAccountFormatting(sfdcAccountId);
            var ascScoreCard = _ascScoreCardLogic.GetAscScoreCardLastFiveMonthData(sfdcAccountIdFormatted, isInternal, theatre, isIncludeCurrentPartial);

            AscScoreCardDto ascScoreCardDto = new AscScoreCardDto();
            ascScoreCardDto.IsLastFiveQtr = false;
            ascScoreCardDto.IsInternal = isInternal;
            ascScoreCardDto.SFDCAccountId = sfdcAccountId;
            ascScoreCardDto.Theatre = theatre;
            ascScoreCardDto.IsincludeCurrentPartial = isIncludeCurrentPartial;

            Mapper.Map(ascScoreCard, ascScoreCardDto);
            Mapper.Map(ascScoreCard.PartnerProfile, ascScoreCardDto.PartnerProfile);

            Mapper.Map(ascScoreCard.AscDeviceTheatrePartnerChart, ascScoreCardDto.AscDeviceTheatrePartnerChart);

            Mapper.Map(ascScoreCard.InternalDeviceInformationChart, ascScoreCardDto.InternalDeviceInformationChart);
            Mapper.Map(ascScoreCard.InternalCustomerInformationChart, ascScoreCardDto.InternalCustomerInformationChart);

            Mapper.Map(ascScoreCard.FrontlineMetricsConsolidated, ascScoreCardDto.FrontlineMetricsConsolidated);
            Mapper.Map(ascScoreCard.ResolutionMetricsConsolidated, ascScoreCardDto.ResolutionMetricsConsolidated);
            Mapper.Map(ascScoreCard.CsatTop3LineChart, ascScoreCardDto.CsatTop3LineChart);
            Mapper.Map(ascScoreCard.CsatResponseLineChart, ascScoreCardDto.CsatResponseLineChart);
            return ascScoreCardDto;
        }

        public ActionResult GetAscSoftwareEngineer([DataSourceRequest]DataSourceRequest request, string isLastFiveQtr, string sfdcAccountId, bool isInternal, string theatre, string isIncludeCurrentPartial)
        {

            var sortInformation = request.Sorts.FirstOrDefault();
            string sortExpression, sortDirection;
            if (sortInformation == null)
            {
                sortExpression = "Total_Case_Count";
                sortDirection = "DESC";
            }
            else
            {
                sortExpression = sortInformation.Member;
                sortDirection = sortInformation.SortDirection.ToString().Equals("Ascending") ? "ASC" : "DESC";
            }
            int totalCount = 0;
            sfdcAccountId = string.IsNullOrEmpty(sfdcAccountId) ? "ALL" : sfdcAccountId;
            sfdcAccountId = SFDCAccountFormatting(sfdcAccountId).Replace("'", "");
            theatre = !string.IsNullOrEmpty(theatre) ? theatre.Replace("'", "") : String.Empty;
            isIncludeCurrentPartial = !string.IsNullOrEmpty(isIncludeCurrentPartial) ? isIncludeCurrentPartial.Replace("'", "") : String.Empty;

            var reportList = _ascScoreCardLogic.GetAscSoftwareEngineer(sfdcAccountId, isInternal, theatre, Convert.ToBoolean(isLastFiveQtr), isIncludeCurrentPartial, request.Page, request.PageSize, sortExpression, sortDirection);

            if (reportList != null && reportList.Count > 0)
            {
                totalCount = _ascScoreCardLogic.GetAscSoftwareEngineerTotalCount(sfdcAccountId, isInternal, theatre, Convert.ToBoolean(isLastFiveQtr), isIncludeCurrentPartial);
            }
            List<SoftwareEngineerDto> reportDetailDto = Mapper.Map<List<SoftwareEngineer>, List<SoftwareEngineerDto>>(reportList);


            var result = new DataSourceResult()
            {
                Data = reportDetailDto, // Process data (paging and sorting applied)
                Total = totalCount // Total number of records
            };

            return Json(result);



        }


        [HttpGet]
        public ActionResult FrontLineMetricsDetail(bool isFiveQuater, string sfdcAccountId, bool isInternal, string theatre, string isIncludeCurrentPartial)
        {
            sfdcAccountId = string.IsNullOrEmpty(sfdcAccountId) ? "ALL" : sfdcAccountId;
            sfdcAccountId = SFDCAccountFormatting(sfdcAccountId).Replace("'", "");
            theatre = !string.IsNullOrEmpty(theatre) ? theatre.Replace("'", "") : String.Empty;
            isIncludeCurrentPartial = !string.IsNullOrEmpty(isIncludeCurrentPartial) ? isIncludeCurrentPartial.Replace("'", "") : String.Empty;
            try
            {

                DataSet ds = _ascScoreCardLogic.GetFrontLineMetricsDetail(sfdcAccountId, isInternal, theatre, isFiveQuater, isIncludeCurrentPartial);
                //ExportToExcel(System.Web.HttpContext.Current.Response, ds, "FrontLineMetricsDetail.csv");
                ExcelHelper.CreateExcelDocumentAsStream(ds, "FrontLineMetricsDetail.xls", System.Web.HttpContext.Current.Response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return View("../Shared/_Error");
            }
            return null;
        }

        [HttpGet]
        public ActionResult ResolutionMetricsDetail(bool isFiveQuater, string sfdcAccountId, bool isInternal, string theatre, string isIncludeCurrentPartial)
        {
            sfdcAccountId = string.IsNullOrEmpty(sfdcAccountId) ? "ALL" : sfdcAccountId;
            sfdcAccountId = SFDCAccountFormatting(sfdcAccountId).Replace("'", "");
            theatre = !string.IsNullOrEmpty(theatre) ? theatre.Replace("'", "") : String.Empty;
            isIncludeCurrentPartial = !string.IsNullOrEmpty(isIncludeCurrentPartial) ? isIncludeCurrentPartial.Replace("'", "") : String.Empty; 
            try
            {
                DataSet ds = _ascScoreCardLogic.GetResolutionMetricsExport(sfdcAccountId, isInternal, theatre, isFiveQuater, isIncludeCurrentPartial);
                //ExportToExcel(System.Web.HttpContext.Current.Response, ds, "FrontLineMetricsDetail.csv");
                ExcelHelper.CreateExcelDocumentAsStream(ds, "ResolutionMetricsDetail.xls", System.Web.HttpContext.Current.Response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return View("../Shared/_Error");
            }
            return null;
        }

        [HttpGet]
        public ActionResult CustomerCommentsDownload(bool isFiveQuater, string sfdcAccountId, bool isInternal, string theatre, string isIncludeCurrentPartial)
        {
            sfdcAccountId = string.IsNullOrEmpty(sfdcAccountId) ? "ALL" : sfdcAccountId;
            sfdcAccountId = SFDCAccountFormatting(sfdcAccountId).Replace("'", "");
            theatre = !string.IsNullOrEmpty(theatre) ? theatre.Replace("'", "") : String.Empty;
            isIncludeCurrentPartial = !string.IsNullOrEmpty(isIncludeCurrentPartial) ? isIncludeCurrentPartial.Replace("'", "") : String.Empty; 
            try
            {
                DataSet ds = _ascScoreCardLogic.GetCustomerComments(sfdcAccountId, isInternal, theatre, isFiveQuater, isIncludeCurrentPartial);
                //ExportToExcel(System.Web.HttpContext.Current.Response, ds, "CustomerComments.csv");

                ExcelHelper.CreateExcelDocumentAsStream(ds, "CustomerComments.xls", System.Web.HttpContext.Current.Response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return View("../Shared/_Error");
            }
            return null;
        }

        [HttpGet]
        public ActionResult CustomerHighScoreCommentsDownload(bool isFiveQuater, string sfdcAccountId, bool isInternal, string theatre, string isIncludeCurrentPartial)
        {
            sfdcAccountId = string.IsNullOrEmpty(sfdcAccountId) ? "ALL" : sfdcAccountId;
            sfdcAccountId = SFDCAccountFormatting(sfdcAccountId).Replace("'", "");
            theatre = !string.IsNullOrEmpty(theatre) ? theatre.Replace("'", "") : String.Empty;
            isIncludeCurrentPartial = !string.IsNullOrEmpty(isIncludeCurrentPartial) ? isIncludeCurrentPartial.Replace("'", "") : String.Empty;
            try
            {
                DataSet ds = _ascScoreCardLogic.GetHighScoreCustomerComments(sfdcAccountId, isInternal, theatre, isFiveQuater, isIncludeCurrentPartial);
                //ExportToExcel(System.Web.HttpContext.Current.Response, ds, "CustomerComments.csv");

                ExcelHelper.CreateExcelDocumentAsStream(ds, "CustomerHighScoreComments.xls", System.Web.HttpContext.Current.Response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return View("../Shared/_Error");
            }
            return null;
        }

        [HttpGet]
        public ActionResult DeviceInformationDownload(bool isFiveQuater, string sfdcAccountId, bool isInternal, string theatre, string isIncludeCurrentPartial)
        {
            sfdcAccountId = string.IsNullOrEmpty(sfdcAccountId) ? "ALL" : sfdcAccountId;
            sfdcAccountId = SFDCAccountFormatting(sfdcAccountId).Replace("'", "");
            theatre = !string.IsNullOrEmpty(theatre) ? theatre.Replace("'", "") : String.Empty;
            isIncludeCurrentPartial = !string.IsNullOrEmpty(isIncludeCurrentPartial) ? isIncludeCurrentPartial.Replace("'", "") : String.Empty;
            try
            {
                DataSet ds = _ascScoreCardLogic.ExportDeviceInformation(sfdcAccountId, isInternal, theatre, isFiveQuater, isIncludeCurrentPartial);
                //ExportToExcel(System.Web.HttpContext.Current.Response, ds, "CustomerComments.csv");

                ExcelHelper.CreateExcelDocumentAsStream(ds, "DeviceCustomerInformation.xls", System.Web.HttpContext.Current.Response);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return View("../Shared/_Error");
            }
            return null;
        }

        [HttpGet]
        public ActionResult AscSupportEngineerDownload(bool isFiveQuater, string sfdcAccountId, bool isInternal, string theatre, string isIncludeCurrentPartial)
        {
            sfdcAccountId = string.IsNullOrEmpty(sfdcAccountId) ? "ALL" : sfdcAccountId;
            sfdcAccountId = SFDCAccountFormatting(sfdcAccountId).Replace("'", "");
            theatre = !string.IsNullOrEmpty(theatre) ? theatre.Replace("'", "") : String.Empty;
            isIncludeCurrentPartial = !string.IsNullOrEmpty(isIncludeCurrentPartial) ? isIncludeCurrentPartial.Replace("'", "") : String.Empty;
            try
            {
                DataSet ds = _ascScoreCardLogic.ExportAscEngineer(sfdcAccountId, isInternal, theatre, isFiveQuater, isIncludeCurrentPartial);

                ExcelHelper.CreateExcelDocumentAsStream(ds, "AscSupportEngineer.xls", System.Web.HttpContext.Current.Response);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return View("../Shared/_Error");
            }
            return null;
        }


        [HttpGet]
        public ActionResult AscReminder()
        {
            try
            {
                _ascScoreCardLogic.AscReminderEmail();

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return View("../Shared/_Error");
            }
            return null;
        }

        [HttpGet]
        public ActionResult UploadAscData()
        {
            try
            {
                if (CurrentState.GetSupportAccountId() != null &&
                    !String.IsNullOrEmpty(CurrentState.GetSupportAccountName()))
                {
                    var viewModel = new UploadAscDataDto();
                    var supportAccount = CurrentState.GetSupportAccount();
                    var supportAccountId = supportAccount.Id;

                    viewModel.LastUploadedOn = _ascScoreCardLogic.GetAscDataLastUploadedOn(supportAccountId);

                    return PartialView("../AscScorecard/_UploadAscData", viewModel);
                }
                return PartialView("../Error/SessionExpired");
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return PartialView("../Shared/_Error");
            }

        }

        [HttpPost]
        public ActionResult UploadAscData(UploadAscDataDto viewModel)
        {
            var errorMessage = string.Empty;
            var isValid = true;

            try
            {
                var errors = new List<string>();

                if (ModelState.IsValid)
                {
                    if (viewModel.Attachment != null && viewModel.Attachment.ContentLength > 0)
                    {
                        var csvFile = viewModel.Attachment;
                        var ascDataList = new List<AscDataItemDto>();
                        var fields = new List<string[]>();
                        var userAccountId = User.UserAccount.Id;
                        var supportAccount = CurrentState.GetSupportAccount();
                        var supportAccountId = supportAccount.Id;
                        string[] fileNameTokens = Path.GetFileNameWithoutExtension(viewModel.Attachment.FileName.ToUpper())
                            .Split(new[] { '_' }, StringSplitOptions.None);
                        long ascId = 0;
                        var fileName = string.Empty;
                        int month = 0;
                        int year = 0;
                        bool hasDataForProcessedMonth = false;

                        if (fileNameTokens.Length == 4)
                        {
                            long.TryParse(fileNameTokens[0], out ascId);
                            fileName = fileNameTokens[1];
                            int.TryParse(fileNameTokens[2], out year);
                            int.TryParse(fileNameTokens[3], out month);
                        }

                        if (fileNameTokens.Length != 4 || fileName != ASCDATA_FILENAME || !(month >= 1 && month <= 12) || !(year >= DateTime.Now.Year - 1 && year <= DateTime.Now.Year))
                        {
                            isValid = false;
                            errorMessage =
                                "Invalid filename.  Please correct the filename and upload again.  Example: {ASCId}_ASCData_{YYYY}_{MM}.csv";
                        }
                        else if (ascId != supportAccountId ||
                                 !_ascDeviceRegistrationService.IsMemberOfAsc(userAccountId, supportAccountId))
                        {
                            isValid = false;
                            errorMessage = "Invalid AscId. Please verify.";
                        }

                        if (isValid)
                        {
                            var csvParser = new TextFieldParser(csvFile.InputStream) { HasFieldsEnclosedInQuotes = true };

                            csvParser.SetDelimiters(",");

                            while (!csvParser.EndOfData)
                            {
                                fields.Add(csvParser.ReadFields());
                            }

                            csvParser.Close();

                            var headerColumns = fields[0];

                            if (headerColumns.Length != 5 || !IsValidAscDataColumns(headerColumns))
                            {
                                isValid = false;
                                errorMessage = "Field names do not meet our requirements.  Please correct the field names and upload again.";
                            }

                            fields.Remove(fields[0]);

                            if (isValid)
                            {
                                var count = 1;

                                foreach (var item in fields)
                                {
                                    count++;
                                    var ascDataItem = new AscDataItemDto();
                                    var createdOnDateString = !string.IsNullOrEmpty(item[0])
                                        ? item[0].Trim()
                                        : string.Empty;
                                    var caseNumber = !string.IsNullOrEmpty(item[1]) ? item[1].Trim() : string.Empty;
                                    var customerName = !string.IsNullOrEmpty(item[2]) ? item[2].Trim() : string.Empty;
                                    var customerEmail = !string.IsNullOrEmpty(item[3]) ? item[3].Trim() : string.Empty;
                                    var closedOnDateString = !string.IsNullOrEmpty(item[4])
                                        ? item[4].Trim()
                                        : string.Empty;
                                    var processedMonth = string.Format("{0}-{1}", year, month.ToString().PadLeft(2, '0'));
                                    DateTime createdOnDate;
                                    DateTime closedOnDate;

                                    DateTime.TryParseExact(createdOnDateString,
                                        SiteHelper.ValidAscScorecardUploadDateFormats,
                                        CultureInfo.InvariantCulture, DateTimeStyles.None,
                                        out createdOnDate);
                                    DateTime.TryParseExact(closedOnDateString,
                                        SiteHelper.ValidAscScorecardUploadDateFormats,
                                        CultureInfo.InvariantCulture, DateTimeStyles.None,
                                        out closedOnDate);

                                    ascDataItem.Id = count;
                                    ascDataItem.AscId = ascId;
                                    ascDataItem.SalesforceAccountId = supportAccount.SalesforceId;
                                    ascDataItem.CreatedOnDateString = createdOnDateString;
                                    ascDataItem.CreatedOn = createdOnDate;
                                    ascDataItem.CaseNumber = caseNumber;
                                    ascDataItem.CustomerName = customerName;
                                    ascDataItem.CustomerEmail = customerEmail;
                                    ascDataItem.ProcessedMonth = processedMonth;
                                    ascDataItem.ClosedOnDateString = closedOnDateString;

                                    if (closedOnDate != DateTime.MinValue)
                                    {
                                        ascDataItem.ClosedOn = closedOnDate;
                                    }

                                    if (!hasDataForProcessedMonth && createdOnDate.Month == month &&
                                        createdOnDate.Year == year)
                                    {
                                        hasDataForProcessedMonth = true;
                                    }

                                    var validator = new AscDataValidator();
                                    var results = validator.Validate(ascDataItem);

                                    if (!results.IsValid)
                                    {
                                        errors.Add(string.Format("Error at Row {0}:", count));

                                        errors.AddRange(results.Errors.Select(error => error.ErrorMessage));
                                    }
                                    else
                                    {
                                        ascDataList.Add(ascDataItem);
                                    }
                                }

                                if (!hasDataForProcessedMonth)
                                {
                                    return new JsonResult
                                    {
                                        Data = new
                                        {
                                            Success = false,
                                            Message = string.Format("The file does not contain any case data for the month {0} specified on the filename.  Please correct this and upload the file again.", CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month))
                                        }
                                    };
                                }

                                var duplicateCaseNumbers = (from a in ascDataList
                                                            group a by a.CaseNumber
                                                                into g
                                                                where g.Count() > 1
                                                                select g.Key).ToArray();

                                if (duplicateCaseNumbers.Any())
                                {
                                    var caseNumbers = string.Join(", ", duplicateCaseNumbers);

                                    return new JsonResult
                                    {
                                        Data = new
                                        {
                                            Success = false,
                                            Message = string.Format("Your file contains the following duplicate case number(s): {0}. Please verify.", caseNumbers)
                                        }
                                    };
                                }

                                if (errors.Count == 0)
                                {
                                    var ascData =
                                        Mapper.Map<IEnumerable<AscDataItemDto>, IEnumerable<AscDataModel>>(ascDataList)
                                            .ToList();
                                    _ascScoreCardLogic.InsertAscData(ascData);

                                    return new JsonResult
                                    {
                                        Data = new
                                        {
                                            Success = true,
                                            Message = "Your data has been imported successfully"
                                        }
                                    };
                                }

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
                        }
                    }
                }
                else
                {
                    errorMessage = string.Join(" ",
                        ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList());
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
                        Message = @"An error occurred while trying to import your data. Please contact <a href=""mailto:asc-program@paloaltonetworks.com"">asc-program@paloaltonetworks.com</a>"
                    }
                };
            }

            return new JsonResult
            {
                Data = new
                {
                    Success = false,
                    Message = errorMessage
                }
            };
        }

        private void ExportToExcel(HttpResponse response, DataSet ds, string fileName)
        {

            if (ds != null && ds.Tables.Count > 0)
            {
                response.Clear();
                response.ClearContent();
                response.BufferOutput = false;
                response.AppendCookie(new HttpCookie("fileDownload", "true") { Path = "/" });
                response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName);

                var sbLines = new StringBuilder();

                //column headers
                sbLines.AppendLine(string.Join(",", ds.Tables[0].Columns.Cast<DataColumn>()
                                                                                                                .Select(s => "\"" + s.ColumnName + "\"")));

                //rows
                ds.Tables[0].AsEnumerable().Select(s => sbLines.AppendLine(
                    string.Join(",", s.ItemArray.Select(
                        i => "\"" + i.ToString() + "\"")))).ToList();

                using (var streamWriter = new StreamWriter(response.OutputStream, Encoding.UTF8))
                {
                    streamWriter.WriteLine(sbLines.ToString());
                    streamWriter.Flush();
                }


                response.Flush();
                response.End();
            }
        }

        private bool IsValidAscDataColumns(string[] columns)
        {
            return (columns[0].Trim().ToUpper() == "CREATIONDATE"
                    && columns[1].Trim().ToUpper() == "CASENUMBER"
                    && columns[2].Trim().ToUpper() == "ENDCUSTOMERNAME"
                    && columns[3].Trim().ToUpper() == "ENDCUSTOMEREMAIL"
                    && columns[4].Trim().ToUpper() == "CLOSEDDATE");
        }
    }
		*/
}
