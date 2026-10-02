using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Newtonsoft.Json;
using Csp.Api.Model.Sfdc;
using Csp.Api.Model.UpdatesDownload;
using Csp.Api.Model.Enums;

namespace Csp.Api.Logic {
	public class SoftwareUpdateLogic : BaseLogic, ISoftwareUpdateLogic {

		private readonly IUpdatesRepository _updatesRepository;
		private readonly IDownloadUrlLogic _downloadUrlLogic;
		private readonly IEPDataAccessLogic _epDataAccessLogic;
        private readonly ISfdcEntitlementLogic _sfdcEntitlementLogic;

		public SoftwareUpdateLogic(IUpdatesRepository updatesRepository, IDownloadUrlLogic downloadUrlLogic, IEPDataAccessLogic epDataAccessLogic,ISfdcEntitlementLogic sfdcEntitlementLogic) {
			_updatesRepository = updatesRepository;
			_downloadUrlLogic = downloadUrlLogic;
			_epDataAccessLogic = epDataAccessLogic;
            _sfdcEntitlementLogic = sfdcEntitlementLogic;
        }

	    private class WildfireVersions
	    {
            private static readonly Regex MajorMinorVersionRegex = new Regex(@"^(\d+)\.(\d+)\.");

            public bool IsValid { get; set; }
            public int MajorVersion { get; set; }
            public int MinorVersion { get; set; }

            /// <summary>
            /// Only Valid wildfire versions from 7.1+ are considered "newest"
            /// </summary>
	        public bool IsNewest
	        {
	            get
	            {
	                if (!IsValid) return false;
                    
                    //Check major version first
	                if (MajorVersion != 7)
	                {
	                    return MajorVersion > 7;
	                }
	                else
	                {
                        //if major version == 7 then ensure hte minor version is 1 or greater
	                    return MinorVersion >= 1;
	                }
	            }
	        }

	        public static WildfireVersions ParseVersion(string minVerRequired)
	        {
                if (string.IsNullOrEmpty(minVerRequired))
                {
                    return new WildfireVersions {IsValid = false};
                }

                var match = MajorMinorVersionRegex.Match(minVerRequired);

	            if (!match.Success || match.Groups.Count < 3)
	            {
                    return new WildfireVersions { IsValid = false };
	            }

	            int major, minor;

	            if (int.TryParse(match.Groups[1].Value, out major) && int.TryParse(match.Groups[2].Value, out minor))
	            {
                    return new WildfireVersions { IsValid = true, MajorVersion = major, MinorVersion = minor};
	            }
	            else
	            {
                    return new WildfireVersions { IsValid = false };
	            }
	        }
	    }

        private string ParseGroupName(string groupName, string minVerRequired)
	    {
	        if (groupName == Wildfire)
	        {
	            var version = WildfireVersions.ParseVersion(minVerRequired);
	            
                if (!version.IsValid)
	            {
	                return WildfireOlder;
	            }

	            return version.IsNewest
	                ? WildfireNewest
	                : WildfireOlder;
	        }

            if (groupName.ToUpper() == "TRAPS")
            {
                if (!string.IsNullOrEmpty(minVerRequired))
                {
                    if(minVerRequired.StartsWith("3.3"))
                    {
                        return "TRAPS3.3";
                    }
                    else if (minVerRequired.StartsWith("3.4"))
                    {
                        return "TRAPS3.4";
                    }
                    else 
                    {
                        return "TRAPS4.2";
                    }
                }
                return "TRAPS";
            }

	        return groupName;
	    }

	    private const string Wildfire = "WILDFIRE";
        private const string WildfireNewest = "WILDFIRE_NEWEST";
        private const string WildfireOlder = "WILDFIRE_OLDER";


