namespace Csp.Api.Services.Contracts {
	public interface IEmailHelper {
		bool SendGeneralMail(string emailTo, string subject, string message);
        bool SendGeneralMail(string emailTo, string subject, string message, bool isEmailOptOutEnabled);
	}
}
