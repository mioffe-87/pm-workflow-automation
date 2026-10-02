using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Data;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.HelperClasses;

namespace Csp.Infrastructure.Data.Persistence {
	public class SupportAccountDevicesRepository : ISupportAccountDevicesRepository {

        private readonly IConnectionStrings _connectionStrings;
        private readonly IApplicationSettings _applicationSettings;
		private const string VmPartNumber = "PAN-PA-VM-SW";
		private const string TrapsProductFamily = "TRAPS";
		private const string OssPanDbId = "OSS";
		private const string SparePanDbId = "SPARE";
		private const string DevPandDbId = "DEV";
        private const string CloudServiceStub = "PAN-PA-SAAS-SW";
        private const string CloudServiceBetaStub = "PAN-PA-SAAS-SW-BETA";
        private const string MagnifierBetaStub = "PAN-PA-MGFR-AGENT";

        public SupportAccountDevicesRepository(IConnectionStrings connectionStrings, IApplicationSettings applicationSettings) {
            _connectionStrings = connectionStrings;
            _applicationSettings = applicationSettings;
        }

		public List<DeviceListItemModelCompatibility> ReadSupportAccountDevices(long supportAccountId, SupportAccountDeviceType supportAccountDeviceType, DataParameters parameters, List<RoleSecurity> roleSecurities, long userAccountId) {

            var assetList = new List<DeviceListItemModelCompatibility>();
            var sqlConn = new MySqlConnection(Secrets.CspDatabase);
            var sqlCmd = new MySqlCommand();

            try
            {
                var outParam = new MySqlParameter();
                outParam.ParameterName = "p_TotalRecordCount";
                outParam.MySqlDbType = MySqlDbType.Int64;
                outParam.Direction = ParameterDirection.Output;

                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "GetAssetsBySupportAccountId";
                sqlCmd.CommandTimeout = 120;
                sqlCmd.Parameters.Add("p_SupportAccountId", MySqlDbType.Int32).Value = supportAccountId;
                sqlCmd.Parameters.Add("p_DeviceType", MySqlDbType.VarChar, 45).Value = supportAccountDeviceType;
                sqlCmd.Parameters.Add("p_PaginationOffset", MySqlDbType.Int32).Value = parameters.PaginationOffset;
                sqlCmd.Parameters.Add("p_PaginationCount", MySqlDbType.Int16).Value = parameters.PaginationCount;
                sqlCmd.Parameters.Add("p_SortField", MySqlDbType.VarChar, 65).Value = parameters.SortField ?? "DeviceId";
                sqlCmd.Parameters.Add("p_SortDirection", MySqlDbType.VarChar, 20).Value = SortDirectionExtension.ToSqlOrderBy(parameters.SortDirection);
                sqlCmd.Parameters.Add("p_Searchfield", MySqlDbType.VarChar, 255).Value = parameters.SearchField;
                sqlCmd.Parameters.Add("p_SearchValue", MySqlDbType.VarChar, 255).Value = parameters.SearchValue ?? string.Empty;
                sqlCmd.Parameters.Add("p_UserAccountId", MySqlDbType.Int32).Value = userAccountId;
                sqlCmd.Parameters.Add(outParam);

                sqlCmd.Connection = sqlConn;
                sqlConn.Open();

                var dtAssetList = new DataTable();
                var sqlDataAdapter = new MySqlDataAdapter(sqlCmd);
                sqlDataAdapter.Fill(dtAssetList);

                if (dtAssetList.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtAssetList.Rows)
                    {
                        long deviceId = 0;
                        long countryId = 0;
                        long deviceCategoryId = 0;
                        bool isRMAReturn = false;
                        bool isRMAReplacement = false;
                        bool isCsspProvisionedDevice = false;

                        long.TryParse(dr["DeviceId"].ToString(), out deviceId);
                        long.TryParse(dr["CountryId"].ToString(), out countryId);
                        long.TryParse(dr["DeviceCategoryId"].ToString(), out deviceCategoryId);
                        bool.TryParse(dr["IsRMAReturn"].ToString(), out isRMAReturn);
                        bool.TryParse(dr["IsRMAReplacement"].ToString(), out isRMAReplacement);
                        bool.TryParse(dr["IsCsspProvisionedDevice"].ToString(), out isCsspProvisionedDevice);

                        var item = new DeviceListItemModelCompatibility
                        {
                            DeviceId = deviceId,
                            SerialNumber = dr["SerialNumber"].ToString(),
                            DeviceName = dr["DeviceName"].ToString(),
                            Address1 = dr["Address1"] == null || dr["Address1"] is DBNull ? string.Empty : dr["Address1"].ToString(),
                            Address2 = dr["Address2"] == null || dr["Address2"] is DBNull ? string.Empty : dr["Address2"].ToString(),                           
                            City = dr["City"].ToString(),
                            PostalCode = dr["PostalCode"].ToString(),
                            CountryId = countryId,
                            RegionState = dr["Region"].ToString(),
                            Country = dr["Country"].ToString(),
                            DeviceTag = dr["DeviceCategoryName"].ToString(),
                            ModelName = dr["ModelName"].ToString(),
                            ModelCompatibility = dr["ModelCompatibility"].ToString(),
                            ASCName = dr["ASCName"].ToString(),
                            DeviceCategoryId = deviceCategoryId,
                            CommentsEncoded = dr["Comments"].ToString(),
                            IsRMAReturn = isRMAReturn,
                            IsRMAReplacement = isRMAReplacement,
                            IsCsspProvisionedDevice = isCsspProvisionedDevice,
                            PanOsVersion = dr["PanOsVersion"].ToString(),
                            ProductFamily = dr["ProductFamily"].ToString(),
                            HyperVisorVersion = dr["HyperVisorVersion"].ToString()
                        };
                        assetList.Add(item);
                    }
                }

                if (sqlCmd.Parameters["p_TotalRecordCount"].Value != DBNull.Value)
                {
                    parameters.TotalRecordCount = (long)(sqlCmd.Parameters["p_TotalRecordCount"].Value);
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(string.Format("ReadSupportAccountDevices: --> {0} {1}", supportAccountId, ex.Message));
            }
            finally
            {
                sqlCmd.Dispose();
                sqlConn.Close();
                sqlConn.Dispose();
            }

            return assetList;

            // Comments By LK On 01/10/2018
            // Switch to stored proc to optimize Devices and Spares tabs
            /*
			var db = SessionDbContext.CspObjectContext;
			{

				db.Database.Log = s => Debug.WriteLine(s);

				IQueryable<DeviceListItemModelCompatibility> deviceData = null;

				var isTac = roleSecurities.IsTac(supportAccountId);
				var isAdmin = roleSecurities.IsSupportAccountAdmin(supportAccountId);

				if (isTac || isAdmin) {
					deviceData = GetUnsecuredDeviceData(db, parameters, supportAccountId, supportAccountDeviceType);

					var tacAdminResult = FilterSortAndPageDevices(db, deviceData, parameters, supportAccountId);

					return tacAdminResult;
				}

				//Otherwise, build a device list by unioning ASC access, Standard Access, group access

				var ascSupportAccountIds = roleSecurities.GetAscSupportAccountIds();

				if (ascSupportAccountIds.Any()) {

					var ascVisibleDevices = GetAscVisibleDeviceIds(db, supportAccountId, ascSupportAccountIds);

					var ascDeviceData = GetUnsecuredDeviceData(db, parameters, supportAccountId, supportAccountDeviceType);//, ascVisibleDevices);

					ascDeviceData = ascDeviceData.Where(x => ascVisibleDevices.Contains(x.DeviceId));

					deviceData = deviceData == null
												 ? ascDeviceData
												 : deviceData.Union(ascDeviceData);
				}

				var groupMembershipIds = roleSecurities.GetGroupIds();

				if (groupMembershipIds.Any()) {

					var groupVisibleDevices = GetGroupVisibleDeviceIds(db, groupMembershipIds);

					var groupDeviceData = GetUnsecuredDeviceData(db, parameters, supportAccountId, supportAccountDeviceType, groupVisibleDevices);

					deviceData = deviceData == null
												 ? groupDeviceData
												 : deviceData.Union(groupDeviceData);
				}

				var isSupportAccountStandard = roleSecurities.IsSupportAccountStandard(supportAccountId);

				if (isSupportAccountStandard) {

					var standardDeviceData = GetUnsecuredDeviceData(db, parameters, supportAccountId, supportAccountDeviceType);

					var groupedDeviceIds = GetGroupedDeviceIds(db, supportAccountId);

					standardDeviceData = standardDeviceData.Where(x => !groupedDeviceIds.Contains(x.DeviceId));

					deviceData = deviceData == null
												 ? standardDeviceData
												 : deviceData.Union(standardDeviceData);
				}

				//If they don't have any access, then return nothing
				if (deviceData == null) {
					return new List<DeviceListItemModelCompatibility>();
				}

				var result = FilterSortAndPageDevices(db, deviceData, parameters, supportAccountId);

				return result;
			}*/
		}

