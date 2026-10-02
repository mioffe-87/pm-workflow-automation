namespace Csp.Api.Services.Contracts {
	public interface IWildfireWipeoutWebService {
		ApiResult[] WildFireWipeout(string serialnumber);
		ApiResult[] DeleteNotificationEmail(string email, string deviceSerialNumber);
		ApiResult[] ReplaceEmailNotification(string oldDeviceSerialNumber, string newDeviceSerialNumber);
	}

	public class ApiResult {
		public bool IsSuccess { get; set; }
		public string Data { get; set; }
		public string ServiceUrl { get; set; }
		public int RunTimeMilliseconds { get; set; }
		public string ResponseStatusCode { get; set; }
		public string ErrorMessage { get; set; }
	}
}
