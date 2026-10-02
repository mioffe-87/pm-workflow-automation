using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;
using Csp.Api.HelperClasses;
using Newtonsoft.Json;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Api.Model.Sfdc;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Xsoar;
using System.Text.RegularExpressions;

namespace Csp.Api.Logic
{
    public class SfdcLogic : ISfdcEntitlementLogic
    {
        private const string NOVA_PRODUCT_TYPE_PRIMARY = "Primary";
        private static string GET_TOKEN_URL = "{0}?grant_type=password&client_id={1}&client_secret={2}&username={3}&password={4}";
        private readonly string HOST = ConfigurationManager.AppSettings["SfdcEntitlementHost"];
        private readonly string CLIENT_ID = ConfigurationManager.AppSettings["SfdcEntitlementClientId"];
        private readonly string CLIENT_SECRET = ConfigurationManager.AppSettings["SfdcEntitlementClientSecret"];
        private readonly string USERNAME = ConfigurationManager.AppSettings["SfdcEntitlementUserName"];
        private readonly string PASSWORD = ConfigurationManager.AppSettings["SfdcEntitlementPassword"];
        private readonly string ENTHOST = ConfigurationManager.AppSettings["SfdcEntitlementEntHost"];
        private readonly string ENDPOINT = ConfigurationManager.AppSettings["SfdcEntitlementEntEndpoint"];

        private static object locker = new object();
        private static SfdcEntitlementToken accessToken = null;

        private SfdcEntitlementToken GetToken()
        {
            if (accessToken == null)
            {
                lock (locker)
                {
                    if (accessToken == null)
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                        WebRequest request = HttpWebRequest.Create(string.Format(GET_TOKEN_URL, HOST, CLIENT_ID, CLIENT_SECRET, USERNAME, PASSWORD));
                        request.Method = "POST";
                        using (WebResponse responseToken = request.GetResponse())
                        {
                            using (Stream dataStream = responseToken.GetResponseStream())
                            {
                                using (StreamReader readerToken = new StreamReader(dataStream))
                                {
                                    //TODO: This Immediate Encoding and Decoding is done for checkmarx purpose .It doesn't serve any purpose.
                                    string encodedString = HttpUtility.HtmlEncode(readerToken.ReadToEnd());
                                    string decodedString = HttpUtility.HtmlDecode(encodedString);
                                    accessToken = JsonConvert.DeserializeObject<SfdcEntitlementToken>(decodedString, new JsonSerializerSettings()
                                    {
                                        MaxDepth = 32,
                                        TypeNameHandling = TypeNameHandling.None
                                    });
                                    readerToken.Close();
                                }
                                dataStream.Close();
                            }
                            responseToken.Close();
                        }
                    }
                }
            }
            return accessToken;
        }

