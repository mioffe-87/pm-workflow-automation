using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Salesforce.Common;
using Salesforce.Force;
using System.Dynamic;
using Salesforce.Common.Models;
using Csp.Api.Exceptions;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Csp.Api.HelperClasses;
using System.Web;

namespace Csp.Api.Logic.Salesforce
{
    public class SalesforceRestApiLogic : ISalesforceRestApiLogic
    {
        private ISalesforceLogsRepository _salesforceLogsRepository;
        private readonly IApplicationSettings _applicationSettings;
        private readonly IConnectionStrings _connectionStrings;

        static SalesforceRestApiLogic()
        {
            // Salesforce requires TLS 1.1 or 1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        public SalesforceRestApiLogic(ISalesforceLogsRepository salesforceLogsRepository, IApplicationSettings applicationSettings, IConnectionStrings connectionStrings)
        {
            _salesforceLogsRepository = salesforceLogsRepository;
            _applicationSettings = applicationSettings;
            _connectionStrings = connectionStrings;
        }

        public async Task<AscContactInfo> GetAscContactInfo(string accountId)
        {
            var query =
                string.Format(
                    @"SELECT Id, Name, ASC_Contact__c, ASC_Contact__r.Id, ASC_Contact__r.Name, ASC_Contact__r.Email, ASC_Support_Email__c, ASC_Support_Number__c, ASC_Support_Site__c FROM account WHERE Id = '{0}' LIMIT 1",
                    accountId);


            var clientInfo = GetSalesforceClient();
            var auth = new AuthenticationClient();

            try
            {
                await auth.UsernamePasswordAsync(clientInfo.ConsumerKey, clientInfo.ConsumerSecret, clientInfo.Username, clientInfo.Password, clientInfo.TokenEndPointUrl);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetAscContactInfo - UsernamePasswordAsync: Username: {0}, Endpoint Url: {1}", clientInfo.Username ?? string.Empty, clientInfo.TokenEndPointUrl ?? string.Empty), ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new CspNotificationException(ex.Message);
            }

            var forceClient = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);
            var accounts = new QueryResult<dynamic>();

            try
            {
                accounts = await forceClient.QueryAsync<dynamic>(query);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetAscContactInfo - QueryAsync: Account Id: {0}", accountId), ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new CspNotificationException(ex.Message);
            }

            if (accounts.Records.Count > 0)
            {
                var account = accounts.Records[0];

                return new AscContactInfo()
                {
                    Id = account.Id,
                    Name = account.Name,
                    ContactId = account.ASC_Contact__c,
                    Contact = account.ASC_Contact__r != null ? new NameValueDescription() { Name = account.ASC_Contact__r.Name, Value = account.ASC_Contact__r.Id, Description = account.ASC_Contact__r.Email } : new NameValueDescription() { Name = string.Empty },
                    Email = account.ASC_Support_Email__c,
                    Phone = account.ASC_Support_Number__c,
                    WebSite = account.ASC_Support_Site__c
                };
            }

            return new AscContactInfo()
            {
                Id = accountId
            };
        }

        public async Task<string> UpdateAscContactInfo(AscContactInfo ascContactInfo)
        {
            var accountId = ascContactInfo.Id;
            dynamic contactInfo = new ExpandoObject();

            contactInfo.ASC_Contact__c = ascContactInfo.ContactId;
            contactInfo.ASC_Support_Email__c = ascContactInfo.Email;
            contactInfo.ASC_Support_Number__c = ascContactInfo.Phone;
            contactInfo.ASC_Support_Site__c = ascContactInfo.WebSite;

            var clientInfo = GetSalesforceClient();
            var auth = new AuthenticationClient();

            try
            {
                await auth.UsernamePasswordAsync(clientInfo.ConsumerKey, clientInfo.ConsumerSecret, clientInfo.Username, clientInfo.Password, clientInfo.TokenEndPointUrl);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: UpdateAscContactInfo - UsernamePasswordAsync: Username: {0}, Endpoint Url: {1}", clientInfo.Username ?? string.Empty, clientInfo.TokenEndPointUrl ?? string.Empty), ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new CspNotificationException(ex.Message);
            }

            var forceClient = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);

            if (!string.IsNullOrEmpty(accountId))
            {
                try
                {
                    await forceClient.UpdateAsync("Account", accountId, contactInfo);
                }
                catch (Exception ex)
                {
                    _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: UpdateAscContactInfo - UpdateAsync, Account Id: {0}", accountId), ex);
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    throw new CspNotificationException(ex.Message);
                }
            }

