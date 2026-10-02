using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Enums;
using Csp.Api.Model.Wildfire;
using Csp.Api.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.DataAccess
{
    public class WildfireDataAccess : BaseDataAccess, IWildfireDataAccess
    {
        public WildfireAssetsResponse<WildfirePortalItem> GetPortals(WildfireApiRequest request)
        {

            if (!SortHelper.ValidateSorting(request.SortField, request.SortOrder))
            {
                request.SortField = SortFieldEnum.PortalName.ToDescription();
                request.SortOrder = SortOrderEnum.Ascending.ToDescription();
            }

            string query = @"
SELECT l.LookupId, l.LookupKey, l.LookupValue 
  FROM lookups l 
  WHERE l.LookupType = 'API_WF_EP'
  {0}
;";
            string sorting = string.Empty;
            if (!string.IsNullOrEmpty(request.SortField)
                && !string.IsNullOrEmpty(request.SortOrder))
            {
                sorting = "ORDER BY l.LookupKey " + request.SortOrder;
            }

            try
            {
                List<WildfirePortalItem> items = new List<WildfirePortalItem>();
                using (var command = Connection.GetSqlStringCommand(string.Format(query, sorting)))
                {
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            items.Add(new WildfirePortalItem
                            {
                                Id = dataReader.GetColumnValue<string>("LookupId"),
                                PortalName = dataReader.GetColumnValue<string>("LookupKey"),
                                KeyUrl = dataReader.GetColumnValue<string>("LookupValue")
                            });
                        }
                    }
                }

                return new WildfireAssetsResponse<WildfirePortalItem>
                {
                    Items = items.Skip((request.Page - 1) * request.PerPage).Take(request.PerPage).ToArray(),
                    Total = items.Count
                };
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        public IEnumerable<KeyValuePair<long, string>> GetSupportAccountIdsByUserId(long userAccountId, string salesforceAccountId)
        {
            string query = @"
SELECT m.SupportAccountId, CONCAT(m.SupportAccountId, ' - ', s.Name) Name
  FROM useraccount u
  JOIN membership m ON u.UserAccountId = m.UserAccountId
  JOIN supportaccount s ON m.SupportAccountId = s.SupportAccountId
  WHERE u.UserAccountId = @p_UserAccountId
  AND s.SalesforceAccountId = @p_salesforceAccountId
  ORDER BY s.Name ASC
;";
            try
            {
                List<KeyValuePair<long, string>> items = new List<KeyValuePair<long, string>>();
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@p_UserAccountId", DbType.Int64, userAccountId);
                    Connection.AddInParameter(command, "@p_salesforceAccountId", DbType.String, salesforceAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        while (dataReader.Read())
                        {
                            items.Add(new KeyValuePair<long, string>(dataReader.GetColumnValue<long>("SupportAccountId"), dataReader.GetColumnValue<string>("Name")));
                        }
                    }
                }

                return items;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        public WildfireActivationLicense GetProductTypeBasedDetails(WildfireApiActivationDetailsRequestData data)
        {
            string query = @"
SELECT a.SalesforceAccountId, l.SupportAccountId, pl.PartNumber Model, pl.Description, l.VMQuantity Quantity, sm.SupportType SupportLicenseType
    FROM license l
    JOIN easyonboardingitem e ON L.Licenseid = E.Licenseid
    JOIN easyonboarding o ON E.Easyonboardingid = O.Easyonboardingid
    JOIN assetactivation a ON o.assetactivationid = a.assetactivationid
    JOIN partmaster pl ON l.Partmasterid = pl.Partmasterid
    LEFT JOIN skusupportmapping sm ON pl.PartMasterId = sm.PartMasterId
    WHERE l.AuthorizationCode = @p_AuthCode;
;";
            try
            {
                List<KeyValuePair<long, string>> items = new List<KeyValuePair<long, string>>();
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@p_AuthCode", DbType.Int64, data.AuthCode);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            return new WildfireActivationLicense
                            {
                                SalesforceAccountId = dataReader.GetColumnValue<string>("SalesforceAccountId"),
                                SupportAccountId = dataReader.GetColumnValue<long?>("SupportAccountId"),
                                ImageUrl = data.ProductType == WildFireApiActivationProductType.FedrampSupportAWF ? "image/PAN-FedrampAWF.png" : string.Empty,
                                Model = dataReader.GetColumnValue<string>("Model"),
                                Description = dataReader.GetColumnValue<string>("Description"),
                                Quantity = dataReader.GetColumnValue<int>("Quantity"),
                                SupportLicenseType = dataReader.GetColumnValue<string>("SupportLicenseType")
                            };
                        }
                    }
                }

                throw new ApplicationException("WF API GetProductTypeBasedDetails Details> Product '" + data.ProductType + "', AuthCode '" + data.AuthCode + "' is not found.");
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }

        public string GetSfdcMappedAccountId(string commercialSfdcAccountId)
        {
            string query = @"
SELECT a.salesforceaccountid 
    FROM mappingservice.account a 
    WHERE a.aliassalesforceaccountid = @p_commercialSfdcAccountId;
;";
            try
            {
                List<KeyValuePair<long, string>> items = new List<KeyValuePair<long, string>>();
                using (var command = Connection.GetSqlStringCommand(query))
                {
                    Connection.AddInParameter(command, "@p_commercialSfdcAccountId", DbType.String, commercialSfdcAccountId);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            return dataReader.GetColumnValue<string>("salesforceaccountid");
                        }
                    }
                }

                throw new ApplicationException("WF API GetSfdcMappedAccountId Details> Commercial SFDC Account ID '" + commercialSfdcAccountId + "' is not found.");
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                throw;
            }
        }
    }
}