        public IEnumerable<SfdcEntitlement> GetEntitlementsBySupportAccountId(SfdcEntitlementRequest sfdcEntitlementRequest, long supportAccountId = 0)
        {
                int attempts = 3; // attempts to get fresh token
                int.TryParse(ConfigurationManager.AppSettings.Get("SfdcEntitlementAttemptsToGetToken"), out attempts);
                string requestBody = string.Empty;
                while (attempts-- > 0) // if attempts counter is 0 then 401 is likely not caused by invalid token
                {
                    try
                    {
                        IEnumerable<SfdcEntitlement> entitlements = null;
                        WebRequest request = HttpWebRequest.Create(ENTHOST + ENDPOINT + "?status=Active");
                        request.Method = "POST";
                        request.ContentType = "application/json";
                        request.Headers.Add("Authorization", "Bearer " + GetToken().AccessToken);
                        requestBody = JsonConvert.SerializeObject(sfdcEntitlementRequest, Formatting.None, new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
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
                                    entitlements = JsonConvert.DeserializeObject<SfdcEntitlement[]>(readerEntitlements.ReadToEnd(), new JsonSerializerSettings()
                                    {
                                        MaxDepth = 32, TypeNameHandling = TypeNameHandling.None
                                    }); //.Where(p => p.Status == "Active");
                                    readerEntitlements.Close();
                                }
                                dataStream.Close();
                            }
                            responseEntitlements.Close();
                        }
                        return entitlements;
                    }
                    catch (WebException ex)
                    {
                        if (ex.Response is HttpWebResponse
                            && (ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
                        {
                            // the token likely has expired, make attempt to get new one from SFDC
                            accessToken = null;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("GetEntitlementsBySupportAccountId exception: all attempts failed to get entitlements by Support Account ID " + JsonConvert.SerializeObject(sfdcEntitlementRequest)));
               
            
            return new SfdcEntitlement[0];
        }
        

        public IEnumerable<AvailableSaaSActivations> AddExpanseEntitlements(IEnumerable<AvailableSaaSActivations> entitlements, long[] supportAccountIds, List<string> ascSalesforceAccountId = null, bool isAscSearchOnly = false)
        {
            var lstEntitlements = entitlements.ToList();
            var sfdcEntitlementRequest = new SfdcEntitlementRequest();

            if (supportAccountIds.Length > 0)
            {
                sfdcEntitlementRequest.CspIds = supportAccountIds.Select(i => i.ToString()).ToArray();
            }

            var sfdcEntitlementEntitlements = GetSfdcEntitlements(sfdcEntitlementRequest, supportAccountIds, ascSalesforceAccountId, isAscSearchOnly);
            if (sfdcEntitlementEntitlements == null)
            {
                return lstEntitlements;
            }
            if (ascSalesforceAccountId != null && !isAscSearchOnly)
            {
                //filter only products with current user's asc accounts
                var filteredModels = ascSalesforceAccountId.Any() ? sfdcEntitlementEntitlements.SfdcEntitlementsModels.Where(p => ascSalesforceAccountId.Contains(p.AscSfAccountId)).AsEnumerable() : sfdcEntitlementEntitlements.SfdcEntitlementsModels;
                sfdcEntitlementEntitlements.SfdcEntitlementsModels = filteredModels != null && filteredModels.Any() ? filteredModels.ToList() : null;
            }

            if (sfdcEntitlementEntitlements.SfdcEntitlementsModels != null && sfdcEntitlementEntitlements.SfdcEntitlementsModels.Any())
            {
                var novaEntitlements = new List<AvailableSaaSActivations>();
                var expanseProductID = ConfigurationManager.AppSettings["EXPANSEProductID"];
                var groups = new List<IGrouping<string, AvailableSaaSActivations>>();
                try
                {
                    groups = sfdcEntitlementEntitlements.SfdcEntitlementsModels
                        .Where(t => !string.IsNullOrWhiteSpace(t.EntitlementGroupId)).GroupBy(t => t.EntitlementGroupId)
                        .Where(w => w != null).SelectMany(w => w.Where(q => q != null && q.Entitlements != null)
                            .SelectMany(q => q.Entitlements.Where(e => e != null &&
                               !string.IsNullOrWhiteSpace(
                                   e.SerialNumber) // clean up invalid data
                               && !string.IsNullOrWhiteSpace(e.Sku)
                               && (e.ProductId.Equals(expanseProductID))
                               && !string.IsNullOrWhiteSpace(e.ProductType) &&
                               e.Status == "ACTIVE")
                                .Select(e => new AvailableSaaSActivations
                                {
                                    SupportAccountId = w.FirstOrDefault() != null
                                        ? w.First().SupportAccountId
                                        : (long?) null,
                                    SupportAccountName = w.FirstOrDefault() != null ? w.First().SalesAccountName : null,
                                    AscAccountName = w.FirstOrDefault() != null ? w.First().AscName : null,
                                    AssoicatedDevices = sfdcEntitlementEntitlements.SfdcEntitlementsModels.Any(p =>
                                        p.EntitlementGroupId == w.Key
                                        && p.Entitlements.Any(a => a.SerialNumber != e.SerialNumber))
                                        ? string.Join(" ", sfdcEntitlementEntitlements.SfdcEntitlementsModels.Where(p =>
                                                p.EntitlementGroupId == w.Key
                                                && p.Entitlements.Any(a => a.SerialNumber != e.SerialNumber))
                                            .SelectMany(
                                                f => f.Entitlements.Where(h => h.SerialNumber != e.SerialNumber))
                                            .Select(s => "<div>" + s.SerialNumber + "</div>\n").Distinct().ToArray())
                                        : "",
                                    AuthorizationCode = "N/A",
                                    BundleLicenseId = 0,
                                    BundleLicenses = new List<BundleLicenseItem>(),
                                    Capacity = "N/A",
                                    Description = e.ProductName,
                                    ExpirationDate = !string.IsNullOrWhiteSpace(e.EndDate) ? Regex.Replace(e.EndDate, "T[Zz0-9\\:\\-]*", string.Empty, RegexOptions.IgnoreCase) : string.Empty,
                                    GroupId = 0,
                                    ModelName = e.Sku,
                                    ProductType = e.ProductType,
                                    Quantity = e.LicensingFeatures != null && e.LicensingFeatures.Any()
                                        ? e.LicensingFeatures.FirstOrDefault().Quantity + " " +
                                          e.LicensingFeatures.FirstOrDefault().UnitOfMeasure
                                        : string.Empty,
                                    Region = e.Configuration != null && e.Configuration.Any()
                                        ? e.Configuration.FirstOrDefault().Region
                                        : string.Empty,
                                    SerialNumber = e.SerialNumber,
                                    AssetType = EAllAssetType.Other
                                })
                                .GroupBy(e => e.SerialNumber) // get Primary/Children groups
                                .Where(g => g.Any(c => c.ProductType == NOVA_PRODUCT_TYPE_PRIMARY))))
                        .ToList(); // Clear out groups w/ no Primary; Primary must exist in group
                    // CSP-4798_XDR_CDL_not_showing_Cloud_Services is not required in this implementation
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }

                groups.ForEach(g =>
                {
                    try
                    {
                        var primary = g.SingleOrDefault(c => c.ProductType == NOVA_PRODUCT_TYPE_PRIMARY);
                        if (primary != null)
                        {
                            primary.Children = g.Where(c => c.ProductType != NOVA_PRODUCT_TYPE_PRIMARY).Select(c => new BundleLicenseItemBase
                            {
                                AuthorizationCode = c.AuthorizationCode,
                                BundleLicenseId = c.BundleLicenseId.HasValue ? c.BundleLicenseId.Value : 0,
                                Capacity = c.Capacity,
                                Description = c.Description,
                                ExpirationDate = c.ExpirationDate,
                                LicenseId = c.LicenseID,
                                ModelName = c.ModelName,
                                Quantity = c.Quantity
                            }).ToList();
                            novaEntitlements.Add(primary);
                        }
                        else
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(new ApplicationException("SfdcLogic.AddExpanseEntitlements exception: Support Account ID '" + string.Join(",", supportAccountIds)
                                + "', ASC Salesforce Account ID '" + ascSalesforceAccountId + "', Serial Number '" + g.Key + "' does not have Primary Product"));
                        }
                    }
                    catch (Exception ex)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(JsonConvert.SerializeObject(g)));

                    }
                });


                var nonCdlExclusions = novaEntitlements.Where(i => i.AssetType != EAllAssetType.Cdl).ToArray(); // Nova takes precedence over Callidus non-CDLs
                if (nonCdlExclusions.Any())
                {
                    lstEntitlements.RemoveAll(e => nonCdlExclusions.Any(n => n.SerialNumber == e.SerialNumber)); // Dedup Callidus records, remove Nova matching non-CDLs
                }

                var cdlExclusions = novaEntitlements.Where(i => i.AssetType == EAllAssetType.Cdl).ToArray(); // Callidus takes precedence over Nova CDLs
                if (cdlExclusions.Any())
                {
                    novaEntitlements.RemoveAll(e => lstEntitlements.Any(n => n.SerialNumber == e.SerialNumber)); // Dedup Nova records, remove Callidus matching CDLs
                }

                if (novaEntitlements.Any())
                {
                    lstEntitlements.AddRange(novaEntitlements);
                }
            }

