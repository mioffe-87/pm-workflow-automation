using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using Csp.Api.DataAccess;
using Csp.Api.Model.Sfdc;
using MySql.Data.MySqlClient;
using System.Data;
using Csp.Api.Logic;

namespace Csp.Infrastructure.Data.Persistence
{
    public class SearchAllRepository : ISearchAllRepository
    {
        private static object locker = new object();
        private static Dictionary<string, string> panServicesActivationLinkWebConfigKeys = null;

        private readonly IConnectionStrings _connectionStrings;
        private readonly long _devPartTypeId;// = 3;
        private readonly long _vmPartMasterId;// = 3786;
        private readonly long _ossPartSubTypeId;// = 39;
        private readonly long _sparePartSubTypeId;// = 92;
        private readonly IEpRepository _epRepository;
        private const string _magnifierBetaStub = "PAN-PA-MGFR-AGENT";
        private const string _xdrbundlestub = "PAN-PA-BND-SAAS-SW";
        private const string _xdrAgent = "PAN-PA-XDR-AGENT";

        private readonly ISaasDataAccess _saasDataAccess;
        private readonly ISfdcEntitlementLogic _sfdcEntitlementLogic;
        private readonly ISupportAccountDevicesRepository _supportAccountDevicesRepository;

        private static Dictionary<string, string> PanSupportWebConfigKeys
        {
            get
            {
                if (panServicesActivationLinkWebConfigKeys == null)
                {
                    lock (locker)
                    {
                        if (panServicesActivationLinkWebConfigKeys == null)
                        {
                            panServicesActivationLinkWebConfigKeys = (new SettingsLogic().GetPanservicesWebConfigKeyValues("[A-Z]{1,10}ActivationLink") as Dictionary<string, string>)?
                                .Where(i => !string.IsNullOrWhiteSpace(i.Value)).ToDictionary(k => k.Key, v => v.Value);
                        }
                    }
                }
                return panServicesActivationLinkWebConfigKeys;
            }
        }

        public SearchAllRepository(IEpRepository epRepository, ISaasDataAccess saasDataAccess, ISfdcEntitlementLogic sfdcEntitlementLogic, ISupportAccountDevicesRepository supportAccountDevicesRepository, IConnectionStrings connectionStrings)
        {
            _connectionStrings = connectionStrings;
            var db = SessionDbContext.CspObjectContext;
            _epRepository = epRepository;
            _saasDataAccess = saasDataAccess;
            _sfdcEntitlementLogic = sfdcEntitlementLogic;
            _supportAccountDevicesRepository = supportAccountDevicesRepository;

            var devPartTypeIdQuery = (from pt in db.parttypes where pt.PanDbIdentification == SearchAllConts.DevPandDbId select pt.PartTypeId);
            var vmPartMasterIdQuery = (from pm in db.partmasters where pm.PartNumber == SearchAllConts.VmPartNumber select pm.PartMasterId);
            var ossPartSubTypeIdQuery = (from pst in db.partsubtypes where pst.PanDbIdentification == SearchAllConts.OssPanDbId select pst.PartSubtypeId);
            var sparePartSubTypeIdQuery = (from pst in db.partsubtypes where pst.PanDbIdentification == SearchAllConts.SparePanDbId select pst.PartSubtypeId);


            var ids = (
                from devPartTypeId in devPartTypeIdQuery
                from vmPartMasterId in vmPartMasterIdQuery
                from ossPartSubTypeId in ossPartSubTypeIdQuery
                from sparePartSubTypeId in sparePartSubTypeIdQuery
                select new
                {
                    devPartTypeId,
                    vmPartMasterId,
                    ossPartSubTypeId,
                    sparePartSubTypeId,
                }).Single();

            _devPartTypeId = ids.devPartTypeId;
            _vmPartMasterId = ids.vmPartMasterId;
            _ossPartSubTypeId = ids.ossPartSubTypeId;
            _sparePartSubTypeId = ids.sparePartSubTypeId;
        }

        private const long[] NoSupportAccountIdsFilter = null;
        private readonly long? NoTacFilter = null;
        private const IQueryable<long> NoDeviceIdsFilter = null;

        public SearchAllModel[] SearchAll(long userAccountId, long? supportAccountId, DataParameters parameters,
            List<RoleSecurity> roleSecurities, SearchAllFilters searchAllFilters, StringBuilder sb = null)
        {

            var db = SessionDbContext.CspObjectContext;

            db.Database.Log = s => Debug.WriteLine(s);

            //IQueryable<SearchAllModel> deviceData = null;
            SearchAllModel[] deviceData = null;

            var tacSupportAccountId = roleSecurities.GetTacSupportAccountId();
            var ascSupportAccountIds = roleSecurities.GetAscSupportAccountIds();
            var ascVisibleSupportAccountIds = GetAscVisibleSupportAccountIds(db, ascSupportAccountIds);

            if (searchAllFilters.SearchProd || searchAllFilters.SearchSpare)
            {
                deviceData = GetUnsecuredDeviceDataFromDb(userAccountId, supportAccountId, searchAllFilters, parameters, true)
                    .ToArray();

                //This is required because in the FilterSortAndPageDevices we are setting the TotalRecordCount from deviceData.Count().
                //However, in the above call to DB in GetUnsecuredDeviceDataFromDb we are only getting subset of data based on the pagination values. 
                //Say if the above DB call returns 10 rows, however TotalRecordCount is 1500. We want to consider 1500 as the TotalRecordCount plus the additional
                //Device data while will be obtained by Union with other data obtained in the other conditions below
                parameters.TotalRecordCount = parameters.TotalRecordCount - deviceData.Count();
                // Prisma Access Phase 1 data
                if (supportAccountId.HasValue
                    || ascVisibleSupportAccountIds != null
                    && ascVisibleSupportAccountIds.Any())
                {
                    var paeData = _supportAccountDevicesRepository.ReadSupportUserPae(userAccountId, supportAccountId.HasValue ? new long[] { supportAccountId.Value } : ascVisibleSupportAccountIds, parameters);
                    parameters.TotalRecordCount = parameters.TotalRecordCount - paeData.Count();
                    deviceData = deviceData != null && deviceData.Any() ? deviceData.Union(paeData, new SearchAllModelComparer()).ToArray() : paeData.ToArray();
                }
            }

            if (searchAllFilters.SearchTraps)
            {
                var trapsSupportAccountIds =
                    supportAccountId.HasValue
                        ? new[] { supportAccountId.Value }
                        : roleSecurities.GetTrapsSupportAccountIds();

                var trapsTacFilter =
                    supportAccountId.HasValue
                        ? NoTacFilter
                        : tacSupportAccountId;

                //var ascSupportAccountIds = roleSecurities.GetAscSupportAccountIds();

                //bool isAscAndCanSearchAcrossMultipleAccounts = ascSupportAccountIds.Any() && !supportAccountId.HasValue;

                var trapsData =
                    GetTrapsData(db, trapsSupportAccountIds, trapsTacFilter, supportAccountId, roleSecurities)
                        .ToArray();

                if (sb != null)
                    sb.AppendLine("Inside Search All - TrapsDeviceCount - " + trapsData.Count());

                deviceData = deviceData == null
                    ? trapsData
                    : deviceData.Union(trapsData, new SearchAllModelComparer()).ToArray();
            }

            if (searchAllFilters.SearchVms)
            {
                //VMs
                long[] vmSupportAccountIds;
                if (supportAccountId.HasValue)
                {
                    vmSupportAccountIds = new[] { supportAccountId.Value };
                }
                else
                {
                    vmSupportAccountIds = roleSecurities.GetVmSupportAccountIds();

                    //var ascSupportAccountIds = roleSecurities.GetAscSupportAccountIds();
                    //var ascVisibleSupportAccountIds = GetAscVisibleSupportAccountIds(db, ascSupportAccountIds);
                    vmSupportAccountIds = vmSupportAccountIds.Union(ascVisibleSupportAccountIds).ToArray();
                }

                var vmTacFilter =
                    supportAccountId.HasValue
                        ? NoTacFilter
                        : tacSupportAccountId;

                var vmData = GetVmData(db, vmSupportAccountIds, vmTacFilter, parameters.SearchField, parameters.SearchValue).ToArray();

                if (sb != null)
                    sb.AppendLine("Inside Search All - VmDeviceCount - " + vmData.Count());

                deviceData = deviceData == null
                    ? vmData
                    : deviceData.Union(vmData, new SearchAllModelComparer()).ToArray();
            }


            //XDR
            long[] cloudSupportAccountIds;
            if (supportAccountId.HasValue)
            {
                cloudSupportAccountIds = new[] { supportAccountId.Value };
            }
            else
            {
                cloudSupportAccountIds = roleSecurities.GetVmSupportAccountIds();

                //var ascSupportAccountIds = roleSecurities.GetAscSupportAccountIds();
                //var ascVisibleSupportAccountIds = GetAscVisibleSupportAccountIds(db, ascSupportAccountIds);
                //cloudSupportAccountIds = cloudSupportAccountIds.Union(ascVisibleSupportAccountIds).ToArray();
            }

            var cloudTacFilter =
                supportAccountId.HasValue
                    ? NoTacFilter
                    : tacSupportAccountId;

            var cloudData = GetEntitlementsFromNovaApi(cloudSupportAccountIds, cloudTacFilter, supportAccountId, userAccountId).ToArray();

            deviceData = deviceData == null
                ? cloudData
                : deviceData.Union(cloudData, new SearchAllModelComparer()).ToArray();

            //If they don't have any access, then return nothing
            if (deviceData == null)
            {
                return new SearchAllModel[0];
            }

            return FilterSortAndPageDevices(db, supportAccountId, deviceData, parameters);
        }

