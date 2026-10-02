using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Api.DataAccess;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Ela2;
using Csp.Api.Model.Ela8;
using Csp.Api.Model.Enums;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Newtonsoft.Json;

namespace Csp.Api.Logic
{
    public class SiteLicenseLogic : ISiteLicenseLogic
    {
        private readonly ISiteLicenseRepository _siteLicenseRepository;
        private readonly ISecurityRepository _securityRepository;
        private readonly IAutoFocusApiRepository _autoFocusApiRepository;
        private readonly ISiteLicenseDetailsRepository _siteLicenseDetailsRepository;
        private readonly ISalesforceRestApiLogic _salesforceRestApiLogic;

        public SiteLicenseLogic(ISiteLicenseRepository siteLicenseRepository, ISecurityRepository securityRepository,
            IAutoFocusApiRepository autoFocusApiRepository, ISiteLicenseDetailsRepository siteLicenseDetailsRepository,
            ISalesforceRestApiLogic salesforceRestApiLogic)
        {
            _siteLicenseRepository = siteLicenseRepository;
            _securityRepository = securityRepository;
            _autoFocusApiRepository = autoFocusApiRepository;
            _siteLicenseDetailsRepository = siteLicenseDetailsRepository;
            _salesforceRestApiLogic = salesforceRestApiLogic;
        }

        public IEnumerable<SiteLicenseItem> GetSiteLicenses(long supportAccountId, long userAccountId, DataParameters dataParameters)
        {
            var siteLics = _siteLicenseRepository.GetSiteLicenses(supportAccountId, dataParameters).ToArray();

            foreach (var siteLic in siteLics)
            {
                if (siteLic.MainAuthCode.ToUpper().StartsWith("X") && siteLic.ParentLicenseId.HasValue)
                {
                    // Get the main authcode
                    string authCode =
                        _siteLicenseRepository.GetEnterpriseAgreementMainAuthCode(siteLic.ParentLicenseId.Value);
                    siteLic.MainAuthCode = authCode;
                }
            }


            var apiKeyStatuses = _siteLicenseDetailsRepository.GetSiteLicenseDetails(supportAccountId).ToArray();

            //Add Threatresearcher stats
            if (siteLics.Any(x => x.IsAutofocus))
            {
                //Set current use count and state
                var currentUseCount = _securityRepository.GetCurrentThreatResearchersCount(supportAccountId);

                foreach (var afLic in siteLics.Where(x => x.IsAutofocus))
                {
                    afLic.UsedLicenses = currentUseCount;

                    var keyState = apiKeyStatuses.SingleOrDefault(x => x.LicenseId == afLic.LicenseId);

                    if (keyState == null)
                    {
                        var keyResult = GetAutofocusApiKey(supportAccountId, userAccountId, afLic.LicenseId);
                        if (keyResult.IsSuccess == false)
                        {
                            throw new Exception("GET AF API Key failure");
                        }

                        afLic.ApiKeyState = keyResult.State;
                    }
                    else
                    {
                        afLic.ApiKeyState = keyState.ApiKeyState;
                    }

                    if (afLic.ApiKeyState == AfApiKeyStatuses.Unknown)
                    {
                        throw new InvalidEnumArgumentException();
                    }
                }
            }

            return siteLics;
        }

        public AutoFocusKeyResult RegenerateAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId)
        {
            var deleteResult = DeleteAutofocusApiKey(supportAccountId, userAccountId, licenseId);

            if (deleteResult.IsSuccess == false)
            {
                return deleteResult;
            }

            return GenerateAutofocusApiKey(supportAccountId, userAccountId, licenseId);
        }