		private IQueryable<FilteredPm> GetProdPartMaster(cspEntities db, SupportAccountDeviceType supportAccountDeviceType) {


			const long devPartTypeId = 3;
			const long vmPartMasterId = 3786;
			const long ossPartSubTypeId = 39;
			const long sparePartSubTypeId = 92;

			//var devPartTypeId = (from x in db.parttypes where x.PanDbIdentification == DevPandDbId select x.PartTypeId).Single();
			//var vmPartMasterId = (from x in db.partmasters where x.PartNumber == VmPartNumber select x.PartMasterId).Single();
			//var ossPartSubTypeId = (from x in db.partsubtypes where x.PanDbIdentification == OssPanDbId select x.PartSubtypeId).Single();
			//var sparePartSubTypeId = (from x in db.partsubtypes where x.PanDbIdentification == SparePanDbId select x.PartSubtypeId).Single();


			var filteredPst =
				from pst in db.partsubtypes
				select new {
					pst.PartSubtypeId,
					pst.PanDbIdentification
				};

			var filteredPartMaster =
						from pm in db.partmasters

						join pst in filteredPst on pm.PartSubtypeId equals pst.PartSubtypeId into tempPst
						from leftPst in tempPst.DefaultIfEmpty()

						//join pt in filteredPartType on pm.PartTypeId equals pt

						where
							pm.PartMasterId != vmPartMasterId
							&& (pm.ProductFamily == null || pm.ProductFamily != TrapsProductFamily)
							&& pm.PartTypeId == devPartTypeId
                                                    && pm.PartNumber != CloudServiceStub
                             && pm.PartNumber != CloudServiceBetaStub 
                             && pm.PartNumber != MagnifierBetaStub
						select new FilteredPm {
							PartMasterId = pm.PartMasterId,
							PartTypeId = pm.PartTypeId,
							PartNumber = pm.PartNumber,
							ModelCompatibility = pm.ModelCompatibility,
							PartSubTypePanDbIdentification = leftPst.PanDbIdentification,
							PartSubtypeId = pm.PartSubtypeId,
                            ProductFamily = pm.ProductFamily
						};

			switch (supportAccountDeviceType) {
				case SupportAccountDeviceType.Prod:
					filteredPartMaster = filteredPartMaster.Where(x =>
																												x.PartSubtypeId == null ||
																												(x.PartSubtypeId != ossPartSubTypeId &&
																												 x.PartSubtypeId != sparePartSubTypeId));
					break;

				case SupportAccountDeviceType.Spare:
					filteredPartMaster = filteredPartMaster.Where(x =>
																												x.PartSubtypeId == ossPartSubTypeId ||
																												x.PartSubtypeId == sparePartSubTypeId);
					break;

				case SupportAccountDeviceType.Both:
					break;

				default: throw new InvalidEnumArgumentException();
			}

			return filteredPartMaster;
		}

