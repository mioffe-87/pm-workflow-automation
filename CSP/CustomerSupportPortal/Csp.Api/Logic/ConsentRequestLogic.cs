using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.DataAccess;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System.Web;
using AutoMapper;

namespace Csp.Api.Logic
{
    public class ConsentRequestLogic : IConsentRequestLogic
    {
        private readonly IUserAccountLogic _userAccountLogic;
        private readonly ISupportAccountLogic _supportAccountLogic;
        private readonly IConsentRequestDataAccess _consentRequestDataAccess;

        public ConsentRequestLogic(IUserAccountLogic UserAccountLogic, ISupportAccountLogic supportAccountLogic, IConsentRequestDataAccess consentRequestDataAccess)
        {
            _userAccountLogic = UserAccountLogic;
            _supportAccountLogic = supportAccountLogic;
            _consentRequestDataAccess = consentRequestDataAccess;
        }

        public async Task<dynamic> ConsentRequestProxy(ConsentRequest consentRequest)
        {
            dynamic configuration = null;
            bool isError = false;
            EventPublisher _eventPublisher = new EventPublisher();

            try
            {

                string jsonResponse = HttpUtility.HtmlDecode(InvokeConsentRequestApi(consentRequest, out isError));

                if (!string.IsNullOrEmpty(jsonResponse) && !isError)
                {
                    if (consentRequest.Action == ConsentAction.Create.ToString().ToUpper())
                    {
                        var result = JsonConvert.DeserializeObject<ConsentRequestResponse>(jsonResponse);
                        configuration = result;

                        #region Publish Event to Pubsub

                        AppNotificationData consentRequestAppData = new AppNotificationData()
                        {
                            AccountId = Convert.ToString(consentRequest.RequestorSupportAccountId),
                            EntityId = Convert.ToString(result.Data.consentRequestId),
                            UserId = Convert.ToString(consentRequest.RequestorUserAccountId)
                        };

                        AppNotification consentRequestNotification = new AppNotification()
                        {
                            AppData = consentRequestAppData,
                            Category = NotificationCategory.ConsentRequest.ToDescription(),
                            DeliveryChannel = new List<string>() { NotificationDeliveryChannel.Web.ToDescription() },
                            Source = NotificationSource.Csp.ToDescription()
                        };

                        await _eventPublisher.PublishEvent(consentRequestNotification);
                        #endregion


                        //Send email to DA
                        EmailHelper.SendConsentRequestEmailToApprover(consentRequest.RequestorSupportAccountId, consentRequest.DestinationUserAccountId, result.Data.consentRequestId, consentRequest.RequestorEmail, consentRequest.RequestorUserAccountId, consentRequest.DestinationSupportAccountId);
                    }
                    else if (consentRequest.Action == ConsentAction.Get.ToString().ToUpper() || consentRequest.Action == ConsentAction.GetLinkedAccounts.ToString().ToUpper())
                    {
                        var consentListResponseModel = JsonConvert.DeserializeObject<ConsentListResponseModel>(jsonResponse);
                        var result = Mapper.Map<ConsentListResponse>(consentListResponseModel);
                        configuration = result;
                        
                            foreach (var request in configuration.ConsentRequestsList.RequestsList)
                            {
                                if (request.SourceUserAccountId > 0)
                                {
                                    UserAccount sourceUserAccount = _userAccountLogic.GetUserAccount(request.SourceUserAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                                    request.SourceUserEmail = sourceUserAccount.Email;
                                    request.SourceUserName = string.Concat(sourceUserAccount.FirstName, " ", sourceUserAccount.LastName);
                                }

                                if (request.UpdatedByUserAccountId > 0)
                                {
                                    UserAccount updatedByUserAccount = _userAccountLogic.GetUserAccount(Convert.ToInt64(request.UpdatedByUserAccountId), ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                                    request.UpdatedByUserEmail = updatedByUserAccount.Email;
                                }

                                if (request.TargetAccountId > 0)
                                {
                                    SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();
                                    SupportAccount targetSupportAccount = supportAccountDataAccess.Read(request.TargetAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                                    request.TargetAccountName = targetSupportAccount.Name;
                                }

                                if (request.TargetUserAccountId > 0)
                                {
                                    UserAccount targetUserAccount = _userAccountLogic.GetUserAccount(request.TargetUserAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                                    request.TargetUserEmail = targetUserAccount.Email;
                                    request.TargetUserName = string.Concat(targetUserAccount.FirstName, " ", targetUserAccount.LastName);
                                }

                                if (request.SourceAccountId > 0)
                                {
                                    SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();
                                    SupportAccount sourceSupportAccount = supportAccountDataAccess.Read(request.SourceAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                                    request.SourceAccountName = sourceSupportAccount.Name;
                                }

                                request.StatusDescription = ((ConsentStatus)request.Status).ToDescription();
                                request.CreatedOn = request.CreatedOn != null ? ((DateTime)request.CreatedOn) : request.CreatedOn;

                                List<ConsentHistory> HistoryList = new List<ConsentHistory>();


                                foreach (var log in request.StatusLogs)
                                {
                                    UserAccount requestorUserAccount = _userAccountLogic.GetUserAccount(log.UpdatedByUserAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);

                                    HistoryList.Add(new ConsentHistory()
                                    {
                                        Action = ((ConsentStatus)log.ConsentStatus).ToDescription(),
                                        RequestorEmail = requestorUserAccount.Email,
                                        RequestorName = string.Concat(requestorUserAccount.FirstName, " ", requestorUserAccount.LastName),
                                        Roles = string.Empty,
                                        LogDate = log.UpdatedDate
                                    });
                                }

                                foreach (var log in request.RolesLogs)
                                {
                                    UserAccount requestorUserAccount = _userAccountLogic.GetUserAccount(log.UpdatedByUserAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);

                                    string[] roleslist = new string[log.Roles.Count];

                                    for (int i = 0; i < log.Roles.Count; i++)
                                    {
                                        roleslist[i] = ((RoleType)log.Roles[i]).ToDescription();
                                    }

                                    HistoryList.Add(new ConsentHistory()
                                    {
                                        Action = ConsentStatus.RolesUpdated.ToDescription(),
                                        RequestorEmail = requestorUserAccount.Email,
                                        RequestorName = string.Concat(requestorUserAccount.FirstName, " ", requestorUserAccount.LastName),
                                        Roles = roleslist.Length > 0 ? string.Join(",", roleslist) : string.Empty,
                                        LogDate = log.UpdatedDate
                                    });
                                }

                                request.HistoryList = HistoryList;
                            
                        }
                    }
                    else if (consentRequest.Action == ConsentAction.MyApprovals.ToString().ToUpper() || consentRequest.Action == ConsentAction.MyRequests.ToString().ToUpper())
                    {
                        var consentListResponseModel = JsonConvert.DeserializeObject<ConsentListResponseModel>(jsonResponse);
                        var result = Mapper.Map<ConsentListResponse>(consentListResponseModel);
                        configuration = result;

                        foreach (var request in configuration.ConsentRequestsList.RequestsDetailsList)
                        {
                            if (request.SourceUserAccountId > 0)
                            {
                                UserAccount sourceUserAccount = _userAccountLogic.GetUserAccount(request.SourceUserAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                                request.SourceUserEmail = sourceUserAccount.Email;
                                request.SourceUserName = string.Concat(sourceUserAccount.FirstName, " ", sourceUserAccount.LastName);
                            }

                            if (request.UpdatedByUserAccountId > 0)
                            {
                                UserAccount updatedByUserAccount = _userAccountLogic.GetUserAccount(Convert.ToInt64(request.UpdatedByUserAccountId), ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                                request.UpdatedByUserEmail = updatedByUserAccount.Email;
                            }

                            if (request.TargetAccountId > 0)
                            {
                                SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();
                                SupportAccount targetSupportAccount = supportAccountDataAccess.Read(request.TargetAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                                request.TargetAccountName = targetSupportAccount.Name;
                            }

                            if (request.TargetUserAccountId > 0)
                            {
                                UserAccount targetUserAccount = _userAccountLogic.GetUserAccount(request.TargetUserAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                                request.TargetUserEmail = targetUserAccount.Email;
                                request.TargetUserName = string.Concat(targetUserAccount.FirstName, " ", targetUserAccount.LastName);
                            }

                            if (request.SourceAccountId > 0)
                            {
                                SupportAccountDataAccess supportAccountDataAccess = new SupportAccountDataAccess();
                                SupportAccount sourceSupportAccount = supportAccountDataAccess.Read(request.SourceAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                                request.SourceAccountName = sourceSupportAccount.Name;
                            }

                            request.StatusDescription = ((ConsentStatus)request.Status).ToDescription();
                            request.CreatedOn = request.CreatedOn != null ? ((DateTime)request.CreatedOn) : request.CreatedOn;
                        }
                    }
                    else if (consentRequest.Action.ToUpper() == ConsentAction.Update.ToString().ToUpper() || consentRequest.Action.ToUpper() == ConsentAction.UpdateRoles.ToString().ToUpper())
                    {
                        UserAccount updatedUser = _userAccountLogic.GetUserAccount(consentRequest.UpdatedUserAccountId, ApiConstants.PANW_SYSTEM_USER_ACCOUNT_ID);
                        consentRequest.UpdatedUserEmail = updatedUser.Email;

                        EmailHelper.SendConsentRequestEmailToRequestor(consentRequest.UpdatedSupportAccountId, consentRequest.RequestorEmail, consentRequest.UpdatedUserEmail, ((ConsentStatus)consentRequest.ConsentStatus).ToDescription(), consentRequest.UpdatedUserAccountId);
                        configuration = JsonConvert.DeserializeObject<ConsentRequestResponse>(jsonResponse);
                       
                        //log consent request details in DB
                        _consentRequestDataAccess.AddUpdateConsentRequest(consentRequest.ConsentRequestId,consentRequest.RequestorSupportAccountId,consentRequest.UpdatedSupportAccountId, consentRequest.ConsentStatus == (int)ConsentStatus.Approved );

                        #region Publish Event to Pubsub
                        if (consentRequest.ConsentStatus == (int)ConsentStatus.Approved || consentRequest.ConsentStatus == (int)ConsentStatus.Revoked)
                        {
                            AppNotificationData consentRequestAppData = new AppNotificationData()
                            {
                                AccountId = Convert.ToString(consentRequest.RequestorSupportAccountId),
                                TargetAccountId = Convert.ToString(consentRequest.DestinationSupportAccountId),
                                Status = consentRequest.ConsentStatus
                            };

                            AppNotification consentRequestNotification = new AppNotification()
                            {
                                AppData = consentRequestAppData,
                                Category = NotificationCategory.ConsentRequest.ToDescription(),
                                DeliveryChannel = new List<string>() { NotificationDeliveryChannel.Web.ToDescription() },
                                Source = NotificationSource.Csp.ToDescription()
                            };

                            await _eventPublisher.PublishEvent(consentRequestNotification);
                        }
                        #endregion
                    }
                    else if (consentRequest.Action.ToUpper() == ConsentAction.SupportAccounts.ToString().ToUpper())
                    {
                        configuration = JsonConvert.DeserializeObject<List<NameValueOnly>>(jsonResponse);
                    }

                }
                else
                {
                    configuration = JsonConvert.DeserializeObject<ErrorResponse>(jsonResponse);                 
                 
                    if (jsonResponse == "500")
                    {
                        configuration = new ErrorResponse()
                        {
                            ErrorMessage = " Please contact Support",
                            ErrorHeader = "Unexpected Error Occured",
                            ErrorCode = "500"
                        };
                    }
                }
            }
                catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            return configuration;
        }

        public async Task<List<NameValueOnly>> GetEligibleAccounts(ConsentRequest consentRequest)
        {
            dynamic accounts = new List<NameValueOnly>();
            accounts = await ConsentRequestProxy(consentRequest);
            return accounts;
        }

        private string InvokeConsentRequestApi(ConsentRequest consentRequest, out bool isError)
        {
            string result = string.Empty;
            bool _isError = false;

            try
            {
                ConsentRequestCreate consentRequestCreate = null;
                ConsentRequestGetDetails consentRequestGetDetails = null;
                ConsentRequestGetLinkedAccounts ConsentRequestGetLinkedAccounts = null;
                ConsentRequestUpdate consentRequestUpdate = null;
                var consentRequestUrl = ConfigurationManager.AppSettings["ConsentRequestUrl"];
                var consentCertSerialNumber = ConfigurationManager.AppSettings["ConsentRequestCertThumbprint"];

                string env = ConfigurationManager.AppSettings["AppName"];
                var certPrivateKey = env == "CSP DEV" ? string.Empty : ConfigurationManager.AppSettings["CertPrivateKey"];
                consentRequest.Action = consentRequest.Action.ToUpper();

                #region Url & Client Certifacte
                if (consentRequest.Action == ConsentAction.Create.ToString().ToUpper())               {
                    consentRequestCreate = Mapper.Map<ConsentRequestCreate>(consentRequest);
                    consentRequestUrl = string.Format("{0}/{1}", consentRequestUrl, ConsentAction.Create.ToDescription());
                }
                else if (consentRequest.Action == ConsentAction.Get.ToString().ToUpper())
                {
                    consentRequestGetDetails = Mapper.Map<ConsentRequestGetDetails>(consentRequest);
                    consentRequestGetDetails.ConsentList = new List<string>() { consentRequest.ConsentRequestId };
                    consentRequestUrl = string.Format("{0}/{1}", consentRequestUrl, ConsentAction.Get.ToDescription());
                }
                else if (consentRequest.Action == ConsentAction.GetLinkedAccounts.ToString().ToUpper())
                {
                    ConsentRequestGetLinkedAccounts = Mapper.Map<ConsentRequestGetLinkedAccounts>(consentRequest);
                    ConsentRequestGetLinkedAccounts.SupportAccountIdList = new List<long>(){consentRequest.RequestorSupportAccountId};
                    consentRequestUrl = string.Format("{0}/{1}", consentRequestUrl, ConsentAction.GetLinkedAccounts.ToDescription());
                }
                else if (consentRequest.Action == ConsentAction.MyApprovals.ToString().ToUpper())
                {
                    consentRequestUrl = string.Format("{0}/{1}?support_account_id={2}&user_account_id={3}", consentRequestUrl, ConsentAction.MyApprovals.ToDescription(), consentRequest.RequestorSupportAccountId, consentRequest.RequestorUserAccountId);
                }
                else if (consentRequest.Action == ConsentAction.MyRequests.ToString().ToUpper())
                {
                    consentRequestUrl = string.Format("{0}/{1}?support_account_id={2}&user_account_id={3}", consentRequestUrl, ConsentAction.MyRequests.ToDescription(), consentRequest.RequestorSupportAccountId, consentRequest.RequestorUserAccountId);
                }
                else if (consentRequest.Action == ConsentAction.Update.ToString().ToUpper())
                {
                    consentRequestUpdate = Mapper.Map<ConsentRequestUpdate>(consentRequest);
                    consentRequestUrl = string.Format("{0}/{1}", consentRequestUrl, ConsentAction.Update.ToDescription());
                }
                else if (consentRequest.Action == ConsentAction.UpdateRoles.ToString().ToUpper())
                {
                    consentRequestUpdate = Mapper.Map<ConsentRequestUpdate>(consentRequest);
                    consentRequestUrl = string.Format("{0}/{1}", consentRequestUrl, ConsentAction.UpdateRoles.ToDescription());
                }
                else if (consentRequest.Action == ConsentAction.SupportAccounts.ToString().ToUpper())
                {
                    consentRequestUrl = string.Format("{0}/{1}?user_account_id={2}", consentRequestUrl, ConsentAction.SupportAccounts.ToDescription(),consentRequest.RequestorUserAccountId);
                }   

                #endregion

                ServicePointManager.Expect100Continue = true;
                if (env != "CSP PROD")
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                }
              
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

                var request = (HttpWebRequest)WebRequest.Create(consentRequestUrl);
                request.ContentType = "application/json; charset=utf-8";
                request.ProtocolVersion = HttpVersion.Version11;

                #region Basic Auth

                if (consentRequest.Action != ConsentAction.GetLinkedAccounts.ToString().ToUpper())
                {
                    CredentialCache credentialCache = new CredentialCache();
                    credentialCache.Add(new System.Uri(consentRequestUrl), "Basic", new NetworkCredential(ConfigurationManager.AppSettings["consentRequest_username"], ConfigurationManager.AppSettings["consentRequest_password"]));
                    request.Credentials = credentialCache;
                }
                else
                {
                    CredentialCache credentialCache = new CredentialCache();
                    credentialCache.Add(new System.Uri(consentRequestUrl), "Basic", new NetworkCredential(ConfigurationManager.AppSettings["consentRequestLink_username"], ConfigurationManager.AppSettings["consentRequest_password"]));
                    request.Credentials = credentialCache;
                }
                #endregion

                #region Request Method
                if (consentRequest.Action == ConsentAction.Create.ToString().ToUpper()) 
                {
                    request.Method = "POST";

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        string jsondata = JsonConvert.SerializeObject(consentRequestCreate);
                        streamWriter.Write(jsondata);
                    }

                }
                else if (consentRequest.Action == ConsentAction.GetLinkedAccounts.ToString().ToUpper())
                {
                    request.Method = "POST";

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        string jsondata = JsonConvert.SerializeObject(ConsentRequestGetLinkedAccounts);
                        streamWriter.Write(jsondata);
                    }

                }
                else if (consentRequest.Action == ConsentAction.Get.ToString().ToUpper())
                {
                    request.Method = "POST";

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        string jsondata = JsonConvert.SerializeObject(consentRequestGetDetails);
                        streamWriter.Write(jsondata);
                    }

                }
                else if (consentRequest.Action == ConsentAction.MyApprovals.ToString().ToUpper() || consentRequest.Action == ConsentAction.MyRequests.ToString().ToUpper() || consentRequest.Action == ConsentAction.SupportAccounts.ToString())
                {
                    request.Method = "GET";
                }
                else if (consentRequest.Action == ConsentAction.Update.ToString().ToUpper() || consentRequest.Action == ConsentAction.UpdateRoles.ToString().ToUpper())
                {
                    request.Method = "PATCH";

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        string jsondata = JsonConvert.SerializeObject(consentRequestUpdate, Formatting.Indented, new JsonSerializerSettings
                        { NullValueHandling = NullValueHandling.Ignore });
                        streamWriter.Write(jsondata);
                    }
                }
                #endregion

                X509Certificate2 clientCertificate = CertificateManager.GetCertificateByCnName(consentCertSerialNumber);

                if (clientCertificate == null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Empty certificate store"));
                }

                request.ClientCertificates.Add(clientCertificate);

                using (var response = request.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;
                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new StreamReader(response.GetResponseStream(), Encoding.Default))
                        {
                            string jsonResponse = HttpUtility.HtmlEncode(reader.ReadToEnd());

                            if (!string.IsNullOrEmpty(jsonResponse))
                            {
                                result = jsonResponse;
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                using (var response = ex.Response)
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)response).StatusCode;
                    if (statusCode != HttpStatusCode.InternalServerError)
                    {
                        _isError = true;                      

                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            string jsonResponse = HttpUtility.HtmlEncode(reader.ReadToEnd());

                            if (!string.IsNullOrEmpty(jsonResponse))
                            {
                                result = jsonResponse;
                            }
                        }
                    }

                    else
                    {
                        _isError = true;
                        result = statusCode.ToString();

                    }

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            isError = _isError;
            return result;
        }
    }
}
