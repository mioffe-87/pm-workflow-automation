using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Csp.Api.EntityClasses;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using MySql.Data.MySqlClient;
using NHibernate;
using NHibernate.Transform;
using PAN.Common.Core.Persistence;
using PAN.Common.Infrastructure.Data.Persistence;
using Membership = Csp.Api.EntityClasses.Membership;
using Role = Csp.Api.EntityClasses.Role;
using Csp.Api.Security;
using Csp.Api.HelperClasses;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Model.Enums;

namespace Csp.Infrastructure.Data.Persistence {
	public class UserAccountRepository : IUserAccountRepository {
		private readonly INHibernateUnitOfWork _unitOfWork;
		private readonly IRepository _repository;
	    private readonly IConnectionStrings _connectionStrings;

		public UserAccountRepository(INHibernateUnitOfWork unitOfWork, IRepository repository, IConnectionStrings connectionStrings) {
			_unitOfWork = unitOfWork;
			_repository = repository;
		    _connectionStrings = connectionStrings;
		}


		public IEnumerable<Useraccount> GetAdminUserAccountsBySupportAccountId(long supportAccountId) {
			var userAccounts =
				(from user in _repository.Query<Useraccount>()
				 join membership in _repository.Query<Membership>() on user equals membership.UserAccountId_Useraccount_
				 join membershipRole in _repository.Query<Membershiprole>() on membership equals
					 membershipRole.MembershipId_Membership_
				 join role in _repository.Query<Role>() on membershipRole.RoleId_Role_ equals role
				 where
					 role.SecurityClassification == (short)SecurityClassification.Administrative &&
					 (membership.ExpirationDate >= DateTime.Now || membership.ExpirationDate == null) &&
					 membership.ActivationDate <= DateTime.Now &&
					 membership.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
				 select user).Distinct();

			return userAccounts;
		}

		public IEnumerable<WildfireUserAccountItem> GetWildfireUsersBySupportAccountId(long supportAccountId,
																																							 DataParameters parameters) {
			var userAccounts =
				(from user in _repository.Query<Wildfireuseraccount>()
				 join device in _repository.Query<Wildfiredevice>() on user equals device.WildfireUserAccountId_Wildfireuseraccount_
				 where
					 device.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
				 select new WildfireUserAccountItem() {
					 WildfireUserAccountId = user.WildfireUserAccountId,
					 Name = string.Format("{0} {1}", user.FirstName, user.LastName),
					 Email = user.Email,
					 CreatedBy = user.CreatedByUser,
					 CreatedOn = user.CreatedOn
				 }).ToList().Distinct(new WildfireUserAccountItemComparator()).ToList();

			if (userAccounts.Any()) {

				if (!String.IsNullOrEmpty(parameters.SearchValue)) {
					switch (parameters.SearchField) {

						case "Name":
							userAccounts = userAccounts.Where(u => u.Name == parameters.SearchValue).ToList();
							break;
						case "Email":
							userAccounts = userAccounts.Where(u => u.Email == parameters.SearchValue).ToList();
							break;
						case "CreatedBy":
							userAccounts = userAccounts.Where(u => u.CreatedBy == parameters.SearchValue).ToList();
							break;
						case "CreatedOn":
							DateTime dtCreatedOn;
							DateTime.TryParse(parameters.SearchValue, out dtCreatedOn);
							userAccounts = userAccounts.Where(u => u.CreatedOn.Equals(dtCreatedOn)).ToList();
							break;
					}
				}

				if (!string.IsNullOrEmpty(parameters.SortField)) {

					switch (parameters.SortField) {

						case "Name":
							userAccounts = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
															 ? userAccounts.OrderByDescending(u => u.Name).ToList()
															 : userAccounts.OrderBy(u => u.Name).ToList();

							break;
						case "Email":
							userAccounts = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
															 ? userAccounts.OrderByDescending(u => u.Email).ToList()
															 : userAccounts.OrderBy(u => u.Email).ToList();
							break;
						case "CreatedBy":
							userAccounts = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
															 ? userAccounts.OrderByDescending(u => u.CreatedBy).ToList()
															 : userAccounts.OrderBy(u => u.CreatedBy).ToList();
							break;
						case "CreatedOn":
							userAccounts = (parameters.SortDirection == Api.Model.Enums.SortDirection.Descending)
															 ? userAccounts.OrderByDescending(u => u.CreatedOn).ToList()
															 : userAccounts.OrderBy(u => u.CreatedOn).ToList();
							break;
						default:
							userAccounts = userAccounts.OrderBy(u => u.CreatedOn).ToList();
							break;

					}
				}

				parameters.TotalRecordCount = userAccounts.Count;

				return parameters.PaginationCount == int.MaxValue ? userAccounts : userAccounts.Skip(Convert.ToInt32(parameters.PaginationOffset)).Take(parameters.PaginationCount).ToList();
			}

			return userAccounts.ToList();
		}

