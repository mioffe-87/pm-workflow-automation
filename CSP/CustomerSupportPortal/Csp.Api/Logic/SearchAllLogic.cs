using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Api.DataAccess;
using LINQtoCSV;

namespace Csp.Api.Logic
{
    public class SearchAllLogic : ISearchAllLogic
    {

        private readonly ISecurityRepository _securityRepository;
        private readonly ISupportAccountDevicesRepository _supportAccountDevicesRepository;
        private readonly ICurrentStateService _currentStateService;
        private readonly ISearchAllRepository _searchAllRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IEpRepository _epRepository;
        private readonly IConfigValues _configValues;
        private readonly ISaasDataAccess _saasDataAccess;

        public SearchAllLogic(ISecurityRepository securityRepository, ISupportAccountDevicesRepository supportAccountDevicesRepository, ICurrentStateService currentStateService, ISearchAllRepository searchAllRepository, IDeviceRepository deviceRepository, IEpRepository epRepository, IConfigValues configValues, ISaasDataAccess saasDataAccess)
        {
            _securityRepository = securityRepository;
            _supportAccountDevicesRepository = supportAccountDevicesRepository;
            _currentStateService = currentStateService;
            _searchAllRepository = searchAllRepository;
            _deviceRepository = deviceRepository;
            _epRepository = epRepository;
            _configValues = configValues;
            _saasDataAccess = saasDataAccess;
        }

        public SearchAllViewModel GetDefaultViewModel(long userAccountId, long? defaultSupportAccountId)
        {

            var searchAssetTypes = new SearchAllViewModel.SearchModel
            {
                SearchDevices = true,
                SearchSpares = true,
                SearchTraps = true,
                SearchVms = true,
            };

            var numAccounts = _securityRepository.GetNumberOfAccountsUserCanAccess(userAccountId);

            SelectableAccount[] availableSupportAccounts = null;
            if (!defaultSupportAccountId.HasValue && numAccounts == 1)
            {
                availableSupportAccounts = _securityRepository.GetAvailableAccounts(userAccountId, new DataParameters());
                defaultSupportAccountId = availableSupportAccounts.FirstOrDefault().SupportAccountId;
            }

            var supportAccountName = string.Empty;
            if (defaultSupportAccountId.HasValue)
            {
                if (availableSupportAccounts == null)
                {
                    availableSupportAccounts = _securityRepository.GetListOfAccountsUserCanAccess(userAccountId).ToArray();
                }

                supportAccountName =
                    availableSupportAccounts.Single(x => x.SupportAccountId == defaultSupportAccountId.Value).CompanyName;
            }

            var model = new SearchAllViewModel
            {
                HasMultipleAccounts = numAccounts > 1,
                CurrentSupportAccountName = supportAccountName,
                SearchCurrentSupportAccount = defaultSupportAccountId.HasValue,
                DefaultSupportAccountId = defaultSupportAccountId,

                SearchAssetTypes = searchAssetTypes,
                DataParameters = new DataParameters { PaginationCount = 10 },
            };

            if (model.DefaultSupportAccountId.HasValue)
            {
                model.SearchAllResults = SearchAllV2(userAccountId, defaultSupportAccountId, model.DataParameters)?.ToArray();
            }
            else
            {

                model.SearchAllResults = new SearchAllModel[1]
					{
						new SearchAllModel
							{
								Id = 0,
								SupportAccountName = string.Empty,
								DeviceCode = string.Empty,
							}
					};
                model.DataParameters.TotalRecordCount = 1;
                model.DataParameters.PaginationCount = 10;
            }

            if (!defaultSupportAccountId.HasValue) // Search Multiple Accounts
            {
                model.CspIds = _securityRepository.GetAvailableAccounts(userAccountId, new DataParameters { PaginationOffset = 0, PaginationCount = int.MaxValue }).ToDictionary(k => k.SupportAccountId, v => v.CompanyName);
            }
            return model;
        }

        public SearchAllModel[] SearchAll(long userAccountId, long? defaultSupportAccountId, DataParameters dataParameters, bool isDebug = false, StringBuilder sb = null)
        {
            if (!defaultSupportAccountId.HasValue
                && dataParameters.SearchField == "CspId")
            {
                defaultSupportAccountId = long.Parse(dataParameters.SearchValue);
                dataParameters.SearchValue = null;
                dataParameters.SearchField = null;
            }

            var sw = new Stopwatch();
            sw.Start();
            var devices = _searchAllRepository.SearchAll(userAccountId, defaultSupportAccountId, dataParameters,
                                                                        _currentStateService.GetCurrentUser().RoleSecurities,
                                                                        new SearchAllFilters(), sb);
            sw.Stop();
            if (sb != null)
            {
                sb.AppendLine(dataParameters.TotalRecordCount + " New Query Time: " + sw.ElapsedMilliseconds);
            }

            AddLicenesToProdDevices(devices);

            AddLicensesToTrapsDevices(devices);

            AddGroupsToDevices(devices, defaultSupportAccountId);

            if (sb != null)
            {
                sb.AppendLine();
            }

            return devices;
        }

