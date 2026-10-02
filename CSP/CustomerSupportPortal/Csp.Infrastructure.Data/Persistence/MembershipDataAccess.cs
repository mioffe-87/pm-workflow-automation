using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Transactions;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using MySql.Data.MySqlClient;
using Csp.Api.HelperClasses;

namespace Csp.Infrastructure.Data.Persistence {
	public class MembershipDataAccess : IMembershipDataAccess {

		public void UpdateMembershipSf(MembershipModel membershipModel) {

			using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
			using (var db = new cspEntities()) {

				var mem = db.memberships.Single(x => x.MembershipId == membershipModel.MembershipId);

				mem.SalesforceContactId = membershipModel.SalesforceContactId;
				mem.SalesforceLogInId = membershipModel.SalesforceLogInId;
				mem.JiveId = membershipModel.JiveId;

				mem.LastUpdate = DateTime.Now;
				mem.LastUpdateByUserAccountId = membershipModel.LastUpdateByUserAccountId;

				db.SaveChanges();

				trans.Complete();
			}
		}

		/// <summary>
		/// WARNING - This method is not secured by the restrictedByUserAccountId value and also
		/// will return inactive and expired members.
		/// 
		/// This method is a rewrite of the GetMembershipsBySupportAccountId stored proc and
		/// my current goal is a direct re-write.  Eventually we should research if this method
		/// can be updated to exclude inactive/expired members and also add security back in (what perms are needed to run this?)
		/// </summary>
		/// <param name="supportAccountId"></param>
		/// <param name="restrictedByUserAccountId"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public List<SupportAccountMembershipModel> ReadSupportAccountMembers(long supportAccountId, long restrictedByUserAccountId, DataParameters parameters) {