        public IEnumerable<SearchAllModel> SearchAllV2(long userAccountId, long? supportAccountId, DataParameters parameters, List<RoleSecurity> roleSecurities)
        {
            IEnumerable<SearchAllModel> callidusData = _supportAccountDevicesRepository.ReadSupportUserDevices_V2(userAccountId, supportAccountId, null, parameters);

            return callidusData;
            /* TODO: restore GraphQL entitlements
                        long[] cloudSupportAccountIds = supportAccountId.HasValue
                            ? new[] { supportAccountId.Value }
                            : roleSecurities.GetVmSupportAccountIds();
                        IEnumerable<SearchAllModel> steelbrickData = GetEntitlementsFromNovaApi(cloudSupportAccountIds, null, supportAccountId, userAccountId).ToArray();
                        if (parameters.SearchValue?.Length > 0) // apply search filter to cloud result
                        {
                            string sv = parameters.SearchValue.ToUpper();
                            if (parameters.SearchField == "DeviceCode") // search by SN
                            {
                                steelbrickData = steelbrickData.Where(i => i.DeviceCode?.ToUpper().Contains(parameters.SearchValue) ?? false);
                            }
                            else // There is no AuthCode concept in Steelbrick and other filter types are not implemented.
                            {
                                steelbrickData = new SearchAllModel[0];
                            }
                        }

                        return SearchAllV2MergeResult(callidusData, steelbrickData, parameters);
            */
        }

        /// <summary>
        /// Custom pagination parser for Callidus and Steelbrick merge.
        /// Callidus content: array of size of single page and total size lenth is in parameters.TotalRecordCount, Steelbrick size: all items array; not pageneted.
        /// ---SDDDDDDDDE-------|------------- -> case 1
        /// -------------SDDDDDD|DDDDE-------- -> case 2
        /// --------------------|--SDDDDDDDE-- -> case 3
        /// </summary>
        /// <param name="callidusData">Callidus data.</param>
        /// <param name="steelbrickData">Steelbrick data.</param>
        /// <param name="parameters">Parameters object.</param>
        /// <returns>Merged page of both sets with respect to DataParameters parameters.</returns>
        private IEnumerable<SearchAllModel> SearchAllV2MergeResult(IEnumerable<SearchAllModel> callidusData, IEnumerable<SearchAllModel> steelbrickData, DataParameters parameters)
        {
            if (callidusData == null)
            {
                callidusData = new SearchAllModel[0];
                parameters.TotalRecordCount = 0;
            }

            if (steelbrickData == null)
            {
                steelbrickData = new SearchAllModel[0];
            }

            List<SearchAllModel> result = new List<SearchAllModel>();
            int totalCallidus = (int)parameters.TotalRecordCount;
            int totalSteelbrick = steelbrickData.Count();
            parameters.TotalRecordCount = totalCallidus + totalSteelbrick;
            int offsetStart = (int)parameters.PaginationCount; //  ((int)parameters.PaginationOffset - 1) * (int)parameters.PaginationCount + 1;
            int offsetEnd = (int)parameters.PaginationOffset + (int)parameters.PaginationCount;
            if (offsetStart <= totalCallidus // case 2
                && offsetEnd > totalCallidus)
            {
                result.AddRange(callidusData);
                result.AddRange(steelbrickData.Take(parameters.PaginationCount - callidusData.Count()).ToArray());
            }
            else if (offsetEnd > totalCallidus) // case 3
            {
                result.AddRange(steelbrickData.Skip(offsetStart - totalCallidus - 1).Take(parameters.PaginationCount).ToArray());
            }
            else if (offsetEnd <= totalCallidus) // case 1
            {
                result.AddRange(callidusData);
            }
            else
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new ApplicationException("Should newer happen> offsetStart: " + offsetStart + ", offsetEnd: " + offsetEnd + ", callidusData: " + callidusData.Count() + ", steelbrickData: " + steelbrickData.Count() + ", parameters.PaginationCount: " + parameters.PaginationCount + ", parameters.PaginationOffset: " + parameters.PaginationOffset + ", parameters.TotalRecordCount: " + parameters.TotalRecordCount));
            }

