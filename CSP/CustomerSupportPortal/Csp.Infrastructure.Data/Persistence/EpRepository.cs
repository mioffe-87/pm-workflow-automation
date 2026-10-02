﻿using System;
 using System.Collections.Generic;
 using System.Data;
 using System.Linq;
 using Csp.Api.DataAccess.HelperClasses;
 using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model;
﻿using Csp.Api.Security;
﻿using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
 using MySql.Data.MySqlClient;

namespace Csp.Infrastructure.Data.Persistence {

	public class EpRepository : IEpRepository {

        //Performance improvement with Stored Procedure
	    public IEnumerable<AvailableEp> GetAvailableEps(string connectionString, DataParameters parameters, long userAccountId, long supportAccountId)
	    {
	        var result = new List<AvailableEp>();
	        //MySqlConnection connection = null;
	        try
	        {
                using (var connection = new MySqlConnection(connectionString))
                {

                    connection.Open();
                    using (var command = new MySqlCommand("GetAvailableEPs", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    })
                    {
                        command.Parameters.AddWithValue("@p_userAccountId", userAccountId);
                        command.Parameters.AddWithValue("@p_supportAccountId", supportAccountId);
                        command.Parameters.AddWithValue("@p_PaginationOffset", parameters.PaginationOffset);
                        command.Parameters.AddWithValue("@p_PaginationCount", parameters.PaginationCount);
                        command.Parameters.AddWithValue("@p_Searchfield", parameters.SearchField);
                        command.Parameters.AddWithValue("@p_SearchValue", parameters.SearchValue);
                        var outParam = new MySqlParameter();
                        outParam.ParameterName = "p_TotalRecordCount";
                        outParam.MySqlDbType = MySqlDbType.Int64;
                        outParam.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outParam);
                        command.CommandTimeout = 300000;
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var ep = new AvailableEp
                                {
                                    SerialNumber = reader.GetColumnValue<string>("SerialNumber"),
                                    AuthorizationCode = reader.GetColumnValue<string>("AuthorizationCode"),
                                    ELAAuthCode = reader.GetColumnValue<string>("ELAAuthCode"),
                                    Description = reader.GetColumnValue<string>("Description"),
                                    UseCount = reader.GetColumnValue<int?>("UseCount"),
                                    DeviceId = reader.GetColumnValue<long?>("DeviceId"),
                                    RegistrationDate = reader.GetColumnValue<DateTime?>("RegistrationDate"),
                                    ExpirationDate = reader.GetColumnValue<DateTime?>("ExpirationDate"),
                                    PartSubType = reader.GetColumnValue<string>("PartSubType"),
                                    PartSubTypeName = reader.GetColumnValue<string>("PartSubTypeName"),
                                    LicenseFeatureName = reader.GetColumnValue<string>("LicenseFeatureName").ToUpper(),
                                    DeviceName = reader.GetColumnValue<string>("DeviceName") ?? string.Empty,
                                    DeviceTag = reader.GetColumnValue<string>("DeviceTag") ?? string.Empty,
                                    LicenseName = reader.GetColumnValue<string>("LicenseName"),
                                    IsEsm = reader.GetColumnValue<bool?>("IsEsm") ?? false,
                                    IsWorkstation = reader.GetColumnValue<bool?>("IsWorkstation") ?? false,
                                    IsServer = reader.GetColumnValue<bool?>("IsServer") ?? false,
                                    IsSupport = reader.GetColumnValue<bool?>("IsSupport") ?? false,
                                    IsWarranty = reader.GetColumnValue<bool?>("IsWarranty") ?? false,
                                    ASCId = reader.GetColumnValue<long?>("ASCId"),
                                    ASCName = reader.GetColumnValue<string>("ASCName"),
                                    PartNumber = reader.GetColumnValue<string>("PartNumber")
                                    // Quantity = reader.GetColumnValue<string>("Quantity")
                                };

                                result.Add(ep);
                            }
                        }

                        if (command.Parameters["p_TotalRecordCount"].Value != DBNull.Value)
                        {
                            parameters.TotalRecordCount = (long)command.Parameters["p_TotalRecordCount"].Value;
                        }

                    }
                }
            }
	        catch (Exception)
	        {
	        }
            //finally
            //{
            //    if (connection != null && connection.State == ConnectionState.Open)
            //    {
            //        //Close doesn't mean the session is closed because of connection pool. We have created temp tables in this SP, 
            //        //So we need to close the session to make sure temp tables take effect in this session only.
            //        connection.Close();
            //        MySqlConnection.ClearPool(connection);
            //    }
            //}

            return result;
	    }

		// Get devices
				//-> Page and Filter data
		// For devices, get endpont data
		public AvailableEp[] GetAvailableEps(DataParameters parameters, long supportAccountId, List<RoleSecurity> rolesSecurity)
		{
			var deviceIds = GetDeviceIdsForThePage(supportAccountId, parameters, rolesSecurity);
			return GetEndpointsWithLicense(deviceIds);
		}


		private AvailableEp[] GetEndpointsWithLicense(long[] deviceIds)
		{
			var db = SessionDbContext.CspObjectContext;
			var filteredDevices = (from d in db.devices
														 where deviceIds.Contains(d.DeviceId)
														 select d);

			// For the devices retrieved, attach licenses and return
			var epData =
				from d in filteredDevices
				join l in db.licenses on d.DeviceId equals l.DeviceId
				join lpm in db.partmasters on l.PartMasterId equals lpm.PartMasterId
				join lpst in db.partsubtypes on lpm.PartSubtypeId equals lpst.PartSubtypeId
				join eplt in db.endpointlicensetypes on lpst.PanDbIdentification equals eplt.licensetype

				join dc in db.devicecategories on d.DeviceCategoryId equals dc.DeviceCategoryId into tempDc
				from leftDc in tempDc.DefaultIfEmpty()

				join scsa in db.supportaccounts on l.SupportCenterSupportAccountId equals scsa.SupportAccountId into tempSCSA
				from leftSCSA in tempSCSA.DefaultIfEmpty()

				where
					l.IsActive
					&& lpm.ModelCompatibility.Contains(EndpointConstants.ModelCompat)
				orderby
					d.DeviceId,
					eplt.sortindex

				select new AvailableEp {
					SerialNumber = d.SerialNumber,
					AuthorizationCode = l.AuthorizationCode,
					Description = lpm.Description,
					UseCount = l.VMUseCount == 0 ? (int?)null : l.VMUseCount,
					DeviceId = d.DeviceId,
					RegistrationDate = d.RegistrationDate,
					ExpirationDate = l.ExpirationDate,
					PartSubType = lpst.PanDbIdentification,
					PartSubTypeName = lpst.Name,
					LicenseFeatureName = lpst.LicenseFeaturesName.ToUpper(),
					DeviceName = d.DeviceName ?? string.Empty,
					DeviceTag = leftDc == null ? string.Empty : (leftDc.DeviceCategoryName ?? string.Empty),
					LicenseName = lpst.Name,
					IsEsm = eplt.isesm ?? false,
					IsWorkstation = eplt.isworkstation ?? false,
					IsServer = eplt.isserver ?? false,
					IsSupport = eplt.issupport ?? false,
					IsWarranty = eplt.iswarranty ?? false,
					ASCId = leftSCSA == null ? null : l.SupportCenterSupportAccountId,
					ASCName = leftSCSA == null ? string.Empty : leftSCSA.Name,
                    PartNumber = lpm.PartNumber
				};

			return epData.ToArray();
		}

		private long[] GetDeviceIdsForThePage(long supportAccountId, DataParameters parameters, List<RoleSecurity> rolesSecurity)
		{
			bool hasFilter = !string.IsNullOrEmpty(parameters.SearchValue);
			long[] aDeviceIds = hasFilter ? GetFilteredEndpointData(supportAccountId, parameters) : GetAllEndpointData(supportAccountId);
			aDeviceIds = RemoveEndpointsThatShouldNotBeShown(supportAccountId, aDeviceIds, rolesSecurity);
			parameters.TotalRecordCount = aDeviceIds.Length;
			return aDeviceIds.Skip((int)parameters.PaginationOffset).Take((int)parameters.PaginationCount).ToArray();
		}

		/*
		 * if the user is a asc,
		 * -> then use supportcentersupportaccountid and filter by supportaccountid

		 * if the user is not a asc
		 * -> then filter just by supportaccountid

     * if the user is a asc and is also part of the traps support account then show all..
		 * 
		 */
		private long[] RemoveEndpointsThatShouldNotBeShown(long supportAccountId, long[] aDeviceIds, List<RoleSecurity> rolesSecurity)
		{
			bool isPartOfSupportAccount = false;
			var ascSupportAccountIds = rolesSecurity.GetAscSupportAccountIds();
			if (rolesSecurity.GetStandardSupportAccountIds().Contains(supportAccountId) ||
				rolesSecurity.GetAdminSupportAccountIds().Contains(supportAccountId) ||
				rolesSecurity.GetTrapsSupportAccountIds().Contains(supportAccountId)) {
				isPartOfSupportAccount = true;
			}
			if (isPartOfSupportAccount)
			{
				return aDeviceIds;
			}
			if (ascSupportAccountIds.Any())
			{
				return GetAllEndpointData(supportAccountId, ascSupportAccountIds, aDeviceIds);
			}
			return aDeviceIds;
		}

		private long[] GetAllEndpointData(long supportAccountId)
		{
			long[] aDeviceIds;
			using (var db = new cspEntities())
			{
				IEnumerable<long> deviceIds = (from d in db.devices
					join l in db.licenses on d.DeviceId equals l.DeviceId
					join lpm in db.partmasters on l.PartMasterId equals lpm.PartMasterId
					join lpst in db.partsubtypes on lpm.PartSubtypeId equals lpst.PartSubtypeId
					join eplt in db.endpointlicensetypes on lpst.PanDbIdentification equals eplt.licensetype
					where
						l.IsActive
						&& lpm.ModelCompatibility.Contains(EndpointConstants.ModelCompat)
						&& d.SupportAccountId == supportAccountId
					orderby
						d.DeviceId,
						eplt.sortindex
					select d.DeviceId).Distinct();
				aDeviceIds = deviceIds.ToArray();
			}
			return aDeviceIds;
		}

		private long[] GetAllEndpointData(long supportAccountId, long[] ascAccountIds, long[] filteredDeviceIds) {
			long[] aDeviceIds;
			using (var db = new cspEntities()) {
				IEnumerable<long> deviceIds = (from d in db.devices
																			 join l in db.licenses on d.DeviceId equals l.DeviceId
																			 join lpm in db.partmasters on l.PartMasterId equals lpm.PartMasterId
																			 join lpst in db.partsubtypes on lpm.PartSubtypeId equals lpst.PartSubtypeId
																			 join eplt in db.endpointlicensetypes on lpst.PanDbIdentification equals eplt.licensetype
																			 where
																				 l.IsActive
																				 && lpm.ModelCompatibility.Contains(EndpointConstants.ModelCompat)
																				 && d.SupportAccountId == supportAccountId
																				 && ascAccountIds.Contains(l.SupportCenterSupportAccountId.Value)
																				 && filteredDeviceIds.Contains(d.DeviceId)
																			 orderby
																				 d.DeviceId,
																				 eplt.sortindex
																			 select d.DeviceId).Distinct();
				aDeviceIds = deviceIds.ToArray();
			}
			return aDeviceIds;
		}

		private long[] GetFilteredEndpointData(long supportAccountId, DataParameters parameters)
		{
			long[] aDeviceIds;
			using (var db = new cspEntities())
			{
				var filteredDevices = GetAvailableEps(supportAccountId, parameters.SearchField, parameters.SearchValue, db);
				if (filteredDevices != null)
				{
					IEnumerable<long> deviceIds = (from d in filteredDevices
																				 join l in db.licenses on d.DeviceId equals l.DeviceId
																				 join lpm in db.partmasters on l.PartMasterId equals lpm.PartMasterId
																				 join lpst in db.partsubtypes on lpm.PartSubtypeId equals lpst.PartSubtypeId
																				 join eplt in db.endpointlicensetypes on lpst.PanDbIdentification equals eplt.licensetype
																				 where
																					 l.IsActive
																					 && lpm.ModelCompatibility.Contains(EndpointConstants.ModelCompat)
																				 orderby
																					 d.DeviceId,
																					 eplt.sortindex
																				 select d.DeviceId).Distinct();
					aDeviceIds = deviceIds.ToArray();
				}
				else
				{
					aDeviceIds = new long[0];
				}
			}
			return aDeviceIds;
		}

		private IQueryable<device> GetAvailableEps(long supportAccountId, string searchField, string searchValue, cspEntities db)
		{
			var filteredDevices = db.devices.Where(d => d.SupportAccountId == supportAccountId);
			if (!string.IsNullOrEmpty(searchValue)) {
				switch (searchField) {
					case "SerialNumber":
						filteredDevices = filteredDevices.Where(x => x.SerialNumber == searchValue);
						break;
					case "DeviceName":
						filteredDevices = filteredDevices.Where(x => x.DeviceName == searchValue);
						break;
					case "DeviceCategoryId":
						long deviceCategoryId;
						if (long.TryParse(searchValue, out deviceCategoryId)) {
							filteredDevices = filteredDevices.Where(x => x.DeviceCategoryId == deviceCategoryId);
						} else {
							return null;
						}
						break;
					case "AuthCode":
						var deviceId =
							(
								from l in db.licenses
								where l.AuthorizationCode == searchValue
								select l.DeviceId).SingleOrDefault();
						if (deviceId.HasValue) {
							filteredDevices = filteredDevices.Where(d => d.DeviceId == deviceId.Value);
						} else {
							return null;
						}
						break;
					default:
						//Do nothing
						break;
				}
			}
			return filteredDevices;
		}

		public SearchAllDetailGroup[] ReadMultipleDeviceLicenses(long[] trapsDeviceIds) {

			{
				var db = SessionDbContext.CspObjectContext;

				var filteredDevices = db.devices.Where(d => trapsDeviceIds.Contains(d.DeviceId));
                var epData =
					from d in filteredDevices
					join l in db.licenses on d.DeviceId equals l.DeviceId
                    join ln in db.licenses on l.BundleLicenseId equals ln.LicenseId into templi
                    from leftli in templi.DefaultIfEmpty()
					join lpm in db.partmasters on l.PartMasterId equals lpm.PartMasterId
					join lpst in db.partsubtypes on lpm.PartSubtypeId equals lpst.PartSubtypeId

					join dc in db.devicecategories on d.DeviceCategoryId equals dc.DeviceCategoryId into tempDc
					from leftDc in tempDc.DefaultIfEmpty()

					join scsa in db.supportaccounts on l.SupportCenterSupportAccountId equals scsa.SupportAccountId into tempSCSA
					from leftSCSA in tempSCSA.DefaultIfEmpty()

					where
						l.IsActive
						&& lpm.ModelCompatibility.Contains(EndpointConstants.ModelCompat)

					select new AvailableEp {
						SerialNumber = d.SerialNumber,
						AuthorizationCode = l.AuthorizationCode,
                        ELAAuthCode = leftli.AuthorizationCode,
						Description = lpm.Description,
						UseCount = l.VMUseCount == 0 ? (int?)null : l.VMUseCount,
						DeviceId = d.DeviceId,
						RegistrationDate = d.RegistrationDate,
						ExpirationDate = l.ExpirationDate,
						PartSubType = lpst.PanDbIdentification,
						PartSubTypeName = lpst.Name,
						LicenseFeatureName = lpst.LicenseFeaturesName.ToUpper(),
						DeviceName = d.DeviceName ?? string.Empty,
						DeviceTag = leftDc == null ? string.Empty : (leftDc.DeviceCategoryName ?? string.Empty),
						LicenseName = lpst.Name,
						ASCId = leftSCSA == null ? null : l.SupportCenterSupportAccountId,
						ASCName = leftSCSA == null ? string.Empty : leftSCSA.Name
					};

				var result = new List<SearchAllDetailGroup>();

				result = (
									 from ep in epData.ToArray()
									 group ep by ep.DeviceId
										 into gEp
										 select new SearchAllDetailGroup {
											 DeviceId = gEp.Key.Value,
											 Licenses = (from l in gEp
																	 select new SearchAllDetail {
																		 License = l.PartSubTypeName,
																		 AuthorizationCode = l.AuthorizationCode,
                                                                         ELAAuthCode = l.ELAAuthCode,
																		 ExpirationDate = (l.ExpirationDate.HasValue && l.PartSubType != EndpointLicenseType.EpSms) ? l.ExpirationDate.Value.ToShortDateString() : "Perpetual",
																		 Quantity = l.UseCount.HasValue ? l.UseCount.Value.ToString() : string.Empty,
																		 AuthorizedSupportCenter = l.ASCName
																	 }).ToList(),
										 }).ToList();

				return result.ToArray();
			}
		}

	    public string GetLicenseApiKey(long supportAccountId)
	    {
            var db = SessionDbContext.CspObjectContext;
	        var result = db.licensingapikeys.Where(l => l.IsActive && l.SupportAccountId == supportAccountId);
	        if(!result.Any())
                return null;
	        return result.First().ApiKey;
	    }
	}
}