	    private Dictionary<string, NameValue> GetGroupNamesForDynamicUpdates() {
			return new Dictionary<string, NameValue>()
                {
                    {"CONTENTS", new NameValue() {Name = "Apps + Threats", Value = "100"}},
                    {"VIRUS", new NameValue() {Name = "Antivirus", Value = "101"}},
                    {"APPS", new NameValue() {Name = "Apps", Value = "102"}},
                    {Wildfire, new NameValue() {Name = "WildFire", Value = "103"}},
                    {WildfireNewest, new NameValue() {Name = "WildFire 7.1 and higher", Value = "104"}},
                    {WildfireOlder, new NameValue() {Name = "WildFire 7.0 and lower", Value = "105"}},
		            {"WF-500 CONTENT", new NameValue() {Name = "WF-500 Content", Value = "106"}},
                    {"BRIGHTCLOUD", new NameValue() {Name = "BrightCloud Seed File", Value = "200"}},
                    {"PANDB", new NameValue() {Name = "M-500 PAN-DB Offline Database", Value = "300"}},
                    {"TRAPS", new NameValue() {Name = "Traps", Value = "400"}},
                    {"TRAPS3.3", new NameValue() {Name = "Traps 3.3.3 and 3.3.4", Value = "401"}},                    
                    {"TRAPS3.4", new NameValue() {Name = "Traps 3.4 to 4.1", Value = "402"}},
                    {"TRAPS4.2", new NameValue() {Name = "Traps 4.2 and above", Value = "403"}},
                    {"GPCONTENTS", new NameValue() {Name = "Clientless VPN (Requires PAN-OS 8.0 or above)", Value= "107"}},
                };
		}

        public string GetDownloadUrl(SoftwareUpdateFileInfo fileInfo)
        {
            return _downloadUrlLogic.GetDownloadUrl(fileInfo.FolderName, fileInfo.FileName);
        }

        #region Content Update Changes
        public List<SoftwareUpdateFilters> GetFiltersForSoftwareUpdates(long userAccountId, long supportAccountId, out string groupIds)
        {
            var filters = _updatesRepository.GetFiltersForSoftwareUpdates(userAccountId, supportAccountId, out groupIds);
            if (supportAccountId == ApiConstants.PANW_SUPPORT_ACCOUNT_ID)
                return filters;

            // Remove the XDR filter from list
            var xdrFilter = filters.Where(x => x.DisplayName.Contains("XDR")).ToList();        

            if (xdrFilter.Any())
                return filters;

            var xdrEntitlements = _sfdcEntitlementLogic.GetEntitlementsBySupportAccountId(new SfdcEntitlementRequest { CspId = supportAccountId });
            if (xdrEntitlements.Any()) 
            {
                //if customer has only nova entitlements then add xdr to list
                filters.Add(new SoftwareUpdateFilters()
                {
                    DisplayName = "Cortex XDR - Analytics",
                    DisplayOrder = "241",
                    Platform = "Magnifier",
                    Product = "Magnifier",
                    HasMultipleReleaseTypes = false
                });

                var ipsFilter = filters.Where(x => x.DisplayName.Contains("IPS")).ToList();

                if (ipsFilter.Any())
                    return filters;
                else
                {
                    filters.Add(new SoftwareUpdateFilters()
                    {
                        DisplayName = "IPS Signature Converter Plugin",
                        DisplayOrder = "361",
                        Platform = "IPS-Sig-Converter",
                        Product = "plugin",
                        HasMultipleReleaseTypes = false
                    });
                }
            }

            return filters;
        }

        public SoftwareUpdateRequest IsEligibleToViewUpdates(SoftwareUpdateRequest requestedData, out string groupIds)
	    {
	        SoftwareUpdateRequest result = new SoftwareUpdateRequest();
            string eligiblegroups = string.Empty;
	        try
	        {
	            List<SoftwareUpdateFilters> filters = GetFiltersForSoftwareUpdates(requestedData.UserAccountId,requestedData.SupportAccountId, out eligiblegroups);

	            List<string> lstPlatforms = requestedData.Platform.Split(',').ToList();
	            List<string> lstProducts = requestedData.Product.Split(',').ToList();

                List<string> eligibleProducts = new List<string>();
	            foreach (var product in lstProducts)
	            {
	                if (filters.Any(x => x.Product == product))
	                {
                        eligibleProducts.Add(product);
	                }
	            }


	            List<string> eligiblePlatforms = new List<string>();
	            foreach (var platform in lstPlatforms)
	            {
                    if (filters.Any(x => x.Platform == platform))
	                {
                        eligiblePlatforms.Add(platform);
	                }
	            }

	            result.Platform = eligiblePlatforms.Any() ? (String.Join(",", eligiblePlatforms)) : string.Empty;
	            result.Product =  eligibleProducts.Any() ? String.Join(",", eligibleProducts)  : string.Empty;
	            result.SupportAccountId = requestedData.SupportAccountId;
	            result.UserAccountId = requestedData.UserAccountId;

                

            }
	        catch (Exception ex)
	        {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
	        }
            groupIds = eligiblegroups;
	        return result;
	    }