		public bool IsWildfireUserAccountExists(string email, long supportAccountId) {
			var userAccount =
				(from user in _repository.Query<Wildfireuseraccount>()
				 join device in _repository.Query<Wildfiredevice>() on user equals device.WildfireUserAccountId_Wildfireuseraccount_
				 where
					 device.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId &&
					 user.Email == email
				 select new {
					 Id = user.WildfireUserAccountId
				 }).ToList();

			return userAccount.Count > 0;
		}

		public long GetWildfireUserAccountId(string email) {
			long wildfireUserAccountId = 0;
			var userAccount =
			(from user in _repository.Query<Wildfireuseraccount>()
			 where
				 user.Email == email
			 select new {
				 Id = user.WildfireUserAccountId
			 }).ToList();

			if (userAccount.Count > 0) {
				wildfireUserAccountId = (long)(int)(userAccount[0].Id);
			}

			return wildfireUserAccountId;
		}



		public long AddUpdateWildfireUserAccount(AddUpdateWildfireUserAccountModel userAccount, long supportAccountId, List<long> deviceIds,  string createdByUser, List<long> changedDeviceIds = null, string clientIpAddress = "") {
            string serialNumbers = string.Empty;
            bool isAdd = false;
            using (var db = new cspEntities()) {

                if (userAccount.WildfireUserAccountId.HasValue) {
                    var acctToUpdate = db.wildfireuseraccounts.Single(x => x.WildfireUserAccountId == userAccount.WildfireUserAccountId.Value);

                    acctToUpdate.FirstName = userAccount.FirstName;
                    acctToUpdate.LastName = userAccount.LastName;
                    acctToUpdate.LastUpdate = DateTime.Now;
                    acctToUpdate.LastUpdateByUserAccountId = userAccount.LastUpdatedByUserAccountId;

                    db.SaveChanges();
                } else {
                    var newWildfireUserAccount = new wildfireuseraccount {
                        Email = userAccount.Email,
                        FirstName = userAccount.FirstName,
                        LastName = userAccount.LastName,
                        CreatedByUser = userAccount.CreatedByUserEmail,
                        CreatedOn = DateTime.Now,
                        LastUpdateByUserAccountId = userAccount.LastUpdatedByUserAccountId,
                        LastUpdate = DateTime.Now
                    };
                    db.wildfireuseraccounts.Add(newWildfireUserAccount);
                    db.SaveChanges();
                    userAccount.WildfireUserAccountId = newWildfireUserAccount.WildfireUserAccountId;
                }

                var wildfiredevicesToDelete =
                    from wfdev in db.wildfiredevices
                    where wfdev.WildfireUserAccountId == userAccount.WildfireUserAccountId
                                && wfdev.SupportAccountId == supportAccountId
                    select wfdev;

                serialNumbers = GetWildfireSerialNumbers(supportAccountId, userAccount.WildfireUserAccountId.Value);

                if (!string.IsNullOrEmpty(serialNumbers))
                {
                    WildfireDeviceLog(supportAccountId, userAccount.WildfireUserAccountId.Value, serialNumbers, ManageUserAction.Update.ToString(), string.Format(ApiConstants.WildfireDeviceUpdateLog, supportAccountId), 
                        createdByUser, clientIpAddress);
                    serialNumbers = string.Empty;
                }
                else
                {
                    isAdd = true;
                }
               

                foreach (var dev in wildfiredevicesToDelete) {
                    if (deviceIds.Contains(dev.DeviceId) || (changedDeviceIds != null && changedDeviceIds.Contains(dev.DeviceId)))
                    {
                        db.wildfiredevices.Remove(dev);
                    }
                }
                db.SaveChanges();

                foreach (var deviceId in deviceIds) {

                    db.wildfiredevices.Add(new wildfiredevice {
                        WildfireUserAccountId = userAccount.WildfireUserAccountId.Value,
                        SupportAccountId = supportAccountId,
                        DeviceId = deviceId,
                        CreatedOn = DateTime.Now,
                        CreatedByUser = createdByUser,
                    });
                    db.SaveChanges();
                }

                if (isAdd)
                {
                    serialNumbers = GetWildfireSerialNumbers(supportAccountId, userAccount.WildfireUserAccountId.Value);
                    WildfireDeviceLog(supportAccountId, userAccount.WildfireUserAccountId.Value, serialNumbers, ManageUserAction.Create.ToString(), string.Format(ApiConstants.WildfireDeviceAddLog, supportAccountId), createdByUser, clientIpAddress);
                }
            }

			return userAccount.WildfireUserAccountId.Value;
		}

