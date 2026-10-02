
namespace Csp.Api.Model
{
    public class UserAccountTransferDetails
    {
        public long TransferId { get; set; }

        public long UserAccountId { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public long SourceSupportAccountId { get; set; }

        public string SourceSupportAccountName { get; set; }

        public long DestinationSupportAccountId { get; set; }

        public string DestinationSupportAccountName { get; set; }

        public bool IsNewMember { get; set; }

        public string CloudService { get; set; }

        public string SerialNumber { get; set; }

        public long TenantId { get; set; }

        public long CreatedByUserAccountId { get; set; }

        public string CreatedByUserEmail { get; set; }
    }
}
