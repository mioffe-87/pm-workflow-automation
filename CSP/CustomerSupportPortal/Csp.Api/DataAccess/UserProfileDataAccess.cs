using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.Model.Enums;
using Csp.Api.HelperClasses;
using System.Web;

namespace Csp.Api.DataAccess
{
    public class UserProfileDataAccess : BaseDataAccess, IUserProfileDataAccess
    {
        public UserProfileModel GetUserData(string email)
        {
            var details = new UserProfileModel
            {
                Email = email
            };

            const string sqlQuery = @"SELECT UserAccountId, FirstName, LastName, DisplayName, Title, Address1, Address2, City, CountryId, Region, 
                                      PostalCode, PhoneNumber, IsOptInEmail, IsOptInSecurityEmail, IsOptInPanOsUpdateEmail, DefaultSupportAccountId, 
                                      IsMfaEnabled, MultiFactorAuthType, Comments FROM useraccount WHERE Email = @p_EmailAddresss";
            string connString = Secrets.CspDatabase;

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                var command = new MySqlCommand(sqlQuery, connection);

                command.Parameters.AddWithValue("p_EmailAddresss", email);

                using (var dataReader = command.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        details.Address1 = dataReader["Address1"] == null || dataReader["Address1"] is DBNull ? null : HttpUtility.HtmlEncode(dataReader["Address1"].ToString());
                        details.Address2 = dataReader["Address2"] == null || dataReader["Address2"] is DBNull ? null : HttpUtility.HtmlEncode(dataReader["Address2"].ToString());
                        details.City = dataReader["City"] == null || dataReader["City"] is DBNull ? null : HttpUtility.HtmlEncode(dataReader["City"].ToString());
                        details.Country = dataReader["CountryId"] == null || dataReader["CountryId"] is DBNull ? (long?)null : Convert.ToInt64(dataReader["CountryId"].ToString());
                        details.DisplayName = dataReader["DisplayName"] == null || dataReader["DisplayName"] is DBNull ? null : HttpUtility.HtmlEncode(dataReader["DisplayName"].ToString());
                        details.FirstName = dataReader["FirstName"] == null || dataReader["FirstName"] is DBNull ? null : HttpUtility.HtmlEncode(dataReader["FirstName"].ToString());
                        details.IsMfaEnabled = Convert.ToBoolean(dataReader["IsMfaEnabled"] ?? false);
                        details.LastName = dataReader["LastName"] == null || dataReader["LastName"] is DBNull ? null : HttpUtility.HtmlEncode(dataReader["LastName"].ToString());
                        details.MultiFactorAuthType = (MultiFactorAuthType)(dataReader["MultiFactorAuthType"] == null || dataReader["MultiFactorAuthType"] is DBNull ? 0 : dataReader["MultiFactorAuthType"]);
                        details.optContentEmails = dataReader["IsOptInEmail"] == null || dataReader["IsOptInEmail"] is DBNull ? (bool?)null : Convert.ToBoolean(dataReader["IsOptInEmail"].ToString());
                        details.optSecurityEmails = dataReader["IsOptInSecurityEmail"] == null || dataReader["IsOptInSecurityEmail"] is DBNull ? (bool?)null : Convert.ToBoolean(dataReader["IsOptInSecurityEmail"].ToString());
                        details.optSoftwareEmails = dataReader["IsOptInPanOsUpdateEmail"] == null || dataReader["IsOptInPanOsUpdateEmail"] is DBNull ? (bool?)null : Convert.ToBoolean(dataReader["IsOptInPanOsUpdateEmail"].ToString());
                        details.PhoneNumber = dataReader["PhoneNumber"] == null || dataReader["PhoneNumber"] is DBNull ? null : HttpUtility.HtmlEncode(dataReader["PhoneNumber"].ToString());
                        details.PostalCode = dataReader["PostalCode"] == null || dataReader["PostalCode"] is DBNull ? null : HttpUtility.HtmlEncode(dataReader["PostalCode"].ToString());
                        details.Region = dataReader["Region"] == null || dataReader["Region"] is DBNull ? null : HttpUtility.HtmlEncode(dataReader["Region"].ToString());
                        details.Title = dataReader["Title"] == null || dataReader["Title"] is DBNull ? null : HttpUtility.HtmlEncode(dataReader["Title"].ToString());
                        details.UserAccountId = dataReader["UserAccountId"] == null || dataReader["UserAccountId"] is DBNull ? (long?)null : Convert.ToInt64(dataReader["UserAccountId"].ToString());
                        details.Comments = dataReader["Comments"] == null || dataReader["Comments"] is DBNull ? null : HttpUtility.HtmlEncode(dataReader["Comments"].ToString());
                    }
                    else
                    {
                        details.Errormessage = HttpUtility.HtmlEncode("No active user account found");
                    }
                }
            }
            return details;
        }

        public List<countries> GetCountries(bool isPartialShipment)
        {
            var countries = new List<countries>();
            string sqlQuery = "SELECT CountryId, Code, Name, ZipcodeLength, ZipcodeValidation, IsZipcodeRequired FROM country c ORDER BY c.Name ASC";
            // Sudan country is removed for Partial Shipment //
            if (isPartialShipment)
            sqlQuery = "SELECT CountryId, Code, Name, ZipcodeLength, ZipcodeValidation, IsZipcodeRequired FROM country c Where c.Code !='SS' ORDER BY c.Name ASC";
            using (var mySqlConnection = new MySqlConnection(Secrets.CspDatabase))
            {
                mySqlConnection.Open();

                var mySqlCommand = new MySqlCommand(sqlQuery, mySqlConnection);

                var mySqlReader = mySqlCommand.ExecuteReader();

                while (mySqlReader.Read())
                {
                    var c = new countries();

                    c.countryId = mySqlReader["CountryId"] == null || mySqlReader["CountryId"] is DBNull ? 0 : Convert.ToInt32(mySqlReader["CountryId"].ToString());
                    c.countrycode = mySqlReader["Code"] == null || mySqlReader["Code"] is DBNull ? string.Empty : HttpUtility.HtmlEncode(mySqlReader["Code"].ToString());
                    c.countryname = mySqlReader["Name"] == null || mySqlReader["Name"] is DBNull ? string.Empty : HttpUtility.HtmlEncode(mySqlReader["Name"].ToString());
                    c.ZipcodeLength = mySqlReader["ZipcodeLength"] == null || mySqlReader["ZipcodeLength"] is DBNull ? 0 : Convert.ToInt32(mySqlReader["ZipcodeLength"].ToString());
                    c.ZipcodeValidation = mySqlReader["ZipcodeValidation"] == null || mySqlReader["ZipcodeValidation"] is DBNull ? string.Empty : HttpUtility.HtmlEncode(mySqlReader["ZipcodeValidation"].ToString()); c.ZipcodeValidation = mySqlReader["ZipcodeValidation"] == null || mySqlReader["ZipcodeValidation"] is DBNull ? string.Empty : HttpUtility.HtmlEncode(mySqlReader["ZipcodeValidation"].ToString());
                    c.IsZipcodeRequired = mySqlReader["IsZipcodeRequired"] == null || mySqlReader["IsZipcodeRequired"] is DBNull ? false : Convert.ToBoolean( mySqlReader["IsZipcodeRequired"]);
                    countries.Add(c);
                }
            }

            return countries;
        }

        public List<regions> GetRegions()
        {
            var regions = new List<regions>();
            string sqlQuery = "SELECT CountryId, RegionId, Code, Name FROM region c where c.CountryId IN (39,233) ORDER BY c.Name ASC";
            // These 3 US regions are not shown in Partial Shipment( Federated States Of Micronesia, Marshall Islands, Palau 
            // Labrador and  Newfoundland not shown in Partial Shipmemt (it got combined Newfoundland & Labr.)

            using (var mySqlConnection = new MySqlConnection(Secrets.CspDatabase))
            {
                mySqlConnection.Open();
                var mySqlCommand = new MySqlCommand(sqlQuery, mySqlConnection);

                var mySqlReader = mySqlCommand.ExecuteReader();
                while (mySqlReader.Read())
                {
                    var r = new regions();

                    r.countryId = mySqlReader["CountryId"] == null || mySqlReader["CountryId"] is DBNull ? 0 : Convert.ToInt32(mySqlReader["CountryId"].ToString());
                    r.regionId = mySqlReader["RegionId"] == null || mySqlReader["RegionId"] is DBNull ? 0 : Convert.ToInt32(mySqlReader["RegionId"].ToString());
                    r.regionCode = mySqlReader["Code"] == null || mySqlReader["Code"] is DBNull ? string.Empty : HttpUtility.HtmlEncode(mySqlReader["Code"].ToString());
                    r.regionName = mySqlReader["Name"] == null || mySqlReader["Name"] is DBNull ? string.Empty : HttpUtility.HtmlEncode(mySqlReader["Name"].ToString());
                    regions.Add(r);
                }
            }

            return regions;
        }

        public List<regions> GetRegions(int countryId)
        {
            var regions = new List<regions>();
            string sqlQuery = "SELECT CountryId, RegionId, Code, Name FROM region c ORDER BY c.Name ASC";
            // These 3 US regions are not shown in Partial Shipment( Federated States Of Micronesia, Marshall Islands, Palau 
            // Labrador and  Newfoundland not shown in Partial Shipmemt (it got combined Newfoundland & Labr.)
            if (countryId != 0)
                sqlQuery = "SELECT CountryId, RegionId, Code, Name FROM region c  where c.CountryId=@countryId AND c.Name NOT IN ('Federated States Of Micronesia', 'Marshall Islands', 'Palau', 'Labrador', 'Newfoundland') ORDER BY c.Name ASC";
            using (var mySqlConnection = new MySqlConnection(Secrets.CspDatabase))
            {
                mySqlConnection.Open();
                var mySqlCommand = new MySqlCommand(sqlQuery, mySqlConnection);
                if (countryId !=0)
                mySqlCommand.Parameters.AddWithValue("@countryId", countryId);
                var mySqlReader = mySqlCommand.ExecuteReader();
                while (mySqlReader.Read())
                {
                    var r = new regions();

                    r.countryId = mySqlReader["CountryId"] == null || mySqlReader["CountryId"] is DBNull ? 0 : Convert.ToInt32(mySqlReader["CountryId"].ToString());
                    r.regionId = mySqlReader["RegionId"] == null || mySqlReader["RegionId"] is DBNull ? 0 : Convert.ToInt32(mySqlReader["RegionId"].ToString());
                    r.regionCode = mySqlReader["Code"] == null || mySqlReader["Code"] is DBNull ? string.Empty : HttpUtility.HtmlEncode(mySqlReader["Code"].ToString());                    
                    r.regionName = mySqlReader["Name"] == null || mySqlReader["Name"] is DBNull ? string.Empty : HttpUtility.HtmlEncode(mySqlReader["Name"].ToString());
                    regions.Add(r);
                }
            }

            return regions;
        }

        public bool UpdateUserData(UserProfileModel userData)
        {
            string message = string.Empty;
            bool isSuccess = false;
            isSuccess = Updateuseraccount(userData, userData.UserAccountId);

            return isSuccess;
        }

        private bool Updateuseraccount(UserProfileModel user, long? updatebyuseraccountid)
        {
            try
            {
                const string sqlQuery = "UPDATE useraccount SET DisplayName = ?p_DisplayName, FirstName = ?p_FirstName, LastName = ?p_LastName, Title = ?p_Title, Address1 = ?p_Address1, Address2 = ?p_Address2," +
                                        " CountryId = ?p_CountryId, Region = ?p_Region, City = ?p_City, PhoneNumber = ?p_PhoneNumber, PostalCode = ?p_PostalCode, LastUpdateByUserAccountId = ?p_LastUpdateByUserAccountId, " +
                                        " Comments = ?p_Comments WHERE Email = ?p_Email";
                string connString = Secrets.CspDatabase;

                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlQuery, connection);

                    command.Parameters.AddWithValue("p_DisplayName", user.DisplayName);
                    command.Parameters.AddWithValue("p_FirstName", user.FirstName);
                    command.Parameters.AddWithValue("p_LastName", user.LastName);
                    command.Parameters.AddWithValue("p_Title", user.Title);
                    command.Parameters.AddWithValue("p_Address1", user.Address1);
                    command.Parameters.AddWithValue("p_Address2", user.Address2);
                    command.Parameters.AddWithValue("p_CountryId", user.Country);
                    command.Parameters.AddWithValue("p_Region", user.Region);
                    command.Parameters.AddWithValue("p_City", user.City);
                    command.Parameters.AddWithValue("p_PhoneNumber", user.PhoneNumber);
                    command.Parameters.AddWithValue("p_PostalCode", user.PostalCode);
                    command.Parameters.AddWithValue("p_LastUpdateByUserAccountId", updatebyuseraccountid);
                   // command.Parameters.AddWithValue("p_IsOptInEmail", user.optContentEmails);
                    //command.Parameters.AddWithValue("p_IsOptInSecurityEmail", user.optSecurityEmails);
                    //command.Parameters.AddWithValue("p_IsOptInPanOsUpdateEmail", user.optSoftwareEmails);
                    command.Parameters.AddWithValue("p_Email", user.Email);
                    command.Parameters.AddWithValue("p_Comments", user.Comments);

                    command.ExecuteNonQuery();
                }

                //const string panloginSqlQuery = "UPDATE accounts SET loginname = ?p_DisplayName, firstname = ?p_FirstName, lastname = ?p_LastName, address1 = ?p_Address1, address2 = ?p_Address2, city = ?p_City, phone = ?p_PhoneNumber, zipcode = ?p_PostalCode WHERE email = ?p_Email";
                //string panloginConnString = ConfigurationManager.ConnectionStrings["PanloginDatabase"].ToString();

                //using (var panloginConn = new MySqlConnection(panloginConnString))
                //{
                //    panloginConn.Open();
                //    var command = new MySqlCommand(panloginSqlQuery, panloginConn);

                //    command.Parameters.AddWithValue("p_DisplayName", user.DisplayName);
                //    command.Parameters.AddWithValue("p_FirstName", user.FirstName);
                //    command.Parameters.AddWithValue("p_LastName", user.LastName);
                //    command.Parameters.AddWithValue("p_Title", user.Title);
                //    command.Parameters.AddWithValue("p_Address1", user.Address1);
                //    command.Parameters.AddWithValue("p_Address2", user.Address2);
                //    command.Parameters.AddWithValue("p_City", user.City);
                //    command.Parameters.AddWithValue("p_PhoneNumber", user.PhoneNumber);
                //    command.Parameters.AddWithValue("p_PostalCode", user.PostalCode);
                //    command.Parameters.AddWithValue("p_Email", user.Email);

                //    command.ExecuteNonQuery();
                //}

                return true;
            }
            catch (Exception ex)
            {
								Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
	              return false;
            }
        }

        public bool IsDisplayNameExists(string displayName, string email)
        {
            bool isExists = false;
            email = email.ToLower();

            try
            {
                const string sqlQuery_CSP = "SELECT LOWER(email) AS email  FROM UserAccount WHERE LOWER(DisplayName) = LOWER(@p_DisplayName)";
                const string sqlQuery_Panlogin = "SELECT LOWER(email) AS email FROM accounts WHERE LOWER(loginName) = LOWER(@p_DisplayName)";

                string connString_CSP = Secrets.CspDatabase;
                string connString_Panlogin = ConfigurationManager.ConnectionStrings["PanloginDatabase"].ToString();

                //Check if displayName exists in CSP 
                using (var connection = new MySqlConnection(connString_CSP))
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlQuery_CSP, connection);

                    command.Parameters.AddWithValue("p_DisplayName", displayName);
                    command.Parameters.AddWithValue("p_Email", email);

                    using (var dataReader = command.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            if (Convert.ToString(dataReader["email"]) != email)
                            {
                                isExists = true;
                                return isExists;
                            }
                        }
                    }
                }

                //Check if displayName exists in PANLOGIN
                //using (var connection = new MySqlConnection(connString_Panlogin))
                //{
                //    connection.Open();
                //    var command = new MySqlCommand(sqlQuery_Panlogin, connection);

                //    command.Parameters.AddWithValue("p_DisplayName", displayName);
                //    command.Parameters.AddWithValue("p_Email", email);

                //    using (var dataReader = command.ExecuteReader())
                //    {
                //        if (dataReader.Read())
                //        {
                //            if (Convert.ToString(dataReader["email"]) != email)
                //            {
                //                isExists = true;
                //                return isExists;
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
	            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
	            return true;
            }
            return isExists;
        }

        public DateTime GetUserLastLoginDateTime(string email)
        {
            DateTime lastLoginDateTime = DateTime.MinValue;

            try
            {
                const string sqlQuery_Panlogin = "SELECT LastLoginAttempt FROM accounts WHERE LOWER(email) = LOWER(@p_Email)";

                string connString_Panlogin = ConfigurationManager.ConnectionStrings["PanloginDatabase"].ToString();

                using (var connection = new MySqlConnection(connString_Panlogin))
                {
                    connection.Open();
                    var command = new MySqlCommand(sqlQuery_Panlogin, connection);

                    command.Parameters.AddWithValue("p_Email", email);

                    using (var dataReader = command.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            DateTime.TryParse(Convert.ToString(dataReader["LastLoginAttempt"]), out lastLoginDateTime);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }

            return lastLoginDateTime;
        }

			  public List<ResourceLink> GetNavigationLinks(string email, bool isPublic, long supportaccountid) {
		    
				
				var navLinks = new List<ResourceLink>();
		    var subLinks = new List<ResourceSubLink>();
					try
					{
						string applicationURL = ConfigurationManager.AppSettings["ApplicationUrl"].ToString();

                       var dsResultset = new DataSet();
                       using (var mySqlConnection = new MySqlConnection(Secrets.CspDatabase))
                        {
                            var mySqlCommand = new MySqlCommand();

                            mySqlCommand.CommandType = CommandType.StoredProcedure;
                            mySqlCommand.CommandText = "GetNavigationLinks";
                            mySqlCommand.CommandTimeout = 120;
                            mySqlCommand.Parameters.AddWithValue("p_email", email);
                            mySqlCommand.Parameters.AddWithValue("p_ispublic", isPublic);
                            mySqlCommand.Parameters.AddWithValue("p_SupportAccountId", supportaccountid);

                            mySqlCommand.Connection = mySqlConnection;
                            mySqlConnection.Open();

                            var myDataAdapter = new MySqlDataAdapter(mySqlCommand);
                            myDataAdapter.Fill(dsResultset);
                        }

                if (dsResultset.Tables.Count > 0 && dsResultset.Tables[1].Rows.Count > 0)
						{
							foreach (DataRow dr in dsResultset.Tables[1].Rows)
							{
								var r = new ResourceSubLink()
								{

									Name = dr["SubLinkName"] == null || dr["SubLinkName"] is DBNull ? string.Empty : dr["SubLinkName"].ToString(),
									Url = dr["SubLinkUrl"] == null || dr["SubLinkUrl"] is DBNull ? string.Empty : dr["SubLinkUrl"].ToString(),
									ResourcelinkId = dr["NavigationLinkId"] == null || dr["NavigationLinkId"] is DBNull ? 0 : Convert.ToInt64(dr["NavigationLinkId"].ToString()),
									ResourceSublinkId = dr["NavigationSubLinkId"] == null || dr["NavigationSubLinkId"] is DBNull ? 0 : Convert.ToInt64(dr["NavigationSubLinkId"].ToString()),
									IsExternalLink = dr["IsExternalLink"] != null && !(dr["IsExternalLink"] is DBNull) && Convert.ToBoolean(dr["IsExternalLink"])
								};
								subLinks.Add(r);
							}
						}


						if (dsResultset.Tables.Count > 0 && dsResultset.Tables[0].Rows.Count > 0)
						{
							foreach (DataRow dr in dsResultset.Tables[0].Rows)
							{
								var r = new ResourceLink()
								{
									Name = dr["Name"] == null || dr["Name"] is DBNull ? string.Empty : dr["Name"].ToString(),
									Url = dr["Url"] == null || dr["Url"] is DBNull
										? string.Empty
										: string.Format(dr["Url"].ToString(), applicationURL, supportaccountid),
									ImageUrl = dr["Image"] == null || dr["Image"] is DBNull ? string.Empty : dr["Image"].ToString(),
									ResourceId = dr["Id"] == null || dr["Id"] is DBNull ? string.Empty : dr["Id"].ToString(),
									IsExternalLink = dr["IsExternalLink"] != null && !(dr["IsExternalLink"] is DBNull) && Convert.ToBoolean(dr["IsExternalLink"]),
									ResourceSubLinks = subLinks.Where(x => x.ResourcelinkId.Equals(Convert.ToInt64(dr["Id"]))).Select(sublink =>
										new ResourceSubLink()
										{
											Name = sublink.Name,
											Url = string.Format(sublink.Url, applicationURL, supportaccountid),
											IsExternalLink = sublink.IsExternalLink
										}).ToList()
								};
								navLinks.Add(r);

							}
						}

						return navLinks;
					}
					catch (Exception ex)
					{
						Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
						return new List<ResourceLink>();
					}
				}

              public List<QuickActionLinkItem> GetQuickActionLinks(string email, string supportaccountid)
                {

                  var newQuickActionLinks = new List<QuickActionLinkItem>();                   
                    using (var command = Connection.GetStoredProcCommand("GetQuickActionLinks"))
                    {
                        Connection.AddInParameter(command, "@p_supportAccountId", DbType.Int64, supportaccountid);
                        Connection.AddInParameter(command, "@p_email", DbType.String, email);
                        
                        using (var dataReader = Connection.ExecuteReader(command))
                        {

                            while (dataReader.Read())
                            {
                                var newLink = new QuickActionLinkItem()
                                {
                                    Name = Convert.ToString(dataReader["name"]),
                                    Url = string.Format(Convert.ToString(dataReader["url"]), string.Empty, supportaccountid),                                    
                                    OrderBy = Convert.ToString(dataReader["OrderBy"]),
                                    IconClass = Convert.ToString(dataReader["Image"]),
                                    IsExternalLink = Convert.ToBoolean(dataReader["IsExternalLink"]),
                                    LinkText = Convert.ToString(dataReader["linktext"]),
                                    IsCaseCreation = Convert.ToBoolean(dataReader["IsCaseCreation"]),
                                };

                                newQuickActionLinks.Add(newLink);
                            }
                        }

                    }
                    
                    return newQuickActionLinks?.OrderBy(l=>l.OrderBy).ToList();
                   
                }


        /// <summary>
        /// Checks if the user has ASC role based on SFDC Portal Username
        /// </summary>
        /// <param name="portalUsername"></param>
        /// <returns></returns>
        public bool IsAscUserBySfdcLoginId(string portalUsername,long userAccountId = 0)
        {
            bool isASCUser = false;

            using (var command = Connection.GetStoredProcCommand("IsASCUserBySfdcLoginId"))
            {
                Connection.AddInParameter(command, "@p_SfdcLoginId", DbType.String, portalUsername);
                Connection.AddInParameter(command, "@p_UserAccountId", DbType.String, userAccountId);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        bool.TryParse(dataReader["IsAscUser"].ToString(), out isASCUser);
                    }
                }
            }

            return isASCUser;
        }

        public string GetSalesforceLoginId(long supportAccountId, long userAccountId)
        {

            var result = string.Empty;
            const string sql = "SELECT m.SalesforceLogInId FROM membership m WHERE m.UserAccountId=@p_UserAccountId AND m.SupportAccountId=@p_SupportAccountId;";

            using (var command = Connection.GetSqlStringCommand(sql))
            {

                Connection.AddInParameter(command, "p_UserAccountId", DbType.String, userAccountId);
                Connection.AddInParameter(command, "p_SupportAccountId", DbType.String, supportAccountId);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    if (dataReader.Read())
                    {
                        result = dataReader["SalesforceLogInId"] == null || dataReader["SalesforceLogInId"] is DBNull ? null : dataReader["SalesforceLogInId"].ToString();

                    }
                }
            }

            return result;
        }


        public bool EnableDayOneConfig(long supportAccountId)
        {
            var enableConfig = false;

            try
            {
                const string sqlQuery = "SELECT COUNT(b.BetaAccessId) 'accounts' FROM csp.betaaccess b WHERE b.FeatureName='DAYONECONFIG' AND b.SupportAccountId=@p_SupportAccountId";

               

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command,"p_SupportAccountId", DbType.Int64,supportAccountId);

                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            enableConfig = Convert.ToInt32(dataReader["accounts"]) > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return enableConfig;
        }

        public UserConfig IsFirstLogin(string email)
        {
            using (var command = Connection.GetStoredProcCommand("IsFirstLogin"))
            {
                Connection.AddInParameter(command, "@p_email", DbType.String, email);

                using (var dataReader = Connection.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        return new UserConfig()
                        {
                            IsFirstLogin = Convert.ToInt32(dataReader["IsFirstLogin"]) > 0,
                            IsDeviceCodeRegistered = Convert.ToInt32(dataReader["IsDeviceRegistered"]) > 0
                        }; 
                    }
                }
            }
            return new UserConfig();
        }
         /// <summary>
        /// Checks if the user is part of atleast one fedramp account
        /// </summary>
        /// <returns></returns>
        public bool IsFedrampUser(string userEmail)
        {
            var isFedrampUser = false;

            try
            {
                const string sqlQuery = @"SELECT  COUNT(*) 'accounts'
                                            FROM csp.useraccount u
                                              JOIN csp.membership m
                                                ON m.UserAccountId = u.UserAccountId
                                              JOIN supportaccount s
                                                ON m.SupportAccountId = s.SupportAccountId AND s.IsFedRamp = 1
                                            WHERE u.Email =@p_Email "; ;               

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_Email", DbType.String, userEmail);                  
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            isFedrampUser = Convert.ToInt32(dataReader["accounts"]) > 0;
                        }
                    }
                }               

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return isFedrampUser;
        }



        /// <summary>
        /// Checks if the user is part of atleast one fedramp account
        /// </summary>
        /// <returns></returns>
        public bool IsUserAcknowledge(string userEmail)
        {
            var IsUserAcknowledge = false;

            try
            {
                const string sqlQuery = @"SELECT  COUNT(*) 'acknowledge'
                                            FROM csp.cveacknowledge                                              
                                            WHERE Email =@p_Email ";

                using (var command = Connection.GetSqlStringCommand(sqlQuery))
                {
                    Connection.AddInParameter(command, "@p_Email", DbType.String, userEmail);
                    using (var dataReader = Connection.ExecuteReader(command))
                    {
                        if (dataReader.Read())
                        {
                            IsUserAcknowledge = Convert.ToInt32(dataReader["acknowledge"]) > 0;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return IsUserAcknowledge;
        }

        public bool UpdateUserAcknowledge(string userEmail, long? supportAccountId)
        {

            string connString = Secrets.CspDatabase;
            const string sqlQuery = "INSERT INTO csp.cveacknowledge (Email, SupportAccountId, CreatedOn) VALUES (@email, @supportAccountId, @createdOn)";

            if (!IsUserAcknowledge(userEmail))
            {
                using (var connection = new MySqlConnection(connString))
                {
                    try
                    {
                        connection.Open();

                        var command = new MySqlCommand(sqlQuery, connection);

                        command.Parameters.Add("@email", MySqlDbType.String).Value = userEmail;
                        if (supportAccountId != null)
                            command.Parameters.Add("@supportAccountId", MySqlDbType.Int64).Value = supportAccountId;
                        else
                            command.Parameters.Add("@supportAccountId", MySqlDbType.Int64).Value = DBNull.Value;

                        command.Parameters.Add("@createdOn", MySqlDbType.DateTime).Value = DateTime.Now;
                        command.ExecuteNonQuery();
                        return true;
                    }


                    catch (Exception ex)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        return false;
                    }
                }

            }
            else
            {
                return true;
            }
        }
    }
    
}