		public void DeleteWildfireUserAccount(long wildfireUserAccountId, long supportAccountId, string clientIpAddress,
                string deletedByUserAccountId) {

            string serialNumbers = GetWildfireSerialNumbers(supportAccountId, wildfireUserAccountId);
            WildfireDeviceLog(supportAccountId, wildfireUserAccountId, serialNumbers, ManageUserAction.Delete.ToString(), string.Format(ApiConstants.WildfireDeviceDeleteLog, supportAccountId), deletedByUserAccountId, clientIpAddress);
            using (var db = new cspEntities()) {

				//Delete all devices for support account
				var wildfiredevicesToDelete =
					from wfdev in db.wildfiredevices
					where wfdev.WildfireUserAccountId == wildfireUserAccountId
								&& wfdev.SupportAccountId == supportAccountId
					select wfdev;

				foreach (var dev in wildfiredevicesToDelete) {
					db.wildfiredevices.Remove(dev);
				}
				db.SaveChanges();


				//Delete wildfire user if they have no other devices for other support accounts
				var userAccountToDelete = db.wildfireuseraccounts.Single(x => x.WildfireUserAccountId == wildfireUserAccountId);

				var hasDevices = userAccountToDelete.wildfiredevices.Any();

				if (!hasDevices) {
					db.wildfireuseraccounts.Remove(userAccountToDelete);
					db.SaveChanges();
				}
			}
		}

		public void SetDefaultAccount(long supportAccountId, long userAccountId) {

			using (var stateless = _unitOfWork.CurrentSession.SessionFactory.OpenSession()) {
				using (var tx = stateless.BeginTransaction()) {
					var userAccount = stateless.Get<Api.EntityClasses.Useraccount>(userAccountId);

					userAccount.DefaultSupportAccountId_Supportaccount_ =
						stateless.Load<Api.EntityClasses.Supportaccount>(supportAccountId);

					stateless.Save(userAccount);
					tx.Commit();
				}
			}
		}

		public void AssignDefaultAccount(long supportAccountId, long userAccountId) {

			using (var db = new DBs.CSP.cspEntities())
			{
				var usr = db.useraccounts.Single(x => x.UserAccountId == userAccountId);
				usr.DefaultSupportAccountId = supportAccountId;
				db.SaveChanges();
			}
		}

		public long? GetDefaultAccount(long userAccountId) {

			var userAccount = _repository.Get<Api.EntityClasses.Useraccount>(userAccountId);

			if (userAccount.DefaultSupportAccountId_Supportaccount_ == null) return null;
			else return userAccount.DefaultSupportAccountId_Supportaccount_.SupportAccountId;
		}

		public void SetJiveId(long userAccountId, string jiveId) {
			using (var db = new DBs.CSP.cspEntities()) {
				var usr = db.useraccounts.Single(x => x.UserAccountId == userAccountId);
				usr.JiveId = jiveId;
				db.SaveChanges();
			}
		}

