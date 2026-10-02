using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Security;
using LINQtoCSV;
using Csp.Api.HelperClasses;

namespace Csp.Api.Logic
{
    public class EpLogic : IEPLogic
    {
        private readonly IEPDataAccessLogic _epDbLogic;
        private readonly ILicenseLogic _licenseLogic;
        private readonly IDeviceLogic _deviceLogic;
		private readonly IEpRepository _epRepository;
		private readonly ICurrentStateService _currentStateService;
        private readonly IConnectionStrings _connectionStrings;

		public EpLogic(IEPDataAccessLogic epLogic, ILicenseLogic licenseLogic, IDeviceLogic deviceLogic, IEpRepository epRepository, 
            ICurrentStateService currentStateService, IConnectionStrings connectionStrings)
        {
            _epDbLogic = epLogic;
            _licenseLogic = licenseLogic;
            _deviceLogic = deviceLogic;
			_epRepository = epRepository;
			_currentStateService = currentStateService;
            _connectionStrings = connectionStrings;
        }


        //Generate Ep License
                public EpLicenseGenerationResult GenerateLicense(DateTime? expirationDate, string customerName, int? vdiLicenseCount, int? clientLicenseCount, int? serverLicenseCount, int? agentLicenseCount, string wildfireApiKey,string serialNumber,
                                    bool includeReflector = false, string licenseApiKey = null)
        {
            var result = new EpLicenseGenerationResult();
            try
            {
                string epLicUrl = ConfigurationManager.AppSettings["EndPointLicensingUrl"];
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(epLicUrl);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                EndPointLicense licPayLoad;

                if (string.IsNullOrEmpty(licenseApiKey))
                    licPayLoad = new EndPointLicense();
                else
                    licPayLoad = new MalborkLicense
                    {
                        LicenseApiKey = licenseApiKey
                    };

                string wildfireAddress = ConfigurationManager.AppSettings["WildfireApplicationUrl"];

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    licPayLoad.CustomerName = ReplaceSpecialChars(customerName);
                    licPayLoad.IsWildFire = true;
                    licPayLoad.WildFireApiKey = wildfireApiKey;
                    licPayLoad.WildFireAddress = wildfireAddress;
                    licPayLoad.Reflector = includeReflector;

                    if(agentLicenseCount.HasValue && agentLicenseCount.Value > 0)
                    {
                        licPayLoad.AgentPoolAmount = agentLicenseCount.Value;
                        licPayLoad.SerialNumber = serialNumber;
                        licPayLoad.LicenseVersion = 2;         
                        if (!string.IsNullOrEmpty(licenseApiKey))
                            licPayLoad.LicenseVersion = 3;
                    }
                    else
                    {
                        licPayLoad.LicenseVersion = 1;
                        if (clientLicenseCount.HasValue)
                        {
                            licPayLoad.AgentPoolAmount = clientLicenseCount.Value;
                        }
					    if (vdiLicenseCount.HasValue) {
						    licPayLoad.VdiPoolAmount = vdiLicenseCount.Value;
					    }
                        if (serverLicenseCount.HasValue)
                        {
                            licPayLoad.ServerPoolAmount = serverLicenseCount.Value;
                        }
                    }
                    
                    if (expirationDate.HasValue)
                    {
                        string expDate = string.Format("{0}-{1}-{2}", expirationDate.Value.Year,
                                                       expirationDate.Value.Month, expirationDate.Value.Day);
                        licPayLoad.ExpirationDate = expDate;
                    }
                    var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string json = jsonSerializer.Serialize(licPayLoad);
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var jsonOp = EscapeStringValue(streamReader.ReadToEnd());
                        var licResponse = jsonSerializer.Deserialize<EndPointLicenseResponse>(jsonOp);
                        if (licResponse.Succeed == true)
                        {
                            result.HasError = false;
                            result.LicenseKey = licResponse.Message;
                        }
                        else
                        {
                            result.HasError = true;
                            result.ErrorMessage = "Error occurred while generating License Key";
                            if (!string.IsNullOrEmpty(licResponse.AdditionalInfo))
                            {
                                result.ErrorMessage = licResponse.AdditionalInfo;
                            }
                            else if (!string.IsNullOrEmpty(licResponse.ValidationInfo))
                            {
                                result.ErrorMessage = licResponse.ValidationInfo;
                            }
                            string msg = String.Format("Cannot get license from Cyvera Licensing Service! Customer is {0}. SerialNumber is {1}", customerName, serialNumber);
                            Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(msg));
                        }
                    }
                }
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(oex);
                result.ErrorMessage = "Error occurred while generating License Key";
                result.HasError = true;
            }

            return result;
        }

        private string ReplaceSpecialChars(string s)
        {
            string regexExpression = ConfigurationManager.AppSettings["RegexForCyvera"];
            return Regex.Replace(s, regexExpression, "-");
        }

        //Used to escape license json string
        static string EscapeStringValue(string value)
        {
            return value.Replace(@"\", "\\");
        }

        //Adds ESM Auth Code
        public AddEpAuthCodeResult AddEpAuthCode(string authCode, long supportAccountId, long userAccountId, string email="")
        {
            long deviceId = 0;
            string newSerialNum;
            string licType;
            string partId = string.Empty;
            DateTime? dtShipDate = null;
            try
            {
                _epDbLogic.GetInfoToCreateStub(authCode, out licType, out partId, out newSerialNum, out dtShipDate);

								if (string.IsNullOrEmpty(partId))
                {
                    return new AddEpAuthCodeResult(string.Format("Could not find the EndPoint Stub for {0}", authCode));
                }

                /*
                 * Per Pam, we dont need this!
                if (!string.IsNullOrEmpty(licType))
                {
                    if (licType == "TRAPSMBETA")
                    {
                        if (!IsBetaCustomer(email))
                        {
                            return new AddEpAuthCodeResult(string.Format("Invalid Traps Authcode {0}", authCode));
                        }
                    }
                }
                 */

                _deviceLogic.PushToDeviceMaster(newSerialNum, Convert.ToInt32(userAccountId), partId,dtShipDate);
                _epDbLogic.UpdateAccountIdBySerialNumber(newSerialNum, userAccountId);
                _epDbLogic.UpdateSupportAccountIdBySerialNumber(newSerialNum, supportAccountId, userAccountId);
                //_epDbLogic.UpdateDeviceRegistrationDate(newSerialNum,System.DateTime.Now);
                deviceId = _epDbLogic.GetDeviceIdBySerialNumber(newSerialNum);
                // using the deviceId, call LicenseIt
                var resultArray = _licenseLogic.LicenseIt(newSerialNum, authorizationCode: authCode);
                if (resultArray.Any(message => message.errmsg != null))
                {
                    // Delete the device and all free subscription license
                    string errInfo =
                        string.Format("Serial Number is {0}. AuthCode is {1}. Error Message from LicenseIt is {2}",
                                      newSerialNum, authCode, resultArray.First().errmsg);
                    var oex = new Exception(errInfo);
                    Elmah.ErrorSignal.FromCurrentContext().Raise(oex);
                    _epDbLogic.RollBackPushToDeviceMasterForTraps(deviceId, authCode);
                    return new AddEpAuthCodeResult(resultArray.First().errmsg);
                }
                //_epDbLogic.SaveStubDeviceIdToLicense(authCode, deviceId, supportAccountId);
                return new AddEpAuthCodeResult(){DeviceId = deviceId};
            }
            catch (Exception oex)
            {
                if (deviceId > 0)
                {
                    _epDbLogic.RollBackPushToDeviceMasterForTraps(deviceId, authCode);
                }
                Elmah.ErrorSignal.FromCurrentContext().Raise(oex);
                return new AddEpAuthCodeResult("Error: An error occurred while adding authcode.");
            }
        }

        // Check if the Auth Code is ESM.
        public bool IsAuthCodeEsm(string authCode)
        {
					return _epDbLogic.IsEpLicense(authCode, EndpointConstants.Esm);
        }

        // Check if the Auth Code is Ep100
        public bool IsAuthCodeEp100(string authCode)
        {
					return _epDbLogic.IsEpLicense(authCode, EndpointConstants.Workstation);
        }

        // Check if the Auth Code is Ep500
        public bool IsAuthCodeEp500(string authCode)
        {
					return _epDbLogic.IsEpLicense(authCode, EndpointConstants.Server);
        }

        // Check if Auth code is already registered.
        public bool IsAuthCodeAlreadyRegistered(string authCode, string epType)
        {
            return _epDbLogic.IsAuthCodeAlreadyRegistered(authCode, epType);
        }
        
        //Validate ESM Auth Code. Actually validates if the authcode is esm or lab or nfr or eval
        public EpValidationResult ValidateEsmAuthCode(string authCode, long supportId)
        {
            var result = new EpValidationResult();
            var isAuthCodeInUse = _epDbLogic.IsAuthCodeAlreadyRegistered(authCode, supportId);

            if (isAuthCodeInUse)
            {
                result.HasError = true;
                result.ErrorMessage = "Authcode entered is already in use";
                return result;
            }
            //string licType = _epDbLogic.GetEndPointLicenseType(authCode);

            if (!_epDbLogic.IsEpLicenseEsmOrBundle(authCode))
            {
                result.HasError = true;
                    result.ErrorMessage =
                           "Sorry, you must enter a Valid Endpoint AuthCode";
                    return result;
            }
            return result;
        }

        // Validates Auth code that will be added to ESM
        public EpValidationResult ValidateAuthCode(string authCode, long supportId, long deviceId)
        {
            var result = new EpValidationResult();
            var isAuthCodeInUse = _epDbLogic.IsAuthCodeAlreadyRegistered(authCode, supportId);

            if (isAuthCodeInUse)
            {
                result.HasError = true;
                result.ErrorMessage = "Authcode entered is already in use";
                return result;
            }

            var isEpAuthCode = _epDbLogic.IsAuthCodeEp(authCode);
            if (!isEpAuthCode)
            {
                result.HasError = true;
                result.ErrorMessage = "Authcode entered is not a valid Endpoint Authcode";
                return result;
            }

            // Get the type of Auth Code
            var epType = _epDbLogic.GetEndPointLicenseType(authCode);

						var deviceHasSupport = _epDbLogic.EndPointHasSupport(deviceId);

						if (deviceHasSupport) {

							var deviceHasBKLNSupport = _epDbLogic.EndPointHasBKLNSupport(deviceId);

							var licenseHasBKLNSupport = _epDbLogic.EndPointHasBKLNSupport(authCode);

							if (deviceHasBKLNSupport && !licenseHasBKLNSupport) {
								result.HasError = true;
								result.ErrorMessage = "Authcode entered is not a Premium Partner Support AuthCode";
								return result;
							}

							if (!deviceHasBKLNSupport && licenseHasBKLNSupport) {
								result.HasError = true;
								result.ErrorMessage = "Authcode entered cannot not be a Premium Partner Support AuthCode.";
								return result;
							}

						}

						if (epType == EndpointConstants.Workstation)
            {
							if (_epDbLogic.IsEpLicenseTypeAlreadyRegistered(supportId, EndpointConstants.Workstation, deviceId)) // End point 100
                {
                    result.HasError = true;
                    result.ErrorMessage = "You already have Endpoint workstation license registered";
                    return result;
                }
            } else if (epType == EndpointConstants.Server)
            {

							if (_epDbLogic.IsEpLicenseTypeAlreadyRegistered(supportId, EndpointConstants.Server, deviceId)) // End point 500
                {
                    result.HasError = true;
                    result.ErrorMessage = "You already have Endpoint server license registered";
                    return result;
                }

            } else if (epType == EndpointConstants.Support)
            {
							if (_epDbLogic.IsEpLicenseTypeAlreadyRegistered(supportId, EndpointConstants.Support, deviceId)) // End point Svc
                {
                    result.HasError = true;
                    result.ErrorMessage = "This License Type already exists";
                    return result;
                }
            } else if (epType == EndpointConstants.Warranty) {
							if (_epDbLogic.IsEpLicenseTypeAlreadyRegistered(supportId, EndpointConstants.Warranty, deviceId)) // End point Svc
                {
								result.HasError = true;
								result.ErrorMessage = "This License Type already exists";
								return result;
							}
						} else if (epType == EndpointConstants.Vdi) {
							if (_epDbLogic.IsEpLicenseTypeAlreadyRegistered(supportId, EndpointConstants.Vdi, deviceId)) // End point Vdi
                {
								result.HasError = true;
								result.ErrorMessage = "This License Type already exists";
								return result;
							}
						}
            else
            {
                result.HasError = true;
                result.ErrorMessage = "Cannot add this Authcode to Security Manager";
                return result;
            }
            return result;
        }

        //Add auth code to esm
        public AddEpAuthCodeResult AddEndpointLicense(string authCode, string esmAuthCode, long supportAccountId, long userAccountId)
        {
            bool hasLicensingError = false;
            var isAuthCodeInUse = _epDbLogic.IsAuthCodeAlreadyRegistered(authCode, supportAccountId);
            string serialNumber;
            if (isAuthCodeInUse)
            {
                return new AddEpAuthCodeResult("Authcode entered is already in use");
            }
            // Get the Stub Id of the esmAuthCode
            DateTime? licExpiryDate = _epDbLogic.GetExpiryDateForEpLicenseAuthCode(authCode);
            long stubId = _epDbLogic.GetStubDeviceId(supportAccountId, esmAuthCode, out serialNumber);
            // Check if license type is already associated with this stub.
            List<string> epTypes = _epDbLogic.GetEndPointLicenseTypes(stubId);
            string authCodeType = _epDbLogic.GetEndPointLicenseType(authCode);

            if (epTypes.Contains(authCodeType))
            {
                return new AddEpAuthCodeResult("This License Type is already added.");
            }
            string serialNum = _epDbLogic.GetSerialNumberByDeviceId(stubId);
            try
            {

                var resultArray = _licenseLogic.LicenseIt(serialNum, authorizationCode: authCode);
                if (resultArray.Any(message => message.errmsg != null))
                {
                    hasLicensingError = true;
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error from LicenseIt while Licensing Ep Svc: " + resultArray.First().errmsg));
                }
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error from LicenseIt while Licensing Ep Svc: " + oex.Message));
            }
            if (!hasLicensingError && _epDbLogic.SaveStubDeviceIdToLicense(authCode, stubId, supportAccountId))
            {
                return new AddEpAuthCodeResult(){DeviceId = stubId};
            }
            else
            {
                return new AddEpAuthCodeResult("Could not add license to ESM");
            }
        }



        public long GetStubDeviceId(long supportAccountId, string authCode, out string serialNumber)
        {
            return _epDbLogic.GetStubDeviceId(supportAccountId, authCode, out serialNumber);
        }


        public DateTime? GetLicenseExpirationDateByEpStubId(long deviceId)
        {
            return _epDbLogic.GetLicenseExpirationDateByEpStubId(deviceId);
        }


        public long? GetLicenseCountByEpStubId(long deviceId)
        {
            return _epDbLogic.GetLicenseCountByEpStubId(deviceId);
        }


        public long? GetClientLicenseCountByEpStubId(long deviceId)
        {
            return _epDbLogic.GetClientLicenseCountByEpStubId(deviceId);
        }

        public long? GetServerLicenseCountByEpStubId(long deviceId)
        {
            return _epDbLogic.GetServerLicenseCountByEpStubId(deviceId);
        }

				public long? GetVdiLicenseCountByEpStubId(long deviceId) {
					return _epDbLogic.GetVdiLicenseCountByEpStubId(deviceId);
				}

			/*
        public AvailableEp GetEndPointInfo(string authCode)
        {
            return _epDbLogic.GetEndPointInfo(authCode);
        }
			*/

                public long? GetEpDeviceInfo(string authCode, out string serialNumber, out string deviceName, out string panDBIdentification)
        {
            return _epDbLogic.GetEpDeviceInfo(authCode, out serialNumber, out deviceName,out panDBIdentification);
        }
        
        public byte[] GetAvailableEpsCsv(long supportAccountId, string searchField, string searchValue) {
			var dataParameters = new DataParameters { SearchField = searchField, SearchValue = searchValue };

			var data = GetAvailableEps(dataParameters, supportAccountId, searchField, searchValue, shouldGroupAuthCodes: false);

			var cc = new CsvContext();
			var outputFileDescription = new CsvFileDescription {
				SeparatorChar = ',',
				FirstLineHasColumnNames = true,
				QuoteAllFields = true,
			};

			using (var ms = new MemoryStream())
			using (TextWriter tw = new StreamWriter(ms)) {

				var trapsCsv = Mapper.Map<AvailableEpCsv[]>(data);

				cc.Write(trapsCsv, tw, outputFileDescription);

				tw.Flush();
				ms.Position = 0;

				return ms.ToArray();
			}
		}




        public List<EpLicenses> GetEpLicenses(long deviceId)
        {
            return _epDbLogic.GetEpLicenses(deviceId);
        }


        public IEnumerable<AvailableEp> GetAvailableEps(DataParameters parameters, long supportAccountId, string searchField, string searchValue, bool shouldGroupAuthCodes, bool isDebug = false)
        {
			IEnumerable<AvailableEp> epData;

			parameters.SearchField = searchField;
			parameters.SearchValue = searchValue;

			/* EE code
            //This gets all the roles of the user along with support account and role
			var rolesSecurity = _currentStateService.GetCurrentUser().RoleSecurities;

			epData = _epRepository.GetAvailableEps(parameters, supportAccountId, rolesSecurity);
            */

            //performance improvement: Stored Procedure code
            var userAccountId = _currentStateService.GetCurrentUser().UserAccount.Id;
            epData = _epRepository.GetAvailableEps(Secrets.CspDatabase, parameters, userAccountId, supportAccountId);
	        return shouldGroupAuthCodes ? GroupEpLicenses(epData) : epData;
        }

		private IEnumerable<AvailableEp> GroupEpLicenses(IEnumerable<AvailableEp> epLicenses) {
			var groupEpLicenses = new List<AvailableEp>();
			foreach (var epLicense in epLicenses) {
				if (!IsSerialNumberAlreadyAdded(epLicense.SerialNumber, groupEpLicenses))
				{
				    bool isMalborkType = IsMalbork(epLicense.PartSubType);
                    bool isTrasELA = epLicense.PartSubType.Contains(EndpointLicenseType.TrapsELA);
					string deviceWindow = string.Format("showEditDeviceDetailWindow('{0}')", epLicense.DeviceId);
					string licDownload =
            string.Format("<a class=\"k-grid-csp-download ep-download-license\" style=\"margin-left: 5px;\" onclick=\"GenerateAndDownloadLicense('{0}','{1}', '{2}')\" title=\"Download {1} License Key\"><span> </span></a>", 
                        epLicense.AuthorizationCode, epLicense.PartSubType, epLicense.PartNumber);
					var grpLicense = new AvailableEp();
					grpLicense.SerialNumber = epLicense.SerialNumber;
					grpLicense.DivSerialNumber = string.Format("<div class='licenserow'><a href='#' onclick={1}>{0}</a></div>",
																												epLicense.SerialNumber, deviceWindow);
					grpLicense.EsmAuthorizationCode = epLicense.AuthorizationCode;
					grpLicense.AuthorizationCode = string.Format("<div class='licenserow'>{0}</div>",
																												epLicense.AuthorizationCode);
                    grpLicense.ELAAuthCode = string.Format("<div class='licenserow'>{0}</div>",
                                                                                                                epLicense.ELAAuthCode);
					grpLicense.Description = string.Format("<div class='licenserow'>{0}</div>",
																									epLicense.Description);
					grpLicense.Quantity = string.Format("<div class='licenserow'>{0}</div>", epLicense.UseCount);
					if (epLicense.ExpirationDate.HasValue && epLicense.PartSubType != EndpointLicenseType.EpSms) {
						grpLicense.DivExpirationDate = string.Format(epLicense.ExpirationDate.Value < System.DateTime.Now ? "<div class='licenserow expired-license'>{0}</div>" : "<div class='licenserow '>{0}</div>", epLicense.ExpirationDate.Value.ToShortDateString());
					} else {
						grpLicense.DivExpirationDate = string.Format("<div class='licenserow '>{0}</div>", "Perpetual");
					}
					grpLicense.DeviceId = epLicense.DeviceId;
					grpLicense.RegistrationDate = epLicense.RegistrationDate;
					grpLicense.PartSubType = string.Format("<div class='licenserow'>{0}</div>",
																									epLicense.PartSubType);
					grpLicense.LicenseFeatureName = string.Format("<div class='licenserow'>{0}{1}</div>",
																									epLicense.LicenseFeatureName, licDownload);
                    //Malbork: always show licnese download
                    if (isMalborkType)
				    {
				        grpLicense.ShowLicenseKey = true;
                        grpLicense.PartSubTypeName = string.Format("<div class='licenserow'>{0}{1}</div>", GetMalborkLicenseDescription(epLicense.PartNumber, epLicense.PartSubTypeName), licDownload);
                    }
                    else if (isTrasELA)
                    {
                        grpLicense.ShowLicenseKey = true;
                        grpLicense.PartSubTypeName = string.Format("<div class='licenserow'>{0}{1}</div>", epLicense.PartSubTypeName, licDownload);
                    }
                    else
                    {
                        grpLicense.PartSubTypeName = string.Format("<div class='licenserow'>{0}{1}</div>", epLicense.PartSubTypeName, licDownload);
                    }

				    if (epLicense.PartSubType != EndpointLicenseType.EpSms && !epLicense.PartSubType.Contains(EndpointLicenseType.EpMobile)
                        && !isMalborkType && !isTrasELA)
                    {
						grpLicense.Actions =
								string.Format(
										"<div class='licenserow'><span><a class='k-button k-button-icontext k-grid-csp-edit' href='javascript:void(0);' onclick=\"javascript: AddEpLicense('{0}')\"><span class=' '></span></a></span></div>",
										grpLicense.EsmAuthorizationCode);
					} else {
                        if (!isMalborkType && !isTrasELA && epLicense.PartSubType.Contains(EndpointLicenseType.EpMobile))
						{
							grpLicense.IsTrapsMobile = true;
							grpLicense.ShowLicenseKey = true;
							grpLicense.ShowSoftwareUpdates = true;
                            grpLicense.Actions = string.Format(
                                        "<div class='licenserow'><span><a class='k-button k-button-icontext k-grid-csp-download ep-download-software' title='Download' href='javascript:void(0);' onclick=\"Javascript: DownloadSoftwareUpdates('{1}')\"><span class=' '></span> </a></span><span><a class='k-button k-button-icontext k-grid-csp-edit' href='javascript:void(0);' onclick=\"javascript: AddEpLicense('{0}')\"><span class=' '></span></a></span></div>",
                                        grpLicense.EsmAuthorizationCode, epLicense.PartSubType);
						}
						else
                        {
                            var partSubType = epLicense.PartSubType;
                            var softwareDownloadingHtml = string.Format("<span><a class='k-button k-button-icontext k-grid-csp-download ep-download-software' title='Download' href='javascript:void(0);' onclick=\"Javascript: DownloadSoftwareUpdates('{0}')\"><span class=' '></span> </a></span>", partSubType);
                            var addEpLicenseHtml = string.Format("<span><a class='k-button k-button-icontext k-grid-csp-edit' href='javascript:void(0);' onclick=\"javascript: AddEpLicense('{0}')\"><span class=' '></span></a></span>",
                                        grpLicense.EsmAuthorizationCode);
                            if (isMalborkType || isTrasELA)
                            {
                                grpLicense.ShowSoftwareUpdates = true;
                                //addEpLicenseHtml = string.Empty;
                            }
                            grpLicense.Actions = string.Format("<div class='licenserow'>{0}{1}</div>", softwareDownloadingHtml, addEpLicenseHtml);
						}
					}
					grpLicense.DeviceName = string.Format("<div class='licenserow'>{0}</div>",
																												epLicense.DeviceName);
					grpLicense.DeviceTag = string.Format("<div class='licenserow'>{0}</div>",
																												epLicense.DeviceTag);

					if (grpLicense.ASCId == null && epLicense.ASCId != null)
					{
						grpLicense.ASCId = epLicense.ASCId;
						grpLicense.ASCName = epLicense.ASCName;
					}
					groupEpLicenses.Add(grpLicense);
				} else {
					var item = groupEpLicenses.Find(x => x.SerialNumber == epLicense.SerialNumber);
					item.AuthorizationCode = string.Format("{0}<div class='licenserow'>{1}</div>",
																									item.AuthorizationCode, epLicense.AuthorizationCode);
					item.Description = string.Format("{0}<div class='licenserow'>{1}</div>", item.Description,
																						epLicense.Description);
					item.Quantity = string.Format("{0}<div class='licenserow'>{1}</div>", item.Quantity,
																				epLicense.UseCount);
					if (epLicense.PartSubType == EndpointLicenseType.EpSms) {
						item.DivExpirationDate = string.Format("{0}<div class='licenserow'>{1}</div>",
																												item.DivExpirationDate,
																												"Perpetual");
					} else if (epLicense.ExpirationDate.HasValue) {
						if (epLicense.ExpirationDate.Value < System.DateTime.Now) {
							item.DivExpirationDate =
									string.Format("{0}<div class='licenserow expired-license'>{1}</div>",
																item.DivExpirationDate,
																epLicense.ExpirationDate.Value.ToShortDateString());
						} else {
							item.DivExpirationDate = string.Format("{0}<div class='licenserow'>{1}</div>",
																											item.DivExpirationDate,
																											epLicense.ExpirationDate.Value.ToShortDateString());
						}
					} else {
						item.DivExpirationDate = string.Format("{0}<div class='licenserow'>{1}</div>",
																												item.DivExpirationDate,
																												string.Empty);
					}

					item.PartSubType = string.Format("{0}<div class='licenserow'>{1}</div>", item.PartSubType,
																						epLicense.PartSubType);
					item.LicenseFeatureName = string.Format("{0}<div class='licenserow'>{1}</div>", item.LicenseFeatureName, (epLicense.PartSubType == EndpointLicenseType.SvcEsm || epLicense.PartSubType == EndpointLicenseType.SvcRenEsm || epLicense.PartSubType == EndpointLicenseType.BKLNSvcEsm || epLicense.PartSubType == EndpointLicenseType.BKLNSvcRenEsm) ? ConvertSupportNameToFriendlySupportName(epLicense.LicenseName, epLicense.LicenseFeatureName) : epLicense.LicenseFeatureName);

					item.PartSubTypeName = string.Format("{0}<div class='licenserow'>{1}</div>", item.PartSubTypeName, (epLicense.PartSubType == EndpointLicenseType.SvcEsm || epLicense.PartSubType == EndpointLicenseType.SvcRenEsm || epLicense.PartSubType == EndpointLicenseType.BKLNSvcEsm || epLicense.PartSubType == EndpointLicenseType.BKLNSvcRenEsm) ? ConvertSupportNameToFriendlySupportName(epLicense.PartSubTypeName, epLicense.LicenseFeatureName) : epLicense.PartSubTypeName);

					if (epLicense.PartSubType != EndpointLicenseType.SvcEsm && epLicense.PartSubType != EndpointLicenseType.SvcRenEsm && epLicense.PartSubType != EndpointLicenseType.SvcEsmWrnt && epLicense.PartSubType != EndpointLicenseType.BKLNSvcEsm) {
						item.ShowLicenseKey = true;
						item.Actions = string.Format("{0}<div class='licenserow'>{1}</div>", item.Actions,
																					string.Format(
																							"<div class='licenserow'><span><a class='k-button k-button-icontext k-grid-csp-download ep-download-software' title='Download' href='javascript:void(0);' onclick=\"Javascript: DownloadSoftwareUpdates('{0}')\"><span class=' '></span> </a></span></div>",
																							epLicense.PartSubType));
					} else {
						item.Actions = string.Format("{0}<div class='licenserow'>{1}</div>",
																												item.Actions,
																												string.Empty);

						if ((epLicense.PartSubType == EndpointLicenseType.SvcEsm || epLicense.PartSubType == EndpointLicenseType.SvcRenEsm || epLicense.PartSubType == EndpointLicenseType.SvcEsmWrnt || epLicense.PartSubType.Contains("BKLN")) && epLicense.ExpirationDate > System.DateTime.Now) {
							item.ShowSoftwareUpdates = true;
						}
					}
					if (item.ASCId == null && epLicense.ASCId != null)
					{
						item.ASCId = epLicense.ASCId;
						item.ASCName = epLicense.ASCName;
					}
				}
			}
			return groupEpLicenses;
		}
        public static bool IsMalbork(string panDbIdentification)
        {
            var regExp = new Regex(EndpointLicenseType.MalborkDbIdentificationPattern, RegexOptions.IgnoreCase);
            if (regExp.IsMatch(panDbIdentification))
                return true;
            return false;
        }
        public bool IsTMSMalbork(string panDbIdentification)
        {
            var regExp = new Regex(EndpointLicenseType.MalborkDbIdentificationPattern, RegexOptions.IgnoreCase);
            if (regExp.IsMatch(panDbIdentification))
                return true;
            return false;
        }

        public static bool IsSkuMalbork(string partNumber)
        {
            var regExp = new Regex(EndpointLicenseType.MalborkSkuPattern, RegexOptions.IgnoreCase);
            if (regExp.IsMatch(partNumber))
                return true;
            return false;            
        }

		private bool IsSerialNumberAlreadyAdded(string serialNumber, List<AvailableEp> grpEpLicenses) {
			var item = grpEpLicenses.Find(x => x.SerialNumber == serialNumber);
			if (item != null) {
				return true;
			}
			return false;
		}

		private string ConvertSupportNameToFriendlySupportName(string name, string featurename) {
			return string.Concat(name, " ", featurename.First().ToString().ToUpper() + featurename.Substring(1).ToLower());
		}


        public bool SaveLicenseKey(string authCode, string licenseKey)
        {
            return _epDbLogic.SaveLicenseKey(authCode, licenseKey);
        }


				public bool GetLicenseKey(string authCode, long supportAccountId, out string licenseKey)
        {
            if (IsEpLicenseEsmOrBundle(authCode))
            {
                return _epDbLogic.GetLicenseKey(authCode, supportAccountId, out licenseKey);
            }
            licenseKey = string.Empty;
            return false;
        }


        public bool IsEpLicenseEsmOrBundle(string authCode)
        {
            return _epDbLogic.IsEpLicenseEsmOrBundle(authCode);
        }


        public bool IsBetaCustomer(string user)
        {
            return _epDbLogic.IsBetaCustomer(user);
        }


        public bool GetTrapsUseCount(string authCode, out int? workstationCount, out int? serverCount, out int? vdiCount, out int? trapsCount)
        {
            return _epDbLogic.GetTrapsUseCount(authCode, out workstationCount, out serverCount, out vdiCount,
                out trapsCount);
        }


        public long? GetAgentLicenseCountByEpStubId(long deviceId)
        {
            return _epDbLogic.GetAgentLicenseCountByEpStubId(deviceId);
        }

        public static string GetMalborkLicenseDescription(string partNumber, string partSubTypeName)
        {
            if (partNumber.Contains("-LAB-"))
                return "Traps LAB, Standard Support";
            if (partNumber.Contains("-NFR-"))
                return "Traps NFR, Standard Support";
            if (partNumber.Contains("-BKLN-"))
                return "Traps, Premium Partner Support";
            if (partNumber.Contains("-BKLNUSG-"))
                return "Traps, US Government Premium Partner Support";
            if (partNumber.Contains("-USG-"))
                return "Traps, US Government Premium Support";
            //Evaluation
            if (partNumber.Contains("AGENT-E"))
                return "Traps Evaluation, Standard Support";
            //BETA
            if (partNumber.Contains("-BETA"))
                return "Traps Beta";

            //all other
            return "Traps, Premium Support";
        }

        public string GetLicenseApiKey(long supportAccountId)
        {
            return _epRepository.GetLicenseApiKey(supportAccountId);
        }
    }

    public class EndPointLicense
    {
        public string ExpirationDate { get; set; }
        public string CustomerName { get; set; }
        public int AgentPoolAmount { get; set; }
				public int VdiPoolAmount { get; set; }
        public int ServerPoolAmount { get; set; }
        public bool Reflector { get; set; }
        public bool IsWildFire { get; set; }
        public string WildFireApiKey { get; set; }
        public string WildFireAddress { get; set; }
        public int LicenseVersion { get; set; } // 1: Starlight, 2: Malbork
        public string SerialNumber { get; set; }
    }

    public class MalborkLicense : EndPointLicense
    {
        public string LicenseApiKey { get; set; }
    }

    public class EndPointLicenseResponse
    {
        public string AdditionalInfo { get; set; }
        public string Message { get; set; }
        public bool Succeed { get; set; }
        public string ValidationInfo { get; set; }
    }
}