	    public async Task<List<SoftwareUpdateListItem>> GetSoftwareUpdates(SoftwareUpdateRequest filters)
	    {
	        List<SoftwareUpdateListItem> updatesList = new List<SoftwareUpdateListItem>();
	        try
	        {
                if (filters.ContentType == ContentTypeEnum.REQUIRED_SW_UPGRADE.ToString())
                {
                    /* removing "and hhigher (everything)
                    List<SoftwareUpdateListItem> set1 = await ReadSoftwareUpdates(filters, false);
                    List<SoftwareUpdateListItem> set2 = await ReadSoftwareUpdates(filters, true);
                    updatesList = set1.Union(set2).GroupBy(x => x.VersionNumber).Select(x => x.First()).ToList();
                    */
                    updatesList = await ReadSoftwareUpdates(filters, false);
                }
                else
                {
                    updatesList = await ReadSoftwareUpdates(filters);
                }
	        }
	        catch (Exception ex)
	        {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
	        }

	        return updatesList;
	    }

        public List<SoftwareUpdateFilters> GetFiltersForDynamicUpdates(long userAccountId, long supportAccountId, string contentType, out string groupIds)
        {

            var filters = _updatesRepository.GetFiltersForDynamicUpdates(userAccountId, supportAccountId, contentType, out groupIds);
            var trapsFilter = filters.Where(x => x.DisplayName.ToUpper().Contains(ContentTypeEnum.TRAPS.ToString())).ToList();

            if (!trapsFilter.Any())
            {
                try
                {
                    var sfdcEntitlements = _sfdcEntitlementLogic.GetEntitlementsBySupportAccountId(new SfdcEntitlementRequest { CspId = supportAccountId });
                    if (sfdcEntitlements.Any(a => a.Product != null && a.Product.Sku != null && (a.Product.Sku.Contains("XDR") || (a.Product.Sku.Contains("-LGS-")))))
                    {
                        //if customer has sdfc entitlements then add traps to list
                        filters.Add(new SoftwareUpdateFilters()
                        {
                            DisplayName = "Traps",
                            ContentType = ContentTypeEnum.TRAPS.ToString(),
                            DisplayOrder = "200"
                        });
                    }
                }
                catch (Exception ex)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }
            }

            return filters;
        }

