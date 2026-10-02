using System;
using System.Collections.Generic;
using System.Linq;
using Csp.Api.EntityClasses;
using Csp.Api.Model;
using Csp.Api.Model.Enums;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using PAN.Common.Core.Persistence;
using Membership = Csp.Api.EntityClasses.Membership;

namespace Csp.Infrastructure.Data.Persistence {

	public class MembershipRepository : IMembershipRepository {
		private readonly IRepository _repository;
        private readonly ISecurityRepository _securityRepository;

        public MembershipRepository(IRepository repository, ISecurityRepository securityRepository)
        {
            _repository = repository;
            _securityRepository = securityRepository;
        }


		public IEnumerable<string> GetUserRoles(long supportAccountId, long userAccountId) {

			var roles =
				from mr in _repository.Query<Api.EntityClasses.Membershiprole>()
				orderby mr.RoleId_Role_.Name
				where
					mr.MembershipId_Membership_.UserAccountId_Useraccount_.UserAccountId == userAccountId
					&& (
							 mr.MembershipId_Membership_.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
							 ||
							 mr.MembershipId_Membership_.GroupId_Supportaccountgroup_.SupportAccountId_Supportaccount_
								 .SupportAccountId == supportAccountId
						 )
				select mr.RoleId_Role_.Name;

			return roles.ToArray();
		}

		/// <summary>
		/// If there is 1 and only 1 membership
		/// and it is of type support account, then return the
		/// SupportAccountId, otherwise return null
		/// </summary>
		/// <param name="userAccountId"></param>
		/// <returns></returns>
		public long? GetSingleSupportAccountMembership(long userAccountId) {

			var memberships =
				(
					from mem in _repository.Query<Api.EntityClasses.Membership>()
					where
						mem.UserAccountId_Useraccount_.UserAccountId == userAccountId
						&& mem.ActivationDate <= DateTime.Now
						&& (
								 mem.ExpirationDate >= DateTime.Now
								 || mem.ExpirationDate.HasValue == false
							 )
						&& (
							mem.SupportAccountId_Supportaccount_ != null
							|| mem.GroupId_Supportaccountgroup_ != null
						)
					select mem.SupportAccountId_Supportaccount_ != null ? mem.SupportAccountId_Supportaccount_.SupportAccountId
					: mem.GroupId_Supportaccountgroup_.SupportAccountId_Supportaccount_.SupportAccountId
				).ToArray().Distinct().ToArray();

			if (memberships.Length != 1) {
				return null;
			}

			var membership = memberships.Single();

			return membership;
		}

		public long GetSupportAccountIdFromMembership(long membershipId) {
			var membershipAccount = (
				from mem in _repository.Query<Api.EntityClasses.Membership>()
				where
					mem.MembershipId == membershipId
					&& mem.ActivationDate <= DateTime.Now
					&& (
							 mem.ExpirationDate >= DateTime.Now
							 || mem.ExpirationDate.HasValue == false
						 )
				select new {
					SupportAccountId = mem.SupportAccountId_Supportaccount_ == null ? (long?)null : mem.SupportAccountId_Supportaccount_.SupportAccountId,
					GroupSupportAccountId = mem.GroupId_Supportaccountgroup_ == null ? (long?)null : mem.GroupId_Supportaccountgroup_.SupportAccountId_Supportaccount_.SupportAccountId,
				}).Single();

			return membershipAccount.SupportAccountId.HasValue
							 ? membershipAccount.SupportAccountId.Value
							 : membershipAccount.GroupSupportAccountId.Value;
		}

