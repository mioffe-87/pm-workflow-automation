using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using AutoMapper;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using LINQtoCSV;
using System.Configuration;
using System.Net.Http;
using System.Net;
using System.Text;
using Csp.Api.DataAccess;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Enums;
using Newtonsoft.Json;
using System.Threading;

namespace Csp.Api.Logic
{
    public class SaasLogic :BaseLogic, ISaasLogic
    {
        private readonly ISaasDataAccess _saasDataAccess;
        private readonly ISaasRegistrationRepository _saasRegistrationRepository;
        private readonly ILicenseLogic _licenseLogic;
        private readonly ISfdcEntitlementLogic _sfdcEntitlementLogic;
        private readonly ILicensingApiLogic _iLicensingApiLogic;

        // CDL activation config
        private readonly string licenseCoreApiURL = ConfigurationManager.AppSettings["LicenseApi"].ToString();
        private readonly string licenseCoreApiCertThumbprint = ConfigurationManager.AppSettings["LicenseApiThumbprint"].ToString();

        private readonly  string coreAPIURL = ConfigurationManager.AppSettings["CoreAPIUrl"].ToString();
        private readonly string coreAPIuser = ConfigurationManager.AppSettings["LicensingServiceUserName"].ToString();
        private readonly string coreAPIkey = ConfigurationManager.AppSettings["LicensingServicePassword"].ToString();
        private readonly string coreAPIAllowedAuthCodeTypes = ConfigurationManager.AppSettings["CoreAPIAllowedAuthCodeTypes"].ToString();
        private readonly bool useCoreAPI = Convert.ToBoolean(ConfigurationManager.AppSettings["UseCoreAPI"]);

        #region Constants

        private const string ZTP_APP = "ztp";

        #endregion

        public SaasLogic(ISaasDataAccess saasDataAccess, ISaasRegistrationRepository saasRegistrationRepository, ILicenseLogic licenseLogic, ISfdcEntitlementLogic sfdcEntitlementLogic, ILicensingApiLogic iLicensingApiLogic)
        {
            _saasDataAccess = saasDataAccess;
            _saasRegistrationRepository = saasRegistrationRepository;
            _licenseLogic = licenseLogic;
            _sfdcEntitlementLogic = sfdcEntitlementLogic;
            _iLicensingApiLogic = iLicensingApiLogic;
        }