            return result;
        }

        private IQueryable<SearchAllModel> GetUnsecuredDeviceData(cspEntities db, long[] filterSupportAccountIds, long? filterTacSupportAccountId, IQueryable<long> filterDeviceIds, SearchAllFilters searchAllFilters)
        {

            var supportAccountDevices = db.devices.Where(x => x.SupportAccountId.HasValue);

            if (filterSupportAccountIds != null)
            {
                supportAccountDevices = supportAccountDevices.Where(x => filterSupportAccountIds.Contains(x.SupportAccountId.Value));
            }
            if (filterTacSupportAccountId.HasValue)
            {
                supportAccountDevices = supportAccountDevices.Where(x => x.SupportAccountId.Value != filterTacSupportAccountId.Value);
            }

            if (filterDeviceIds != null)
            {
                supportAccountDevices =
                    from d in supportAccountDevices
                    join filter in filterDeviceIds on d.DeviceId equals filter
                    select d;
            }

            var ascLics =
                from x in db.licenses
                where
                x.IsActive
                && x.SupportCenterSupportAccountId.HasValue
                && x.DeviceId.HasValue
                select new AscLic { DeviceId = x.DeviceId.Value, SupportCenterSupportAccountId = x.SupportCenterSupportAccountId.Value };

            var filteredPartMaster = GetProdPartMaster(db, searchAllFilters);

            var deviceData =
                        from d in supportAccountDevices
                        join pm in filteredPartMaster on d.PartMasterId equals pm.PartMasterId

                        join co in db.countries on d.CountryId equals co.CountryId into tempCo
                        from leftCo in tempCo.DefaultIfEmpty()

                        join lasc in ascLics on d.DeviceId equals lasc.DeviceId into tempLasc
                        from leftLasc in tempLasc.DefaultIfEmpty()

                        join ascSa in db.supportaccounts on leftLasc.SupportCenterSupportAccountId equals ascSa.SupportAccountId into tempAscSa
                        from leftAscSa in tempAscSa.DefaultIfEmpty()

                            //join devcat in db.devicecategories on d.DeviceCategoryId equals devcat.DeviceCategoryId into tempDevcat
                            //from leftDevcat in tempDevcat.DefaultIfEmpty()

                        select new SearchAllModel
                        {
                            SupportAccountId = d.SupportAccountId.Value,
                            SupportAccountName = d.supportaccount.Name,
                            AssetType = (pm.PartSubtypeId == _ossPartSubTypeId || pm.PartSubtypeId == _sparePartSubTypeId) ? AssetTypes.Spare : AssetTypes.Prod,

                            Id = d.DeviceId,
                            DeviceCode = d.SerialNumber,
                            ModelName = pm.PartNumber,
                            DeviceName = d.DeviceName ?? string.Empty,

                            City = d.City,
                            PostalCode = d.PostalCode,
                            Region = d.Region ?? string.Empty,
                            Country = d.CountryId != null ? leftCo.Name : string.Empty,
                            // DeviceTag = leftDevcat != null ? leftDevcat.DeviceCategoryName : string.Empty,
                            AuthorizedSupportCenter = leftAscSa.Name,

                            VmDetails = new VmDetail
                            {
                                VmUseCount = null,
                                VmTotalCount = null,
                                IsNsx = null,
                                IsPerpetualNsx = null,
                            },
                        };

            return deviceData;
        }

        private IList<SearchAllModel> GetUnsecuredDeviceDataFromDb(long userAccountId, long? supportAccountId, SearchAllFilters searchAllFilters, DataParameters parameters, bool ignorePagination = false)
        {
            //string deviceType = searchAllFilters.SearchProd ? "PROD" :
            //    searchAllFilters.SearchSpare ? "SPARE" : string.Empty;
            string deviceType = string.Empty; //TODO: For now we will retrieve both SPARE AND PROD
            var deviceData = _supportAccountDevicesRepository.ReadSupportUserDevices(userAccountId, supportAccountId, deviceType, parameters, ignorePagination);

            return deviceData;
        }

        private IQueryable<FilteredPm> GetProdPartMaster(cspEntities db, SearchAllFilters searchAllFilters)
        {

            //var filteredPartType =
            //	from pt in db.parttypes
            //	where pt.PanDbIdentification == DevPandDbId
            //	select pt.PartTypeId;

            var filteredPst =
                from pst in db.partsubtypes
                select new
                {
                    pst.PartSubtypeId,
                    pst.PanDbIdentification
                };

            var filteredPartMaster =
                        from pm in db.partmasters

                        join pst in filteredPst on pm.PartSubtypeId equals pst.PartSubtypeId into tempPst
                        from leftPst in tempPst.DefaultIfEmpty()

                            //join pt in filteredPartType on pm.PartTypeId equals pt

                        where
                            pm.PartMasterId != _vmPartMasterId
                            && (pm.ProductFamily == null || pm.ProductFamily != SearchAllConts.TrapsProductFamily)
                            && pm.PartTypeId == _devPartTypeId
                             && pm.PartNumber != _magnifierBetaStub
                              && pm.PartNumber != _xdrAgent
                              && pm.PartNumber != _xdrbundlestub
                        select new FilteredPm
                        {
                            PartMasterId = pm.PartMasterId,
                            PartTypeId = pm.PartTypeId,
                            PartNumber = pm.PartNumber,
                            ModelCompatibility = pm.ModelCompatibility,
                            PartSubTypePanDbIdentification = leftPst.PanDbIdentification,
                            PartSubtypeId = pm.PartSubtypeId,
                        };

            if (searchAllFilters.SearchProd && searchAllFilters.SearchSpare)
            {
                //Do Nothing
            }
            else
            {

                //If ONLY prod
                if (searchAllFilters.SearchProd)
                {
                    filteredPartMaster = filteredPartMaster.Where(x =>
                                                                                                                x.PartSubtypeId == null ||
                                                                                                                (x.PartSubtypeId != _ossPartSubTypeId &&
                                                                                                                 x.PartSubtypeId != _sparePartSubTypeId));
                }

                //If ONLY spare
                if (searchAllFilters.SearchSpare)
                {
                    filteredPartMaster = filteredPartMaster.Where(x =>
                                                                                                                x.PartSubtypeId == _ossPartSubTypeId ||
                                                                                                                x.PartSubtypeId == _sparePartSubTypeId);
                }
            }

            return filteredPartMaster;
        }

        private IQueryable<long> GetAscVisibleDeviceIds(cspEntities db, long[] ascSupportAccountIds)
        {

            var ascLicDeviceIds =
                from lic in db.licenses
                where
                    lic.IsActive
                    && lic.DeviceId.HasValue
                    && lic.SupportCenterSupportAccountId.HasValue
                    && ascSupportAccountIds.Contains(lic.SupportCenterSupportAccountId.Value)
                select lic.DeviceId.Value;


            return ascLicDeviceIds.Distinct();
        }

        private IEnumerable<long> GetAscVisibleSupportAccountIds(cspEntities db, long[] ascSupportAccountIds)
        {

            var ascLicSupportAccountIds =
                from lic in db.licenses
                join dev in db.devices on lic.DeviceId equals dev.DeviceId
                where
                    lic.IsActive
                    && lic.SupportCenterSupportAccountId.HasValue
                    && dev.SupportAccountId.HasValue
                    && ascSupportAccountIds.Contains(lic.SupportCenterSupportAccountId.Value)
                select dev.SupportAccountId.Value;

            return ascLicSupportAccountIds.Distinct().ToArray();
        }

        private IEnumerable<string> GetAscSalesforceAccountIds(cspEntities db, long[] ascSupportAccountIds)
        {
            var ascSalesforceAccountIds =
                from sa in db.supportaccounts
                where ascSupportAccountIds.Contains(sa.SupportAccountId)
                select sa.SalesforceAccountId;

            return ascSalesforceAccountIds.Distinct().ToArray();
        }

        private IQueryable<supportaccount> GetSupportAccountById(cspEntities db, long[] supportAccountIds)
        {
            var ascSalesforceAccountIds =
                from sa in db.supportaccounts
                where supportAccountIds.Contains(sa.SupportAccountId)
                select sa;

            return ascSalesforceAccountIds;
        }

        private IQueryable<long> GetGroupVisibleDeviceIds(cspEntities db, long[] groupMembershipIds)
        {

            var deviceIds =
                from gd in db.groupdevices
                where groupMembershipIds.Contains(gd.GroupId)
                select gd.DeviceId;

            return deviceIds;
        }

        private IQueryable<long> GetAllGroupedDeviceIdsForStdExclusion(cspEntities db, long[] supportAccountIds)
        {

            var deviceIds =
                from gd in db.groupdevices
                join sag in db.supportaccountgroups on gd.GroupId equals sag.GroupId
                where supportAccountIds.Contains(sag.SupportAccountId)
                //select new GroupDeviceId { DeviceId = gd.DeviceId };  //TODO maybe comment back in if needed
                select gd.DeviceId;

            return deviceIds;
        }

        private List<SearchAllModel> GetVmData(cspEntities db, long[] supportAccountIds, long? tacSupportAccountId, string searchField, string searchValue)
        {
            const string sql = @"
SELECT d.SupportAccountId, sa.Name, l.LicenseId, d.SerialNumber, lpm.Description, l.VMUseCount, l.VMQuantity, lpm.PartNumber, lpst.PanDbIdentification, lpst.LicenseType, l.DeviceId 
    FROM  device d
    JOIN license l ON d.DeviceId = l.VMStubDeviceId
    JOIN partmaster lpm ON l.PartMasterId = lpm.PartMasterId
    JOIN partsubtype lpst ON lpm.PartSubtypeId = lpst.PartSubtypeId
    JOIN partmaster dpm ON d.PartMasterId = dpm.PartMasterId
    JOIN supportaccount sa ON d.SupportAccountId = sa.SupportAccountId
    WHERE {0}
    d.SupportAccountId IN ({1})
    AND (dpm.PartNumber = 'PAN-PA-VM-SW' OR lpst.PanDbIdentification LIKE 'PHOENIX%')
    AND (((l.DeviceId IS NULL 
    AND l.VMUseCount = 0 
    AND l.VMParentLicenseId IS NULL) 
    OR !l.AuthorizationCode LIKE 'VM%') 
    AND (l.VMParentLicenseId IS null 
    OR l.VMParentLicenseId = 0));
";
            string querySearch = "";
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                if (searchField == "DeviceCode")
                {
                    querySearch = "d.SerialNumber = '" + searchValue + "' AND ";
                }
                if (searchField == "AuthorizationCodes")
                {
                    querySearch = "l.AuthorizationCode = '" + searchValue + "' AND ";
                }
                if (searchField == "ModelName")
                {
                    querySearch = "lpm.PartNumber LIKE '%" + searchValue + "%' AND ";
                }
                if (searchField == "DeviceName")
                {
                    querySearch = "lpm.Description LIKE '%" + searchValue + "%' AND ";
                }
                if (searchField == "ExpiresBy"
                    || searchField == "DeviceTag"
                    || searchField == "AuthorizedSupportCenter"
                    || searchField == "Location")
                {
                    // defer to the filtering logic after. TODO:?
                }
            }

            Dictionary<string, long> keys = supportAccountIds.Distinct().ToDictionary(k => "@cspId" + k, v => v);
            string query = string.Format(sql, querySearch, string.Join(", ", keys.Keys));
            List<SearchAllModel> model = new List<SearchAllModel>();
            try
            {
                using (var sqlConn = new MySqlConnection(_connectionStrings.CspDatabase))
                {
                    sqlConn.Open();
                    using (var sqlCmd = new MySqlCommand(query, sqlConn))
                    {
                        try
                        {
                            sqlCmd.CommandType = CommandType.Text;
                            sqlCmd.CommandTimeout = 300;
                            //sqlCmd.Parameters.AddWithValue("?p_tacSupportAccountId", tacSupportAccountId);
                            foreach (KeyValuePair<string, long> key in keys)
                            {
                                sqlCmd.Parameters.AddWithValue(key.Key, key.Value);
                            }

                            using (var dataReader = sqlCmd.ExecuteReader())
                            {
                                while (dataReader.Read())
                                {
                                    var isPanorama = SearchAllConts.PanoramaIds.Contains(dataReader["PanDbIdentification"] != DBNull.Value ? Convert.ToString(dataReader["PanDbIdentification"]) : string.Empty);
                                    var isPerpetualPanorama =
                                        (SearchAllConts.PanoramaIds.Contains(dataReader["PanDbIdentification"] != DBNull.Value ? Convert.ToString(dataReader["PanDbIdentification"]) : string.Empty) &&
                                         (dataReader["LicenseType"] != DBNull.Value && Convert.ToBoolean(dataReader["LicenseType"])).Equals(true));

                                    model.Add(new SearchAllModel
                                    {
                                        SupportAccountId = dataReader["SupportAccountId"] == DBNull.Value ? 0 : Convert.ToInt64(dataReader["SupportAccountId"]),
                                        SupportAccountName = dataReader["Name"] == DBNull.Value ? string.Empty : Convert.ToString(dataReader["Name"]),
                                        AssetType = AssetTypes.Vm,
                                        Id = dataReader["LicenseId"] == DBNull.Value ? 0 : Convert.ToInt64(dataReader["LicenseId"]),
                                        DeviceCode = dataReader["SerialNumber"] == DBNull.Value ? string.Empty : Convert.ToString(dataReader["SerialNumber"]),
                                        //DeviceCode = dataReader["AuthorizationCode"] == DBNull.Value ? string.Empty : Convert.ToString(dataReader["AuthorizationCode"]),
                                        ModelName = dataReader["PartNumber"] == DBNull.Value ? string.Empty : Convert.ToString(dataReader["PartNumber"]),
                                        DeviceName = dataReader["Description"] == DBNull.Value ? string.Empty : Convert.ToString(dataReader["Description"]),
                                        City = string.Empty,
                                        PostalCode = string.Empty,
                                        Region = string.Empty,
                                        Country = string.Empty,
                                        DeviceTag = string.Empty,
                                        AuthorizedSupportCenter = string.Empty,
                                        VmDetails = new VmDetail
                                        {
                                            VmUseCount = ((dataReader["VMUseCount"] != DBNull.Value && Convert.ToInt32(dataReader["VMUseCount"]) == 0) && (dataReader["DeviceId"] == DBNull.Value) && (dataReader["VMQuantity"] != DBNull.Value && (Convert.ToInt32(dataReader["VMQuantity"])) == 0)) ? 1 : (dataReader["VMUseCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["VMUseCount"])),
                                            VmTotalCount = ((dataReader["VMQuantity"] != DBNull.Value && Convert.ToInt32(dataReader["VMQuantity"]) == 0) && (isPanorama == false || isPerpetualPanorama == true)) ? 1 : (dataReader["VMQuantity"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["VMQuantity"])),
                                            IsNsx = isPanorama ? 1 : 0,
                                            IsPerpetualNsx = isPerpetualPanorama ? 1 : 0,
                                        }
                                    });
                                }
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                            Elmah.ErrorSignal.FromCurrentContext().Raise(new ApplicationException("SearchAllRepository::GetVmData: " + string.Join(", ", supportAccountIds ?? new long[0]) + ", " + tacSupportAccountId + ", " + searchField + ", " + searchValue + ", " + query));
                        }
                    }
                }
            }
            catch (Exception exl)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(exl);
                Elmah.ErrorSignal.FromCurrentContext().Raise(new ApplicationException("SearchAllRepository::GetVmData2: " + string.Join(", ", supportAccountIds ?? new long[0]) + ", " + tacSupportAccountId + ", " + searchField + ", " + searchValue));
                throw;
            }


            return model.Distinct().ToList();
        }

        private IQueryable<SearchAllModel> GetTrapsData(cspEntities db, long[] trapsSupportAccountIds, long? trapsTacFilter, long? supportAccountId, List<RoleSecurity> rolesSecurity)
        {

            bool isPartOfSupportAccount = false;
            var ascSupportAccountIds = rolesSecurity.GetAscSupportAccountIds();

            bool isAsc = ascSupportAccountIds.Any();

            bool isSearchAcrossAllAccounts = !supportAccountId.HasValue && isAsc;


            if (supportAccountId.HasValue && (rolesSecurity.GetStandardSupportAccountIds().Contains(supportAccountId.Value) ||
                rolesSecurity.GetAdminSupportAccountIds().Contains(supportAccountId.Value) ||
                rolesSecurity.GetTrapsSupportAccountIds().Contains(supportAccountId.Value)))
            {
                isPartOfSupportAccount = true;
            }


            var filteredDevices =
                    from d in db.devices
                    where d.IsActive
                    && d.SupportAccountId.HasValue
                    select d;

            filteredDevices =
                trapsTacFilter.HasValue
                    ? filteredDevices.Where(d => trapsSupportAccountIds.Contains(d.SupportAccountId.Value) || d.SupportAccountId.Value != trapsTacFilter)
                    : filteredDevices.Where(d => trapsSupportAccountIds.Contains(d.SupportAccountId.Value));

            if (isSearchAcrossAllAccounts)
            {
                var ascVisibleEpDevices =
                    from d in db.devices
                    join l in db.licenses on d.DeviceId equals l.DeviceId
                    join lpm in db.partmasters on l.PartMasterId equals lpm.PartMasterId
                    join lpst in db.partsubtypes on lpm.PartSubtypeId equals lpst.PartSubtypeId
                    join eplt in db.endpointlicensetypes on lpst.PanDbIdentification equals eplt.licensetype
                    where
                        l.IsActive && (l.BundleLicenseId == 0 || l.BundleLicenseId == null)
                        && lpm.ModelCompatibility.Contains(EndpointConstants.ModelCompat)
                        && ascSupportAccountIds.Contains(l.SupportCenterSupportAccountId.Value)
                    select d;
                filteredDevices = filteredDevices.Union(ascVisibleEpDevices);
            }
            else if (isAsc && !isPartOfSupportAccount)
            {
                var ascVisibleEpDevices =
                    from d in db.devices
                    join l in db.licenses on d.DeviceId equals l.DeviceId
                    join lpm in db.partmasters on l.PartMasterId equals lpm.PartMasterId
                    join lpst in db.partsubtypes on lpm.PartSubtypeId equals lpst.PartSubtypeId
                    join eplt in db.endpointlicensetypes on lpst.PanDbIdentification equals eplt.licensetype
                    where
                        l.IsActive && (l.BundleLicenseId == 0 || l.BundleLicenseId == null)
                        && lpm.ModelCompatibility.Contains(EndpointConstants.ModelCompat)
                        && ascSupportAccountIds.Contains(l.SupportCenterSupportAccountId.Value)
                        && d.IsActive && d.SupportAccountId.HasValue && trapsSupportAccountIds.Contains(d.SupportAccountId.Value)
                    select d;
                filteredDevices = ascVisibleEpDevices;
            }


            var epData =
                from d in filteredDevices
                join l in db.licenses on d.DeviceId equals l.DeviceId
                join lpm in db.partmasters on l.PartMasterId equals lpm.PartMasterId
                join lpst in db.partsubtypes on lpm.PartSubtypeId equals lpst.PartSubtypeId
                join eplt in db.endpointlicensetypes on lpst.PanDbIdentification equals eplt.licensetype

                join dc in db.devicecategories on d.DeviceCategoryId equals dc.DeviceCategoryId into tempDc
                from leftDc in tempDc.DefaultIfEmpty()

                where
                    l.IsActive && (l.BundleLicenseId == 0 || l.BundleLicenseId == null)
                    && lpm.ModelCompatibility.Contains(EndpointConstants.ModelCompat)

                //TODO verify these orderby's work the same way as the old code (they might be backwards... if so switch == to !=)
                orderby
                    d.DeviceId,
                    eplt.sortindex


                select new SearchAllModel
                {
                    SupportAccountId = d.SupportAccountId.Value,
                    SupportAccountName = d.supportaccount.Name,
                    AssetType = AssetTypes.Traps,

                    Id = d.DeviceId,
                    DeviceCode = d.SerialNumber,
                    ModelName = string.Empty,
                    DeviceName = d.DeviceName ?? string.Empty,

                    City = string.Empty,
                    PostalCode = string.Empty,
                    Region = string.Empty,
                    Country = string.Empty,
                    DeviceTag = leftDc == null ? string.Empty : (leftDc.DeviceCategoryName ?? string.Empty),
                    AuthorizedSupportCenter = string.Empty,

                    VmDetails = new VmDetail
                    {
                        VmUseCount = null,
                        VmTotalCount = null,
                        IsNsx = null,
                        IsPerpetualNsx = null,
                    },


                    //SerialNumber = d.SerialNumber,
                    //AuthorizationCode = l.AuthorizationCode,
                    //Description = lpm.Description,
                    //UseCount = l.VMUseCount == 0 ? (int?)null : l.VMUseCount,
                    //DeviceId = d.DeviceId,
                    //RegistrationDate = d.RegistrationDate,
                    //ExpirationDate = l.ExpirationDate,
                    //PartSubType = lpst.PanDbIdentification,
                    //PartSubTypeName = lpst.Name,
                    //LicenseFeatureName = lpst.LicenseFeaturesName.ToUpper(),
                    //DeviceName = d.DeviceName ?? string.Empty,
                    //DeviceTag = leftDc == null ? string.Empty : (leftDc.DeviceCategoryName ?? string.Empty),
                    //LicenseName = lpst.Name,
                };

            return epData.Distinct();
        }

        private IEnumerable<SearchAllModel> FilterPaeData(string filterType, IEnumerable<SearchAllModel> data, long? supportAccountId, string value)
        {
            if (!data?.Any() ?? true)
            {
                return new SearchAllModel[0];
            }

            IEnumerable<long> ascSupportAccounts = null;

            if (!supportAccountId.HasValue // ASC case
                && data != null
                && data.Any())
            {
                ascSupportAccounts = data.Select(i => i.SupportAccountId).Distinct().ToArray();
                if (!ascSupportAccounts.Any())
                {
                    return new SearchAllModel[0];
                }
            }

            if (!supportAccountId.HasValue
                && ascSupportAccounts == null)
            {
                return new SearchAllModel[0];
            }

            // search by AuthCode
            const string sqlAuthCode = @"
SELECT d.DeviceId 
  FROM license l 
  JOIN device d ON l.DeviceId = d.DeviceId
  WHERE l.AuthorizationCode = @p_AuthCode
  AND d.IsActive = 1
  AND l.IsActive = 1
  AND l.SupportAccountId IN ({0});
";
            // search by model name
            const string sqlModelName = @"
SELECT DISTINCT d.DeviceId 
  FROM license l 
  JOIN partmaster p ON l.PartMasterId = p.PartMasterId
  JOIN device d ON l.DeviceId = d.DeviceId
  WHERE p.PartNumber LIKE @p_ModelName
  AND d.IsActive = 1
  AND l.IsActive = 1
  AND l.SupportAccountId IN ({0});
";
            // search by expires by
            const string sqlExpiresBy = @"
SELECT DISTINCT d.DeviceId 
  FROM license l 
  JOIN device d ON l.DeviceId = d.DeviceId
  WHERE l.ExpirationDate < @p_ByDate
  AND d.IsActive = 1
  AND l.IsActive = 1
  AND l.SupportAccountId IN ({0});
";
            // search by authorized support center
            const string sqlAuthorizedSupportCenter = @"
SELECT DISTINCT d.DeviceId 
  FROM license l 
  JOIN device d ON l.DeviceId = d.DeviceId
  WHERE l.SupportCenterSupportAccountId = @p_SupportCenterId
  AND d.IsActive = 1
  AND l.IsActive = 1
  AND l.SupportAccountId IN ({0});
";
            List<long> deviceIds = new List<long>();
            Dictionary<string, long> supportAccountParams = supportAccountId.HasValue
                ? new Dictionary<string, long>() { { "@saId" + supportAccountId.Value, supportAccountId.Value } } // single account
                : ascSupportAccounts?.ToDictionary(k => "@saId" + k); // ASC case
            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand { CommandType = CommandType.Text, Connection = connection })
                {
                    supportAccountParams?.ToList().ForEach(i => command.Parameters.AddWithValue(i.Key, i.Value)); // ASC case
                    if (filterType == "modelName")
                    {
                        command.Parameters.AddWithValue("@p_ModelName", string.Format("%{0}%", value));
                        command.CommandText = string.Format(sqlModelName, string.Join(", ", supportAccountParams.Keys));
                    }
                    else if (filterType == "expiresBy")
                    {
                        command.Parameters.AddWithValue("@p_ByDate", Convert.ToDateTime(value).Date.ToString("yyyy-MM-dd"));
                        command.CommandText = string.Format(sqlExpiresBy, string.Join(", ", supportAccountParams.Keys));
                    }
                    else if (filterType == "authorizedSupportCenter")
                    {
                        command.Parameters.AddWithValue("@p_SupportCenterId", value);
                        command.CommandText = string.Format(sqlAuthorizedSupportCenter, string.Join(", ", supportAccountParams.Keys));
                    }
                    else // default search is by AuthCode
                    {
                        command.Parameters.AddWithValue("@p_AuthCode", value);
                        command.CommandText = string.Format(sqlAuthCode, string.Join(", ", supportAccountParams.Keys));
                    }
                    using (var dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            var deviceId = dataReader["DeviceId"] != DBNull.Value ? Convert.ToInt64(dataReader["DeviceId"]) : 0;
                            if (deviceId > 0)
                            {
                                deviceIds.Add(deviceId);
                            }
                        }
                    }
                }
            }

            return deviceIds.Any()
                ? data.Where(d => deviceIds.Any(i => i == d.Id)).ToArray()
                : new SearchAllModel[0];
        }

        private SearchAllModel[] FilterSortAndPageDevices(cspEntities db, long? supportAccountId, IEnumerable<SearchAllModel> deviceData, DataParameters parameters)
        {

            if (!string.IsNullOrEmpty(parameters.SearchField) && !string.IsNullOrEmpty(parameters.SearchValue))
            {
                IEnumerable<SearchAllModel> tmpDeviceData = null;
                long bundlelicenseid = 0;
                switch (parameters.SearchField)
                {
                    case "DeviceCode":
                        var fdeviceData = deviceData.Where(x => x.DeviceCode.ToLower().Contains(parameters.SearchValue.ToLower())); // && x.AssetType != AssetTypes.Vm);

                        bundlelicenseid = _saasDataAccess.GetBundlelicenseIdBySNAuthcode(parameters.SearchValue.ToLower(), null);

                        if (bundlelicenseid > 0) //XDR bundle 
                        {
                            //get bundle license and append to filtered data
                            var cdeviceData = deviceData.Where(x =>
                                x.BundleLicenseId == Convert.ToInt64(bundlelicenseid) && x.AssetType == AssetTypes.CloudServices);

                            deviceData = fdeviceData.Union(cdeviceData);
                        }
                        else
                        {
                            deviceData = fdeviceData;
                        }

                        break;

                    case "AuthorizationCodes":
                        AddLicenesDataToTrapsDevices(deviceData.ToArray());
                        bundlelicenseid = _saasDataAccess.GetBundlelicenseIdBySNAuthcode(null, parameters.SearchValue.ToLower());
                        try
                        {
                            if (bundlelicenseid > 0)
                            {
                                tmpDeviceData = deviceData.Where(x => x.BundleLicenseId == Convert.ToInt64(bundlelicenseid) && x.AssetType == AssetTypes.CloudServices);
                            }
                            else
                            {
                                var authCodeDevice = (from lic in db.licenses
                                                      where lic.IsActive
                                                      && lic.AuthorizationCode == parameters.SearchValue
                                                      && lic.DeviceId.HasValue
                                                      select lic.DeviceId).SingleOrDefault();
                                tmpDeviceData = deviceData.Where(x => x.DeviceCode.ToLower().Contains(parameters.SearchValue.ToLower()) && x.AssetType == AssetTypes.Vm
                                    || (authCodeDevice.HasValue && x.Id == authCodeDevice.Value && x.AssetType == AssetTypes.Prod)
                                    || (x.AssetType == AssetTypes.Traps
                                    && (authCodeDevice.HasValue
                                    && x.Id == authCodeDevice.Value
                                    || x.SearchAllDetails.Where(y => y.ELAAuthCode != null && y.ELAAuthCode.ToLower().Contains(parameters.SearchValue.ToLower())).Any())));
                            }

                            var paeData = FilterPaeData("authCode", deviceData, supportAccountId, parameters.SearchValue.Trim());
                            deviceData = paeData.Any() ? tmpDeviceData.Union(paeData) : tmpDeviceData;
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.Message.ToString());
                        }
                        break;

                    case "ExpiresBy":
                        DateTime dtExpiresBy;
                        if (DateTime.TryParse(parameters.SearchValue, out dtExpiresBy))
                        {

                            var supportAccountIds = deviceData.Select(x => x.SupportAccountId).Distinct().ToArray();

                            var expiringLics = (
                                from l in db.licenses
                                join d in db.devices on l.DeviceId equals d.DeviceId
                                join pm in db.partmasters on l.PartMasterId equals pm.PartMasterId
                                join pst in db.partsubtypes on pm.PartSubtypeId equals pst.PartSubtypeId
                                let isPerpetual = (pst != null && pst.LicenseType == true)
                                where
                                    l.IsActive
                                    && supportAccountIds.Contains(d.SupportAccountId.Value)
                                    && isPerpetual == false
                                    && l.ExpirationDate <= dtExpiresBy
                                select l.DeviceId).ToArray();

                            tmpDeviceData = deviceData.Where(x => (x.AssetType == AssetTypes.Prod || x.AssetType == AssetTypes.Traps) && expiringLics.Contains(x.Id));
                            var paeData = FilterPaeData("expiresBy", deviceData, supportAccountId, parameters.SearchValue.Trim());
                            deviceData = paeData.Any() ? tmpDeviceData.Union(paeData) : tmpDeviceData;
                        }
                        break;

                    case "SupportAccountName":
                        deviceData = deviceData.Where(x => x.SupportAccountName.ToLower().Contains(parameters.SearchValue.ToLower()));
                        break;

                    case "ModelName":
                        tmpDeviceData = deviceData.Where(x => x.ModelName != null && x.ModelName.ToLower().Contains(parameters.SearchValue.ToLower()));
                        var paeDataModelName = FilterPaeData("modelName", deviceData, supportAccountId, parameters.SearchValue.Trim());
                        deviceData = paeDataModelName.Any() ? tmpDeviceData.Union(paeDataModelName) : tmpDeviceData;
                        break;

                    case "DeviceName":
                        deviceData = deviceData.Where(x => x.DeviceName != null && x.DeviceName.ToLower().Contains(parameters.SearchValue.ToLower()));
                        break;

                    case "DeviceTag":
                        var decodedDeviceTag = ApiCommon.DecodeString(parameters.SearchValue.ToLower());
                        deviceData = deviceData.Where(x => x.DeviceTag.ToLower() == decodedDeviceTag);
                        break;

                    case "AuthorizedSupportCenter":
                        tmpDeviceData = deviceData.Where(x => x.AuthorizedSupportCenter != null && x.AuthorizedSupportCenter.ToLower().Contains(parameters.SearchValue.ToLower()));
                        var paeDataAuthorizedSupportCenter = FilterPaeData("authorizedSupportCenter", deviceData, supportAccountId, parameters.SearchValue.Trim());
                        deviceData = paeDataAuthorizedSupportCenter.Any() ? tmpDeviceData.Union(paeDataAuthorizedSupportCenter) : tmpDeviceData;
                        break;

                    case "Location":
                        deviceData = deviceData.Where(x =>
                            (x.City != null && x.City.ToLower().Contains(parameters.SearchValue.ToLower()))
                            || (x.PostalCode != null && x.PostalCode.ToLower().Contains(parameters.SearchValue.ToLower()))
                            || (x.Region != null && x.Region.ToLower().Contains(parameters.SearchValue.ToLower()))
                            || (x.Country != null && x.Country.ToLower().Contains(parameters.SearchValue.ToLower())));
                        break;
                }
            }

            switch (parameters.SortField)
            {

                case "SupportAccountName":
                    deviceData = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
                                                                ? deviceData.OrderByDescending(u => u.SupportAccountName)
                                                                : deviceData.OrderBy(u => u.SupportAccountName);
                    break;

                case "AssetType":
                    deviceData = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
                                                                ? deviceData.OrderByDescending(u => u.AssetType)
                                                                : deviceData.OrderBy(u => u.AssetType);
                    break;

                case "DeviceCode":
                    deviceData = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
                                                                ? deviceData.OrderByDescending(u => u.DeviceCode)
                                                                : deviceData.OrderBy(u => u.DeviceCode);
                    break;

                case "ModelName":
                    deviceData = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
                                                                ? deviceData.OrderByDescending(u => u.ModelName)
                                                                : deviceData.OrderBy(u => u.ModelName);
                    break;

                case "DeviceName":
                    deviceData = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
                                                                ? deviceData.OrderByDescending(u => u.DeviceName)
                                                                : deviceData.OrderBy(u => u.DeviceName);
                    break;

                case "HtmlLocation":
                    deviceData = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
                                                                ? deviceData.OrderByDescending(u => u.City).ThenByDescending(u => u.PostalCode).ThenByDescending(u => u.Region).ThenByDescending(u => u.Country)
                                                                : deviceData.OrderBy(u => u.City).ThenBy(u => u.PostalCode).ThenBy(u => u.Region).ThenBy(u => u.Country);
                    break;

                case "DeviceTag":
                    deviceData = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
                                                                ? deviceData.OrderByDescending(u => u.DeviceTag)
                                                                : deviceData.OrderBy(u => u.DeviceTag);
                    break;

                case "AuthorizedSupportCenter":
                    deviceData = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
                                                                ? deviceData.OrderByDescending(u => u.AuthorizedSupportCenter)
                                                                : deviceData.OrderBy(u => u.AuthorizedSupportCenter);
                    break;

                default:
                    deviceData = deviceData.OrderBy(x => x.AssetType).ThenBy(x => x.Id);
                    break;
            }

            parameters.TotalRecordCount = deviceData.Count();
            if (parameters.SupportsPaging)
            {
                deviceData = deviceData.Skip((int)parameters.PaginationOffset).Take((int)parameters.PaginationCount);
            }

            return deviceData.ToArray();
        }


        private class FilteredPm
        {
            public long PartMasterId { get; set; }
            public long? PartTypeId { get; set; }
            public long? PartSubtypeId { get; set; }
            public string PartNumber { get; set; }
            public string ModelCompatibility { get; set; }
            public string PartSubTypePanDbIdentification { get; set; }
        }

        private class AscLic
        {
            public long DeviceId { get; set; }
            public long SupportCenterSupportAccountId { get; set; }
        }

        private void AddLicenesDataToTrapsDevices(SearchAllModel[] devices)
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


        private IEnumerable<SearchAllModel> GetEntitlementsFromNovaApi(long[] cloudSupportAccountIds, long? tacSupportAccountId, long? supportAccountId, long userAccountId)
        {
            UserAccountDataAccess userAccountDataAccess = new UserAccountDataAccess();
            var ascSalesforceAccountId = userAccountDataAccess.GetAscSalesForceAccountIdsByUserAccountId(userAccountId);

            bool isAsc = ascSalesforceAccountId != null && ascSalesforceAccountId.Any();
            bool isSearchAcrossAllAccounts = !supportAccountId.HasValue && isAsc;
            long count = 1000000;

            List<long> accessibleAccountIds = new List<long>();
            accessibleAccountIds.Add(supportAccountId.HasValue ? supportAccountId.Value : tacSupportAccountId.HasValue ? tacSupportAccountId.Value : 0);

            if (cloudSupportAccountIds.Length > 0)
                accessibleAccountIds.AddRange(cloudSupportAccountIds);

            IEnumerable<SearchAllModel> result = new List<SearchAllModel>();
            IEnumerable<AvailableSaaSActivations> items = new List<AvailableSaaSActivations>();

            IEnumerable<AvailableSaaSActivations> xdrs = _sfdcEntitlementLogic.AddXdrEntitlements(items, accessibleAccountIds.ToArray(), null, false);

            IEnumerable<AvailableSaaSActivations> ascXdrs = _sfdcEntitlementLogic.AddXdrEntitlements(items, accessibleAccountIds.ToArray(),
                isSearchAcrossAllAccounts ? (ascSalesforceAccountId.Any() ? ascSalesforceAccountId : null) : null,
                true);
            //Here we are fetching expanse products with productid and filter with accounts ids
            // No ASC support at this time for Expanse customers
            IEnumerable<AvailableSaaSActivations> expanseProds = _sfdcEntitlementLogic.AddExpanseEntitlements(items, accessibleAccountIds.ToArray(), null, false);

            if (xdrs.Any())
            {
                xdrs = (ascXdrs.Any() ? xdrs.Union(ascXdrs.Where(a => !xdrs.Any(x => x.SerialNumber == a.SerialNumber)/*do not merge the same SN*/)) : xdrs).ToArray();
                //Adding expanse product to exiting data if data available
                xdrs = (expanseProds.Any() ? xdrs.Union(expanseProds) : xdrs).ToArray();
            }
            else
            {
                xdrs = (ascXdrs.Any() ? ascXdrs : xdrs).ToArray();
                //Adding expanse product to exiting data if data available
                xdrs = (expanseProds.Any() ? expanseProds : xdrs).ToArray();
            }
            result = xdrs.Select(i => new SearchAllModel
            {
                Id = --count,
                SupportAccountId = i.SupportAccountId ?? -1,
                SupportAccountName = i.SupportAccountName,
                AuthorizedSupportCenter = i.AscAccountName,
                AssetType = "Cloud Service",
                DeviceCode = i.SerialNumber,
                ModelName = i.ModelName,
                DeviceName = string.Empty,
                Licenses = i.Description,
                Region = i.Region,
                AllAssetType = i.AssetType,
                SearchAllDetails = new SearchAllDetail[] { new SearchAllDetail { Quantity = i.Quantity } },
                BundleLicenses = new List<BundleLicenseItem> { new BundleLicenseItem { ExpirationDate = i.ExpirationDate } },
                AuthorizationCodes = i.AuthorizationCode,
                ChildLicenses = i.Children != null && i.Children.Any()
                ? i.Children.Select(c => new SearchAllDetail
                {
                    AuthorizationCode = c.AuthorizationCode,
                    ExpirationDate = c.ExpirationDate,
                    Model = c.ModelName,
                    License = c.Description,
                    Quantity = c.Quantity
                }).ToArray()
                : null,
                DeviceTag = string.Empty
            });


            return result;
        }

        public IEnumerable<ActivationLinksLookupItem> GetActivationLinksLookup(DataParameters parameters, long supportAccountId, string search)
        {
            const string querySupportAccount = @"
SELECT s.SalesforceAccountId FROM supportaccount s 
  WHERE s.SupportAccountId = @p_SupportAccountId 
  AND s.SalesforceAccountId IS NOT NULL 
;";
            const string querySalesForce = @"
SELECT DISTINCT i.easyonboardingitemid ID, a.SalesforceAccountId, l.SupportAccountId, a.salesordernumber SalesOrderNumber, a.MasterOrderNumber, 
  CASE 
    WHEN l.IsActive = 0 THEN 'Inactive' 
    WHEN l.SupportAccountId IS NOT NULL THEN 'Active'
    ELSE 'Available'
  END 'Status', 
  o.license_hash ActivationLinkHash, 
  CASE 
    WHEN p.PartNumber LIKE '%-SVC%' AND p.ProductFamily = 'Panorama' THEN CONCAT('Panorama with ', i.productdisplayname, ' Support') 
    ELSE i.productdisplayname 
  END Product,
  CASE
    WHEN o.product = 'EPDLP' THEN 'CASBX'
    WHEN o.product LIKE '%SLS%' THEN 'SLS'
    ELSE o.product
  END 'Group',
  CASE 
    WHEN o.product LIKE '%SLS%' THEN 'SLS'
    WHEN p.PartNumber LIKE '%-SVC%' AND p.ProductFamily = 'Panorama' THEN 'PANORAMA' 
    ELSE i.subgroup 
  END SubGroup,
  l.AuthorizationCode, a.evalid EvalId, l.RegistrationDate, p.PartNumber,
  COALESCE(i.expirationdate, l.ExpirationDate, o.expirationdate) ExpirationDate, 
  l.VMQuantity Quantity, l.VMUseCount UseCount, l.SaasQuantity, 
  IFNULL(l.LastUpdate, l.CreatedOn) CreatedOn, u.DisplayName, l.BundleLicenseId, ps.Name  
  FROM assetactivation a
  JOIN supportaccount s ON a.salesforceaccountid = s.SalesforceAccountId 
  JOIN easyonboarding o ON a.assetactivationid = o.assetactivationid
  JOIN easyonboardingitem i ON o.easyonboardingid = i.easyonboardingid
  JOIN license l ON i.licenseid = l.LicenseId
  JOIN partmaster p ON l.PartMasterId = p.PartMasterId
  JOIN partsubtype ps ON p.PartSubtypeId = ps.PartSubtypeId
  LEFT JOIN useraccount u ON IFNULL(l.LastUpdateByUserAccountId, l.CreatedByUserAccountId) = u.UserAccountId
  WHERE a.salesforceaccountid = @p_SalesforceAccountId
  AND (l.SupportAccountId IS NULL -- Available
  OR l.SupportAccountId = @p_SupportAccountId) -- Activated
  AND (a.salesordernumber LIKE @p_SearchValue
  OR a.MasterOrderNumber LIKE @p_SearchValue
  OR l.AuthorizationCode LIKE @p_SearchValue
  OR DATE_FORMAT(IFNULL(l.LastUpdate, l.CreatedOn), '%m/%d/%Y') LIKE @p_SearchValue
  OR u.DisplayName LIKE @p_SearchValue
  OR CASE 
    WHEN l.IsActive = 0 THEN 'Inactive' 
    WHEN l.SupportAccountId IS NOT NULL THEN 'Active'
    ELSE 'Available'
  END LIKE @p_SearchValue)
  ORDER BY o.license_hash, IFNULL(a.salesordernumber, a.MasterOrderNumber), IFNULL(l.LastUpdate, l.CreatedOn) DESC
  LIMIT @p_PaginationOffset, @p_PaginationCount;
SELECT COUNT(1) Total
  FROM
  (
    SELECT DISTINCT i.easyonboardingitemid ID, a.SalesforceAccountId, l.SupportAccountId, a.salesordernumber SalesOrderNumber, a.MasterOrderNumber, 
      CASE 
        WHEN l.IsActive = 0 THEN 'Inactive' 
        WHEN l.SupportAccountId IS NOT NULL THEN 'Active'
        ELSE 'Available'
      END 'Status', 
      o.license_hash ActivationLinkHash, 
      i.productdisplayname Product, o.product 'Group', i.subgroup SubGroup,
      l.AuthorizationCode, a.evalid EvalId, l.RegistrationDate, p.PartNumber,
      COALESCE(i.expirationdate, l.ExpirationDate, o.expirationdate) ExpirationDate, 
      l.VMQuantity Quantity, l.VMUseCount UseCount, l.SaasQuantity, 
      IFNULL(l.LastUpdate, l.CreatedOn) CreatedOn, u.DisplayName
      FROM assetactivation a
      JOIN supportaccount s ON a.salesforceaccountid = s.SalesforceAccountId 
      JOIN easyonboarding o ON a.assetactivationid = o.assetactivationid
      JOIN easyonboardingitem i ON o.easyonboardingid = i.easyonboardingid
      JOIN license l ON i.licenseid = l.LicenseId
      JOIN partmaster p ON l.PartMasterId = p.PartMasterId
      LEFT JOIN useraccount u ON IFNULL(l.LastUpdateByUserAccountId, l.CreatedByUserAccountId) = u.UserAccountId
      WHERE a.salesforceaccountid = @p_SalesforceAccountId
      AND (l.SupportAccountId IS NULL -- Available
      OR l.SupportAccountId = @p_SupportAccountId) -- Activated
      AND (a.salesordernumber LIKE @p_SearchValue
      OR a.MasterOrderNumber LIKE @p_SearchValue
      OR l.AuthorizationCode LIKE @p_SearchValue
      OR DATE_FORMAT(IFNULL(l.LastUpdate, l.CreatedOn), '%m/%d/%Y') LIKE @p_SearchValue
      OR u.DisplayName LIKE @p_SearchValue
      OR CASE 
        WHEN l.IsActive = 0 THEN 'Inactive' 
        WHEN l.SupportAccountId IS NOT NULL THEN 'Active'
        ELSE 'Available'
      END LIKE @p_SearchValue)
  ) a
;";
            Dictionary<string, string> urlMapping = PanSupportWebConfigKeys;
            string salesforceAccountId = string.Empty;
            var result = new List<ActivationLinksLookupItem>();
            using (var connection = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                connection.Open();
                using (var command = new MySqlCommand(querySupportAccount, connection))
                {
                    command.Parameters.AddWithValue("@p_SupportAccountId", supportAccountId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            salesforceAccountId = reader["SalesforceAccountId"] == DBNull.Value ? string.Empty : Convert.ToString(reader["SalesforceAccountId"]);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(salesforceAccountId))
                {
                    return result;
                }

                using (var command = new MySqlCommand(querySalesForce, connection))
                {
                    command.Parameters.AddWithValue("@p_SupportAccountId", supportAccountId);
                    command.Parameters.AddWithValue("@p_SalesforceAccountId", salesforceAccountId);
                    command.Parameters.AddWithValue("@p_SearchValue", search == null ? "%%" : string.Format("%{0}%", search.Trim()));
                    command.Parameters.AddWithValue("@p_PaginationOffset", parameters.PaginationOffset);
                    command.Parameters.AddWithValue("@p_PaginationCount", parameters.PaginationCount);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productGroup = reader["Group"] == DBNull.Value ? "" : Convert.ToString(reader["Group"]) + "ActivationLink";
                            result.Add(new ActivationLinksLookupItem
                            {
                                Id = reader["Id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Id"]),
                                ActivationLinkHash = reader["ActivationLinkHash"] != DBNull.Value
                                    && !string.IsNullOrWhiteSpace(productGroup)
                                    && urlMapping.ContainsKey(productGroup)
                                    ? urlMapping[productGroup] + Convert.ToString(reader["ActivationLinkHash"])
                                    : string.Empty,
                                AuthorizationCode = reader["AuthorizationCode"] == DBNull.Value ? "" : Convert.ToString(reader["AuthorizationCode"]),
                                CreatedBy = reader["DisplayName"] == DBNull.Value ? "" : Convert.ToString(reader["DisplayName"]),
                                CreatedOn = reader["CreatedOn"] == DBNull.Value ? "" : Convert.ToDateTime(reader["CreatedOn"]).ToString("MM/dd/yyyy hh:mm:ss tt"),
                                MasterOrderNumber = reader["MasterOrderNumber"] == DBNull.Value ? "" : Convert.ToString(reader["MasterOrderNumber"]),
                                SalesforceAccountId = reader["SalesforceAccountId"] == DBNull.Value ? "" : Convert.ToString(reader["SalesforceAccountId"]),
                                SalesOrderNumber = reader["SalesOrderNumber"] == DBNull.Value ? "" : Convert.ToString(reader["SalesOrderNumber"]),
                                Status = reader["Status"] == DBNull.Value ? "" : Convert.ToString(reader["Status"]),
                                SupportAccountId = reader["SupportAccountId"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["SupportAccountId"]),
                                Details = new ActivationLinksLookupItemDetails
                                {
                                    EvalId = reader["EvalId"] == DBNull.Value ? "" : Convert.ToString(reader["EvalId"]),
                                    ExpirationDate = reader["ExpirationDate"] == DBNull.Value ? "" : Convert.ToString(reader["ExpirationDate"]),
                                    Group = reader["Group"] == DBNull.Value ? "" : Convert.ToString(reader["Group"]),
                                    PartNumber = reader["PartNumber"] == DBNull.Value ? "" : Convert.ToString(reader["PartNumber"]),
                                    Product = reader["Product"] == DBNull.Value ? "" : Convert.ToString(reader["Product"]),
                                    Quantity = reader["Quantity"] == DBNull.Value ? "" : Convert.ToString(reader["Quantity"]),
                                    RegistrationDate = reader["RegistrationDate"] == DBNull.Value ? "" : Convert.ToString(reader["RegistrationDate"]),
                                    SaasQuantity = reader["SaasQuantity"] == DBNull.Value ? "" : Convert.ToString(reader["SaasQuantity"]),
                                    SubGroup = reader["SubGroup"] == DBNull.Value ? "" : Convert.ToString(reader["SubGroup"]),
                                    UseCount = reader["UseCount"] == DBNull.Value ? "" : Convert.ToString(reader["UseCount"]),
                                    BundleLicenseId = reader["BundleLicenseId"] == DBNull.Value ? "" : Convert.ToString(reader["BundleLicenseId"])
                                }
                            });
                        }
                        reader.NextResult();
                        if (reader.Read())
                        {
                            parameters.TotalRecordCount = Convert.ToInt64(reader["Total"]);
                        }
                    }
                }
            }

            return result;
        }

        public Dictionary<string, string> GetPanSupportWebConfigKeys()
        {
            return PanSupportWebConfigKeys;
        }

    }
}