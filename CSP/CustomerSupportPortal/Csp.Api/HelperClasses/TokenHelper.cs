using Csp.Api.DataAccess;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Flex;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Csp.Api.HelperClasses
{
    public static class TokenHelper
    {
        public static T GetFlexData<T>(string url, string credentials, string method, string thumbPrint, string requestPayload, long userAccountId = 0, long supportAccountId = 0) where T : class
        {
            string response = string.Empty;

            try
            {
                var licensingApiRequest = WebCommon.GetWebRequestWithClientCertificate(url, method, thumbPrint, requestPayload);
                licensingApiRequest.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);

                using (var licensingApiResponse = licensingApiRequest.GetResponse())
                {
                    HttpStatusCode statusCode = ((HttpWebResponse)licensingApiResponse).StatusCode;

                    using (var reader = new StreamReader(licensingApiResponse.GetResponseStream()))
                    {
                        response = reader.ReadToEnd();
                    }

                    new FirewallFlexDataAccess().LogAuditHistory(supportAccountId, FwFlexRequestType.TokenSummary, url, requestPayload, response, userAccountId);

                    if (statusCode == HttpStatusCode.OK)
                    {
                        var result = JsonConvert.DeserializeObject<FlexBaseResponsePayload<T>>(response, new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        });

                        if (result != null && result.Errors != null)
                        {
                            throw new InvalidDataException(result.Errors.First().Message);
                        }
                        return result.Data;
                    }
                }
            }
            catch (WebException ex)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        string responseBody = reader.ReadToEnd();
                        new FirewallFlexDataAccess().LogAuditHistory(supportAccountId, FwFlexRequestType.TokenSummary, url, 
                            requestPayload, "GetFlexData: '" + responseBody + "'. EXCEPTION: " + ex.Message, userAccountId);
                    }
                }
                catch
                {
                    new FirewallFlexDataAccess().LogAuditHistory(supportAccountId, FwFlexRequestType.TokenSummary, url, requestPayload, ex.Message, userAccountId);
                }
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            catch (Exception ex)
            {
                new FirewallFlexDataAccess().LogAuditHistory(supportAccountId, FwFlexRequestType.TokenSummary, url, requestPayload, ex.Message, userAccountId);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return default(T);
        }

        public static List<T> PostAsync<T>(string url, string requestUrl, string credentials, string thumbPrint, string request, FwFlexRequestType requestType,
            long supportAccountId = 0, long userAccountId = 0) where T : class
        {
            string response = string.Empty;
            try
            {
                using (var client = new HttpClient(GetRequestHandler(thumbPrint)))
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                    var postContent = new StringContent(request, Encoding.UTF8, "application/json");

                    var result = client.PostAsync(requestUrl, postContent).Result;

                    response = result.Content.ReadAsStringAsync().Result;

                    new FirewallFlexDataAccess().LogAuditHistory(supportAccountId, requestType, requestUrl, request,
                      string.IsNullOrEmpty(response) ? result.StatusCode.ToString() : response + " - " + result.StatusCode.ToString(), userAccountId);

                    if (result.IsSuccessStatusCode)
                    {
                        return JsonConvert.DeserializeObject<List<T>>(response);
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return default(List<T>);
        }

        // TO DO - mapping table
        public static string GetCreditType(string sku, bool isEval)
        {
            if (isEval)
            {
                if (sku.ToUpper() == FwFlexCreditType.PRISMA_PROD.ToDescription())
                {
                    return FwFlexCreditType.PRISMA_EVAL.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.PRISMA_NFR_EVAL.ToDescription())
                {
                    return FwFlexCreditType.PRISMA_NFR_EVAL.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.PRISMA_SE_EVAL.ToDescription())
                {
                    return FwFlexCreditType.PRISMA_SE_EVAL.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.VIRTUAL_PROD.ToDescription())
                {
                    return FwFlexCreditType.VIRTUAL_EVAL.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.VIRTUAL_NFR_EVAL.ToDescription())
                {
                    return FwFlexCreditType.VIRTUAL_NFR_EVAL.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.VIRTUAL_SE_EVAL.ToDescription())
                {
                    return FwFlexCreditType.VIRTUAL_SE_EVAL.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SOFTWARE_EVAL.ToDescription())
                {
                    return FwFlexCreditType.SOFTWARE_EVAL.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SOFTWARE_LAB.ToDescription())
                {
                    return FwFlexCreditType.SOFTWARE_LAB.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SOFTWARE_NFR.ToDescription())
                {
                    return FwFlexCreditType.SOFTWARE_NFR.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SOFTWARE_SE_EVAL.ToDescription())
                {
                    return FwFlexCreditType.SOFTWARE_SE_EVAL.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SUBSCRIPTION_PROD.ToDescription())
                {
                    return FwFlexCreditType.SUBSCRIPTION_PROD.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SUBSCRIPTION_LAB.ToDescription())
                {
                    return FwFlexCreditType.SUBSCRIPTION_LAB.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SUBSCRIPTION_NFR.ToDescription())
                {
                    return FwFlexCreditType.SUBSCRIPTION_NFR.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SUBSCRIPTION_SE_EVAL.ToDescription())
                {
                    return FwFlexCreditType.SUBSCRIPTION_SE_EVAL.ToString();
                }

            }
            else
            {
                if (sku.ToUpper() == FwFlexCreditType.PRISMA_LAB.ToDescription())
                {
                    return FwFlexCreditType.PRISMA_LAB.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.PRISMA_NFR.ToDescription())
                {
                    return FwFlexCreditType.PRISMA_NFR.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.VIRTUAL_LAB.ToDescription())
                {
                    return FwFlexCreditType.VIRTUAL_LAB.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.VIRTUAL_NFR.ToDescription())
                {
                    return FwFlexCreditType.VIRTUAL_NFR.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.PRISMA_PROD.ToDescription() || sku.ToUpper() == FwFlexCreditType.VIRTUAL_PROD.ToDescription())
                {
                    return sku.ToUpper() == FwFlexCreditType.PRISMA_PROD.ToDescription() ? FwFlexCreditType.PRISMA_PROD.ToString()
                        : FwFlexCreditType.VIRTUAL_PROD.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SOFTWARE_PROD.ToDescription())
                {
                    return FwFlexCreditType.SOFTWARE_PROD.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SOFTWARE_LAB.ToDescription())
                {
                    return FwFlexCreditType.SOFTWARE_LAB.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SOFTWARE_NFR.ToDescription())
                {
                    return FwFlexCreditType.SOFTWARE_NFR.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SOFTWARE_SE_EVAL.ToDescription())
                {
                    return FwFlexCreditType.SOFTWARE_SE_EVAL.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.VIRTUAL_SE_EVAL.ToDescription())
                {
                    return FwFlexCreditType.VIRTUAL_SE_EVAL.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SUBSCRIPTION_PROD.ToDescription())
                {
                    return FwFlexCreditType.SUBSCRIPTION_PROD.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SUBSCRIPTION_LAB.ToDescription())
                {
                    return FwFlexCreditType.SUBSCRIPTION_LAB.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SUBSCRIPTION_NFR.ToDescription())
                {
                    return FwFlexCreditType.SUBSCRIPTION_NFR.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.SUBSCRIPTION_SE_EVAL.ToDescription())
                {
                    return FwFlexCreditType.SUBSCRIPTION_SE_EVAL.ToString();
                }
                // MSSP sku is mapped to PAN-SOFTWARE-NGFW-CR (SOFTWARE_MSSP)
                if (sku.ToUpper() == FwFlexCreditType.MSSP_PROD.ToDescription())
                {
                    return FwFlexCreditType.MSSP_PROD.ToString();
                }
                if (sku.ToUpper() == FwFlexCreditType.CLOUD_NGFW_CR.ToDescription())
                {
                    return FwFlexCreditType.CLOUD_NGFW_CR.ToString();
                }
            }
            return string.Empty;
        }

        public static string GetPoolName(string creditType)
        {

            var poolNameMapping = new Dictionary<string, string>()
            {
                {"PRISMA_PROD", "Prisma NGFW Credits" },
                {"PRISMA_NFR", "Prisma NGFW Credits for NFR" },
                {"PRISMA_EVAL", "Prisma NGFW Credits for Eval" },
                {"PRISMA_LAB", "Prisma NGFW Credits for LAB" },
                {"VIRTUAL_PROD", "Virtual NGFW Credits" },
                {"VIRTUAL_NFR", "Virtual NGFW Credits for NFR"},
                {"VIRTUAL_LAB", "Virtual NGFW Credits for LAB"},
                {"VIRTUAL_EVAL", "Virtual NGFW Credits for Eval"},
                {"PRISMA_NFR_EVAL", "Prisma NGFW Credits for NFR Eval"},
                {"VIRTUAL_NFR_EVAL", "Virtual NGFW Credits for NFR Eval"},
                {"PRISMA_SE_EVAL", "Prisma NGFW Credits for SE Eval"},
                {"VIRTUAL_SE_EVAL", "Virtual NGFW Credits for SE Eval"},
                {"SOFTWARE_PROD", "Software NGFW Credits"},
                {"MSSP_PROD", "MSSP Software NGFW Credits"},
                {"SOFTWARE_EVAL", "Software NGFW Credits for Eval"},
                {"SOFTWARE_NFR", "Software NGFW Credits for NFR"},
                {"SOFTWARE_LAB", "Software NGFW Credits for LAB"},
                {"SOFTWARE_SE_EVAL", "Software NGFW Credits for SE Eval"},
                {"SUBSCRIPTION_PROD", "Cloud-Delivered Security Services Credits"},
                {"SUBSCRIPTION_NFR", "Cloud-Delivered Security Services Credits for NFR"},
                {"SUBSCRIPTION_LAB", "Cloud-Delivered Security Services Credits LAB"},
                {"SUBSCRIPTION_SE_EVAL", "Cloud-Delivered Security Services Credits for Eval" },
                {"PAN-CLOUD-NGFW-CR", "Cloud NGFW" }
            };

            return poolNameMapping.FirstOrDefault(x => x.Key == creditType).Value;

        }
        private static WebRequestHandler GetRequestHandler(string thumbPrint)
        {
            WebRequestHandler requestHandler = new WebRequestHandler();
            X509Certificate2 certificate = null;
            certificate = CertificateManager.GetCertificateByCnName(thumbPrint);
            if (certificate != null)
                requestHandler.ClientCertificates.Add(certificate);
            return requestHandler;
        }
    }
}