        public IEnumerable<SearchAllModel> SearchAllV2(long userAccountId, long? defaultSupportAccountId, DataParameters dataParameters)
        {
            return _searchAllRepository.SearchAllV2(userAccountId, defaultSupportAccountId, dataParameters, _currentStateService.GetCurrentUser().RoleSecurities);
        }

        private void AddLicenesToProdDevices(SearchAllModel[] devices)
        {

            var licenseDataAccess = new LicenseDataAccess();
            var grouppedLicenses = licenseDataAccess.ReadMultipleDeviceLicenses(devices.Where(x => x.AssetType == AssetTypes.Prod).Select(x => x.Id).ToArray(), false);

            foreach (var d in devices)
            {
                if (d.AssetType != AssetTypes.Prod)
                {
                    continue;
                }
                var grouppedLicense = grouppedLicenses.SingleOrDefault(x => x.DeviceId == d.Id && d.AssetType == AssetTypes.Prod);
                if (grouppedLicense == null)
                {
                    d.SearchAllDetails = new SearchAllDetail[0];
                }
                else
                {
                    d.SearchAllDetails =
                        (
                        from lic in grouppedLicense.Licenses
                        where // Exclude PAE internal licenses only 
                        !((string.IsNullOrWhiteSpace(lic.AuthorizationCode)
                        || lic.AuthorizationCode.StartsWith("K"))
                        && System.Text.RegularExpressions.Regex.IsMatch(lic.ModelName, "^(PAN-PRISMA-ACCESS|PAN-FEDRAMP|PAN-GPCS)(-|[0-9A-Z])*"))
                        select new SearchAllDetail
                        {
                            License = lic.LicenseNameDisplay,
                            AuthorizationCode = !string.IsNullOrWhiteSpace(lic.AuthorizationCode) ? lic.AuthorizationCode : "&nbsp;",
                            ExpirationDate = lic.ExpirationDate.HasValue ? lic.ExpirationDate.Value.ToShortDateString() : string.Empty,
                            IsPerpetual = lic.IsPerpetual,
                            DtExpirationDate = lic.ExpirationDate,
                            Quantity = (lic.ModelName?.Contains("LGS") ?? false) || (lic.ModelName?.Contains("CDL") ?? false) ? lic.SaasQuantity?.ToString() : null
                        }).ToArray();
                }
            }
            // Get PAE licenses
            licenseDataAccess.ReadMultipleDevicePaeLicenses(devices.Where(x => x.AllAssetType == EAllAssetType.Pae));
        }

        private void AddLicensesToTrapsDevices(SearchAllModel[] devices)
        {

            var grouppedLicenses = _epRepository.ReadMultipleDeviceLicenses(devices.Where(x => x.AssetType == AssetTypes.Traps).Select(x => x.Id).ToArray());

            foreach (var d in devices)
            {
                if (d.AssetType != AssetTypes.Traps)
                {
                    continue;
                }
                var grouppedLicense = grouppedLicenses.SingleOrDefault(x => x.DeviceId == d.Id && d.AssetType == AssetTypes.Traps);
                if (grouppedLicense == null)
                {
                    d.SearchAllDetails = new SearchAllDetail[0];
                }
                else
                {
                    var ascName = (from x in grouppedLicense.Licenses
                                   where !string.IsNullOrEmpty(x.AuthorizedSupportCenter)
                                   select x.AuthorizedSupportCenter).FirstOrDefault();

                    d.AuthorizedSupportCenter = ascName;

                    d.SearchAllDetails =
                        (
                        from lic in grouppedLicense.Licenses
                        select new SearchAllDetail
                        {
                            License = lic.License,
                            AuthorizationCode = lic.AuthorizationCode,
                            ELAAuthCode = lic.ELAAuthCode,
                            ExpirationDate = lic.ExpirationDate,
                            Quantity = lic.Quantity,
                        }).ToArray();
                }
            }
        }