        public AutoFocusKeyResult EnableOrGenerateAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId)
        {
            var currentKey = GetAutofocusApiKey(supportAccountId, userAccountId, licenseId);

            if (currentKey.IsSuccess == false)
            {
                throw new Exception("Failed to get key");
            }

            if (currentKey.State == AfApiKeyStatuses.Disabled)
            {
                return EnableAutofocusApiKey(supportAccountId, userAccountId, licenseId);
            }

            if (currentKey.State == AfApiKeyStatuses.Deleted)
            {
                return GenerateAutofocusApiKey(supportAccountId, userAccountId, licenseId);
            }

            if (currentKey.State == AfApiKeyStatuses.Enabled)
            {
                return currentKey;
            }

            throw new InvalidEnumArgumentException();
        }

        public AutoFocusKeyResult GenerateAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId)
        {
            //Send license data first (numusers/exp date) b/c you can't create a new key if they don't already have this data
            var afLic = _siteLicenseRepository.GetAutoFocusLicenseDetails(supportAccountId, licenseId);
            var postLicenseResult = _autoFocusApiRepository.PutLicenseKey(supportAccountId, afLic.UseCount, afLic.ExpirationDate, userAccountId);
            if (postLicenseResult.IsErrorMessage)
            {
                _siteLicenseDetailsRepository.ClearSiteLicenseDetail(supportAccountId, licenseId);
                return new AutoFocusKeyResult
                {
                    IsSuccess = false,
                };
            }

            var creationResult = _autoFocusApiRepository.PostKeyCreation(supportAccountId, userAccountId);
            if (creationResult.IsErrorMessage)
            {
                _siteLicenseDetailsRepository.ClearSiteLicenseDetail(supportAccountId, licenseId);
                return new AutoFocusKeyResult
                {
                    IsSuccess = false,
                };
            }

            _siteLicenseDetailsRepository.SetSiteLicenseDetail(supportAccountId, licenseId, AfApiKeyStatuses.Enabled);

            return new AutoFocusKeyResult
            {
                IsSuccess = true,
                ApiKey = creationResult.Key,
            };
        }

        public AutoFocusKeyResult DisableAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId)
        {
            var disableResult = _autoFocusApiRepository.PutStatusUpdate(supportAccountId, enable: false, userAccountId: userAccountId);
            if (disableResult.IsErrorMessage)
            {
                _siteLicenseDetailsRepository.ClearSiteLicenseDetail(supportAccountId, licenseId);
                return new AutoFocusKeyResult
                {
                    IsSuccess = false,
                };
            }

            _siteLicenseDetailsRepository.SetSiteLicenseDetail(supportAccountId, licenseId, AfApiKeyStatuses.Disabled);

            return new AutoFocusKeyResult
            {
                IsSuccess = true,
            };
        }

        public AutoFocusKeyResult EnableAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId)
        {
            var enableResult = _autoFocusApiRepository.PutStatusUpdate(supportAccountId, enable: true, userAccountId: userAccountId);
            if (enableResult.IsErrorMessage)
            {
                _siteLicenseDetailsRepository.ClearSiteLicenseDetail(supportAccountId, licenseId);
                return new AutoFocusKeyResult
                {
                    IsSuccess = false,
                };
            }

            _siteLicenseDetailsRepository.SetSiteLicenseDetail(supportAccountId, licenseId, AfApiKeyStatuses.Enabled);

            return new AutoFocusKeyResult
            {
                IsSuccess = true,
                ApiKey = enableResult.Key,
            };
        }

        public AutoFocusKeyResult DeleteAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId)
        {
            var deleteResult = _autoFocusApiRepository.DeleteKey(supportAccountId, userAccountId);
            if (deleteResult.IsErrorMessage)
            {
                _siteLicenseDetailsRepository.ClearSiteLicenseDetail(supportAccountId, licenseId);
                return new AutoFocusKeyResult
                {
                    IsSuccess = false,
                };
            }

            _siteLicenseDetailsRepository.SetSiteLicenseDetail(supportAccountId, licenseId, AfApiKeyStatuses.Deleted);

            return new AutoFocusKeyResult
            {
                IsSuccess = true,
            };
        }

        public AutoFocusKeyResult GetAutofocusApiKey(long supportAccountId, long userAccountId, long licenseId)
        {
            var keyResult = _autoFocusApiRepository.GetKey(supportAccountId, userAccountId);
            if (keyResult.IsErrorMessage || keyResult.Status == AfApiKeyStatuses.Unknown)
            {
                _siteLicenseDetailsRepository.ClearSiteLicenseDetail(supportAccountId, licenseId);
                return new AutoFocusKeyResult
                {
                    IsSuccess = false,
                };
            }

            _siteLicenseDetailsRepository.SetSiteLicenseDetail(supportAccountId, licenseId, keyResult.Status);

            return new AutoFocusKeyResult
            {
                IsSuccess = true,
                ApiKey = keyResult.Status == AfApiKeyStatuses.Enabled ? keyResult.Key : string.Empty,
                State = keyResult.Status,
            };
        }

        public async Task<IEnumerable<EnterpriseAgreementItem>> GetEnterpriseAgreements(long supportAccountId, bool hasVmElaTokenManagementPermission)
        {
            var hasSiteLicense = _siteLicenseRepository.HasSiteLicense(supportAccountId);
            if (!hasSiteLicense)
                Enumerable.Empty<EnterpriseAgreementItem>();

            var licenseDataAccess = new LicenseDataAccess();
            var result = (List<EnterpriseAgreementItem>)_siteLicenseRepository.GetEnterpriseAgreements(supportAccountId);
            result.AddRange(licenseDataAccess.GetEnterpriseAgreementsForTrapsELA(supportAccountId));
            var vmElaItems = await _siteLicenseRepository.GetVmELaItems(supportAccountId);
            if (vmElaItems != null && vmElaItems.Any())
            {
                //bounded/unbounded period check
                var grandAuthCodesInfo = await _siteLicenseRepository.GetVmElaAuthCodesInfo(supportAccountId);
                var unboundedDic = new Dictionary<string, bool>();
                foreach (var model in grandAuthCodesInfo)
                {
                    unboundedDic[model.GrandParentAuthCode] = VirtualMachineLogic.CheckVmElaUnboundedStatus(EVmType.Vm, model.GrandParentAuthCode, model);
                    if (unboundedDic[model.GrandParentAuthCode] && model.AdditionalTokenAppliedDate == null)
                    {
                        await _siteLicenseRepository.ApplyVmElaAdditionalTokens(model.GrandParentAuthCode, model.TotalTokens);
                        model.AdditionalTokenAppliedDate = DateTime.Now;
                        var item = vmElaItems.SingleOrDefault(e => e.AuthorizationCode.Equals(model.GrandParentAuthCode));
                        if (item != null)
                        {
                            item.VmElaGrandParentTotalTokens = model.TotalTokens;
                        }
                    }
                }

                foreach (var item in vmElaItems)
                {
                    if (unboundedDic[item.AuthorizationCode])
                        item.IsUnbounded = true;
                    item.HasTokenManagementPermission = hasVmElaTokenManagementPermission;
                }

                var result1 = (List<EnterpriseAgreementItem>)result;
                result1.AddRange((List<EnterpriseAgreementItem>)vmElaItems);
                return result1;
            }

            return result;
        }

            public bool HasEsaLicense(long supportAccountId)
            {
                return _siteLicenseRepository.HasEsaLicense(supportAccountId);
            }

            public bool Has4HREsaLicense(long supportAccountId)
            {
                return _siteLicenseRepository.Has4HREsaLicense(supportAccountId);
            }

        public bool HasEaLicense(long supportAccountId)
        {
            return _siteLicenseRepository.HasEaLicense(supportAccountId);
        }

        public bool HasUrl4License(long supportAccountId)
        {
            return _siteLicenseRepository.HasElaUrl4License(supportAccountId);
        }

        public long GetELAConversionRequestId(long supportAccountId)
        {
            return _siteLicenseRepository.GetELAConversionRequestId(supportAccountId);
        }

        public bool HasConvertURL4ToADVURLRoles(long supportAccountId, long userAccountId)
        {
            // Check if user has the necessary roles to Convert URL4 to ADVURL
            bool hasEla2ConvertADVURL2Roles = _securityRepository.HasRole(supportAccountId, userAccountId, RoleNames.SuperUser)
                                          || _securityRepository.HasRole(supportAccountId, userAccountId, RoleNames.ElaAdministrator);

            return hasEla2ConvertADVURL2Roles;
        }

        public Ela2ButtonLinks GetEla2ButtonLinks(long supportAccountId, long userAccountId)
        {
            Ela2ButtonLinks ela2ButtonLinks = new Ela2ButtonLinks();

            // Check if user has the necessary roles for Ela2 activation buttons
            ela2ButtonLinks.HasEla2Roles = _securityRepository.HasRole(supportAccountId, userAccountId, RoleNames.DomainAdministrator)
                || _securityRepository.HasRole(supportAccountId, userAccountId, RoleNames.ElaAdministrator); // either role suffices to see the buttons

            // Call API to get the hub activiation links
            var responsePayload = LicenseApis.GetEla2EligibilityData<Ela2EligibilityModel>(supportAccountId);

            ela2ButtonLinks.Ela2IoTButtonLink = responsePayload?.Ela2Links?.Ela2EligibilityIoTLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.Ela2SaaSButtonLink = responsePayload?.Ela2Links?.Ela2EligibilitySaaSLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.Ela2AiOpsButtonLink = responsePayload?.Ela2Links?.Ela2EligibilityAiOpsLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.ElaScmButtonLink = responsePayload?.Ela2Links?.Ela2EligibilityScmLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.ElaDlpButtonLink = responsePayload?.Ela2Links?.Ela2EligibilityDlpLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.ElaAdnsrButtonLink = responsePayload?.Ela2Links?.Ela2EligibilityAdnsrLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.EsaScmButtonLink = responsePayload?.EsaLinks?.EsaEligibilityScmLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.ElaQuantumButtonLink = responsePayload?.Ela2Links?.Ela2EligibilityQuantumLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.ElaSdWanAddCgxButtonLink = responsePayload?.Ela2Links?.EsaEligibilitySdWanAddCgxLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.ElaSdWanAddCoresecButtonLink = responsePayload?.Ela2Links?.EsaEligibilitySdWanAddCoresecLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.ElaSdWanZingboxButtonLink = responsePayload?.Ela2Links?.EsaEligibilitySdWanZingboxLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.ElaSdWanLoggingServiceButtonLink = responsePayload?.Ela2Links?.EsaEligibilitySdWanLoggingServiceLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.EsaSdWanAddCgxButtonLink = responsePayload?.EsaLinks?.EsaEligibilitySdWanAddCgxLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.EsaSdWanAddCoresecButtonLink = responsePayload?.EsaLinks?.EsaEligibilitySdWanAddCoresecLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.EsaSdWanZingboxButtonLink = responsePayload?.EsaLinks?.EsaEligibilitySdWanZingboxLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.EsaSdWanLoggingServiceButtonLink = responsePayload?.EsaLinks?.EsaEligibilitySdWanLoggingServiceLink?.EsaEla2EligibilityUrl;
            ela2ButtonLinks.Ela8AdnsrButtonLink = responsePayload?.Ela8Links?.Ela8EligibilityAdnsrLink?.EsaEla2EligibilityUrl;

            return ela2ButtonLinks;
        }

        /// <summary>
        /// Sends email to all ELA Admins, bcc Sales Ops, Product Managemnt and Account Team
        /// when token consumption is nearing 80%, 90% or 100%
        /// </summary>
        /// <returns></returns>
        public void SendVmElaTokenUsageNotification()
        {
            var salesOpsEmailAlias = ConfigurationManager.AppSettings["SalesOpsEmail"];
            var productMgmtEmailAlias = ConfigurationManager.AppSettings["ProductMgmtEmail"];
            var internalOpsEmails = new List<string> { salesOpsEmailAlias, productMgmtEmailAlias };

            var tokenNotificationList = _siteLicenseRepository.GetVmElaTokenNotificationList();
            var sfdcAccounts = tokenNotificationList.Select(t => t.SalesforceAccountId).Distinct().ToList();

            var accountTeamTask = Task.Run(async () => await _salesforceRestApiLogic.GetAccountTeamMembers(sfdcAccounts));
            var accountTeamMembers = accountTeamTask.Result.ToList();

            foreach (var sfdcAccount in sfdcAccounts)
            {
                var elaOpsEmails = accountTeamMembers.Where(a => a.AccountId.Equals(sfdcAccount)).Select(e => e.Email).ToList();
                elaOpsEmails.AddRange(internalOpsEmails);
                tokenNotificationList.Where(t => t.SalesforceAccountId.Equals(sfdcAccount)).ForEach(t => t.InternalElaOps = elaOpsEmails);
            }

            //Send Email to all the ELA Admins
            foreach (var tokenNotification in tokenNotificationList)
            {
                //Send email to ELA Admin
                EmailHelper.SendVmElaTokenUsageNotification(tokenNotification);
                //Update the VM ELA token notification log
                var tokenNotificationId = _siteLicenseRepository.UpdateVmElaTokenNotificationLog(tokenNotification);

                //Publish Event to PubSub
                if (tokenNotificationId > 0)
                {
                    #region Publish Event to Pubsub

                    AppNotificationData tokenUtilizationData = new AppNotificationData()
                    {
                        AccountId = Convert.ToString(tokenNotification.SupportAccountId),
                        EntityId = Convert.ToString(tokenNotificationId),
                        UserId = Convert.ToString(tokenNotification.UserAccountId)
                    };

                    AppNotification tokenUtilizationNotification = new AppNotification()
                    {
                        AppData = tokenUtilizationData,
                        Category = NotificationCategory.VMELATokenUtilization.ToDescription(),
                        DeliveryChannel = new List<string>() { NotificationDeliveryChannel.Web.ToDescription() },
                        Source = NotificationSource.Csp.ToDescription()
                    };
                    EventPublisher eventPublisher = new EventPublisher();
                    Task.Run(async () => await eventPublisher.PublishEvent(tokenUtilizationNotification));
                    #endregion
                }
            }
        }

        public ElaConversionStatusDataResponse GetConversionStatus(long requestId)
        {
            HttpWebRequest request = null;
            ElaConversionStatusDataResponse itemData = new ElaConversionStatusDataResponse();

            if (requestId > 0)
            {
                var entitlementServiceBffHostThumbprint = ConfigurationManager.AppSettings["EntitlementBffThumbprint"];
                string restUrl = ConfigurationManager.AppSettings["EntitlementServiceBffHost"];
                string url = restUrl + "/v1/ela-conversion-history?request_id=" + requestId;
                request = WebCommon.GetWebRequestWithClientCertificate(url, "GET", entitlementServiceBffHostThumbprint,
                    null);

                using (WebResponse webResponse = request.GetResponse())
                 {
                     using (Stream webStream = webResponse.GetResponseStream())
                     {
                         using (StreamReader responseReader = new StreamReader(webStream))
                         {
                             itemData = JsonConvert.DeserializeObject<ElaConversionStatusDataResponse>(
                                 responseReader.ReadToEnd(), new JsonSerializerSettings()
                                 {
                                     MaxDepth = 32,
                                     TypeNameHandling = TypeNameHandling.None
                                 });
                         }
                     }
                 }
            }
            ElaConversionStatusDataResponse returnData = new ElaConversionStatusDataResponse();
            if (itemData.Data != null)
                returnData.Data = itemData.Data;

            return returnData;
        }

        public ElaConversionStatusDataResponse CreateConversionStatus(ElaConversionStatus elaConversionStatus)
        {
            string requestData = JsonConvert.SerializeObject(elaConversionStatus);
            HttpWebRequest request = null;
            var entitlementServiceBffHostThumbprint = ConfigurationManager.AppSettings["EntitlementBffThumbprint"];
            string restUrl = ConfigurationManager.AppSettings["EntitlementServiceBffHost"];
            string url = restUrl + "/v1/ela-conversion-history";
            request = WebCommon.GetWebRequestWithClientCertificate(url, "POST", entitlementServiceBffHostThumbprint, requestData);

            ElaConversionStatusDataResponse itemData = new ElaConversionStatusDataResponse();
            using (WebResponse webResponse = request.GetResponse())
            {
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    using (StreamReader responseReader = new StreamReader(webStream))
                    {
                        itemData = JsonConvert.DeserializeObject<ElaConversionStatusDataResponse>(responseReader.ReadToEnd(), new JsonSerializerSettings()
                        {
                            MaxDepth = 32,
                            TypeNameHandling = TypeNameHandling.None
                        });
                    }
                }
            }

            return itemData;
        }

        public ElaConversionStatusLicenseDataResponse[] LicenseUrl4ConversionAPI(ElaLicenseConversionStatus elaConversionStatus)
        {
            string requestData = "[" + JsonConvert.SerializeObject(elaConversionStatus) + "]";
            HttpWebRequest request = null;

            var licenseApiThumbprint = ConfigurationManager.AppSettings["LicenseApiThumbprint"];
            string baseUrl = ConfigurationManager.AppSettings["LicenseApi"];
            string url = baseUrl + "/coresvc/v1/urlconversion";
            request = WebCommon.GetWebRequestWithClientCertificate(url, "POST", licenseApiThumbprint, requestData);
            ElaConversionStatusLicenseDataResponse[] itemData = null;

            using (WebResponse webResponse = request.GetResponse())
            {
                using (Stream webStream = webResponse.GetResponseStream())
                {
                    using (StreamReader responseReader = new StreamReader(webStream))
                    {
                        itemData = JsonConvert.DeserializeObject<ElaConversionStatusLicenseDataResponse[]>(responseReader.ReadToEnd(), new JsonSerializerSettings()
                        {
                            MaxDepth = 32,
                            TypeNameHandling = TypeNameHandling.None
                        });
                    }
                }
            }

            return itemData;
        }
    }
}