		public IEnumerable<MembershipForSa> GetMembershipsForSupportAccountId(long userAccountId, long supportAccountId) {

			var supportAccountMemberships =
				(
				from mem in _repository.Query<Membership>()
				where
					mem.UserAccountId_Useraccount_.UserAccountId == userAccountId
					&& mem.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
					&& mem.ActivationDate <= DateTime.Now
					&& (
							 mem.ExpirationDate >= DateTime.Now
							 || mem.ExpirationDate.HasValue == false
						 )
				select new MembershipForSa {
					MembershipId = mem.MembershipId,
					SupportAccountId = mem.SupportAccountId_Supportaccount_.SupportAccountId,
				}).ToArray();

			var groupSupportAccountMemberships =
				(
				from mem in _repository.Query<Membership>()
				where
					mem.UserAccountId_Useraccount_.UserAccountId == userAccountId
					&& mem.GroupId_Supportaccountgroup_.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
					&& mem.ActivationDate <= DateTime.Now
					&& (
							 mem.ExpirationDate >= DateTime.Now
							 || mem.ExpirationDate.HasValue == false
						 )
				select new MembershipForSa {
					MembershipId = mem.MembershipId,
					GroupSupportAccountId = mem.GroupId_Supportaccountgroup_.SupportAccountId_Supportaccount_.SupportAccountId,
				}).ToArray();

			const short ascSecurityClassification = (short)SecurityClassification.Support;
			var ascSupportAccountMemberships =
				(
				from mr in _repository.Query<Membershiprole>()
				join lic in _repository.Query<Api.EntityClasses.License>() on mr.MembershipId_Membership_.SupportAccountId_Supportaccount_.SupportAccountId equals lic.SupportCenterSupportAccountId
				join dev in _repository.Query<Api.EntityClasses.Device>() on lic.DeviceId equals dev.DeviceId
				where
					mr.RoleId_Role_.SecurityClassification == ascSecurityClassification
					&& mr.MembershipId_Membership_.UserAccountId_Useraccount_.UserAccountId == userAccountId
					&& lic.IsActive == 1
					&& dev.SupportAccountId_Supportaccount_.SupportAccountId == supportAccountId
					&& mr.MembershipId_Membership_.ActivationDate <= DateTime.Now
					&& (
							 mr.MembershipId_Membership_.ExpirationDate >= DateTime.Now
							 || mr.MembershipId_Membership_.ExpirationDate.HasValue == false
						 )
				select new MembershipForSa {
					MembershipId = mr.MembershipId_Membership_.MembershipId,
					AscSupportAccountId = dev.SupportAccountId_Supportaccount_.SupportAccountId
				}).ToArray();

			const short globalSecurityClassification = (short)SecurityClassification.Global;
			var globalSupportAccountMemberships =
				(
				from mr in _repository.Query<Membershiprole>()
				where
					mr.RoleId_Role_.SecurityClassification == globalSecurityClassification
					&& mr.MembershipId_Membership_.SupportAccountId_Supportaccount_.SupportAccountId != supportAccountId
					&& mr.MembershipId_Membership_.UserAccountId_Useraccount_.UserAccountId == userAccountId
					&& mr.MembershipId_Membership_.ActivationDate <= DateTime.Now
					&& (
							 mr.MembershipId_Membership_.ExpirationDate >= DateTime.Now
							 || mr.MembershipId_Membership_.ExpirationDate.HasValue == false
						 )
				select new MembershipForSa {
					MembershipId = mr.MembershipId_Membership_.MembershipId,
					GlobalSupportAccountId = supportAccountId,
				}).ToArray();


			return
				supportAccountMemberships.Union(groupSupportAccountMemberships)
																 .Union(ascSupportAccountMemberships)
																 .Union(globalSupportAccountMemberships);
		}

		public SelectedSalesForceData GetSelectedSalesforceDataByMembership(long membershipId) {

			var db = SessionDbContext.CspObjectContext;

			var selectedMembership =
				(
					from mem in db.memberships
					where mem.MembershipId == membershipId
					select new {
						mem.MembershipId,
						mem.SalesforceContactId,
						mem.SalesforceLogInId,
						mem.SupportAccountId,
						mem.GroupId,
					}).Single();

			var selectedData = new SelectedSalesForceData {
				MembershipId = selectedMembership.MembershipId,
			};

			const int permissionNum = (int)Permission.ViewCaseManagement;

			var hasViewCaseManagementPermission =
				(
					from mr in db.membershiproles
					where mr.MembershipId == selectedData.MembershipId
								&& mr.role.roleprivileges.Any(rp => rp.privilege.Permission == permissionNum)
					select mr).Any();

			if (!hasViewCaseManagementPermission) {
				return selectedData;
			}

			selectedData.SalesForceContactId = selectedMembership.SalesforceContactId;
			selectedData.SalesForceLoginId = selectedMembership.SalesforceLogInId;

			if (selectedMembership.SupportAccountId.HasValue) {
				selectedData.SalesForceAccountId =
					(
						from sa in db.supportaccounts
						where sa.SupportAccountId == selectedMembership.SupportAccountId.Value
						select sa.SalesforceAccountId).Single();

			} else if (selectedMembership.GroupId.HasValue) {
				selectedData.SalesForceAccountId =
					(
						from sag in db.supportaccountgroups
						where sag.GroupId == selectedMembership.GroupId.Value
						select sag.supportaccount.SalesforceAccountId).Single();

			} else {
				Elmah.ErrorSignal.FromCurrentContext().Raise(new Exception("Selected Support Account Not found for MembershipId: " + membershipId));
			}

			return selectedData;
		}