        private IQueryable<DeviceListItemModelCompatibility> GetUnsecuredDeviceData(cspEntities db, DataParameters parameters, long supportAccountId, SupportAccountDeviceType supportAccountDeviceType, IQueryable<long> filterDeviceIds = null)
        {

			var supportAccountDevices = db.devices.Where(d => d.SupportAccountId == supportAccountId);
            var csspProvisionedDevices = new List<long>();
            var filterCsspParentLicsForAsc = db.csspskus.Select(x => x.PartMasterId).ToList();

            if (IsSupportAccountCssp(supportAccountId))
            {
                csspProvisionedDevices = GetCsspProvisionedDevices(supportAccountId).ToList();
                //supportAccountDevices = supportAccountDevices.Where(e => !csspProvisionedDevices.Contains(e.DeviceId));
            }


			if (filterDeviceIds != null) {
				supportAccountDevices =
					from d in supportAccountDevices
					join filter in filterDeviceIds on d.DeviceId equals filter
					select d;
			}

            /* x.AuthorizationCode != null this depends on the support center staying specified in a bundled sku 
             * otherwise support center will not return in the sub query
             * */
			var ascLics =
				from x in db.licenses
				where
				 x.AuthorizationCode != null 
                && x.IsActive
				&& x.SupportCenterSupportAccountId.HasValue
				&& x.DeviceId.HasValue
                
                && !filterCsspParentLicsForAsc.Contains(x.PartMasterId)
				select new AscLic { DeviceId = x.DeviceId.Value, SupportCenterSupportAccountId = x.SupportCenterSupportAccountId.Value };

			var filteredPartMaster = GetProdPartMaster(db, supportAccountDeviceType);

			if (!string.IsNullOrEmpty(parameters.SearchField) && !string.IsNullOrEmpty(parameters.SearchValue)) {
				switch (parameters.SearchField) {

					case "SerialNumber":
						var deviceIds =
							(
								from d in db.devices
								where d.SerialNumber.Contains(parameters.SearchValue)
								select (long)d.DeviceId).ToList<long>();

						if (deviceIds != null) {
							supportAccountDevices = supportAccountDevices.Where(x => deviceIds.Contains(x.DeviceId));
						}
						break;

					case "AuthorizationCodes":
						var authCodeDevice = (
																	 from lic in db.licenses
																	 where
																		 lic.IsActive && lic.AuthorizationCode == parameters.SearchValue && lic.DeviceId.HasValue
																	 select lic.DeviceId).SingleOrDefault();

						if (authCodeDevice.HasValue) {
							supportAccountDevices = supportAccountDevices.Where(x => x.DeviceId == authCodeDevice.Value);
						}
						break;

					case "ExpiresBy":
						DateTime dtExpiresBy;
						if (DateTime.TryParse(parameters.SearchValue, out dtExpiresBy)) {

							var expiringLics =
								from l in db.licenses
								join d in db.devices on l.DeviceId equals d.DeviceId
								join pm in db.partmasters on l.PartMasterId equals pm.PartMasterId
								join pst in db.partsubtypes on pm.PartSubtypeId equals pst.PartSubtypeId
								let isPerpetual = (pst != null && pst.LicenseType == true)
								where
									l.IsActive
									&& d.SupportAccountId.Value == supportAccountId
									&& isPerpetual == false
									&& l.ExpirationDate <= dtExpiresBy
								select l.DeviceId;

							supportAccountDevices = supportAccountDevices.Where(x => expiringLics.Contains(x.DeviceId));
						}
						break;

					//case "ModelName":
					//	deviceData = deviceData.Where(x => x.ModelName.Contains(parameters.SearchValue));
					//	break;

					case "DeviceName":
						supportAccountDevices = supportAccountDevices.Where(x => x.DeviceName.Contains(parameters.SearchValue));
						break;

					//case "ASCName":
					//	deviceData = deviceData.Where(x => x.ASCName.Contains(parameters.SearchValue));
					//	break;

					case "DeviceCategoryId":
						long deviceCategoryId;
						if (long.TryParse(parameters.SearchValue, out deviceCategoryId)) {
							supportAccountDevices = supportAccountDevices.Where(x => x.DeviceCategoryId == deviceCategoryId);
						}
						break;

					case "Comments":
						supportAccountDevices = supportAccountDevices.Where(x => x.Comments.Contains(parameters.SearchValue));
						break;

                    case "City":
                        supportAccountDevices = supportAccountDevices.Where(x => x.City.Contains(parameters.SearchValue));
				        break;

                    case "PostalCode":
                        supportAccountDevices = supportAccountDevices.Where(x => x.PostalCode.Contains(parameters.SearchValue));
				        break;
				}
			}

            if (csspProvisionedDevices.Count > 0)
            {
                var deviceData =
                        from d in supportAccountDevices
                        join pm in filteredPartMaster on d.PartMasterId equals pm.PartMasterId

                        join co in db.countries on d.CountryId equals co.CountryId into tempCo
                        from leftCo in tempCo.DefaultIfEmpty()

                        join lasc in ascLics on d.DeviceId equals lasc.DeviceId into tempLasc
                        from leftLasc in tempLasc.DefaultIfEmpty()

                        join ascSa in db.supportaccounts on leftLasc.SupportCenterSupportAccountId equals ascSa.SupportAccountId into tempAscSa
                        from leftAscSa in tempAscSa.DefaultIfEmpty()

                        join devcat in db.devicecategories on d.DeviceCategoryId equals devcat.DeviceCategoryId into tempDevcat
                        from leftDevcat in tempDevcat.DefaultIfEmpty()

                        join csspDev in db.csspenduserdevices on d.DeviceId equals csspDev.ProvisionedDeviceId into tempCsspDev
                        from leftCsspDev in tempCsspDev.DefaultIfEmpty()

                        join hyperVersion in db.hypervisors on d.HyperVisorVersion equals hyperVersion.hypervisorversion into tempHyperV
                        from leftHyperV in tempHyperV.DefaultIfEmpty()

                        where csspProvisionedDevices.Contains(d.DeviceId) == false

                        select new DeviceListItemModelCompatibility
                        {
                            DeviceId = d.DeviceId,
                            SerialNumber = d.SerialNumber,
                            DeviceName = d.DeviceName ?? string.Empty,


                            City = d.City,
                            PostalCode = d.PostalCode,
                            CountryId = d.CountryId,
                            RegionState = d.Region ?? string.Empty,
                            Country = d.CountryId != null ? leftCo.Name : string.Empty,

                            DeviceTag = leftDevcat != null ? leftDevcat.DeviceCategoryName : string.Empty,
                            ModelName = pm.PartNumber,
                            ModelCompatibility = pm.ModelCompatibility,
                            ASCName = leftAscSa.Name,


                            DeviceCategoryId = d.DeviceCategoryId,
                            CommentsEncoded = d.Comments,
                            IsRMAReturn = d.IsRMAReturn ?? false,
                            IsRMAReplacement = d.IsRMAReplacement ?? false,
                            IsCsspProvisionedDevice = leftCsspDev != null,
                            PanOsVersion = d.PanOsVersion,
                            ProductFamily = pm.ProductFamily,
                            HyperVisorVersion = leftHyperV.displayvalue
                        };

                return deviceData;
            }
            else
            {
                var deviceData =
                        from d in supportAccountDevices
                        join pm in filteredPartMaster on d.PartMasterId equals pm.PartMasterId

                        join co in db.countries on d.CountryId equals co.CountryId into tempCo
                        from leftCo in tempCo.DefaultIfEmpty()

                        join lasc in ascLics on d.DeviceId equals lasc.DeviceId into tempLasc
                        from leftLasc in tempLasc.DefaultIfEmpty()

                        join ascSa in db.supportaccounts on leftLasc.SupportCenterSupportAccountId equals ascSa.SupportAccountId into tempAscSa
                        from leftAscSa in tempAscSa.DefaultIfEmpty()

                        join devcat in db.devicecategories on d.DeviceCategoryId equals devcat.DeviceCategoryId into tempDevcat
                        from leftDevcat in tempDevcat.DefaultIfEmpty()

                        join csspDev in db.csspenduserdevices on d.DeviceId equals csspDev.ProvisionedDeviceId into tempCsspDev
                        from leftCsspDev in tempCsspDev.DefaultIfEmpty()

                        join hyperVersion in db.hypervisors on d.HyperVisorVersion equals hyperVersion.hypervisorversion into tempHyperV
                        from leftHyperV in tempHyperV.DefaultIfEmpty()

                        select new DeviceListItemModelCompatibility
                        {
                            DeviceId = d.DeviceId,
                            SerialNumber = d.SerialNumber,
                            DeviceName = d.DeviceName ?? string.Empty,


                            City = d.City,
                            PostalCode = d.PostalCode,
                            CountryId = d.CountryId,
                            RegionState = d.Region ?? string.Empty,
                            Country = d.CountryId != null ? leftCo.Name : string.Empty,

                            DeviceTag = leftDevcat != null ? leftDevcat.DeviceCategoryName : string.Empty,
                            ModelName = pm.PartNumber,
                            ModelCompatibility = pm.ModelCompatibility,
                            ASCName = leftAscSa.Name,


                            DeviceCategoryId = d.DeviceCategoryId,
                            CommentsEncoded = d.Comments,
                            IsRMAReturn = d.IsRMAReturn ?? false,
                            IsRMAReplacement = d.IsRMAReplacement ?? false,
                            IsCsspProvisionedDevice = leftCsspDev != null,
                            PanOsVersion = d.PanOsVersion,
                            ProductFamily = pm.ProductFamily,
                            HyperVisorVersion = leftHyperV.displayvalue
                        };

                return deviceData;
            }			
		}

