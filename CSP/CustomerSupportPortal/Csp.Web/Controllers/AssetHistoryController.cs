using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Web.DataTransferObjects;
using Csp.Web.HelperClasses;
using Csp.Web.Security.Attributes;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using NPOI.HSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Csp.Web.Controllers
{
    [Authorize]
    public class AssetHistoryController : BaseController
    {
        private readonly IAssetHistoryLogic _assetHistoryLogic;

        public AssetHistoryController(IAssetHistoryLogic assetHistoryLogic)
        {
            _assetHistoryLogic = assetHistoryLogic;
        }

        [System.Web.Mvc.HttpGet, CspAuthorize(Permission.ViewSupportAccount)]
        public ActionResult ViewHistory(long id)
        {
            var model = new AssetHistoryDto()
            {
                ActionHelper = new ActionHelper("SupportAccountId", id),
                AssetHistory = new AssetHistoryModel()
                {
                    SearchModel = new AssetHistorySearchModel()
                    {
                        StartDate = DateTime.Now.AddMonths(-2),
                        EndDate = DateTime.Now
                    }
                }
            };

            return PartialView("../Partial/_AssetHistory", model);
        }

        [CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult GetAssetHistory([DataSourceRequest] DataSourceRequest request, string code, DateTime? startDate, DateTime? endDate)
        {
            var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
            var userAccountId = User.UserAccount.Id;
            var dataParameters = request.ToDataParameters();

            var results = _assetHistoryLogic.GetAssetHistory(code, startDate, endDate, supportAccountId, userAccountId, dataParameters, User.RoleSecurities, false);

            var result = new DataSourceResult()
            {
                Data = results,
                Total = (int)dataParameters.TotalRecordCount
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, CspAuthorize(Permission.ViewSupportAccountDevices)]
        public ActionResult ExportAssetHistory(string code, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var supportAccountId = CurrentState.GetSupportAccountId() ?? 0;
                var userAccountId = User.UserAccount.Id;

                var assets = _assetHistoryLogic.GetAssetHistory(code, startDate, endDate, supportAccountId, userAccountId, null, User.RoleSecurities, true);
                List<AssetHistorySpareLicenseTransferDetails> transferredSpareLicenses = new List<AssetHistorySpareLicenseTransferDetails>();

                foreach (var item in assets)
                {
                    if (item.SpareLicenseTransferDetails != null && item.SpareLicenseTransferDetails.Count > 0)
                    {
                        transferredSpareLicenses.AddRange(item.SpareLicenseTransferDetails);
                    }
                }
                
                // Create new Excel workbook
                var workbook = new HSSFWorkbook();

                // Create new Excel sheet
                var sheet = workbook.CreateSheet("Asset Details");

                // (Optional) set the width of the columns
                sheet.SetColumnWidth(0, 40 * 256);
                sheet.SetColumnWidth(1, 60 * 256);
                sheet.SetColumnWidth(2, 50 * 256);
                sheet.SetColumnWidth(3, 40 * 256);
                sheet.SetColumnWidth(4, 40 * 256);
                sheet.SetColumnWidth(5, 60 * 256);
                sheet.SetColumnWidth(6, 60 * 256);
                sheet.SetColumnWidth(7, 50 * 256);

                // Create header row
                var headerRow = sheet.CreateRow(0);

                //Set column names for the header row
                headerRow.CreateCell(0).SetCellValue("Asset Type");
                headerRow.CreateCell(1).SetCellValue("Serial Number/Auth Code");
                headerRow.CreateCell(2).SetCellValue("Model/Feature Name");
                headerRow.CreateCell(3).SetCellValue("Date Registered");
                headerRow.CreateCell(4).SetCellValue("Date Purchased");
                headerRow.CreateCell(5).SetCellValue("Registered/Activated By");
                headerRow.CreateCell(6).SetCellValue("Registered/Activated Email");
                headerRow.CreateCell(7).SetCellValue("Date Registered/Activated");

                // (Optional) freeze the header row so it is not scrolled
                sheet.CreateFreezePane(0, 1, 0, 1);

                int rowNumber = 1;

                // Create Content
                foreach (var asset in assets)
                {
                    //Create a new row
                    var row = sheet.CreateRow(rowNumber++);

                    var registeredByName = string.Empty;
                    var registeredByEmail = string.Empty;
                    var dateRegistered = string.Empty;

                    if (asset.AssetType == AssetTypeEnum.Device)
                    {
                        registeredByName = asset.RegisteredByName;
                        registeredByEmail = asset.RegisteredByEmail;
                        dateRegistered = (asset.DateRegistered != null) ? asset.DateRegistered.Value.ToString(SiteHelper.DateFormat) : string.Empty;
                    }
                    else if (asset.AssetType == AssetTypeEnum.License)
                    {
                        dateRegistered = (asset.DateRegistered != null) ? asset.DateRegistered.Value.ToString(SiteHelper.DateFormat) : string.Empty;
                    }

                    //Set values for the cells
                    row.CreateCell(0).SetCellValue(asset.AssetType);
                    row.CreateCell(1).SetCellValue(asset.Code);
                    row.CreateCell(2).SetCellValue(asset.Name);
                    row.CreateCell(3).SetCellValue((asset.DateRegistered != null)
                                         ? asset.DateRegistered.Value.ToString(SiteHelper.DateFormat)
                                         : string.Empty);
                    row.CreateCell(4).SetCellValue((asset.DatePurchased != null)
                                         ? asset.DatePurchased.Value.ToString(SiteHelper.DateFormat)
                                         : string.Empty);
                    row.CreateCell(5).SetCellValue(registeredByName);
                    row.CreateCell(6).SetCellValue(registeredByEmail);
                    row.CreateCell(7).SetCellValue(dateRegistered);
                }

                // Create new Excel sheet
                var sheet1 = workbook.CreateSheet("Transfer Licenses to Spare");

                // (Optional) set the width of the columns
                sheet1.SetColumnWidth(0, 40 * 256);
                sheet1.SetColumnWidth(1, 60 * 256);
                sheet1.SetColumnWidth(2, 50 * 256);
                sheet1.SetColumnWidth(3, 40 * 256);
                sheet1.SetColumnWidth(4, 40 * 256);
                sheet1.SetColumnWidth(5, 60 * 256);
                sheet1.SetColumnWidth(6, 60 * 256);
                sheet1.SetColumnWidth(7, 50 * 256);

                // Create header row
                var headerRow1 = sheet1.CreateRow(0);

                //Set column names for the header row
                headerRow1.CreateCell(0).SetCellValue("Transfer Status");
                headerRow1.CreateCell(1).SetCellValue("Transferred By Email");
                headerRow1.CreateCell(2).SetCellValue("Transferred By Name");
                headerRow1.CreateCell(3).SetCellValue("Date Transferred");
                headerRow1.CreateCell(4).SetCellValue("Source Serial Number");
                headerRow1.CreateCell(5).SetCellValue("Destination Serial Number");

                // (Optional) freeze the header row so it is not scrolled
                sheet1.CreateFreezePane(0, 1, 0, 1);

                int rowNumber1 = 1;

                // Create Content
                foreach (var lic in transferredSpareLicenses)
                {
                    //Create a new row
                    var row = sheet1.CreateRow(rowNumber1++);

                    var dateTransferred = string.Empty;

                    dateTransferred = (lic.DateTransferred != null) ? lic.DateTransferred.Value.ToString(SiteHelper.DateFormat) : string.Empty;

                    //Set values for the cells
                    row.CreateCell(0).SetCellValue(lic.TransferStatus);
                    row.CreateCell(1).SetCellValue(lic.TransferredByEmail);
                    row.CreateCell(2).SetCellValue(lic.TransferredByName);
                    row.CreateCell(3).SetCellValue(dateTransferred);
                    row.CreateCell(4).SetCellValue(lic.SourceSerialNumber);
                    row.CreateCell(5).SetCellValue(lic.DestinationSerialNumber);
                }

                Response.AppendCookie(new HttpCookie("fileDownload", "true") { Path = "/" });
                Response.AddHeader("Cache-Control", "no-cache, no-store, must-revalidate");

                // Write the workbook to a memory stream
                var output = new MemoryStream();
                workbook.Write(output);

                var fileName = string.Format("AssetHistory.{0:MM.dd.yy}.xls", DateTime.Now);
                return File(output.ToArray(), "application/vnd.ms-excel", fileName);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return PartialView("../Shared/_Error");
            }
        }
    }
}