            return accountId;
        }

        public async Task<AscAccountCaseNotification> GetAscAccountCaseNotificationInfo(string accountId)
        {
            var query =
                string.Format(
                    @"SELECT Id, Account__c, Full_Name__c, Email_Address__c, Case_Comment_Notification__c, Case_Creation_Notification__c, Case_Creation_Notification_Critical__c, Is_CSP__c FROM Case_Creation_CC_List__c WHERE Account__c = '{0}' AND Is_CSP__c = true ORDER BY CreatedDate DESC LIMIT 1",
                    accountId);


            var clientInfo = GetSalesforceClient();
            var auth = new AuthenticationClient();

            try
            {
                await auth.UsernamePasswordAsync(clientInfo.ConsumerKey, clientInfo.ConsumerSecret, clientInfo.Username, clientInfo.Password, clientInfo.TokenEndPointUrl);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetAscAccountCaseNotificationInfo - UsernamePasswordAsync: Username: {0}, Endpoint Url: {1}", clientInfo.Username ?? string.Empty, clientInfo.TokenEndPointUrl ?? string.Empty), ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new CspNotificationException(ex.Message);
            }

            var forceClient = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);
            var accounts = new QueryResult<dynamic>();

            try
            {
                accounts = await forceClient.QueryAsync<dynamic>(query);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetAscAccountCaseNotificationInfo - QueryAsync: Account Id: {0}", accountId), ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new CspNotificationException(ex.Message);
            }

            if (accounts.Records.Count > 0)
            {
                var account = accounts.Records[0];

                return new AscAccountCaseNotification()
                {
                    Id = account.Id,
                    SfdcAccountId = account.Account__c,
                    FullName = account.Full_Name__c,
                    EmailAddress = account.Email_Address__c,
                    IsCaseCommentNotification = account.Case_Comment_Notification__c,
                    IsCaseCreationNotification = account.Case_Creation_Notification__c,
                    IsCriticalCaseCreationNotification = account.Case_Creation_Notification_Critical__c
                };
            }

