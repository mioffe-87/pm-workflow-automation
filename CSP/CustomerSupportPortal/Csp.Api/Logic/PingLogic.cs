using System;
using System.Linq;
using Csp.Api.Services.Contracts;
using Csp.Api.DataAccess;

namespace Csp.Api.Logic {
	public class PingLogic : IPingLogic {

		private readonly IMembershipRepository _membershipRepository;

		public PingLogic(IMembershipRepository membershipRepository) {
			_membershipRepository = membershipRepository;
		}

		public long GuessCurrentMembershipId(long userAccountId, long supportAccountId) {

            MembershipDataAccess membershipDataAccess= new MembershipDataAccess();
            var memberships = membershipDataAccess.GetMembershipsForSupportAccountId(userAccountId, supportAccountId).ToArray();
			//var memberships = _membershipRepository.GetMembershipsForSupportAccountId(userAccountId, supportAccountId).ToArray();

			//check for direct memberships to the support account, and pick one
			if (memberships.Any(x => x.SupportAccountId == supportAccountId)) {
				return memberships.First(x => x.SupportAccountId == supportAccountId).MembershipId;
			}

			//check for group membership to support account, and pick one
			if (memberships.Any(x => x.GroupSupportAccountId == supportAccountId)) {
				return memberships.First(x => x.GroupSupportAccountId == supportAccountId).MembershipId;
			}

			//check for asc membership that supports the support account
			if (memberships.Any(x => x.AscSupportAccountId == supportAccountId)) {
				return memberships.First(x => x.AscSupportAccountId == supportAccountId).MembershipId;
			}

			//check for global membership
			if (memberships.Any(x => x.GlobalSupportAccountId == supportAccountId)) {
				return memberships.First(x => x.GlobalSupportAccountId == supportAccountId).MembershipId;
			}

            //check for consent membership
            if (memberships.Any(x => x.ConsentSupportAccountId == supportAccountId))
            {
                return memberships.First(x => x.ConsentSupportAccountId == supportAccountId).MembershipId;
            }

			throw new UnauthorizedAccessException();
		}

		public long GetSupportAccountIdFromMembership(long membershipId) {
			return _membershipRepository.GetSupportAccountIdFromMembership(membershipId);
		}
	}
}