		private IQueryable<long> GetAscVisibleDeviceIds(cspEntities db, long supportAccountId, long[] ascSupportAccountIds) {
			//var deviceIds =
			//	from lic in db.licenses
			//	join dev in db.devices on lic.DeviceId equals dev.DeviceId
			//	where
			//	dev.SupportAccountId == supportAccountId
			//	&& lic.SupportCenterSupportAccountId.HasValue
			//	&& ascSupportAccountIds.Contains(lic.SupportCenterSupportAccountId.Value)
			//	select dev.DeviceId;

			var license =
				from lic in db.licenses
				where
					lic.IsActive
					&& lic.SupportCenterSupportAccountId.HasValue
					&& ascSupportAccountIds.Contains(lic.SupportCenterSupportAccountId.Value)
				select lic.DeviceId;

			var device =
				from d in db.devices
				where d.SupportAccountId == supportAccountId
				select d.DeviceId;

			var deviceIds =
				from lic in license
				join dev in device on lic equals dev
				select dev;

			return deviceIds.Distinct();
		}

		private IQueryable<long> GetGroupVisibleDeviceIds(cspEntities db, long[] groupMembershipIds) {

			var deviceIds =
				from gd in db.groupdevices
				where groupMembershipIds.Contains(gd.GroupId)
				select gd.DeviceId;

			return deviceIds;
		}

		private IQueryable<long> GetGroupedDeviceIds(cspEntities db, long supportAccountId) {

			var deviceIds =
				from gd in db.groupdevices
				join sag in db.supportaccountgroups on gd.GroupId equals sag.GroupId
				where sag.SupportAccountId == supportAccountId
				//select new GroupDeviceId { DeviceId = gd.DeviceId };  //TODO maybe comment back in if needed
				select gd.DeviceId;

			return deviceIds;
		}

		private List<DeviceListItemModelCompatibility> FilterSortAndPageDevices(cspEntities db, IQueryable<DeviceListItemModelCompatibility> deviceData, DataParameters parameters, long supportAccountId) {

			if (!string.IsNullOrEmpty(parameters.SearchField) && !string.IsNullOrEmpty(parameters.SearchValue)) {
				switch (parameters.SearchField) {

					//case "SerialNumber":
					//	var deviceId =
					//		(
					//			from d in db.devices
					//			where d.SerialNumber == parameters.SearchValue
					//			select (long?)d.DeviceId).SingleOrDefault();

					//	if (deviceId.HasValue) {
					//		deviceData = deviceData.Where(x => x.DeviceId == deviceId.Value);
					//	}
					//	break;

					//case "AuthorizationCodes":
					//	var authCodeDevice = (
					//												 from lic in db.licenses
					//												 where
					//													 lic.IsActive && lic.AuthorizationCode == parameters.SearchValue && lic.DeviceId.HasValue
					//												 select lic.DeviceId).SingleOrDefault();

					//	if (authCodeDevice.HasValue) {
					//		deviceData = deviceData.Where(x => x.DeviceId == authCodeDevice.Value);
					//	}
					//	break;

					//case "ExpiresBy":
					//	DateTime dtExpiresBy;
					//	if (DateTime.TryParse(parameters.SearchValue, out dtExpiresBy)) {

					//		var expiringLics =
					//			from l in db.licenses
					//			join d in db.devices on l.DeviceId equals d.DeviceId
					//			join pm in db.partmasters on l.PartMasterId equals pm.PartMasterId
					//			join pst in db.partsubtypes on pm.PartSubtypeId equals pst.PartSubtypeId
					//			let isPerpetual = (pst != null && pst.LicenseType == true)
					//			where
					//				l.IsActive
					//				&& d.SupportAccountId.Value == supportAccountId
					//				&& isPerpetual == false
					//				&& l.ExpirationDate <= dtExpiresBy
					//			select l.DeviceId;

					//		deviceData = deviceData.Where(x => expiringLics.Contains(x.DeviceId));
					//	}
					//	break;

					case "ModelName":
						deviceData = deviceData.Where(x => x.ModelName.Contains(parameters.SearchValue));
						break;

					//case "DeviceName":
					//	deviceData = deviceData.Where(x => x.DeviceName.Contains(parameters.SearchValue));
					//	break;

					case "ASCName":
						deviceData = deviceData.Where(x => x.ASCName.Contains(parameters.SearchValue));
						break;

					//case "DeviceTag":
					//	deviceData = deviceData.Where(x => x.DeviceTag == parameters.SearchValue);
					//	break;

					//case "Comments":
					//	deviceData = deviceData.Where(x => x.Comments.Contains(parameters.SearchValue));
					//	break;
				}
			}

			switch (parameters.SortField) {

				case "SerialNumber":
					deviceData = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
																? deviceData.OrderByDescending(u => u.SerialNumber)
																: deviceData.OrderBy(u => u.SerialNumber);
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

				case "ASCName":
					deviceData = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
																? deviceData.OrderByDescending(u => u.ASCName)
																: deviceData.OrderBy(u => u.ASCName);
					break;

				case "DeviceTag":
					deviceData = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
																? deviceData.OrderByDescending(u => u.DeviceTag)
																: deviceData.OrderBy(u => u.DeviceTag);
					break;

				default:
					deviceData = deviceData.OrderBy(x => x.DeviceId);
					break;
			}

			parameters.TotalRecordCount = deviceData.Select(x => x.DeviceId).Count();
			if (parameters.SupportsPaging) {
				deviceData = deviceData.Skip((int)parameters.PaginationOffset).Take((int)parameters.PaginationCount);
			}

			return deviceData.ToList();
		}

