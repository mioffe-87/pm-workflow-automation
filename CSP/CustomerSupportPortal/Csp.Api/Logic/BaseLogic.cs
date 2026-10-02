using Csp.Api.DeviceAndLicenseService;
using Csp.Api.HelperClasses;
using System;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;

namespace Csp.Api.Logic {
	public class BaseLogic {

		public static string CalculateMd5Hash(string input) {
			// Calculate MD5 hash from input
			var md5 = MD5.Create();
			var inputBytes = Encoding.ASCII.GetBytes(input);
			var hash = md5.ComputeHash(inputBytes);

			// Convert byte array to lowercase hex string
			var sb = new StringBuilder();
			for (var i = 0; i < hash.Length; i++) {
				sb.Append(hash[i].ToString("x2"));	// Use X2 for Uppercase
			}
			return sb.ToString();
		}

        protected LicenseClient GetDeviceAndLicenseServiceInstance()
        {
            try
            {
                var licenseService = new LicenseClient();

                bool isPanServiceBasicAuthEnabled = ConfigurationManager.AppSettings["PanServicesIsBasicAuthEnabled"].ToUpper() == "TRUE";
                string panServiceCertThumbprint = (isPanServiceBasicAuthEnabled) ? string.Empty : ConfigurationManager.AppSettings["PanServicesCertThumbprint"];
                var userName = ConfigurationManager.AppSettings["PanServicesUserName"];
                var userKey = ConfigurationManager.AppSettings["PanServicesUserKey"];
                var panServiceUrl = ConfigurationManager.AppSettings["PanServicesUrl"];

                if (string.IsNullOrEmpty(panServiceCertThumbprint))
                {

                    if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userKey))
                    {
                        var basicHttpBinding = new BasicHttpBinding
                        {
                            Security =
                            {
                                Mode = BasicHttpSecurityMode.Transport,
                                Transport = { ClientCredentialType = HttpClientCredentialType.Basic }
                            }
                        };

                        licenseService = new LicenseClient(basicHttpBinding, new EndpointAddress(panServiceUrl + "/License.svc"));

                        var clientCredentials = new ClientCredentials();

                        clientCredentials.UserName.UserName = userName;
                        clientCredentials.UserName.Password = userKey;
                        licenseService.ChannelFactory.Endpoint.Behaviors.Remove<ClientCredentials>();
                        licenseService.ChannelFactory.Endpoint.Behaviors.Add(clientCredentials);
                    }

                    return licenseService;
                }

                var certBasicHttpBinding = new BasicHttpBinding
                {
                    Security =
                    {
                        Mode = BasicHttpSecurityMode.Transport,
                        Transport = { ClientCredentialType = HttpClientCredentialType.Certificate }
                    }
                };

                licenseService = new LicenseClient(certBasicHttpBinding, new EndpointAddress(panServiceUrl + "/License.svc"));
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                       | SecurityProtocolType.Tls11
                                                       | SecurityProtocolType.Tls12
                                                       | SecurityProtocolType.Ssl3;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var clientCertificate = CertificateManager.GetCertificateByCnName(panServiceCertThumbprint);

                if (clientCertificate == null)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Empty certificate store"));
                }
                else
                {
                    var clientCredentials = new ClientCredentials();
                    clientCredentials.ClientCertificate.Certificate = clientCertificate;
                    licenseService.ChannelFactory.Endpoint.Behaviors.Remove<ClientCredentials>();
                    licenseService.ChannelFactory.Endpoint.Behaviors.Add(clientCredentials);

                }
                return licenseService;
            }
            catch (Exception ex)           {
               
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error occured in GetDeviceAndLicenseServiceInstance method in BaseLogic" + ex.Message, ex));
                throw;
            }
        }

        /// <summary> 
        ///     Calling panservices CallDevOpsApi method which will constuct request payload 
        ///     and call the HUB.
        /// </summary>
        /// <param name="cloudAuthcode"></param>
        /// <param name="region"></param>
        /// <param name="reason"></param>
        public void CallDevOpsApi(string cloudAuthcode, string region, string reason)
        {
            try
            {
                var t = new Thread(() => GetDeviceAndLicenseServiceInstance().CallDevOpsApi(cloudAuthcode, region, reason));
                t.Start();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error occured in CallDevOpsApi method in Pan services" + ex.Message, ex));
            }
        }
    }
}
