using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Wildfire;
using Csp.Api.Services.Contracts;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Csp.Api.HelperClasses;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using static Csp.Api.HelperClasses.WebCommon;

namespace Csp.Api.Logic
{
    public class WildfireLogic : BaseLogic, IWildfireLogic
    {
        private const string MESSAGE_SUCCESS = "success";
        private const string WF_API_PRODUCT_ID = "78ee1a30-7576-46eb-b363-e836f66ca0b9";
        private readonly string AES_KEY_256 = ConfigurationManager.AppSettings.Get("AesKey256");
        private readonly string AES_ENCRYPTION_KEY = ConfigurationManager.AppSettings["AESEncryptionKey"];
        private readonly bool IsFedramp = ConfigurationManager.AppSettings["AppEnvVerify"]?.ToUpper().StartsWith("FED") ?? false;

        private readonly IWildfireDataAccess wildfireDataAccess = null;
        private readonly IApplicationSettings applicationSettings = null;

        public WildfireLogic(IWildfireDataAccess wildfireDataAccess, IApplicationSettings applicationSettings)
        {
            this.wildfireDataAccess = wildfireDataAccess;
            this.applicationSettings = applicationSettings;
        }

        public WildfireAssetsResponse<WildfireAssetItem> GetApiAssets(WildfireApiRequest request)
        {
            List<WildfireAssetItem> items = new List<WildfireAssetItem>();
            try
            {
                if (request == null)
                {
                    throw new ApplicationException("WF API GetApiAssets Request cannot be NULL.");
                }

                string graphQlRequest = @"
{
    ""query"":""query ($supportAccountId: [Long], $productId: [String], $status: [String]) { 
        license(supportAccountId:$supportAccountId, productId:$productId, status:$status) {
            salesAccountId,
            supportAccountId,
            supportType,
            entitlements {
                productId,
                entitlementId,
                serialNumber,
                isTrial,
                productId,
                sku,
                licenseData,
                productName,
                endDate,
                productType
            }
        }
    }"",
    ""variables"": {
        ""supportAccountId"":{0},
        ""productId"":""{1}"",
        ""status"":""ACTIVE""
    }
}".Replace("\r\n", string.Empty).Replace(" ", string.Empty).Replace("{0}", request.SupportAccountId.ToString()).Replace("{1}", WF_API_PRODUCT_ID);
                WildfireApiNova responsePayload = NovaApis.GetXsoarMarketplaceData<WildfireApiNova>(graphQlRequest);
                if (responsePayload == null)
                {
                    throw new ApplicationException("Failed to get WF API API Assets for Support Account ID " + request.SupportAccountId + ". Query: " + graphQlRequest);
                }

                items = responsePayload.MapToAssets().ToList();

                if (!string.IsNullOrWhiteSpace(request.SearchValue)
                    && !string.IsNullOrWhiteSpace(request.SearchField))
                {
                    if (request.SearchField == "serialNumber")
                    {
                        items = items.Where(i => i.SerialNumber == request.SearchValue).ToList();
                    }
                    else if (request.SearchField == "modelName")
                    {
                        string modelName = request.SearchValue.Trim().ToUpper();
                        items = items.Where(i => i.ModelName?.ToUpper() == modelName).ToList();
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.SortField)
                    && (request.SortOrder == "ASC"
                    || request.SortOrder == "DESC"))
                {
                    if (request.SortField == "serialNumber")
                    {
                        items = request.SortOrder == "ASC"
                            ? items.OrderBy(i => i.SerialNumber).ToList()
                            : items.OrderByDescending(i => i.SerialNumber).ToList();
                    }
                    else if (request.SortField == "modelName")
                    {
                        items = request.SortOrder == "ASC"
                            ? items.OrderBy(i => i.ModelName).ToList()
                            : items.OrderByDescending(i => i.ModelName).ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }

            return new WildfireAssetsResponse<WildfireAssetItem>
            {
                Items = items.Skip((request.Page - 1) * request.PerPage).Take(request.PerPage).ToArray(),
                Total = items.Count()
            };
        }

        public WildfireApiActivationDetailsResponse GetActivationDetails(WildfireApiActivationDetailsRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ApplicationException("WF API GetActivationDetails Details Request cannot be NULL.");
                }

                if (string.IsNullOrWhiteSpace(request.Hash))
                {
                    throw new ApplicationException("WF API GetActivationDetails Details Request HASH is required.");
                }

                string hash = WebUtility.UrlDecode(request.Hash).Replace(" ", "+");
                string payload = IsFedramp // CSP FEDRAMP HIGH deployment requires FIPS compliant cryptographic algorithm decoding
                    ? Decrypt(hash)
                    : DecryptRijndael(hash);
                if (string.IsNullOrWhiteSpace(payload))
                {
                    throw new ApplicationException("WF API GetActivationDetails Details Payload is required. Hash: '" + request.Hash + "'.");
                }
                // CSP-16976 CDSS: FedRamp Phase 1
                if (payload.Contains("\"product_type\"") // Product Type driven workflow
                    && IsFedramp)
                {
                    #region Product Type hash based code: FEDRAMP HIGH only
                    WildfireApiActivationDetailsRequestData requestData = JsonConvert.DeserializeObject<WildfireApiActivationDetailsRequestData>(payload);

                    if (requestData == null)
                    {
                        throw new ApplicationException("WF API GetActivationDetails Details Request HASH Payload is invalid.");
                    }

                    requestData.Validate();
                    // Call Commercial end-point to get Authcode info
                    WildfireActivationLicense deatils = null;

                    string url = ConfigurationManager.AppSettings["CspApiCommercial"] + "/api/WildfireApi/GetActivationDetailsBasics?authCode=" + requestData.AuthCode + "&productType=" + (int)requestData.ProductType;
                    using (var httpResponse = (HttpWebResponse)WebCommon.GetWebRequest(url, "GET").GetResponse())
                    {
                        if (httpResponse.StatusCode != HttpStatusCode.OK)
                        {
                            return WildfireApiActivationDetailsResponse.SetError("WF API Commercial GetActivationDetailsBasics Details Request failed. Authcode '" + requestData.AuthCode + "'.");
                        }
                        else
                        {
                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                string resultString = reader.ReadToEnd();
                                deatils = JsonConvert.DeserializeObject<WildfireActivationLicense>(resultString, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    MissingMemberHandling = MissingMemberHandling.Ignore
                                });
                            }
                        }
                    }

                    if (deatils == null)
                    {
                        return WildfireApiActivationDetailsResponse.SetError("The Authorization Code '" + requestData.AuthCode + "' license details are missing.");
                    }

                    if (string.IsNullOrWhiteSpace(deatils.SalesforceAccountId))
                    {
                        return WildfireApiActivationDetailsResponse.SetError("The Authorization Code '" + requestData.AuthCode + "' license SalesforceAccountId is missing.");
                    }

                    // Get FEDRAMP SFDC Account ID
                    if (IsFedramp) // FED env only
                    {
                        deatils.SalesforceAccountId = wildfireDataAccess.GetSfdcMappedAccountId(deatils.SalesforceAccountId);
                    }
                    WildfireApiActivationDetailsResponse result = deatils.Validate(requestData.ProductType, null, requestData.AuthCode, wildfireDataAccess, request.UserAccountId, null, payload);
                    return result;
                    #endregion
                }
                else if (IsFedramp)
                {
                    return WildfireApiActivationDetailsResponse.SetError("WF API call is for FEDRAMP HIGH only.");
                }
                else // Backward compatibility w/ CSP-8587 (Serial Number only).
                {
                    #region SN hash based code
                    string serialNumber = payload.Split(',').Distinct().FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(serialNumber))
                    {
                        throw new ApplicationException("WF API GetActivationDetails Details Request Serial Number is required. Hash: " + request.Hash);
                    }