        private void AddGroupsToDevices(SearchAllModel[] devices, long? supportAccountId)
        {

            long[] supportAccountIds = null;
            if (!supportAccountId.HasValue)
            {
                supportAccountIds = devices.Select(x => x.SupportAccountId).Distinct().ToArray();
            }

            var groupNames =
                supportAccountId.HasValue
                ? _deviceRepository.GetGroups(supportAccountId.Value)
                : _deviceRepository.GetGroups(supportAccountIds);

            if (!groupNames.Any())
            {
                return;
            }

            var deviceGroups =
                supportAccountId.HasValue
                ? _deviceRepository.GetDeviceGroups(supportAccountId.Value)
                : _deviceRepository.GetDeviceGroups(supportAccountIds);

            foreach (var d in devices)
            {

                var groups =
                    from g in groupNames
                    join dg in deviceGroups.Where(x => x.DeviceId == d.Id && (d.AssetType == AssetTypes.Prod || d.AssetType == AssetTypes.Spare)) on g.GroupId equals dg.GroupId
                    orderby g.GroupName
                    select g.GroupName;

                d.RawGroups = groups.ToArray();
            }
        }

        private SearchAllModel[] DoBigSearchAll(long? supportAccountId, long restrictedByUserAccountId,
                                                                                        DataParameters dataParameters)
        {
            return SearchAllV2(restrictedByUserAccountId, supportAccountId, dataParameters).ToArray();

            /*
            //large search alls can hog the DB so break them up
            const int maxRecsPerQuery = 1000;
            const int maxTotalQuery = 10000;

            dataParameters.PaginationCount = maxRecsPerQuery;
            var bigData = new List<SearchAllModel>();
            bool stillMoreData = true;

            while (stillMoreData)
            {

                var data = SearchAll(restrictedByUserAccountId, supportAccountId, dataParameters);

                if (dataParameters.TotalRecordCount > maxTotalQuery)
                {
                    throw new Exception("Data request too large, unable to run");
                }

                bigData.AddRange(data);

                var recsProcessed = dataParameters.PaginationOffset + dataParameters.PaginationCount;

                if (recsProcessed >= dataParameters.TotalRecordCount)
                {
                    stillMoreData = false;
                }
                else
                {
                    dataParameters.PaginationOffset += dataParameters.PaginationCount;
                }
            }

            return bigData.ToArray();
            */
        }

        public byte[] SearchAllCsv(long? supportAccountId, long restrictedByUserAccountId, string searchField, string searchValue)
        {
            var dataParameters = new DataParameters { SearchField = searchField, SearchValue = searchValue };

            var data = DoBigSearchAll(supportAccountId, restrictedByUserAccountId, dataParameters);

            var cc = new CsvContext();
            var outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = ',',
                FirstLineHasColumnNames = true,
                QuoteAllFields = true,
            };

            using (var ms = new MemoryStream())
            using (TextWriter tw = new StreamWriter(ms))
            {

                var searchAllCsv =
                    from d in data
                    from dd in d.SearchAllDetails.DefaultIfEmpty()
                    select new SearchAllAccountsModelCsv
                    {
                        SupportAccount = d.SupportAccountName,
                        AssetType = d.AssetType,
                        DeviceCode = d.DeviceCode,
                        ModelName = d.ModelName,
                        DeviceName = d.DeviceName,
                        License = dd != null ? dd.License : string.Empty,
                        AuthCode = dd != null ? dd.AuthorizationCode : string.Empty,
                        ExpirationDate = dd != null ? (dd.IsPerpetual ? "Perpetual" : dd.ExpirationDate) : string.Empty,
                        Quantity = dd != null ? dd.Quantity : (d.VmQuantity ?? string.Empty),
                        Location = d.CsvLocation,
                        Groups = d.CsvGroups,
                        DeviceTag = d.DeviceTag,
                        Asc = d.AuthorizedSupportCenter,
                        DeviceUrl = _configValues.GetAppSetting(AppSettings.ApplicationUrl) + "/SupportAccount/AccountDevices/" + d.SupportAccountId + "?assettype=" + d.AssetType + "&devicecode=" + d.DeviceCode,
                        SupportAccountUrl = _configValues.GetAppSetting(AppSettings.ApplicationUrl) + "/SupportAccount/AccountDevices/" + d.SupportAccountId + "?assettype=" + d.AssetType,
                    };

                if (supportAccountId.HasValue)
                {
                    var allAssets = AutoMapper.Mapper.Map<SearchAllAssetsModelCsv[]>(searchAllCsv);
                    cc.Write(allAssets, tw, outputFileDescription);
                }
                else
                {
                    cc.Write(searchAllCsv, tw, outputFileDescription);
                }

                tw.Flush();
                ms.Position = 0;

                return ms.ToArray();
            }
        }

        public IEnumerable<ActivationLinksLookupItem> GetActivationLinksLookup(DataParameters parameters, long supportAccountId, string search)
        {
            return _searchAllRepository.GetActivationLinksLookup(parameters, supportAccountId, search);
        }
    }
}
