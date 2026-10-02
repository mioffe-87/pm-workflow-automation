
namespace Csp.Api.Model {
	/// <summary>
	/// The security classification enum is used in the role class. This is primarily used in the database internally and the enum
	/// is a reprensentation to be used on the application side to assign the value with a meaningful name. This is used in the 
	/// role and is combined with membership to determine what data can be viewed. A user account has a membership with a group or 
	/// support account. That membership has a list of roles. Roles have the security classification to allow data to be viewed. 
	/// Roles can be combined to allow data to be viewed both from the account the user has membership with (default ot adiministrative)
	/// or data belonging to other accounts where no membership exists (support or global).
	/// </summary>
	public enum SecurityClassification {
		/*Basic access requires direct membership to support account or group, changes in the system say that group membership allows
        readonly access to the account. This would be the only exception to this classification A default role must exist for each membership
         to a group unless the user has an administrative role in membership to the support account that owns the groups*/
		Default = 1,
		/*This classification allows full access to an account and all access to a group if the membership is to a support account. 
			This is the only way this was designed to be used. other usage is unknown at this time if modified*/
		Administrative = 2,
		/*Allows access to all accounts that have a license that has the user as a member of the the account 
         * that is designated as the support center in the license. The license must not be expired. */
		Support = 3,
		/*Allows access to all accounts in the system except the account the user is a member of that has a role with this classification.*/
		Global = 4,
	}
}
