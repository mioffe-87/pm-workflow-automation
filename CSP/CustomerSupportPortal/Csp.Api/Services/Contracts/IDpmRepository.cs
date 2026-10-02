namespace Csp.Api.Services.Contracts {
	public interface IDpmRepository {
		string GetDevicePartNumber(string serialNumber);
		bool IsModelDpmEnabled(string model);
		bool HasDpmLicense(string serialNumber);
	}
}