        public IEnumerable<SfContact> GetUniqueContactsForSfUpdate(long userAccountId)
        {
            var db = SessionDbContext.CspObjectContext;

            var allContacts =
                from mem in db.memberships

                join sa in db.supportaccounts on mem.SupportAccountId equals sa.SupportAccountId into tempSa
                from leftSa in tempSa.DefaultIfEmpty()

                join sag in db.supportaccountgroups on mem.GroupId equals sag.GroupId into tempSag
                from leftSag in tempSag.DefaultIfEmpty()

                join grpSa in db.supportaccounts on leftSag.SupportAccountId equals grpSa.SupportAccountId into tempGrpSa
                from leftGrpSa in tempGrpSa.DefaultIfEmpty()

                where mem.UserAccountId == userAccountId
                && mem.SalesforceContactId != null
                && mem.SalesforceContactId != string.Empty
                select new
                {
                    SupportAccountId = leftSa == null ? leftGrpSa.SupportAccountId : leftSa.SupportAccountId,
                    SalesforceAccountId = leftSa == null ? leftGrpSa.SalesforceAccountId : leftSa.SalesforceAccountId,
                    mem.MembershipId,
                    mem.SalesforceContactId
                };

            var allContactsArray = allContacts.ToArray();

            var uniqueContacts =
                from c in allContactsArray
                group c by c.SalesforceContactId
                    into gc

                    join ac in allContactsArray on gc.Min(x => x.MembershipId) equals ac.MembershipId

                    select new SfContact
                    {
                        MembershipId = ac.MembershipId,
                        SupportAccountId = ac.SupportAccountId,
                        SalesforceId = ac.SalesforceAccountId,
                    };

            return uniqueContacts.ToArray();
        }

	    public IEnumerable<MembershipDetailForSfdcContactSync> GetAllMembershipsWithoutSfdcContact()
	    {
            var db = SessionDbContext.CspObjectContext;

            var allMemberships =
                from mem in db.memberships

                join ua in db.useraccounts on mem.UserAccountId equals ua.UserAccountId

                join sa in db.supportaccounts on mem.SupportAccountId equals sa.SupportAccountId

                join c in db.countries on ua.CountryId equals c.CountryId into tempCountry
                from leftC in tempCountry.DefaultIfEmpty()

                where mem.UserAccountId != null && !string.IsNullOrEmpty(ua.LastName)
                && mem.SupportAccountId != null
                && (string.IsNullOrEmpty(mem.SalesforceContactId) || string.IsNullOrEmpty(mem.SalesforceLogInId))
                && (mem.ExpirationDate >= DateTime.Now || mem.ExpirationDate.HasValue == false)

                select new MembershipDetailForSfdcContactSync
                {
                    MembershipId = mem.MembershipId,
                    UserAccountId = ua.UserAccountId,
                    SupportAccountId = sa.SupportAccountId,
                    SalesforceAccountId = sa.SalesforceAccountId,
                    SalesforceContactId = mem.SalesforceContactId,
                    CompanyName = sa.Name,
                    IsGroupUser = false
                };

	        var groupMemberships =
                from mem in db.memberships

                join ua in db.useraccounts on mem.UserAccountId equals ua.UserAccountId

                join sag in db.supportaccountgroups on mem.GroupId equals sag.GroupId

                join sa in db.supportaccounts on sag.SupportAccountId equals sa.SupportAccountId

                join c in db.countries on ua.CountryId equals c.CountryId into tempCountry
                from leftC in tempCountry.DefaultIfEmpty()

                where mem.UserAccountId != null && !string.IsNullOrEmpty(ua.LastName)
                && mem.GroupId != null
                && (string.IsNullOrEmpty(mem.SalesforceContactId) || string.IsNullOrEmpty(mem.SalesforceLogInId))
                && (mem.ExpirationDate >= DateTime.Now || mem.ExpirationDate.HasValue == false)

                select new MembershipDetailForSfdcContactSync
                {
                    MembershipId = mem.MembershipId,
                    UserAccountId = ua.UserAccountId,
                    SupportAccountId = sa.SupportAccountId,
                    SalesforceAccountId = sa.SalesforceAccountId,
                    SalesforceContactId = mem.SalesforceContactId,
                    CompanyName = sa.Name,
                    IsGroupUser = true
                };

	        return allMemberships.Union(groupMemberships);
	    }