	    public bool IsSupportAccountCssp(long supportAccountId)
	    {
            var db = SessionDbContext.CspObjectContext;
	        bool isCssp = db.supportaccounts.Any(x => x.SupportAccountId == supportAccountId && x.IsCssp == true);
	        return isCssp;
	    }


		private class FilteredPm {
			public long PartMasterId { get; set; }
			public long? PartTypeId { get; set; }
			public long? PartSubtypeId { get; set; }
			public string PartNumber { get; set; }
			public string ModelCompatibility { get; set; }
			public string PartSubTypePanDbIdentification { get; set; }
            public string ProductFamily { get; set; }
		}

		private class AscLic {
			public long DeviceId { get; set; }
			public long SupportCenterSupportAccountId { get; set; }
		}

		private class GroupDeviceId {
			public long DeviceId { get; set; }
		}


        public bool HasCsspProvisionedVm(long supportAccountId, string csspDeviceTransferLog)
        {
            var db = SessionDbContext.CspObjectContext;
            return db.csspenduserpanaccounts.Any(x => x.SupportAccountId == supportAccountId);
        }


        public IEnumerable<long> GetCsspProvisionedDevices(long csspSupportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var provisionedDeviceIds = (from x in db.csspenduserdevices
                join y in db.csspenduserinformations on x.CsspEndUserInformationId equals y.CsspEndUserInformationId
                where y.CsspSupportAccountId == csspSupportAccountId
                select x.ProvisionedDeviceId).ToList();
            return provisionedDeviceIds;
        }

        public IEnumerable<long> GetSaasStubDevices(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;
            var saasStubDevices = (from dev in db.devices
                                   join pm in db.partmasters on dev.PartMasterId equals pm.PartMasterId
                                   where pm.PartNumber.Contains(CloudServiceStub)
                                   && dev.SupportAccountId == supportAccountId
                                   select dev.DeviceId).ToList();
            return saasStubDevices;
        }