            return lstEntitlements;
        }


        public IEnumerable<AvailableSaaSActivations> AddXdrEntitlements(IEnumerable<AvailableSaaSActivations> entitlements, long[] supportAccountIds, List<string> ascSalesforceAccountId = null, bool isAscSearchOnly = false)
        {
            var lstEntitlements = entitlements.ToList();
            var sfdcEntitlementRequest = new SfdcEntitlementRequest();
           
            if (supportAccountIds.Length > 0)
            {
                sfdcEntitlementRequest.CspIds = supportAccountIds.Select(i => i.ToString()).ToArray();
            }

            var sfdcEntitlementEntitlements = GetSfdcEntitlements(sfdcEntitlementRequest, supportAccountIds, ascSalesforceAccountId,isAscSearchOnly);
            if (sfdcEntitlementEntitlements == null)
            {
                return lstEntitlements;
            }

            if (ascSalesforceAccountId != null && !isAscSearchOnly)
            {
                //filter only products with current user's asc accounts
                var filteredModels = ascSalesforceAccountId.Any() ? sfdcEntitlementEntitlements.SfdcEntitlementsModels?.Where(p => ascSalesforceAccountId.Contains(p.AscSfAccountId)).AsEnumerable() : sfdcEntitlementEntitlements.SfdcEntitlementsModels;
                sfdcEntitlementEntitlements.SfdcEntitlementsModels = filteredModels != null && filteredModels.Any() ? filteredModels.ToList() : null;
            }

            if (sfdcEntitlementEntitlements.SfdcEntitlementsModels != null && sfdcEntitlementEntitlements.SfdcEntitlementsModels.Any())
            {
                var novaEntitlements = new List<AvailableSaaSActivations>();
                var groups = sfdcEntitlementEntitlements.SfdcEntitlementsModels.GroupBy(t => t.EntitlementGroupId)
                    .SelectMany(w => w.SelectMany(q => q.Entitlements.Where(e => !string.IsNullOrWhiteSpace(e.SerialNumber) // clean up invalid data
                    && !string.IsNullOrWhiteSpace(e.Sku)
                    && (e.Sku.Contains("XDR") || e.Sku.Contains("XSIAM") || e.Sku.Contains("-LGS-"))
                    && !string.IsNullOrWhiteSpace(e.ProductType) && e.Status == "ACTIVE")
                    .Select(e => new AvailableSaaSActivations
                    {
                        SupportAccountId = w.FirstOrDefault() != null ? w.First().SupportAccountId : (long?)null,
                        SupportAccountName = w.FirstOrDefault() != null ? w.First().SalesAccountName : null,
                        AscAccountName = w.FirstOrDefault() != null ? w.First().AscName : null,
                        AssoicatedDevices = sfdcEntitlementEntitlements.SfdcEntitlementsModels.Any(p => p.EntitlementGroupId == w.Key
                            && p.Entitlements.Any(a => a.SerialNumber != e.SerialNumber))
                            ? string.Join(" ", sfdcEntitlementEntitlements.SfdcEntitlementsModels.Where(p => p.EntitlementGroupId == w.Key
                                && p.Entitlements.Any(a => a.SerialNumber != e.SerialNumber)).SelectMany(f => f.Entitlements.Where(h => h.SerialNumber != e.SerialNumber))
                                .Select(s => "<div>" + s.SerialNumber + "</div>\n").Distinct().ToArray())
                            : "",
                        AuthorizationCode = "N/A",
                        BundleLicenseId = 0,
                        BundleLicenses = new List<BundleLicenseItem>(),
                        Capacity = "N/A",
                        Description = e.ProductName,
                        ExpirationDate = !string.IsNullOrWhiteSpace(e.EndDate) ? Regex.Replace(e.EndDate, "T[Zz0-9\\:\\-]*", string.Empty, RegexOptions.IgnoreCase) : string.Empty,
                        GroupId = 0,
                        ModelName = e.Sku,
                        ProductType = e.ProductType,
                        Quantity = e.LicensingFeatures != null 
                            && e.LicensingFeatures.Any()
                            ? string.Join(", ", e.LicensingFeatures?.Select(u => u.Quantity + (!string.IsNullOrWhiteSpace(u.UnitOfMeasure) ? " " + u.UnitOfMeasure : string.Empty)).ToArray())
                            // ? string.Join("", e.LicensingFeatures?.Select(u => "<div>" + u.Quantity + (!string.IsNullOrWhiteSpace(u.UnitOfMeasure) ? u.UnitOfMeasure + " " + u.UnitOfMeasure : string.Empty) + "</div>").ToArray())
                            : string.Empty,
                        Region = e.Configuration != null && e.Configuration.Any() ? e.Configuration.FirstOrDefault().Region : string.Empty,
                        SerialNumber = e.SerialNumber,
                        AssetType = e.Sku.Contains("XDR") ? EAllAssetType.Xdr : e.Sku.Contains("XSIAM") ? EAllAssetType.XsIam : e.Sku.Contains("-LGS-") ? EAllAssetType.Cdl : EAllAssetType.Other
                    })
                    .GroupBy(e => e.SerialNumber) // get Primary/Children groups
                    .Where(g => g.Any(c => string.Compare(c.ProductType, NOVA_PRODUCT_TYPE_PRIMARY, true) == 0)))).ToList(); // Clear out groups w/ no Primary; Primary must exist in group
                    // CSP-4798_XDR_CDL_not_showing_Cloud_Services is not required in this implementation
                    groups.ForEach(g =>
                    {
                        try
                        {
                            var primary = g.SingleOrDefault(c => string.Compare(c.ProductType, NOVA_PRODUCT_TYPE_PRIMARY, true) == 0);
                            if (primary != null)
                            {
                                primary.Children = g.Where(c => string.Compare(c.ProductType, NOVA_PRODUCT_TYPE_PRIMARY, true) != 0).Select(c => new BundleLicenseItemBase
                                {
                                    AuthorizationCode = c.AuthorizationCode,
                                    BundleLicenseId = c.BundleLicenseId.HasValue ? c.BundleLicenseId.Value : 0,
                                    Capacity = c.Capacity,
                                    Description = c.Description,
                                    ExpirationDate = c.ExpirationDate,
                                    LicenseId = c.LicenseID,
                                    ModelName = c.ModelName,
                                    Quantity = c.Quantity
                                }).ToList();
                                novaEntitlements.Add(primary);
                            }
                            else
                            {
                                Elmah.ErrorSignal.FromCurrentContext().Raise(new ApplicationException("SfdcLogic.AddXdrEntitlements exception: Support Account ID '" + string.Join(",", supportAccountIds)
                                    + "', ASC Salesforce Account ID '" + ascSalesforceAccountId + "', Serial Number '" + g.Key + "' does not have Primary Product"));
                            }
                        }
                        catch (Exception ex)
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                            Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(JsonConvert.SerializeObject(g)));
                        }
                    });

                var nonCdlExclusions = novaEntitlements.Where(i => i.AssetType != EAllAssetType.Cdl).ToArray(); // Nova takes precedence over Callidus non-CDLs
                if (nonCdlExclusions.Any())
                {
                    lstEntitlements.RemoveAll(e => nonCdlExclusions.Any(n => n.SerialNumber == e.SerialNumber)); // Dedup Callidus records, remove Nova matching non-CDLs
                }

                var cdlExclusions = novaEntitlements.Where(i => i.AssetType == EAllAssetType.Cdl).ToArray(); // Callidus takes precedence over Nova CDLs
                if (cdlExclusions.Any())
                {
                    novaEntitlements.RemoveAll(e => lstEntitlements.Any(n => n.SerialNumber == e.SerialNumber)); // Dedup Nova records, remove Callidus matching CDLs
                }

                if (novaEntitlements.Any())
                {
                    lstEntitlements.AddRange(novaEntitlements);
                }
            }

            return lstEntitlements;
        }

        public IEnumerable<SfdcEntitlement> GetXSOAREntitlements(SfdcEntitlementSearchCriteria criteria, string status = "Active")
        {
            try
            {
                return GetEntitlements(JsonConvert.SerializeObject(criteria))
                    .Where(i => i.Status == status).Distinct().ToList();
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse
                    && (ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
                {
                    accessToken = null;
                }
                else
                {
                    throw;
                }
            }
            var searchValue = criteria.SearchType == SfdcEntitlementSearchType.SupportAccountId ? criteria.CspId : criteria.SerialNumber;
            Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("GetEntitlements exception: all attempts failed to get entitlements by '" + criteria.SearchType + "' "
                                                           + searchValue));
            return new SfdcEntitlement[0];
        }

        private IEnumerable<SfdcEntitlement> GetEntitlements(string searchCriteria)
        {
            int attempts = 3; // attempts to get fresh token
            int.TryParse(ConfigurationManager.AppSettings.Get("SfdcEntitlementAttemptsToGetToken"), out attempts);
            while (attempts-- > 0) // if attempts counter is 0 then 401 is likely not caused by invalid token
            {
                IEnumerable<SfdcEntitlement> entitlements = null;
                WebRequest request = HttpWebRequest.Create(ENTHOST + ENDPOINT);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", "Bearer " + GetToken().AccessToken);
                var bodyData = Encoding.Default.GetBytes(searchCriteria);
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
                            //TODO: This Immediate Encoding and Decoding is done for checkmarx purpose .It doesn't serve any purpose.
                            string encodedString = HttpUtility.HtmlEncode(readerEntitlements.ReadToEnd());
                            string decodedString = HttpUtility.HtmlDecode(encodedString);
                            entitlements = JsonConvert.DeserializeObject<SfdcEntitlement[]>(decodedString, new JsonSerializerSettings()
                            {
                                MaxDepth = 32,
                                TypeNameHandling = TypeNameHandling.None
                            }); //.Where(p => p.Status == "Active");
                            readerEntitlements.Close();
                        }
                        dataStream.Close();
                    }
                    responseEntitlements.Close();
                }
                return entitlements;
            }
            return new SfdcEntitlement[0];
        }

        public SfdcEntitlementResponse GetSfdcEntitlements(SfdcEntitlementRequest sfdcEntitlementRequest, long[] supportAccountIds , List<string> ascSalesforceAccountIds = null, bool isAscSearchOnly = false)
        {
            try
            {
                string requestQuery= string.Empty;
                var supportAccounts = supportAccountIds.Where(i => i > 0);
                string formattedSupportAccountIds = supportAccounts.Any() ? string.Join(",", supportAccounts.ToList()) : string.Empty;

                if (isAscSearchOnly && ascSalesforceAccountIds != null)
                {
                    string formattedAscIds = string.Join(",", ascSalesforceAccountIds.Select(s => $"\"{s}\""));

                    requestQuery = "{\"query\":\"query($status: [String], $ascAccountId: [String]) { license(ascAccountId:$ascAccountId , isCSPAssociated : true, status: $status) {entitlementGroupId,salesAccountName,salesAccountId,supportAccountId,isCSPAssociated,ascAccountName,ascAccountId," +
                                       "entitlements {status,serialNumber,tenantId,productId,productName,productType,sku,startDate,endDate,channelPlatform,instanceUrl," +
                                       "licenseFeatures{unitOfMeasure,quantity}," +
                                       "configuration{region}}}}\"" +
                                       ",\"variables\":{\"status\":\"ACTIVE\" , \"ascAccountId\":[ " + formattedAscIds + "]}}\"}";
                }
                else if(!string.IsNullOrEmpty(formattedSupportAccountIds))
                {
                    requestQuery = "{\"query\":\"query($status: [String]) { license(supportAccountId: [" + formattedSupportAccountIds + "], status: $status) {entitlementGroupId,salesAccountName,salesAccountId,supportAccountId,isCSPAssociated,ascAccountName,ascAccountId," +
                                       "entitlements {status,serialNumber,tenantId,productId,productName,productType,sku,startDate,endDate,channelPlatform,instanceUrl," +
                                       "licenseFeatures{unitOfMeasure,quantity}," +
                                       "configuration{region}}}}\"" +
                                       ",\"variables\":{\"status\":\"ACTIVE\"}}\"}";
                }

                if (!string.IsNullOrEmpty(requestQuery))
                {
                    var responsePayload = WebCommon.GetEntitlementsFromSfdc<WebCommon.CommonApiResponse<SfdcEntitlementResponse>>(requestQuery);
                    if (responsePayload != null)
                    {
                        return responsePayload.ResponseObject;
                    }
                }
               
                return new SfdcEntitlementResponse();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("EntitlementSearchRepository.GetXsoarMarketplaceConsumption supportAccountId exception:" + ex.Message, ex));
                return null;
            }

            //int attempts = 3; // attempts to get fresh token
            //int.TryParse(ConfigurationManager.AppSettings.Get("SfdcEntitlementAttemptsToGetToken"), out attempts);
            //string requestBody = string.Empty;
            //while (attempts-- > 0) // if attempts counter is 0 then 401 is likely not caused by invalid token
            //{
            //    try
            //    {
            //        IEnumerable<SfdcEntitlement> entitlements = null;
            //        WebRequest request = HttpWebRequest.Create(ENTHOST + ENDPOINT + "?status=Active");
            //        request.Method = "POST";
            //        request.ContentType = "application/json";
            //        request.Headers.Add("Authorization", "Bearer " + GetToken().AccessToken);
            //        requestBody = JsonConvert.SerializeObject(sfdcEntitlementRequest, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            //        var bodyData = Encoding.Default.GetBytes(requestBody);
            //        request.ContentLength = bodyData.Length;
            //        var requestStream = request.GetRequestStream();
            //        requestStream.Write(bodyData, 0, bodyData.Length);
            //        requestStream.Close();
            //        using (WebResponse responseEntitlements = request.GetResponse())
            //        {
            //            using (Stream dataStream = responseEntitlements.GetResponseStream())
            //            {
            //                using (StreamReader readerEntitlements = new StreamReader(dataStream))
            //                {
            //                    entitlements = JsonConvert.DeserializeObject<SfdcEntitlement[]>(readerEntitlements.ReadToEnd(), new JsonSerializerSettings()
            //                    {
            //                        MaxDepth = 32,
            //                        TypeNameHandling = TypeNameHandling.None
            //                    }); //.Where(p => p.Status == "Active");
            //                    readerEntitlements.Close();
            //                }
            //                dataStream.Close();
            //            }
            //            responseEntitlements.Close();
            //        }
            //        return entitlements;
            //    }
            //    catch (WebException ex)
            //    {
            //        if (ex.Response is HttpWebResponse
            //            && (ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized)
            //        {
            //            // the token likely has expired, make attempt to get new one from SFDC
            //            accessToken = null;
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //}
            //Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("GetEntitlementsBySupportAccountId exception: all attempts failed to get entitlements by Support Account ID " + JsonConvert.SerializeObject(sfdcEntitlementRequest)));
            //return new SfdcEntitlement[0];
        }

        

    }
}
