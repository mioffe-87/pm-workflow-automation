using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Csp.Api.DataAccess;
using Csp.Api.Exceptions;
using Csp.Api.HelperClasses;
using Csp.Api.Model;
using Csp.Api.SalesforceWebApi;
using Csp.Api.Services.Contracts;
using System.Web.Services.Protocols;
using System.Text;
using System.Threading;

namespace Csp.Api.Logic.Salesforce {
	public class SalesforceLogic {

		protected ISalesforceLogsRepository baseSalesforceLogsRepository;

		private bool IsPartnerPortalUserExists(string contactId, SessionInfo sessionInfo) {
			var queryString =
				string.Format(
					"SELECT PortalRole, Id, Email, ProfileId, IsPortalEnabled FROM User WHERE usertype LIKE '%partner%' AND Isportalenabled = true AND contactid = '{0}'",
					contactId);

			QueryResult queryResult;
			try {
				queryResult = sessionInfo.Binding.query(queryString);
            } catch (SoapException soapEx) {
                baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, soapEx.Code.Name, string.Format("Method: IsPartnerPortalUserExists, Contact Id: {0}", contactId), soapEx);
                throw;
            } catch (Exception ex) {
                throw new RetryAbleException(ex);
            }

			return (queryResult.size > 0);
		}

		private bool IsCustomerPortalUserExists(string email, string contactId, SessionInfo sessionInfo, out string username) {

			var queryString =
                string.Format("SELECT Id, Username FROM User WHERE Email = '{0}' AND (usertype LIKE '%powercustomersuccess%' OR Isportalenabled = true OR usertype LIKE '%powerpartner%')  AND contactid = '{1}'", email.Replace("'", @"\'"), contactId);
			//string.Format("SELECT Id, Username FROM User WHERE Email = '{0}' AND Isportalenabled = true AND contactid = '{1}'", email, contactId);

			QueryResult queryResult;
			try {
				queryResult = sessionInfo.Binding.query(queryString);
            } catch (SoapException soapEx) {
                baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, soapEx.Code.Name, string.Format("Method: IsCustomerPortalUserExists, Email: {0}, Contact Id: {1}", email, contactId), soapEx);
                throw;
            } catch (Exception ex) {
                throw new RetryAbleException(ex);
            }

			username = (queryResult.size > 0) ? ((User)queryResult.records[0]).Username : string.Empty;

