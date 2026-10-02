using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Transactions;
using AutoMapper;
using Csp.Api.DataAccess.HelperClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.DBs.CSP;
using Csp.Api.HelperClasses;
using FluentNHibernate.Conventions;
using NHibernate.Util;
using MySql.Data.MySqlClient;
using IsolationLevel = System.Transactions.IsolationLevel;
using Csp.Api.DataAccess;

namespace Csp.Infrastructure.Data.Persistence {
	public class SecurityRepository : ISecurityRepository
	{
	    private const int PaloAltoNetworksAccount = 245;

		public IEnumerable<RoleSecurity> GetUserAccountRoleSecurities(long userAccountId) {

			using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted })) {

				var db = SessionDbContext.CspObjectContext;

				var now = DateTime.Now;

				var security =
					(from mr in db.membershiproles
					 where
						 mr.membership.UserAccountId == userAccountId
						 && mr.membership.ActivationDate <= now
						 && (
									mr.membership.ExpirationDate >= now
									|| mr.membership.ExpirationDate == null
								)
					 select new RoleSecurity {
						 SecurityClassificationNum = mr.role.SecurityClassification,
						 CategoryNum = mr.role.Category,
						 SupportAccountId = mr.membership.SupportAccountId,
						 GroupId = mr.membership.GroupId,
					 }).ToArray();

				trans.Complete();

				if (security.Any(x => x.IsValid() == false)) {
					throw new ArgumentOutOfRangeException("Invalid Security Settings");
				}

				return security;
			}
		}

		public bool IsTacSuperUser(long userAccountId, long supportAccountId) {
			using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted })) {

				var db = SessionDbContext.CspObjectContext;

				var now = DateTime.Now;

				var tacSuperUserRoles =
					(from mr in db.membershiproles
					 where
						 mr.membership.UserAccountId == userAccountId
						 && mr.membership.SupportAccountId == supportAccountId
						 && mr.membership.ActivationDate <= now
						 && (
									mr.membership.ExpirationDate >= now
									|| mr.membership.ExpirationDate == null
								)
								&& mr.role.Name == RoleNames.TACSuperUser
					 select mr.MembershipId).ToArray();

				var isTacSuper = tacSuperUserRoles.Any();

				trans.Complete();

				return isTacSuper;
			}
		}

        public bool IsTacUser(long userAccountId, long supportAccountId)
        {
            using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                var db = SessionDbContext.CspObjectContext;

                var now = DateTime.Now;

                var tacUserRoles =
                    (from mr in db.membershiproles
                     where
                         mr.membership.UserAccountId == userAccountId
                         && mr.membership.SupportAccountId == supportAccountId
                         && mr.membership.ActivationDate <= now
                         && (
                                    mr.membership.ExpirationDate >= now
                                    || mr.membership.ExpirationDate == null
                                )
                                && mr.role.RoleId == (long)RoleType.TACUser
                     select mr.MembershipId).ToArray();

                var isTacUser = tacUserRoles.Any();

                trans.Complete();

                return isTacUser;
            }
        }

        public bool IsTacUserOrAdmin(long userAccountId, long supportAccountId) {
	        using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted })) {
		        var db = SessionDbContext.CspObjectContext;

		        var now = DateTime.Now;

		        var tacUserRoles =
			        (from mr in db.membershiproles
				        where
					        mr.membership.UserAccountId == userAccountId
					        && mr.membership.SupportAccountId == supportAccountId
					        && mr.membership.ActivationDate <= now
					        && (
						        mr.membership.ExpirationDate >= now
						        || mr.membership.ExpirationDate == null
					        )
					        && (
						        mr.role.RoleId == (long)RoleType.TACUser
										|| mr.role.RoleId == (long)RoleType.TACAdmin
										)
				        select mr.MembershipId).ToArray();

		        var isTacUser = tacUserRoles.Any();

		        trans.Complete();

		        return isTacUser;
	        }
        }

        public bool IsSuperUser(long userAccountId, long supportAccountId)
        {
            using (var trans = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                var db = SessionDbContext.CspObjectContext;

                var now = DateTime.Now;

                var superUserRoles =
                    (from mr in db.membershiproles
                     where
                         mr.membership.UserAccountId == userAccountId
                         && mr.membership.SupportAccountId == supportAccountId
                         && mr.membership.ActivationDate <= now
                         && (
                                    mr.membership.ExpirationDate >= now
                                    || mr.membership.ExpirationDate == null
                                )
                                && mr.role.Name == RoleNames.SuperUser
                     select mr.MembershipId).ToArray();

                var isSuperUser = superUserRoles.Any();

                trans.Complete();

                return isSuperUser;
            }
        }

		public bool CanUserSignInToSupportAccount(long userAccountId, long supportAccountId) {

			var allAccess = GetListOfAccountsUserCanAccess(userAccountId).Where(x => x.SupportAccountId == supportAccountId);
			var canSignIn = allAccess.Any();

			return canSignIn;
		}

		public int GetNumberOfAccountsUserCanAccess(long userAccountId) {
			var supportAccounts = GetListOfAccountsUserCanAccess(userAccountId);

			return supportAccounts.Count;
		}

		public List<SelectableAccount> GetListOfAccountsUserCanAccess(long userAccountId) {
			var supportAccounts = new List<SelectableAccount>();

            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
				connection.Open();


				using (var command = new MySqlCommand("GetEligibleAccountsByUserId", connection)) {
					command.CommandType = System.Data.CommandType.StoredProcedure;
					command.Parameters.Add("@p_userAccountId", MySqlDbType.LongText).Value = userAccountId;

					using (var reader = command.ExecuteReader()) {
						while (reader.Read()) {
							var supportAccountId = reader.GetColumnValue<long>("SupportAccountId");
							var supportAccountName = reader.GetColumnValue<string>("Name");
							
							supportAccounts.Add(new SelectableAccount() { SupportAccountId = supportAccountId, CompanyName = supportAccountName });
						}
					}
				}

				connection.Close();
			}

			return supportAccounts;
		}

		public SelectableAccount[] GetAvailableAccounts(long userAccountId, DataParameters dataParameters) {

			var db = SessionDbContext.CspObjectContext;

			var allAccess = GetSupportAccountsUserCanCaccess(userAccountId, db);
            allAccess = allAccess.ToList().AsQueryable();
			// Filter
			if (!string.IsNullOrEmpty(dataParameters.SearchValue)) {
				switch (dataParameters.SearchField) {

					case "CompanyName":
						allAccess =
							allAccess.Where(x => x.CompanyName.ToLower().Contains(dataParameters.SearchValue.ToLower()));
						break;
                    case "IdOrName":
                        allAccess =
                            allAccess.Where(x => x.CompanyName.ToLower().Contains(dataParameters.SearchValue.ToLower()) || x.SupportAccountId.ToString().Contains(dataParameters.SearchValue.ToString()));
                        break;

					case "SupportAccountId":
						long supportAccountId = 0;
						if (Int64.TryParse(dataParameters.SearchValue, out supportAccountId)) {
							allAccess = allAccess.Where(x => x.SupportAccountId == supportAccountId);
						}
						break;

					case "Type":
						var accountTypeId = db.accounttypes.Single(x => x.Name.ToLower() == dataParameters.SearchValue.ToLower()).AccountTypeId;
						allAccess = allAccess.Where(x => x.AccountTypeId == accountTypeId);
						break;

					default:
						break;
				}
			}


			// Sort
			switch (dataParameters.SortField) {

				case "CompanyName":
					allAccess =
						dataParameters.SortDirection == SortDirection.Ascending
							? allAccess.OrderBy(x => x.CompanyName)
							: allAccess.OrderByDescending(x => x.CompanyName);
					break;

				case "SupportAccountId":
					allAccess =
						dataParameters.SortDirection == SortDirection.Ascending
							? allAccess.OrderBy(x => x.SupportAccountId)
							: allAccess.OrderByDescending(x => x.SupportAccountId);
					break;

				case "Type":
					allAccess =
						dataParameters.SortDirection == SortDirection.Ascending
							? allAccess.OrderBy(x => x.Type)
							: allAccess.OrderByDescending(x => x.Type);
					break;

				default:
					allAccess =
						allAccess.OrderBy(x => x.CompanyName).ThenBy(x => x.SupportAccountId);
					break;
			}


			// Page

			dataParameters.TotalRecordCount = allAccess.Count();
			if(dataParameters.PaginationCount > 0)
				allAccess = allAccess.Skip((int)dataParameters.PaginationOffset).Take(dataParameters.PaginationCount);

			var result = allAccess.ToArray();

			AddAccountTypeNames(result, db);

			return result;
		}

        public SelectableAccount[] GetAvailableAccountsForDevice(long userAccountId, DataParameters dataParameters, List<SelectableAccount> ascBklnAccounts )
        {

            var db = SessionDbContext.CspObjectContext;

            var allAccess = GetSupportAccountsUserCanCaccess(userAccountId, db);

            var commonAccounts = allAccess.ToList().Intersect(ascBklnAccounts).ToList();

            foreach (var c in commonAccounts)
            {
                ascBklnAccounts.Remove(c);
            }

            var selectableAccounts = allAccess.ToList().Union(ascBklnAccounts);

            allAccess = selectableAccounts.AsQueryable();

            // Filter
            if (!string.IsNullOrEmpty(dataParameters.SearchValue))
            {
                switch (dataParameters.SearchField)
                {

                    case "CompanyName":
                        allAccess =
                            allAccess.Where(x => x.CompanyName.ToLower().Contains(dataParameters.SearchValue.ToLower()));
                        break;

                    case "SupportAccountId":
                        long supportAccountId = 0;
                        if (Int64.TryParse(dataParameters.SearchValue, out supportAccountId))
                        {
                            allAccess = allAccess.Where(x => x.SupportAccountId == supportAccountId);
                        }
                        break;

                    case "Type":
                        var accountTypeId = db.accounttypes.Single(x => x.Name.ToLower() == dataParameters.SearchValue.ToLower()).AccountTypeId;
                        allAccess = allAccess.Where(x => x.AccountTypeId == accountTypeId);
                        break;

                    default:
                        break;
                }
            }


            // Sort
            switch (dataParameters.SortField)
            {

                case "CompanyName":
                    allAccess =
                        dataParameters.SortDirection == SortDirection.Ascending
                            ? allAccess.OrderBy(x => x.CompanyName)
                            : allAccess.OrderByDescending(x => x.CompanyName);
                    break;

                case "SupportAccountId":
                    allAccess =
                        dataParameters.SortDirection == SortDirection.Ascending
                            ? allAccess.OrderBy(x => x.SupportAccountId)
                            : allAccess.OrderByDescending(x => x.SupportAccountId);
                    break;

                case "Type":
                    allAccess =
                        dataParameters.SortDirection == SortDirection.Ascending
                            ? allAccess.OrderBy(x => x.Type)
                            : allAccess.OrderByDescending(x => x.Type);
                    break;

                default:
                    allAccess =
                        allAccess.OrderBy(x => x.CompanyName).ThenBy(x => x.SupportAccountId);
                    break;
            }


            // Page

            dataParameters.TotalRecordCount = allAccess.Count();

            allAccess = allAccess.Skip((int)dataParameters.PaginationOffset).Take(dataParameters.PaginationCount);

            var result = allAccess.ToArray();

            AddAccountTypeNames(result, db);

            return result;
        }

		private void AddAccountTypeNames(SelectableAccount[] result, cspEntities db) {
			var accountTypesLookup =
				(
					from at in db.accounttypes
					select new {
						at.AccountTypeId,
						at.Name,
					}).ToDictionary(x => x.AccountTypeId, x => x.Name);

			foreach (var r in result) {
				r.Type = accountTypesLookup[r.AccountTypeId];
			}
		}

		private IQueryable<SelectableAccount> GetSupportAccountsUserCanCaccess(long userAccountId, cspEntities db) {
			var now = DateTime.Now;

			var activeMemberships =
				(
				from mem in db.memberships
				join mr in db.membershiproles on mem.MembershipId equals mr.MembershipId
				where
					mem.UserAccountId == userAccountId
					&& mem.ActivationDate <= now
					&& (
							 mem.ExpirationDate >= now
							 || mem.ExpirationDate == null
						 )
				select new {
					mem.MembershipId,
					mem.SupportAccountId,
					mem.GroupId,
					mr.RoleId,
				}).ToArray();

			var dbRoles = db.roles.Select(x => new { x.RoleId, x.SecurityClassification, Category = x.Category }).ToArray();

			var standardOrSuperSecurityClassifications = new[]
        {
          (int) RoleSecurity.SecurityClassifications.Standard,
          (int) RoleSecurity.SecurityClassifications.Super
        };

			const int supportAccountCategory = (int)RoleSecurity.Categories.Account;

			var standardRoles = dbRoles.Where(r => standardOrSuperSecurityClassifications.Contains(r.SecurityClassification)
																							&& r.Category == supportAccountCategory).Select(r => r.RoleId);

			var standardMemberships = activeMemberships.Where(mem => standardRoles.Contains(mem.RoleId) && mem.SupportAccountId.HasValue).Select(mem => mem.SupportAccountId.Value).Distinct().ToArray();

			var standardAccess =
				from sa in db.supportaccounts
				where sa.IsActive == true
				&& standardMemberships.Contains(sa.SupportAccountId)

				select new SelectableAccount {
					SupportAccountId = sa.SupportAccountId,
					CompanyName = sa.Name,
					AccountTypeId = sa.AccountTypeId,
                    IsMfaEnabled = sa.IsMfaEnabled
				};

			var allAccess = standardAccess;


			const int standardSecurityClassification = (int)RoleSecurity.SecurityClassifications.Standard;
			const int groupCategory = (int)RoleSecurity.Categories.Group;

			var groupRoles = dbRoles.Where(r => r.SecurityClassification == standardSecurityClassification
																					 && r.Category == groupCategory).Select(r => r.RoleId);

			var groupMemberships = activeMemberships.Where(mem => groupRoles.Contains(mem.RoleId) && mem.GroupId.HasValue).Select(mem => mem.GroupId.Value).ToArray();

			if (groupMemberships.Any()) {
				var groupAccess =
					from sag in db.supportaccountgroups
					join sa in db.supportaccounts on sag.SupportAccountId equals sa.SupportAccountId
					where sa.IsActive == true
								&& groupMemberships.Contains(sag.GroupId)

					select new SelectableAccount {
						SupportAccountId = sag.SupportAccountId,
						CompanyName = sa.Name,
						AccountTypeId = sa.AccountTypeId,
                        IsMfaEnabled = sa.IsMfaEnabled
					};


				allAccess = allAccess.Union(groupAccess);
			}


			const int ascSecurityClassification = (int)RoleSecurity.SecurityClassifications.Asc;

			var ascRoles = dbRoles.Where(r => r.SecurityClassification == ascSecurityClassification
																				 && r.Category == supportAccountCategory).Select(r => r.RoleId);

			var ascMemberships = activeMemberships.Where(mem => ascRoles.Contains(mem.RoleId) && mem.SupportAccountId.HasValue).Select(mem => mem.SupportAccountId.Value).ToArray();

			if (ascMemberships.Any()) {

				var ascAccess =
					from l in db.licenses
					join d in db.devices on l.DeviceId equals d.DeviceId
					where
						l.SupportCenterSupportAccountId.HasValue
						&& l.IsActive
						&& ascMemberships.Contains(l.SupportCenterSupportAccountId.Value)
						&& d.SupportAccountId != null
					select new SelectableAccount {
						SupportAccountId = d.SupportAccountId.Value,
						CompanyName = d.supportaccount.Name,
						AccountTypeId = d.supportaccount.AccountTypeId,
                        IsMfaEnabled = d.supportaccount.IsMfaEnabled
					};

				allAccess = allAccess.Union(ascAccess);
			}




			const int tacSecurityClassification = (int)RoleSecurity.SecurityClassifications.Tac;

			var tacRoles = dbRoles.Where(r => r.SecurityClassification == tacSecurityClassification
																				 && r.Category == supportAccountCategory).Select(r => r.RoleId);

			var tacMemberships = activeMemberships.Where(mem => tacRoles.Contains(mem.RoleId) && mem.SupportAccountId.HasValue).Select(mem => mem.SupportAccountId.Value).ToArray();

			if (tacMemberships.Any()) {

				var tacSupportAccountId = tacMemberships.FirstOrDefault();

				var tacAccess =
					from nonTacSas in db.supportaccounts
					where
						nonTacSas.IsActive == true
						&& tacSupportAccountId != nonTacSas.SupportAccountId
					select new SelectableAccount {
						SupportAccountId = nonTacSas.SupportAccountId,
						CompanyName = nonTacSas.Name,
						AccountTypeId = nonTacSas.AccountTypeId,
                        IsMfaEnabled = nonTacSas.IsMfaEnabled
					};

				allAccess = allAccess.Union(tacAccess);
			}

			return allAccess;
		}

		public bool IsPanEmployeeRegisteredToSupportSite(string email) {
            SecurityDataAccess securityDataAccess = new SecurityDataAccess();
            return securityDataAccess.IsUserExist(email);
		}

		private readonly List<UserRolesCache> _userRolesCache = new List<UserRolesCache>();

		public string GetRoles(long userAccountId, long supportAccountId) {

			var now = DateTime.Now;

			var db = SessionDbContext.CspObjectContext;

			if (_userRolesCache.Count == 0) {

				var cache = (
					from mr in db.membershiproles
					join mem in db.memberships on mr.MembershipId equals mem.MembershipId
					join sag in db.supportaccountgroups on mem.GroupId equals sag.GroupId into tempSag
					from leftSag in tempSag.DefaultIfEmpty()

					where mem.UserAccountId == userAccountId
								&& mem.ActivationDate <= now
								&& (
									mem.ExpirationDate >= now
									|| mem.ExpirationDate == null
									)

									&& !mr.role.Name.Contains("TAC")
								&& !mr.role.Name.Contains("ASC")
					select new UserRolesCache {
						UserAccountId = userAccountId,
						SupportAccountId = mem.SupportAccountId ?? leftSag.SupportAccountId,
						Roles = mr.role.Name,
					});

				_userRolesCache.AddRange(cache);
			}

			var roles = (
				from x in _userRolesCache
				where x.UserAccountId == userAccountId
							&& x.SupportAccountId == supportAccountId
				select x.Roles).Distinct();

			return string.Join(", ", roles);
		}

		private const string CanaryEvalPstPanDbIdentification = "SITECANARYEVAL";
        private const string CanaryPstPanDbIdentification = "SITECANARY";
        private const string RenSiteCanaryPartSubTypePanDbIdentification = "RENSITECANARY";

		public bool HasActiveCanaryLicense(long supportAccountId) {

			var now = DateTime.Now;

			using (var scope = new TransactionScope(TransactionScopeOption.Suppress)) {

				var db = SessionDbContext.CspObjectContext;

				var filteredPm = (
					from pst in db.partsubtypes
					join pm in db.partmasters on pst.PartSubtypeId equals pm.PartSubtypeId
					where (pst.PanDbIdentification == CanaryPstPanDbIdentification || pst.PanDbIdentification == RenSiteCanaryPartSubTypePanDbIdentification || pst.PanDbIdentification == CanaryEvalPstPanDbIdentification)
					select pm.PartMasterId).ToArray();

				var lics =
					from lic in db.licenses
					where
						lic.IsActive
						&& lic.SiteLicenseSupportAccountId == supportAccountId
						&& lic.ExpirationDate > now
					where filteredPm.Contains(lic.PartMasterId)
					select lic.LicenseId;

				var anyActiveLics = lics.Any();

				return anyActiveLics;
			}
		}

	    public bool HasCanaryLicense(long supportAccountId, out bool isActive)
	    {
	        var now = DateTime.Now;
	        var hasCanaryLic = false;
	        isActive = false;

	        using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
	        {
	            var db = SessionDbContext.CspObjectContext;

	            var filteredPm = (
	                from pst in db.partsubtypes
	                join pm in db.partmasters on pst.PartSubtypeId equals pm.PartSubtypeId
	                where
	                (pst.PanDbIdentification == CanaryPstPanDbIdentification ||
	                 pst.PanDbIdentification == RenSiteCanaryPartSubTypePanDbIdentification ||
	                 pst.PanDbIdentification == CanaryEvalPstPanDbIdentification)
	                select pm.PartMasterId).ToArray();

                var canaryLics = (from lic in db.licenses
                                   where lic.SiteLicenseSupportAccountId == supportAccountId
                                   where filteredPm.Contains(lic.PartMasterId)
                                   select lic).ToList();

	            if (canaryLics.Any())
	            {
	                hasCanaryLic = true;
	                isActive = canaryLics.Any(l => l.IsActive && l.ExpirationDate > now);
	            }

                return hasCanaryLic;
	        }
	    }

        public bool IsThreatResearcher(long userAccountId, long supportAccountId, out NameValue category)
        {
			var now = DateTime.Now;
		    var hasMembership = false;
            category = new NameValue() { Name = RoleCategory.SupportAccount.ToString(), Value = string.Empty };

		    using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
		    {
		        var db = SessionDbContext.CspObjectContext;

		        var membership = from u in db.useraccounts
		            join m in db.memberships on u.UserAccountId equals m.UserAccountId
		            join mr in db.membershiproles on m.MembershipId equals mr.MembershipId
		            join r in db.roles on mr.RoleId equals r.RoleId
		            join g in db.supportaccountgroups on m.GroupId equals g.GroupId into groups
		            from sg in groups.DefaultIfEmpty()
		            where m.UserAccountId == userAccountId
		                  && ((sg == null) ? m.SupportAccountId == supportAccountId : sg.SupportAccountId == supportAccountId)
		                  && !u.Email.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION)
		                  && r.Name.Contains(RoleNames.ThreatResearcher)
		                  && m.ActivationDate < now
		                  && (m.ExpirationDate == null
		                      || m.ExpirationDate > now)
		            select m;

		        if (membership.Any())
		        {
		            hasMembership = true;
		            if (membership.First().GroupId != null)
		                category = new NameValue() {Name = RoleCategory.Group.ToString(), Value = membership.First().supportaccountgroup.Name};
		        }

		        var hasActiveNonExpiredCanary = HasActiveCanaryLicense(supportAccountId);

		        return hasMembership && hasActiveNonExpiredCanary;
		    }
		}

		public bool HasFreeCanaryLicenseUses(long supportAccountId) {

			var now = DateTime.Now;

			using (var scope = new TransactionScope(TransactionScopeOption.Suppress)) {

				var db = SessionDbContext.CspObjectContext;

                var filteredPm = (
                    from pst in db.partsubtypes
                    join pm in db.partmasters on pst.PartSubtypeId equals pm.PartSubtypeId
                    where (pst.PanDbIdentification == CanaryPstPanDbIdentification || pst.PanDbIdentification == RenSiteCanaryPartSubTypePanDbIdentification || pst.PanDbIdentification == CanaryEvalPstPanDbIdentification)
                    select pm.PartMasterId).ToArray();

                var filteredLics =
                    from lic in db.licenses
                    where
                        lic.IsActive
                        && lic.SiteLicenseSupportAccountId == supportAccountId
                        && lic.ExpirationDate > now
                        && filteredPm.Contains(lic.PartMasterId)
                    select lic;

				var canaryLic = filteredLics.FirstOrDefault();

				if (canaryLic == null) {
					return false;
				}

				var maxCanaryLics = canaryLic.VMUseCount;

				var currentThreatResearchersCount = GetCurrentThreatResearchersCount(db, supportAccountId);

				return currentThreatResearchersCount < maxCanaryLics;
			}
		}

		private int GetCurrentThreatResearchersCount(cspEntities db, long supportAccountId) {

			var now = DateTime.Now;

            var numThreatResearchers = (from u in db.useraccounts
                                         join m in db.memberships on u.UserAccountId equals m.UserAccountId
                                         join mr in db.membershiproles on m.MembershipId equals mr.MembershipId
                                         join r in db.roles on mr.RoleId equals r.RoleId
                                         join g in db.supportaccountgroups on m.GroupId equals g.GroupId into groups
                                         from sg in groups.DefaultIfEmpty()
                                         where (sg == null ? m.SupportAccountId == supportAccountId : sg.SupportAccountId == supportAccountId)
                                         && !u.Email.ToLower().Contains(ApiConstants.PANW_EMPLOYEE_EMAIL_EXTENSION)
                                                                                     && r.Name.Contains(RoleNames.ThreatResearcher)
                                                                                     && m.ActivationDate < now
                                                                                     && (m.ExpirationDate == null
                                                                                        || m.ExpirationDate > now)
                                         select m.UserAccountId).Distinct().Count();

			return numThreatResearchers;
		}

	    public int GetCurrentThreatResearchersCount(long supportAccountId)
	    {
	        return GetCurrentThreatResearchersCount(SessionDbContext.CspObjectContext, supportAccountId);
	    }

        public bool IsPartnerForAnySupportAccount(long restrictedByUserAccountId)
	    {
            var db = SessionDbContext.CspObjectContext;
	        return (
                from s in db.supportaccounts
                join m in db.memberships on s.SupportAccountId equals m.SupportAccountId
                where
                s.IsPartner == true
                && m.UserAccountId == restrictedByUserAccountId
                select s
            ).Any();
	    }

	    public bool IsPan245Member(long userAccountId)
	    {
            var db = SessionDbContext.CspObjectContext;
            return (
                from m in db.memberships
                where
                    m.SupportAccountId == 245
                    && m.UserAccountId == userAccountId
                select m
            ).Any();
	    }


	    class UserRolesCache {
			public long UserAccountId { get; set; }
			public long SupportAccountId { get; set; }
			public string Roles { get; set; }
		}



        public bool IsCsspSupportAccount(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;
            var isCssp = (from x in db.supportaccounts
                where x.SupportAccountId == supportAccountId && x.IsCssp
                select x.SupportAccountId).Any();
            return isCssp;
        }

        #region Autofocus Trial

        /// <summary>
        /// Check if AF Trial role is active for the given User and Support Account
        /// </summary>
        /// <param name="userAccountId"></param>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
        public bool? IsAutofocusTrialRoleActive(long userAccountId, long supportAccountId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                var db = SessionDbContext.CspObjectContext;
                bool? isActive = null;

                var autoFocusTrialUser = db.autofocustrialusers
                                           .SingleOrDefault(u => u.UserAccountId == userAccountId && u.SupportAccountId == supportAccountId);
                if (autoFocusTrialUser != null)
                    isActive = autoFocusTrialUser.ExpirationDate > DateTime.Now && (autoFocusTrialUser.IsActive ?? false);

                return isActive;
            }
        }

        /// <summary>
        /// Check if user has access to see Autofocus link
        /// </summary>
        /// <param name="userAccountId"></param>
        /// <param name="supportAccountId"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool IsAutoFocusTrialUser(long userAccountId, long supportAccountId, out NameValue category)
        {
            var now = DateTime.Now;
            var hasMembership = false;
            category = new NameValue() { Name = RoleCategory.None.ToString(), Value = string.Empty };

            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                var db = SessionDbContext.CspObjectContext;

                var hasActiveCanaryLicense = HasActiveCanaryLicense(supportAccountId);
                var isActiveAutofocusTrialUser = IsAutofocusTrialRoleActive(userAccountId, supportAccountId);

                if (hasActiveCanaryLicense && (isActiveAutofocusTrialUser ?? false))
                {
                    var membership = from m in db.memberships
                                     join mr in db.membershiproles on m.MembershipId equals mr.MembershipId
                                     join r in db.roles on mr.RoleId equals r.RoleId
                                     join g in db.supportaccountgroups on m.GroupId equals g.GroupId into groups
                                     from sg in groups.DefaultIfEmpty()
                                     where m.UserAccountId == userAccountId
                                           &&
                                           (sg == null
                                               ? m.SupportAccountId == supportAccountId
                                               : sg.SupportAccountId == supportAccountId)
                                           && r.Name.Contains(RoleNames.AutofocusTrial)
                                           && m.ActivationDate < now
                                           && (m.ExpirationDate == null || m.ExpirationDate > now)
                                     select m;

                    if (membership.Any())
                    {
                        hasMembership = true;
                        category = membership.First().GroupId != null
                                                                ? new NameValue()
                                                                {
                                                                    Name = RoleCategory.Group.ToString(),
                                                                    Value = membership.First().supportaccountgroup.Name
                                                                }
                                                                : new NameValue() { Name = RoleCategory.SupportAccount.ToString(), Value = string.Empty };
                    }
                }

                return hasMembership;
            }
        }

        /// <summary>
        /// Checks if user can be assigned AutoFocus Trial Role, based on expiration of AF  Site License
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
        public bool AutofocusTrialAllowed(long supportAccountId)
        {
            var nearlyExpired = DateTime.Now.AddDays(30);
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                //Autofocus Trial is not valid if Autofocus Site License is within 30 days of expiration
                var db = SessionDbContext.CspObjectContext;

                var filteredPm = (
                    from pst in db.partsubtypes
                    join pm in db.partmasters on pst.PartSubtypeId equals pm.PartSubtypeId
                    where
                    (pst.PanDbIdentification == CanaryPstPanDbIdentification ||
                     pst.PanDbIdentification == RenSiteCanaryPartSubTypePanDbIdentification ||
                     pst.PanDbIdentification == CanaryEvalPstPanDbIdentification)
                    select pm.PartMasterId).ToArray();

                //Check if license is active and expiration date is more than 30 days from now
                var lics =
                    from lic in db.licenses
                    where
                    lic.IsActive
                    && lic.SiteLicenseSupportAccountId == supportAccountId
                    && lic.ExpirationDate > nearlyExpired
                    where filteredPm.Contains(lic.PartMasterId)
                    select lic.LicenseId;

                var autofocusTrialAllowed = lics.Any();

                return autofocusTrialAllowed;
            }
        }


        /// <summary>
        /// Retrieve all AutoFocus Trial Users for the given SupportAccount
        /// </summary>
        /// <param name="supportAccountId"></param>
        public List<AutoFocusTrialUser> GetAutoFocusTrialUsersForSupportAccount(long supportAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var autofocusTrialUsers = db.autofocustrialusers.Where(u => u.SupportAccountId == supportAccountId).ToList();
            
            return Mapper.Map<List<AutoFocusTrialUser>>(autofocusTrialUsers);
        }

        /// <summary>
        /// Retrieve all Autofocus Trial Users whose Trial role expires today
        /// </summary>
        /// <returns></returns>
        public List<long> GetAutofocusTrialUserIdsExpiringToday()
        {
            var today = DateTime.Now.Date;
            var db = SessionDbContext.CspObjectContext;

            var autofocusTrailUsers = db.autofocustrialusers.ToList().Where(af => (af.IsActive ?? false) && af.ExpirationDate.Date <= today)
                                                                     .Select(u => u.AutoFocusTrialId).ToList();

            return autofocusTrailUsers;
        }

        /// <summary>
        /// Process expired Autofocus Trial roles
        /// </summary>
        /// <param name="autofocusTrialId"></param>
        public void ProcessAutofocusTrialRoleExpiration(long autofocusTrialId)
        {
            var db = SessionDbContext.CspObjectContext;
            var now = DateTime.Now;
            var autofocusTrialUser = db.autofocustrialusers.Find(autofocusTrialId);

            //Update AutoFocusTrial User - IsActive flag
            autofocusTrialUser.IsActive = false;
            autofocusTrialUser.LastUpdate = now;
            autofocusTrialUser.LastUpdateByUserAccountId = 1;

            //Insert record in Audit table
            db.autofocustrialusersaudits.Add(new autofocustrialusersaudit
            {
                AutoFocusTrialId = autofocusTrialId,
                UserAccountId = autofocusTrialUser.UserAccountId,
                SupportAccountId = autofocusTrialUser.SupportAccountId,
                Operation = "U",
                StatusId = (int)AutofocusTrialRoleStatus.Expired,
                CreatedOn = now,
                CreatedByUserAccountId = 1
            });
            db.SaveChanges();
        }

        #endregion

        #region Partner Services

        /// <summary>
        /// Checks if ATC Partner Roles are available for the given user and support account
        /// </summary>
        /// <param name="userAccountId"></param>
        /// <param name="supportAccountId"></param>
        /// <param name="isAtcManagerOnly"></param>
        /// <returns></returns>
        public bool ShowAtcPartnerRoles(long userAccountId, long supportAccountId, out bool isAtcManagerOnly)
	    {
            var db = SessionDbContext.CspObjectContext;
            var now = DateTime.Now;
            isAtcManagerOnly = false;
	        var showAtcPartnerRoles = false;

	        var isAtcAccount = (from sa in db.supportaccounts
	                            where sa.SupportAccountId == supportAccountId &&
	                            sa.IsJiveTrainingCenterMember.Value
                                select sa.SupportAccountId).Any();
	        if (isAtcAccount)
	        {
                var userAccountMemberships = (from mr in db.membershiproles
                                       where mr.membership.UserAccountId == userAccountId
                                             && mr.membership.ActivationDate <= now
                                             && (mr.membership.ExpirationDate >= now || mr.membership.ExpirationDate == null)
                                       select mr).ToList();

                var atcAccountMembership = userAccountMemberships.Where(m => m.membership.SupportAccountId == supportAccountId).ToList();
                var isAtcSuperUser = atcAccountMembership.Any(mr => mr.role.RoleId == (long)RoleType.SuperUser);
                var isAtcManager = atcAccountMembership.Any(mr => mr.role.RoleId == (long)RoleType.ATCManager);
	            isAtcManagerOnly = isAtcManager && !isAtcSuperUser; 

                var internalMembership = userAccountMemberships.Where(m => m.membership.SupportAccountId == PaloAltoNetworksAccount).ToList();
								var isInternalUser = internalMembership.Any(mr => mr.role.RoleId.IsAny((long)RoleType.TACUser, (long)RoleType.TACAdmin, (long)RoleType.TACSuperUser, (long)RoleType.PartnerProgramUser));

	            showAtcPartnerRoles = isInternalUser || isAtcSuperUser || isAtcManager;
	        }
            return showAtcPartnerRoles;
        }

        /// <summary>
        /// Seat Limitation for ATC Manager role
        /// </summary>
        /// <param name="supportAccountId"></param>
        /// <returns></returns>
	    public bool IsAtcManagerSeatsAvailable(long supportAccountId)
	    {
            var now = DateTime.Now;
	        bool isAvailable;
	        int defaultSeats;
	        int.TryParse(ConfigurationManager.AppSettings["DefaultATCManagerSeats"], out defaultSeats);

	        using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
	        {
	            var db = SessionDbContext.CspObjectContext;

	            //Check for Number of ATC Manager Seats for the SupportAccount, if exists, otherwise fall back to default count in config file
                var atcManagerSeats = (from seats in db.atcmanagerseats
                                       where seats.SupportAccountId == supportAccountId
                                       select seats).FirstOrDefault();

                var totalSeats = atcManagerSeats != null ? atcManagerSeats.NumberOfSeats : defaultSeats;
	            

	            var atcManagerCount = (from mr in db.membershiproles
	                                    where mr.membership.SupportAccountId == supportAccountId
	                                          && mr.RoleId == (long) RoleType.ATCManager
	                                          && mr.membership.ActivationDate <= now
	                                          && (mr.membership.ExpirationDate >= now || mr.membership.ExpirationDate == null)
	                                    select mr.membership).ToList().Count;

	            isAvailable = atcManagerCount < totalSeats;
	        }

	        return isAvailable;
	    }
        
        #endregion

        public bool IsAccountMfaEnabled(long supportAccountId)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var db = new cspEntities())
                {
                    var isMfaEnabled =
                        db.supportaccounts.Any(x => x.SupportAccountId == supportAccountId && x.IsMfaEnabled);
                    return isMfaEnabled;
                }
            }
        }

        public bool HasViewCaseManagementPrivilege(long supportAccountId, long userAccountId)
        {
            bool hasViewCaseManagementPrivilege = false;
            int permissionId = (int)Permission.ViewCaseManagement;

            try
            {
                string sqlQuery = string.Format(@"SELECT COUNT(rp.PrivilegeId)
                                                FROM csp.membership m
                                                  JOIN csp.useraccount u
                                                    ON m.UserAccountId = u.UserAccountId
                                                  JOIN csp.membershiproles mr
                                                    ON m.MembershipId = mr.MembershipId
                                                  JOIN csp.roleprivileges rp
                                                    ON rp.RoleId = mr.RoleId
                                                  JOIN csp.privilege p 
                                                    ON p.PrivilegeId = rp.PrivilegeId
                                                WHERE u.UserAccountId = @p_userAccountId 
                                                  AND m.SupportAccountId = @p_supportAccountId
                                                  AND p.Permission = @p_Permission;");

                using (var sqlConn = new MySqlConnection(Secrets.CspDatabase))
                {
                    var sqlCmd = new MySqlCommand();
                    sqlCmd.CommandType = System.Data.CommandType.Text;
                    sqlCmd.CommandText = sqlQuery;
                    sqlCmd.Parameters.Add("@p_userAccountId", MySqlDbType.LongText).Value = userAccountId;
                    sqlCmd.Parameters.Add("@p_supportAccountId", MySqlDbType.LongText).Value = supportAccountId;
                    sqlCmd.Parameters.Add("@p_Permission", MySqlDbType.Int16).Value = permissionId;

                    sqlCmd.Connection = sqlConn;
                    sqlConn.Open();

                    using (var reader = sqlCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hasViewCaseManagementPrivilege = (Convert.ToInt16(reader[0]) > 0);
                        }
                    } 
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception(string.Format("HasViewCaseManagementPrivilege : --> {0} {1} {2}", userAccountId, supportAccountId, ex.Message));
            }
            //finally
            //{
            //    sqlCmd.Dispose();
            //    sqlConn.Close();
            //    sqlConn.Dispose();
            //}

            return hasViewCaseManagementPrivilege;
        }

        public bool HasRole(long supportAccountId, long userAccountId, string roleName)
        {
            bool isDomainAdministrator = false;

            string sqlQuery = string.Format(@"SELECT COUNT(m.MembershipId)
                                                FROM membership m
                                                  JOIN useraccount u
                                                    ON m.UserAccountId = u.UserAccountId
                                                  JOIN membershiproles mr
                                                    ON m.MembershipId = mr.MembershipId
																									JOIN role r
																										ON mr.RoleId = r.RoleId
                                                WHERE (m.ExpirationDate IS NULL OR m.ExpirationDate >= CURDATE())
																									AND u.UserAccountId = @p_userAccountId 
                                                  AND ( m.SupportAccountId = @p_supportAccountId OR @p_supportAccountId = 0)
                                                  AND LOWER(r.Name) = @p_RoleName;");

            using (var sqlConnection = new MySqlConnection(Secrets.CspDatabase))
            {
                var sqlCommand = new MySqlCommand();
                sqlCommand.CommandType = System.Data.CommandType.Text;
                sqlCommand.CommandText = sqlQuery;
                sqlCommand.Parameters.Add("@p_userAccountId", MySqlDbType.LongText).Value = userAccountId;
                sqlCommand.Parameters.Add("@p_supportAccountId", MySqlDbType.LongText).Value = supportAccountId;
                sqlCommand.Parameters.Add("@p_RoleName", MySqlDbType.String).Value = roleName.ToLower();

                sqlCommand.Connection = sqlConnection;
                sqlConnection.Open();

                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        isDomainAdministrator = (Convert.ToInt16(reader[0]) > 0);
                    }
                }
            }

            return isDomainAdministrator;
        }

        public bool IsValidSupportAccount(long supportAccountId)
        {
            const string sqlQuery = @"SELECT count(s.SupportAccountId) FROM csp.supportaccount s WHERE s.IsActive=1 AND s.SupportAccountId = @p_SupportAccountId;";
          
            using (var sqlConn = new MySqlConnection(Secrets.CspDatabase))
            {
                var sqlCmd = new MySqlCommand();
                sqlCmd.CommandType = System.Data.CommandType.Text;
                sqlCmd.CommandText = sqlQuery;
                sqlCmd.Parameters.Add("@p_SupportAccountId", MySqlDbType.Int64).Value = supportAccountId;

                sqlCmd.Connection = sqlConn;
                sqlConn.Open();

                using (var reader = sqlCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return (Convert.ToInt16(reader[0]) > 0);
                    }
                } 
            }

            return false;
        }

        public List<RoleSecurity> GetRoleSecuritiesByUserAccountId(long userAccountId)
        {
            var roleSecurities = new List<RoleSecurity>();

            using (var connection = new MySqlConnection(Secrets.CspDatabase))
            {
                connection.Open();


                using (var command = new MySqlCommand("GetRoleSecuritiesByUserAccountId", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add("@p_userAccountId", MySqlDbType.LongText).Value = userAccountId;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var securityClassification = reader.GetColumnValue<int>("SecurityClassification");
                            var roleCategory = reader.GetColumnValue<int>("Category");
                            var supportAccountId = reader.GetColumnValue<long>("SupportAccountId");
                            var groupId = reader.GetColumnValue<long>("GroupId");

                            roleSecurities.Add(new RoleSecurity() { SupportAccountId = supportAccountId  , CategoryNum = roleCategory,GroupId = groupId, SecurityClassificationNum = securityClassification});
                        }
                    }
                }

                connection.Close();
            }

            return roleSecurities;
        }

        public bool IsConsentedUser(long userAccountId, long supportAccountId)
        {
            const string sqlQuery = @"SELECT
      COUNT(DISTINCT m.MembershipId)
    FROM csp.consentrequests c
      JOIN csp.membership m
        ON m.ConsentRequestId = c.ConsentRequestId
      JOIN csp.membership ms
        ON ms.supportAccountId = c.SourceSupportAccountId
      JOIN csp.membershiproles mr
        ON mr.MembershipId = m.MembershipId
      JOIN role r
        ON mr.RoleId = r.RoleId
    WHERE c.IsActive = 1 AND ms.UserAccountId = @p_UserAccountId
    AND (ms.ExpirationDate >= CURRENT_DATE() OR ms.ExpirationDate IS NULL) AND ms.ActivationDate <= CURRENT_DATE()
    AND c.TargetSupportAccountId = @p_SupportAccountId;";
            
            using (var sqlConn = new MySqlConnection(Secrets.CspDatabase))
            {
                var sqlCmd = new MySqlCommand();
                sqlCmd.CommandType = System.Data.CommandType.Text;
                sqlCmd.CommandText = sqlQuery;
                sqlCmd.Parameters.Add("@p_SupportAccountId", MySqlDbType.Int64).Value = supportAccountId;
                sqlCmd.Parameters.Add("@p_UserAccountId", MySqlDbType.Int64).Value = userAccountId;

                sqlCmd.Connection = sqlConn;
                sqlConn.Open();

                using (var reader = sqlCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return (Convert.ToInt16(reader[0]) > 0);
                    }
                } 
            }

            return false;
        }
    }
}
