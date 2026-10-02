using System.Collections.Generic;
using System.Linq;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using MySql.Data.MySqlClient;
using System.Data;
using System;
using System.Configuration;
using Csp.Api.HelperClasses;
using FluentNHibernate.Conventions;
using Csp.Api.Model.UpdatesDownload;

namespace Csp.Infrastructure.Data.Persistence {
    public class UpdatesRepository : IUpdatesRepository
    {
        private readonly IConnectionStrings _connectionStrings;

        public UpdatesRepository(IConnectionStrings connectionStrings) 
        {
            _connectionStrings = connectionStrings;
        }

        public bool ShowTrapsSoftwareUpdates(long supportAccountId)
        {
            bool showTraps = false;
            bool hasEsm = false, hasValidSupport = false, hasServerOrWorkstationOrVdi = false, hasValidWarranty = false;
            bool hasTrapsMobile = false, isMalbork = false;
            using (var db = new cspEntities())
            {

                var trapsLicenses = (from l in db.licenses
                                join pm in db.partmasters on l.PartMasterId equals pm.PartMasterId
                                join pst in db.partsubtypes on pm.PartSubtypeId equals pst.PartSubtypeId
																join epst in db.endpointlicensetypes on pst.PanDbIdentification equals epst.licensetype
                                where
                                    l.SupportAccountId == supportAccountId &&
                                    pm.ModelCompatibility == EndpointConstants.ModelCompat
                                select new
                                    {
                                        l.ExpirationDate,
                                        pst.PanDbIdentification,
																				epst.isesm,
																				epst.isserver,
																				epst.isworkstation,
																				epst.isvdi,
																				epst.issupport,
																				epst.iswarranty
                                    }).ToList();

                #region Check if Traps software can be shown

                foreach (var lic in trapsLicenses)
                {
                    if(EpLogic.IsMalbork(lic.PanDbIdentification))
                    {
                        isMalbork = true;
                    }
										if (lic.isesm.HasValue && lic.isesm.Value)
                    {
                        hasEsm = true;
                    }

                    if (lic.PanDbIdentification == "TRAPSMBETA")
                    {
                        hasTrapsMobile = true;
                    }

										if (lic.isworkstation.HasValue && lic.isworkstation.Value &&
                        lic.ExpirationDate > System.DateTime.Now)
                    {
                        hasServerOrWorkstationOrVdi = true;
                    }

										if (lic.isserver.HasValue && lic.isserver.Value &&
                        lic.ExpirationDate > System.DateTime.Now)
                    {
                        hasServerOrWorkstationOrVdi = true;
                    }

										if (lic.isvdi.HasValue && lic.isvdi.Value &&
													lic.ExpirationDate > System.DateTime.Now) {
											hasServerOrWorkstationOrVdi = true;
										}

										if (((lic.issupport.HasValue && lic.issupport.Value) || (lic.PanDbIdentification.Contains("BKLN"))) &&
                        lic.ExpirationDate > System.DateTime.Now)
                    {
                        hasValidSupport = true;
                    }
										if (lic.iswarranty.HasValue && lic.iswarranty.Value &&
													lic.ExpirationDate > System.DateTime.Now) {
											hasValidWarranty = true;
										}
										

                }

                if ((hasEsm && hasServerOrWorkstationOrVdi && (hasValidWarranty || hasValidSupport)) || hasTrapsMobile || isMalbork )
                {
                    showTraps = true;
                }
                #endregion
                return showTraps;
            }
        }