        public List<SearchAllModel> ReadSupportUserPae(long userAccountId, IEnumerable<long> supportAccountIds, DataParameters parameters)
        {
            const string deviceType = "Cloud Services";
            const string sql = @"
SELECT a.SupportAccountId, a.SupportAccountName, a.DeviceId, a.SerialNumber, a.DeviceName, a.ModelName, 
  a.City, a.PostalCode, a.Region, a.Country,  a.AuthorizationCode, a.PartMasterId, a.ASC
  FROM
  (
    SELECT d.SupportAccountId, sa.Name AS SupportAccountName, d.DeviceId, d.SerialNumber, d.DeviceName, pmd.PartNumber ModelName, 
      d.City, d.PostalCode, d.Region, c.Name Country,  l.AuthorizationCode, pm.PartMasterId, 
      (    
        SELECT s2.Name
          FROM license l2
          JOIN device d2 ON l2.DeviceId = d2.DeviceId
          JOIN partmaster pm2 ON l2.PartMasterId = pm2.PartMasterId 
          LEFT JOIN supportaccount s2 ON l2.SupportCenterSupportAccountId = s2.SupportAccountId
          WHERE pm2.PartTypeId IN (8, 13)
          AND d2.DeviceId = d.DeviceId
          AND l2.IsActive = 1
          LIMIT 1
      ) 'Asc'
      FROM license l
      JOIN device d ON l.DeviceId = d.DeviceId
      JOIN partmaster pm ON l.PartMasterId = pm.PartMasterId 
      JOIN partmaster pmd ON d.PartMasterId = pmd.PartMasterId 
      LEFT JOIN devicecategory dc ON d.DeviceCategoryId = dc.DeviceCategoryId
      LEFT JOIN country c ON d.CountryId = c.CountryId
      LEFT JOIN supportaccount sa ON d.SupportAccountId = sa.SupportAccountId 
      WHERE pm.PartNumber LIKE 'PAN-PRISMA-ACCESS-%'
      AND l.AuthorizationCode REGEXP '^[^P]' -- exclude Panorama linked Authcodes
      AND pm.PartNumber NOT LIKE '%INT-%'
      AND d.IsActive = 1
      AND l.IsActive = 1
      AND l.SupportAccountId IN ({0})
      --    AND l.VMParentLicenseId IS NULL
      AND (l.BundleLicenseId IS NULL OR l.BundleLicenseId = 0)
      ORDER BY l.BundleLicenseId, l.RegistrationDate ASC
  ) a
  GROUP BY a.SupportAccountId, a.DeviceId, a.PartMasterId;
";
            if (!(supportAccountIds != null
                && supportAccountIds.Any()))
            {
                return new List<SearchAllModel>();
            }

            Dictionary<string, long> keys = supportAccountIds.Distinct().ToDictionary(k => "@cspId" + k, v => v);
            List<SearchAllModel> model = new List<SearchAllModel>();
            using (var sqlConn = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                sqlConn.Open();
                using (var sqlCmd = new MySqlCommand(string.Format(sql, string.Join(", ", keys.Keys)), sqlConn))
                {
                    try
                    {
                        sqlCmd.CommandType = CommandType.Text;
                        sqlCmd.CommandTimeout = 120;
                        foreach (KeyValuePair<string, long> key in keys)
                        {
                            sqlCmd.Parameters.AddWithValue(key.Key, key.Value);
                        }

                        using (var dataReader = sqlCmd.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                var deviceId = dataReader["DeviceId"] != DBNull.Value ? Convert.ToInt64(dataReader["DeviceId"]) : 0;
                                if (deviceId == 0)
                                {
                                    continue;
                                }
                                model.Add(new SearchAllModel
                                {
                                    Id = deviceId,
                                    AllAssetType = EAllAssetType.Pae, 
                                    SupportAccountId = dataReader["SupportAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["SupportAccountId"]) : 0,
                                    SupportAccountName = dataReader["SupportAccountName"] != DBNull.Value ? Convert.ToString(dataReader["SupportAccountName"]) : string.Empty,
                                    AssetType = deviceType,
                                    AuthorizedSupportCenter = dataReader["Asc"] != DBNull.Value ? Convert.ToString(dataReader["Asc"]) : string.Empty,
                                    DeviceCode = dataReader["SerialNumber"] != DBNull.Value ? Convert.ToString(dataReader["SerialNumber"]) : string.Empty,
                                    DeviceName = dataReader["DeviceName"] != DBNull.Value ? Convert.ToString(dataReader["DeviceName"]) : string.Empty,
                                    ModelName = dataReader["ModelName"] != DBNull.Value ? Convert.ToString(dataReader["ModelName"]) : string.Empty,
                                    City = dataReader["City"] != DBNull.Value ? Convert.ToString(dataReader["City"]) : string.Empty,
                                    PostalCode = dataReader["PostalCode"] != DBNull.Value ? Convert.ToString(dataReader["PostalCode"]) : string.Empty,
                                    Region = dataReader["Region"] != DBNull.Value ? Convert.ToString(dataReader["Region"]) : string.Empty,
                                    Country = dataReader["Country"] != DBNull.Value ? Convert.ToString(dataReader["Country"]) : string.Empty,
                                    AuthorizationCodes = dataReader["AuthorizationCode"] != DBNull.Value ? Convert.ToString(dataReader["AuthorizationCode"]) : string.Empty
                                });
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        throw new Exception(string.Format("ReadSupportUserDevices: --> {0} {1}", userAccountId, ex.Message));
                    }
                }
            }

            return model;
        }

        public List<SearchAllModel> ReadSupportUserDevices(long userAccountId, long? supportAccountId, string deviceType, DataParameters parameters, bool ignorePagination = false)
        {
            var assetList = new List<SearchAllModel>();

            using (var sqlConn = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                sqlConn.Open();
                using (var sqlCmd = new MySqlCommand("GetAssetsByUserAccountId", sqlConn))
                {
                    try
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.CommandTimeout = 120;

                        var outParam = new MySqlParameter();
                        outParam.ParameterName = "p_TotalRecordCount";
                        outParam.MySqlDbType = MySqlDbType.Int64;
                        outParam.Direction = ParameterDirection.Output;

                        sqlCmd.Parameters.Add("p_DeviceType", MySqlDbType.VarChar, 45).Value = deviceType;
                        if (!ignorePagination)
                        {
                            sqlCmd.Parameters.Add("p_PaginationOffset", MySqlDbType.Int32).Value = parameters.PaginationOffset;
                            sqlCmd.Parameters.Add("p_PaginationCount", MySqlDbType.Int16).Value = parameters.PaginationCount;
                        }
                        else
                        {
                            sqlCmd.Parameters.Add("p_PaginationOffset", MySqlDbType.Int32).Value = 0;
                            sqlCmd.Parameters.Add("p_PaginationCount", MySqlDbType.Int16).Value = int.MaxValue;
                        }
                        sqlCmd.Parameters.Add("p_SortField", MySqlDbType.VarChar, 65).Value = parameters.SortField ?? "DeviceId";
                        sqlCmd.Parameters.Add("p_SortDirection", MySqlDbType.VarChar, 20).Value = SortDirectionExtension.ToSqlOrderBy(parameters.SortDirection);
                        sqlCmd.Parameters.Add("p_Searchfield", MySqlDbType.VarChar, 255).Value = parameters.SearchField;
                        sqlCmd.Parameters.Add("p_SearchValue", MySqlDbType.VarChar, 255).Value = GetSearchValue(parameters.SearchField, parameters.SearchValue);
                        sqlCmd.Parameters.Add("p_UserAccountId", MySqlDbType.Int32).Value = userAccountId;
                        sqlCmd.Parameters.Add("p_SupportAccountId", MySqlDbType.Int32).Value = supportAccountId.HasValue ? supportAccountId.Value : 0;
                        sqlCmd.Parameters.Add(outParam);

                        var dtAssetList = new DataTable();
                        var sqlDataAdapter = new MySqlDataAdapter(sqlCmd);
                        sqlDataAdapter.Fill(dtAssetList);

                        if (dtAssetList.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dtAssetList.Rows)
                            {
                                long deviceId;
                                long supportAccountIdSp;

                                long.TryParse(dr["DeviceId"].ToString(), out deviceId);
                                long.TryParse(dr["SupportAccountId"].ToString(), out supportAccountIdSp);

                                var item = new SearchAllModel()
                                {
                                    SupportAccountId = supportAccountIdSp,
                                    SupportAccountName = dr["SupportAccountName"] != null? dr["SupportAccountName"].ToString() : string.Empty,
                                    AssetType = dr["AssetType"] != null ? dr["AssetType"].ToString() : string.Empty,

                                    Id = deviceId,
                                    DeviceCode =  dr["SerialNumber"] != null ? dr["SerialNumber"].ToString() : string.Empty,
                                    ModelName =  dr["ModelName"] != null ? dr["ModelName"].ToString() : string.Empty,
                                    DeviceName = dr["DeviceName"] != null ? dr["DeviceName"].ToString() : string.Empty,
                                    DeviceTag = dr["DeviceCategoryName"] != null ? dr["DeviceCategoryName"].ToString() : string.Empty,

                                    City = dr["City"] != null ? dr["City"].ToString() : string.Empty,
                                    PostalCode = dr["PostalCode"] != null ? dr["PostalCode"].ToString() : string.Empty,
                                    Region = dr["Region"] != null ? dr["Region"].ToString() : string.Empty,
                                    Country = dr["Country"] != null ? dr["Country"].ToString() : string.Empty,

                                    AuthorizedSupportCenter = dr["AuthorizedSupportCenter"] != null ? dr["AuthorizedSupportCenter"].ToString() : string.Empty,

                                    VmDetails = new VmDetail
                                    {
                                        VmUseCount = null,
                                        VmTotalCount = null,
                                        IsNsx = null,
                                        IsPerpetualNsx = null,
                                    }
                                };
                                assetList.Add(item);
                            }
                        }

                        if (sqlCmd.Parameters["p_TotalRecordCount"].Value != DBNull.Value)
                        {
                            parameters.TotalRecordCount = (long)(sqlCmd.Parameters["p_TotalRecordCount"].Value);
                        }

                        sqlConn.Close();
                    }
                    catch (MySqlException ex)
                    {
                        throw new Exception(string.Format("ReadSupportUserDevices: --> {0} {1}", userAccountId, ex.Message));
                    }
                }


            }

            return assetList;
        }