        private async Task<List<SoftwareUpdateListItem>> ReadSoftwareUpdates(SoftwareUpdateRequest request, bool? orHigher = null)
	    {
            List<SoftwareUpdateListItem> updatesList = new List<SoftwareUpdateListItem>(); 
            List<SoftwareUpdateModel> updatesmodelList = new List<SoftwareUpdateModel>(); 
	        try
            {

                #region filter params
                List<queryFilters> queryfilter = new List<queryFilters>()
                {
                    new queryFilters()
                    {

                        filterName = FilterNameEnum.RELEASEDATE.ToString(),
                        filterType = FilterTypeEnum.ORDERBY.ToString(),
                        filterValue = "DESC"
                    }
                };

                if (!string.IsNullOrEmpty(request.GroupIds))
                {
                    queryfilter.Add(new queryFilters()
                    {
                        filterName = FilterNameEnum.GROUPID.ToString(),
                        filterType = FilterTypeEnum.IN.ToString(),
                        filterValue = request.GroupIds
                    });
                }

                if (!string.IsNullOrEmpty(request.Platform))
                {
                    queryfilter.Add(new queryFilters()
                    {

                        filterName = FilterNameEnum.PLATFORM.ToString(),
                        filterType = FilterTypeEnum.IN.ToString(),
                        filterValue = request.Platform
                    });
                }

                if (!string.IsNullOrEmpty(request.Product))
                {
                    queryfilter.Add(new queryFilters()
                    {

                        filterName = FilterNameEnum.PRODUCT.ToString(),
                        filterType = FilterTypeEnum.IN.ToString(),
                        filterValue = request.Product
                    });
                }

                if (!string.IsNullOrEmpty(request.Feature))
                {
                    queryfilter.Add(new queryFilters()
                    {

                        filterName = FilterNameEnum.FEATURE.ToString(),
                        filterType = FilterTypeEnum.EQUALS.ToString(),
                        filterValue = request.Feature
                    });
                }

                if (!string.IsNullOrEmpty(request.MaxVersionRequired))
                {
                    queryfilter.Add(new queryFilters()
                        {

                            filterName = FilterNameEnum.MINIMUMVERSIONREQUIRED.ToString(),
                            filterType = FilterTypeEnum.LESSEROREQUAL.ToString(),
                            filterValue = request.MaxVersionRequired
                        });
                }

                if (!string.IsNullOrEmpty(request.MinVersionRequired))
                {
                    queryfilter.Add(new queryFilters()
                    {

                        filterName = FilterNameEnum.MINIMUMVERSIONREQUIRED.ToString(),
                        filterType = FilterTypeEnum.GREATEROREQUAL.ToString(),
                        filterValue = request.MinVersionRequired
                    });
                }

                if (request.ContentType == ContentTypeEnum.SOFTWARE.ToString()
                    && !string.IsNullOrEmpty(request.ReleaseType))
                {
                    if (string.Compare(request.ReleaseType, ReleaseType.All.ToString(), true) != 0 
                        && string.Compare(request.ReleaseType, ReleaseType.Other.ToString(), true) != 0)
                    {
                        string releaseType = string.Compare(request.ReleaseType, ReleaseType.Preferred.ToString(), true) == 0 ?
                             ReleaseType.Preferred.ToDescription() : string.Compare(request.ReleaseType, ReleaseType.Remediated.ToString(), true) == 0 ?
                             ReleaseType.Remediated.ToDescription() : request.ReleaseType;
                        if (releaseType == "Certificate Expiration Remediated")
                        {
                            queryfilter.Add(new queryFilters()
                            {
                                filterName = FilterNameEnum.RELEASETYPE.ToString(),
                                filterType = FilterTypeEnum.IN.ToString(),
                                filterValue = "Certificate Expiration Remediated,CVE Remediated,CVE/Certificate Expiration Remediated"
                            });
                        }
                        else
                        {
                            queryfilter.Add(new queryFilters()
                            {
                                filterName = FilterNameEnum.RELEASETYPE.ToString(),
                                filterType = FilterTypeEnum.EQUALS.ToString(),
                                filterValue = releaseType
                            });
                        }
                    }
                    if (string.Compare(request.ReleaseType, ReleaseType.Other.ToString(), true) == 0)
                    {
                        queryfilter.Add(new queryFilters()
                        {

                            filterName = FilterNameEnum.RELEASETYPE.ToString(),
                            filterType = FilterTypeEnum.IN.ToString(),
                            filterValue = ReleaseType.Other.ToDescription()
                        });

                        queryfilter.Add(new queryFilters()
                        {
                            filterName = FilterNameEnum.RELEASETYPE.ToString(),
                            filterType = FilterTypeEnum.ORDERBY.ToString(),
                            filterValue = "DESC"
                        });
                    }
                }

                // CSP-18237 [BE] Endpoints for new CSP PAN-OS Advisory page
                if (request.ContentType == ContentTypeEnum.REQUIRED_SW_UPGRADE.ToString() && request.Product != "ts agent" && request.Product != "id agent")
                {
                    if (string.IsNullOrWhiteSpace(request.CurrentVersionId))
                    {
                        throw new Exception("SoftwareUpdateLogic::ReadSoftwareUpdates model.CurrentVersionId is required.");
                    }

                    ContentDownloadResponseVersionUpgrade currentVersion = _updatesRepository.GetSoftwareUpgradeVersion(request);
                    if (currentVersion == null)
                    {
                        throw new Exception("SoftwareUpdateLogic::ReadSoftwareUpdates currentVersion is not found; Id: " + request.CurrentVersionId);
                    }

                    if (string.IsNullOrWhiteSpace(currentVersion?.NewMinVersion))
                    {
                        throw new Exception("SoftwareUpdateLogic::ReadSoftwareUpdates currentVersion.NewMinVersion is required");
                    }

                    if (orHigher == true)
                    {
                        queryfilter.Add(new queryFilters()
                        {

                            filterName = FilterNameEnum.VERSIONNUMBER.ToString(),
                            filterType = FilterTypeEnum.GREATER.ToString(),
                            filterValue = currentVersion.NewMinVersion
                        });
                    }
                    else if (orHigher == false)
                    {
                        queryfilter.Add(new queryFilters()
                        {

                            filterName = FilterNameEnum.VERSIONNUMBER.ToString(),
                            filterType = FilterTypeEnum.IN.ToString(),
                            filterValue = currentVersion.Whitelist
                        });
                    }
                }

                #endregion

                #region Content Type
                InputParams readInputParams = new InputParams()
	            {
	                contentType = request.ContentType == ContentTypeEnum.REQUIRED_SW_UPGRADE.ToString() ? ContentTypeEnum.SOFTWARE.ToString().ToLower() : request.ContentType.ToLower(),
	                queryFilters = queryfilter
	            };
                #endregion

                updatesmodelList = await GetUpdates(readInputParams);

                if (request.ContentType == ContentTypeEnum.SOFTWARE.ToString() || request.ContentType == ContentTypeEnum.PLUGIN.ToString() || request.ContentType == ContentTypeEnum.REQUIRED_SW_UPGRADE.ToString())
                {
                    foreach (var update in updatesmodelList)
                    {
                        _downloadUrlLogic.PopulateSoftwareUpdatesDownloadUrl(update);
                    }
                }
                else
                {
                    foreach (var update in updatesmodelList)
                    {
                        _downloadUrlLogic.PopulateDynamicUpdatesDownloadUrl(update);
                    }
                }

                // CSP-18237 [BE] Endpoints for new CSP PAN-OS Advisory page
                if (request.ContentType == ContentTypeEnum.REQUIRED_SW_UPGRADE.ToString())
                {
                    IEnumerable<string> expired = _updatesRepository.GetSoftwareUpgradeExpired(); // Blacklisted versions
                    updatesmodelList = updatesmodelList.Where(i => !expired.Any(e => i.VersionNumber.ToLower() == e.ToLower())).ToList();
                }

                Dictionary<string, string> updateTypes = _updatesRepository.GetModelsSupportedItems() ?? new Dictionary<string, string>();
                var result = (from softwareUpdate in updatesmodelList
                    select new SoftwareUpdateListItem
                    {
                        Id = softwareUpdate.GroupDisplayOrder,
                        Key = softwareUpdate.GroupDisplayName,
                        DisplayName = softwareUpdate.GroupDisplayName,
                        DisplayFileName = softwareUpdate.FileDisplayName,
                        VersionNumber = request.ContentType == "BRIGHTCLOUD" ? "Latest": softwareUpdate.VersionNumber,
                        ReleaseDate = softwareUpdate.ReleaseDate,
                        ReleaseNotesType = softwareUpdate.ReleaseNotesType,
                        ReleaseNotesFileName = softwareUpdate.ReleaseNotes ?? string.Concat(softwareUpdate.FileDisplayName, "-Release-Notes"),
                        ReleaseNotesUrl = softwareUpdate.ReleaseNotesUrl,
                        Platform = softwareUpdate.Platform,
                        Product = softwareUpdate.Product,
                        DownloadUrl = softwareUpdate.DownloadUrl,
                        FolderName = softwareUpdate.FolderName,
                        FileName = softwareUpdate.FileName,
                        FileSize = softwareUpdate.FileSize.ToString(),
                        Md5Key = softwareUpdate.Md5Cks,
                        Sha256Key = softwareUpdate.Sha256Cks,
                        PolicyFile = softwareUpdate.PolicyFile,
                        PolicyFileUrl = softwareUpdate.PolicyFileUrl,
                        SoftwareUpdateId = softwareUpdate.SoftwareUpdateId,
                        ContentType = request.ContentType,
                        Incremental = softwareUpdate.Incremental,
                        ModelsSupported = softwareUpdate.ModelsSupported == null ? string.Empty : (updateTypes.ContainsKey(softwareUpdate.ModelsSupported)
                            ? updateTypes[softwareUpdate.ModelsSupported]
                            : string.Empty),
                        ReleaseType = string.Compare(softwareUpdate.ReleaseType, ReleaseType.NotTagged.ToDescription(), true) == 0
                            ? "" 
                            : softwareUpdate.ReleaseType, 
                        SbomFile = softwareUpdate.SbomFile,
                        SbomDisplayName = softwareUpdate.SbomDisplayName,
                        Md5ChecksumSbom = softwareUpdate.Md5ChecksumSbom,
                        Sha256ChecksumSbom = softwareUpdate.Sha256ChecksumSbom,
                        FileSizeSbom = softwareUpdate.FileSizeSbom.ToString(),
                        Label = request.Label,
                        CurrentVersionId = request.CurrentVersionId
                    }).ToList();

                updatesList = result;

            }
	        catch (Exception e)
	        {
	            Elmah.ErrorSignal.FromCurrentContext().Raise(e);
	        }
	        return updatesList;
	    }