		public void SetSelectedSalesfoceData(long userAccountId, SelectedSalesForceData data) {
			
			var db = SessionDbContext.CspObjectContext;
			
			{
				var usr = db.useraccounts.Single(x => x.UserAccountId == userAccountId);

				//Only log if the membership changed
				if (usr.SelectedMembershipId == data.MembershipId
					&& usr.SelectedSalesForceAccountId == data.SalesForceAccountId
					&& usr.SelectedSalesForceContactId == data.SalesForceContactId
					&& usr.SelectedSalesForceLoginId == data.SalesForceLoginId) return;

				usr.SelectedMembershipId = data.MembershipId;
				usr.SelectedSalesForceAccountId = data.SalesForceAccountId;
				usr.SelectedSalesForceContactId = data.SalesForceContactId;
				usr.SelectedSalesForceLoginId = data.SalesForceLoginId;
				db.SaveChanges();
			}
		}

		public IEnumerable<UserAccount> GetAwsSupportAccountUsers(long supportAccountId) {
			using (var db = new DBs.CSP.cspEntities())
			{
                var users = (from ua in db.useraccounts
					join m in db.memberships on ua.UserAccountId equals m.UserAccountId
                    join c in db.countries on ua.CountryId equals c.CountryId into tempCountry
                    from leftCountry in tempCountry.DefaultIfEmpty()

					where ua.IsBlocked == false && ua.IsActive == true && m.SupportAccountId == supportAccountId
					select new UserAccount()
					{
						Id = ua.UserAccountId,
						Email = ua.Email,
						Address = new Address()
						{
							Address1 = ua.Address1 ?? string.Empty,
							Address2 = ua.Address2 ?? string.Empty,
							City = ua.City ?? string.Empty,
							CountryId = ua.CountryId ?? 233,
                            Country = leftCountry.Name ?? string.Empty,
							Region = ua.Region ?? string.Empty,
							PostalCode = ua.PostalCode ?? string.Empty
						},
						PhoneNumber = ua.PhoneNumber ?? string.Empty,
						FirstName = ua.FirstName ?? string.Empty,
						LastName = ua.LastName ?? string.Empty,
						DisplayName = ua.DisplayName ?? string.Empty,
						Title = ua.Title ?? string.Empty
					}).ToArray();
				return users;
			}
		}


        public IEnumerable<UserAccount> GetSupportAccountUsers(long supportAccountId)
        {
            using (var db = new DBs.CSP.cspEntities())
            {
                var users = (from ua in db.useraccounts
                             join m in db.memberships on ua.UserAccountId equals m.UserAccountId
                             join c in db.countries on ua.CountryId equals c.CountryId into tempCountry
                             from leftCountry in tempCountry.DefaultIfEmpty()

                             where ua.IsBlocked == false && ua.IsActive == true && m.SupportAccountId == supportAccountId
                             select new UserAccount()
                             {
                                 Id = ua.UserAccountId,
                                 Email = ua.Email,
                                 Address = new Address()
                                 {
                                     Address1 = ua.Address1 ?? string.Empty,
                                     Address2 = ua.Address2 ?? string.Empty,
                                     City = ua.City ?? string.Empty,
                                     CountryId = ua.CountryId ?? 233,
                                     Country = leftCountry.Name ?? string.Empty,
                                     Region = ua.Region ?? string.Empty,
                                     PostalCode = ua.PostalCode ?? string.Empty
                                 },
                                 PhoneNumber = ua.PhoneNumber ?? string.Empty,
                                 FirstName = ua.FirstName ?? string.Empty,
                                 LastName = ua.LastName ?? string.Empty,
                                 DisplayName = ua.DisplayName ?? string.Empty,
                                 Title = ua.Title ?? string.Empty
                             }).ToArray();
                return users;
            }
        }

        public IEnumerable<NameValueDescription> GetSupportAccountUsersWithSfdcContactId(long supportAccountId, string searchTerm) {
            var db = SessionDbContext.CspObjectContext;

            var users =
                (from usr in db.useraccounts
                 join mem in db.memberships on usr.UserAccountId equals mem.UserAccountId
                 where
                     mem.SupportAccountId == supportAccountId && (usr.FirstName.Contains(searchTerm) || usr.LastName.Contains(searchTerm))
                 
                 select new { FirstName = usr.FirstName, LastName = usr.LastName, SalesforceContactId = mem.SalesforceContactId, Email = usr.Email })
                 .AsEnumerable()
                 .Select(x => new NameValueDescription()
                 {
                     Value = x.SalesforceContactId,
                     Name = string.Format("{0} {1}", x.FirstName ?? string.Empty, x.LastName ?? string.Empty),
                     Description = x.Email
                 }).OrderByDescending(x => x.Name).Distinct().ToList();

            return users;
        }

