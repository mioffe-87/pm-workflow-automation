using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using LINQtoCSV;

namespace Csp.Api.Logic
{
    public class CsspLogic : ICsspLogic
    {
        private ICsspRepository _csspRepository;
        private IVmRegistrationRepository _vmRegistrationRepository;
        private IVirtualMachineLogic _vmVirtualMachineLogic;
        private readonly ILicenseLogic _licenseLogic;
        private readonly IUserAccountLogic _userAccountLogic;
        private readonly ICsspDataAccess _csspDataAccess;

        public CsspLogic(ICsspRepository csspRepository, IVmRegistrationRepository vmRegistrationRepository, IVirtualMachineLogic vmVirtualMachineLogic, ILicenseLogic licenseLogic, IUserAccountLogic userAccountLogic,ICsspDataAccess csspDataAccess)
        {
            this._csspRepository = csspRepository;
            this._vmRegistrationRepository = vmRegistrationRepository;
            this._vmVirtualMachineLogic = vmVirtualMachineLogic;
            this._licenseLogic = licenseLogic;
            this._userAccountLogic = userAccountLogic;
            this._csspDataAccess = csspDataAccess;
        }

        public CsspInfo GetCsspInfo(long supportAccountId)
        {
            return _csspRepository.GetCsspInfo(supportAccountId);
        }

        public IEnumerable<CsspSku> SearchCsspSkus(DataParameters parameters, long? supportAccountId, int? model, int? term, int? supportType, int? bundle, string filterProducts = "")
        {
            return _csspRepository.SearchCsspSkus(parameters,supportAccountId, model, term, supportType,bundle,filterProducts);
        }


        public IList<CsspModel> GetCsspModels(bool addCaptionItem = true)
        {
            var models = _csspRepository.GetCsspModels().ToList();
            if (addCaptionItem)
            {
                var modelCaption = new CsspModel()
                {
                    CsspModelId = 0,
                    Model = "-Select Model-"
                };
                models.Insert(0, modelCaption);
            }
            return models;
        }

        public IList<CsspSupportType> GetCsspSupportTypes(bool addCaptionItem = true)
        {
            var supportTypes = _csspRepository.GetCsspSupportTypes().ToList();
            if (addCaptionItem)
            {
                var supportTypeCaption = new CsspSupportType()
                {
                    CsspSupportTypeId = 0,
                    SupportType = "-Select SupportType-"
                };
                supportTypes.Insert(0, supportTypeCaption);
            }
            return supportTypes;
        }

        public IList<CsspTerm> GetCsspTerms(bool addCaptionItem = true)
        {
            var terms = _csspRepository.GetCsspTerms().ToList();
            if (addCaptionItem)
            {
                var termCaption = new CsspTerm()
                {
                    CsspTermId = 0,
                    Term  = "-Select Term-"
                };
                terms.Insert(0, termCaption);
            }
            return terms;
        }


        public CsspSku GetSkuDetails(int skuId)
        {
            return _csspRepository.GetSkuDetails(skuId);
        }

        public IEnumerable<CsspSku> GetSkuDetails(List<int> SkuIds)
        {
            return _csspRepository.GetSkuDetails(SkuIds);
        }


        public bool SubmitCsspOrder(List<int> skuIds, long supportAccountId, long userAccountId, out int? cartId, out string cartNumber)
        {
            return _csspRepository.SubmitCsspOrder(skuIds, supportAccountId, userAccountId, out cartId, out cartNumber);
        }


        public CsspOrder GetCsspOrderDetails(int cartId)
        {
            return _csspRepository.GetCsspOrderDetails(cartId);
        }