        public List<ProductModelListItem> GetAllProductModels()
        {
            var productModelsList = new List<ProductModelListItem>();
            var sqlConn = new MySqlConnection(Secrets.CspDatabase);
            var sqlCmd = new MySqlCommand();

            try
            {
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "GetAllProductModels";
                sqlCmd.CommandTimeout = 120;

                sqlCmd.Connection = sqlConn;
                sqlConn.Open();


                var dtProductModelsList = new DataTable();
                var sqlDataAdapter = new MySqlDataAdapter(sqlCmd);
                sqlDataAdapter.Fill(dtProductModelsList);

                if (dtProductModelsList.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtProductModelsList.Rows)
                    {
                        long modelId = 0;
                        int displayOrder = 0;

                        long.TryParse(dr["ModelId"].ToString(), out modelId);
                        int.TryParse(dr["DisplayOrder"].ToString(), out displayOrder);

                        var item = new ProductModelListItem
                        {
                            ModelId = modelId,
                            Name = Convert.ToString(dr["ModelName"]),
                            ProductLine = Convert.ToString(dr["ProductLine"]),
                            Platform = Convert.ToString(dr["Platform"]),
                            MinimumRequiredVersion = Convert.ToString(dr["MinimumRequiredVersion"]),
                            MaximumSupportedVersion = Convert.ToString(dr["MaximumSupportedVersion"]),
                            DisplayName = Convert.ToString(dr["DisplayName"]),
                            DisplayOrder = displayOrder
                        };

                        productModelsList.Add(item);
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(string.Format("GetAllProductModels : --> {0}", ex.Message));
            }
            finally
            {
                sqlCmd.Dispose();
                sqlConn.Close();
                sqlConn.Dispose();
            }

            return productModelsList.ToList();
        }

        public List<SoftwareUpdateFilters> GetFiltersForSoftwareUpdates(long userAccountId, long supportAccountId, out string groupIds)
        {
            groupIds = string.Empty;
            var filterOptions = new List<SoftwareUpdateFilters>();
            try
            {
                var sqlConn = new MySqlConnection(Secrets.CspDatabase);
                var sqlCmd = new MySqlCommand();

                try
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.CommandTimeout = 120;
                    sqlCmd.CommandText = supportAccountId == 245 
                        ? "GetGroupIdsForSoftwareUpdates_245" 
                        : "GetGroupIdsForSoftwareUpdates";
                    sqlCmd.Parameters.Add("p_userAccountId", MySqlDbType.Int64).Value = userAccountId;
                    sqlCmd.Parameters.Add("p_supportAccountId", MySqlDbType.Int64).Value = supportAccountId;
                  
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    using (var reader = sqlCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var filterOption = new SoftwareUpdateFilters()
                            {
                                DisplayName = Convert.ToString(reader["displayName"]),
                                Product = Convert.ToString(reader["product"]),
                                DisplayOrder = Convert.ToString(reader["displayOrder"]),
                                Platform = Convert.ToString(reader["platform"]),
                                HasMultipleReleaseTypes = Convert.ToBoolean(reader["hasMultipleReleaseTypes"])
                            };


                            filterOptions.Add(filterOption);
                        }

                        reader.NextResult();

                        while (reader.Read())
                        {
                            groupIds = Convert.ToString(reader["groupIds"]);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception(string.Format("GetSoftwareUpdatesFilters - GetSoftwareUpdatesFilters : --> {0} {1} {2}", userAccountId, supportAccountId, ex.Message));
                }
                finally
                {
                    sqlCmd.Dispose();
                    sqlConn.Close();
                    sqlConn.Dispose();
                }

            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

            return filterOptions;
        }

        public List<SoftwareUpdateFilters> GetFiltersForDynamicUpdates(long userAccountId, long supportAccountId,string contentType ,out string groupIds)
        {
            groupIds = string.Empty;
            var filterOptions = new List<SoftwareUpdateFilters>();

            string tpGroupIds = string.Empty;
            string wfGroupIds = string.Empty;
            string url2GroupIds = string.Empty;
            string url4GroupIds = string.Empty;
            string wf500GroupIds = string.Empty;
            string trapsGroupIds = string.Empty;
            string gpGroupIds = string.Empty;
            string aceGroupIds = string.Empty;

            try
            {
                var sqlConn = new MySqlConnection(Secrets.CspDatabase);
                var sqlCmd = new MySqlCommand();

                try
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.CommandTimeout = 120;
                    if (supportAccountId == 245)
                    {
                        sqlCmd.CommandText = "GetGroupIdsForDynamicUpdates_245";
                    }
                    else
                    {
                        sqlCmd.CommandText = "GetGroupIdsForDynamicUpdates";
                        sqlCmd.Parameters.Add("p_userAccountId", MySqlDbType.Int32).Value = userAccountId;
                        sqlCmd.Parameters.Add("p_supportAccountId", MySqlDbType.Int32).Value = supportAccountId;
                        sqlCmd.Parameters.Add("p_Type", MySqlDbType.String).Value = contentType;
                    }
                    
                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    using (var reader = sqlCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tpGroupIds = Convert.ToString(reader["ThreatProtection"]);
                            wfGroupIds = Convert.ToString(reader["Widfire"]);
                            wf500GroupIds = Convert.ToString(reader["Wildfire500"]);
                            url2GroupIds = Convert.ToString(reader["Url"]);
                            url4GroupIds = Convert.ToString(reader["Url4"]);
                            trapsGroupIds = Convert.ToString(reader["Traps"]);
                            gpGroupIds = Convert.ToString(reader["GlobalProtect"]);
                            aceGroupIds = Convert.ToString(reader["ACE"]);
                        }

                        reader.NextResult();

                        List<SoftwareUpdateFilters> filtersList = new List<SoftwareUpdateFilters>();
                        while (reader.Read())
                        {
                            
                            var filterOption = new SoftwareUpdateFilters()
                            {
                                DisplayName = Convert.ToString(reader["displayName"]),
                                DisplayOrder = Convert.ToString(reader["displayOrder"]),
                                MinVersionRequired = Convert.ToString(reader["MinVersionRequired"]),
                                MaxVersionRequired = Convert.ToString(reader["MaxVersionRequired"]),
                                ContentType = Convert.ToString(reader["Name"]),
                            };

                            filtersList.Add(filterOption);
                        }

                        if (filtersList.Any())
                        {
                            List<SoftwareUpdateFilters> eligiblefilters = new List<SoftwareUpdateFilters>();

                            #region Eligible Filters
                            if (!string.IsNullOrEmpty(tpGroupIds))
                            {
                                eligiblefilters.AddRange( filtersList.Where(x => x.ContentType == ContentTypeEnum.CONTENTS.ToString()));

                                //if content type is selected then filter by content group ids
                                if (contentType == ContentTypeEnum.CONTENTS.ToString() || contentType == ContentTypeEnum.CONTENT.ToString())
                                    groupIds = tpGroupIds;
                            }

                            //Always add ANTIVIRUS & APPS
                            eligiblefilters.AddRange(filtersList.Where(x => x.ContentType == ContentTypeEnum.VIRUS.ToString()));
                            if (contentType == ContentTypeEnum.VIRUS.ToString())
                            {
                                groupIds = "1";

                                if (supportAccountId == ApiConstants.PANW_SUPPORT_ACCOUNT_ID)
                                    groupIds = string.Concat(groupIds, ",3");
                            }

                            if (!string.IsNullOrEmpty(wfGroupIds))
                            {
                                eligiblefilters.AddRange(filtersList.Where(x => x.ContentType == ContentTypeEnum.WILDFIRE.ToString()));

                                if (contentType == ContentTypeEnum.WILDFIRE.ToString())
                                    groupIds = wfGroupIds;
                            }

                            if (!string.IsNullOrEmpty(wf500GroupIds))
                            {
                                eligiblefilters.AddRange(filtersList.Where(x => x.ContentType == ContentTypeEnum.WFMETA.ToString()));

                                if (contentType == ContentTypeEnum.WFMETA.ToString())
                                    groupIds = wf500GroupIds;
                            }
                            
                            if (!string.IsNullOrEmpty(gpGroupIds))
                            {
                                eligiblefilters.AddRange(filtersList.Where(x => x.ContentType == ContentTypeEnum.GPCONTENT.ToString()));

                                if (contentType == ContentTypeEnum.GPCONTENT.ToString())
                                    groupIds = gpGroupIds;
                            }

                            if (!string.IsNullOrEmpty(url4GroupIds))
                            {
                                eligiblefilters.AddRange(filtersList.Where(x => x.ContentType == ContentTypeEnum.BRIGHTCLOUD.ToString()));

                                if (contentType == ContentTypeEnum.BRIGHTCLOUD.ToString())
                                    groupIds = url4GroupIds;
                            }

                            if (!string.IsNullOrEmpty(url2GroupIds))
                            {
                                eligiblefilters.AddRange(filtersList.Where(x => x.ContentType == ContentTypeEnum.PANDB.ToString()));

                                if (contentType == ContentTypeEnum.PANDB.ToString())
                                    groupIds = url2GroupIds;
                            }

                            if (!string.IsNullOrEmpty(trapsGroupIds))
                            {
                                eligiblefilters.AddRange(filtersList.Where(x => x.ContentType == ContentTypeEnum.TRAPS.ToString()));

                                if (contentType == ContentTypeEnum.TRAPS.ToString())
                                    groupIds = trapsGroupIds;
                            }

                            // CSP-15106 Add new Content Type in CSP Updates > Dynamic Update. 
                            // if (!string.IsNullOrEmpty(aceGroupIds))
                            {
                                eligiblefilters.AddRange(filtersList.Where(x => x.ContentType == ContentTypeEnum.ACE.ToString()));
                                //if (contentType == ContentTypeEnum.ACE.ToString())
                                //    groupIds = aceGroupIds;
                            }

                            // CSP-24996 PAN Traffic Redirector - any CSP ID, any user
                            {
                                eligiblefilters.AddRange(filtersList.Where(x => x.ContentType == ContentTypeEnum.TRAFFICREDIRECTOR.ToString()));
                            }

                            eligiblefilters.AddRange(filtersList.Where(x => x.ContentType == ContentTypeEnum.APPS.ToString()));
                            if (contentType == ContentTypeEnum.APPS.ToString())
                            {
                                groupIds = tpGroupIds.IsNotEmpty() ? string.Concat(tpGroupIds, ",1") : "1";
                            }

                            if (supportAccountId == ApiConstants.PANW_SUPPORT_ACCOUNT_ID
                                && (contentType != ContentTypeEnum.ACE.ToString()
                                || contentType != ContentTypeEnum.TRAFFICREDIRECTOR.ToString()))
                                groupIds = string.IsNullOrEmpty(groupIds)? "3" : string.Concat(groupIds, ",3");
                           

                            #endregion

                            return eligiblefilters.OrderBy(x=> x.DisplayOrder).ToList();
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw new Exception(string.Format("GetDynamicUpdatesFilters - GetDynamicUpdatesFilters : --> {0} {1} {2}", userAccountId, supportAccountId, ex.Message));
                }
                finally
                {
                    sqlCmd.Dispose();
                    sqlConn.Close();
                    sqlConn.Dispose();
                }

            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

            return filterOptions;
        }

        public Dictionary<string, string> GetModelsSupportedItems()
        {
            const string sqlQuery = @"
SELECT l.LookupKey 'Key', l.LookupValue 'DisplayValue'
  FROM lookups l 
  WHERE l.LookupType = 'SW_UPDATE_UPLOAD_TYPE_DISPLAY';";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(Secrets.CspDatabase))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                    {
                        using (MySqlDataReader dataReader = command.ExecuteReader())
                        {
                            Dictionary<string, string> result = new Dictionary<string, string>();
                            while (dataReader.Read())
                            {
                                result.Add
                                (
                                    dataReader["Key"] != DBNull.Value ? dataReader["Key"].ToString() : string.Empty,
                                    dataReader["DisplayValue"] != DBNull.Value ? dataReader["DisplayValue"].ToString() : string.Empty
                                );
                            }
                            return result;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                return new Dictionary<string, string> { [string.Empty] = "Undefined" };
            }
        }

        #region Cellular

        public ContentDownloadResponseCellularFilters GetCellularFilters(ContentDownloadRequestBase model)
        {
            const string sqlQuery = @"
SELECT m.Id, m.DisplayName FROM pandb.modelscellular m;
SELECT l.`key` Id, l.val1 DisplayName FROM pandb.lookups l WHERE l.fld = 'SW_UPDATE_CARRIER' ORDER BY l.val1
;";
            List<ContentDownloadContentValue> products = new List<ContentDownloadContentValue>();
            List<ContentDownloadContentValue> carriers = new List<ContentDownloadContentValue>();
            using (MySqlConnection connection = new MySqlConnection(Secrets.PanDatabase))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                {
                    using (MySqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            products.Add(new ContentDownloadContentValue
                            {
                                Value = dataReader["Id"] != DBNull.Value ? dataReader["Id"].ToString() : string.Empty,
                                Label = dataReader["DisplayName"] != DBNull.Value ? dataReader["DisplayName"].ToString() : string.Empty

                            });
                        }
                        dataReader.NextResult();
                        while (dataReader.Read())
                        {
                            carriers.Add(new ContentDownloadContentValue
                            {
                                Value = dataReader["Id"] != DBNull.Value ? dataReader["Id"].ToString() : string.Empty,
                                Label = dataReader["DisplayName"] != DBNull.Value ? dataReader["DisplayName"].ToString() : string.Empty

                            });
                        }
                    }
                }
            }

            return new ContentDownloadResponseCellularFilters
            {
                Products = products,
                Carriers = carriers
            };
        }

        public ContentDownloadRequestCellularDbRecord GetCellularData(ContentDownloadRequestCellularData model)
        {
            const string modelsQuery = @"
SELECT m.Product, m.Platform, m.Model FROM pandb.modelscellular m WHERE m.Id = @p_ProductFilterId;
SELECT l.`key` Carrier FROM pandb.lookups l WHERE l.fld = 'SW_UPDATE_CARRIER' AND l.`key` = @p_CarrierId;
";
            ContentDownloadRequestCellularDbRecord result = new ContentDownloadRequestCellularDbRecord();
            using (MySqlConnection connection = new MySqlConnection(Secrets.PanDatabase))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(modelsQuery, connection))
                {
                    command.Parameters.Add("@p_ProductFilterId", MySqlDbType.String).Value = model.ProductFilterId;
                    command.Parameters.Add("@p_CarrierId", MySqlDbType.String).Value = model.CarrierId;
                    using (MySqlDataReader dataReader = command.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            result.Product = dataReader["Product"] != DBNull.Value ? dataReader["Product"].ToString() : string.Empty;
                            result.Platform = dataReader["Platform"] != DBNull.Value ? dataReader["Platform"].ToString() : string.Empty;
                            result.Model = dataReader["Model"] != DBNull.Value ? dataReader["Model"].ToString() : string.Empty;
                        }
                        dataReader.NextResult();
                        if (dataReader.Read())
                        {
                            result.Carrier = dataReader["Carrier"] != DBNull.Value ? dataReader["Carrier"].ToString() : string.Empty;
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Mandatory upgrades - start with CSP-18237 [BE] Endpoints for new CSP PAN-OS Advisory page

        public List<SoftwareUpdateFilters> GetSoftwareUpgradeModels()
        {
            const string sqlQuery = @"
SELECT m.Id DisplayOrder, m.DisplayName, m.Platform, m.Product 
    FROM SwUpdateMandatoryUpgradeModels m 
    WHERE m.ProjectName = 'PAN-OS Advisory'
;";

            using (MySqlConnection connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                {
                    using (MySqlDataReader dataReader = command.ExecuteReader())
                    {
                        List<SoftwareUpdateFilters> result = new List<SoftwareUpdateFilters>();
                        while (dataReader.Read())
                        {
                            result.Add(new SoftwareUpdateFilters
                            {
                                DisplayName = dataReader["DisplayName"] != DBNull.Value ? dataReader["DisplayName"].ToString() : string.Empty,
                                DisplayOrder = dataReader["DisplayOrder"] != DBNull.Value ? dataReader["DisplayOrder"].ToString() : string.Empty,
                                Platform = dataReader["Platform"] != DBNull.Value ? dataReader["Platform"].ToString() : string.Empty,
                                Product = dataReader["Product"] != DBNull.Value ? dataReader["Product"].ToString() : string.Empty
                            });
                        }
                        return result;
                    }
                }
            }
        }

        public List<ContentDownloadResponseVersionUpgrade> GetSoftwareUpgradeVersions()
        {
            const string sqlQuery = @"
SELECT v.Id, v.Version CurrentVersion, v.NewMinVersion, v.Details, v.Whitelist, v.SeriesId
    FROM SwUpdateMandatoryUpgradeVersion v
    WHERE v.ProjectName = 'PAN-OS Advisory'
    AND v.SeriesId = 1
;";

            using (MySqlConnection connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                {
                    using (MySqlDataReader dataReader = command.ExecuteReader())
                    {
                        List<ContentDownloadResponseVersionUpgrade> result = new List<ContentDownloadResponseVersionUpgrade>();
                        while (dataReader.Read())
                        {
                            result.Add(new ContentDownloadResponseVersionUpgrade
                            {
                                Id = dataReader["Id"] != DBNull.Value ? dataReader["Id"].ToString() : string.Empty,
                                CurrentVersion = dataReader["CurrentVersion"] != DBNull.Value ? dataReader["CurrentVersion"].ToString() : string.Empty,
                                NewMinVersion = dataReader["NewMinVersion"] != DBNull.Value ? dataReader["NewMinVersion"].ToString() : string.Empty,
                                Details = dataReader["Details"] != DBNull.Value ? dataReader["Details"].ToString() : string.Empty,
                                Whitelist = dataReader["Whitelist"] != DBNull.Value ? dataReader["Whitelist"].ToString() : string.Empty,
                                SeriesId = dataReader["SeriesId"] != DBNull.Value ? int.Parse(dataReader["SeriesId"].ToString()) : 0
                            });
                        }
                        return result;
                    }
                }
            }
        }

        public List<string> GetSoftwareUpgradeExpired()
        {
            const string sqlQuery = @"
SELECT DISTINCT ExpiredBuild
    FROM swupdatemandatoryupgradeexpired e
    WHERE e.ProjectName = 'PAN-OS Advisory';
;";

            using (MySqlConnection connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                {
                    using (MySqlDataReader dataReader = command.ExecuteReader())
                    {
                        List<string> result = new List<string>();
                        while (dataReader.Read())
                        {
                            result.Add(dataReader["ExpiredBuild"] != DBNull.Value ? dataReader["ExpiredBuild"].ToString() : string.Empty);
                        }
                        return result;
                    }
                }
            }
        }

        public ContentDownloadResponseVersionUpgrade GetSoftwareUpgradeVersion(SoftwareUpdateRequest request)
        {
            const string sqlQuery = @"
SELECT v2.Id, v2.Version CurrentVersion, v2.NewMinVersion, v2.Details, v2.Whitelist, v2.SeriesId
    FROM SwUpdateMandatoryUpgradeVersion v
    JOIN SwUpdateMandatoryUpgradeVersion v2 ON v.Version = v2.Version
    JOIN SwUpdateMandatoryUpgradeModels m ON v2.SeriesId = m.SeriesId AND m.Platform = @p_Platform AND m.Product = @p_Product
    WHERE v.Id = @p_CurrentVersionId
;";

            using (MySqlConnection connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                {
                    command.Parameters.Add("@p_Platform", MySqlDbType.String).Value = request.Platform;
                    command.Parameters.Add("@p_Product", MySqlDbType.String).Value = request.Product;
                    command.Parameters.Add("@p_CurrentVersionId", MySqlDbType.Int32).Value = request.CurrentVersionId;
                    using (MySqlDataReader dataReader = command.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            return new ContentDownloadResponseVersionUpgrade
                            {
                                Id = dataReader["Id"] != DBNull.Value ? dataReader["Id"].ToString() : string.Empty,
                                CurrentVersion = dataReader["CurrentVersion"] != DBNull.Value ? dataReader["CurrentVersion"].ToString() : string.Empty,
                                NewMinVersion = dataReader["NewMinVersion"] != DBNull.Value ? dataReader["NewMinVersion"].ToString() : string.Empty,
                                Details = dataReader["Details"] != DBNull.Value ? dataReader["Details"].ToString() : string.Empty,
                                Whitelist = dataReader["Whitelist"] != DBNull.Value ? dataReader["Whitelist"].ToString() : string.Empty,
                                SeriesId = dataReader["SeriesId"] != DBNull.Value ? int.Parse(dataReader["SeriesId"].ToString()) : 0
                            };
                        }
                        return null;
                    }
                }
            }
        }

        public List<ContentDownloadResponseTooltipUpgrade> GetSoftwareUpgradeTooltip()
        {
            const string sqlQuery = @"
SELECT v.Id, v.Version CurrentVersion, v.Details, v.SeriesName, v.Specifics
    FROM SwUpdateMandatoryUpgradeTooltip v
    WHERE v.ProjectName = 'PAN-OS Advisory';
";

            using (MySqlConnection connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                {
                    using (MySqlDataReader dataReader = command.ExecuteReader())
                    {
                        List<ContentDownloadResponseTooltipUpgrade> result = new List<ContentDownloadResponseTooltipUpgrade>();
                        while (dataReader.Read())
                        {
                            result.Add(new ContentDownloadResponseTooltipUpgrade
                            {
                                Id = dataReader["Id"] != DBNull.Value ? dataReader["Id"].ToString() : string.Empty,
                                SeriesName = dataReader["SeriesName"] != DBNull.Value ? dataReader["SeriesName"].ToString() : string.Empty,
                                CurrentVersion = dataReader["CurrentVersion"] != DBNull.Value ? dataReader["CurrentVersion"].ToString() : string.Empty,
                                Details = dataReader["Details"] != DBNull.Value ? dataReader["Details"].ToString() : string.Empty,
                                Specifics = dataReader["Specifics"] != DBNull.Value ? dataReader["Specifics"].ToString() : string.Empty
                            });
                        }
                        return result;
                    }
                }
            }
        }

        #endregion
    }
}