                    string graphQlRequest = @"
{
    ""query"":""query ($productId: [String], $serialNumber: [String], $status: [String]) { 
        license(productId:$productId, serialNumber:$serialNumber, status:$status) {
            salesAccountId,
            supportAccountId,
            supportType,
            entitlements {
                productId,
                entitlementId,
                serialNumber,
                isTrial,
                productId,
                sku,
                licenseData,
                productName,
                endDate,
                productType
            }
        }
    }"",
    ""variables"": {
        ""serialNumber"":""{0}"",
        ""productId"":""{1}"",
        ""status"":""ACTIVE""
    }
}".Replace("\r\n", string.Empty).Replace(" ", string.Empty).Replace("{0}", serialNumber).Replace("{1}", WF_API_PRODUCT_ID);
                    WildfireApiNova responsePayload = NovaApis.GetXsoarMarketplaceData<WildfireApiNova>(graphQlRequest);
                    if (responsePayload == null)
                    {
                        return WildfireApiActivationDetailsResponse.SetError("The Serial Number '" + serialNumber + "' data is unavailable.");
                    }

                    WildfireActivationLicense mappedLicense = responsePayload.Data.Licenses.SingleOrDefault()?.MapToActivationLicenses();
                    if (mappedLicense == null)
                    {
                        return WildfireApiActivationDetailsResponse.SetError("The Serial Number '" + serialNumber + "' license details are missing.");
                    }

