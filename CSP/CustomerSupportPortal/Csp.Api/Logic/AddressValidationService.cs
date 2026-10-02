using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Csp.Api.Logic
{
    public class AddressValidationService : IAddressValidationService
    {
        private readonly string addressAPIURL = ConfigurationManager.AppSettings["AddressValidationService"].ToString();

        private readonly IAddressValidationDataAccess _addressValidationDataAccess;

        public AddressValidationService(IAddressValidationDataAccess addressValidationDataAccess)
        {
            _addressValidationDataAccess = addressValidationDataAccess;
        }
        public IEnumerable<AddressAPIResponse>  ValidateAddress(AddressAPIRequest addressAPIRequest)
    
        {
            List<AddressAPIResponse> addressAPIResponseList = new List<AddressAPIResponse>();
            try
            {

                string URL = addressAPIURL;
                AddressAPIRequest addressAPIreq = new AddressAPIRequest()
                {
                    Street = addressAPIRequest.Street,
                    City = addressAPIRequest.City,
                    State = addressAPIRequest.State,
                    Zip = addressAPIRequest.Zip,
                    Country = addressAPIRequest.Country,
                    CompanyName = addressAPIRequest.CompanyName,
                    Domain = addressAPIRequest.Domain
                };
                string requestData = JsonConvert.SerializeObject(addressAPIreq, Formatting.Indented, new JsonSerializerSettings
                { NullValueHandling = NullValueHandling.Ignore });

                var addressServiceCertThumbprint =
                        ConfigurationManager.AppSettings["AddressValidationServiceCertificateThumbprint"];
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = requestData.Length;
                if (!string.IsNullOrEmpty(addressServiceCertThumbprint))
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                           | SecurityProtocolType.Tls11
                                                           | SecurityProtocolType.Tls12
                                                           | SecurityProtocolType.Ssl3;

                    var clientCertificate = CertificateManager.GetCertificateByCnName(addressServiceCertThumbprint);             
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
                using (var addressServiceResponse = request.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)addressServiceResponse).StatusCode;
                    if (statusCode == HttpStatusCode.OK)
                    {
                        using (var reader = new System.IO.StreamReader(addressServiceResponse.GetResponseStream()))
                        {
                            var response = reader.ReadToEnd();
                            addressAPIResponseList = JsonConvert.DeserializeObject<List<AddressAPIResponse>>(response,
                                new JsonSerializerSettings()
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    MissingMemberHandling = MissingMemberHandling.Ignore,
                                    TypeNameHandling = TypeNameHandling.None

                                }).ToList<AddressAPIResponse>();
                        }
                    }
                }
            }


            catch (Exception ex)
            {
                var errorMsg = "Error Occured on  Address Validation API call";
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(errorMsg));              
               
            }
            return _addressValidationDataAccess.ValidateRegioncode(addressAPIResponseList);                
        }

        public bool IsAddressValidationRequiredForCountry(int countryId)
        {
            return _addressValidationDataAccess.ValidateCountryforAddressAPI(countryId);

        }

    }
}
