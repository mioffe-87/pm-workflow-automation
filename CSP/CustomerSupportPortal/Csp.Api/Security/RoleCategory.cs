namespace Csp.Api.Security {

	/// <summary>
	/// This enum is used to determine if the role is applicable to a support account,
	/// group or a device. The device is for wildfire only access at this time. However due 
	/// to changes in how it was initially planned to work, and how the new implementation mimics the 
	/// existing system the CSP replaces, the device role category is not as useful. 
	/// </summary>
	public enum RoleCategory {

        None =0,
		SupportAccount = 1,
		Group = 2,
		Device = 3

	}
}