        /// <summary>
        /// Get all the available Saas Activations for current Support Account ID.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="supportAccountId">Current Support Account Id</param>
        /// <param name="authCode">Authorization Code if searching for one</param>
        /// <param name="authCode">Authorization Code if searching for one</param>
        /// 
        /// <returns>available Saas details</returns>
        public IEnumerable<AvailableSaaSActivations> GetAvailableSaasActivations(DataParameters parameters, long supportAccountId, string searchBy, string searchValue , long userAccountId, bool isAscAccount = false)
        {
            var loggingRegions = GetLoggingRegions().ToList();
            var availableSaas = _saasDataAccess.GetAvailableSaasActivations(supportAccountId);

            if (supportAccountId != 0)
            {
                var ascSalesforceAccountId = new List<string>();
                if (!isAscAccount)
                {
                    UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();
                    ascSalesforceAccountId = userAccountDataAccess.GetAscSalesForceAccountIdsByUserAccountId(userAccountId);
                }
               

                availableSaas = _sfdcEntitlementLogic.AddXdrEntitlements(availableSaas, new long[] { supportAccountId }, ascSalesforceAccountId);
                availableSaas = _sfdcEntitlementLogic.AddExpanseEntitlements(availableSaas, new long[] { supportAccountId }, null);
               
            }

            foreach (var saasActivation in availableSaas)
            {
                if(loggingRegions.Any() && !string.IsNullOrEmpty(saasActivation.Region))
                {
                    var loggingRegion = loggingRegions.FirstOrDefault(r => r.Name.Equals(saasActivation.Region.ToLower()));
                    saasActivation.Region = loggingRegion != null ? loggingRegion.DisplayName : saasActivation.Region;
                }
            }
            

            if (!string.IsNullOrWhiteSpace(searchBy) && !string.IsNullOrWhiteSpace(searchValue)) 
            {
                string searchVal = searchValue.ToLower();
                if (searchBy == "AuthCode")
                {
                    availableSaas = availableSaas.Where(x => x.AuthorizationCode.ToLower().Contains(searchVal) 
                        || x.DependendMasterRecords != null && x.DependendMasterRecords.Any(d => d.AuthorizationCode.ToLower().Contains(searchVal))
                        || x.Children != null && x.Children.Any(c => c.AuthorizationCode.ToLower().Contains(searchVal)));
                }
                else if (searchBy == "SerialNumber")
                {
                    availableSaas = availableSaas.Where(x => x.SerialNumber.ToLower().Contains(searchVal) 
                        || x.DependendMasterRecords != null && x.DependendMasterRecords.Any(d => d.SerialNumber.ToLower().Contains(searchVal)));
                }
                else if (searchBy == "AssociatedServicesorDevices")
                {
                    availableSaas = availableSaas.Where(x => x.AssoicatedDevices.ToLower().Contains(searchVal) 
                        || x.DependendMasterRecords != null && x.DependendMasterRecords.Any(d => d.AssoicatedDevices.ToLower().Contains(searchVal)));
                }
                else if (searchBy == "ModelName")
                {
                    availableSaas = availableSaas.Where(x => x.ModelName != null && x.ModelName.ToLower().Contains(searchVal) 
                        || x.DependendMasterRecords != null && x.DependendMasterRecords.Any(d => d.ModelName != null && d.ModelName.ToLower().Contains(searchVal))
                        || x.Children != null && x.Children.Any(d => d.ModelName != null && d.ModelName.ToLower().Contains(searchVal)));
                }
            }

            if (parameters != null)
            {
                var pageOffset = Convert.ToInt32(parameters.PaginationOffset);
                parameters.TotalRecordCount = availableSaas.Count();
                availableSaas = availableSaas.Skip(pageOffset).Take(parameters.PaginationCount);
            }

            // Flatten dependent master records
            var groupId = 0;
            var result = new List<AvailableSaaSActivations>();
            foreach (var record in availableSaas)
            {
                if (record.Children != null)
                {
                    if (record.DependendMasterRecords != null // move childer to last dependent master record
                        && record.DependendMasterRecords.Any())
                    {
                        record.DependendMasterRecords.Last().Children = record.Children;
                        record.Children = null;
                    }
                }
                record.GroupId = ++groupId;
                result.Add(record);
                if (record.DependendMasterRecords != null)
                {
                    var dependendMasterRecords = record.DependendMasterRecords.ToList();
                    dependendMasterRecords.ForEach(i => i.GroupId = record.GroupId);
                    result.AddRange(dependendMasterRecords);
                    record.DependendMasterRecords = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if the Authcode given is valid and activates with the Panorama Serial Number
        /// </summary>
        /// <param name="authCode">Authorization Code to be activated</param>
        /// <param name="supportAccountId">Current Support Account Id</param>
        /// <param name="userAccountId">Current User Account Id</param>
        /// <returns>model containing data whether activation was done or not</returns>
        public AddSaasAuthCodeResult AddSaasAuthCode(string authCode,string panoramaSerialNumber, long supportAccountId, long userAccountId,string loggingRegion, bool isPanoramaAssociation)
        {
            var result = new AddSaasAuthCodeResult("");

            var authcodeType = GetCloudServiceAuthCodeType(authCode);

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                return result;
            }

            if (coreAPIAllowedAuthCodeTypes.Split(',').Contains(authcodeType.Type) && useCoreAPI)
            {
                var activationResponse = ActivateSaasAuthCode(authCode, panoramaSerialNumber, supportAccountId, loggingRegion);
                if (activationResponse == null || activationResponse.Issuccess == false)
                {
                    string errInfo =
                        string.Format("Panorama Serial Number is {0}. AuthCode is {1}. Error Message from LicenseIt is {2}",
                                      panoramaSerialNumber, authCode, activationResponse.Error_message);
                    var oex = new Exception(errInfo);
                    Elmah.ErrorSignal.FromCurrentContext().Raise(oex);
                    return new AddSaasAuthCodeResult("Error Occured while processing your Request!");
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(activationResponse.Activation_link))
                    {
                        result.ActivationLink = activationResponse.Activation_link;
                    }
                    return result;
                }
            }
            else if (authcodeType.Type == SaasAuthCodeTypes.LoggingService && isPanoramaAssociation)
            {

                var activationResponse = AssociateCDLPanorama(authCode, panoramaSerialNumber, supportAccountId, loggingRegion);
                if (activationResponse == null || !activationResponse.IsSuccess)
                {
                    string errInfo =
                        string.Format("Panorama Serial Number is {0}. AuthCode is {1}. Error Message from LicenseIt is {2}",
                                      panoramaSerialNumber, authCode, activationResponse.ErrorMessage);
                    var oex = new Exception(errInfo);
                    Elmah.ErrorSignal.FromCurrentContext().Raise(oex);
                    return new AddSaasAuthCodeResult("Error Occured while processing your Request!");
                }
                else
                {
                    // Call Hub through Pan Services 
                    CallDevOpsApi(authCode, loggingRegion, "update");
                    if (activationResponse.IsSuccess)
                    {
                        result.ActivationLink = "";
                    }
                    return result;
                }

            }
            else
            {
                var stubType = authcodeType.Type == SaasAuthCodeTypes.Magnifier ? "MGFR" : "SAAS";


                bool isStubCreated = _licenseLogic.CreateStub(stubType, authCode, userAccountId, supportAccountId);

                if (isStubCreated)
                {
                    loggingRegion = string.IsNullOrEmpty(loggingRegion) ? "" : loggingRegion;
                    var resultArray = _licenseLogic.LicenseIt(panoramaSerialNumber, cpuid: loggingRegion, authorizationCode: authCode);

                    if (resultArray.Any(message => message.errmsg != null))
                    {
                        // Delete the device and all free subscription license
                        string errInfo =
                            string.Format("Panorama Serial Number is {0}. AuthCode is {1}. Error Message from LicenseIt is {2}",
                                          panoramaSerialNumber, authCode, resultArray.First().errmsg);
                        var oex = new Exception(errInfo);
                        Elmah.ErrorSignal.FromCurrentContext().Raise(oex);
                        return new AddSaasAuthCodeResult("Error Occured while processing your Request!");
                    }
                }
                else
                    return new AddSaasAuthCodeResult(string.Format("Panorama Serial Number is {0}. AuthCode is {1}. Error on Stub Creation!",
                        panoramaSerialNumber, authCode));
                return new AddSaasAuthCodeResult();
            }
        }

        /// <summary>
        /// Checks if the given Authorization code is valid one and can be registered
        /// </summary>
        /// <param name="authCode">Authorization code to be validated</param>
        /// <param name="supportAccountId">Current Account Id</param>
        /// <returns>Valid/Invalid flag for Authorization Code</returns>
        private AddSaasAuthCodeResult CanRegisterDevice(string authCode, long supportAccountId)
        {

            if (string.IsNullOrEmpty(authCode))
            {
                return new AddSaasAuthCodeResult(SaasAuthCodeMessages.EmptyAuthCode);
            }

            

            //Get details
            var authCodeDetails = _saasDataAccess.GetAuthCodeDetails(authCode);
            if (authCodeDetails == null)
            {
                return new AddSaasAuthCodeResult(SaasAuthCodeMessages.AuthCodeNotFound);
            }
            if (!_saasDataAccess.IsPanoramaAssociation(authCode))
            {
                //Is Auth Code Already Registered?
                if (_saasDataAccess.IsAuthCodeAlreadyRegistered(authCode))
                {
                    return new AddSaasAuthCodeResult(SaasAuthCodeMessages.AlreadyInUse);
                }
                //Is code already in use?
                if (authCodeDetails.IsActive == false || (authCodeDetails.DeviceId ?? 0) > 0
                        || authCodeDetails.RegistrationDate.HasValue || authCodeDetails.ExpirationDate.HasValue)
                {
                    return new AddSaasAuthCodeResult(SaasAuthCodeMessages.AuthCodeAlreadyAssociated);
                }
            }
            //No problems, allow registration
            return new AddSaasAuthCodeResult();
        }

        /// <summary>
        /// Get available Saas records for CSV Export
        /// </summary>
        /// <param name="supportAccountId">Current Support AccountID</param>
        /// <param name="authCode">Authorization Code in case of search</param>
        /// <returns>Array of Saas data in file format </returns>
        public byte[] GetAvailableSaasCsv(long supportAccountId, string searchBy,string searchValue , long userAccountId)
        {
            var dataParameters = new DataParameters();

            var data = GetAvailableSaasActivations(dataParameters, supportAccountId, searchBy, searchValue , userAccountId);

            var cc = new CsvContext();
            var outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                IgnoreUnknownColumns = true,
                EnforceCsvColumnAttribute = true,
                QuoteAllFields = true,
            };

            using (var ms = new MemoryStream())
            using (TextWriter tw = new StreamWriter(ms))
            {

                var saasCalc = Mapper.Map<AvailableSaasCalc[]>(data);
                var saasCsv = Mapper.Map<AvailableSaasCsv[]>(saasCalc);

                cc.Write(saasCsv, tw, outputFileDescription);

                tw.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Get the list of Panorama Serial Numbers for the support Account Id given
        /// </summary>
        /// <param name="supportAccountId">Current Support Account ID</param>
        /// <returns>List of Panorama serial numbers for the given support Account ID</returns>
        public IEnumerable<PanoramaSerialNumber> GetPanoramaSerialNumbers(long supportAccountId)
        {
            return _saasDataAccess.GetPanoramaSerialNumbers(supportAccountId);
        }

        public IEnumerable<PanoramaSerialNumber> GetPanoramaWithCloudService(long SupportAccountID, string mode, string serialNumber)
        {
            return _saasDataAccess.GetPanoramaWithCloudService(SupportAccountID, mode, serialNumber);
        }

        public List<PanoramaSerialNumber> GetPrimaryPanorama(long supportAccountId)
        {
            return _saasDataAccess.GetPrimaryPanorama(supportAccountId);
        }

        public IEnumerable<LoggingRegions> GetLoggingRegions()
        {
            var loggingRegions = (IEnumerable<LoggingRegions>)HttpRuntime.Cache.Get("LoggingRegions");
            if (loggingRegions == null)
            {
                //Fetch the CDL Logging Regions from Registry Service
                try
                {
                    var registryService = ConfigurationManager.AppSettings["RegistryService"];
                    var registryServiceCertThumbprint =
                        ConfigurationManager.AppSettings["RegistryServiceCertificateThumbprint"];
                    var cdlRegionsEndpoint = string.Format("{0}/{1}", registryService, @"v2/regions");

                    var cdlRegionsRequest = WebCommon.GetWebRequestWithClientCertificate(cdlRegionsEndpoint, "GET",
                        registryServiceCertThumbprint, string.Empty);

                    using (var cdlRegionsResponse = cdlRegionsRequest.GetResponse())
                    {
                        HttpStatusCode statusCode = ((HttpWebResponse) cdlRegionsResponse).StatusCode;
                        if (statusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(cdlRegionsResponse.GetResponseStream()))
                            {
                                //TODO: This Immediate Encoding and Decoding is done for checkmarx purpose .It doesn't serve any purpose.
                                string encodedString = HttpUtility.HtmlEncode(reader.ReadToEnd());
                                string decodedString = HttpUtility.HtmlDecode(encodedString);
                                var response = JsonConvert.DeserializeObject<List<LoggingRegions>>(decodedString,
                                    new JsonSerializerSettings()
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        MissingMemberHandling = MissingMemberHandling.Ignore
                                    });
                                loggingRegions = response.Where(lg => lg.Status.Equals(LoggingRegionStatus.ACTIVE.ToString())).ToList();
                                HttpRuntime.Cache.Insert("LoggingRegions", loggingRegions, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }

            return loggingRegions;
        }

        public bool UpdateSaasOTP(long saasOTPId, string updatedBy, string OTP, bool isActive)
        {
            return _saasRegistrationRepository.UpdateSaasOTP(saasOTPId,updatedBy,OTP,isActive);
        }
        public long InsertSaasOTP(string serialNumber, long supportAccountId, string lookUp, string createdBy)
        {
            return _saasRegistrationRepository.InsertSaasOTP(serialNumber, supportAccountId, lookUp, createdBy);
        }
        public OTPOutput GetSaasOTP(long? supportAccountID, string serialNumber, OtpType otpType = OtpType.LoggingService)
        {
            OTPOutput otpOp = new OTPOutput();
            var otp = _saasDataAccess.GetSaasOtpByType(supportAccountID ?? 0, serialNumber, otpType);

            if (otp != null)
            {
                var expirationInMins = otpType.Equals(OtpType.LoggingService) ? 10 : 60;
                var expiryDate = otp.ExpiryDate != null ? DateTime.Parse(otp.ExpiryDate) : new DateTime();
                TimeSpan ts = DateTime.Now.Subtract(expiryDate);
                if (ts.TotalMinutes <= expirationInMins && otp.OneTimePassword != null && otp.OneTimePassword != "EXPIRED!")
                {
                    otpOp.OneTimePassword = otp.OneTimePassword;
                    otpOp.ExpiryDate = expiryDate.AddMinutes(expirationInMins).ToString();
                    return otpOp;
                }
            }

            return otpOp;
        }

        public long GetPanoramaTenantId(long supId, string serialNumber, AppPortalAppType appType = AppPortalAppType.LoggingService)
        {
            long tenantId = _saasDataAccess.GetPanoramaTenantByAppType(supId, serialNumber, appType);
			return tenantId;
        }

        public bool? HasLcaasLicense(string authCode, string panoramaSerialNumber)
        {
            if(authCode == "Transfer")
                return _saasRegistrationRepository.HasLcaasLicense(panoramaSerialNumber); 
            var authcodeType = _saasRegistrationRepository.GetAuthcodeType(authCode);
            try
            {
                if (authcodeType.ToUpper() != SaasPartSubType.LoggingService)
                    return _saasRegistrationRepository.HasLcaasLicense(panoramaSerialNumber);
                else
                    return true;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return null;
        }

        public bool CheckIfPreviouslyActivated(string authCode, string panoramaSerialNumber)
        {
           return _saasRegistrationRepository.CheckIfPreviouslyActivated(authCode, panoramaSerialNumber);
        }
        public bool VerifyAuthorizationBySupportAccount(long supportAccountId, string email)
        {
            if(_saasRegistrationRepository.IsTacUserOrAdmin(supportAccountId,email))
                return true;
            else
                return _saasDataAccess.VerifyAuthorizationBySerialNumberOrSupportAccountId(email, string.Empty, supportAccountId);
        }

        public bool VerifyAuthorizationBySerailNumber(string serialNumber, string email)
        {
            return _saasDataAccess.VerifyAuthorizationBySerialNumberOrSupportAccountId(email, serialNumber, 0);
        }

        public AuthCodeType GetCloudServiceAuthCodeType(string authcode)
        {

            var authcodeType = new AuthCodeType();
            var type = _saasRegistrationRepository.GetAuthcodeType(authcode);
            switch (type.ToUpper())
            {
                case SaasPartSubType.LoggingService:    authcodeType.Type = SaasAuthCodeTypes.LoggingService;                                
                                                        authcodeType.IsPanoramaAssociation = _saasDataAccess.IsPanoramaAssociation(authcode);
                                                        authcodeType.Region = _saasDataAccess.GetLoggingServiceRegion(authcode);
                                                        break;
                case SaasPartSubType.Magnifier: authcodeType.Type = SaasAuthCodeTypes.Magnifier;
                                                        break;
                case SaasPartSubType.MagnifierBeta: authcodeType.Type = SaasAuthCodeTypes.MagnifierBeta;
                                                        break;
                case SaasPartSubType.GPCSMobileUsers:
                case SaasPartSubType.GPCSCleanPipe:
                case SaasPartSubType.GPCSRemoteNetworks: authcodeType.Type = SaasAuthCodeTypes.Others;
                                                        break;
                default: authcodeType.Type = string.Empty;
                                                         break;
            }

            return authcodeType;
        }

        public IEnumerable<LoggingServiceDevice> GetLoggingServiceDevices(long supportAccountId)
        {
            return _saasRegistrationRepository.GetLoggingServiceDevices(supportAccountId);
        }

        public bool HasBetaAccess(long supportAccountId)
        {
            return _saasRegistrationRepository.HasSaasBetaAccess(supportAccountId);
        }
        public bool IsHAPanorama(string serialNumber)
        {
            return _saasDataAccess.IsHAPanorama(serialNumber);
        }
        public string GetFreeLoggingService(string region, long supportAccountId)
        {
            return _saasDataAccess.GetFreeLoggingService(region, supportAccountId);
        }

        private CoreAPIResponse ActivateSaasAuthCode(string authCode, string serialNumber, long supportAccountId, string region)
        {
            try
            {
                string URL = coreAPIURL;
                CoreAPIRequest corerequest = new CoreAPIRequest()
                {
                    Auth_code = authCode,
                    Associated_SN = serialNumber,
                    Support_account_id = supportAccountId,
                    Region = region,
                    User_email = HttpContext.Current.User.Identity.Name
                };
                string requestData = JsonConvert.SerializeObject(corerequest);
               
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = requestData.Length;

                request.Headers["Authorization"] = string.Format("Basic {0}",Convert.ToBase64String(Encoding.Default.GetBytes(string.Format("{0}:{1}",coreAPIuser,coreAPIkey))));

								// Comments By LK On 06/25/2020
								// Same PanServices cert is used for all licensing services
								var panServiceCertThumbprint = ConfigurationManager.AppSettings["PanServicesCertThumbprint"];

								if (!string.IsNullOrEmpty(panServiceCertThumbprint)) {
									ServicePointManager.Expect100Continue = true;
									ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
									ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
									                                       | SecurityProtocolType.Tls11
									                                       | SecurityProtocolType.Tls12
									                                       | SecurityProtocolType.Ssl3;

									var clientCertificate = CertificateManager.GetCertificateByCnName(panServiceCertThumbprint);

									if (clientCertificate == null) {
										Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Empty certificate store"));
									} else {
										request.ClientCertificates.Add(clientCertificate);
									}
								}
								
								StreamWriter requestWriter = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
                requestWriter.Write(requestData);
                requestWriter.Close();
         
                WebResponse webResponse = request.GetResponse();
                Stream webStream = webResponse.GetResponseStream();
                StreamReader responseReader = new StreamReader(webStream);
                string response = responseReader.ReadToEnd();
                CoreAPIResponse activationResponse = JsonConvert.DeserializeObject<CoreAPIResponse>(response, new JsonSerializerSettings()
                {
                    MaxDepth = 32,
                    TypeNameHandling = TypeNameHandling.None
                });
                responseReader.Close();
                return activationResponse;
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new CoreAPIResponse()
                {
                    Issuccess = false, Error_message="Error Occured on CoreAPI Call"
                };
            }
            
        }
        private LicenseCoreAPIResponse AssociateCDLPanorama(string authCode, string panormaSerialNumber, long supportAccountId, string region)
        {
            
            string URL = "";
            try
            {
                LicenseCoreAPIRequest corerequest = new LicenseCoreAPIRequest();
               
                    var tenantId = GetCDLTenantId(authCode, supportAccountId, region);
                    URL = string.Format("{0}/coresvc/v1/panorama/mapCdl", licenseCoreApiURL);
                    corerequest = new LicenseCoreAPIRequest()
                    {

                        PanoramaSerialNumber = string.IsNullOrEmpty(panormaSerialNumber) ? "" : panormaSerialNumber,
                        SupportAccountId = supportAccountId,
                        CDLTenantId = tenantId,
                        IsEulaAccepted = true,
                        UserEmail = HttpContext.Current.User.Identity.Name
                    };

                

                string requestData = JsonConvert.SerializeObject(corerequest, Formatting.Indented, new JsonSerializerSettings
                { NullValueHandling = NullValueHandling.Ignore });
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = requestData.Length;
                if (!string.IsNullOrEmpty(licenseCoreApiCertThumbprint))
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                           | SecurityProtocolType.Tls11
                                                           | SecurityProtocolType.Tls12
                                                           | SecurityProtocolType.Ssl3;

                    var clientCertificate = CertificateManager.GetCertificateByCnName(licenseCoreApiCertThumbprint);

                    if (clientCertificate == null)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Empty certificate store"));
                    }
                    else
                    {
                        request.ClientCertificates.Add(clientCertificate);
                    }
                }

                StreamWriter requestWriter = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
                requestWriter.Write(requestData);
                requestWriter.Close();

                WebResponse webResponse = request.GetResponse();
                Stream webStream = webResponse.GetResponseStream();
                StreamReader responseReader = new StreamReader(webStream);
                string response = responseReader.ReadToEnd();
                HttpStatusCode statusCode = ((HttpWebResponse)webResponse).StatusCode;
                LicenseCoreAPIResponse activationResponse = new LicenseCoreAPIResponse();
                if (response == "" && statusCode == HttpStatusCode.OK)
                {
                    activationResponse.IsSuccess = true;

                }
                else
                {
                    activationResponse = JsonConvert.DeserializeObject<LicenseCoreAPIResponse>(response, new JsonSerializerSettings()
                    {
                        MaxDepth = 32,
                        TypeNameHandling = TypeNameHandling.None
                    });
                }
                responseReader.Close();
                return activationResponse;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new LicenseCoreAPIResponse()
                {
                    IsSuccess = false,
                    ErrorMessage = "Error Occured on LicenseApi Call"
                };
            }

        }
        private string BuildRequestData(string authcode, string serialNumber,long supportAccountId, string region)
        {
            string user = HttpContext.Current.User.Identity.Name;
            serialNumber = serialNumber ?? string.Empty;
            region = region ?? string.Empty;
            string data = string.Format("{{\"auth_code\":\"{0}\", \"support_account_id\":{1}, \"user_email\":\"{2}\", \"associated_SN\": \"{3}\", \"region\": \"{4}\"}}", authcode, supportAccountId.ToString(), user, serialNumber, region);
            return data;
        }

        public bool IsPanoramaAssociation(string authcode)
        {
           return _saasDataAccess.IsPanoramaAssociation(authcode);
        }

        public bool IsEligibleForCdlPanoramaAssociation(string authcode)
        {
            return _saasDataAccess.IsEligibleForCdlPanoramaAssociation(authcode);
        }

        public EActivationActionType IsProductEligibleForActivation(string authcode, out string message)
        {
            return _saasDataAccess.IsProductEligibleForActivation(authcode, out message);
        }

        public bool IsSeAuthCode(string authcode)
        {
            return _saasDataAccess.IsSeAuthCode(authcode); 
        }

        public bool IsCdlAuthCode(string authcode)
        {
            return _saasDataAccess.IsCdlAuthCode(authcode);
        }

        public long GenerateTenantId(int retryLimit, bool isLoggingService)
        {
            var tenantId = _saasDataAccess.GenerateTenantId(retryLimit, isLoggingService);

            return tenantId;
        }

        public void CreateZtpTenantForPanorama(long supportAccountId, string panoramaSerialNumber, string userAccount)
        {
            long tenantId = GetPanoramaTenantId(supportAccountId, panoramaSerialNumber, AppPortalAppType.ZtpService);

            if (tenantId == 0)
            {
                tenantId = GenerateTenantId(3, false);
                _saasDataAccess.CreateTenantForPanorama(supportAccountId, tenantId, panoramaSerialNumber, ZTP_APP, userAccount);
            }
        }

        public List<NameValueOnly> GetDevicesForOtp(long supportAccountId, long userAccountId, OtpDeviceType deviceType)
        {
            return _saasDataAccess.GetDevicesForOtp(supportAccountId, userAccountId, deviceType);
        }

        public IEnumerable<PanoramaSerialNumber> GetPanoramaWithCloudServiceForTransfer(long supportAccountID, string serialNumber)
        {
            return _saasDataAccess.GetPanoramaWithCloudServiceForTransfer(supportAccountID, serialNumber);
        }

        public EasyOnboardingAPIResponse GetCdlMagicLink(string authCode)
        {
            HttpWebRequest request = null;

            // Use support account id 245 to get the License API Key for all support accounts.
            long supportAccountId = ApiConstants.PANW_SUPPORT_ACCOUNT_ID;
            string panServicesUserName = ConfigurationManager.AppSettings["PanServicesUserName"];
            string panServicesUserKey = ConfigurationManager.AppSettings["PanServicesUserKey"];

            try
            {
                if (!string.IsNullOrEmpty(authCode))
                {
                    var panServiceCertThumbprint = ConfigurationManager.AppSettings["PanServicesCertThumbprint"];

                    // If there's no PanServicesRestUrl key in the secrets file then use PanServicesUrl key
                    string restUrl = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["PanServicesRestUrl"])
                        ? ConfigurationManager.AppSettings["PanServicesRestUrl"]
                        : ConfigurationManager.AppSettings["PanServicesUrl"];
                    string url = restUrl + "/api/EasyOnboarding/GetMagicLinkByAuthCode?authcode=" + authCode.Trim();
                    request = WebCommon.GetWebRequestWithClientCertificate(url, "GET", panServiceCertThumbprint, null,
                        username: panServicesUserName, password: panServicesUserKey);
                    request.Headers["LicenseAPIKey"] = _iLicensingApiLogic.GetLicensingApiKey(supportAccountId).LicensingApiKey;

                    EasyOnboardingAPIResponse activationResponse = null;
                    using (WebResponse webResponse = request.GetResponse())
                    {
                        using (Stream webStream = webResponse.GetResponseStream())
                        {
                            using (StreamReader responseReader = new StreamReader(webStream))
                            {
                                string response = responseReader.ReadToEnd();
                                var generatedcsResponce = JsonConvert.DeserializeObject(response);
                                activationResponse = JsonConvert.DeserializeObject<EasyOnboardingAPIResponse>(response,
                                    new JsonSerializerSettings()
                                    {
                                        MaxDepth = 32,
                                        TypeNameHandling = TypeNameHandling.None
                                    });
                            }
                        }
                    }

                    return activationResponse;
                }
            }
            catch (Exception ex)
            {
                //If the API returns 404 that means the authcode doesn't have a magic link 
                if (ex.Message.Contains("404"))
                    return new EasyOnboardingAPIResponse();

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new EasyOnboardingAPIResponse()
                {
                    Issuccess = false,
                    Error_message = "Error Occured on GetMagicLinkByAuthCode API Call"
                };
            }

            return null;
        }
      

        public string GetCDLTenantId(string serialNumber, long supportAccountID, string region)
        {
            return _saasDataAccess.GetCDLTenantId(serialNumber, supportAccountID, region);
        }
    }

    /// <summary>
    /// Contains all the messages necessary for Saas
    /// </summary>
    public static class SaasAuthCodeMessages
    {
        public const string Success = "Cloud Services Auth Code registered successfully!";
        public const string EmptyAuthCode = "Auth Code cannot be empty";
        public const string AlreadyInUse = "This Auth Code is already associated with another user";
        public const string AuthCodeNotFound = "The Auth Code could not be found";
        public const string AuthCodeAlreadyAssociated = "This Auth Code is already associated with another user";
        public const string NotAVmDevice = "The Auth Code is invalid";
        public const string PanoramaAlreadyActivated = "Panorama is already activated with this type of license!";
        public const string RegionNotSelected = "Please select region for Cortex Data Lake Activation!";
        public const string LoggingServiceNotSelected = "Please select Cortex Data Lake for Cortex XDR - Analytics Activation!";
        public const string PanoramaNotSelected = "Please select Panorama to activate a Cloud Service!";
        public static readonly string MagniferRedirect = "This product cannot be activated here. Please go to the <a class='k-link'  style='color:#00acdc !important' target='_blank' href='" + ConfigurationManager.AppSettings["AppPortalURL"] + "'>hub</a> to continue with the activation. ";
        public static readonly string MagniferRedirectSe = "This auth code can only be activated on the SE support account that it was created for.";
        public static readonly string MagniferRedirectNonSe = "You do not have permission to activate this auth code on this support account. This auth code can only be activated by the SE that it was created for.";
        public const string LogQuotaNotAccepted = "Please acknowledge configuration of Cortex Data Lake Quota for firewall Logs!";
        public const string AuthCodeActivatedInAppPortal = "AuthCode {0} has been activated in AppPortal and cannot be processed in CSP.";
        public const string ExternalActivationLink = "AuthCode {0} can only be activated in <a href='{1}' target='_blank'>Easy Onboarding</a>.";
        public const string CDLAuthCodeWithNoMagicLink = "The ability to activate Cortex Data Lake AUTH CODES in the Cloud Services page is no longer valid.";
        public const string CDLAuthCodeWithMagicLink = "The ability to activate Cortex Data Lake AUTH CODES in the Cloud Services page is no longer valid, please click on the easy activation link below:<br> <a href = '{0}' target='_blank'>Activation Link</a>.";

    }

    public static class SaasAuthCodeTypes
    {
        public const string Magnifier = "Magnifier";
        public const string MagnifierBeta = "MagnifierBeta";
        public const string LoggingService = "LoggingService";
        public const string Others = "Others";
    }
    public static class SaasPartSubType
    {
        public const string LoggingService = "LCAAS";
        public const string Magnifier = "MGFR";
        public const string GPCSRemoteNetworks = "FWAAS";
        public const string GPCSMobileUsers = "GPAAS";
        public const string MagnifierBeta = "MGFRBETA";
        public const string GPCSCleanPipe = "CPAAS";
    }
}
