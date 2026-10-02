namespace Csp.Api.Services.Contracts {
	public interface IPingLogic {
		long GuessCurrentMembershipId(long userAccountId, long supportAccountId);
		long GetSupportAccountIdFromMembership(long membershipId);
	}
}