        //grant permission to the specific user
	    public bool GrantPermissionToUser(long supportAccountId, long userAccountId, Permission permission)
	    {
	        var query = @"
                SELECT r.RoleId FROM role r
                  INNER JOIN roleprivileges rp ON r.RoleId = rp.RoleId
                  INNER JOIN privilege p ON rp.PrivilegeId = p.PrivilegeId
                  WHERE p.Permission=?p_PermissionId;";
            using (var connection = new MySqlConnection(Secrets.CspDatabase))
	        {
                connection.Open();
	            using (var command = new MySqlCommand(query, connection))
	            {
	                command.CommandType = CommandType.Text;
									command.Parameters.AddWithValue("?p_PermissionId", (int)permission);

	                var result = command.ExecuteScalar();
	                if (result != null && (long)result > 0)
	                {
	                    var sql = @"
                        INSERT IGNORE INTO Membershiproles(MembershipId, RoleId, CreatedOn, CreatedByUserAccountId)
                            SELECT(
                            SELECT m.MembershipId FROM useraccount u
                            INNER JOIN membership m ON u.UserAccountId = m.UserAccountId
                            WHERE m.SupportAccountId=?p_SupportAccountId AND u.UserAccountId =?p_UserAccountId), ?p_Result, CURRENT_TIMESTAMP(), 1;";

	                    command.Parameters.Clear();
	                    command.CommandText = sql;
	                    command.Parameters.AddWithValue("?p_SupportAccountId", supportAccountId);
	                    command.Parameters.AddWithValue("?p_UserAccountId", userAccountId);
	                    command.Parameters.AddWithValue("?p_Result", (long) result);

	                    command.ExecuteNonQuery();
	                    return true;
	                }	                
	            }
	        }

	        return false;
	    }

        private string GetWildfireSerialNumbers(long supportAccountId, long userAccountId)
        {
            var serialNumbers = string.Empty;
            var query = @"SELECT d.SerialNumber FROM csp.wildfiredevice wd JOIN csp.device d ON d.DeviceId = wd.DeviceId WHERE wd.SupportaccountId = @p_SupportAccountId 
                        AND wd.WildfireUserAccountId = @p_UserAccountId;";

            try
            {
                using (var connection = new MySqlConnection(Secrets.CspDatabase))
                {
                    connection.Open();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("p_SupportAccountId", supportAccountId);
                        command.Parameters.AddWithValue("p_UserAccountId", userAccountId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var serialNumber = reader.GetColumnValue<string>("SerialNumber");
                                serialNumbers = (string.IsNullOrEmpty(serialNumbers)) ? serialNumber.ToString() : serialNumbers + ", " + serialNumber.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("GetWildfireSerialNumbers exception:" + ex.Message, ex));
            }
            return serialNumbers;
        }

        private void WildfireDeviceLog(long supportAccountId, long userAccountId, string serialNumbers, string eventType, string eventDescription, string createdByUser, string clientIpAddress)
        {
            try
            {
                var query = @"INSERT INTO `csp`.`wildfiredevicelog`(`Email`, `EventType`, `EventDescription`, `SerialNumbers`, `IpAddress`, `CreatedOn`, `CreatedByUser`)
                                          SELECT u.Email, @p_EventType, @p_EventDescription, @p_SerialNumbers, @p_IpAddress, NOW(), @p_CreatedByUser FROM csp.wildfireuseraccount u 
                                           WHERE u.WildfireUserAccountId = @p_UserAccountId";

                using (var connection = new MySqlConnection(Secrets.CspDatabase))
                {
                    connection.Open();
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("p_EventType", eventType);
                        command.Parameters.AddWithValue("p_EventDescription", eventDescription);
                        command.Parameters.AddWithValue("p_SerialNumbers", serialNumbers);
                        command.Parameters.AddWithValue("p_CreatedByUser", createdByUser);
                        command.Parameters.AddWithValue("p_IpAddress", clientIpAddress);
                        command.Parameters.AddWithValue("p_UserAccountId", userAccountId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("WildfireDeviceLog exception:" + ex.Message, ex));
            }
        }
    }
}