			{
				var db = SessionDbContext.CspObjectContext;
                var now = DateTime.Now;
                var securityRepository = new SecurityRepository();
                var hasActiveCanaryLicense = securityRepository.HasActiveCanaryLicense(supportAccountId);

				var filteredMemberships =
					from m in db.memberships
					where m.SupportAccountId == supportAccountId
					select new {
						m.UserAccountId,
                        m.SupportAccountId,
						m.MembershipId,
						m.ActivationDate,
						m.ExpirationDate,
						m.Description,
						m.membershiproles,
                        m.LastUpdateByUserAccountId,
                        m.CreatedByUserAccountId
					};

				var filteredUserAccounts =
					from usr in db.useraccounts
					select new {
						usr.UserAccountId,
						usr.FirstName,
						usr.LastName,
						usr.DisplayName,
						usr.Email,
                        usr.IsBlocked,
                        usr.IsMfaEnabled,
                        usr.MultiFactorAuthType
					};

                var members = from mem in filteredMemberships
                            join usr in filteredUserAccounts on mem.UserAccountId equals usr.UserAccountId
                            join afusers in db.autofocustrialusers.AsEnumerable() on 
                            new {UserAccountId = (long)mem.UserAccountId, SupportAccountId = (long)mem.SupportAccountId} equals new {afusers.UserAccountId,afusers.SupportAccountId} into af
                            from afuser in af.DefaultIfEmpty()

                              join usrs in filteredUserAccounts on mem.LastUpdateByUserAccountId equals usrs.UserAccountId into updateaudit
                              from usrs in updateaudit.DefaultIfEmpty()

                              join createdusrs in filteredUserAccounts on mem.CreatedByUserAccountId equals createdusrs.UserAccountId into createaudit
                              from createdusrs in createaudit.DefaultIfEmpty()
                   
                            where afuser == null || afuser.SupportAccountId == supportAccountId
                            select new SupportAccountMembershipModel
                            {
                                UserAccountId = usr.UserAccountId,
                                FirstName = usr.FirstName,
                                LastName = usr.LastName,
                                DisplayName = usr.DisplayName,
                                Email = usr.Email,
                                MembershipId = mem.MembershipId,
                                ActivationDate = mem.ActivationDate,
                                ExpirationDate = mem.ExpirationDate,
                                Description = mem.Description,
                                AutoFocusTrialUser = (afuser != null) && afuser.IsActive.Value && afuser.ExpirationDate >= now,
                                AutoFocusTrialExpirationDate = (afuser != null) ? afuser.ExpirationDate : (DateTime?)null,
                                IsMfaEnabled = usr.IsMfaEnabled,
                                EnableEmailMfa = usr.IsMfaEnabled && usr.MultiFactorAuthType == 1,
                                MultiFactorAuthType = (MultiFactorAuthType) (usr.MultiFactorAuthType ?? 0),
                                Roles = from mr in mem.membershiproles
                                        orderby mr.role.Name
                                        select new NameValue
                                        {
                                            Name = mr.role.Name,
                                            ValueLong = mr.RoleId,
                                            IsActive = mr.RoleId == (int)RoleType.ThreatResearcher ? hasActiveCanaryLicense
                                                      : mr.RoleId == (int)RoleType.AutoFocusTrial ? hasActiveCanaryLicense && (afuser != null) 
                                                                                                    && afuser.IsActive.Value && afuser.ExpirationDate > now
                                                      : true
                                        },
                                LastUpdatedBy = usrs.Email,
                                CreatedBy = createdusrs.Email
                            };

				if (!string.IsNullOrEmpty(parameters.SearchField) && !string.IsNullOrEmpty(parameters.SearchValue)) {
					switch (parameters.SearchField) {
						case "RoleNames":
						case "Roles":

							var filteredMembershipIds =
								from mr in db.membershiproles
								where mr.membership.SupportAccountId == supportAccountId
											&& mr.role.Name.Contains(parameters.SearchValue)
								select mr.MembershipId;

							members =
								from m in members
								join fmids in filteredMembershipIds on m.MembershipId equals fmids
								select m;

							break;

						case "FirstName":
							members = members.Where(m => m.FirstName.Contains(parameters.SearchValue));
							break;

						case "LastName":
							members = members.Where(m => m.LastName.Contains(parameters.SearchValue));
							break;

						case "DisplayName":
							members = members.Where(m => m.DisplayName.Contains(parameters.SearchValue));
							break;

						case "Email":
							members = members.Where(m => m.Email.Contains(parameters.SearchValue));
							break;

						case "ActivationDate":
							members = members.Where(m => m.ActivationDate == DateTime.Parse(parameters.SearchValue));
							break;

						case "ExpirationDate":
							members = members.Where(m => m.ExpirationDate == DateTime.Parse(parameters.SearchValue));
							break;

						case "Description":
							members = members.Where(m => m.Description.Contains(parameters.SearchValue));
							break;
					}
				}

				switch (parameters.SortField) {
					case "RoleNames":
					case "Roles":
						members = ((parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
																? members.ToArray().OrderByDescending(u => u.RoleNames)
																: members.ToArray().OrderBy(u => u.RoleNames)).AsQueryable();
						break;

					case "FirstName":
						members = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
																? members.OrderByDescending(u => u.FirstName)
																: members.OrderBy(u => u.FirstName);
						break;

					case "LastName":
						members = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
																? members.OrderByDescending(u => u.LastName)
																: members.OrderBy(u => u.LastName);
						break;

					case "DisplayName":
						members = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
																? members.OrderByDescending(u => u.DisplayName)
																: members.OrderBy(u => u.DisplayName);
						break;

					case "Email":
						members = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
																? members.OrderByDescending(u => u.Email)
																: members.OrderBy(u => u.Email);
						break;

					case "ActivationDate":
						members = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
																? members.OrderByDescending(u => u.ActivationDate)
																: members.OrderBy(u => u.ActivationDate);
						break;

					case "ExpirationDate":
						members = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
																? members.OrderByDescending(u => u.ExpirationDate)
																: members.OrderBy(u => u.ExpirationDate);
						break;

					case "Description":
						members = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
																? members.OrderByDescending(u => u.Description)
																: members.OrderBy(u => u.Description);
						break;

					default:
						members = members.OrderBy(x => x.LastName).ThenBy(x => x.FirstName);
						break;
				}

				parameters.TotalRecordCount = members.Count();
				if (parameters.SupportsPaging) {
					members = members.Skip((int)parameters.PaginationOffset).Take((int)parameters.PaginationCount);
				}

				return members.ToList();
			}
		}

        public int GetCountOfUsersByRole(long supportAccountId, int roleId)
	    {
	        int superUsersCount = 0;

	        try
	        {

	            const string getQuery = "SELECT COUNT(DISTINCT m.UserAccountId) As sCount FROM csp.membership m JOIN csp.membershiproles mr ON m.MembershipId=mr.MembershipId " +
                                        " WHERE (m.ExpirationDate IS NULL OR m.ExpirationDate >=CURDATE()) AND mr.RoleId=@p_RoleId AND m.SupportAccountId= @p_SupportAccountId";
                string connString = Secrets.CspDatabase;

	            using (var connection = new MySqlConnection(connString))
	            {
	                connection.Open();
	                var command = new MySqlCommand(getQuery, connection);

	                command.Parameters.AddWithValue("p_SupportAccountId", supportAccountId);
                    command.Parameters.AddWithValue("p_RoleId", roleId);

	                using (var dataReader = command.ExecuteReader())
	                {
	                    if (dataReader.Read())
	                    {
	                        superUsersCount = dataReader["sCount"] == null || dataReader["sCount"] is DBNull ? 0 : Convert.ToInt32(dataReader["sCount"].ToString());
	                    }

	                }
	            }
	            return superUsersCount;
	        }
	        catch (Exception ex)
	        {
	            Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
	            return superUsersCount;
	        }
	    }
	    
	}
}
