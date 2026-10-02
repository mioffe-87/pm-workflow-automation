using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Api.Model;
using System.Configuration;
using MySql.Data.MySqlClient;
using Csp.Api.Model.Flex;
using Csp.Api.DataAccess.HelperClasses;
using Newtonsoft.Json;
using Csp.Api.EntityClasses;

namespace Csp.Api.DataAccess
{
    public class FirewallFlexDataAccess : BaseDataAccess, IFirewallFlexDataAccess
    {
        #region Constants

        public const string SupportAccountIdParam = "p_SupportAccountId";
        public const string RequestTypeParam = "p_RequestType";
        public const string EndpointParam = "p_Endpoint";
        public const string RequestParam = "p_Request";
        public const string ResponseParam = "p_Response";
        public const string RequestedByUserParam = "p_RequestedByUserAccountId";

        #endregion

        public void LogAuditHistory(long supportAccountId, FwFlexRequestType requestType, string api, string request, string response, long userAccountId)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand("LogFirewallFlexCalls"))
                {
                    Connection.AddInParameter(command, SupportAccountIdParam, DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, RequestTypeParam, DbType.String, requestType.ToDescription());
                    Connection.AddInParameter(command, EndpointParam, DbType.String, api);
                    Connection.AddInParameter(command, RequestParam, DbType.String, request);
                    Connection.AddInParameter(command, ResponseParam, DbType.String, response);
                    Connection.AddInParameter(command, RequestedByUserParam, DbType.String, userAccountId);

                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public UseCaseSubscription GetUseCaseSubscriptions(long supportAccountId, DeploymentProfileType type, EDeploymentProfileSubType? subType, string panOs, string productId, string genType)
        {
            var useCaseSubscriptionMapptingList = new List<UseCaseSubscription>();
            var usecaseList = new List<UseCase>();
            var subscriptionList = new List<Availablesubscription>();
            var featureList = new List<AvailableFeature>();
            var iotSubscriptionList = new List<IOTSubscription>();
            bool cspIdHasCdlForMigration = false;
            DateTime? grandfatheringEnds = null;

            try
            {
                using (var command = Connection.GetStoredProcCommand("GetUseCaseSubscriptions_V3"))
                {
                    Connection.AddInParameter(command, "@p_supportAccountId", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "@p_deploymentProfileTypeId", DbType.Int64, type);
                    Connection.AddInParameter(command, "@p_deploymentProfileSubTypeId", DbType.Int64, subType);
                    Connection.AddInParameter(command, "@p_PanOs", DbType.String, panOs);
                    Connection.AddInParameter(command, "@p_productId", DbType.String, productId);
                    Connection.AddInParameter(command, "@p_genType", DbType.String, genType);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        int flexUseCaseId = 0;
                        UseCase useCase = null;

                        while (dataReader.Read())
                        {
                            if (flexUseCaseId != dataReader.GetColumnValue<int>("FlexUseCaseId"))
                            {
                                flexUseCaseId = dataReader.GetColumnValue<int>("FlexUseCaseId");

                                if (useCase != null)
                                    usecaseList.Add(useCase);

                                useCase = new UseCase();
                                useCase.DefaultLicIdentifiers = new List<string>();
                                useCase.FlexUseCaseId = flexUseCaseId;
                                useCase.FlexUseCaseName = dataReader.GetColumnValue<string>("FlexUseCaseName");
                            }
                            var licIdentifier = dataReader.GetColumnValue<string>("LicIdentifier");
                            if (licIdentifier != null)
                                useCase.DefaultLicIdentifiers.Add(licIdentifier);
                        }

                        if (useCase != null)
                            usecaseList.Add(useCase);

                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                if (string.Compare(dataReader.GetColumnValue<string>("GroupName"), "IOT", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    var iotSubscription = new IOTSubscription();
                                    iotSubscription.LicIdentifier = dataReader.GetColumnValue<string>("LicIdentifier");
                                    iotSubscription.DisplayName = dataReader.GetColumnValue<string>("DisplayName");
                                    iotSubscriptionList.Add(iotSubscription);
                                }
                                else
                                {
                                    var subscription = new Availablesubscription();
                                    subscription.LicIdentifier = dataReader.GetColumnValue<string>("LicIdentifier");
                                    subscription.DisplayName = dataReader.GetColumnValue<string>("DisplayName");
                                    subscription.IsLegacy = dataReader.GetColumnValue<bool>("IsLegacy");
                                    subscription.HiddenOnUi = dataReader.GetColumnValue<bool>("HiddenOnUi");
                                    subscriptionList.Add(subscription);
                                }
                            }
                        }
                        if (dataReader.NextResult()) // VM has 3rd recordset, which stores Features
                        {
                            while (dataReader.Read())
                            {
                                var feature = new AvailableFeature();
                                feature.FeatureCode = dataReader.GetColumnValue<string>("LicIdentifier");
                                feature.FeatureName = dataReader.GetColumnValue<string>("DisplayName");
                                featureList.Add(feature);
                            }
                        }
                    }
                }
                if (supportAccountId > 0) // Not Culculator case
                {
                    try
                    {
                        string sqlHasCdlQuery = @"
SELECT COUNT(1) > 0 HasCdlForMigration
    FROM pandb.lic_features lf
    JOIN pandb.device_master dm ON dm.lid = lf.devID
    JOIN csp.tenantlookup t ON dm.serialnum = t.SerialNumber
    WHERE t.AppPortalAppsId =
    (
        SELECT appportalappsid
        FROM csp.appportalapps
        WHERE AppInternalName = 'logging service'
    )
    AND lf.active = 1
    AND lf.part_id not in ('PAN-LGS-METRICS','PAN-FLGS-100GB','PAN-LGS-DL','PAN-LGS-APLO-BETA','PAN-XDR-LGS-BETA','PAN-MGFR-LGS-BETA', 'PAN-LGS-1TB-E30','PAN-LGS-1TB-USG-E30','PAN-LGS-1TB-E60','PAN-LGS-1TB-USG-E60','PAN-CLCS-0TB-1YR','PAN-LGS-SE-1YR')
    AND lf.expiration >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
    AND auth_code NOT LIKE 'v%'
    AND t.SupportAccountId = @SupportAccountId
";
                        const string selectGrandfatheringEndDate = @"
SELECT g.GrandFatheringEndDate
    FROM GrandfatheringScmSlsMigration g
    WHERE g.SupportAccountId = @p_supportAccountId";
                        using (var command = Connection.GetSqlStringCommand(sqlHasCdlQuery))
                        {
                            Connection.AddInParameter(command, "@SupportAccountId", DbType.UInt64, supportAccountId);
                            using (var dataReader = Connection.ExecuteReader(command))
                            {
                                if (dataReader.Read())
                                {
                                    cspIdHasCdlForMigration = dataReader.GetColumnValue<bool>("HasCdlForMigration");
                                }
                            }
                        }

                        using (var selectCommand = Connection.GetSqlStringCommand(selectGrandfatheringEndDate))
                        {
                            Connection.AddInParameter(selectCommand, "@p_supportAccountId", DbType.Int64, supportAccountId);
                            using (var dataReader = Connection.ExecuteReader(selectCommand))
                            {
                                if (dataReader.Read())
                                {
                                    grandfatheringEnds = Convert.ToDateTime(dataReader["GrandFatheringEndDate"])
                                        .Date.AddDays(1).AddMilliseconds(-1);
                                }
                            }
                        }
                    }
                    catch (Exception exGrandfathering)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(exGrandfathering);
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            
            //AURL , URL4          
            
            if( !subscriptionList.Any(x => x.LicIdentifier == "AURL"))
            {
                var foundsubscription = subscriptionList.FirstOrDefault(y => y.LicIdentifier == "URL4");
                if(foundsubscription!=null) foundsubscription.IsLegacy = false;
            }
            // ATP TP
            if (!subscriptionList.Any(x => x.LicIdentifier == "ATP"))
            {
                var foundsubscription = subscriptionList.FirstOrDefault(y => y.LicIdentifier == "TP");
                if (foundsubscription != null) foundsubscription.IsLegacy = false;
            }

            //// AWF WF
            if (!subscriptionList.Any(x => x.LicIdentifier == "AWF"))
            {
                var foundsubscription = subscriptionList.FirstOrDefault(y => y.LicIdentifier == "WF");
                if (foundsubscription != null) foundsubscription.IsLegacy = false;
            }
            return new UseCaseSubscription
            {
                UseCases = usecaseList,
                AvailableSubscriptions = subscriptionList,
                AvailableFeatures = featureList,
                IOTSubscription = iotSubscriptionList,
                GrandfatheringEnds = grandfatheringEnds, 
                CspIdHasCdlForMigration = cspIdHasCdlForMigration
            };
        }

        public IEnumerable<SupportAccountLicense> GetSupportAccountsLicenses(long userAccountId)
        {
            var supportAccountLicenseList = new List<SupportAccountLicense>();

            try
            {
                using (var command = Connection.GetStoredProcCommand("GetFlexEligibleAccountsByUserId"))
                {
                    Connection.AddInParameter(command, "@p_UserAccountId", DbType.Int64, userAccountId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var supportAccountLicense = new SupportAccountLicense()
                            {
                                SupportAccountId = dataReader.GetColumnValue<long>("SupportAccountId"),
                                SupportAccountName = dataReader.GetColumnValue<string>("Name"),
                                DLPLicenseCount = dataReader.GetColumnValue<int>("DLPLicenseCount"),
                                IoTLicenseCount = dataReader.GetColumnValue<int>("IoTLicenseCount"),
                                PanoramaDeviceCount = dataReader.GetColumnValue<int>("PanoramaDeviceCount")
                            };
                            supportAccountLicenseList.Add(supportAccountLicense);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return supportAccountLicenseList;
        }

        public EFedRampType GetFedRampType(long supportAccountId)
        {
            string sql = @"
SELECT CASE
        WHEN s.Isfedramp > 0 AND f.Mappingid IS NOT NULL THEN 2 -- HIGH
        WHEN s.Isfedramp > 0 AND f.Mappingid IS NULL THEN 1 -- MOD
        ELSE 0
    END FedFampType
    FROM csp.Supportaccount s
    LEFT JOIN csp.supportaccountcategorymapping f ON s.SupportAccountID = f.SupportAccountID
    WHERE s.Supportaccountid = @SupportAccountId
";
            using (var command = Connection.GetSqlStringCommand(sql))
            {
                Connection.AddInParameter(command, "@SupportAccountId", DbType.UInt64, supportAccountId);
                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        return (EFedRampType)dataReader.GetColumnValue<int>("FedFampType");
                    }
                }
            }
            Exception ex = new Exception("Error of getting FedRAMP Type for Support Account " + supportAccountId);
            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            throw ex;
        }

        public IEnumerable<DeploymentProfile> GetExpiredDeploymentProfiles(long supportAccountId, long tokenId)
        {
            var expiredProfiles = new List<DeploymentProfile>();

            try
            {
                using (var command = Connection.GetStoredProcCommand("GetExpiredDeploymentProfile"))
                {
                    Connection.AddInParameter(command, SupportAccountIdParam, DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "@p_TokenId", DbType.Int64, tokenId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            if (dataReader.GetColumnValue<Int64>("DeploymentProfileId") <= 0) continue;
                            var expiredProfile = new DeploymentProfile()
                            {
                                Id = dataReader.GetColumnValue<long>("DeploymentProfileId"),
                                ProfileName = dataReader.GetColumnValue<string>("ProfileName"),
                                AuthCode = dataReader.GetColumnValue<string>("AuthorizationCode"),
                                Type = dataReader.GetColumnValue<string>("FirewallType"),
                                SubType = dataReader.GetColumnValue<string>("SubType"),
                                SubTypeAltName = dataReader.GetColumnValue<string>("AltName"),
                                FlattenedName = dataReader.GetColumnValue<string>("FlattenedName"),
                                SubTypeDisplayName = dataReader.GetColumnValue<string>("SubTypeDisplayName"),
                                SubTypeDescription = dataReader.GetColumnValue<string>("SubTypeDescription"),
                                TokenId = dataReader.GetColumnValue<long>("TokenId"),
                                SecurityFeatures = dataReader.GetColumnValue<string>("Subscriptions"),
                                FirewallsCreated = dataReader.GetColumnValue<int>("FirewallsCreated"),
                                FirewallsProjected = dataReader.GetColumnValue<int>("FirewallsProjected"),
                                FirewallQuantity = dataReader.GetColumnValue<int>("FirewallsProjected"),
                                ExpirationDate = dataReader.GetColumnValue<DateTime>("ExpirationDate"),
                                Status = dataReader.GetColumnValue<string>("ProfileStatus"),
                                TokensAllocated = dataReader.GetColumnValue<int>("TokensAllocated"),
                                VcpuQuantity = dataReader.GetColumnValue<int>("vCPUQuantity"),
                                VcpuAvailable = dataReader.GetColumnValue<int>("VcpuAvailable"),
                                VcpuConsumed = dataReader.GetColumnValue<int>("VcpuConsumed"),
                                VcpuAllocated = dataReader.GetColumnValue<int>("VcpuAllocated"),
                                Panorama = !string.IsNullOrEmpty(dataReader.GetColumnValue<string>("Panorama")) ? dataReader.GetColumnValue<string>("Panorama").Split(',').ToList() : new List<string>(),
                                PanOS = dataReader.GetColumnValue<string>("PanOS"),
                                Subscriptions = GetFlexUseCaseSubscription(dataReader.GetColumnValue<int>("DeploymentProfileId")).ToArray(),
                                UseCaseId = GetUseCaseIds(dataReader.GetColumnValue<int>("DeploymentProfileId")),
                                AvgTxtRecordsPerDay = dataReader.GetColumnValue<int?>("AvgTxtRecordsPerDay"),
                                MaxThreatSimulations = dataReader.GetColumnValue<int?>("MaxThreatSimulations"),
                                MaxScans = dataReader.GetColumnValue<int?>("MaxScans"), 
                                AttributesValues = !string.IsNullOrWhiteSpace(dataReader.GetColumnValue<string>("Attributes"))
                                    ? JsonConvert.DeserializeObject<DeploymentProfileAttributeValue[]>(dataReader.GetColumnValue<string>("Attributes"))
                                    : new DeploymentProfileAttributeValue[0]
                            };
                            if (dataReader.GetColumnValue<string>("FirewallType") == DeploymentProfileType.CN.ToDescription() && ApiConstants.CNNonlegacyPanOs.Any(s => s.Contains(dataReader.GetColumnValue<string>("PanOS"))))
                            {
                                var tokens = dataReader.GetColumnValue<string>("cnxtokens");
                                if (!string.IsNullOrEmpty(tokens))
                                {
                                    var cnxTokens = JsonConvert.DeserializeObject<Dictionary<string, int>>(tokens);
                                    var availableNodeTokens = cnxTokens["node_tokens"];
                                    var availableCoreTokens = cnxTokens["core_tokens"];
                                    expiredProfile.VcpuAllocated = expiredProfile.VcpuQuantity;
                                    expiredProfile.VcpuConsumed = expiredProfile.VcpuAllocated - availableCoreTokens;
                                }
                            }
                            expiredProfiles.Add(expiredProfile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return expiredProfiles;
        }

        public void CheckandAssignCreditAdminRole(long supportAccountId, long userAccountId)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand("CheckandAssignCreditAdminRole"))
                {
                    Connection.AddInParameter(command, SupportAccountIdParam, DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, RequestedByUserParam, DbType.Int64, userAccountId);
                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public void LogCreditTransfer(CreditTransferRequest request)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand("LogCreditTransfer"))
                {
                    Connection.AddInParameter(command, "p_SourceSupportAccountId", DbType.Int64, request.SourceSupportAccountId);
                    Connection.AddInParameter(command, "p_DestinationSupportAccountId", DbType.Int64, request.DestinationSupportAccountId);
                    Connection.AddInParameter(command, "p_SourceTokenId", DbType.Int64, request.SourceTokenId);
                    Connection.AddInParameter(command, "p_DestinationTokenId", DbType.Int64, request.DestinationTokenId);
                    Connection.AddInParameter(command, "p_Quantity", DbType.Double, request.Quantity);
                    Connection.AddInParameter(command, RequestedByUserParam, DbType.Int64, request.UserAccountId);
                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public DeploymentProfileStatus AddUpdateDeploymentProfile(DeploymentProfile deploymentProfile)
        {
            long profileId = 0;
            bool status = false;
            string authCode = string.Empty;
            DeploymentProfileStatus deploymentProfileStatus = null;
            try
            {
                if (deploymentProfile.Id != null)
                {
                    var logStatus = LogDeploymentProfileHistory(deploymentProfile.Id.Value, deploymentProfile.CreatedBy, ApiConstants.DeploymentProfileBackupMessage);
                    if (!logStatus) return new DeploymentProfileStatus();
                }
                using (var command = Connection.GetStoredProcCommand("AddUpdateDeploymentProfile_V2"))
                {
                    Connection.AddInParameter(command, "@p_DeploymentProfileId", DbType.Int64, deploymentProfile.Id);
                    Connection.AddInParameter(command, "@p_DeploymentProfileTypeId", DbType.Int32,
                        deploymentProfile.Type == DeploymentProfileType.VM.ToDescription()
                        ? DeploymentProfileType.VM
                        : deploymentProfile.Type == DeploymentProfileType.CN.ToDescription()
                            ? DeploymentProfileType.CN
                            : DeploymentProfileType.AI);
                    Connection.AddInParameter(command, "@p_DeploymentProfileSubType", DbType.String, deploymentProfile.SubType);
                    Connection.AddInParameter(command, "@p_ProfileName", DbType.String, string.IsNullOrWhiteSpace(deploymentProfile.ProfileName) ? null :
                                            deploymentProfile.ProfileName);
                    Connection.AddInParameter(command, SupportAccountIdParam, DbType.Int64, deploymentProfile.SupportAccountId);
                    Connection.AddInParameter(command, "@p_TokenId", DbType.Int64, deploymentProfile.TokenId);
                    Connection.AddInParameter(command, "@p_Authcode", DbType.String, deploymentProfile.AuthCode);
                    Connection.AddInParameter(command, "@p_PanOS", DbType.String, deploymentProfile.PanOS);
                    Connection.AddInParameter(command, "@p_FirewallQuantity", DbType.Int32, deploymentProfile.FirewallQuantity);
                    Connection.AddInParameter(command, "@p_VcpuQuantity", DbType.Int32, deploymentProfile.VcpuQuantity);
                    Connection.AddInParameter(command, "@p_TokensAllocated", DbType.Double, deploymentProfile.TokensAllocated);
                    Connection.AddInParameter(command, "@p_FirewallModel", DbType.String, deploymentProfile.VmModel);
                    Connection.AddInParameter(command, "@p_UseCaseId", DbType.String, string.Join(",", deploymentProfile.UseCaseId));
                    Connection.AddInParameter(command, "@p_Panorama", DbType.String, deploymentProfile.Panorama != null ? string.Join(",", deploymentProfile.Panorama.OrderBy(x => x).ToList()) : null);
                    Connection.AddInParameter(command, "@p_Subscriptions", DbType.String, deploymentProfile.Subscriptions != null ? string.Join(",", deploymentProfile.Subscriptions) : null);
                    Connection.AddInParameter(command, "@p_Features", DbType.String, deploymentProfile.Features != null ? string.Join(",", deploymentProfile.Features) : null);
                    Connection.AddInParameter(command, "@p_QuantityNode", DbType.Int32, deploymentProfile.KubernatesNodes);
                    Connection.AddInParameter(command, "@p_ProductId", DbType.String, deploymentProfile.ProductId);
                    Connection.AddInParameter(command, RequestedByUserParam, DbType.Int64, deploymentProfile.CreatedBy);
                    Connection.AddInParameter(command, "@p_genType", DbType.String, deploymentProfile.GenType);
                    Connection.AddInParameter(command, "@p_IsAirgapEligible", DbType.Boolean, deploymentProfile.IsAirgapEligible);
                    Connection.AddInParameter(command, "@p_vSysQuantity", DbType.Int32, deploymentProfile.VSysQuantity);
                    Connection.AddInParameter(command, "@p_Avgtxtrecordsperday", DbType.Int64, deploymentProfile.AvgTxtRecordsPerDay);
                    Connection.AddOutParameter(command, "@p_output_profileId", DbType.Int64, Int32.MaxValue);
                    Connection.AddOutParameter(command, "@p_output_authCode", DbType.String, 50);

                    Connection.ExecuteNonQuery(command);

                    if (command.Parameters["@p_output_profileId"].Value != DBNull.Value)
                    {
                        profileId = Convert.ToInt64(command.Parameters["@p_output_profileId"].Value);
                    }
                    if (command.Parameters["@p_output_authCode"].Value != DBNull.Value)
                    {
                        authCode = command.Parameters["@p_output_authCode"].Value.ToString();
                    }
                    status = profileId > 0 ? true : false;

                    deploymentProfileStatus = new DeploymentProfileStatus
                    {
                        Id = profileId,
                        AuthCode = authCode,
                        Message = profileId > 0 ? ApiConstants.DeploymentProfileSuccessMessage : ApiConstants.DeploymentProfileFailedMessage,
                        Success = profileId > 0
                    };
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return deploymentProfileStatus;
        }

        public bool IsUserHasPermissionToDelete(string authCode, long userAccountId, long supportAccountId)
        {
            const string sqlQuery = @"SELECT 1 FROM csp.license l
                                    JOIN csp.device d ON l.VMStubDeviceId = d.DeviceId
                                    JOIN csp.membership m ON d.SupportAccountId = @SupportAccountId AND m.UserAccountId = @UserAccountId
                                    AND (m.ExpirationDate >= NOW() OR m.ExpirationDate IS NULL)
                                    AND m.ActivationDate <= NOW()
                                    WHERE m.SupportAccountId = @SupportAccountId
                                    AND l.AuthorizationCode =@AuthorizationCode";
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@UserAccountId", DbType.Int64, userAccountId);
                    Connection.AddInParameter(command, "@SupportAccountId", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "@AuthorizationCode", DbType.String, authCode);
                    var objResult = Connection.ExecuteScalar(command);
                    int result = 0;
                    if (objResult != null && int.TryParse(objResult.ToString(), out result) && result != 0)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception(string.Format("User account {0} do not have permission to delete the Authcode {1}", userAccountId, authCode));
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return false;
        }

        public bool IsProfileNameExists(string profileName, long? profileId)
        {
            int count = 0;
            string sqlQuery = string.Format(@"SELECT COUNT(d.DeploymentProfileId) FROM csp.deploymentprofile d 
                                                JOIN csp.deploymentprofilestatus d1 ON d.Status=d1.DeploymentProfileStatusId 
                                                WHERE d.Name = @p_ProfileName
                                                AND d1.Name<>'Deleted'
                                                AND (@p_ProfileId IS NULL OR @p_ProfileId < 1 OR DeploymentProfileId != @p_ProfileId )");
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_ProfileName", DbType.String, profileName);
                    Connection.AddInParameter(command, "@p_ProfileId", DbType.Int64, profileId);
                    count = Convert.ToInt32(Connection.ExecuteScalar(command));
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return count > 0 ? true : false;
        }

        public FlexDevices GetFlexDevices(FlexDeviceRequest flexDeviceRequest, long supportAccountId, int type, long userAccountId)
        {
            FlexDevices flexDevices = null;

            try
            {
                using (var command = Connection.GetStoredProcCommand("GetFlexDevices"))
                {
                    string sortFields = flexDeviceRequest.requestParameters.Sort;
                    string sortDirection = string.Empty;
                    if (!string.IsNullOrEmpty(flexDeviceRequest.requestParameters.Sort))
                    {
                        if (flexDeviceRequest.requestParameters.Sort.LastIndexOf(",") != -1)
                        {
                            sortFields = flexDeviceRequest.requestParameters.Sort.Substring(0, flexDeviceRequest.requestParameters.Sort.LastIndexOf(","));
                            sortDirection = flexDeviceRequest.requestParameters.Sort.Substring(flexDeviceRequest.requestParameters.Sort.LastIndexOf(",") + 1, flexDeviceRequest.requestParameters.Sort.Length - flexDeviceRequest.requestParameters.Sort.LastIndexOf(",") - 1);
                            if (!SortHelper.ValidateSorting(sortFields, sortDirection))
                            {
                                sortFields = string.Empty;
                                sortDirection = string.Empty;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(flexDeviceRequest?.requestParameters?.SearchField) && !SearchHelper.ValidateSearchField(flexDeviceRequest.requestParameters.SearchField))
                    {
                        flexDeviceRequest.requestParameters.SearchField = string.Empty;
                        flexDeviceRequest.requestParameters.SearchValue = string.Empty;
                    }

                    if(!string.IsNullOrEmpty(sortFields) && !string.IsNullOrEmpty(sortDirection) && !SortHelper.ValidateSorting(sortFields, sortDirection))
                    {
                        sortFields = string.Empty;
                        sortDirection = string.Empty;
                    }

                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "@p_Type", DbType.Int32, type);
                    Connection.AddInParameter(command, "@p_PaginationOffset", DbType.Int32, flexDeviceRequest.requestParameters.PaginationOffset);
                    Connection.AddInParameter(command, "@p_PaginationCount", DbType.Int32, flexDeviceRequest.requestParameters.PaginationCount);
                    Connection.AddInParameter(command, "@p_SortField", DbType.String, sortFields);
                    Connection.AddInParameter(command, "@p_SortDirection", DbType.String, sortDirection);
                    Connection.AddInParameter(command, "@p_SearchField", DbType.String, flexDeviceRequest.requestParameters.SearchField);
                    Connection.AddInParameter(command, "@p_SearchValue", DbType.String, flexDeviceRequest.requestParameters.SearchValue);
                    Connection.AddInParameter(command, "@p_UserAccountId", DbType.Int64, userAccountId);
                    Connection.AddOutParameter(command, "@p_TotalRecordCount", DbType.Int64, Int32.MaxValue);
                    command.Parameters[9].Direction = ParameterDirection.Output;

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        var devices = new List<FlexDevice>();
                        while (dataReader.Read())
                        {
                            if (dataReader.GetColumnValue<Int64>("SupportAccountId") <= 0) continue;
                            var device = new FlexDevice()
                            {
                                Id = dataReader.GetColumnValue<Int64>("Id"),
                                SupportAccountId = dataReader.GetColumnValue<Int64>("SupportAccountId"),
                                FirewallType = dataReader.GetColumnValue<string>("Name"),
                                SerialNumber = dataReader.GetColumnValue<string>("SerialNumber"),
                                DeviceName = dataReader.GetColumnValue<string>("DeviceName"),
                                VMModel = dataReader.GetColumnValue<string>("VMModel"),
                                VCpuQuantity = dataReader.GetColumnValue<Int64?>("vCPUQuantity").HasValue ? dataReader.GetColumnValue<Int64>("vCPUQuantity")
                                       : (Int64?)null,
                                Subscriptions = dataReader.GetColumnValue<string>("Subscriptions"),
                                AuthorizationCode = dataReader.GetColumnValue<string>("AuthorizationCode"),
                                ExpirationDate = dataReader.GetColumnValue<DateTime?>("ExpirationDate").HasValue
                                                ? dataReader.GetColumnValue<DateTime?>("ExpirationDate").Value.ToShortDateString()
                                                : string.Empty,
                                AscName = dataReader.GetColumnValue<string>("AscName"),
                                DeviceId = dataReader.GetColumnValue<long>("DeviceId"),
                                SubscriptionModel = dataReader.GetColumnValue<string>("SubscriptionModel"),
                                PanOs = dataReader.GetColumnValue<string>("PanOs"),
                                ProductId = dataReader.GetColumnValue<string>("ProductId"),
                                TokenId = dataReader.GetColumnValue<long>("TokenId"),
                                IsSubOnly = dataReader.GetColumnValue<int>("IsSubOnly"),
                                IsWarmFw = type == 1 ? dataReader.GetColumnValue<bool>("IsWarmFw") : false, 
                                FedRampType = (EFedRampType)dataReader.GetColumnValue<int>("FedRampType"), // same as the DP FedRampType
                                Attributes = DeploymentProfileAttributeValue.ToAttributesArray(dataReader.GetColumnValue<string>("Attributes"))
                            };
                            devices.Add(device);
                        }

                        dataReader.Close();
                        long totalRecordCount = 0;

                        if (command.Parameters["@p_TotalRecordCount"].Value != DBNull.Value)
                        {
                            totalRecordCount = Convert.ToInt64(command.Parameters["@p_TotalRecordCount"].Value);
                        }

                        flexDevices = new FlexDevices
                        {
                            FlexDeviceList = new FlexDevicesList
                            {
                                devices = devices,
                                totalElementsCount = totalRecordCount,
                                totalPages = flexDeviceRequest.requestParameters.PaginationCount > 0 ?
                                                (totalRecordCount / flexDeviceRequest.requestParameters.PaginationCount)
                                             + (totalRecordCount % flexDeviceRequest.requestParameters.PaginationCount == 0 ? 0 : 1)
                                             : totalRecordCount > 0 ? 1 : 0
                            }
                        };

                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return flexDevices;
        }

        public PanoramaDevices GetPanorams(FlexDeviceRequest flexDeviceRequest, long supportAccountId)
        {
            PanoramaDevices panoramaDevices = null;

            try
            {
                using (var command = Connection.GetStoredProcCommand("GetPanoramas"))
                {
                    string sortFields = flexDeviceRequest.requestParameters.Sort;
                    string sortDirection = string.Empty;
                    if (!string.IsNullOrEmpty(flexDeviceRequest.requestParameters.Sort))
                    {
                        if (flexDeviceRequest.requestParameters.Sort.LastIndexOf(",") != -1)
                        {
                            sortFields = flexDeviceRequest.requestParameters.Sort.Substring(0, flexDeviceRequest.requestParameters.Sort.LastIndexOf(","));
                            sortDirection = flexDeviceRequest.requestParameters.Sort.Substring(flexDeviceRequest.requestParameters.Sort.LastIndexOf(",") + 1, flexDeviceRequest.requestParameters.Sort.Length - flexDeviceRequest.requestParameters.Sort.LastIndexOf(",") - 1);
                        }
                    }

                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "@p_PaginationOffset", DbType.Int32, flexDeviceRequest.requestParameters.PaginationOffset);
                    Connection.AddInParameter(command, "@p_PaginationCount", DbType.Int32, flexDeviceRequest.requestParameters.PaginationCount);
                    Connection.AddInParameter(command, "@p_SortField", DbType.String, sortFields);
                    Connection.AddInParameter(command, "@p_SortDirection", DbType.String, sortDirection);
                    Connection.AddInParameter(command, "@p_SearchField", DbType.String, flexDeviceRequest.requestParameters.SearchField);
                    Connection.AddInParameter(command, "@p_SearchValue", DbType.String, flexDeviceRequest.requestParameters.SearchValue);
                    Connection.AddOutParameter(command, "@p_TotalRecordCount", DbType.Int64, Int32.MaxValue);
                    command.Parameters[7].Direction = ParameterDirection.Output;

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        var panoramaDeviceList = new List<PanoramaDevice>();
                        while (dataReader.Read())
                        {
                            var panoromaDevice = new PanoramaDevice
                            {
                                Id = dataReader.GetColumnValue<long>("DeviceId"),
                                AuthorizationCode = dataReader.GetColumnValue<string>("AuthorizationCode"),
                                SerialNumber = dataReader.GetColumnValue<string>("SerialNumber"),
                                CreditPoolId = dataReader.GetColumnValue<long>("CreditPoolId"),
                                DeploymentProfile = dataReader.GetColumnValue<string>("DeploymentProfile"),
                                ExpirationDate = dataReader.GetColumnValue<DateTime?>("ExpirationDate").HasValue
                                                ? dataReader.GetColumnValue<DateTime?>("ExpirationDate").Value.ToShortDateString()
                                                : string.Empty,
                                PanoramaModel = dataReader.GetColumnValue<string>("PanoramaModel"),
                                IsFwFlexPanorama = dataReader.GetColumnValue<bool>("IsFwFlexPanorama"), 
                                FedRampType = (EFedRampType)dataReader.GetColumnValue<int>("FedRampType")
                            };
                            panoramaDeviceList.Add(panoromaDevice);
                        }

                        dataReader.Close();
                        long totalRecordCount = 0;

                        if (command.Parameters["@p_TotalRecordCount"].Value != DBNull.Value)
                        {
                            totalRecordCount = Convert.ToInt64(command.Parameters["@p_TotalRecordCount"].Value);
                        }

                        panoramaDevices = new PanoramaDevices
                        {
                            PanoromaDevices = new PanoromaDeviceList
                            {
                                panoramas = panoramaDeviceList,
                                totalElementsCount = totalRecordCount,
                                totalPages = flexDeviceRequest.requestParameters.PaginationCount > 0 ?
                                                (totalRecordCount / flexDeviceRequest.requestParameters.PaginationCount)
                                             + (totalRecordCount % flexDeviceRequest.requestParameters.PaginationCount == 0 ? 0 : 1)
                                             : totalRecordCount > 0 ? 1 : 0
                            }
                        };

                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return panoramaDevices;
        }

        public void UpdateDeleteDeploymentProfileStatus(long userAccountId, string authCode, string statusMessage)
        {
            var transferHistoryList = new List<TokenTransferHistory>();
            try
            {
                using (var command = Connection.GetStoredProcCommand("DeleteDeploymentProfile"))
                {
                    Connection.AddInParameter(command, "@p_UserAccountId", DbType.Int64, userAccountId);
                    Connection.AddInParameter(command, "@p_StatusMessage", DbType.String, statusMessage);
                    Connection.AddInParameter(command, "@p_AuthorizationCode", DbType.String, authCode);
                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public void UpdateProfileStatusOnDeactivate(long userAccountId, string authCode)
        {
            const string sqlQuery = @"UPDATE csp.deploymentprofile d
                                      JOIN csp.license l
                                      JOIN csp.deploymentprofilestatus dps
                                      ON d.DpLicenseId=l.licenseId AND dps.Name='PendingUpdate'
                                      SET 
                                      d.Status = dps.DeploymentProfileStatusId,
                                      d.UpdatedBy = @p_UserAccountId,
                                      d.UpdatedOn = NOW()
                                      WHERE l.AuthorizationCode = @p_AuthorizationCode";
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_UserAccountId", DbType.Int64, userAccountId);
                    Connection.AddInParameter(command, "@p_AuthorizationCode", DbType.String, authCode);
                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public DeploymentProfile GetDeploymentProfile(long deploymentProfileId, string authCode = "")
        {
            var profile = new DeploymentProfile();
            const string sqlQuery = @"SELECT dp.Name, d.SupportAccountId, d.TokenId, l.AuthorizationCode, d.PanOS, d.FirewallQuantity,
                                             d.vCPUQuantity, d.VMModel, d.Panorama, d.TokensAllocated, d.IsAirgapEligible,
                                             GROUP_CONCAT(COALESCE(f.LicIdentifier,'')) AS FlexSubscriptions, l.ExpirationDate, d.FedRampType 
                                      FROM   csp.deploymentprofile d JOIN csp.deploymentprofiletype dp ON d.DeploymentProfileTypeId = dp.DeploymentProfileTypeId
                                             JOIN csp.license l ON l.LicenseId = d.DpLicenseId 
                                             LEFT JOIN csp.deploymentprofilesubscriptions ds ON ds.DeploymentProfileId = d.DeploymentProfileId
                                             LEFT JOIN csp.flexsubscriptions f ON f.FlexSubscriptionId = ds.FlexSubscriptionId
                                      WHERE  d.DeploymentProfileId = @p_DeploymentProfileId || l.AuthorizationCode =@p_AuthorizationCode";
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_DeploymentProfileId", DbType.Int64, deploymentProfileId);
                    Connection.AddInParameter(command, "@p_AuthorizationCode", DbType.String, authCode);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            profile = new DeploymentProfile()
                            {
                                Type = dataReader.GetColumnValue<string>("Name"),
                                SupportAccountId = dataReader.GetColumnValue<long>("SupportAccountId"),
                                TokenId = dataReader.GetColumnValue<long>("TokenId"),
                                AuthCode = dataReader.GetColumnValue<string>("AuthorizationCode"),
                                PanOS = dataReader.GetColumnValue<string>("PanOS"),
                                FirewallQuantity = dataReader.GetColumnValue<int>("FirewallQuantity"),
                                VcpuQuantity = dataReader.GetColumnValue<int>("vCPUQuantity"),
                                VmModel = dataReader.GetColumnValue<string>("VMModel"),
                                IsAirgapEligible = dataReader.GetColumnValue<bool>("IsAirgapEligible"),
                                Panorama = new List<string> { dataReader.GetColumnValue<string>("Panorama") },
                                TokensAllocated = dataReader.GetColumnValue<int>("TokensAllocated"),
                                Subscriptions = dataReader.GetColumnValue<string>("FlexSubscriptions").Split(new string[] { "," },
                                  StringSplitOptions.None),
                                ExpirationDate = dataReader.GetColumnValue<DateTime>("ExpirationDate"), 
                                FedRampType = (EFedRampType)dataReader.GetColumnValue<int>("FedRampType")
                            };
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return profile;
        }

        public TokenAllocationDetail GetAllocationDetail(long tokenId, long? deploymentProfileId)
        {
            var tokenAvailable = new TokenAllocationDetail();

            const string sqlQuery = @"SELECT COALESCE(SUM(d.TokensAllocated),0) AS NetBalance FROM csp.deploymentprofile d 
                                      WHERE d.TokenId = @p_TokenId AND d.Status IN (1,2) AND IsActive; SELECT COALESCE(d.TokensAllocated,0) AS TokensAllocated
                                      FROM csp.deploymentprofile d WHERE DeploymentProfileId = @p_DeploymentProfileId";

            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_TokenId", DbType.String, tokenId);
                    Connection.AddInParameter(command, "@p_DeploymentProfileId", DbType.String, deploymentProfileId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            tokenAvailable.NetBalance = dataReader.GetColumnValue<double>("NetBalance");
                        }

                        if (dataReader.NextResult())
                        {
                            if (dataReader.Read())
                            {
                                tokenAvailable.TokensAllocated = dataReader.GetColumnValue<double>("TokensAllocated");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return tokenAvailable;
        }

        public DeploymentProfileList GetDeploymentProfiles(long supportAccountId, long tokenId)
        {
            string sqlQuery = @"
SELECT a.DeploymentProfileId, a.Name, a.Type, a.SubType, a.AltName, a.FlattenedName, a.SubTypeDisplayName, a.SubTypeDescription,
    CASE WHEN a.type = 'CN' THEN a.vmquantity - a.use_count ELSE COUNT(DISTINCT childLic.Serialnumber) END AS FirewallsCreated,
    a.firewallsProjected, a.AuthorizationCode, a.Status, a.PanOS, a.VMModel, a.Panorama, a.vCPUQuantity, a.IsAirgapEligible, a.VcpuAvailable,
    a.VcpuConsumed, a.VcpuAllocated, a.QuantityNode, a.cnxtokens, HasPanorama(a.authcode) HasPanorama, a.expiration,
    IsHubSetupRequired(a.deploymentProfileId) IsHubSetupRequired, a.vSysQuantity, a.AvgTxtRecordsPerDay,
    a.MaxThreatSimulations, a.MaxScans, a.IsAiopsMigrated, a.Note, a.FedRampType, a.Attributes, a.IsGraceAllowed
    FROM
    (
        SELECT d.DeploymentProfileId, l.Licenseid, d.Name, dpt.Name Type, st.Name SubType, st.AltName, st.FlattenedName,
            st.Displayname SubTypeDisplayName, st.Description SubTypeDescription, d.FirewallQuantity firewallsProjected,
            CASE WHEN dps.Name LIKE 'Pending%' OR dps.Name='Failure' THEN '' ELSE l.AuthorizationCode END AuthorizationCode,
            dps.Name Status, d.PanOS, d.VMModel, d.Panorama, d.vCPUQuantity, d.IsAirgapEligible, dp.vcpuusecount VcpuAvailable,
            CASE WHEN dp.islegacy = 1 THEN (lf.vmquantity - lf.use_count) * d.vCPUQuantity ELSE dp.vcpuquantity - dp.vcpuusecount END VcpuConsumed,
            CASE WHEN dp.islegacy = 1 THEN d.FirewallQuantity * d.vCPUQuantity ELSE dp.vcpuquantity END VcpuAllocated,
            d.QuantityNode, c.cnxtokens, lf.expiration, d.vSysQuantity, d.AvgTxtRecordsPerDay, d.MaxThreatSimulations, d.MaxScans, d.IsAiopsMigrated,
            d.Note, d.FedRampType, d.Attributes, COALESCE(dp.isgraceallowed, 0) IsGraceAllowed, lf.vmquantity, l.AuthorizationCode AuthCode, lf.use_count
            FROM csp.deploymentprofile d
            JOIN csp.deploymentprofiletype dpt ON d.DeploymentProfileTypeId=dpt.DeploymentProfileTypeId
            LEFT JOIN csp.Deploymentprofilesubtype st ON dpt.Deploymentprofiletypeid = st.Deploymentprofiletypeid AND d.Deploymentprofilesubtypeid = st.Deploymentprofilesubtypeid
            JOIN csp.license l ON l.LicenseId=D.DpLicenseId
            JOIN csp.deploymentprofilestatus dps ON dps.DeploymentProfileStatusId = d.Status
            JOIN csp.license dpLic  ON dpLic.AuthorizationCode = l.AuthorizationCode
            LEFT JOIN pandb.dpinfo dp ON dp.dplicenseid = d.DpLicenseId
            LEFT JOIN pandb.cnxinfo c ON c.cnxlicenseid = d.DpLicenseId
            LEFT JOIN pandb.lic_features lf ON lf.lfid = dp.dplicenseid AND lf.auth_code = l.AuthorizationCode
            WHERE l.ExpirationDate >= NOW()
            AND (l.IsActive = 1 OR dps.Name='PendingDelete')
            AND d.TokenId = @TokenId 
            AND d.SupportAccountId = @SupportAccountId
    ) a
    LEFT JOIN LATERAL
    (
        SELECT DISTINCT d1.Serialnumber
            FROM csp.license ld1
             JOIN csp.Partmaster pl1 ON ld1.Partmasterid = pl1.Partmasterid AND pl1.Partnumber LIKE 'PAN-%-FLEX%'
            JOIN Device d1 ON ld1.Deviceid = d1.Deviceid AND d1.Isactive = 1
            LEFT JOIN Panoramapartmaster pan1 ON d1.Partmasterid = pan1.Partmasterid           
            WHERE a.LicenseId = ld1.VMParentLicenseId AND ld1.ExpirationDate >= NOW() AND ld1.IsActive = 1
            AND pan1.Partmasterid IS NULL
        UNION
        SELECT DISTINCT d2.Serialnumber
            FROM csp.license ld2
            JOIN csp.Partmaster pl2 ON ld2.Partmasterid = pl2.Partmasterid AND pl2.Partnumber LIKE 'PAN-%-FLEX%'
            JOIN Device d2 ON ld2.Deviceid = d2.Deviceid AND d2.Isactive = 1
            LEFT JOIN Panoramapartmaster pan2 ON d2.Partmasterid = pan2.Partmasterid            
            WHERE a.LicenseId = ld2.Bundlelicenseid AND ld2.ExpirationDate >= NOW() AND ld2.IsActive = 1
            AND pan2.Partmasterid IS NULL
    ) childLic ON TRUE
    GROUP BY a.DeploymentProfileId;
SELECT f.LicIdentifier, f.DisplayName 
    FROM csp.flexsubscriptions f;";

            var deploymentProfileList = new DeploymentProfileList();
            try
            {
                var deploymentProfiles = new List<DeploymentProfile>();
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@TokenId", DbType.UInt64, tokenId);
                    Connection.AddInParameter(command, "@SupportAccountId", DbType.UInt64, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            string type = dataReader.GetColumnValue<string>("Type");
                            string subType = dataReader.GetColumnValue<string>("SubType");

                            var deploymentProfile = new DeploymentProfile()
                            {
                                Id = dataReader.GetColumnValue<int>("DeploymentProfileId"),
                                ProfileName = dataReader.GetColumnValue<string>("Name"),
                                Type = type,
                                SubType = subType,
                                SubTypeAltName = dataReader.GetColumnValue<string>("AltName"),
                                FlattenedName = dataReader.GetColumnValue<string>("FlattenedName"),
                                SubTypeDisplayName = dataReader.GetColumnValue<string>("SubTypeDisplayName"),
                                SubTypeDescription = dataReader.GetColumnValue<string>("SubTypeDescription"),
                                Subscriptions = GetFlexUseCaseSubscription(dataReader.GetColumnValue<int>("DeploymentProfileId")).ToArray(),
                                Features = GetFlexFeatures(dataReader.GetColumnValue<int>("DeploymentProfileId")).ToArray(),
                                UseCaseId = GetUseCaseIds(dataReader.GetColumnValue<int>("DeploymentProfileId")),
                                FirewallsCreated = dataReader.GetColumnValue<int>("FirewallsCreated"),
                                FirewallsProjected = dataReader.GetColumnValue<int>("FirewallsProjected"),
                                FirewallQuantity = dataReader.GetColumnValue<int>("FirewallsProjected"),
                                AuthCode = dataReader.GetColumnValue<string>("AuthorizationCode"),
                                Status = dataReader.GetColumnValue<string>("Status"),
                                PanOS = dataReader.GetColumnValue<string>("PanOS"),
                                VmModel = dataReader.GetColumnValue<string>("vmModel"),
                                Panorama = !string.IsNullOrEmpty(dataReader.GetColumnValue<string>("Panorama")) ? dataReader.GetColumnValue<string>("Panorama").Split(',').ToList() : new List<string>(),
                                VcpuQuantity = dataReader.GetColumnValue<int>("vCPUQuantity"),
                                VcpuAvailable = dataReader.GetColumnValue<int>("VcpuAvailable"),
                                VcpuConsumed = dataReader.GetColumnValue<int>("VcpuConsumed"),
                                VcpuAllocated = dataReader.GetColumnValue<int>("VcpuAllocated"),
                                KubernatesNodes = dataReader.GetColumnValue<int>("QuantityNode"),
                                HasPanorama = dataReader.GetColumnValue<bool>("HasPanorama"),
                                IsAirgapEligible = dataReader.GetColumnValue<bool>("IsAirgapEligible"),
                                ExpirationDate = dataReader.GetColumnValue<DateTime>("expiration"),
                                IsHubSetupRequired = dataReader.GetColumnValue<bool>("IsHubSetupRequired"),
                                VSysQuantity = dataReader.GetColumnValue<int>("vSysQuantity"),
                                AvgTxtRecordsPerDay = dataReader.GetColumnValue<int?>("AvgTxtRecordsPerDay"),
                                MaxThreatSimulations = dataReader.GetColumnValue<int?>("MaxThreatSimulations"),
                                MaxScans = dataReader.GetColumnValue<int?>("MaxScans"),
                                IsAiopsMigrated = dataReader.GetColumnValue<bool>("IsAiopsMigrated"),
                                Note = !string.IsNullOrEmpty(dataReader.GetColumnValue<string>("Note"))? dataReader.GetColumnValue<string>("Note"):string.Empty,
                                FedRampType = (EFedRampType)dataReader.GetColumnValue<int>("FedRampType"),
                                AttributesValues = !string.IsNullOrWhiteSpace(dataReader.GetColumnValue<string>("Attributes"))
                                    ? JsonConvert.DeserializeObject<DeploymentProfileAttributeValue[]>(dataReader.GetColumnValue<string>("Attributes"))
                                    : new DeploymentProfileAttributeValue[0],
                                IsGraceAllowed = dataReader.GetColumnValue<bool?>("IsGraceAllowed") ?? false
                            };

                            if (dataReader.GetColumnValue<string>("Type") == DeploymentProfileType.CN.ToDescription() && ApiConstants.CNNonlegacyPanOs.Any(s => s.Contains(dataReader.GetColumnValue<string>("PanOS"))))
                            {
                                var tokens = dataReader.GetColumnValue<string>("cnxtokens");
                                if (!string.IsNullOrEmpty(tokens))
                                {
                                    var cnxTokens = JsonConvert.DeserializeObject<Dictionary<string, int>>(tokens);
                                    var availableNodeTokens = cnxTokens["node_tokens"];
                                    var availableCoreTokens = cnxTokens["core_tokens"];
                                    deploymentProfile.VcpuAllocated = deploymentProfile.VcpuQuantity;
                                    deploymentProfile.VcpuConsumed = deploymentProfile.VcpuAllocated - availableCoreTokens;
                                }
                            }

                            deploymentProfiles.Add(deploymentProfile);
                        }
                        var subscriptions = new Dictionary<string, string>();

                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                subscriptions.Add(dataReader.GetColumnValue<string>("LicIdentifier"), dataReader.GetColumnValue<string>("DisplayName"));
                            }
                        }
                        deploymentProfileList = new DeploymentProfileList
                        {
                            profiles = deploymentProfiles,
                            availableSubscriptions = subscriptions
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return deploymentProfileList;
        }

        public DeploymentProfile GetDeploymentProfile(string authCode)
        {
            string sqlQuery = @"SELECT 
                                    d.DeploymentProfileId,
                                    d.TokenId, 
                                    d.Name,
                                    dpt.Name Type,
                                    CASE WHEN dpt.Name='CN' THEN lf.vmquantity - lf.use_count ELSE COUNT(DISTINCT childLic.Serialnumber) END AS FirewallsCreated,
                                    d.FirewallQuantity firewallsProjected,
									l.AuthorizationCode,
								    dps.Name Status,
                                    d.PanOS,
                                    d.VMModel,
                                    d.Panorama,
                                    d.vCPUQuantity,
                                    d.IsAirgapEligible,
                                    dp.vcpuusecount VcpuAvailable, 
                                    CASE WHEN dp.islegacy=1 THEN (lf.vmquantity - lf.use_count)*d.vCPUQuantity ELSE dp.vcpuquantity - dp.vcpuusecount END VcpuConsumed,  
                                    CASE WHEN dp.islegacy=1 THEN d.FirewallQuantity*d.vCPUQuantity ELSE dp.vcpuquantity  END VcpuAllocated,
                                     d.QuantityNode,
                                    c.cnxtokens,
                                    HasPanorama(l.AuthorizationCode) HasPanorama,
                                    lf.expiration, d.FedRampType
                            FROM csp.deploymentprofile d 
                                JOIN csp.deploymentprofiletype dpt ON d.DeploymentProfileTypeId=dpt.DeploymentProfileTypeId
                                JOIN csp.license l ON l.LicenseId=D.DpLicenseId
                                JOIN csp.deploymentprofilestatus dps ON dps.DeploymentProfileStatusId = d.Status
                                JOIN csp.license dpLic  ON dpLic.AuthorizationCode=l.AuthorizationCode
                                LEFT JOIN pandb.dpinfo dp ON dp.dplicenseid = d.DpLicenseId
                                LEFT JOIN pandb.cnxinfo c ON c.cnxlicenseid = d.DpLicenseId
                                LEFT JOIN pandb.lic_features lf ON lf.lfid = dp.dplicenseid AND lf.auth_code=l.AuthorizationCode
                                LEFT JOIN LATERAL
                                (
                                    SELECT DISTINCT d1.Serialnumber
                                        FROM csp.license ld1
                                        JOIN Device d1 ON ld1.Deviceid = d1.Deviceid AND d1.Isactive = 1
                                        LEFT JOIN Panoramapartmaster pan1 ON d1.Partmasterid = pan1.Partmasterid
                                        WHERE l.LicenseId = ld1.VMParentLicenseId AND ld1.ExpirationDate >= NOW() AND ld1.IsActive = 1
                                        AND pan1.Partmasterid IS NULL

                                    UNION
                                    SELECT DISTINCT d2.Serialnumber
                                        FROM csp.license ld2
                                        JOIN Device d2 ON ld2.Deviceid = d2.Deviceid AND d2.Isactive = 1
                                        LEFT JOIN Panoramapartmaster pan2 ON d2.Partmasterid = pan2.Partmasterid
                                        WHERE l.LicenseId = ld2.Bundlelicenseid AND ld2.ExpirationDate >= NOW() AND ld2.IsActive = 1
                                        AND pan2.Partmasterid IS NULL
                                ) childLic ON TRUE
                                WHERE  l.ExpirationDate > NOW() AND (l.IsActive <> 0 OR dps.Name='PendingDelete') AND 
                                l.AuthorizationCode=@AuthorizationCode";

            DeploymentProfile deploymentProfile = null;
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@AuthorizationCode", DbType.String, authCode);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            deploymentProfile = new DeploymentProfile()
                            {
                                Id = dataReader.GetColumnValue<int>("DeploymentProfileId"),
                                ProfileName = dataReader.GetColumnValue<string>("Name"),
                                Type = dataReader.GetColumnValue<string>("Type"),
                                Subscriptions = GetFlexUseCaseSubscription(dataReader.GetColumnValue<int>("DeploymentProfileId")).ToArray(),
                                UseCaseId = GetUseCaseIds(dataReader.GetColumnValue<int>("DeploymentProfileId")),
                                FirewallsCreated = dataReader.GetColumnValue<int>("FirewallsCreated"),
                                FirewallsProjected = dataReader.GetColumnValue<int>("FirewallsProjected"),
                                FirewallQuantity = dataReader.GetColumnValue<int>("FirewallsProjected"),
                                AuthCode = dataReader.GetColumnValue<string>("AuthorizationCode"),
                                Status = dataReader.GetColumnValue<string>("Status"),
                                PanOS = dataReader.GetColumnValue<string>("PanOS"),
                                VmModel = dataReader.GetColumnValue<string>("vmModel"),
                                Panorama = !string.IsNullOrEmpty(dataReader.GetColumnValue<string>("Panorama")) ? dataReader.GetColumnValue<string>("Panorama").Split(',').ToList() : new List<string>(),
                                VcpuQuantity = dataReader.GetColumnValue<int>("vCPUQuantity"),
                                VcpuAvailable = dataReader.GetColumnValue<int>("VcpuAvailable"),
                                VcpuConsumed = dataReader.GetColumnValue<int>("VcpuConsumed"),
                                VcpuAllocated = dataReader.GetColumnValue<int>("VcpuAllocated"),
                                IsAirgapEligible = dataReader.GetColumnValue<bool>("IsAirgapEligible"),
                                KubernatesNodes = dataReader.GetColumnValue<int>("QuantityNode"),
                                HasPanorama = dataReader.GetColumnValue<bool>("HasPanorama"),
                                ExpirationDate = dataReader.GetColumnValue<DateTime>("expiration"),
                                TokenId = dataReader.GetColumnValue<long>("TokenId"),
                                FedRampType = (EFedRampType)dataReader.GetColumnValue<int>("FedRampType")
                            };

                            if (dataReader.GetColumnValue<string>("Type") == DeploymentProfileType.CN.ToDescription() && ApiConstants.CNNonlegacyPanOs.Any(s => s.Contains(dataReader.GetColumnValue<string>("PanOS"))))
                            {
                                var tokens = dataReader.GetColumnValue<string>("cnxtokens");
                                if (!string.IsNullOrEmpty(tokens))
                                {
                                    var cnxTokens = JsonConvert.DeserializeObject<Dictionary<string, int>>(tokens);
                                    var availableNodeTokens = cnxTokens["node_tokens"];
                                    var availableCoreTokens = cnxTokens["core_tokens"];
                                    deploymentProfile.VcpuAllocated = deploymentProfile.VcpuQuantity;
                                    deploymentProfile.VcpuConsumed = deploymentProfile.VcpuAllocated - availableCoreTokens;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return deploymentProfile;
        }

        public IEnumerable<TokenTransferHistory> GetTransactionHistory(string tokenId)
        {
            var transferHistoryList = new List<TokenTransferHistory>();
            try
            {
                using (var command = Connection.GetStoredProcCommand("GetTokenTransferHistory"))
                {
                    Connection.AddInParameter(command, "@p_TokenId", DbType.Int64, tokenId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var transactionHistory = new TokenTransferHistory
                            {
                                Id = dataReader.GetColumnValue<long>("Id"),
                                RequestedOn = dataReader.GetColumnValue<string>("DateTime"),
                                Name = dataReader.GetColumnValue<string>("Name"),
                                Description = dataReader.GetColumnValue<string>("Description"),
                                Quantity = dataReader.GetColumnValue<double>("Quantity"),
                                TransactionType = dataReader.GetColumnValue<string>("TransactionType"),
                                EndBalance = dataReader.GetColumnValue<double>("EndBalance")
                            };
                            transferHistoryList.Add(transactionHistory);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return transferHistoryList;
        }

        public IEnumerable<DeploymentProfileAudit> GetDeploymentProfileAudit(string tokenId)
        {
            var deploymentProfileAudit = new List<DeploymentProfileAudit>();
            try
            {
                using (var command = Connection.GetStoredProcCommand("GetDeploymentProfileAudit"))
                {
                    Connection.AddInParameter(command, "@p_TokenId", DbType.Int64, tokenId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var transactionHistory = new DeploymentProfileAudit
                            {
                                Id = dataReader.GetColumnValue<long>("Id"),
                                CreatedOn = dataReader.GetColumnValue<string>("CreatedOn"),
                                ProfileName = dataReader.GetColumnValue<string>("ProfileName"),
                                AuthCode = dataReader.GetColumnValue<string>("AuthCode"),
                                ModifiedBy = dataReader.GetColumnValue<string>("ModifiedBy"),
                                Status = dataReader.GetColumnValue<string>("Status"),
                                Description = dataReader.GetColumnValue<string>("Description").TrimEnd(','),
                            };
                            deploymentProfileAudit.Add(transactionHistory);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return deploymentProfileAudit;
        }

        public IEnumerable<DeploymentProfileAuditDetail> GetDeploymentProfileAuditDetail(long auditId)
        {
            var deploymentProfileAuditDetail = new List<DeploymentProfileAuditDetail>();
            try
            {
                using (var command = Connection.GetStoredProcCommand("GetDeploymentProfileAuditDetail"))
                {
                    Connection.AddInParameter(command, "@p_AuditId", DbType.Int64, auditId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        int auditDetailId = 0;
                        if (dataReader.Read())
                        {
                            string oldProfileName = dataReader.GetColumnValue<string>("OldProfileName");
                            string newProfileName = dataReader.GetColumnValue<string>("NewProfileName");
                            string oldTokensAllocated = dataReader.GetColumnValue<string>("OldTokensAllocated");
                            string newTokensAllocated = dataReader.GetColumnValue<string>("NewTokensAllocated");
                            string oldFirewallQuantity = dataReader.GetColumnValue<string>("OldFirewallQuantity");
                            string newFirewallQuantity = dataReader.GetColumnValue<string>("NewFirewallQuantity");
                            string oldVcpuQuantity = dataReader.GetColumnValue<string>("OldVcpuQuantity");
                            string newVcpuQuantity = dataReader.GetColumnValue<string>("NewVcpuQuantity");
                            string oldSubscriptions = dataReader.GetColumnValue<string>("OldSubscriptions");
                            string newSubscriptions = dataReader.GetColumnValue<string>("newSubscriptions");
                            string oldVMModel = dataReader.GetColumnValue<string>("OldVMModel");
                            string newVMModel = dataReader.GetColumnValue<string>("NewVMModel");
                            string oldPanorama = dataReader.GetColumnValue<string>("OldPanorama");
                            string newPanorama = dataReader.GetColumnValue<string>("NewPanorama");
                            string status = dataReader.GetColumnValue<string>("Status");
                            string profileType = dataReader.GetColumnValue<string>("ProfileType");
                            string panOsVersion = dataReader.GetColumnValue<string>("PanOS");
                            string oldTokenId = dataReader.GetColumnValue<string>("OldTokenId");
                            string newTokenId = dataReader.GetColumnValue<string>("NewTokenId");
                            string oldAvgTxtRecordsPerDay = dataReader.GetColumnValue<string>("OldAvgTxtRecordsPerDay");
                            string newAvgTxtRecordsPerDay = dataReader.GetColumnValue<string>("NewAvgTxtRecordsPerDay");
                            string oldMaxThreatSimulations = dataReader.GetColumnValue<string>("OldMaxThreatSimulations");
                            string newMaxThreatSimulations = dataReader.GetColumnValue<string>("NewMaxThreatSimulations");
                            string oldMaxScans = dataReader.GetColumnValue<string>("OldMaxScans");
                            string newMaxScans = dataReader.GetColumnValue<string>("NewMaxScans");
                            string oldAttributes = dataReader.GetColumnValue<string>("OldAttributes");
                            string newAttributes = dataReader.GetColumnValue<string>("NewAttributes");

                            if (oldTokensAllocated != newTokensAllocated)
                            {
                                deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                {
                                    Id = ++auditDetailId,
                                    ChangeDetail = "TokensAllocated",
                                    PreviousVersion = !string.IsNullOrEmpty(oldTokensAllocated) ? new List<string> { oldTokensAllocated } : new List<string>(),
                                    NewVersion = !string.IsNullOrEmpty(newTokensAllocated) ? new List<string> { newTokensAllocated } : new List<string>(),
                                });
                            }

                            if (oldProfileName != newProfileName)
                            {
                                deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                {
                                    Id = ++auditDetailId,
                                    ChangeDetail = "ProfileName",
                                    PreviousVersion = !string.IsNullOrEmpty(oldProfileName) ? new List<string> { oldProfileName } : new List<string>(),
                                    NewVersion = !string.IsNullOrEmpty(newProfileName) ? new List<string> { newProfileName } : new List<string>(),
                                });
                            }

                            if (oldFirewallQuantity != newFirewallQuantity && ApiConstants.CNNonlegacyPanOs.Any(s => s.Contains(panOsVersion)))
                            {
                                deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                {
                                    Id = ++auditDetailId,
                                    ChangeDetail = "FirewallQuantity",
                                    PreviousVersion = !string.IsNullOrEmpty(oldFirewallQuantity) ? new List<string> { oldFirewallQuantity } : new List<string>(),
                                    NewVersion = !string.IsNullOrEmpty(newFirewallQuantity) ? new List<string> { newFirewallQuantity } : new List<string>(),
                                });
                            }

                            if ((oldVcpuQuantity != newVcpuQuantity && profileType != DeploymentProfileType.CN.ToDescription())
                                 || (oldVcpuQuantity != newVcpuQuantity && profileType == DeploymentProfileType.CN.ToDescription() && panOsVersion != ApiConstants.CNLegacyPanOs))
                            {
                                deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                {
                                    Id = ++auditDetailId,
                                    ChangeDetail = ApiConstants.CNNonlegacyPanOs.Any(s => s.Contains(panOsVersion)) ? "VcpuQuantityCore" : "VcpuQuantity",
                                    PreviousVersion = !string.IsNullOrEmpty(oldVcpuQuantity) ? new List<string> { oldVcpuQuantity } : new List<string>(),
                                    NewVersion = !string.IsNullOrEmpty(newVcpuQuantity) ? new List<string> { newVcpuQuantity } : new List<string>(),
                                });
                            }


                            if ((oldVMModel != newVMModel && profileType != DeploymentProfileType.CN.ToDescription())
                                 || (oldVMModel != newVMModel && profileType == DeploymentProfileType.CN.ToDescription() && panOsVersion != ApiConstants.CNLegacyPanOs))
                            {
                                deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                {
                                    Id = ++auditDetailId,
                                    ChangeDetail = "VMModel",
                                    PreviousVersion = !string.IsNullOrEmpty(oldVMModel) ? new List<string> { oldVMModel } : new List<string>(),
                                    NewVersion = !string.IsNullOrEmpty(newVMModel) ? new List<string> { newVMModel } : new List<string>(),
                                });
                            }

                            if ((oldPanorama != newPanorama && profileType != DeploymentProfileType.CN.ToDescription())
                                 || (oldPanorama != newPanorama && profileType == DeploymentProfileType.CN.ToDescription() && panOsVersion != ApiConstants.CNLegacyPanOs))
                            {
                                deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                {
                                    Id = ++auditDetailId,
                                    ChangeDetail = "Panorama",
                                    PreviousVersion = !string.IsNullOrEmpty(oldPanorama) ? oldPanorama.Split(',').ToList() : new List<string>(),
                                    NewVersion = !string.IsNullOrEmpty(newPanorama) ? newPanorama.Split(',').ToList() : new List<string>(),
                                });
                            }
                            if (oldSubscriptions != newSubscriptions)
                            {
                                deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                {
                                    Id = ++auditDetailId,
                                    ChangeDetail = "Subscriptions",
                                    PreviousVersion = !string.IsNullOrEmpty(oldSubscriptions) ? oldSubscriptions.Split(',').ToList() : new List<string>(),
                                    NewVersion = !string.IsNullOrEmpty(newSubscriptions) ? newSubscriptions.Split(',').ToList() : new List<string>()
                                });
                            }

                            if (oldTokenId != newTokenId)
                            {
                                deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                {
                                    Id = ++auditDetailId,
                                    ChangeDetail = "TokenId",
                                    PreviousVersion = !string.IsNullOrEmpty(oldTokenId) ? new List<string> { oldTokenId } : new List<string>(),
                                    NewVersion = !string.IsNullOrEmpty(newTokenId) ? new List<string> { newTokenId } : new List<string>(),
                                });
                            }

                            if (oldAvgTxtRecordsPerDay != newAvgTxtRecordsPerDay)
                            {
                                deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                {
                                    Id = ++auditDetailId,
                                    ChangeDetail = "AvgTxtRecordsPerDay",
                                    PreviousVersion = !string.IsNullOrEmpty(oldAvgTxtRecordsPerDay) ? new List<string> { oldAvgTxtRecordsPerDay } : new List<string>(),
                                    NewVersion = !string.IsNullOrEmpty(newAvgTxtRecordsPerDay) ? new List<string> { newAvgTxtRecordsPerDay } : new List<string>(),
                                });
                            }

                            if (oldMaxThreatSimulations != newMaxThreatSimulations)
                            {
                                deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                {
                                    Id = ++auditDetailId,
                                    ChangeDetail = "MaxThreatSimulations",
                                    PreviousVersion = !string.IsNullOrEmpty(oldMaxThreatSimulations) ? new List<string> { oldMaxThreatSimulations } : new List<string>(),
                                    NewVersion = !string.IsNullOrEmpty(newMaxThreatSimulations) ? new List<string> { newMaxThreatSimulations } : new List<string>(),
                                });
                            }

                            if (oldMaxScans != newMaxScans)
                            {
                                deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                {
                                    Id = ++auditDetailId,
                                    ChangeDetail = "MaxScans",
                                    PreviousVersion = !string.IsNullOrEmpty(oldMaxScans) ? new List<string> { oldMaxScans } : new List<string>(),
                                    NewVersion = !string.IsNullOrEmpty(newMaxScans) ? new List<string> { newMaxScans } : new List<string>(),
                                });
                            }

                            if (oldAttributes != newAttributes)
                            {
                                try
                                {
                                    List<DeploymentProfileAttributeValue> oldAttrs = JsonConvert.DeserializeObject<List<DeploymentProfileAttributeValue>>(string.IsNullOrWhiteSpace(oldAttributes) ? "[]" : oldAttributes);
                                    List<DeploymentProfileAttributeValue> newAttrs = JsonConvert.DeserializeObject<List<DeploymentProfileAttributeValue>>(string.IsNullOrWhiteSpace(newAttributes) ? "[]" : newAttributes);

                                    oldAttrs.ForEach(o =>
                                    {
                                        DeploymentProfileAttributeValue n = newAttrs.FirstOrDefault(i => i.UnitOfMeasure == o.UnitOfMeasure);

                                        if (n != null) // Old exists/New exists - attribute value changed
                                        {
                                            if (n.Value != o.Value)
                                            {
                                                deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                                {
                                                    Id = ++auditDetailId,
                                                    ChangeDetail = o.UnitOfMeasure,
                                                    PreviousVersion = !string.IsNullOrEmpty(o.Value) ? new List<string> { o.Value } : new List<string>(),
                                                    NewVersion = !string.IsNullOrEmpty(n.Value) ? new List<string> { n.Value } : new List<string>(),
                                                });
                                            }
                                        }
                                        else // Old exists/New does not exist - attribute removed
                                        {
                                            deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                            {
                                                Id = ++auditDetailId,
                                                ChangeDetail = o.UnitOfMeasure,
                                                PreviousVersion = !string.IsNullOrEmpty(o.Value) ? new List<string> { o.Value } : new List<string>(),
                                                NewVersion = new List<string>(),
                                            });
                                        }
                                    });
                                    // Old does not exist/New exists - attribute added
                                    newAttrs.Where(n => !oldAttrs.Any(o => o.UnitOfMeasure == n.UnitOfMeasure)).ToList().ForEach(n =>
                                    {
                                        deploymentProfileAuditDetail.Add(new DeploymentProfileAuditDetail
                                        {
                                            Id = ++auditDetailId,
                                            ChangeDetail = n.UnitOfMeasure,
                                            PreviousVersion = new List<string>(),
                                            NewVersion = !string.IsNullOrEmpty(n.Value) ? new List<string> { n.Value } : new List<string>(),
                                        });
                                    });
                                }
                                catch (Exception ex1)
                                {
                                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex1);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return deploymentProfileAuditDetail;
        }

        public IEnumerable<Hypervisor> GetHyperVisors(string firewallType = "")
        {
            var hyperVisors = new List<Hypervisor>();
            try
            {
                const string sqlQuery = @"
SELECT h.displayvalue, h.hypervisorversion 
    FROM csp.hypervisor h 
    ORDER BY h.displayvalue;
";
                const string sqlQueryAi = @"
SELECT h.DisplayValue, h.HyperVisorVersion
    FROM csp.HyperVisor h
    WHERE h.HyperVisorVersion IN ('VM-GCP', 'VM-AWS', 'VM-AZR', 'VM-KVM')
    ORDER BY h.DisplayValue;
";

                using (var command = Connection.GetSqlStringCommand(firewallType == "AI" ? sqlQueryAi : sqlQuery))
                {
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var hypervisorItem = new Hypervisor()
                            {
                                DisplayValue = Convert.ToString(dataReader["displayvalue"]),
                                HypervisorVersion = Convert.ToString(dataReader["hypervisorversion"])
                            };

                            hyperVisors.Add(hypervisorItem);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return hyperVisors;
        }

        public IEnumerable<PanOsVersion> GetPanOsVersions(string panOs)
        {
            var panOsVersions = new List<PanOsVersion>();
            var panOsversion = panOs.Substring(0, panOs.IndexOf("_"));

            try
            {
                string comparisionOperator = string.Empty;
                if (panOs.Contains("or-below"))
                    comparisionOperator = "<=";
                else
                    comparisionOperator = ">=";

                string sqlQuery = @"
                    SELECT 
                      p.panosversion
                    FROM  csp.panos p  WHERE 
                      CONCAT(LPAD(SUBSTRING_INDEX(SUBSTRING_INDEX(SUBSTRING_INDEX(p.panosversion, '-', 1), '.', 1), '.', -1), 2, '0'),
                     LPAD(SUBSTRING_INDEX(SUBSTRING_INDEX(SUBSTRING_INDEX(p.panosversion, '-', 1), '.', 2), '.', -1), 2, '0'),
                       LPAD(SUBSTRING_INDEX(SUBSTRING_INDEX(SUBSTRING_INDEX(p.panosversion, '-', 1), '.', 3), '.', -1), 2, '0')) 
                    {0} CONCAT(LPAD(SUBSTRING_INDEX(SUBSTRING_INDEX(SUBSTRING_INDEX(@panOsversion, '-', 1), '.', 1), '.', -1), 2, '0'),
                    LPAD(SUBSTRING_INDEX(SUBSTRING_INDEX(SUBSTRING_INDEX(@panOsversion, '-', 1), '.', 2), '.', -1), 2, '0'),
                    LPAD(SUBSTRING_INDEX(SUBSTRING_INDEX(SUBSTRING_INDEX(@panOsversion, '-', 1), '.', 3), '.', -1), 2, '0'))";

                sqlQuery = string.Format(sqlQuery, comparisionOperator);

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@panOsversion", DbType.String, panOsversion);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var version = new PanOsVersion()
                            {
                                PanOsVersionLabel = dataReader.GetColumnValue<string>("panosversion"),
                                PanOsVersionValue = dataReader.GetColumnValue<string>("panosversion"),
                            };
                            panOsVersions.Add(version);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return panOsVersions;
        }

        public IEnumerable<PanoramaProvisionedDevice> GetProvisionedPanoramas(string authCode)
        {
            var PanoramaProvisionedDevices = new List<PanoramaProvisionedDevice>();
            try
            {
                using (var command = Connection.GetStoredProcCommand("GetProvisionedPanoramas"))
                {
                    Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var device = new PanoramaProvisionedDevice
                            {
                                Id = dataReader.GetColumnValue<long>("LicenseId"),
                                SerialNumber = dataReader.GetColumnValue<string>("SerialNumber"),
                                License = dataReader.GetColumnValue<string>("License"),
                                AuthCode = dataReader.GetColumnValue<string>("AuthCode"),
                                ExpirationDate = dataReader.GetColumnValue<DateTime?>("Expiration").HasValue
                                                ? dataReader.GetColumnValue<DateTime?>("Expiration").Value.ToShortDateString()
                                                : string.Empty,
                                IsFwFlexPanorama = dataReader.GetColumnValue<bool>("IsFwFlexPanorama"),
                                FedRampType = (EFedRampType)dataReader.GetColumnValue<int>("FedRampType")
                            };
                            PanoramaProvisionedDevices.Add(device);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return PanoramaProvisionedDevices;
        }

        public IEnumerable<DeploymentProfileInfo> GetNumberofDeployments(long supportAccountId)
        {
            var profileInfoList = new List<DeploymentProfileInfo>();
            const string sqlQuery = @"SELECT d.SupportAccountId, d.TokenId, COUNT(*) AS NumberOfDeployments
                                      FROM   csp.deploymentprofile d 
                                      JOIN   csp.deploymentprofilestatus ds ON ds.DeploymentProfileStatusId = d.Status
                                      WHERE  d.SupportAccountId = @p_SupportAccountId AND ds.DeploymentProfileStatusId NOT IN (6,7)
                                      GROUP BY d.SupportAccountId, d.TokenId;";
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var profileInfo = new DeploymentProfileInfo()
                            {
                                SupportAccountId = dataReader.GetColumnValue<long>("SupportAccountId"),
                                TokenId = dataReader.GetColumnValue<long>("TokenId"),
                                NumberOfDeployments = dataReader.GetColumnValue<int>("NumberOfDeployments"),
                            };

                            profileInfoList.Add(profileInfo);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return profileInfoList;
        }

        public IEnumerable<DeploymentProfileInfo> GetNumberofRenewalDeployments(long supportAccountId, long tokenId)
        {
            var profileInfoList = new List<DeploymentProfileInfo>();
            const string sqlQuery = @"SELECT 
                                        d.SupportAccountId,
                                        d.TokenId,
                                        COUNT(d.DeploymentProfileId) AS NumberOfRenewalDeployments
                                      
                                      FROM csp.deploymentprofile d 
                                      INNER JOIN csp.license l ON l.LicenseId = d.DpLicenseId
                                      INNER JOIN csp.deploymentprofilestatus dps ON dps.DeploymentProfileStatusId = d.Status
                                      INNER JOIN pandb.dpinfo dp ON dp.dplicenseid = d.DpLicenseId 
                                      AND dp.isgraceallowed = 1
                                      WHERE d.TokenId = @p_TokenId
                                          AND d.SupportAccountId = @p_SupportAccountId
                                          AND l.ExpirationDate > NOW() 
                                          AND (l.IsActive <> 0 OR dps.Name = 'PendingDelete')
                                      GROUP BY d.SupportAccountId, d.TokenId;";
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_TokenId", DbType.Int64, tokenId);
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var profileInfo = new DeploymentProfileInfo()
                            {
                                SupportAccountId = dataReader.GetColumnValue<long>("SupportAccountId"),
                                TokenId = dataReader.GetColumnValue<long>("TokenId"),
                                NumberOfDeployments = dataReader.GetColumnValue<int>("NumberOfRenewalDeployments")
                               
                            };

                            profileInfoList.Add(profileInfo);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return profileInfoList;
        }

        public IEnumerable<(long tokenId, bool hasSaasCloud, bool HasCloudNgfw)> GetCreditPoolTraits(long[] acpIds)
        {
            if (!(acpIds?.Length > 0))
            {
                return new List<(long tokenId, bool hasSaasCloud, bool HasCloudNgfw)>();
            }

            List<(long tokenId, bool hasSaasCloud, bool HasCloudNgfw)> results = new List<(long tokenId, bool hasSaasCloud, bool HasCloudNgfw)>();
            const string sqlQuery = @"
SELECT dp.Tokenid,
        SUM(CASE WHEN dt.Name = 'AI' AND ds.Name = 'APIM' THEN 1 ELSE 0 END) > 0 AS HasSaasCloud,
        SUM(CASE WHEN dt.Name = 'CLOUD' AND ds.Name = 'CNGFW' THEN 1 ELSE 0 END) > 0 AS HasCloudNgfw
    FROM csp.Deploymentprofile dp
    JOIN csp.License l ON dp.Dplicenseid = l.Licenseid AND l.Isactive = 1 AND l.Expirationdate > NOW()
    JOIN csp.Deploymentprofiletype dt ON dp.Deploymentprofiletypeid = dt.Deploymentprofiletypeid
    JOIN csp.Deploymentprofilesubtype ds ON dp.Deploymentprofiletypeid = ds.Deploymentprofiletypeid AND dp.Deploymentprofilesubtypeid = ds.Deploymentprofilesubtypeid
    WHERE FIND_IN_SET(dp.Tokenid, @p_TokenIds)
    AND dp.Isactive = 1
    GROUP BY dp.Tokenid;
";
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_TokenIds", DbType.String, string.Join(",", acpIds));
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            results.Add((dataReader.GetColumnValue<long>("Tokenid"),
                                dataReader.GetColumnValue<long>("HasSaasCloud") > 0,
                                dataReader.GetColumnValue<long>("HasCloudNgfw") > 0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return results;
        }

        public List<string> GetSubscriptionsByUseCase(List<int> useCase)
        {
            var subscriptions = new List<string>();

            const string sqlQuery = @"SELECT  fs.LicIdentifier FROM csp.flexusecasesubscriptionsmapping f 
                                      LEFT JOIN csp.flexsubscriptions fs ON fs.FlexSubscriptionId = f.FlexSubscriptionId WHERE f.FlexUseCaseId = @p_UseCase";
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_UseCase", DbType.String, string.Join(",", useCase));
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            subscriptions.Add(dataReader.GetColumnValue<string>("LicIdentifier"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return subscriptions;
        }

        public List<string> GetSubscriptionsByDeploymentProfileId(long? deploymentProfileId)
        {
            string sqlQuery = @"SELECT GROUP_CONCAT(f.LicIdentifier) FROM csp.deploymentprofilesubscriptions d 
                                    JOIN csp.flexsubscriptions f ON d.FlexSubscriptionId=f.FlexSubscriptionId 
                                    WHERE d.DeploymentProfileId=@DeploymentProfileId";

            var subscriptionList = new List<string>();
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@DeploymentProfileId", DbType.Int32, deploymentProfileId);
                    var subscriptions = Connection.ExecuteScalar(command);
                    subscriptionList = subscriptions.ToString().Split(',').ToList();
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return subscriptionList;
        }

        public int GetFirewallsCreated(string authCode)
        {
            int firewallsCreated = 0;
            const string sqlQuery = @"SELECT COUNT(child.LicenseId) AS FirewallCreated FROM csp.license l JOIN csp.license child ON l.LicenseId = child.VMParentLicenseId 
                                      WHERE l.AuthorizationCode = @p_AuthCode AND child.IsActive;";
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                    firewallsCreated = Convert.ToInt32(Connection.ExecuteScalar(command));
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return firewallsCreated;
        }

        public AscAccountInfo GetAscAccountInfo(string salesforceAccountId)
        {
            var ascAccountInfo = new AscAccountInfo();
            try
            {
                const string sqlQuery = @"SELECT s.SupportAccountId AscId, S.Name AscName FROM csp.supportaccount s
                                                 LEFT JOIN CSP.sapsupportaccountmapping s1 ON s.SupportAccountId = s1.SupportAccountId
                                                 JOIN csp.accounttype a ON a.AccountTypeId = s.AccountTypeId AND a.Name='ASC'
                                          WHERE  s.IsActive AND s1.IsPrimary AND s.SalesforceAccountId = @p_SalesforceAccountId LIMIT 1";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SalesforceAccountId", DbType.String, salesforceAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            ascAccountInfo.AscId = dataReader.GetColumnValue<long>("AscId");
                            ascAccountInfo.AscName = dataReader.GetColumnValue<string>("AscName");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return ascAccountInfo;
        }

        public List<FwSerialAndSubscription> GetFirewalls(long supportAccountId, long userAccountId, string authCode, string type, string subType = null)
        {
            var fwSerialAndSubscription = new List<FwSerialAndSubscription>();

            try
            {
                var deploymentProfileTypeId = Enum.Parse(typeof(DeploymentProfileType), type, true);
                using (var command = Connection.GetStoredProcCommand("GetFirewalls_V2"))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                    Connection.AddInParameter(command, "@p_Type", DbType.Int32, deploymentProfileTypeId);
                    Connection.AddInParameter(command, "@p_SubType", DbType.String, subType);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            fwSerialAndSubscription.Add(
                                new FwSerialAndSubscription
                                {
                                    SerialNumber = dataReader.GetColumnValue<string>("SerialNumber"),
                                    Subscriptions = dataReader.GetColumnValue<string>("Subscriptions")
                                });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return fwSerialAndSubscription;
        }

        public List<string> DeactivateFirewall(List<string> serialNumbers, string userEmailId)
        {
            var response = new List<string>();
            for (int i = 0; i < serialNumbers.Count; i++)
            {
                try
                {
                    using (var command = PANDBConnection.GetStoredProcCommand("uspUnlicenseVM"))
                    {
                        PANDBConnection.AddInParameter(command, "p_serialnumber", DbType.String, serialNumbers[i]);
                        PANDBConnection.AddInParameter(command, "reason", DbType.String, "Firewall Flex - Deregister from CSP");
                        PANDBConnection.AddInParameter(command, "whoby", DbType.String, userEmailId);
                        PANDBConnection.ExecuteNonQuery(command);
                    }
                }
                catch (Exception exception)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(exception);
                    response.Add(serialNumbers[i]);
                }
            }

            return response;
        }

        public List<WarmPoolDeviceState> GetWarmPoolDeviceState(List<string> serialNumbers)
        {
            string sqlQuery = @"
SELECT w.Id, d.DeviceId, d.Serialnumber, IFNULL(w.Targetstate, 'Active') Targetstate
    FROM csp.device d
    JOIN pandb.devicestate w ON d.DeviceId = w.DeviceId AND w.Active = 1
    WHERE d.Serialnumber IN ('{0}')
    AND d.Isactive = 1
    AND w.Targetstate <> 'De-registered';
";
            List<WarmPoolDeviceState> result = new List<WarmPoolDeviceState>();
            try
            {
                using (var command = Connection.GetSqlStringCommand(string.Format(sqlQuery, string.Join("','", serialNumbers))))
                {
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            result.Add(new WarmPoolDeviceState
                            {
                                Id = dataReader.GetColumnValue<long>("Id"),
                                DeviceId = dataReader.GetColumnValue<long>("DeviceId"),
                                SerialNumber = dataReader.GetColumnValue<string>("Serialnumber"),
                                TargetState = dataReader.GetColumnValue<string>("Targetstate")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return result;
        }

        public void UpdateWarmPoolDeviceState(List<WarmPoolDeviceState> warmStates, string userEmail)
        {
            if ((warmStates?.Count ?? 0) == 0)
            {
                return;
            }

            // Update old targetState active = 0 in "devicestate" table 
            // and add new row with "De-registered" state in new table "devicestate" for all device Id - active = 1 

            try
            {
                // update the current state of Active to 0 of the device(s)
                const string sqlUpdateStateIds = @"
UPDATE pandb.devicestate w
    SET w.Active = 0, 
    Modifiedby = '{1}'
    WHERE w.id IN ({0});
";
                using (var command = Connection.GetSqlStringCommand(string.Format(sqlUpdateStateIds, string.Join(", ", warmStates.Select(i => i.Id)), userEmail)))
                {
                    int affectedRecords = Connection.ExecuteNonQuery(command);
                    if (affectedRecords == 0)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("FirewallFlexDataAccess::UpdateWarmPoolDeviceState(IEnumerable<string> '{0}', string '{1}') made no updates in pandb.devicestate. stateIdRows: [{2}]", string.Join("', '", warmStates), userEmail, JsonConvert.SerializeObject(warmStates))));
                    }
                }

                // insert new De-registered state of the device(s)
                const string sqlInsertStateHeader = @"
INSERT INTO pandb.devicestate (Deviceid, Targetstate, Active, Createdby) VALUES ";
                const string sqlInsertStateRow = @"
    ({0}, 'De-registered', 1, '{1}'){2}";
                int count = 0;
                string sqlInserts = sqlInsertStateHeader;
                foreach (WarmPoolDeviceState warmState in warmStates)
                {
                    sqlInserts += string.Format(sqlInsertStateRow, warmState.DeviceId, userEmail, ++count == warmStates.Count ? ";" : ",");
                }
                using (var command = Connection.GetSqlStringCommand(sqlInserts))
                {
                    int affectedRecords = Connection.ExecuteNonQuery(command);
                    if (affectedRecords == 0)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("FirewallFlexDataAccess::UpdateWarmPoolDeviceState(IEnumerable<string> '{0}', string '{1}') made no inserts in pandb.devicestate. stateIdRows: [{2}]", string.Join("', '", warmStates), userEmail, JsonConvert.SerializeObject(warmStates))));
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
        }

        public List<PanoramaMigration> GetPanoramasForMigration(long supportAccountId, string authCode, string type, string subType = null)
        {
            var PanoramaProvisionedDevices = new List<PanoramaMigration>();
            try
            {
                var deploymentProfileTypeId = Enum.Parse(typeof(DeploymentProfileType), type, true);
                using (var command = Connection.GetStoredProcCommand("GetPanoramasForMigration_V2"))
                {
                    Connection.AddInParameter(command, "@p_SupportAccountId", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                    Connection.AddInParameter(command, "@p_Type", DbType.Int32, deploymentProfileTypeId);
                    Connection.AddInParameter(command, "@p_SubType", DbType.Int32, subType);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var device = new PanoramaMigration
                            {
                                Id = dataReader.GetColumnValue<long>("DeviceId"),
                                SerialNumber = dataReader.GetColumnValue<string>("SerialNumber"),
                                Resides = dataReader.GetColumnValue<string>("Resides"),
                                ModelName = dataReader.GetColumnValue<string>("ModelName"),
                                ExpirationDate = !string.IsNullOrEmpty(dataReader.GetColumnValue<string>("CurrentExpirationDate"))
                                                ? dataReader.GetColumnValue<DateTime?>("CurrentExpirationDate").Value.ToShortDateString()
                                                : string.Empty,
                                NewExpirationDate = !string.IsNullOrEmpty(dataReader.GetColumnValue<string>("NewExpirationDate"))
                                                ? dataReader.GetColumnValue<DateTime?>("NewExpirationDate").Value.ToShortDateString()
                                                : string.Empty,

                                SourceAuthcode = dataReader.GetColumnValue<string>("AuthorizationCode")
                                                    
                            };
                            PanoramaProvisionedDevices.Add(device);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return PanoramaProvisionedDevices;
        }

        public List<PanoramaMigrationDetail> GetPanoramaMigrationDetail(long deviceId, string authCode)
        {
            var panoramaMigrationDetailList = new List<PanoramaMigrationDetail>();
            try
            {
                using (var command = Connection.GetStoredProcCommand("GetPanoramaMigrationDetail"))
                {
                    Connection.AddInParameter(command, "@p_DeviceId", DbType.Int64, deviceId);
                    Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var device = new PanoramaMigrationDetail
                            {
                                Id = dataReader.GetColumnValue<long>("LicenseId"),
                                AuthCode = dataReader.GetColumnValue<string>("AuthCode"),
                                CurrentLicense = dataReader.GetColumnValue<string>("License"),
                                NewLicense = dataReader.GetColumnValue<string>("NewLicense"),
                                ExpirationDate = !string.IsNullOrEmpty(dataReader.GetColumnValue<string>("ExpirationDate")) && dataReader.GetColumnValue<int>("LicenseType") == 0
                                                ? dataReader.GetColumnValue<DateTime?>("ExpirationDate").Value.ToShortDateString()
                                                : !string.IsNullOrEmpty(dataReader.GetColumnValue<string>("ExpirationDate")) && dataReader.GetColumnValue<int>("LicenseType") == 1 ? "Perpetual" : string.Empty
                            };

                            panoramaMigrationDetailList.Add(device);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return panoramaMigrationDetailList;
        }

        public bool TransferDeploymentDeploymentProfile(long? deploymentProfileId, long tokenId, double tokensAllocated, long requestedBy)
        {
            try
            {
                using (var command = Connection.GetStoredProcCommand("TransferDeploymentProfile"))
                {
                    Connection.AddInParameter(command, "@p_DeploymentProfileId", DbType.Int64, deploymentProfileId);
                    Connection.AddInParameter(command, "@p_TokenId", DbType.Int64, tokenId);
                    Connection.AddInParameter(command, "@p_TokensAllocated", DbType.Double, tokensAllocated);
                    Connection.AddInParameter(command, "@p_UpdatedBy", DbType.Int64, requestedBy);
                    Connection.ExecuteNonQuery(command);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return false;
            }
            return true;
        }

        public IEnumerable<DeploymentProfileForHub> GetDeploymentProfiles(long supportAccountId)
        {
            string sqlQuery = @"select d.DeploymentProfileId,
                                d.Name ProfileName,
		                        l.AuthorizationCode AuthCode, 
		                        GROUP_CONCAT(DISTINCT Concat(f.LicIdentifier, ':', f.AppId,':',COALESCE(packageversion,'0'), ':',COALESCE(dpshs.HubSetupStatus,'0'))) AS Subscriptions,
		                        dt.Name Type,
                                group_concat(distinct COALESCE(d1.serialnumber,'')) SerialNumbers
                              from csp.deploymentprofile d 
		                        join csp.license l on d.DpLicenseId = l.LicenseId
                                left join csp.license l1 on l1.VMParentLicenseId=d.DpLicenseId
                                left join csp.device d1 on d1.deviceid=l1.DeviceId
		                        join csp.deploymentprofiletype dt on d.DeploymentProfileTypeId = dt.DeploymentProfileTypeId
		                        join csp.deploymentprofilesubscriptions dps on d.DeploymentProfileId = dps.DeploymentProfileId 
                                join csp.flexsubscriptions f ON f.FlexSubscriptionId = dps.FlexSubscriptionId 
							    left join DeploymentProfileSubscriptionHubStatus dpshs on dpshs.FlexSubscriptionId = f.FlexSubscriptionId and dpshs.DeploymentProfileId = d.DeploymentProfileId
                                join csp.deploymentprofilestatus ds on ds.DeploymentProfileStatusId=d.Status
                                and f.LicIdentifier in ('ENIOTDRDL','ENIOTPDRDL','INDIOTDRDL','MDIOTDRDL','ENIOTPRDL','INDIOTRDL','MDIOTRDL','AIOPS','SAASINLINE')
                            where d.IsActive=1 and d.SupportAccountId=@SupportAccountId group  by d.DeploymentProfileId";


            var deploymentProfiles = new List<DeploymentProfileForHub>();
            try
            {

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@SupportAccountId", DbType.UInt64, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var deploymentProfile = new DeploymentProfileForHub()
                            {
                                DeploymentProfileId = dataReader.GetColumnValue<long>("DeploymentProfileId"),
                                ProfileName = dataReader.GetColumnValue<string>("ProfileName"),
                                AuthCode = dataReader.GetColumnValue<string>("AuthCode"),
                                SerialNumber= !string.IsNullOrEmpty(dataReader.GetColumnValue<string>("SerialNumbers")) ? dataReader.GetColumnValue<string>("SerialNumbers").Split(',').ToList() : new List<string>(),
                                Type = dataReader.GetColumnValue<string>("Type"),
                                Subscriptions = GetHubSubscriptions(dataReader.GetColumnValue<string>("Subscriptions"))
                            };

                            deploymentProfiles.Add(deploymentProfile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return deploymentProfiles;
        }


        public bool IsConsumptionReported(string serailNumber, string authCode, string retryUrlService)
        {
           
            try
            {
                const string sqlQuery = @"SELECT 1 FROM csp.genericrestcalllog  s                                                 
                                          WHERE s.retryUrlService = @p_RetryUrlService AND s.serialNumber = @p_SerialNumber AND s.authcode = @p_AuthCode AND  s.success = 1 and url regexp 'CONSUME$' order by createddate desc  LIMIT 1";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_SerialNumber", DbType.String, serailNumber);
                    Connection.AddInParameter(command, "@p_AuthCode", DbType.String, authCode);
                    Connection.AddInParameter(command, "@p_RetryUrlService", DbType.String, retryUrlService);
                    var objResult =Connection.ExecuteScalar(command);

                    int result = 0;
                    if (objResult != null && int.TryParse(objResult.ToString(), out result) && result != 0)
                    {
                        return true;
                    }

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return false;
        }

        public string GetAuthCodeFromDeploymentProfileId(long id, long supportAccountId)
        {
            const string sqlQuery = @"SELECT l.AuthorizationCode FROM csp.deploymentprofile d
                                    JOIN csp.license l ON d.DpLicenseId = l.LicenseId                                    
                                    WHERE d.SupportAccountId = @SupportAccountId
                                    AND d.DeploymentProfileId =@Id";
            string authCode = string.Empty;
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@SupportAccountId", DbType.Int64, supportAccountId);
                    Connection.AddInParameter(command, "@Id", DbType.String, id);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            authCode = dataReader.GetColumnValue<string>("AuthorizationCode");

                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return authCode;
        }

        public string FlattenSubType(string type, string subType)
        {
            const string sqlQuery = @"
SELECT s.FlattenedName
    FROM csp.DeploymentProfileType t
    JOIN csp.DeploymentProfileSubtype s ON t.DeploymentProfileTypeId = s.DeploymentProfileTypeId AND s.Name = @p_subType
    WHERE t.Name = @p_type
;";
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_type", DbType.String, type);
                    Connection.AddInParameter(command, "@p_subType", DbType.String, subType);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            return dataReader.GetColumnValue<string>("FlattenedName");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
            return null;
        }
        public int GetPubSubMessageConfig()
        {
            int timeOutValue = 0;
            const string sqlQuery = @"SELECT timeoutvalue FROM csp.pubsubmessageconfig LIMIT 1;";
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {

                    timeOutValue = Convert.ToInt32(Connection.ExecuteScalar(command));
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return timeOutValue;
        }
        #region private method
        private bool LogDeploymentProfileHistory(long profileId, long userAccountId, string reason)
        {
            long logId = 0;

            try
            {
                using (var command = Connection.GetStoredProcCommand("LogDeploymentProfile"))
                {
                    Connection.AddInParameter(command, "@p_DeploymentProfileId", DbType.Int64, profileId);
                    Connection.AddInParameter(command, "@p_LogCreatedBy", DbType.Int64, userAccountId);
                    Connection.AddInParameter(command, "@p_Reason", DbType.String, reason);
                    Connection.AddOutParameter(command, "@p_output", DbType.Int64, Int32.MaxValue);
                    Connection.ExecuteNonQuery(command);
                    if (command.Parameters["@p_output"].Value != DBNull.Value)
                    {
                        logId = Convert.ToInt64(command.Parameters["@p_output"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return logId > 0 ? true : false;
        }

        private List<string> GetFlexUseCaseSubscription(int deploymentProfileId)
        {
            string sqlQuery = @"SELECT f.LicIdentifier, f.DisplayName FROM csp.deploymentprofilesubscriptions d  
                                JOIN csp.flexsubscriptions f ON d.FlexSubscriptionId=f.FlexSubscriptionId
                                WHERE d.DeploymentProfileId=@DeploymentProfileId";

            List<string> flexSubscriptionList = null;
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@DeploymentProfileId", DbType.Int32, deploymentProfileId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        flexSubscriptionList = new List<string>();
                        while (dataReader.Read())
                        {
                            flexSubscriptionList.Add(dataReader.GetColumnValue<string>("LicIdentifier"));
                        }
                        return flexSubscriptionList;
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return flexSubscriptionList;
        }

        public List<string> GetFlexFeatures(int deploymentProfileId)
        {
            string sqlQuery = @"SELECT LicIdentifier FROM csp.deploymentprofilesubscriptions d  
                                JOIN csp.flexsubscriptions f ON d.FlexSubscriptionId=f.FlexSubscriptionId AND f.IsFeature=1
                                WHERE d.DeploymentProfileId=@DeploymentProfileId";

            List<string> flexFeatureList = null;
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@DeploymentProfileId", DbType.Int32, deploymentProfileId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        flexFeatureList = new List<string>();
                        while (dataReader.Read())
                        {
                            flexFeatureList.Add(dataReader.GetColumnValue<string>("LicIdentifier"));
                        }
                        return flexFeatureList;
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return flexFeatureList;
        }
        public string GetDeploymentProfileStatus(string authCode)
        {
            const string sqlQuery = @"SELECT dl.Name  FROM deploymentprofile d 
                                  JOIN  csp.license l ON D.DpLicenseId=l.LicenseId 
                                  JOIN csp.deploymentprofilestatus dl ON d.Status=dl.DeploymentProfileStatusId
                                  WHERE  l.AuthorizationCode=@p_AuthorizationCode";
            string deploymentProfileStatus = string.Empty;
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_AuthorizationCode", DbType.String, authCode);
                    var status = Connection.ExecuteScalar(command);
                    deploymentProfileStatus = Convert.ToString(status);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return deploymentProfileStatus;
        }
        private List<int> GetUseCaseIds(int deploymentProfileId)
        {
            string sqlQuery = @"SELECT d.UseCaseId FROM csp.deploymentprofileusecase d WHERE d.DeploymentProfileId=@p_deploymentProfileId";

            List<int> useCaseIdList = null;
            try
            {
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_deploymentProfileId", DbType.Int32, deploymentProfileId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        useCaseIdList = new List<int>();
                        while (dataReader.Read())
                        {
                            useCaseIdList.Add(dataReader.GetColumnValue<int>("UseCaseId"));
                        }
                        return useCaseIdList;
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return useCaseIdList;
        }

        public DeploymentProfileList GetRenewalDeploymentProfiles(long supportAccountId, long tokenId)
        {
            string sqlQuery = @"SELECT 
                                    d.DeploymentProfileId, 
                                    d.Name,
                                    dpt.Name Type,
                                    CASE WHEN dpt.Name='CN' THEN lf.vmquantity - lf.use_count ELSE  COUNT(childLic.LicenseId) END AS FirewallsCreated,
                                    d.FirewallQuantity firewallsProjected,
                                    IF(dps.Name LIKE 'Pending%', '', l.AuthorizationCode) AuthorizationCode,
                                    dps.Name Status,
                                    d.PanOS,
                                    d.VMModel,
                                    d.Panorama,
                                    d.vCPUQuantity,
                                    dp.vcpuusecount VcpuAvailable, 
                                    CASE WHEN dp.islegacy=1 THEN (lf.vmquantity - lf.use_count)*d.vCPUQuantity ELSE dp.vcpuquantity - dp.vcpuusecount END VcpuConsumed,  
                                    CASE WHEN dp.islegacy=1 THEN d.FirewallQuantity*d.vCPUQuantity ELSE dp.vcpuquantity  END VcpuAllocated,
                                    d.QuantityNode,
                                    c.cnxtokens,
                                    HasPanorama(l.AuthorizationCode) HasPanorama,
                                    lf.expiration
                            FROM csp.deploymentprofile d 
                                JOIN csp.deploymentprofiletype dpt ON d.DeploymentProfileTypeId=dpt.DeploymentProfileTypeId
                                JOIN csp.license l ON l.LicenseId=D.DpLicenseId
                                JOIN csp.deploymentprofilestatus dps ON dps.DeploymentProfileStatusId = d.Status
                                JOIN csp.license dpLic  ON dpLic.AuthorizationCode=l.AuthorizationCode
                                LEFT JOIN pandb.dpinfo dp ON dp.dplicenseid = d.DpLicenseId AND dp.isgraceallowed IS NOT NULL AND dp.isgraceallowed = 1 
                                LEFT JOIN pandb.cnxinfo c ON c.cnxlicenseid = d.DpLicenseId
                                LEFT JOIN pandb.lic_features lf ON lf.lfid = dp.dplicenseid AND lf.auth_code=l.AuthorizationCode
                                LEFT JOIN csp.license childLic ON dpLic.LicenseId = childLic.VMParentLicenseId 
                                     AND childLic.IsActive AND childLic.ExpirationDate > NOW()
                                WHERE  l.ExpirationDate > NOW() AND (l.IsActive <> 0 OR dps.Name='PendingDelete') AND 
                                d.TokenId=@TokenId AND d.SupportAccountId =@SupportAccountId
                                GROUP BY d.DeploymentProfileId;SELECT f.LicIdentifier,f.DisplayName FROM csp.flexsubscriptions f;";


            var deploymentProfileList = new DeploymentProfileList();
            try
            {
                var deploymentProfiles = new List<DeploymentProfile>();
                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@TokenId", DbType.UInt64, tokenId);
                    Connection.AddInParameter(command, "@SupportAccountId", DbType.UInt64, supportAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            var deploymentProfile = new DeploymentProfile()
                            {
                                Id = dataReader.GetColumnValue<int>("DeploymentProfileId"),
                                ProfileName = dataReader.GetColumnValue<string>("Name"),
                                Type = dataReader.GetColumnValue<string>("Type"),
                                Subscriptions = GetFlexUseCaseSubscription(dataReader.GetColumnValue<int>("DeploymentProfileId")).ToArray(),
                                Features = GetFlexFeatures(dataReader.GetColumnValue<int>("DeploymentProfileId")).ToArray(),
                                UseCaseId = GetUseCaseIds(dataReader.GetColumnValue<int>("DeploymentProfileId")),
                                FirewallsCreated = dataReader.GetColumnValue<int>("FirewallsCreated"),
                                FirewallsProjected = dataReader.GetColumnValue<int>("FirewallsProjected"),
                                FirewallQuantity = dataReader.GetColumnValue<int>("FirewallsProjected"),
                                AuthCode = dataReader.GetColumnValue<string>("AuthorizationCode"),
                                Status = dataReader.GetColumnValue<string>("Status"),
                                PanOS = dataReader.GetColumnValue<string>("PanOS"),
                                VmModel = dataReader.GetColumnValue<string>("vmModel"),
                                Panorama = !string.IsNullOrEmpty(dataReader.GetColumnValue<string>("Panorama")) ? dataReader.GetColumnValue<string>("Panorama").Split(',').ToList() : new List<string>(),
                                VcpuQuantity = dataReader.GetColumnValue<int>("vCPUQuantity"),
                                VcpuAvailable = dataReader.GetColumnValue<int>("VcpuAvailable"),
                                VcpuConsumed = dataReader.GetColumnValue<int>("VcpuConsumed"),
                                VcpuAllocated = dataReader.GetColumnValue<int>("VcpuAllocated"),
                                KubernatesNodes = dataReader.GetColumnValue<int>("QuantityNode"),
                                HasPanorama = dataReader.GetColumnValue<bool>("HasPanorama"),
                                ExpirationDate = dataReader.GetColumnValue<DateTime>("expiration")
                            };

                            if (dataReader.GetColumnValue<string>("Type") == DeploymentProfileType.CN.ToDescription() && ApiConstants.CNNonlegacyPanOs.Any(s => s.Contains(dataReader.GetColumnValue<string>("PanOS"))))
                            {
                                var tokens = dataReader.GetColumnValue<string>("cnxtokens");
                                if (!string.IsNullOrEmpty(tokens))
                                {
                                    var cnxTokens = JsonConvert.DeserializeObject<Dictionary<string, int>>(tokens);
                                    var availableNodeTokens = cnxTokens["node_tokens"];
                                    var availableCoreTokens = cnxTokens["core_tokens"];
                                    deploymentProfile.VcpuAllocated = deploymentProfile.VcpuQuantity;
                                    deploymentProfile.VcpuConsumed = deploymentProfile.VcpuAllocated - availableCoreTokens;
                                }
                            }

                            deploymentProfiles.Add(deploymentProfile);
                        }
                        var subscriptions = new Dictionary<string, string>();

                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                subscriptions.Add(dataReader.GetColumnValue<string>("LicIdentifier"), dataReader.GetColumnValue<string>("DisplayName"));
                            }
                        }
                        deploymentProfileList = new DeploymentProfileList
                        {
                            profiles = deploymentProfiles,
                            availableSubscriptions = subscriptions
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return deploymentProfileList;
        }

        private List<HubSubscription> GetHubSubscriptions(string strSubscription)
        {
            List<HubSubscription> subscriptions = new List<HubSubscription>();

            strSubscription.Split(',').ToList().ForEach(x =>
            {
                subscriptions.Add(new HubSubscription
                {
                    Subscription = x.Split(':')[0],
                    AppId = x.Split(':')[1],
                    PackageVersion =  int.Parse(x.Split(':')[2]),
                    Status = int.Parse(x.Split(':')[3])
                });
            });

            return subscriptions;
        }
        #endregion
    }
}