	public bool HasPendingMemberships(long userAccountId)
	    {
            var db = SessionDbContext.CspObjectContext;

	        return db.memberships.Any(
	            x => x.UserAccountId == userAccountId
	                 && x.MembershipStatus == (int) MembershipStatus.Pending);
	    }

	    public long GetNumberOfPendingMembershipsBySupportAccountId(long supportAccountId)
	    {
	        var db = SessionDbContext.CspObjectContext;

	        return
	            db.memberships.Count(
	                x => x.SupportAccountId == supportAccountId && x.MembershipStatus == (int) MembershipStatus.Pending);
	    }

	    public IEnumerable<PendingMembershipModel> GetAllPendingMemberships(long supportAccountId)
	    {
            var db = SessionDbContext.CspObjectContext;

	        var pendingMemberships =
	            from mem in db.memberships
                join usr in db.useraccounts on mem.UserAccountId equals usr.UserAccountId
	            where mem.SupportAccountId == supportAccountId
	                  && mem.MembershipStatus == (int) MembershipStatus.Pending
	            select new PendingMembershipModel
	            {
                    DisplayName = usr.FirstName + " " + usr.LastName,
                    Email = usr.Email,
                    CreateDate = mem.ActivationDate,
                    MembershipResponseGuid = mem.MembershipResponseGUID,
	            };

	        return pendingMemberships.ToArray();
	    }

        public MembershipDetails GetMembershipDetailsBySfdcPortalUsername(string portalUsername, bool hasMultipleAccounts = false)
        {
            var db = SessionDbContext.CspObjectContext;

            var membershipDetails =
                (
                from mem in db.memberships
                join ua in db.useraccounts on mem.UserAccountId equals ua.UserAccountId
                join sa in db.supportaccounts on mem.SupportAccountId equals sa.SupportAccountId
                where
                    mem.SalesforceLogInId == portalUsername
                select new MembershipDetails
                {
                    MembershipId = mem.MembershipId,
                    UserAccountId = ua.UserAccountId,
                    FirstName = ua.FirstName, 
                    LastName = ua.LastName,
                    SupportAccountId = sa.SupportAccountId,
                    CompanyName = sa.Name,
                    SalesforceAccountId = sa.SalesforceAccountId,
                    HasMultipleAccounts = false
                }).FirstOrDefault();

            if (!hasMultipleAccounts && membershipDetails != null)
            {
                var memberships = _securityRepository.GetNumberOfAccountsUserCanAccess(membershipDetails.UserAccountId);

                if (memberships > 1)
                {
                    membershipDetails.HasMultipleAccounts = true;
                }
            }

            return membershipDetails;
        }

        public string GetSalesforceContactIdByUserAndSupportAccountId(long userAccountId, long supportAccountId)
        {
            var contactId = "";
            var db = SessionDbContext.CspObjectContext;

            var membership =
            (
                from mem in db.memberships
                where
                    mem.UserAccountId == userAccountId
                    && mem.SupportAccountId == supportAccountId
                select new 
                {
                    mem.SalesforceContactId
                }).FirstOrDefault();

            if (membership != null)
            {
                contactId = membership.SalesforceContactId;
            }

            return contactId;
        }
    }
}
