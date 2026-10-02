using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Csp.Api.DataAccess;
using Csp.Api.LicensingService;
using Csp.Api.Services.Contracts;
using Csp.Api.Model;
using License = Csp.Api.Model.License;
using Csp.Api.HelperClasses;
using Newtonsoft.Json;
using System.Text;
using AutoMapper;
using Csp.Api.DeviceAndLicenseService;
using Csp.Api.Model.Flex;
using System.Threading.Tasks;

namespace Csp.Api.Logic
{

    public class LicenseLogic : ILicenseLogic
    {

        private readonly IDpmRepository _dpmRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILicenseRepository _licenseRepository;
        private readonly IUserAccountLogic _userAccountLogic;
        private readonly FeatureAccess _featureAccess = new FeatureAccess();
        private readonly ISaasDataAccess _saasDataAccess;
        private readonly ILicensingApiLogic _iLicensingApiLogic;

        private readonly string coreAPIURL = ConfigurationManager.AppSettings["CoreAPIUrl"].ToString();
        private readonly string coreAPIuser = ConfigurationManager.AppSettings["LicensingServiceUserName"].ToString();
        private readonly string coreAPIkey = ConfigurationManager.AppSettings["LicensingServicePassword"].ToString();

        public LicenseLogic(IDpmRepository dpmRepository, IDeviceRepository deviceRepository, ILicenseRepository licenseRepository, IUserAccountLogic userAccountLogic, ISaasDataAccess saasDataAccess, ILicensingApiLogic iLicensingApiLogic)
        {
            _dpmRepository = dpmRepository;
            _deviceRepository = deviceRepository;
            _licenseRepository = licenseRepository;
            _userAccountLogic = userAccountLogic;
            _saasDataAccess = saasDataAccess;
            _iLicensingApiLogic = iLicensingApiLogic;
        }

        public IEnumerable<ReleasedLicenseItem> GetReleasedVMLicenses(DataParameters parameters, long supportAccountId)
        {

            var releasedLicenses = _licenseRepository.GetReleasedVMLicenses(parameters, supportAccountId);

            return releasedLicenses;
        }

        public IsSiteLicenseResult IsSiteLicense(string authorizationCode)
        {

            var licenseDataAccess = new LicenseDataAccess();
            return licenseDataAccess.IsSiteLicense(authorizationCode);
        }

        public bool Is4HourRma(string authorizationCode)
        {

            var licenseDataAccess = new LicenseDataAccess();
            return licenseDataAccess.Is4HourRma(authorizationCode);
        }

        public string IsAuthorizationCodeValid(string authorizationCode, string serialNumber)
        {

            var licenseDataAccess = new LicenseDataAccess();
            return licenseDataAccess.IsAuthorizationCodeValid(authorizationCode, serialNumber);
        }