			return (queryResult.size > 0);
		}

		private bool IsSalesfoceAccountExist(string accountId, SessionInfo sessionInfo) {

			var queryString =
				string.Format(
					@"SELECT Id FROM account WHERE Id = '{0}'", accountId);

			QueryResult queryResult;
			try {
				queryResult = sessionInfo.Binding.query(queryString);
            } catch (SoapException soapEx) {
                baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, soapEx.Code.Name, string.Format("Method: IsSalesfoceAccountExist, Account Id: {0}", accountId), soapEx);
                throw;
            } catch (Exception ex) {
				throw new RetryAbleException(ex);
			}

			return (queryResult.size > 0);
		}

		private string GetContactIdByEmail(string email, string salesforceId, SessionInfo sessionInfo) {

			var doNotMergeAccount = GetConfigValue(AppSettings.SalesforceDoNotMergeAccountId);
			var defaultAccount = GetConfigValue(AppSettings.SalesforceDefaultAccountId);
			var contactId = string.Empty;
			var queryString =
				string.Format(
					@"SELECT Id FROM contact WHERE Email = '{0}' AND AccountId = '{1}' AND AccountId <> '{2}' AND AccountId <> '{3}'",
                    email.Replace("'", @"\'"),
					salesforceId, doNotMergeAccount, defaultAccount);

			QueryResult queryResult;
			try {
				queryResult = sessionInfo.Binding.query(queryString);
            } catch (SoapException soapEx) {
                baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, soapEx.Code.Name, string.Format("Method: GetContactIdByEmail, Email: {0}", email), soapEx);
                throw;
            } catch (Exception ex) {
				throw new RetryAbleException(ex);
			}

			if (queryResult.size > 0) {
				contactId = queryResult.records[0].Id;
			}

			if (!string.IsNullOrEmpty(contactId) && IsPartnerPortalUserExists(contactId, sessionInfo)) {
				contactId = string.Empty;
			}

			return contactId;
		}

		private string CreateCustomerPortalUser(User user, SessionInfo sessionInfo) {

			string userId;
			SaveResult[] results;
			try {
				results = sessionInfo.Binding.create(new sObject[] { user });
            } catch (SoapException soapEx) {
                baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, soapEx.Code.Name, string.Format("Method: CreateCustomerPortalUser Username: {0}", user.Username), soapEx);
                throw;
            } catch (Exception ex) {
                throw new RetryAbleException(ex);
            }

			if (results[0].success) {
				userId = results[0].id;
			} else {
                var cspEx =  new CspException(string.Format("Error: Creating CustomerPortal User {0}", results[0].errors[0].message));
                baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, SalesforceLogTypes.DatabaseError, string.Format("Method: CreateCustomerPortalUser Username: {0}", user.Username), cspEx);
			    throw cspEx;
			}

			return userId;
		}

        private void DeleteCustomerPortalUser(string email, long supportAccountId, SessionInfo sessionInfo)
        {
            var contactId = string.Empty;
            var queryString =
                 string.Format(
                     @"SELECT Id,Contact.Email FROM EntitlementContact WHERE Contact.Email='{0}' AND Entitlement.CSP_ID__c='{1}'",
                     email.Replace("'", @"\'"),
                     supportAccountId);

            QueryResult queryResult;
            try
            {
                queryResult = sessionInfo.Binding.query(queryString);
            }
            catch (SoapException soapEx)
            {
                baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, soapEx.Code.Name, string.Format("Method: DeleteCustomerPortalUser, Email: {0}", email), soapEx);
                throw;
            }
            catch (Exception ex)
            {
                throw new RetryAbleException(ex);
            }

            if (queryResult.size > 0)
            {
                string[] contactIds = new string[queryResult.size];
                for (int i = 0; i < queryResult.size ; i++)
                {
                    contactIds[i] = queryResult.records[i].Id;
                }
                DeleteResult[] deleteResults;
                try
                {
                    deleteResults = sessionInfo.Binding.delete(contactIds);
                    for (int i = 0; i < deleteResults.Length; i++)
                    {
                        DeleteResult deleteResult = deleteResults[i];
                        if (!deleteResult.success)
                        {
                            var cspEx = new CspException(string.Format("Error: Deleting CustomerPortal Email {0}", deleteResults[0].errors[0].message));
                            baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, SalesforceLogTypes.DatabaseError, string.Format("Method: DeleteCustomerPortalUser Email: {0}", email), cspEx);
                            throw cspEx;
                        }
                    }

                }
                catch (SoapException soapEx)
                {
                    baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, soapEx.Code.Name, string.Format("Method: DeleteCustomerPortalUser Email: {0}", email), soapEx);
                    throw;
                }
                catch (Exception ex)
                {
                    throw new RetryAbleException(ex);
                }

            }
        }
        protected SalesforceUserModel AddUpdateSalesforceContactRetryAble(UserAccount userAccount, long? supportAccountId, string salesforceId,
																					 long membershipId,
                                                                                     long createdByUserAccountId, bool isCsspDefaultAccount = false)
        {
            var salesforceUserModel = new SalesforceUserModel();
            RetryAbleHelper retryAbleHelper = null;
            try
            {
                retryAbleHelper = new RetryAbleHelper(baseSalesforceLogsRepository);
                var sessionInfo = retryAbleHelper.GetSession();

                salesforceUserModel = AddUpdateSalesforceContact(userAccount, supportAccountId, salesforceId, membershipId,
                    createdByUserAccountId, sessionInfo, isCsspDefaultAccount);

            }
            catch (Exception ex)
            {
                //AddUpdateSalesforceContact unhandled exceptions should not block registration so just log and move on
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return salesforceUserModel;

            // Comments By LK On 04/12/2019
            // Remove the logic to retry
            /*var salesforceUserModel = new SalesforceUserModel();

			try {
				var retryAbleHelper = new RetryAbleHelper(baseSalesforceLogsRepository);

				while (true) {

					try {

						var sessionInfo = retryAbleHelper.GetSession();

						salesforceUserModel = AddUpdateSalesforceContact(userAccount, supportAccountId, salesforceId, membershipId,
                                                                             createdByUserAccountId, sessionInfo, isCsspDefaultAccount);

					} catch (RetryAbleException ex) {
						if (retryAbleHelper.ShouldThrow(ex)) {
							throw;
						}
					}
				}
			} catch (Exception ex) {
				//AddUpdateSalesforceContact unhandled exceptions should not block registration so just log and move on
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
			}

            return salesforceUserModel;*/
        }

		protected void UpdateSalesforceContactsAccountRetryAble(UserAccount userAccount, long? supportAccountId, string currentSfdcId, string newSfdcId)
        {

            RetryAbleHelper retryAbleHelper = null;

            try
            {
                retryAbleHelper = new RetryAbleHelper(baseSalesforceLogsRepository);
                var sessionInfo = retryAbleHelper.GetSession();

                UpdateSalesforceContactsAccount(userAccount, supportAccountId, currentSfdcId, newSfdcId,
                    sessionInfo);

                return;

            }
            catch (Exception ex)
            {
                //AddUpdateSalesforceContact unhandled exceptions should not block registration so just log and move on
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            // Comments By LK On 04/12/2019
            // Remove the logic to retry
			/*try {
				var retryAbleHelper = new RetryAbleHelper(baseSalesforceLogsRepository);

				while (true) {

					try {

						var sessionInfo = retryAbleHelper.GetSession();

						UpdateSalesforceContactsAccount(userAccount, supportAccountId, currentSfdcId, newSfdcId,
																			  sessionInfo);

						return;

					} catch (RetryAbleException ex) {
						if (retryAbleHelper.ShouldThrow(ex)) {
							throw;
						}
					}
				}
			} catch (Exception ex) {
                //AddUpdateSalesforceContact unhandled exceptions should not block registration so just log and move on
				Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
			}*/
		}
        protected void DeleteSalesforceContactsAccountRetryAble(string email, long supportAccountId)
        {
            RetryAbleHelper retryAbleHelper = null;

            try
            {
                retryAbleHelper = new RetryAbleHelper(baseSalesforceLogsRepository);
                var sessionInfo = retryAbleHelper.GetSession();
                var t = new Thread(() => DeleteCustomerPortalUser(email, supportAccountId, sessionInfo));
                t.Start();
            }
            catch (Exception ex)
            {                
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

        }

        private SalesforceUserModel AddUpdateSalesforceContact(UserAccount userAccount, long? supportAccountId, string salesforceId, long membershipId,
																						long createdByUserAccountId, SessionInfo sessionInfo, bool isCsspDefaultAccount = false)
        {
            var salesforceUserModel = new SalesforceUserModel();
			var strSupportAccountId = supportAccountId.HasValue ? supportAccountId.ToString() : string.Empty;

			if (salesforceId != null && salesforceId.Length > 18) {
				salesforceId = salesforceId.Substring(0, 18);
			}

			var contact = new Contact {
				FirstName = userAccount.FirstName,
				LastName = userAccount.LastName,
				Email = userAccount.Email,
				Title = userAccount.Title,
				Phone = userAccount.PhoneNumber,
				Alt_Mailing_Address__c = (userAccount.Address != null)
																	 ? string.Format("{0} {1}", userAccount.Address.Address1,
																									 userAccount.Address.Address2)
																	 : string.Empty,
				Alt_Mailling_City__c = (userAccount.Address != null) ? userAccount.Address.City : string.Empty,
				Alt_Mailing_State__c =
					(userAccount.Address != null) ? userAccount.Address.Region : string.Empty,
				Alt_Mailing_Zip_Postal_Code__c =
					(userAccount.Address != null) ? userAccount.Address.PostalCode : string.Empty,
				Alt_Mailing_Country__c =
					(userAccount.Address != null && userAccount.Address.Country != null)
						? userAccount.Address.Country
						: string.Empty,
				AccountId = salesforceId,
				pandb_accountid__c = strSupportAccountId
			};

		    if (!string.IsNullOrEmpty(contact.FirstName) && contact.FirstName.Length > 40) {
		        contact.FirstName = contact.FirstName.Substring(0, 40);
		    }

		    if (!string.IsNullOrEmpty(contact.Alt_Mailling_City__c) && contact.Alt_Mailling_City__c.Length > 40) {
		        contact.Alt_Mailling_City__c = contact.Alt_Mailling_City__c.Substring(0, 40);
		    }

            if (!string.IsNullOrEmpty(contact.Alt_Mailing_Zip_Postal_Code__c) && contact.Alt_Mailing_Zip_Postal_Code__c.Length > 15) {
                contact.Alt_Mailing_Zip_Postal_Code__c = contact.Alt_Mailing_Zip_Postal_Code__c.Substring(0, 15);
            }

			// Add/Update Salesforce Contact
            var contactResult = AddUpdateContact(contact, sessionInfo, isCsspDefaultAccount);

			if (contactResult != null) {
				var membershipLogic = DependencyResolver.Current.GetService<IMembershipLogic>();
			    var membership = membershipLogic.GetMembership(membershipId, createdByUserAccountId);

				// Update Membership table with SalesforceContactId and SalesforceLoginId

				var membershipModel = new MembershipModel {
					MembershipId = membershipId,
					SalesforceContactId = contactResult[0],
					SalesforceLogInId = contactResult[1],
					JiveId = (membership != null && !String.IsNullOrEmpty(membership.JiveId)) ? membership.JiveId : BaseLogic.CalculateMd5Hash(string.Format("{0}{1}{2}", DateTime.Now.ToString(), userAccount.Email,
																														GetConfigValue(AppSettings.PaloAltoHomeAddress))),
					LastUpdateByUserAccountId = createdByUserAccountId,
				};
				var uada = new UserAccountDataAccess();
				uada.UpdateSalesforceInfoForMembership(membershipModel);
				//membershipLogic.UpdateMembershipSf(membershipModel);

                if (membershipModel != null)
                {
                    salesforceUserModel.SalesForceAccountId = salesforceId;
                    salesforceUserModel.SalesForceContactId = membershipModel.SalesforceContactId;
                    salesforceUserModel.SalesForceLoginId = membershipModel.SalesforceLogInId;
                }
            }

            return salesforceUserModel;
        }

		private bool UpdateSalesforceContactsAccount(UserAccount userAccount, long? supportAccountId, string currentSfdcId, string newSfdcId,
																						SessionInfo sessionInfo) {
			var strSupportAccountId = supportAccountId.HasValue ? supportAccountId.ToString() : string.Empty;


			if (newSfdcId != null && newSfdcId.Length > 18) {
				newSfdcId = newSfdcId.Substring(0, 18);
			}

			// Get the contact Id

			var contactId = GetContactIdByEmail(userAccount.Email, currentSfdcId, sessionInfo);

			if (contactId == null)
				return false;

			var contact = new Contact {
				Id = contactId,
                FirstName = userAccount.FirstName,
				LastName = userAccount.LastName,
				Email = userAccount.Email,
				Title = userAccount.Title,
				Phone = userAccount.PhoneNumber,
				Alt_Mailing_Address__c = (userAccount.Address != null)
																	 ? string.Format("{0} {1}", userAccount.Address.Address1,
																									 userAccount.Address.Address2)
																	 : string.Empty,
                Alt_Mailling_City__c = (userAccount.Address != null) ? userAccount.Address.City : string.Empty,
				Alt_Mailing_State__c =
					(userAccount.Address != null) ? userAccount.Address.Region : string.Empty,
				Alt_Mailing_Zip_Postal_Code__c =
                    (userAccount.Address != null) ? userAccount.Address.PostalCode : string.Empty,
				Alt_Mailing_Country__c =
					(userAccount.Address != null && userAccount.Address.Country != null)
						? userAccount.Address.Country
						: string.Empty,
				AccountId = newSfdcId,
				pandb_accountid__c = strSupportAccountId
			};

            if (!string.IsNullOrEmpty(contact.FirstName) && contact.FirstName.Length > 40) {
                contact.FirstName = contact.FirstName.Substring(0, 40);
            }

            if (!string.IsNullOrEmpty(contact.Alt_Mailling_City__c) && contact.Alt_Mailling_City__c.Length > 40) {
                contact.Alt_Mailling_City__c = contact.Alt_Mailling_City__c.Substring(0, 40);
            }

            if (!string.IsNullOrEmpty(contact.Alt_Mailing_Zip_Postal_Code__c) && contact.Alt_Mailing_Zip_Postal_Code__c.Length > 15) {
                contact.Alt_Mailing_Zip_Postal_Code__c = contact.Alt_Mailing_Zip_Postal_Code__c.Substring(0, 15);
            }

			// Add/Update Salesforce Contact
			return UpdateContactsAccount(contact, sessionInfo);
		}

		protected SalesforceEvalData GetSalesforceEvalDataByEvalIdRetryAble(string evalId) {

			var retryAbleHelper = new RetryAbleHelper(baseSalesforceLogsRepository);

			while (true) {

				try {

					var sessionInfo = retryAbleHelper.GetSession();

					return GetSalesforceEvalDataByEvalId(evalId, sessionInfo);

				} catch (RetryAbleException ex) {
					if (retryAbleHelper.ShouldThrow(ex)) {
						throw;
					}
				}
			}
		}

		private SalesforceEvalData GetSalesforceEvalDataByEvalId(string evalId, SessionInfo sessionInfo) {

			var client = new EvalWebService.EvalWebServicePortTypeClient();

			EvalWebService.Account result;
			try {
				client.getAccount(new EvalWebService.SessionHeader { sessionId = sessionInfo.SessionId },
													new EvalWebService.CallOptions(), new EvalWebService.DebuggingHeader(),
													new EvalWebService.AllowFieldTruncationHeader(), evalId, out result);
			} catch (Exception ex) {
				throw new RetryAbleException(ex);
			}

			return result == null ? null : new SalesforceEvalData {
				SfAccountId = result.Id,
				Name = result.Name,
				PhoneNumber = result.Phone,
				Addr1 = result.BillingStreet,
				City = result.BillingCity,
				Region = result.BillingState,
				PostalCode = result.BillingPostalCode,
				Country = result.BillingCountry,
			};
		}

		private bool UpdateContactsAccount(Contact contact, SessionInfo sessionInfo)
		{
			var result = new string[2];
			SaveResult[] results;

			try {
				results = sessionInfo.Binding.update(new sObject[] { contact });
			} catch (SoapException soapEx) {
                baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, soapEx.Code.Name, string.Format("Method: UpdateContactsAccount, Contact Id: {0}", contact.Id), soapEx);
                throw;
            } catch (Exception ex) {
				throw new RetryAbleException(ex);
			}

			if (!results[0].success) {
                var cspEx = new CspException(string.Format("Error: Updating Contact {0}", results[0].errors[0].message));
                baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, SalesforceLogTypes.DatabaseError, string.Format("Method: UpdateContactsAccount, Contact Id: {0}", contact.Id), cspEx);

			    throw cspEx;
			}
			
			return true;
		}

        private string[] AddUpdateContact(Contact contact, SessionInfo sessionInfo, bool isCsspDefaultAccount = false)
        {

			var result = new string[2];

			var defaultAccountId = GetConfigValue(AppSettings.SalesforceDefaultAccountId);
			var defaultAwsAccountId = GetConfigValue(AppSettings.SalesforceDefaultAWSAccountId);
            var defaultAzureAccountId = GetConfigValue(AppSettings.SalesforceDefaultAzureAccountId);
            var defaultGoogleAccountId = GetConfigValue(AppSettings.SalesforceDefaultGoogleCloudAccountId);

			if (string.IsNullOrEmpty(contact.AccountId)) {
				// Assign default Account to contact
				contact.AccountId = defaultAccountId;
			}

			if (!IsSalesfoceAccountExist(contact.AccountId, sessionInfo)) {
				// Assign default Account to contact
				contact.AccountId = defaultAccountId;
			}

			var contactId = (!string.IsNullOrEmpty(contact.Id))
												? contact.Id
												: GetContactIdByEmail(contact.Email, contact.AccountId, sessionInfo);

			if (string.IsNullOrEmpty(contactId)) {
				// Create Contact
				SaveResult[] results;
				try {
					results = sessionInfo.Binding.create(new sObject[] { contact });
				} catch (Exception ex) {
					throw new RetryAbleException(ex);
				}

				if (results[0].success) {
					contactId = results[0].id;
				} else {
                    var cspEx = new CspException(string.Format("Error: Creating Contact {0}", results[0].errors[0].message));
                    baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, SalesforceLogTypes.DatabaseError, string.Format("Method: AddUpdateContact Contact Email: {0}", contact.Email), cspEx);
				    throw cspEx;
				}
			} else {
				// Update Contact
				contact.Id = contactId;

				SaveResult[] results;
				try {
					results = sessionInfo.Binding.update(new sObject[] { contact });
				} catch (Exception ex) {
					throw new RetryAbleException(ex);
				}

				if (!results[0].success) {
                    var cspEx = new CspException(string.Format("Error: Updating Contact {0}", results[0].errors[0].message));
                    baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, SalesforceLogTypes.DatabaseError, string.Format("Method: AddUpdateContact Contact Id: {0}", contact.Id), cspEx);
                    throw cspEx;
				}
			}

			string customerPortalUsername = string.Empty;
			if (!string.IsNullOrEmpty(contactId) && !IsCustomerPortalUserExists(contact.Email, contactId, sessionInfo, out customerPortalUsername))
			{

				string portalUserName = string.Format("{0}{1}", GenerateUsername(contact.Email),
													GetConfigValue(AppSettings.SalesforceCustomerPortalDomain));

				try
				{
					if (ConfigurationManager.AppSettings["AppName"] != "CSP PROD")
					{
                        var salesforceInstance = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["SalesforceInstance"])
                            ? ConfigurationManager.AppSettings["SalesforceInstance"]
                            : string.Empty;

                        portalUserName = portalUserName + "." + salesforceInstance;
					}
				}
                catch (Exception ex) {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                }


                // Create CustomerPortal User
                var customerPortalUser = new User {
					LastName = contact.LastName,
					Email = contact.Email,
					Username = portalUserName,
					Alias = GenerateAlias(contact.Email),
					CommunityNickname = GenerateCommunityNickname(contact.Email),
					ProfileId =
                        (contact.AccountId == defaultAccountId || contact.AccountId == defaultAwsAccountId || contact.AccountId == defaultAzureAccountId
                        || contact.AccountId == defaultGoogleAccountId || isCsspDefaultAccount)
							? GetConfigValue(AppSettings.SalesforcePanPendingPortalId)
							: GetConfigValue(AppSettings.SalesforcePanCustomerPortalId),
					IsActive = true,
					EmailEncodingKey = GetConfigValue(AppSettings.SalesforceEmailEncodingKey),
					PAN_Internal_User__c = GetConfigValue(AppSettings.SalesforcePanInternalUser),
					Theatre__c = GetConfigValue(AppSettings.SalesforceTheatre),
					My_Sales_Level__c = GetConfigValue(AppSettings.SalesforceMySalesLevel),
					TimeZoneSidKey = GetConfigValue(AppSettings.SalesforceTimeZoneSidKey),
					LocaleSidKey = GetConfigValue(AppSettings.SalesforceLocaleSidKey),
					LanguageLocaleKey = GetConfigValue(AppSettings.SalesforceLanguageLocaleKey),
					ReceivesAdminInfoEmails = false,
					ReceivesInfoEmails = false,
					ContactId = contactId
				};

				try {
					var portalUserId = CreateCustomerPortalUser(customerPortalUser, sessionInfo);

				    if (!string.IsNullOrEmpty(portalUserId)) {
                        customerPortalUsername = customerPortalUser.Username;
				    }
				} catch (Exception ex) {
					Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
				}
			}

			result[0] = contactId;
			result[1] = customerPortalUsername;

			return result;
		}

		private string GetConfigValue(AppSettings key) {
			var configValues = new ConfigValues();

			return configValues.GetAppSetting(key);
		}

		// Create an alias for Customer Portal User
		private string GenerateAlias(string email) {
		    var username = email.Split(new[] { '@' })[0];
            var rnd = new Random();
            var randomUsername = new string(username.OrderBy(r => rnd.Next()).ToArray());

			var alias = (username.Length < 3) ? username : username.Substring(0, 3);

			alias += (randomUsername.Length < 2) ?  RandomString(2) : randomUsername.Substring(0, 2);
			alias += new Random().Next(0, 100);
            alias += RandomString(2);

		    if (alias.Length > 8) {
		        alias = alias.Substring(0, 8);
		    }

			return alias;
		}

		// Create a nickname for Customer Portal User	 
		private string GenerateCommunityNickname(string email) {
			var username = email.Split(new[] { '@' })[0];
            var rnd = new Random();
            var randomUsername = new string(username.OrderBy(r => rnd.Next()).ToArray());
			var communityNickname = username;

            communityNickname += (randomUsername.Length < 3) ? RandomString(3) : randomUsername.Substring(0, 3);
			communityNickname += new Random().Next(1, 100);
            communityNickname += RandomString(4); 

			return (communityNickname.Length > 40) ? communityNickname.Substring(0, 40) : communityNickname;
		}

		// Create a username for Customer Portal User	 
		private string GenerateUsername(string email) {
			var username = email.Split(new[] { '@' })[0];
            var rnd = new Random();
            var randomUsername = new string(username.OrderBy(r => rnd.Next()).ToArray());
            var name = username.Length > 20 ? username.Substring(0, 20) : username;

            name += (randomUsername.Length < 3) ? RandomString(3) : randomUsername.Substring(0, 3);
			name += new Random().Next(1, 100);
            name += (randomUsername.Length < 3) ? RandomString(5) : RandomString(4);

			return name;
		}

        private string RandomString(int size) {
            var builder = new StringBuilder();
            var random = new Random((int)DateTime.Now.Ticks);

            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString().ToLower();
        }

		protected AscContactInfo GetAscContactInfoRetryAble(string id) {

			var retryAbleHelper = new RetryAbleHelper(baseSalesforceLogsRepository);

			while (true) {

				try {

					var sessionInfo = retryAbleHelper.GetSession();

					return GetAscContactInfo(id, sessionInfo);

				} catch (RetryAbleException ex) {
					if (retryAbleHelper.ShouldThrow(ex)) {
						throw;
					}
				}
			}
		}

		private AscContactInfo GetAscContactInfo(string id, SessionInfo sessionInfo) {

			AscContactInfo contactInfo = null;
			Account account = null;
			var queryString =
				string.Format(
					@"SELECT Id, Name, ASC_Support_Email__c, ASC_Support_Number__c, ASC_Support_Site__c FROM account WHERE Id = '{0}'",
					id);

			QueryResult queryResult;
			try {
				queryResult = sessionInfo.Binding.query(queryString);
            } catch (SoapException soapEx) {
                baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, soapEx.Code.Name, string.Format("Method: GetAscContactInfo, Account Id: {0}", id), soapEx);
                throw;
            } catch (Exception ex) {
                throw new RetryAbleException(ex);
            }

			if (queryResult.size > 0) {
				account = (Account)queryResult.records[0];
			}

			if (account != null) {
				contactInfo = new AscContactInfo {
					Id = account.Id,
					Name = account.Name,
					Email = account.ASC_Support_Email__c,
					Phone = account.ASC_Support_Number__c,
					WebSite = account.ASC_Support_Site__c
				};
			}

			return contactInfo;
		}

        protected bool getSfAccountTypeByEmail(string email, SessionInfo sessionInfo)
        {
            bool flag = false;
            var queryString =
                        string.Format(
                            @"SELECT Account.Account_Designation__c FROM Contact where email ='{0}'", email.Replace("'", @"\'")); //Account.Account_Designation__c
            QueryResult queryResult;
            try
            {
                queryResult = sessionInfo.Binding.query(queryString);
            } catch (SoapException soapEx) {
                baseSalesforceLogsRepository.Log(sessionInfo.SessionId, sessionInfo.SalesforceUserName, string.Empty, soapEx.Code.Name, string.Format("Method: getSfAccountTypeByEmail, Email: {0}", email), soapEx);
                throw;
            } catch (Exception ex) {
                throw new RetryAbleException(ex);
            }

            var accountRecordType = string.Empty;
            if (queryResult.size > 0)
            {
                sObject[] records = queryResult.records;
                foreach (var rec in records)
                {
                    Contact contact = (Contact)rec;
                    accountRecordType = contact.Account.Account_Designation__c;

                    if (accountRecordType.Contains("Partner"))
                        flag = true;

                    if (flag == true)
                        break;
                }
            }
            return flag;
        }

        protected bool getSfAccountTypeByEmailRetryAble(string email)
        {

            var retryAbleHelper = new RetryAbleHelper(baseSalesforceLogsRepository);

            while (true)
            {

                try
                {
                    var sessionInfo = retryAbleHelper.GetSession();

                    return getSfAccountTypeByEmail(email, sessionInfo);
                }
                catch (RetryAbleException ex)
                {
                    if (retryAbleHelper.ShouldThrow(ex))
                    {
                        throw;
                    }
                }
            }
        }
	}
}