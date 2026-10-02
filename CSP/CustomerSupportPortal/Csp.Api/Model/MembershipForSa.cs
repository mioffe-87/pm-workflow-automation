namespace Csp.Api.Model {
	public class MembershipForSa {
		public long MembershipId { get; set; }
		public long? SupportAccountId { get; set; }
		public long? GroupSupportAccountId { get; set; }
		public long? AscSupportAccountId { get; set; }
		public long? GlobalSupportAccountId { get; set; }
        public long? ConsentSupportAccountId { get; set; }
    }
}