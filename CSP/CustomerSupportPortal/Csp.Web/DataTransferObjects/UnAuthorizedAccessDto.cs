namespace Csp.Web.DataTransferObjects
{
    public class UnAuthorizedAccessDto : DefaultDto
    {
        public string Email { get; set; }

        public bool HasExpiredMembership { get; set; }

        public bool HasImpersonateAccessToAccount { get; set; }
        public bool IsMembershipPending { get; set; }

        public bool HasAccess { get; set; }

    }
}