                    return mappedLicense.Validate(WildFireApiActivationProductType.StandaloneWfApi, serialNumber, null, wildfireDataAccess, request.UserAccountId, graphQlRequest, payload);
                    #endregion
                }
            }
            catch (CryptographicException exs)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(exs);
                return WildfireApiActivationDetailsResponse.SetError("Error of Encryption of Activation Details. Please contact customer support.");
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return WildfireApiActivationDetailsResponse.SetError("Error of Getting Activation Details. Please contact customer support.");
            }
        }

        public WildfireActivationLicense GetProductTypeBasedDetails(WildfireApiActivationDetailsRequestData data)
        {
            return wildfireDataAccess.GetProductTypeBasedDetails(data);
        }

        public WildfireApiActivateResponse Activate(WildfireApiActivateRequest request)
        {
            try
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("WF API Activate log> request: '" + JsonConvert.SerializeObject(request) + "'."));
                if (request.ProductType == WildFireApiActivationProductType.StandaloneWfApi)
                {
                    #region SN hash based code
                    string url = ConfigurationManager.AppSettings["LicenseApi"] + "/coresvc/v1/product/activate";
                    string thumbprint = ConfigurationManager.AppSettings["LicenseApiThumbprint"];
                    string payload = JsonConvert.SerializeObject(new { serial_number = request.SerialNumber, support_account_id = request.CspId });
                    using (var httpResponse = (HttpWebResponse)WebCommon.GetWebRequestWithClientCertificate(url, "POST", thumbprint, payload).GetResponse())
                    {
                        if (httpResponse.StatusCode != HttpStatusCode.OK)
                        {
                            return WildfireApiActivateResponse.SetError("Failed to Activate the WF API Serial Number '" + request?.SerialNumber + "' license to Support Account '" + request?.CspId + "'.");
                        }
                        else
                        {
                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                string resultString = reader.ReadToEnd();
                                dynamic result = JsonConvert.DeserializeObject<dynamic>(resultString, new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore,
                                    MissingMemberHandling = MissingMemberHandling.Ignore
                                });
                                JObject additionalInfo = result is JObject && ((JObject)result)["additional_info"] != null ? ((JObject)result)["additional_info"] as JObject : null;
                                return new WildfireApiActivateResponse
                                {
                                    Success = true,
                                    Message = "The license is successfully activated.",
                                    SerialNumber = request.SerialNumber,
                                    Quantity = additionalInfo != null
                                        ? new
                                        {
                                            queries = ((int)additionalInfo["queryMaxLimit"]).ToString("N0"),
                                            submissions = ((int)additionalInfo["uploadMaxLimit"]).ToString("N0")
                                        }
                                        : null
                                };
                            }
                        }
                    }
                    #endregion 
                }
                else if (request.ProductType == WildFireApiActivationProductType.FedrampSupportAWF)
                {
                    #region Product Type hash based code
                    string url = ConfigurationManager.AppSettings["LicenseApi"] + "/coresvc/v1/fedrampSupportAWF/activate";
                    string thumbprint = ConfigurationManager.AppSettings["LicenseApiThumbprint"];
                    string payload = JsonConvert.SerializeObject(new
                    {
                        product_type = request.ProductType.ToString(),
                        auth_code = request.AuthCode,
                        support_account_id = request.CspId,
                        user_account_id = request.UserAccountId
                    });
                    using (var httpResponse = (HttpWebResponse)WebCommon.GetWebRequestWithClientCertificate(url, "POST", thumbprint, payload).GetResponse())
                    {
                        if (httpResponse.StatusCode != HttpStatusCode.OK)
                        {
                            return WildfireApiActivateResponse.SetError("Failed to Activate the Fedramp High AWF AuthCode '" + request?.AuthCode + "' license to Support Account '" + request?.CspId + "'.");
                        }
                        else
                        {
                            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                string resultString = reader.ReadToEnd();
                                if (resultString.Contains("\"success\":"))
                                {
                                    dynamic resultObject = JsonConvert.DeserializeObject<dynamic>(resultString, new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore,
                                        MissingMemberHandling = MissingMemberHandling.Ignore
                                    });
                                    string message = resultObject is JObject && ((JObject)resultObject)["message"] != null ? ((JObject)resultObject)["message"].ToString() : null;
                                    if (!(bool)resultObject.success)
                                    {
                                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error of Activating Product response> Product Type '" + request.ProductType + "'. Details: '" + resultString + "'"));
                                    }
                                    return new WildfireApiActivateResponse
                                    {
                                        Success = (bool)resultObject.success,
                                        Message = message,
                                        AuthCode = request.AuthCode
                                    };
                                }
                                else
                                {
                                    Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error of Activating Product response unexpected data> Product Type '" + request.ProductType + "'. Details: '" + resultString + "'"));
                                    return new WildfireApiActivateResponse
                                    {
                                        Success = false,
                                        Message = "Error of Activating Product '" + request.ProductType + "' license.",
                                        AuthCode = request.AuthCode
                                    };
                                }
                            }
                        }
                    }
                    #endregion 
                }
                else
                {
                    return WildfireApiActivateResponse.SetError("Error of Activating Product> Invalid Product Type '" + request.ProductType + "'.");
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Error of Activating Product> Payload '" + JsonConvert.SerializeObject(request) + "'.", ex));
                return WildfireApiActivateResponse.SetError("Error of Activating Product. Please contact customer support.");
            }
        }

        public WildfireAssetsResponse<WildfirePortalItem> GetPortals(WildfireApiRequest request)
        {
            if (request == null)
            {
                Exception ex = new ApplicationException("WF API GetPortals Request cannot be NULL.");
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw ex;
            }

            return wildfireDataAccess.GetPortals(request);
        }

        public dynamic GetWildfireKeys(long supportAccountId)
        {
            var method = HttpMethod.Get.Method;
            string baseUrl = string.Format(ApiConstants.WFApiKeyEndpoint, applicationSettings.WFAPIBaseURL, supportAccountId);

            var responseWildfireKeyList = CommonHelper.ProcessWebRequest<WildfireApiKeysResponseDetails>(baseUrl, method, string.Empty, string.Empty, true, cnName: applicationSettings.WFAPICertCnName);
            var result = responseWildfireKeyList.ResponseObject.WildfireAPIkeys.Where(a => a.ProductType.ToUpper() == "WILDFIRE" || a.ProductType.ToUpper() == "WFAPISTANDALONE" || a.ProductType.ToUpper() == "ADVWILDFIRE").ToList();

            var WildfireApiKeyList = new WildfireApiKeyList()
            {
                WildfireAPIkeys = result,
                Total = result.Count()
            };
            return WildfireApiKeyList;
        }

        public WfApiConsumptionHealh CheckConsumptionHealth()
        {
            try
            {
                string payload = "{ \"url\": \"" + applicationSettings.WfApiAnalyticsHealthCheckUrl + "\" }";
                dynamic result = QueryWfApiGet<dynamic>("/internalapi/submit_sample", HttpMethod.Post, payload, applicationSettings.WfApiAnalyticsHealthCheckApiKey);
                if (!(Convert.ToBoolean(result?.success?.ToString()) ?? false)
                    || result?.result?.error?["error-message"]?.ToString() != null)
                {
                    return new WfApiConsumptionHealh
                    {
                        Operational = false,
                        Message = "The endpoint is not operational. ",
                        Details = result
                    };
                }

                return new WfApiConsumptionHealh
                {
                    Operational = true,
                    Message = "The endpoint is operational.",
                    Details = result
                };
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new WfApiConsumptionHealh
                {
                    Operational = false,
                    Message = "Error of checking Consumption Health.",
                    Details = ex.Message
                };
            }
        }

        public WfApiConsumptionSettings GetConsumptionSettings(long supportAccountId)
        {
            try
            {
                WfApiExtConsumptionResponseResult result = GetConsumptionDataInternal(new WfApiConsumptionRequest
                {
                    SupportAccountId = supportAccountId
                });
                if (result?.Status != MESSAGE_SUCCESS)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(new ApplicationException("Error of fetching Consumption Data for Support Account ID '" + supportAccountId + "'."));
                    return new WfApiConsumptionSettings
                    {
                        Success = false,
                        Message = result?.Error?.Message ?? ("Response Error of fetching Consumption Settings for Support Account ID '" + supportAccountId + "'.")
                    };
                }

                return new WfApiConsumptionSettings
                {
                    Success = true,
                    SupportAccountId = Convert.ToInt64(result.CspId),
                    ApiKeys = result.ApiKeys?.GroupBy(g => g.ApiKey).Select(i => i.Key).Distinct().ToArray() ?? new string[0]
                };
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new WfApiConsumptionSettings
                {
                    Success = false,
                    Message = "Error of fetching Consumption Settings for Support Account ID '" + supportAccountId + "'."
                };
            }
        }

        public WfApiConsumptionResponse GetConsumptionData(WfApiConsumptionRequest request)
        {
            try
            {
                WfApiExtConsumptionResponseResult result = GetConsumptionDataInternal(request);
                if (result?.Status != MESSAGE_SUCCESS)
                {
                    return new WfApiConsumptionResponse
                    {
                        Success = false,
                        Message = result?.Error?.Message ?? ("Response Error of fetching Consumption Data for Support Account ID '" + request.SupportAccountId + "'.")
                    };
                }

                IEnumerable<WfApiExtConsumptionResponseResultItem> apiKeys = request.ApiKeys?.Any() ?? false
                    ? result.ApiKeys.Where(i => request.ApiKeys.Any(k => string.Compare(k, i.ApiKey, true) == 0)).ToArray() // filter by requested API Keys
                    : result.ApiKeys?.ToArray() ?? new WfApiExtConsumptionResponseResultItem[0]; // no filter by requested API Keys
                IEnumerable<WfApiExtConsumptionResponseResultItemUsageData> data = !string.IsNullOrWhiteSpace(request?.Type)
                    ? apiKeys.Where(i => i.Usage != null).SelectMany(i => i.Usage.Where(u => string.Compare(u.Type, request?.Type, true) == 0).SelectMany(u => u.Data)).ToArray()
                    : apiKeys.Where(i => i.Usage != null).SelectMany(i => i.Usage).SelectMany(u => u.Data).ToArray();
                IEnumerable<WfApiConsumptionResponseItemBase> groupped = data.GroupBy(g => g.Date)
                    .Select(i => new WfApiConsumptionResponseItemBase
                    {
                        Date = i.Key,
                        QueryLimit = i.Sum(p => p.QueryLimit),
                        QueryUsage = i.Sum(p => p.QueryUsage),
                        UploadLimit = i.Sum(p => p.UploadLimit),
                        UploadUsage = i.Sum(p => p.UploadUsage)
                    }).OrderBy(i => i.Date).ToArray();
                return new WfApiConsumptionResponse
                {
                    Success = true,
                    SupportAccountId = Convert.ToInt64(result.CspId),
                    Items = groupped
                };
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return new WfApiConsumptionResponse
                {
                    Success = false,
                    Message = "Error of fetching Consumption Data for payload: '" + JsonConvert.SerializeObject(request) + "'."
                };
            }
        }

        private string DecryptRijndael(string value)
        {
            using (RijndaelManaged cipher = new RijndaelManaged())
            {
                cipher.Key = Encoding.UTF8.GetBytes(AES_KEY_256);
                cipher.IV = new byte[16];
                cipher.Mode = CipherMode.ECB;
                cipher.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform decryptor = cipher.CreateDecryptor())
                {
                    using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(value)))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }

        public string Encrypt(string clearText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.ASCII.GetBytes(AES_ENCRYPTION_KEY);
                aesAlg.IV = new byte[16];
                using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                {
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(clearText);
                            }
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
        }

        public string Decrypt(string clearText)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.ASCII.GetBytes(AES_ENCRYPTION_KEY);
                aesAlg.IV = new byte[16];
                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor())
                {
                    using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(clearText)))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }

        private WfApiExtConsumptionResponseResult GetConsumptionDataInternal(WfApiConsumptionRequest request)
        {
            string payload = JsonConvert.SerializeObject(new WfApiExtConsumptionRequest
            {
                CspId = request.SupportAccountId.ToString(),
                ApiKey = applicationSettings.WfApiAnalyticsApiKey,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            });
            return QueryWfApiGet<WfApiExtConsumptionResponse>("/supportapi/apikey/usage", HttpMethod.Post, payload)?.Result;
        }

        private T QueryWfApiGet<T>(string relativeUrl, HttpMethod httpMethod, string payload = null, string apiKey = null)
        {
            try
            {
                string url = applicationSettings.WfApiAnalyticsHost + relativeUrl;
                HttpWebRequest request = WebCommon.GetWebRequest(url, httpMethod.Method, "application/json");
                request.Accept = "*/*";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(payload);
                }
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    request.Headers.Add("apiKey", apiKey);
                }
                using (var response = request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string jsonResponse = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(jsonResponse))
                        {
                            return JsonConvert.DeserializeObject<T>(jsonResponse, new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            });
                        }
                    }
                }
                return default(T);
            }
            catch (WebException wex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(wex);
                using (var reader = new StreamReader(wex.Response.GetResponseStream()))
                {
                    string jsonResponse = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(jsonResponse))
                    {
                        return JsonConvert.DeserializeObject<T>(jsonResponse, new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        });
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }
    }
}