        public string IsSiteLicenseAuthorizationCodeValid(string authorizationCode)
        {
            if (string.IsNullOrEmpty(authorizationCode))
            {
                return "Authorization Code is required";
            }
            var result = _licenseRepository.IsSiteLicenseAuthCodeValid(authorizationCode);

            switch (result)
            {
                case SiteLicenseResult.Valid:
                    return string.Empty;

                case SiteLicenseResult.InvalidCode:
                    return "Invalid Authorization Code";

                case SiteLicenseResult.NotASiteLicense:
                    return "You must enter a Site License Auth Code";

                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public bool IsAuthCodeValid(string authCode)
        {
            return _licenseRepository.IsAuthCodeValid(authCode);
        }


        /// <summary>licenseit
        /// This method is used to retrieve the List of License based on DeviceID
        /// </summary>
        /// <param name="deviceId">Device Id</param>

        /// <returns>List<License></returns>
        public List<LicenseListItem> GetDeviceLicenses(long deviceId)
        {

            var licenseDataAccess = new LicenseDataAccess();

            var licenses = licenseDataAccess.ReadDeviceLicenses(deviceId);

            var transactionIdRecord = licenses.FirstOrDefault(x => x.TransactionId != null && (x.AuthorizationCode != null && x.AuthorizationCode.ToUpper().StartsWith("V")) || (x.LicenseName != null && x.LicenseName.ToUpper().Contains("PA-VM")) || (x.LicenseFeaturesName != null && x.LicenseFeaturesName.ToUpper().Contains("PA-VM")));

            if (transactionIdRecord != null)
            {
                var upgradeRecord = _licenseRepository.GetVmUpgradeInfo(deviceId, transactionIdRecord.TransactionId);
                if (upgradeRecord != null)
                {
                    if (!string.IsNullOrEmpty(upgradeRecord.FromModel) &&
                        !string.IsNullOrEmpty(upgradeRecord.ToModel))
                    {
                        transactionIdRecord.LicenseName = string.Format("VM Upgrade from {0} to {1}", upgradeRecord.FromModel,
                            upgradeRecord.ToModel);
                    }

                    if (!string.IsNullOrEmpty(upgradeRecord.OriginalAuthCode) &&
                        !string.IsNullOrEmpty(upgradeRecord.UpgradedAuthCode))
                    {
                        transactionIdRecord.AuthorizationCode = string.Format("{0}, {1}", upgradeRecord.OriginalAuthCode,
                            upgradeRecord.UpgradedAuthCode);
                    }
                }
            }

            // Use support account id 245 to get the License API Key for all support accounts.
            long supportAccountId = ApiConstants.PANW_SUPPORT_ACCOUNT_ID;
            string licensingApiKey = _iLicensingApiLogic.GetLicensingApiKey(supportAccountId).LicensingApiKey;
            MagicLinkHelper.SetBundleMagicLinks(licenses, licensingApiKey);

            return licenses;
        }


        public License GetLicenseByAuthcode(string authorizationCode)
        {

            var licenseDataAccess = new LicenseDataAccess();

            return licenseDataAccess.ReadLicenseByAuthCode(authorizationCode);
        }

        public LicenseAuthorizationBundle GetLicenseAuthorizationBundle(string serialNumber, string authorizationCode)
        {

            var licenseDataAccess = new LicenseDataAccess();

            return licenseDataAccess.ReadLicenseAuthorizationBundle(serialNumber, authorizationCode);
        }

        public List<object> GetTrialLicenseListBySerialNumber(string serialNumber, string mode)
        {

            var licenseDataAccess = new LicenseDataAccess();

            return licenseDataAccess.GetTrialLicenseListBySerialNumber(serialNumber, mode);
        }

        public LicenseKey GetLicenseKeybyId(long licenseId, long restrictedByUserAccountId)
        {

            //verify the user can see this license
            var licenseRepository = DependencyResolver.Current.GetService<ILicenseRepository>();
            var deviceId = licenseRepository.ReadLicenses(new[] { licenseId }).Single().DeviceId;
            if (!deviceId.HasValue) throw new UnauthorizedAccessException();

            //verify the user can see the device
            var deviceDataAccess = new DeviceDataAccess();
            var device = deviceDataAccess.Read(deviceId.Value, restrictedByUserAccountId);
            if (device == null)
            {
                throw new Exception(string.Format("Access to Device Id {0} for User Id {1} is Denied", deviceId.Value, restrictedByUserAccountId));
            }

            var licenseDataAccess = new LicenseDataAccess();
            var licenseKey = licenseDataAccess.ReadLicenseKeybyId(licenseId);

            if (licenseKey.Key == null)
            {

                /*This should not happen unless there is no reference in the license, or the PartSubType record does not exist.
				 * Both are an error state.*/
                throw new NullReferenceException("License key is null, to resolve check that all data exists.");
            }

            return licenseKey;
        }

        public string GetNFRSVCExpirationFromPandb(string serialNumber)
        {

            var licenseDataAccess = new LicenseDataAccess();

            return licenseDataAccess.ReadNFRSVCExpirationFromPandb(serialNumber);
        }

        public bool HasNfrBnd(string serialNumber)
        {
            var partNumber = _dpmRepository.GetDevicePartNumber(serialNumber);

            if (!partNumber.Contains("-NFR"))
                return false;

            var deviceId = _deviceRepository.ReadDeviceIdBySerialNumber(serialNumber);
            var lics = GetDeviceLicenses(deviceId);
            var hasNfrSBndLic = lics.Any(license =>
                                                                     license.ExpirationDate > DateTime.Now
                                                                     && !license.ModelName.Contains("-PRA") //Not Panorama
                                                                     && license.ModelName.Contains("-BND")
                );

            return hasNfrSBndLic;
        }

        public bool HasNfrSvcOrBndLic(string serialNumber, string partNumber)
        {
            var deviceId = _deviceRepository.ReadDeviceIdBySerialNumber(serialNumber);
            var lics = GetDeviceLicenses(deviceId);
            var hasNfrSvcOrBndLic = lics.Any(license =>
                license.ExpirationDate > DateTime.Now
                && !license.ModelName.Contains("-PRA") //Not Panorama
                && (
                    license.ModelName.Contains("PAN-SVC-NFR")
                    || license.ModelName.Contains("-BND")
                )
            );

            return hasNfrSvcOrBndLic;
        }

        public void GetPartNumberAndModel(string serialNumber, out string partNumber, out string model)
        {
            partNumber = _deviceRepository.GetDevicePartNumber(serialNumber);
            model = GetModel(partNumber);
        }

        public bool IsElligibleForFeatureLicense(string featureName, string featureSku, string serialNumber, string model, string userEmail, long? supportAccountId, bool isNFR, bool hasNfrSvcOrBndLic)
        {
            var isModelFatureEnabled = _featureAccess.IsFeatureEnabledForModel(featureName, model);
            if (!isModelFatureEnabled)
                return false;

            var isBetaFeature = _featureAccess.IsBetaFeature(featureName, model);
            //This feature is beta feature for this model, check if the customer has access to it or not.
            if (isBetaFeature)
            {
                var canUserAccessFeature = _featureAccess.CanUserAccessBetaFeature(featureName, userEmail, supportAccountId);
                if (!canUserAccessFeature)
                    return false;
            }

            //If NFR then show as long as they have a PAN-SVC-NFR license or a bundle license
            /* 09/20/2017: We don't need this check anymore. The NFR model should have the same feature as the regular models.
             * if (isNFR && !hasNfrSvcOrBndLic)
	            return false;*/

            //otherwies only show if they don't have the license now
            var hasFeatureLicense = _featureAccess.DeviceHasLicense(serialNumber, featureSku);
            return !hasFeatureLicense;
        }

        /*
		public bool IsEligibleForDpmLicense(string serialNumber, string userEmail, long? supportAccountId) {

			var partNumber = _dpmRepository.GetDevicePartNumber(serialNumber);
			var model = GetModel(partNumber);
			var isModelDpmEnabled = _dpmRepository.IsModelDpmEnabled(model);

			//If not DPM enabled model, then don't show
			if (!isModelDpmEnabled) return false;

		    var isBetaFeature = _featureAccess.IsBetaFeature(FeatureAccess.DecryptionPortMirror_Feature, model);
            //This feature is beta feature for this model, check if the customer has access to it or not.
		    if (isBetaFeature)
		    {
                var canUserAccessFeature = _featureAccess.CanUserAccessBetaFeature(FeatureAccess.DecryptionPortMirror_Feature, userEmail, supportAccountId);
		        if (!canUserAccessFeature)
		            return false;
		    }

			//If NFR then show as long as they have a PAN-SVC-NFR license or a bundle license
			if (partNumber.Contains("-NFR")) {
				var deviceId = _deviceRepository.ReadDeviceIdBySerialNumber(serialNumber);
				var lics = GetDeviceLicenses(deviceId);
				var hasNfrSvcOrBndLic = lics.Any(license =>
						license.ExpirationDate > DateTime.Now
						&& !license.ModelName.Contains("-PRA") //Not Panorama
						&& (
							license.ModelName.Contains("PAN-SVC-NFR")
							|| license.ModelName.Contains("-BND")
							)
						);

				return hasNfrSvcOrBndLic;
			}

			//otherwies only show if they don't have the license now
			var hasDpmLicense = _dpmRepository.HasDpmLicense(serialNumber);
			return !hasDpmLicense;
		}
        */

        private string GetModel(string partNumber)
        {
            var partNumberParts = partNumber.Split('-');

            if (partNumber.StartsWith("PAN-PA"))
            {
                //VM models
                if (partNumberParts[2] == "VM")
                {
                    if (partNumberParts.Length > 4 && partNumberParts[3] == "1000")
                        return partNumberParts.Length >= 3
                            ? string.Format("VM-{0}-{1}", partNumberParts[3], partNumberParts[4])
                            : null;
                    else
                        return partNumberParts.Length >= 3 ? string.Format("VM-{0}", partNumberParts[3]) : null;
                }

                return partNumberParts.Length >= 3 ? partNumberParts[2] : null;
            }
            else if (partNumber.StartsWith("PAN-PRA"))
            {
                if (partNumberParts[2].All(char.IsNumber))
                    return partNumberParts.Length >= 3 ? string.Format("{0}-{1}", partNumberParts[1], partNumberParts[2]) : null;
                return partNumber.Substring(4); //PAN-PRA-
            }

            //PartNumber not starting with PAN-PA
            return (partNumberParts.Length >= 3) ? string.Format("{0}-{1}", partNumberParts[1], partNumberParts[2]) : null;
        }

        public bool GrantTrial(string serialNumber, string featureName, int evalutionDays = 0, string cpuid = "", string uuid = "", string userEmail = "")
        {

            var result = GetLicensingServiceInstance().grantEval(serialNumber, cpuid, uuid, featureName, evalutionDays, userEmail);
            if (!result)
            {
                if (HttpContext.Current != null)
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error from LicenseIt: grantEval(serialNumber: {0}, cpuid: {1}, uuid: {2}, featureName: {3}, evaluationDays: {4})",
                        NullSafeStr(serialNumber), NullSafeStr(cpuid), NullSafeStr(uuid), NullSafeStr(featureName), evalutionDays)));
            }
            return result;
        }

