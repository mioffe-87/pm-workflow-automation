using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;


namespace Csp.Api.Logic
{
    public class SfdcEntitlementLogic : ISfdcEntitlementLogic
    {
       
        private readonly string host = ConfigurationManager.AppSettings["NovaHost"].ToString();
        private readonly string client_id = ConfigurationManager.AppSettings["NovaClientId"].ToString();
        private readonly string client_secret = ConfigurationManager.AppSettings["NovaClientSecret"].ToString();
        private readonly string username = ConfigurationManager.AppSettings["NovaUserName"].ToString();
        private readonly string password = ConfigurationManager.AppSettings["NovaPassword"].ToString();
        private readonly string endpoint = ConfigurationManager.AppSettings["NovaEntEndpoint"].ToString();
        private readonly string enthost = ConfigurationManager.AppSettings["NovaEntHost"].ToString();

        public NovaSfdcToken GetToken()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            NovaSfdcToken accessToken = null;
            string url = string.Format("{0}?grant_type=password&client_id={1}&client_secret={2}&username={3}&password={4}", host, client_id,client_secret, username,password);
            WebRequest request = HttpWebRequest.Create(url);
            request.Method = "POST";
            using (WebResponse responseToken = request.GetResponse())
            {
                using (Stream dataStream = responseToken.GetResponseStream())
                {
                    using (StreamReader readerToken = new StreamReader(dataStream))
                    {
                        accessToken = JsonConvert.DeserializeObject<NovaSfdcToken>(readerToken.ReadToEnd());
                        readerToken.Close();
                    }
                    dataStream.Close();
                }
                responseToken.Close();
            }
            return accessToken;
        }

        public IEnumerable<NovaEntitlement> GetEntitlements(long supportAccountId, string ascSalesForceAccountId = "")
        {            
            var accessToken = GetToken();
            IEnumerable<NovaEntitlement> entitlements = null;
            string url = enthost + endpoint +"?status=Active";
            string requestBody = string.Empty;
            WebRequest request = HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Bearer " + accessToken.AccessToken);
            if (!string.IsNullOrWhiteSpace(ascSalesForceAccountId))
            {
                requestBody = " \"ascAccountId\": \"" + ascSalesForceAccountId + "\", ";
            }            
            requestBody = "{ " + requestBody + " \"cspid\": \"" + supportAccountId.ToString() + "\" }";
            var bodyData = Encoding.Default.GetBytes(requestBody);
            request.ContentLength = bodyData.Length;
            var requestStream = request.GetRequestStream();
            requestStream.Write(bodyData, 0, bodyData.Length);
            requestStream.Close();

            using (WebResponse responseEntitlements = request.GetResponse())
            {                
                using (Stream dataStream = responseEntitlements.GetResponseStream())
                {
                    using (StreamReader readerEntitlements = new StreamReader(dataStream))
                    {
                        entitlements = JsonConvert.DeserializeObject<NovaEntitlement[]>(readerEntitlements.ReadToEnd()).Where(p=>p.Status == "Active");
                        readerEntitlements.Close();
                    }
                    dataStream.Close();
                }
                responseEntitlements.Close();
            }
            return entitlements;
        }

        public IEnumerable<AvailableSaaSActivations> AddNovaXDREntitlements(IEnumerable<AvailableSaaSActivations> entitlements, long supportAccountId,string ascSalesforceAccountId = "")
        {
            var lstEntitlements = entitlements.ToList();
            var novaEntitlements = GetEntitlements(supportAccountId,ascSalesforceAccountId);
            var novaActivations = new List<AvailableSaaSActivations>();
            if (novaEntitlements.Count() > 0)
            {
                foreach (var nentitlement in novaEntitlements)
                {
                    if (nentitlement.Product != null && nentitlement.SerialNumber != null)
                    {
                        if (nentitlement.Product.Sku != null && nentitlement.Product.Sku.Contains("XDR"))
                        {
                            if (!lstEntitlements.Where(p => p.SerialNumber == nentitlement.SerialNumber).Any() && !novaActivations.Where(p => p.SerialNumber == nentitlement.SerialNumber).Any())
                            {
                                string quantity = "", description = "";
                                var featureEntitlements = novaEntitlements.Where(p => p.SerialNumber == nentitlement.SerialNumber).OrderBy(p => p.SfdcEntitlementId);
                                var cloudEntitlement = new AvailableSaaSActivations();
                                cloudEntitlement.SerialNumber = nentitlement.SerialNumber;
                                cloudEntitlement.AuthorizationCode = "N/A";
                                cloudEntitlement.ModelName = nentitlement.Product.AppName != null ? nentitlement.Product.AppName:"";
                                foreach (var fentitlements in featureEntitlements)
                                {
                                    if (fentitlements.Product.LicensingFeatures != null && fentitlements.Product.LicensingFeatures.Any())
                                    {
                                        quantity = quantity + "<div class=''>" + fentitlements.Product.LicensingFeatures.FirstOrDefault().Quantity + "</div>\n";
                                    }
                                    if(fentitlements.Product.Name != null)
                                        description = description + "<div class=''>" + fentitlements.Product.Name + "</div>\n";
                                }
                                cloudEntitlement.Quantity = quantity;
                                cloudEntitlement.Description = description;

                                if(nentitlement.EffectivePeriod != null)
                                    cloudEntitlement.ExpirationDate = nentitlement.EffectivePeriod.EndDate != null ? DateTime.Parse(nentitlement.EffectivePeriod.EndDate).ToShortDateString() : "";
                                cloudEntitlement.Region = nentitlement.Product.Configuration != null && nentitlement.Product.Configuration.Any() ? nentitlement.Product.Configuration.FirstOrDefault().Region : "";
                                var entitlementGroup = nentitlement.EntitlementGroupId ?? "";
                                cloudEntitlement.AssoicatedDevices = novaEntitlements.Where(p => p.EntitlementGroupId == entitlementGroup && p.SerialNumber != nentitlement.SerialNumber).Any() ?
                                    novaEntitlements.Where(p => p.EntitlementGroupId == entitlementGroup && p.SerialNumber != nentitlement.SerialNumber).FirstOrDefault().SerialNumber : "";
                                novaActivations.Add(cloudEntitlement);
                            }
                        }
                    }
                }
            }
            lstEntitlements.AddRange(novaActivations);
            return lstEntitlements;
        }
    }
}