        /// <summary>
        /// Asyn call to Retrieve Updates
        /// </summary>
        /// <param name="readInputParams"></param>
        /// <returns></returns>
        private async Task<List<SoftwareUpdateModel>> GetUpdates(InputParams readInputParams)
        {
            //List<SoftwareUpdateListItem> retValue = new List<SoftwareUpdateListItem>(); now changed
            List<SoftwareUpdateModel> retValue = new List<SoftwareUpdateModel>();
            try
            {
                string updatesUrl = ConfigurationManager.AppSettings["UpdatesReaderAPI"];
                string postUrl = string.Concat(updatesUrl, "getcontent");

                string apikey = ConfigurationManager.AppSettings["apikey"];

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(postUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("apiKey", apikey);

                    var jsondata = JsonConvert.SerializeObject(readInputParams);
                    HttpContent postContent = new StringContent(jsondata, Encoding.UTF8, "application/json");

                    //HttpResponseMessage response =  client.PostAsJsonAsync(postUrl, readInputParams).Result;
                    HttpResponseMessage response = await client.PostAsync(postUrl, postContent);
                    response.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                    if (response.IsSuccessStatusCode)
                    {
                        string jresponse = await response.Content.ReadAsStringAsync();

                        var responsevalue = JsonConvert.DeserializeObject<SoftwareUpdateResponse>(jresponse, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        });

                        //retValue = responsevalue.success ? responsevalue.data : new List<SoftwareUpdateListItem>(); now changed
                        retValue = responsevalue.success ? responsevalue.data : new List<SoftwareUpdateModel>();
                    }
                }
                
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return retValue;
        }

