namespace Csp.Web.DataTransferObjects
{
    public class RegistrationConfirmationViewModel : DefaultDto
    {
        public string EmailAddress { get; set; }
        public string Name { get; set; }       
        public bool IsMembershipPending { get; set; }
        public bool IsAlreadySignedIn { get; set; }
    }
}