        public IEnumerable<SearchAllModel> ReadSupportUserDevices_V2(long userAccountId, long? supportAccountIds, string deviceType, DataParameters parameters)
        {
            Dictionary<long, SearchAllModel> assets = new Dictionary<long, SearchAllModel>();
            Dictionary<long, List<SearchAllDetail>> licenses = new Dictionary<long, List<SearchAllDetail>>();
            Dictionary<long, List<SearchAllDetail>> complexLicenses = new Dictionary<long, List<SearchAllDetail>>();
            using (var sqlConn = new MySqlConnection(_connectionStrings.CspDatabase))
            {
                sqlConn.Open();
                using (var sqlCmd = new MySqlCommand("GetAssetsByUserAccountId_V2", sqlConn))
                {
                    try
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.CommandTimeout = 120;
                        MySqlParameter outParam = new MySqlParameter();
                        outParam.ParameterName = "p_TotalRecordCount";
                        outParam.MySqlDbType = MySqlDbType.Int64;
                        outParam.Direction = ParameterDirection.Output;
                        sqlCmd.Parameters.Add("p_DeviceType", MySqlDbType.VarChar, 45).Value = deviceType;
                        sqlCmd.Parameters.Add("p_PaginationOffset", MySqlDbType.Int32).Value = parameters.PaginationOffset;
                        sqlCmd.Parameters.Add("p_PaginationCount", MySqlDbType.Int16).Value = parameters.PaginationCount;
                        sqlCmd.Parameters.Add("p_SortField", MySqlDbType.VarChar, 65).Value = parameters.SortField ?? "DeviceId";
                        sqlCmd.Parameters.Add("p_SortDirection", MySqlDbType.VarChar, 20).Value = SortDirectionExtension.ToSqlOrderBy(parameters.SortDirection);
                        sqlCmd.Parameters.Add("p_Searchfield", MySqlDbType.VarChar, 255).Value = parameters.SearchField;
                        sqlCmd.Parameters.Add("p_SearchValue", MySqlDbType.VarChar, 255).Value = parameters.SearchField == "CspId" && !string.IsNullOrWhiteSpace(parameters.SearchValue)
                            ? null // search selected CDP IDs - ignore filter
                            : GetSearchValue(parameters.SearchField, parameters.SearchValue);
                        sqlCmd.Parameters.Add("p_UserAccountId", MySqlDbType.Int32).Value = userAccountId;
                        sqlCmd.Parameters.Add("p_SupportAccountIds", MySqlDbType.String).Value = parameters.SearchField == "CspId" && !string.IsNullOrWhiteSpace(parameters.SearchValue)
                            ? parameters.SearchValue // search selected CDP IDs
                            : supportAccountIds?.ToString();
                        sqlCmd.Parameters.Add(outParam);
                        using (var dataReader = sqlCmd.ExecuteReader())
                        {
                            // First recordset - devices
                            while (dataReader.Read())
                            {
                                long deviceId = dataReader["DeviceId"] != DBNull.Value ? Convert.ToInt64(dataReader["DeviceId"]) : 0;
                                if (deviceId == 0)
                                {
                                    continue;
                                }
                                assets.Add(deviceId, new SearchAllModel()
                                {
                                    Id = deviceId,
                                    SupportAccountId = dataReader["SupportAccountId"] != DBNull.Value ? Convert.ToInt64(dataReader["SupportAccountId"]) : 0,
                                    SupportAccountName = dataReader["SupportAccountName"] != null ? dataReader["SupportAccountName"].ToString() : string.Empty,
                                    AssetType = dataReader["AssetType"] != null ? dataReader["AssetType"].ToString() : string.Empty,
                                    DeviceCode = dataReader["SerialNumber"] != null ? dataReader["SerialNumber"].ToString() : string.Empty,
                                    ModelName = dataReader["ModelName"] != null ? dataReader["ModelName"].ToString() : string.Empty,
                                    DeviceName = dataReader["DeviceName"] != null ? dataReader["DeviceName"].ToString() : string.Empty,
                                    DeviceTag = dataReader["DeviceCategoryName"] != null ? dataReader["DeviceCategoryName"].ToString() : string.Empty,
                                    City = dataReader["City"] != null ? dataReader["City"].ToString() : string.Empty,
                                    PostalCode = dataReader["PostalCode"] != null ? dataReader["PostalCode"].ToString() : string.Empty,
                                    Region = dataReader["Region"] != null ? dataReader["Region"].ToString() : string.Empty,
                                    Country = dataReader["Country"] != null ? dataReader["Country"].ToString() : string.Empty,
                                    AuthorizedSupportCenter = dataReader["AuthorizedSupportCenter"] != null ? dataReader["AuthorizedSupportCenter"].ToString() : string.Empty,
                                    /*
                                    VmDetails = new VmDetail
                                    {
                                        VmUseCount = ((dataReader["VMUseCount"] != DBNull.Value && Convert.ToInt32(dataReader["VMUseCount"]) == 0) && (dataReader["DeviceId"] == DBNull.Value) && (dataReader["VMQuantity"] != DBNull.Value && (Convert.ToInt32(dataReader["VMQuantity"])) == 0)) ? 1 : (dataReader["VMUseCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["VMUseCount"])),
                                        VmTotalCount = ((dataReader["VMQuantity"] != DBNull.Value && Convert.ToInt32(dataReader["VMQuantity"]) == 0) && (isPanorama == false || isPerpetualPanorama == true)) ? 1 : (dataReader["VMQuantity"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["VMQuantity"])),
                                        IsNsx = isPanorama ? 1 : 0,
                                        IsPerpetualNsx = isPerpetualPanorama ? 1 : 0,
                                    }
                                    */
                                    VmDetails = new VmDetail
                                    {
                                        VmUseCount = null,
                                        VmTotalCount = null,
                                        IsNsx = null,
                                        IsPerpetualNsx = null,
                                    }
                                });
                            }
                            // Second recordset - simple licenses
                            if (dataReader.NextResult())
                            {
                                while (dataReader.Read())
                                {
                                    long deviceId = dataReader["DeviceId"] != DBNull.Value ? Convert.ToInt64(dataReader["DeviceId"]) : 0;
                                    if (deviceId == 0)
                                    {
                                        continue;
                                    }
                                    if (!licenses.Keys.Contains(deviceId))
                                    {
                                        licenses.Add(deviceId, new List<SearchAllDetail>());
                                    }
                                    licenses[deviceId].Add(new SearchAllDetail
                                    {
                                        License = dataReader["License"] != null ? dataReader["License"].ToString() : string.Empty,
                                        AuthorizationCode = dataReader["AuthorizationCode"] != null ? dataReader["AuthorizationCode"].ToString() : string.Empty,
                                        ExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ExpirationDate"]).ToString("d") : string.Empty,
                                        Quantity = dataReader["Quantity"] != null ? dataReader["Quantity"].ToString() : string.Empty,
                                        IsPerpetual = dataReader["IsPerpetual"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsPerpetual"]) : false,
                                        ELAAuthCode = dataReader["ElaAuthCode"] != null ? dataReader["ElaAuthCode"].ToString() : string.Empty,
                                        Model = dataReader["Model"] != null ? dataReader["Model"].ToString() : string.Empty
                                    });
                                }
                            }
                            // Third recordset - complex licenses: PAE, CDL
                            if (dataReader.NextResult())
                            {
                                while (dataReader.Read())
                                {
                                    long deviceId = dataReader["DeviceId"] != DBNull.Value ? Convert.ToInt64(dataReader["DeviceId"]) : 0;
                                    if (deviceId == 0)
                                    {
                                        continue;
                                    }
                                    if (dataReader["IsParent"] != DBNull.Value ? Convert.ToBoolean(dataReader["IsParent"]) : true) // Parent license
                                    {
                                        if (!licenses.Keys.Contains(deviceId))
                                        {
                                            licenses.Add(deviceId, new List<SearchAllDetail>());
                                        }
                                        licenses[deviceId].Add(new SearchAllDetail
                                        {
                                            License = dataReader["License"] != null ? dataReader["License"].ToString() : string.Empty,
                                            AuthorizationCode = dataReader["AuthorizationCode"] != null ? dataReader["AuthorizationCode"].ToString() : string.Empty,
                                            ExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ExpirationDate"]).ToString("d") : string.Empty,
                                            Quantity = dataReader["Quantity"] != null ? dataReader["Quantity"].ToString() : string.Empty,
                                            Model = dataReader["Model"] != null ? dataReader["Model"].ToString() : string.Empty
                                        });
                                    }
                                    else // Children
                                    {
                                        if (!complexLicenses.Keys.Contains(deviceId))
                                        {
                                            complexLicenses.Add(deviceId, new List<SearchAllDetail>());
                                        }
                                        complexLicenses[deviceId].Add(new SearchAllDetail
                                        {
                                            License = dataReader["License"] != null ? dataReader["License"].ToString() : string.Empty,
                                            AuthorizationCode = dataReader["AuthorizationCode"] != null ? dataReader["AuthorizationCode"].ToString() : string.Empty,
                                            ExpirationDate = dataReader["ExpirationDate"] != DBNull.Value ? Convert.ToDateTime(dataReader["ExpirationDate"]).ToString("d") : string.Empty,
                                            Quantity = dataReader["Quantity"] != null ? dataReader["Quantity"].ToString() : string.Empty,
                                            Model = dataReader["Model"] != null ? dataReader["Model"].ToString() : string.Empty
                                        });
                                    }
                                }
                            }
                        }

                        if (sqlCmd.Parameters["p_TotalRecordCount"].Value != DBNull.Value)
                        {
                            parameters.TotalRecordCount = (long)(sqlCmd.Parameters["p_TotalRecordCount"].Value);
                        }
                    }
                    catch (MySqlException ex)
                    {
                        throw new Exception(string.Format("SupportAccountDevicesRepository::ReadSupportUserDevices_V2> Support Account ID: {0}, User Account ID(s): {1}, Error: {2}", supportAccountIds, userAccountId, ex.Message));
                    }
                }
            }

            // Assemble result.
            if (assets.Any())
            {
                assets.ToList().ForEach(d =>
                {
                    if (licenses.Keys.Contains(d.Key)) // adding licenses to the devices
                    {
                        IEnumerable<SearchAllDetail> ls = licenses[d.Key].ToArray() ?? new SearchAllDetail[0];
                        if (d.Value.ModelName?.Contains("SAAS-SW") ?? false)
                        {
                            d.Value.ModelName = string.Join("<br/>", ls.Select(l => l.Model));
                        }
                        d.Value.SearchAllDetails = ls as SearchAllDetail[];
                    }

                    if (complexLicenses.Keys.Contains(d.Key) // adding child licenses to the devices
                        && complexLicenses[d.Key].Any())
                    {
                        d.Value.ChildLicenses = complexLicenses[d.Key].ToArray();
                    }
                });
            }

            return assets.Values;
        }

        private string GetSearchValue(string searchField, string searchValue)
        {
            if (string.IsNullOrWhiteSpace(searchField)) return searchValue;

            if (string.IsNullOrWhiteSpace(searchValue)) return string.Empty;

            var sanitizedSearchValue = string.Empty;

            switch (searchField)
            {
                case "DeviceTag":
                    sanitizedSearchValue = ApiCommon.DecodeString(searchValue.ToLower());
                    break;
                default:
                    sanitizedSearchValue = searchValue;
                    break;
            }

            return sanitizedSearchValue;
        }
    }
}