        private async Task<string> GetDownloadUrl(DownloadUrlParams readInputParams)
        {
            string retValue = string.Empty;
            try
            {
                string updatesUrl = ConfigurationManager.AppSettings["UpdatesReaderAPI"];
                string postUrl = string.Concat(updatesUrl, "signurl");

                string apikey = ConfigurationManager.AppSettings["apikey"];

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(postUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("apiKey", apikey);

                    var jsondata = JsonConvert.SerializeObject(readInputParams);
                    HttpContent postContent = new StringContent(jsondata, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = client.PostAsync(postUrl, postContent).Result;
                    response.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

                    if (response.IsSuccessStatusCode)
                    {
                        string jresponse = await response.Content.ReadAsStringAsync();

                        var responsevalue = JsonConvert.DeserializeObject<DownloadUrlResponse>(jresponse, new JsonSerializerSettings()
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore
                        });

                        retValue = responsevalue.success ? HttpUtility.HtmlDecode(responsevalue.data) : null;
                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return retValue;
        }

        public async Task<List<SoftwareUpdateListItem>> GetDynamicUpdates(SoftwareUpdateRequest filters)
        {
            List<SoftwareUpdateListItem> updatesList = new List<SoftwareUpdateListItem>();
            try
            {
                 if (filters.ContentType.ToUpper() == ContentTypeEnum.CONTENT.ToString() || filters.ContentType.ToUpper() == ContentTypeEnum.CONTENTS.ToString())
                {
                    filters.ContentType = ContentTypeEnum.CONTENT.ToString();
                    filters.Feature = ContentTypeEnum.CONTENTS.ToString();
                }
                
                if (filters.ContentType.ToUpper() == ContentTypeEnum.APPS.ToString())
                {
                    filters.ContentType = ContentTypeEnum.CONTENT.ToString();
                    filters.Feature = ContentTypeEnum.APPS.ToString();
                }

                if (filters.ContentType.ToUpper() == ContentTypeEnum.ACE.ToString())
                {
                    filters.ContentType = ContentTypeEnum.ACE.ToString();
                    filters.Feature = ContentTypeEnum.ACE.ToString();
                }

                if (filters.ContentType.ToUpper() == ContentTypeEnum.TRAFFICREDIRECTOR.ToString())
                {
                    filters.ContentType = ContentTypeEnum.TRAFFICREDIRECTOR.ToString();
                    filters.Feature = ContentTypeEnum.TRAFFICREDIRECTOR.ToString();
                }

               

                List<SoftwareUpdateListItem> result = await ReadSoftwareUpdates(filters);

                if (result.Count > 0)
                {
                    //display only non-incremental values
                    if (filters.ContentType.ToUpper() == ContentTypeEnum.VIRUS.ToString() ||
                        filters.ContentType.ToUpper() == ContentTypeEnum.WILDFIRE.ToString() ||
                        filters.ContentType.ToUpper() == ContentTypeEnum.GPCONTENT.ToString() ||
                        (filters.ContentType.ToUpper() ==ContentTypeEnum.CONTENT.ToString() && filters.Feature.ToUpper() == ContentTypeEnum.APPS.ToString()) ||
                        (filters.ContentType.ToUpper() == ContentTypeEnum.CONTENT.ToString() && filters.Feature.ToUpper() == ContentTypeEnum.CONTENTS.ToString()))
                    {
                        updatesList = result.Where(x => x.Incremental == 0).ToList();
                    }
                    else
                    {
                        updatesList = result;
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return updatesList;
        }



        public List<ProductModelListItem> GetAllProductModels()
        {
            return _updatesRepository.GetAllProductModels();
        }

        public bool ShowTrapsSoftwareUpdates(long supportAccountId)
        {
            return _updatesRepository.ShowTrapsSoftwareUpdates(supportAccountId);
        }

        public async Task<List<PanOs>> GetPanOSVersions()
        {
            List<PanOs> osVersionList = new List<PanOs>();
            try
            {
                List<queryFilters> queryfilter = new List<queryFilters>()
                {
                    new queryFilters()
                    {

                        filterName = FilterNameEnum.VERSIONNUMBER.ToString(),
                        filterType = FilterTypeEnum.ORDERBY.ToString(),
                        filterValue = "DESC"
                    }
                };

                InputParams readInputParams = new InputParams()
                {
                    contentType = ContentTypeEnum.SOFTWARE.ToString().ToLower(),
                    queryFilters = queryfilter
                };
                var updatesOSVersionsList = await GetUpdates(readInputParams);
                var distinctOSVersionList = updatesOSVersionsList.Select(os=>os.VersionNumber).Distinct().ToList();
                osVersionList = distinctOSVersionList.Select(os => new PanOs() { PanOsVersion = os }).OrderBy(x => x.PanOsVersion).ToList();
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return osVersionList;
        }

        public Dictionary<string, string> GetModelsSupportedItems()
        {
            return _updatesRepository.GetModelsSupportedItems();
        }

        #endregion

        #region Cellular

        public Task<ContentDownloadResponseCellularFilters> GetCellularFilters(ContentDownloadRequestBase model)
        {
            return Task.Run(() => _updatesRepository.GetCellularFilters(model));
        }

        public async Task<ContentDownloadResponseCellularDownloads> GetCellularData(ContentDownloadRequestCellularData model)
        {
            ContentDownloadRequestCellularDbRecord dbRecord = _updatesRepository.GetCellularData(model);
            if (string.IsNullOrWhiteSpace(dbRecord.Platform))
            {
                throw new Exception("SoftwareUpdateLogic::GetCellularData 'Platform' is invalid. Details: " + JsonConvert.SerializeObject(model));
            }

            if (string.IsNullOrWhiteSpace(dbRecord.Product))
            {
                throw new Exception("SoftwareUpdateLogic::GetCellularData 'Product' is invalid. Details: " + JsonConvert.SerializeObject(model));
            }

            if (string.IsNullOrWhiteSpace(dbRecord.Model))
            {
                throw new Exception("SoftwareUpdateLogic::GetCellularData 'Product Model' is invalid. Details: " + JsonConvert.SerializeObject(model));
            }

            List<queryFilters> queryfilter = new List<queryFilters>()
            {
                new queryFilters() { filterName = FilterNameEnum.RELEASEDATE.ToString(), filterType = FilterTypeEnum.ORDERBY.ToString(), filterValue = "DESC" }
            };
            queryfilter.Add(new queryFilters() { filterName = FilterNameEnum.PLATFORM.ToString(), filterValue = dbRecord.Platform, filterType = FilterTypeEnum.EQUALS.ToString() });
            queryfilter.Add(new queryFilters() { filterName = FilterNameEnum.PRODUCT.ToString(), filterValue = dbRecord.Product, filterType = FilterTypeEnum.EQUALS.ToString() });
            queryfilter.Add(new queryFilters() { filterName = FilterNameEnum.PLATFORMMODEL.ToString(), filterValue = dbRecord.Model, filterType = FilterTypeEnum.LIKEALL.ToString() });
            if (!string.IsNullOrWhiteSpace(model.CarrierId))
            {
                if (string.IsNullOrWhiteSpace(dbRecord.Carrier))
                {
                    throw new Exception("SoftwareUpdateLogic::GetCellularData 'Carrier' is invalid. Details: " + JsonConvert.SerializeObject(model));
                }

                queryfilter.Add(new queryFilters() { filterName = FilterNameEnum.CARRIER.ToString(), filterValue = dbRecord.Carrier, filterType = FilterTypeEnum.EQUALS.ToString() });
            }

            InputParams readInputParams = new InputParams()
            {
                contentType = ContentTypeEnum.CELLULAR.ToString().ToLower(),
                queryFilters = queryfilter
            };
            List<SoftwareUpdateModel> content = await GetUpdates(readInputParams);
            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                string search = model.Search.Trim().ToLower();
                content = content.Where(i => i.FileName.ToLower().Contains(search) || (i.FileDisplayName?.ToLower().Contains(search) ?? false)).ToList();
            }

            content.ForEach(i => _downloadUrlLogic.PopulateCellulerUpdatesDownloadUrl(i));
            int count = 1;
            IEnumerable<ContentDownloadResponseCellularDownloadItem> items = content.Select(i => new ContentDownloadResponseCellularDownloadItem
            {
                Id = count++.ToString(),
                Version = i.VersionNumber,
                ReleaseDate = i.ReleaseDate.ToString(),
                ReleaseNotes = i.ReleaseNotes, 
                ReleaseNotesType = i.ReleaseNotesType,
                UpdateType = i.ModelsSupported,
                Carrier = i.Carrier,
                Platform = i.Platform,
                Product = i.Product,
                ProductFilterId = model.ProductFilterId,
                CarrierId = model.CarrierId,
                PlatformModel = i.PlatformModel,
                DisplayFileName = i.FileDisplayName,
                FolderName = i.FolderName,
                FileName = i.FileName,
                FileSize = i.FileSize.ToString(),
                Md5Checksum = i.Md5Cks,
                ShA256Checksum = i.Sha256Cks,
                FirmwareDetails = i.FirmwareDetails,
                Deprecated = i.Deprecated
            }).ToArray();
            return new ContentDownloadResponseCellularDownloads
            {
                Downloads = items.Skip((model.Pagination.Current - 1) * model.Pagination.PageSize).Take(model.Pagination.PageSize).ToArray(), 
                Pagination = new ContentDownloadPagination
                {
                    Current = model.Pagination.Current,
                    PageSize = model.Pagination.PageSize,
                    Total = items.Count()
                }
            };
        }
        public async Task<bool> GetFileDataFromUpdatesReader(SoftwareUpdateFileInfo fileInfo)
        {
            
            try
            {
                List<queryFilters> queryfilter = new List<queryFilters>()
                {
                    new queryFilters()
                    {

                        filterName = FilterNameEnum.VERSIONNUMBER.ToString(),
                        filterType = FilterTypeEnum.ORDERBY.ToString(),
                        filterValue = "DESC"
                    } ,
                    new queryFilters()
                    {

                        filterName = FilterNameEnum.FILENAME.ToString(),
                        filterType = FilterTypeEnum.EQUALS.ToString(),
                        filterValue = fileInfo.FileName
                    }

                };
                if (!String.IsNullOrEmpty(fileInfo.Platform))
                    queryfilter.Add(new queryFilters() { filterName = FilterNameEnum.PLATFORM.ToString(), filterValue = fileInfo.Platform, filterType = FilterTypeEnum.EQUALS.ToString() });
                if (!String.IsNullOrEmpty(fileInfo.Product))
                    queryfilter.Add(new queryFilters() { filterName = FilterNameEnum.PRODUCT.ToString(), filterValue = fileInfo.Product, filterType = FilterTypeEnum.EQUALS.ToString() });

                if (!string.IsNullOrEmpty(fileInfo.GroupIds))
                {
                    queryfilter.Add(new queryFilters()
                    {
                        filterName = FilterNameEnum.GROUPID.ToString(),
                        filterType = FilterTypeEnum.IN.ToString(),
                        filterValue = fileInfo.GroupIds
                    });
                }
                InputParams readInputParams = new InputParams()
                {
                    contentType = fileInfo.ContentType,
                    queryFilters = queryfilter
                };
                var getFileData =  await GetUpdates(readInputParams);
                if (getFileData != null) return true;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return false;
        }
        #endregion

        #region Mandatory upgrades

        public List<SoftwareUpdateFilters> GetSoftwareUpgradeModels()
        {
            return _updatesRepository.GetSoftwareUpgradeModels();
        }

        public List<ContentDownloadResponseVersionUpgrade> GetSoftwareUpgradeVersions()
        {
            return _updatesRepository.GetSoftwareUpgradeVersions();
        }

        public List<string> GetSoftwareUpgradeExpired()
        {
            return _updatesRepository.GetSoftwareUpgradeExpired();
        }

        public List<ContentDownloadResponseTooltipUpgrade> GetSoftwareUpgradeTooltip()
        {
            return _updatesRepository.GetSoftwareUpgradeTooltip();
        }

        #endregion
    }
}