            return new AscAccountCaseNotification()
            {
                SfdcAccountId = accountId
            };
        }

        public async Task<string> AddUpdateAscAccountCaseNotificationInfo(AscAccountCaseNotification ascAccountCaseNotificationInfo)
        {
            var caseNotificationId = ascAccountCaseNotificationInfo.Id;
            dynamic caseNotificationInfo = new ExpandoObject();

            caseNotificationInfo.Account__c = ascAccountCaseNotificationInfo.SfdcAccountId;
            caseNotificationInfo.Email_Address__c = ascAccountCaseNotificationInfo.EmailAddress;
            caseNotificationInfo.Case_Comment_Notification__c = ascAccountCaseNotificationInfo.IsCaseCommentNotification;
            caseNotificationInfo.Case_Creation_Notification__c = ascAccountCaseNotificationInfo.IsCaseCreationNotification;
            caseNotificationInfo.Case_Creation_Notification_Critical__c = ascAccountCaseNotificationInfo.IsCriticalCaseCreationNotification;

            var clientInfo = GetSalesforceClient();
            var auth = new AuthenticationClient();

            try
            {
                await auth.UsernamePasswordAsync(clientInfo.ConsumerKey, clientInfo.ConsumerSecret, clientInfo.Username, clientInfo.Password, clientInfo.TokenEndPointUrl);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: AddUpdateAscAccountCaseNotificationInfo - UsernamePasswordAsync: Username: {0}, Endpoint Url: {1}", clientInfo.Username ?? string.Empty, clientInfo.TokenEndPointUrl ?? string.Empty), ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new CspNotificationException(ex.Message);
            }

            var forceClient = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);

            if (string.IsNullOrEmpty(ascAccountCaseNotificationInfo.Id))
            {
                try
                {
                    caseNotificationInfo.Is_CSP__c = true;

                    var caseNotification = await forceClient.CreateAsync("Case_Creation_CC_List__c", caseNotificationInfo);

                    if (caseNotification.Success)
                    {
                        caseNotificationId = caseNotification.Id;
                    }
                }
                catch (Exception ex)
                {
                    _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: AddUpdateAscAccountCaseNotificationInfo - CreateAsync, Email Address: {0}", ascAccountCaseNotificationInfo.EmailAddress), ex);
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    throw new CspNotificationException(ex.Message);
                }
            }
            else
            {
                try
                {
                    if (string.IsNullOrEmpty(ascAccountCaseNotificationInfo.EmailAddress))
                    {
                        await forceClient.DeleteAsync("Case_Creation_CC_List__c", caseNotificationId);
                    }
                    else
                    {
                        await forceClient.UpdateAsync("Case_Creation_CC_List__c", caseNotificationId, caseNotificationInfo);
                    }
                }
                catch (Exception ex)
                {
                    _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: AddUpdateAscAccountCaseNotificationInfo - UpdateAsync: Case Notification Id: {0}, Email Address: {1}", caseNotificationId, ascAccountCaseNotificationInfo.EmailAddress), ex);
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    throw new CspNotificationException(ex.Message);
                }
            }

            return caseNotificationId;
        }

        public async Task<List<SupportCaseInfo>> GetCaseAlerts(string accountId, DateTime lastLoginDateTime)
        {
            // Comments By LK On 05/16/2018
            // GCSOS-1417 - Ignore Last login date to show all the pending case notifications for a given account
            var lastModifiedDate = ConvertToForceDateTime(lastLoginDateTime);
            var caseAlertInfoList = new List<SupportCaseInfo>();
            var query =
                string.Format(
                    @"SELECT Id, AccountId, CaseNumber, Status, CreatedBy.Name, CreatedDate, LastModifiedBy.Name, LastModifiedDate FROM case WHERE AccountId = '{0}' AND Status IN ('Awaiting Customer Verification', 'Customer Action Requested', 'Pending Customer') ORDER BY LastModifiedDate DESC",
                    accountId, lastModifiedDate);

            var clientInfo = GetSalesforceClient();
            var auth = new AuthenticationClient();

            try
            {
                await auth.UsernamePasswordAsync(clientInfo.ConsumerKey, clientInfo.ConsumerSecret, clientInfo.Username, clientInfo.Password, clientInfo.TokenEndPointUrl);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetCaseAlerts - UsernamePasswordAsync: Username: {0}, Endpoint Url: {1}", clientInfo.Username ?? string.Empty, clientInfo.TokenEndPointUrl ?? string.Empty), ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new CspNotificationException(ex.Message);
            }


            var forceClient = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);
            var supportCases = new QueryResult<dynamic>();

            try
            {
                supportCases = await forceClient.QueryAsync<dynamic>(query);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetCaseAlerts - QueryAsync: Account Id: {0}", accountId), ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new CspNotificationException(ex.Message);
            }

            if (supportCases.Records.Count > 0)
            {
                var caseIdList = new List<string>();
                
                for (int i = 0; i < supportCases.Records.Count; i++)
                {
                    var supportCase = supportCases.Records[i];
                    string caseId = supportCase.Id;

                    var caseAlertInfo = new SupportCaseInfo()
                    {
                        Id = supportCase.Id,
                        AccountId = supportCase.AccountId,
                        CaseNumber = supportCase.CaseNumber,
                        Status = supportCase.Status,
                        CreatedDate = supportCase.CreatedDate,
                        LastModifiedDate = supportCase.LastModifiedDate,
                        CreatedBy = (supportCase.CreatedBy != null) ? supportCase.CreatedBy.Name : string.Empty,
                        LastModifiedBy = (supportCase.LastModifiedBy != null) ? supportCase.LastModifiedBy.Name : string.Empty
                    };

                    caseAlertInfoList.Add(caseAlertInfo);
                    caseIdList.Add(caseId);
                }

                var caseIds = string.Format("'{0}'", string.Join("','", caseIdList));

                // RMA Status Info
                var caseRmaStatusList = new QueryResult<dynamic>();

                try
                {
                    // Case_Number__c is a lookup to Case Id in Salesforce
                    var caseRmaStatusQuery = string.Format(@"SELECT Case_Number__c FROM RMA__c WHERE RMA_status__c = 'Pending Address' AND Case_Number__c IN ({0})", caseIds);
                    caseRmaStatusList = await forceClient.QueryAsync<dynamic>(caseRmaStatusQuery);
                }
                catch (Exception ex)
                {
                    _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetCaseAlerts - QueryAsync: CaseRmaStatus: {0} {1}", accountId, lastModifiedDate), ex);
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    throw new CspNotificationException(ex.Message);
                }

                var caseRmaStatusIdList = new List<string>();

                if (caseRmaStatusList.Records.Count > 0)
                {
                    for (int i = 0; i < caseRmaStatusList.Records.Count; i++)
                    {
                        var caseRmaStatus = caseRmaStatusList.Records[i];
                        string caseNumber = caseRmaStatus.Case_Number__c;

                        caseRmaStatusIdList.Add(caseNumber);
                    }
                }

                for (int i = 0; i < caseRmaStatusIdList.Count; i++)
                {
                    caseAlertInfoList[i].IsRma = caseRmaStatusIdList.Contains(caseAlertInfoList[i].Id);
                }
            }

            return caseAlertInfoList;
        }

        public async Task<List<SupportCaseInfo>> GetCaseRecentActivity(string accountId, string contactId, DateTime lastLoginDateTime)
        {
            var lastModifiedDate = ConvertToForceDateTime(lastLoginDateTime);
            var caseAlertInfoList = new List<SupportCaseInfo>();

            var contactIdQuery = !string.IsNullOrEmpty(contactId) ? string.Format(" AND ContactId = '{0}'", contactId) : string.Empty;
            var query = 
                string.Format(
                    @"SELECT Id, AccountId, CaseNumber, Status, Subject, CreatedBy.Name, CreatedDate, LastModifiedBy.Name, LastModifiedDate, (SELECT Id, CaseId, NewValue, OldValue FROM Histories WHERE Field = 'Status' ORDER BY CreatedDate DESC LIMIT 1) FROM case WHERE AccountId = '{0}'{1} AND Status IN ('New', 'Awaiting Customer Verification', 'Customer Action Requested', 'Pending Customer', 'Reopen', 'Closed') AND LastModifiedDate >= {2} ORDER BY LastModifiedDate DESC LIMIT 21",
                    accountId, contactIdQuery, lastModifiedDate);

            var clientInfo = GetSalesforceClient();
            var auth = new AuthenticationClient();

            try
            {
                await auth.UsernamePasswordAsync(clientInfo.ConsumerKey, clientInfo.ConsumerSecret, clientInfo.Username, clientInfo.Password, clientInfo.TokenEndPointUrl);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetCaseRecentActivity - UsernamePasswordAsync: Username: {0}, Endpoint Url: {1}", clientInfo.Username ?? string.Empty, clientInfo.TokenEndPointUrl ?? string.Empty), ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new CspNotificationException(ex.Message);
            }


            var forceClient = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);
            var supportCases = new QueryResult<dynamic>();

            try
            {
                supportCases = await forceClient.QueryAsync<dynamic>(query);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetCaseRecentActivity - QueryAsync: Account Id: {0}", accountId), ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new CspNotificationException(ex.Message);
            }

            if (supportCases.Records.Count > 0)
            {
                for (int i = 0; i < supportCases.Records.Count; i++)
                {
                    var supportCase = supportCases.Records[i];

                    var caseAlertInfo = new SupportCaseInfo()
                    {
                        Id = supportCase.Id,
                        AccountId = supportCase.AccountId,
                        CaseNumber = supportCase.CaseNumber,
                        Status = supportCase.Status,
                        Subject = supportCase.Subject,
                        CreatedDate = supportCase.CreatedDate,
                        CreatedBy = (supportCase.CreatedBy != null) ? supportCase.CreatedBy.Name : string.Empty,
                        LastModifiedBy = (supportCase.LastModifiedBy != null) ? supportCase.LastModifiedBy.Name : string.Empty,
                        LastModifiedDate = supportCase.LastModifiedDate
                    };

                    if (supportCase.Histories != null && supportCase.Histories.records != null && supportCase.Histories.records.Count > 0)
                    {
                        var supportCaseHistory = supportCase.Histories.records[0];

                        var caseHistoryInfo = new SupportCaseHistoryInfo()
                        {
                            Id = supportCaseHistory.Id,
                            CaseId = supportCaseHistory.CaseId,
                            NewStatus = supportCaseHistory.NewValue,
                            OldStatus = supportCaseHistory.OldValue
                        };

                        caseAlertInfo.Status = "StatusChange";
                        caseAlertInfo.CaseHistory = caseHistoryInfo;
                    }

                    caseAlertInfoList.Add(caseAlertInfo);
                }

                // Case Comments
                var caseComments = new QueryResult<dynamic>();

                try
                {
                    var caseCommentsQuery = string.Format(@"SELECT Id, AccountId, CaseNumber, Status, Subject, CreatedBy.Name, CreatedDate, LastModifiedBy.Name, LastModifiedDate, (SELECT Id, ParentId, CreatedBy.Id, CreatedBy.Name, CreatedDate FROM CaseComments WHERE IsPublished = true AND CreatedDate >= {2} ORDER BY CreatedDate DESC LIMIT 1) FROM case WHERE AccountId = '{0}'{1} AND LastModifiedDate >= {2} ORDER BY LastModifiedDate DESC LIMIT 20", accountId, contactIdQuery, lastModifiedDate);
                    caseComments = await forceClient.QueryAsync<dynamic>(caseCommentsQuery);
                }
                catch (Exception ex)
                {
                    _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetCaseRecentActivity - QueryAsync: CaseComments: {0} {1}", accountId, lastModifiedDate), ex);
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    throw new CspNotificationException(ex.Message);
                }

                if (caseComments.Records.Count > 0)
                {
                    var caseCommentAlertInfoList = new List<SupportCaseInfo>();

                    for (int i = 0; i < caseComments.Records.Count; i++)
                    {
                        var supportCase = caseComments.Records[i];

                        if (supportCase.CaseComments != null && supportCase.CaseComments.records != null && supportCase.CaseComments.records.Count > 0)
                        {
                            var supportCaseComment = supportCase.CaseComments.records[0];

                            var caseAlertInfo = new SupportCaseInfo()
                            {
                                Id = supportCase.Id,
                                AccountId = supportCase.AccountId,
                                CaseNumber = supportCase.CaseNumber,
                                Status = "Customer Comment Added",
                                Subject = supportCase.Subject,
                                CreatedDate = supportCaseComment.CreatedDate,
                                CreatedBy = (supportCaseComment.CreatedBy != null) ? supportCaseComment.CreatedBy.Name : string.Empty,
                                LastModifiedBy = (supportCaseComment.CreatedBy != null) ? supportCaseComment.CreatedBy.Name : string.Empty,
                                LastModifiedDate = supportCaseComment.CreatedDate
                            };

                            caseCommentAlertInfoList.Add(caseAlertInfo);
                        }
                    }

                    caseAlertInfoList = caseAlertInfoList.Union(caseCommentAlertInfoList).ToList();
                }
            }

            return caseAlertInfoList.OrderByDescending(x => x.LastModifiedDate).Take(5).ToList();
        }

        public async Task<DateTime> GetUserLastLoginDateTime(long userAccountId, long accountId)
        {
            var lastLoginDate = DateTime.Now.AddDays(-3);
            var portalUsername = string.Empty;
            const string sqlQuery = @"SELECT SalesforceLogInId FROM membership 
                                      WHERE UserAccountId = @p_UserAccountId AND SupportAccountId = @p_SupportAccountId LIMIT 1";
            string connString = Secrets.CspDatabase;

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                var command = new MySqlCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("p_UserAccountId", userAccountId);
                command.Parameters.AddWithValue("p_SupportAccountId", accountId);

                using (var dataReader = command.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        portalUsername = dataReader["SalesforceLogInId"] == null || dataReader["SalesforceLogInId"] is DBNull ? null : dataReader["SalesforceLogInId"].ToString();
                    }
                }
            }

            if (!string.IsNullOrEmpty(portalUsername))
            {
                var query =
                string.Format(
                    @"SELECT LastLoginDate FROM User WHERE Username = '{0}' LIMIT 1", portalUsername);


                var clientInfo = GetSalesforceClient();
                var auth = new AuthenticationClient();

                try
                {
                    await auth.UsernamePasswordAsync(clientInfo.ConsumerKey, clientInfo.ConsumerSecret, clientInfo.Username, clientInfo.Password, clientInfo.TokenEndPointUrl);
                }
                catch (Exception ex)
                {
                    _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetUserLastLoginDate - UsernamePasswordAsync: Username: {0}, Endpoint Url: {1}", clientInfo.Username ?? string.Empty, clientInfo.TokenEndPointUrl ?? string.Empty), ex);
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    throw new CspNotificationException(ex.Message);
                }

                var forceClient = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);
                var userAccounts = new QueryResult<dynamic>();

                try
                {
                    userAccounts = await forceClient.QueryAsync<dynamic>(query);
                }
                catch (Exception ex)
                {
                    _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetUserLastLoginDate - QueryAsync: Username: {0}", portalUsername), ex);
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    throw new CspNotificationException(ex.Message);
                }

                if (userAccounts.Records.Count > 0)
                {
                    var userAccount = userAccounts.Records[0];

                    if (userAccount != null && userAccount.LastLoginDate != null)
                    {
                        lastLoginDate = userAccount.LastLoginDate;
                    }
                }
            }

            return lastLoginDate;
        }

        public async Task<bool> IsSupportOnlyAccount(string accountId)
        {
            var query =
                string.Format(@"SELECT Id, Name FROM account WHERE Id = '{0}' AND Ownership='Support Segmentation' LIMIT 1", accountId);


            var clientInfo = GetSalesforceClient();
            var auth = new AuthenticationClient();

            try
            {
                await auth.UsernamePasswordAsync(clientInfo.ConsumerKey, clientInfo.ConsumerSecret, clientInfo.Username, clientInfo.Password, clientInfo.TokenEndPointUrl);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetAscContactInfo - UsernamePasswordAsync: Username: {0}, Endpoint Url: {1}", clientInfo.Username ?? string.Empty, clientInfo.TokenEndPointUrl ?? string.Empty), ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new CspNotificationException(ex.Message);
            }

            var forceClient = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);
            var accounts = new QueryResult<dynamic>();

            try
            {
                accounts = await forceClient.QueryAsync<dynamic>(query);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: IsSupportOnlyAccount - QueryAsync: Account Id: {0}", accountId), ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw new CspNotificationException(ex.Message);
            }

            if (accounts.Records.Count > 0)
            {
                return true;
            }

            return false;
        }

        public string ConvertToForceDateTime(DateTime dateTime)
        {
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        private SalesforceClient GetSalesforceClient()
        {
            return new SalesforceClient
            {
                Username = _applicationSettings.SalesforceUser,
                Password = _applicationSettings.SalesforcePassword,
                ConsumerKey = _applicationSettings.SalesforceConsumerKey,
                ConsumerSecret = _applicationSettings.SalesforceConsumerSecret,
                TokenEndPointUrl = _applicationSettings.SalesforceTokenRequestEndpointUrl
            };
        }

        public async Task<List<AccountTeamInfo>> GetAccountTeamMembers(List<string> salesforceAccountList)
        {
            var accountIds = string.Format("'{0}'", string.Join("','", salesforceAccountList));
            var query =
                string.Format(
                    @"SELECT AccountId, UserId, User.FirstName, User.LastName, User.Email, TeamMemberRole FROM AccountTeamMember WHERE AccountId IN ({0}) AND User.IsActive = true",
                    accountIds);

            var clientInfo = GetSalesforceClient();
            var auth = new AuthenticationClient();
            var accountTeamMembers = new List<AccountTeamInfo>();

            try
            {
                await auth.UsernamePasswordAsync(clientInfo.ConsumerKey, clientInfo.ConsumerSecret, clientInfo.Username,
                    clientInfo.Password, clientInfo.TokenEndPointUrl);

            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetAscContactInfo - UsernamePasswordAsync: Username: {0}, Endpoint Url: {1}", clientInfo.Username ?? string.Empty, clientInfo.TokenEndPointUrl ?? string.Empty), ex);
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                else
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                }
                throw new CspNotificationException(ex.Message);
            }

            var forceClient = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);
            var members = new QueryResult<dynamic>();

            try
            {
                members = await forceClient.QueryAsync<dynamic>(query);
            }
            catch (Exception ex)
            {
                _salesforceLogsRepository.Log(string.Empty, clientInfo.Username, string.Empty, SalesforceLogTypes.RestApiError, string.Format("Method: GetAscContactInfo - QueryAsync: Account Id: {0}", salesforceAccountList), ex);
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
                else
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Elmah.Error(ex));
                }
                throw new CspNotificationException(ex.Message);
            }

            if (members.Records.Count > 0)
            {
                for (int i = 0; i < members.Records.Count; i++)
                {
                    var member = members.Records[i];

                    var accountTeamMember = new AccountTeamInfo()
                    {
                        AccountId = member.AccountId,
                        UserId = member.UserId,
                        FirstName = member.User.FirstName,
                        LastName = member.User.LastName,
                        Email = member.User.Email,
                        Role = member.TeamMemberRole
                    };
                    accountTeamMembers.Add(accountTeamMember);
                }
            }

            return accountTeamMembers;
        }
    }

    public class SalesforceClient
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string TokenEndPointUrl { get; set; }
    }
}