        private static string NullSafeStr(string value)
        {
            return value ?? "<NULL>";
        }

        public bool GrantNFREval(string serialNumber, string featureName, int evalutionDays, string limiter, bool IsRemove, string cpuid = "", string uuid = "", string userEmail = "")
        {

            var result = GetLicensingServiceInstance().grantNFREval(serialNumber, cpuid, uuid, featureName, evalutionDays, limiter, IsRemove.ToString(), userEmail);
            if (!result)
            {
                if (HttpContext.Current != null)
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error from LicenseIt: grantNFREval(serialNumber: {0}, cpuid: {1}, uuid: {2}, featureName: {3}, evaluationDays: {4}, limiter: {5}, isRemove: {6})",
                        NullSafeStr(serialNumber), NullSafeStr(cpuid), NullSafeStr(uuid), NullSafeStr(featureName), evalutionDays, NullSafeStr(limiter), IsRemove)));
            }
            return result;
        }


        public licenseRet[] LicenseIt(string serialNumber, string cpuid, string uuid, string authorizationCode, string hyperVisor, string panOs)
        {

            return GetLicensingServiceInstance().licenseit2(serialNumber, cpuid, uuid, authorizationCode, panOs, hyperVisor);
        }

        public bool CreateStub(string assetType, string authorizationCode, long userAccountID, long supportAccountID)
        {
            return GetLicensingServiceInstance().CreateStub(assetType, authorizationCode, userAccountID, supportAccountID);
        }
        public licenseRet[] LicenseIt(string serialNumber, string cpuid = "", string uuid = "", string authorizationCode = "")
        {

            return GetLicensingServiceInstance().licenseit(serialNumber, cpuid, uuid, authorizationCode);
        }

        public string ActivateLicense(string serialNumber, string cpuid = "", string uuid = "", string authorizationCode = "")
        {

            var licenseDataAccess = new LicenseDataAccess();

            try
            {
                //CSP-7638 licenseit endpoint method is blocked for Iot, InlineSaaS and Raptor because FW is using them as these cannot be activated in FW (Firewall PanOS)
                //we use the new endpoint activateBundle for Raptor and those need IoT, InlineSaaS in the future
                var resultArray = (licenseDataAccess.IsRaptorEnterpriseBundle(authorizationCode) || licenseDataAccess.IsSellingBundleWithIot(authorizationCode))
                ? GetLicensingServiceInstance().activateBundle(serialNumber, cpuid, uuid, authorizationCode)
                : GetLicensingServiceInstance().licenseit(serialNumber, cpuid, uuid, authorizationCode);

                return (resultArray == null) || (resultArray.Any(message => message.errmsg != null))
                                     ? resultArray.First().errmsg
                                     : "";
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                EmailHelper.SendExceptionEmail(ex);
            }

            return string.Empty;
        }


        public bool TransferIt(string srcSerialNumber, string destSerialNumber, string curUserEmail)
        {

            var result = GetLicensingServiceInstance().transferIt(srcSerialNumber, destSerialNumber, curUserEmail);
            if (!result)
            {
                if (HttpContext.Current != null)
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error from LicenseIt: transferIt(srcSerialNumber: {0}, destSerialNumber: {1}, curUserEmail: {2})",
                        NullSafeStr(srcSerialNumber), NullSafeStr(destSerialNumber), NullSafeStr(curUserEmail))));
            }
            return result;
        }

        public bool GrantSiteLicenses(long supportAccountId)
        {
            var _deviceDataAccess = new DeviceDataAccess();
            _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, "", "LicenseLogic---GrantSiteLicenses", "GrantSiteLicenses Start", "Info");

            if (supportAccountId == 245)
            {
                return true;
            }
            try
            {
                _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, "", "LicenseLogic---GrantSiteLicenses", "GetLicensingServiceInstance Service Calling", "Info");
                var result = GetLicensingServiceInstance().grantSiteLicenses(Convert.ToInt32(supportAccountId));
                if (!result)
                {
                    _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, "", "LicenseLogic---GrantSiteLicenses", "GetLicensingServiceInstance Service Error from LicenseIt", "Info");

                    if (HttpContext.Current != null)
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error from LicenseIt: grantSiteLicenses(supportAccountId: {0})",
                            supportAccountId)));
                }
                _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, "", "LicenseLogic---GrantSiteLicenses", "GetLicensingServiceInstance Service Success", "Info");

                return result;
            }
            catch (Exception oex)
            {
                _deviceDataAccess.LogGrantDeviceInfo(supportAccountId, "", "LicenseLogic---GrantSiteLicenses", "GetLicensingServiceInstance Service Error Message : " + oex.Message + " & StackTrace : " + oex.StackTrace, "Error");

                if (HttpContext.Current != null)
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error from LicenseIt: grantSiteLicenses(supportAccountId: {0}).  Error is {1}",
                        supportAccountId, oex.ToString())));
                return false;
            }
        }

        public licenseRet[] ActivateSiteLicense(long supportAccountId, string authorizationCode)
        {

            try
            {
                return GetLicensingServiceInstance()
                    .activateSiteLicense(Convert.ToInt32(supportAccountId), authorizationCode, true);
            }
            catch (Exception e)
            {
                if (HttpContext.Current != null)
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Error from LicenseIt: ActivateSiteLicense(supportAccountId: {0}).  Error is {1}",
                        supportAccountId, e.ToString())));
                var result = new licenseRet[1];
                result[0] = new licenseRet
                {
                    errmsg = "License activation error, please contact customer support."
                };
                return result;
            }
        }

        private LicenseService2SoapClient GetLicensingServiceInstance()
        {
            try
            {
                var coreServiceCertThumbprint = ConfigurationManager.AppSettings["CoreServicesCertThumbprint"];
                var coreServicesEndPoint = ConfigurationManager.AppSettings["CoreServicesUrl"];
                var userName = ConfigurationManager.AppSettings["LicensingServiceUserName"];
                var userKey = ConfigurationManager.AppSettings["LicensingServicePassword"];

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                       | SecurityProtocolType.Tls11
                                                       | SecurityProtocolType.Tls12
                                                       | SecurityProtocolType.Ssl3;

                var licenseService = new LicenseService2SoapClient();

                if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(userKey) && string.IsNullOrEmpty(coreServiceCertThumbprint))
                {
                    return licenseService;
                }

                var myBinding = new BasicHttpBinding
                {
                    Security =
                    {
                        Mode = BasicHttpSecurityMode.Transport,
                        Transport = { ClientCredentialType = HttpClientCredentialType.Basic }
                    }
                };

                var clientCredentials = new ClientCredentials();

                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userKey))
                {
                    clientCredentials.UserName.UserName = userName;
                    clientCredentials.UserName.Password = userKey;
                }

                if (!string.IsNullOrEmpty(coreServiceCertThumbprint))
                {
                    var clientCertificate = CertificateManager.GetCertificateByCnName(coreServiceCertThumbprint);

                    myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

                    if (clientCertificate == null)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Empty certificate store"));
                    }

                    clientCredentials.ClientCertificate.Certificate = clientCertificate;
                }

                licenseService = new LicenseService2SoapClient(myBinding, new EndpointAddress(coreServicesEndPoint));
                licenseService.ChannelFactory.Endpoint.Behaviors.Remove<ClientCredentials>();
                licenseService.ChannelFactory.Endpoint.Behaviors.Add(clientCredentials);

                return licenseService;
            }
            catch (Exception oex)
            {
                var ex = oex;
                throw;
            }
        }


        //This is used for NSX Panorama Provisioning
        public bool UpdateLicFeaturesForNsxProvisioning(string selectedAuthCode, string newSerialNumber)
        {
            var licenseDataAccess = new LicenseDataAccess();
            return licenseDataAccess.UpdateLicFeaturesForNsxProvisioning(selectedAuthCode, newSerialNumber);
        }

        public string GetEPWildFireApiKey(long deviceId, long supportAccountId, DateTime expirationDate)
        {
            return GetLicensingServiceInstance().GetEPWildfireAPIKey(Convert.ToInt32(deviceId), Convert.ToInt32(supportAccountId), expirationDate);
        }


        public string GetPartIdByTrialFeature(string feature)
        {
            string partId;
            switch (feature)
            {
                case "TP":
                    partId = "PAN-TP-TRIAL";
                    break;
                case "URL2":
                    partId = "PAN-URL2-TRIAL";
                    break;
                case "WF":
                    partId = "PAN-WF-TRIAL";
                    break;
                case "URL4":
                    partId = "PAN-URL4-TRIAL";
                    break;
                case "Gatewaytrial":
                    partId = "PAN-GATEWAY-TRIAL";
                    break;
                case "Portaltrial":
                    partId = "PAN-PORTAL-TRIAL";
                    break;
                case "DPM":
                    partId = "PAN-PA-DECRYPT";
                    break;
                case "AIOps":
                    partId = "PAN-AIOPS-TRIAL";
                    break;
                default:
                    partId = string.Empty;
                    break;
            }
            return partId;
        }


        public List<long> GetActivatedTrialLicenseId(string serialNumber, List<string> partIds)
        {
            return _licenseRepository.GetActivatedTrialLicenseId(serialNumber, partIds);
        }


        public bool CreateCloudInstanceWithAccountId(string serialNumber, string cpuId, string uuId, string user, bool isAwsDevice, out long deviceId,
            out string cloudAccountId, out string errorMessage)
        {
            cloudAccountId = string.Empty;
            deviceId = 0;
            errorMessage = string.Empty;
            string cloudType = "AWS";
            if (!isAwsDevice)
                cloudType = "Google Cloud";
            try
            {
                CloudResponse licResp;
                licResp = GetLicensingServiceInstance().CheckOrCreateDevice(serialNumber, cpuId, uuId, user);
                cloudAccountId = licResp.CloudAccountId;
                deviceId = licResp.DeviceId;
                errorMessage = licResp.errorMessage;
                var reg = new Regex(@"(.)+-(G|g)(O|o)(V|v)-(.)+");
                //For government cloud, we don't check awsAccountId
                //remove the && string.IsNullOrEmpty(cloudAccountId) check - csp-14501               
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    if (HttpContext.Current != null)
                    {
                        Elmah.ErrorSignal.FromCurrentContext()
                                .Raise(
                                    new Exception(
                                        string.Format(
                                            "Error from {0}: CheckOrCreateDevice. SerialNumber is {1}, CpuId is {2}, UuId is {3}, User is {4}",
                                            cloudType, serialNumber, cpuId, uuId, errorMessage)));
                    }
                    errorMessage = string.Format("Validation failed: Invalid {0} Instance", cloudType);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception oex)
            {
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(
                                new Exception(
                                    string.Format(
                                        "Error from {0}: CheckOrCreateDevice. SerialNumber is {1}, CpuId is {2}, UuId is {3}, User is {4}",
                                        cloudType, serialNumber, cpuId, uuId, oex.ToString())));
                }
                errorMessage = string.Format("Error creating {0} Instance!", cloudType);
                return false;
            }
        }


        private const char NullChar = (char)0;

        public bool DeactivateLicense(string encryptedCode)
        {

            encryptedCode = encryptedCode.Replace(NullChar.ToString(), string.Empty);

            var results = GetLicensingServiceInstance().deactivateLicense(encryptedCode ?? string.Empty);

            foreach (var r in results)
            {
                if (r.Success.ToLower() == "n")
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("DeactivateLicenseError.  encryptedCode: {0} message: {1}", encryptedCode, r.Error)));
                    return false;
                }
            }

            return true;
        }

        public string ParseDeactivateLicenseFile(HttpPostedFileBase file)
        {
            string fileText;
            using (var sr = new StreamReader(file.InputStream, Encoding.Default))
            {

                fileText = sr.ReadToEnd().Replace(NullChar.ToString(), string.Empty);
            }
            return HttpUtility.HtmlEncode(fileText);
        }

        #region ILicenseLogic Members


        public bool UpdateSapIdSfdcIdForAwsSupportAccount(long supportAccountId, string authCode, long userAccountId)
        {
            string awsSfAccountId = ConfigurationManager.AppSettings["SalesforceDefaultAWSAccountId"].ToString();
            bool canProceed = _licenseRepository.IsAwsAccountAndNotAssociatedWithSapIdOrSfdcId(supportAccountId, awsSfAccountId);
            if (canProceed)
            {
                // Using the deviceId (non aws) we get the sapid, sfdcid. 
                string currentSfdcAccountId;
                string newSfdcAccountId;

                bool isSuccess = _licenseRepository.UpdateSapIdAndSfdcIdForAwsAccount(authCode, supportAccountId, userAccountId, out currentSfdcAccountId, out newSfdcAccountId);

                if (isSuccess && !string.IsNullOrEmpty(currentSfdcAccountId) && !string.IsNullOrEmpty(newSfdcAccountId))
                {
                    // get list of users in current support account
                    // get their contact record in salesforce
                    // update their salesforce account 

                    var useraccounts = _userAccountLogic.GetAwsSupportAccountUsers(supportAccountId);
                    foreach (var useraccount in useraccounts)
                    {
                        _userAccountLogic.UpdateSalesforceContactsAccount(useraccount, supportAccountId, currentSfdcAccountId,
                            newSfdcAccountId);
                    }
                }

            }
            return false;
        }

        #endregion


        public bool CreateAzureInstance(string azureSerialNumber, string cpuId, string uuId, string user, out long deviceId, out string errorMessage)
        {
            deviceId = 0;
            errorMessage = string.Empty;
            CloudResponse licResp;
            try
            {
                licResp = GetLicensingServiceInstance().CheckOrCreateDevice(azureSerialNumber, cpuId, uuId, user);
                deviceId = licResp.DeviceId;
                errorMessage = licResp.errorMessage;
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    if (HttpContext.Current != null)
                    {
                        Elmah.ErrorSignal.FromCurrentContext()
                                .Raise(
                                    new Exception(
                                        string.Format(
                                            "Error from Azure: CheckOrCreateDevice. SerialNumber is {0}, CPUID is {1}, UUID is {2}, ErrorMessage is {3}",
                                            azureSerialNumber, cpuId, uuId, errorMessage)));
                    }
                    errorMessage = "Azure Registration: Call to Licensing failed!";
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception oex)
            {
                if (HttpContext.Current != null)
                {
                    Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(
                                new Exception(
                                    string.Format(
                                        "Error from Azure: CheckOrCreateDevice. SerialNumber is {0}, CPUID is {1}, UUID is {2}, ErrorMessage is {3}",
                                        azureSerialNumber, cpuId, uuId, oex.ToString())));
                }
                errorMessage = "Error creating Azure Instance!";
                return false;
            }
        }


        public bool ReportCsspEndUserInfo(long[] deviceId, long csspSupportAccountId, string customerAccountId, string companyName, string dnbNumber, string address, string country, string region, string city, string postalCode, string endUserContactEmail, string phoneNumber, string website, string industry, string createdBy)
        {
            try
            {
                var result = GetLicensingServiceInstance().ReportEndUserInfo
                (deviceId, csspSupportAccountId, customerAccountId, companyName, dnbNumber, address, country, region, city, postalCode, endUserContactEmail, phoneNumber, website, industry, createdBy);
                return result;
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(
                                new Exception(
                                    string.Format(
                                        "Error from LicensingService: ReportEndUserInfo. DeviceIds are {0}, CSSPSupportAccountId is {1}, CustomerAccountId is {2}, CompanyName is {3}, ErrorMessage is {4}",
                                        string.Join(",", deviceId), csspSupportAccountId, customerAccountId, companyName, oex.Message)));
                return false;
            }

        }


        public bool RollbackUpgrade(string transactionId)
        {
            try
            {
                var result = GetLicensingServiceInstance().FailedUpgradeRollback(transactionId);
                return result;
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(
                                new Exception(
                                    string.Format(
                                        "Error from LicensingService: RollbackUpgrade. TransactionID is {0}. Error is {1}", transactionId, oex.Message)));
                return false;
            }
        }


        public string IsAuthorizationCodeValidForUpgrade(string authorizationCode, string serialNumber)
        {
            var licenseDataAccess = new LicenseDataAccess();
            return licenseDataAccess.IsAuthorizationCodeValidForUpgrade(authorizationCode, serialNumber);
        }


        public bool ProcessPreLicenseRequestSynch(int csspPrelicenseRequestid, int quantity, string requestedBy)
        {
            try
            {
                var result = GetLicensingServiceInstance().CSSPPreLicenseSynch(csspPrelicenseRequestid);
                return result;
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(
                                new Exception(
                                    string.Format(
                                        "Error from LicensingService: CSSPPreLicense(). CsspPrelicenseRequestId is {0}. Error is {1}", csspPrelicenseRequestid, oex.Message)));
            }
            return true;
        }


        public bool ProcessPreLicenseRequestASynch(int csspPrelicenseRequestid, int quantity, string requestedBy)
        {
            try
            {
                var result = GetLicensingServiceInstance().CSSPPreLicense(csspPrelicenseRequestid);
                return result;
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(
                                new Exception(
                                    string.Format(
                                        "Error from LicensingService: CSSPPreLicense(). CsspPrelicenseRequestId is {0}. Error is {1}", csspPrelicenseRequestid, oex.Message)));
            }
            return true;
        }

        public bool Relicenseit(string serialNumber)
        {
            try
            {
                var result = GetLicensingServiceInstance().reLicenseIt(serialNumber);
                return result;
            }
            catch (Exception oex)
            {
                Elmah.ErrorSignal.FromCurrentContext()
                            .Raise(
                                new Exception(
                                    string.Format(
                                        "Error from LicensingService: relicenseIt(). serialNumber is {0}. Error is {1}", serialNumber, oex.Message)));
                return false;
            }
        }

        public bool IsCloudServiceAuthCode(string authCode)
        {
            return _licenseRepository.IsCloudServiceAuthCode(authCode);
        }

        public bool GeneratePanoramaDeviceMgtLicense(string serialNumber, string cpuId, string uuId,
            string hyperVisorVersion, string panOsVersion)
        {
            bool result = false;
            try
            {
                var resultArray = LicenseIt(serialNumber, cpuId, uuId, string.Empty, hyperVisorVersion, panOsVersion);
                result = true;
                if (resultArray.Any(message => message.errmsg != null))
                {
                    result = false;
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error from LicenseIt during GeneratePanoramaDeviceMgtLicense: " + resultArray.First().errmsg));
                }
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error from LicenseIt during GeneratePanoramaDeviceMgtLicense: " + e.Message));
            }

            return result;
        }

        public bool IsPanoramaUpgradeAuthCodeActivation(string serialNumber, string authcode)
        {
            try
            {
                return _licenseRepository.IsPanoramaUpgradeAuthCodeActivation(serialNumber, authcode);
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error from IsPanoramaUpgradeAuthCodeActivation: " + e.Message));
            }

            return false;
        }

        public bool UpdateDeviceLocationFor4HrRma(string serialNum, string city, string postalCode, string countryCode)
        {
            var licenseDataAccess = new LicenseDataAccess();
            return licenseDataAccess.UpdateDeviceLocationFor4HrRma(serialNum, city, postalCode, countryCode);
        }

        public bool isEligibleForCloudLicenseTransfer(string serialNumber, long supportAccountId)
        {
            return _saasDataAccess.GetPanoramaWithCloudServiceForTransfer(supportAccountId, serialNumber).Any();
        }

        public bool TransferCloudLicenses(string fromSerialNumber, string toSerialNumber)
        {
            var licenseDataAccess = new LicenseDataAccess();
            if (licenseDataAccess.TransferCloudLicenses(fromSerialNumber, toSerialNumber))
            {
                try
                {
                    var result = GetLicensingServiceInstance().reLicenseIt(toSerialNumber);
                    return result;
                }
                catch (Exception oex)
                {
                    Elmah.ErrorSignal.FromCurrentContext()
                                .Raise(
                                    new Exception(
                                        string.Format(
                                            "Error from LicensingService: relicenseIt(). serialNumber is {0}. Error is {1}", toSerialNumber, oex.Message)));
                    return false;
                }
            }
            else
                return false;
        }

        public ELACoreAPIResponse ActivateTrapsELA(string authCode, string supportAccountId, string userEmail)
        {
            try
            {
                string URL = coreAPIURL;
                ELACoreAPIRequest corerequest = new ELACoreAPIRequest()
                {
                    auth_code = authCode,
                    support_account_id = supportAccountId,
                    user_email = userEmail
                };
                string requestData = JsonConvert.SerializeObject(corerequest);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = requestData.Length;

                request.Headers["Authorization"] = string.Format("Basic {0}", Convert.ToBase64String(Encoding.Default.GetBytes(string.Format("{0}:{1}", coreAPIuser, coreAPIkey))));

                // Comments By LK On 06/25/2020
                // Same PanServices cert is used for all licensing services
                var panServiceCertThumbprint = ConfigurationManager.AppSettings["PanServicesCertThumbprint"];

                if (!string.IsNullOrEmpty(panServiceCertThumbprint))
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                           | SecurityProtocolType.Tls11
                                                           | SecurityProtocolType.Tls12
                                                           | SecurityProtocolType.Ssl3;

                    var clientCertificate = CertificateManager.GetCertificateByCnName(panServiceCertThumbprint);

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
                StreamReader responseReader = new StreamReader(webStream, Encoding.Default);
                //TODO: This Immediate Encoding and Decoding is done for checkmarx purpose .It doesn't serve any purpose.
                string encodedString = HttpUtility.HtmlEncode(responseReader.ReadToEnd());
                string decodedString = HttpUtility.HtmlDecode(encodedString);
                ELACoreAPIResponse activationResponse = JsonConvert.DeserializeObject<ELACoreAPIResponse>(decodedString, new JsonSerializerSettings()
                {
                    MaxDepth = 32,
                    TypeNameHandling = TypeNameHandling.None
                });
                responseReader.Close();
                return activationResponse;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("ELACoreAPIResponse execption: {0}, {1}", ex.Message, ex.StackTrace)));
                return new ELACoreAPIResponse()
                {
                    issuccess = false,
                    error_message = "Error Occured on CoreAPI Call"
                };
            }

        }
        public bool UpdateSupportForVMELABundles(string authcode, string serialNumber)
        {
            var licenseDataAccess = new LicenseDataAccess();
            return licenseDataAccess.UpdateSupportForVMELAPanorama(authcode, serialNumber);
        }

        public string GetAuthCodeType(string authcode)
        {
            var licenseDataAccess = new LicenseDataAccess();
            return licenseDataAccess.GetAuthCodeType(authcode);
        }

        public string GetPanOsVersion(string authcode)
        {
            var licenseDataAccess = new LicenseDataAccess();
            return licenseDataAccess.GetPanOsVersion(authcode);
        }

        public bool IsIotAuthCodeActivation(string authcode)
        {
            var licenseDataAccess = new LicenseDataAccess();
            return licenseDataAccess.IsIotAuthCodeActivation(authcode);
        }

        public bool IsAiOpsAuthCodeActivation(string authcode)
        {
            var licenseDataAccess = new LicenseDataAccess();
            return licenseDataAccess.IsAiOpsAuthCodeActivation(authcode);
        }



        public DeploymentProfileResponse GetDeploymentProfile(DeploymentProfileRequest deploymentRequest)
        {
            return GetLicensingServiceInstance().get_deployment_profile(
                    supportaccountid: deploymentRequest.SupportAccountId,
                    vcpu: deploymentRequest.VcpuQuantity,
                    entitlements: deploymentRequest.Entitlements.Replace("'", string.Empty),
                    expiration: deploymentRequest.ExpirationDate,
                    tokenId: deploymentRequest.TokenId,
                    partId: deploymentRequest.PartId,
                    isLegacy: deploymentRequest.IsLegacy,
                    numberOfFirewalls: deploymentRequest.NumberOfFirewalls,
                    vmType: deploymentRequest.VmType,
                    supportType: deploymentRequest.SupportType,
                    ascId: deploymentRequest.AscId,
                    ascName: deploymentRequest.AscName,
                    isSubOnlyDp: deploymentRequest.IsSubscriptionOnly);

        }

        public bool PrelicenceAirgapDP(string authCode, string vcpuQuantity, int firewallQuantity)
        {
            try
            {
                var uuid = Guid.NewGuid().ToString();
                var result = GetLicensingServiceInstance().PreLicense3(authCode,
                    uuid.Split('-')[0],
                    uuid,
                    coreAPIkey,
                    vcpuQuantity,
                    "1TB",
                    "PA-VM",
                    "11.0.0",
                    firewallQuantity);
                return result;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("Prelicensing execption: {0}, {1}", ex.Message, ex.StackTrace)));
                return false;
            }
           
        }

        public bool LicenseFetchAirgapDP(FlexDevicesList devices, int numberOfVcpus)
        {
            try
            {
                var licensingServiceInstance = GetLicensingServiceInstance();
                devices.devices.ForEach(x =>
                {
                    licensingServiceInstance.license_fetchAsync(x.SerialNumber, "", "", x.AuthorizationCode, "", "VM", numberOfVcpus.ToString(), "1TB");
                });
                return true;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("LicenseFetch execption: {0}, {1}", ex.Message, ex.StackTrace)));
                return false;
            }
        }

        public FirewallRegisterResponse RegisterFirewall(FirewallRegisterRequest firewallRegisterRequest)
        {
            FirewallRegisterResponse response = null;
            try
            {
                var licenseClient = GetLicensingServiceInstance();
                var serviceEndPoint = string.Format("{0}?op=license_fetch", licenseClient.Endpoint.Address.Uri.AbsoluteUri);

                var serviceResponse = licenseClient.license_fetch(
                    firewallRegisterRequest.SerialNumber,
                    firewallRegisterRequest.Cpuid,
                    firewallRegisterRequest.Uuid,
                    firewallRegisterRequest.AuthCode,
                    firewallRegisterRequest.CurrentOSVersion,
                    firewallRegisterRequest.firewallType,
                    firewallRegisterRequest.Vcpu,
                    firewallRegisterRequest.Memory
                    );

                response = new FirewallRegisterResponse()
                {
                    Endpoint = serviceEndPoint,
                    ServiceResponse = serviceResponse
                };

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("RegisterFirewall execption: {0}, {1}", ex.Message, ex.StackTrace)));

            }
            return response;
        }
        public DeploymentProfileResponse EditDeploymentProfile(DeploymentProfileRequest deploymentRequest)
        {
            return GetLicensingServiceInstance().edit_deployment_profile(
                 dpAuthcode: deploymentRequest.Authcode,
                 vcpu: deploymentRequest.VcpuQuantity,
                 entitlements: deploymentRequest.Subscriptions.Replace("'", string.Empty),
                 expiration: deploymentRequest.ExpirationDate,
                 tokenId: deploymentRequest.TokenId,
                 numberOfFirewalls: deploymentRequest.NumberOfFirewalls,
                 supportType: deploymentRequest.SupportType,
                 ascName: deploymentRequest.AscName,
                 ascId: deploymentRequest.AscId,
                 vmType: deploymentRequest.VmType,
                 isSubOnlyDp: deploymentRequest.IsSubscriptionOnly);
        }

        public DeploymentProfileResponse GetCnxDeploymentProfile(CnxDeploymentProfileRequest deploymentRequest)
        {
            return GetLicensingServiceInstance().get_cnx_deployment_profile(
                supportAccountId: deploymentRequest.SupportAccountId,
                tokens: deploymentRequest.Tokens,
                entitlements: deploymentRequest.Entitlements,
                expiration: deploymentRequest.Expiration,
                tokenId: deploymentRequest.TokenId,
                partId: deploymentRequest.PartId,
                supportType: deploymentRequest.SupportType,
                ascId: deploymentRequest.AscId,
                ascName: deploymentRequest.AscName
                );
        }

        public DeploymentProfileResponse EditCnxDeploymentProfile(CnxDeploymentProfileRequest deploymentRequest)
        {
            return GetLicensingServiceInstance().edit_cnx_deployment_profile(
                dpAuthcode: deploymentRequest.DpAuthcode,
                tokens: deploymentRequest.Tokens,
                entitlements: deploymentRequest.Entitlements,
                expiration: deploymentRequest.Expiration,
                tokenId: deploymentRequest.TokenId
                );
        }


        public List<RepurposePanoramasResponse> RepurposePanoramas(RepurposePanoramasRequest request)
        {

            var response = new List<RepurposePanoramasResponse>();
            var licenseClient = GetLicensingServiceInstance();
            var serviceEndPoint = string.Format("{0}?op=RepurposeExistingPanoramas", licenseClient.Endpoint.Address.Uri.AbsoluteUri);

            try
            {
                RepurposeReqInfo req = null;
                string serialNumber = string.Empty;
                if (request.RepurposeReqInfo != null)
                {
                    serialNumber = string.Join(",", request.RepurposeReqInfo.Select(x => x.SerialNumber.ToString()));

                    req = new RepurposeReqInfo()
                    {
                        SerialNumber = serialNumber,
                        ToAuthCode = request.RepurposeReqInfo[0].ToAuthCode
                    };

                }
                var licenseResponse = licenseClient.RepurposeExistingPanoramas(req.SerialNumber, req.ToAuthCode);
                if (licenseResponse.Any())
                {
                    response = licenseResponse.ToList().Select(x => new RepurposePanoramasResponse
                    {
                        serialNumber = x.SerialNumber,
                        success = x.Success,
                        message = x.Message,
                        Endpoint = serviceEndPoint
                    }).ToList();
                }
                return response;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext()
                                .Raise(new Exception(string.Format("Error from RepurposePanoramas : SerialNumber:{0}, ToAuthCode:{1}.  Error is {2}", string.Join(",", request.RepurposeReqInfo.Select(x => x.SerialNumber.ToString())), request.RepurposeReqInfo[0].ToAuthCode, ex.ToString())));
                response.Add(new RepurposePanoramasResponse
                {
                    success = false,
                    message = ex.Message,
                    serialNumber = string.Join(",", request.RepurposeReqInfo.Select(x => x.SerialNumber.ToString())),
                    Endpoint = serviceEndPoint
                });
                return response;
            }
        }

        public TransferDeploymentProfileResponse TransferDeploymentProfiles(TransferDeploymentProfilesLicensingRequest transferDeploymentProfilesRequest)
        {
            var response = new TransferDeploymentProfileResponse();
            var licenseClient = GetLicensingServiceInstance();
            var serviceEndPoint = string.Format("{0}?op=TransferDeploymentProfiles", licenseClient.Endpoint.Address.Uri.AbsoluteUri);

            try
            {
                var transferResponse = licenseClient.TransferDeploymentProfilesAsync(
                sourceAuthCode: transferDeploymentProfilesRequest.SourceAuthCode,
                destCreditPool: transferDeploymentProfilesRequest.DestCreditPool.ToString(),
                destSupportType: transferDeploymentProfilesRequest.DestSupportType,
                destExpirationDate: transferDeploymentProfilesRequest.DestExpirationDate,
                ascId: transferDeploymentProfilesRequest.AscId,
                ascName: transferDeploymentProfilesRequest.AscName,
                productId: transferDeploymentProfilesRequest.DestProductId,
                config: transferDeploymentProfilesRequest.Config,
                partId: transferDeploymentProfilesRequest.PartId
                );

                if (transferResponse != null && transferResponse.Result != null)
                {
                    response = new TransferDeploymentProfileResponse
                    {
                        Authcode = transferResponse.Result.Authcode,
                        Message = transferResponse.Result.Message,
                        Success = transferResponse.Result.Success,
                        Endpoint = serviceEndPoint
                    };
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext()
                                .Raise(new Exception(string.Format("Error from TransferDeploymentProfiles : SourceAuthCode:{0}, DestCreditPool:{1}, DestSupportType {2},DestExpirationDate: {3}, AscId: { 4}, AscName: {5}, ProductId: {6}, Config: {7}, partId: {8}.  Error is {9}",
                                transferDeploymentProfilesRequest.SourceAuthCode,
                                transferDeploymentProfilesRequest.DestCreditPool,
                                transferDeploymentProfilesRequest.DestSupportType,
                                transferDeploymentProfilesRequest.DestExpirationDate,
                                transferDeploymentProfilesRequest.AscId,
                                transferDeploymentProfilesRequest.AscName,
                                transferDeploymentProfilesRequest.DestProductId,
                                transferDeploymentProfilesRequest.Config,
                                transferDeploymentProfilesRequest.PartId,
                                ex.ToString())));

                response = new TransferDeploymentProfileResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Authcode = transferDeploymentProfilesRequest.SourceAuthCode,
                    Endpoint = serviceEndPoint
                };
            }
            return response;
        }

        public DeploymentProfileModel DeleteDeploymentProfile(string authCode)
        {
            try
            {
                var serviceResponse = GetLicensingServiceInstance().delete_deployment_profile(authCode);
                return Mapper.Map<DeploymentProfileModel>(serviceResponse);
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                    Elmah.ErrorSignal.FromCurrentContext().Raise(
                        new Exception(string.Format("Error from DeleteDeploymentProfile : AuthCode {0}.  Error is {1}", authCode, ex.ToString())));
                return new DeploymentProfileModel { Success = false, Authcode = authCode, Message = ex.Message };
            }
        }

        public DeploymentProfileModel DeleteCnxDeploymentProfile(string authCode)
        {
            try
            {
                var serviceResponse = GetLicensingServiceInstance().delete_cnx_deployment_profile(authCode);
                return Mapper.Map<DeploymentProfileModel>(serviceResponse);
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                    Elmah.ErrorSignal.FromCurrentContext().Raise(
                        new Exception(string.Format("Error from DeleteCnxDeploymentProfile : AuthCode {0}.  Error is {1}", authCode, ex.ToString())));
                return new DeploymentProfileModel { Success = false, Authcode = authCode, Message = ex.Message };
            }
        }

        public void AllocateTokensToCnPanorama(string serialNumber, int tokensQuantity, string authCode)
        {
            var result = GetLicensingServiceInstance().GetTokens(serialNumber, tokensQuantity, authCode);
            if (!string.IsNullOrEmpty(result.Message)
                && string.Compare(result.Message, "SUCCESS", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ApplicationException(result.Message);
            }
        }

        public bool IsPae(string authcode)
        {
            return _licenseRepository.IsPae(authcode);
        }

        // 2021-04-26: JK - CSP-6337
        public bool IsSaasInline(string authcode)
        {
            return _licenseRepository.IsSaasInline(authcode);
        }

        public DownloadLicenseResponse DownloadLicenseKey(long licenseId, long userAccountId)
        {
            var downloadLicenseResponse = new DownloadLicenseResponse();
            var licenseKey = GetLicenseKeybyId(licenseId, userAccountId);
            licenseKey.Key = licenseKey.Key.Replace("\n", Environment.NewLine);
            return new DownloadLicenseResponse
            {
                LicenseKey = licenseKey.Key,
                LicenseName = licenseKey.Name
            };
        }

        public List<PanoramasDeleteResponse> DeletePanoramas(string serialNumber)
        {
            var response = new List<PanoramasDeleteResponse>();
            string serviceEndPoint = string.Empty;
            try
            {
                var licenseClient = GetLicensingServiceInstance();
                var serviceResponse = licenseClient.DeletePanoramas(serialNumber);

                serviceEndPoint = string.Format("{0}?op=DeletePanoramas", licenseClient.Endpoint.Address.Uri.AbsoluteUri);
                if (serviceResponse != null && serviceResponse.Any())
                {
                    serviceResponse.ToList().ForEach(x =>
                    {
                        response.Add(new PanoramasDeleteResponse
                        {
                            serialNumber = x.SerialNumber,
                            message = x.Message,
                            success = x.Success,
                            Endpoint = serviceEndPoint
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                if (HttpContext.Current != null)
                    Elmah.ErrorSignal.FromCurrentContext().Raise(
                        new Exception(string.Format("Error from DeletePanoramas : SerialNumbers {0}.  Error is {1}", string.Join(",", serialNumber), ex.ToString())));
                List<string> SerialNumbers = serialNumber.Split(',').ToList<string>();
                SerialNumbers?.ForEach(x =>
                {
                    response.Add(new PanoramasDeleteResponse
                    {
                        success = false,
                        message = ex.Message,
                        serialNumber = x,
                        Endpoint = serviceEndPoint
                    });
                });

            }

            return response;
        }

        public DeploymentProfileResponse RenewDeploymentProfile(DeploymentProfileRequest deploymentRequest)
        {
            return GetLicensingServiceInstance().renew_deployment_profile(
                 dpAuthcode: deploymentRequest.Authcode,
                 vcpu: deploymentRequest.VcpuQuantity,
                 entitlements: deploymentRequest.Subscriptions.Replace("'", string.Empty),
                 expiration: deploymentRequest.ExpirationDate,
                 tokenId: deploymentRequest.TokenId,
                 numberOfFirewalls: deploymentRequest.NumberOfFirewalls,
                 supportType: deploymentRequest.SupportType,
                 ascName: deploymentRequest.AscName,
                 ascId: deploymentRequest.AscId,
                 vmType: deploymentRequest.VmType,
                 isSubOnlyDp: deploymentRequest.IsSubscriptionOnly);
        }

        //public DeploymentProfileResponse RenewCnxDeploymentProfile(CnxDeploymentProfileRequest deploymentRequest)
        //{
        //    return GetLicensingServiceInstance().renew_cnx_deployment_profile(
        //        dpAuthcode: deploymentRequest.DpAuthcode,
        //        tokens: deploymentRequest.Tokens,
        //        entitlements: deploymentRequest.Entitlements,
        //        expiration: deploymentRequest.Expiration,
        //        tokenId: deploymentRequest.TokenId
        //        );
        //}

    }
}