        public bool PushOrderToSap(CsspOrderSapInput sapInput, out string sapReturnMessage, out string inputXML)
        {
            inputXML = string.Empty;
            string errorMessage = string.Empty;
            bool isSuccess = CsspOrderProcessor.PushToSAP(sapInput, out inputXML, out sapReturnMessage, out errorMessage);
            try
            {
                if (!isSuccess && string.IsNullOrEmpty(sapReturnMessage))
                {
                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        sapReturnMessage = "SAP Returned empty response!";
                    }
                    else
                    {
                        sapReturnMessage = string.Format("{0}{1}","Application Error: ",errorMessage);
                    }
                }
                _csspRepository.LogOrderXML(sapInput.CartId, inputXML,
                    sapReturnMessage.Length > 250 ? sapReturnMessage.Substring(0, 250) : sapReturnMessage);
                if (isSuccess)
                {
                    string sapOrderNumber =
                    sapReturnMessage.Replace("S - Order ", "").Replace(" Created Successfully", "").Trim();
                    return _csspRepository.SetSapOrderNumberForOrder(sapInput.CartId, sapOrderNumber);
                }
            }
            catch (Exception oex)
            {
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = oex.ToString();
                }
                if (string.IsNullOrEmpty(sapReturnMessage))
                {
                    sapReturnMessage = errorMessage;
                }
                return false;
            }
            return false;
        }

        public CsspOrderSapInput GetSapInput(long supportAccountId, int cartId)
        {
            return _csspRepository.GetSapInput(supportAccountId, cartId);
        }

		public IEnumerable<CsspOrderDetails> GetOrders(long supportAccountId)
        {
            return _csspRepository.GetOrders(supportAccountId);
        }


        public IEnumerable<AvailableVm> GetAvailableVms(DataParameters parameters, long supportAccountId, long userAccountId, string authCode)
        {
            return _vmRegistrationRepository.GetAvailableVms(parameters, supportAccountId, userAccountId, authCode, true);
        }


        public IEnumerable<CsspOrderHistory> GetOrderHistory(long supportAccountId, string orderNumber = "")
        {
            return _csspRepository.GetOrderHistory(supportAccountId, orderNumber);
        }

        public void SendSapOrderNotification(CsspOrderSapInput sapInput, bool isOrderCreated, string sapMessage, string inputXML)
        {
            var configValues = new ConfigValues();
            var isQa = ConfigurationManager.AppSettings["AppName"] != "CSP PROD";
            string mHeader = "";
            mHeader = "<div style=\"width:98%;border:2px solid black;padding: 10px\">";
            mHeader = mHeader + "<div style=\"height:40px;background:#E3E1E1;text-align:center;\"><span style=\"font-size:30px;color:rgb(50, 106, 138);margin-left:20px;font-family:Georgia, sans-serif;\">CSSP Order</span></div>";
            if (isOrderCreated)
            {
                if (isQa)
                {
                    mHeader = mHeader + "<h3 style=\"text-align:center\">CSSP Order created successfully in QA!</h3>";
                }
                else
                {
                    mHeader = mHeader + "<h3 style=\"text-align:center\">CSSP Order created successfully!</h3>";
                }
            }
            else
            {
                if (isQa)
                {
                    mHeader = mHeader + "<h3 style=\"text-align:center\">Failed to create CSSP Order in QA!</h3>";
                }
                else
                {
                    mHeader = mHeader + "<h3 style=\"text-align:center\">Failed to create CSSP Order!</h3>";
                }
            }

            var message = new[] {
					string.Format("Attention Fulfillment:"),
					string.Empty,
					string.Format(
						"{1} (Support Account - {0}) has requested a CSSP Order.  Please review and fulfill this request.", sapInput.SupportAccountId, sapInput.OrderedByUser),
                    string.Empty,
                    "PO# : " + sapInput.CartNumber,
                    string.Empty,
                    "Message from SAP : " + sapMessage
				};
            var msgBody = new MessageBody();
            msgBody.AddTextBlock(mHeader);
            msgBody.AddTextBlock(message);
            msgBody.AddItems(sapInput.OrderItems);
            msgBody.AddTextBlock("</div>");
            var bodyText = msgBody.GetBodyText();
            var msg = new MailMessage
            {
                From = new MailAddress(configValues.GetAppSetting(AppSettings.FromAddressForMails)),
                Subject = isOrderCreated == true ? "CSSP Order Request - Order Created" : "CSSP Order Request - Errored.",
                Body = bodyText,
                IsBodyHtml = true,
            };
            var fulfillmentEmails = configValues.GetAppSetting(AppSettings.FulfillmentEmails).Split(';');
            foreach (var fEmail in fulfillmentEmails)
            {
                msg.To.Add(fEmail);
            }
            SendMessageAsync(msg);
        }

        private void SendMessageAsync(MailMessage msg)
        {
            ThreadPool.QueueUserWorkItem(x => SendMessage(msg));
        }

        private void SendMessage(MailMessage msg)
        {
            try
            {
                using (var smtp = new SmtpClient())
                {
                    smtp.Send(msg);
                }
            }
            catch(Exception oex)
            {
                string errMessage = oex.ToString();
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errMessage));
            }
        }

        private class MessageBody
        {
            private readonly StringBuilder _sb = new StringBuilder();

            public string GetBodyText()
            {
                return _sb.ToString();
            }

            public void AddHeader(string text)
            {
                var line = string.Format("<br /><p style=\"font-weight: bold; font-size: 150%;\">{0}</p><hr />", text);

                _sb.Append(line);
            }

            public void AddTextBlock(IEnumerable<string> lines)
            {
                foreach (var line in lines)
                {
                    AddTextBlock(line);
                }
            }

            public void AddTextBlock(string text)
            {
                var line = string.Format("<p>{0}</p>", text);

                _sb.Append(line);
            }

            public void AddItems(List<string> items)
            {
                AddHeader("Sku Requested");
                StringBuilder oHtmlTable = new StringBuilder();
                oHtmlTable.Append("<table width=\"96%\" cellpadding=\"5\" style=\"border:2px solid #e3e3e3;\"><tr><td>#</td><td><b>Sku</b></td></tr>");
                var iPart = 0;
                foreach (var part in items)
                {
                    iPart = iPart + 1;
                    oHtmlTable.Append("<tr><td>" + iPart.ToString() + "</td><td>" + part + "</td></tr>");
                }
                oHtmlTable.Append("</table>");
                AddTextBlock("<div>&nbsp;</div>");
                AddTextBlock(oHtmlTable.ToString());
            }
        }



        public IEnumerable<CsspOrderHistoryItems> GetOrderHistoryItems(int csspOrderId)
        {
            return _csspRepository.GetOrderHistoryItems(csspOrderId);
        }


        public void CreateSapOrderAsync(long supportAccountId, int shoppingCartId, string cartNumber, string orderedBy)
        {
            var sapInput = GetSapInput(supportAccountId, shoppingCartId);
            sapInput.CartNumber = cartNumber;
            sapInput.OrderedByUser = orderedBy;
            ThreadPool.QueueUserWorkItem(x => CreateSapOrder(sapInput));
        }

        private void CreateSapOrder(CsspOrderSapInput sapInput)
        {
            string sapResponse;
            string inputXML;
            bool isSuccess = PushOrderToSap(sapInput, out sapResponse, out inputXML);
            SendSapOrderNotification(sapInput, isSuccess, sapResponse, inputXML);
        }

        public RegisterVmResult RegisterVm(string uuId, string cpuId, string authCode, long supportAccountId, long userAccountId)
        {
            return _vmVirtualMachineLogic.RegisterVm(EVmType.Vm, uuId, cpuId, authCode, supportAccountId, userAccountId, string.Empty, string.Empty, true);
        }

        public ParseVmFileResult ParseVmFile(System.Web.HttpPostedFileBase file)
        {
            return _vmVirtualMachineLogic.ParseVmFile(EVmType.Vm, file);
        }

        public IEnumerable<CsspProvisionedDevices> GetCsspProvisionedDevices(DataParameters parameters, long supportAccountId, string serialNumber = "", string authorizationCode = "", string endUserAccountId = "", string endUserCompany = "", string contactEmail = "")
        {
            return _csspRepository.GetCsspProvisionedDevices(supportAccountId, serialNumber,
                authorizationCode, endUserAccountId, endUserCompany, contactEmail, parameters);
        }

        public CsspProvisionedDevicesMonthlyData<IEnumerable<CsspProvisionedDevicesMonthly>> GetCsspProvisionedDevicesMonthly(DataParameters parameters, long supportAccountId, int year, int month)
        {
            return _csspRepository.GetCsspProvisionedDevicesMonthly(supportAccountId, year, month);
        }

        public bool AddEndUserInformationForCsspCustomer(CsspEndUser endUser, out string errorMessage)
        {
            errorMessage = "";
            var result = ReportCsspEndUserInfo(endUser);
            if (!result)
            {
                errorMessage = "Error occured while updating End User information.";
            }
            return result;
        }


        public bool ReportCsspEndUserInfo(CsspEndUser endUser)
        {
            long[] deviceIds = string.IsNullOrEmpty(endUser.ProvisionedDeviceIds) ? new[] { endUser.ProvisionedDeviceId.Value } : endUser.ProvisionedDeviceIds.Split(',').Select(Int64.Parse).ToArray();
            return _licenseLogic.ReportCsspEndUserInfo(deviceIds, endUser.CSSPSupportAccountId,
                endUser.CustomerAccountId, endUser.CompanyName, endUser.DnBNumber, endUser.Address, endUser.Country,
                endUser.Region, endUser.City, endUser.PostalCode, endUser.EndUserContactEmail,endUser.PhoneNumber,endUser.CompanyWebsite,endUser.Industry, endUser.CreatedBy);
        }


        public byte[] GetCsspProvisionedDevicesCsv(long supportAccountId, string serialNumber = "", string authorizationCode = "", 
            string endUserAccountId = "", string endUserCompany = "", string contactEmail = "")
        {
            var data = _csspRepository.GetCsspProvisionedDevices(supportAccountId, serialNumber, authorizationCode, endUserAccountId, endUserCompany, contactEmail);
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

                var csspDevicesCsv = Mapper.Map<CsspProvisionedDevicesCsv[]>(data);

                cc.Write(csspDevicesCsv, tw, outputFileDescription);

                tw.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }

        public CsspProvisionedDevicesMonthlyData<byte[]> GetCsspProvisionedDevicesMonthlyCsv(long supportAccountId, int year, int month)
        {
            CsspProvisionedDevicesMonthlyData<IEnumerable<CsspProvisionedDevicesMonthly>> data = _csspRepository.GetCsspProvisionedDevicesMonthly(supportAccountId, year, month);
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
                cc.Write(data.Items, tw, outputFileDescription);
                tw.Flush();
                ms.Position = 0;
                return new CsspProvisionedDevicesMonthlyData<byte[]>
                {
                    SupportAccountName = data.SupportAccountName,
                    SupportAccountId = data.SupportAccountId,
                    Items = ms.ToArray(),
                    Month = data.Month,
                    Year = data.Year
                };
            }
        }


        public bool IsUserCssp(long userAccountId, long supportAccountId)
        {
            return _csspRepository.IsUserCssp(userAccountId, supportAccountId);
        }


        public IEnumerable<CsspEndUser> SearchEndUser(long csspSupportAccountId, string searchField, string searchValue)
        {
            return _csspRepository.SearchEndUser(csspSupportAccountId, searchField, searchValue);
        }


        //This is called when there were multiple matches and using adminsite tool or someother mechanism we select the sfdc account we want to tie the end user to
        // 1, If the end user is already tied to csp account, then the contacts created needs to be updated with new sfdcaccountid
        // 2, Update cssppanaccounts table with selected sfdcaccount and csp account for the enduser.
				// Comments By LK On 02/07/2020
				// This method is obsolete and is no longer being used
				/*
        public bool UpdateCsspEndUserDefaultAccountsToExistingPanAccounts(string referenceId, long csspSupportAccountId, string currentSfdcAccountId, long? currentSupportAccountId, string newSfdcAccountId, long? newSupportAccountId, out string errorMessage)
        {
            try
            {
                using (var transactionScope = new TransactionScope())
                {
                    bool isSuccess = _csspRepository.UpdatePanAccountForEndUser(csspSupportAccountId, referenceId,
                        currentSupportAccountId, currentSfdcAccountId, newSfdcAccountId, newSupportAccountId, out errorMessage);
                    if (isSuccess)
                    {
                        if (currentSupportAccountId.HasValue)
                        {
                            var useraccounts = _userAccountLogic.GetSupportAccountUsers(currentSupportAccountId.Value);
                            foreach (var useraccount in useraccounts)
                            {
                                _userAccountLogic.UpdateSalesforceContactsAccount(useraccount, currentSupportAccountId.Value, currentSfdcAccountId,
                                    newSfdcAccountId);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception oex)
            {
                errorMessage = oex.Message;
                return false;
            }
        }*/


        public bool StoreMultipleSfdcAccountMatchesForEndUser(string referenceId, long csspSupportAccountId, string[] sfdcAccountIds, string preferredSfdcAccountId)
        {
            return _csspRepository.StoreMultipleSfdcAccountMatchesForEndUser(referenceId, csspSupportAccountId,
                sfdcAccountIds, preferredSfdcAccountId);
        }


        public bool StoreSfdcAccountForEndUser(string referenceId, long csspSupportAccountId, string sfdcAccountId)
        {
            return _csspRepository.StoreSfdcAccountForEndUser(referenceId, csspSupportAccountId, sfdcAccountId);
        }


        public bool LogCsspPanAccountCreationDetails(long enduserId, string panAccountType, string logType, string notes, string createdBy)
        {
            return _csspRepository.LogCsspPanAccountCreationDetails(enduserId, panAccountType, logType, notes, createdBy);
        }

        public bool CanGeneratePrelicenseKeys(long supportAccountId)
        {
            return _csspRepository.CanGeneratePrelicenseKeys(supportAccountId);
        }

        public int GetVmUseCountByAuthCode(string authCode)
        {
            return _vmRegistrationRepository.GetVmUseCountByAuthCode(EVmType.Vm, authCode);
        }

        public PrelicenseResults ProcessPrelicenseKeys(long supportAccountId, string authorizationCode, int quantity, string createdBy)
        {
            string batchId;
            bool isSuccess = false;
            var requestId = _csspRepository.CreatePrelicenseRequest(supportAccountId, authorizationCode, quantity, createdBy, out batchId);

            var syncProcessingCount = Convert.ToInt32(ConfigurationManager.AppSettings["ProcessCsspPreLicenseKeySynchronouslyWhenCountIsLessThan"]);

            bool processRequestSynchronously = quantity <= syncProcessingCount;
            if (processRequestSynchronously)
            {
                isSuccess = _licenseLogic.ProcessPreLicenseRequestSynch(requestId, quantity, createdBy);
            }
            else
            {
                isSuccess = _licenseLogic.ProcessPreLicenseRequestASynch(requestId, quantity, createdBy);
            }
            var result = new PrelicenseResults()
            {
                HasError = !isSuccess,
                IsAsync = processRequestSynchronously == false,
                PrelicenseRequestId = requestId,
                BatchId = batchId
            };
            return result;
        }


        public IEnumerable<CsspPreLicenseDevice> GetCsspPrelicenseDevices(DataParameters parameters, long supportAccountId, string authorizationCode, string csspBatchId = "", string serialNumber = "")
        {
            return _csspRepository.GetCsspPrelicenseDevices(parameters, supportAccountId, authorizationCode, csspBatchId, serialNumber);
        }


        public CsspDownloadLicense DownloadLicense(List<long> aDeviceId)
        {
            return _csspRepository.DownloadLicense(aDeviceId);
        }


        public int GetPreLicenseRequestStatusId(int requestId)
        {
            return _csspRepository.GetPreLicenseRequestStatusId(requestId);
        }


        public List<long> GetDeviceIdsToDownloadLicenseKeys(long supportAccountId, string searchField, string searchValue)
        {
            if (searchField == "AuthorizationCodes")
            {
                return _csspRepository.GetDeviceIdsToDownloadLicenseKeysByAuthorizationCode(supportAccountId,
                    searchValue);
            }
            else
            {
                return _csspRepository.GetDeviceIdsToDownloadLicenseKeysByPrelicenseBatchId(supportAccountId,
                    searchValue);
            }
        }
    }
}
