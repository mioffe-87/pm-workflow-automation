namespace Csp.Web.HelperClasses {

	/// <summary>
	/// This class is used to as an object in the base DTO that simplifies the passing of controller names and Ids so that they are not 
	/// specified in each controller or the view bag specifically when information needs to be passed back to the view. 
	/// </summary>
	public class ActionHelper {


		private string formAction;

		public enum EntityAction {
			Create,
			Edit,
			View
		}

		/// <summary>
		/// This is used to store the controller name, the Id of the a a record, typically a support account or group to be passed to the controller
		/// and the type of action.
		/// </summary>
		/// <param name="controller">Name of the controller</param>
		/// <param name="id">The id that is passed to the action</param>
		/// <param name="entityAction">Determines if the action performs a view, edit or create which is typically used as the action name for a form when posting </param>
		public ActionHelper(string controller, long id = 0, EntityAction entityAction = EntityAction.View) {

			Controller = controller;
			Id = id;

			switch (entityAction) {

				case EntityAction.Create:
					formAction = Create;
					break;
				case EntityAction.Edit:
					formAction = Edit;
					break;
				default:
					formAction = View;
					break;

			}

		}

		public string FormAction {

			get { return formAction; }
		}

		public long Id { get; set; }

		public string Controller { get; set; }


		public string ProductionDevices {

			get { return "ProductionDevices"; }
		}

		public string SpareDevices {

			get { return "SpareDevices"; }
		}

		public string Groups {

			get { return "Groups"; }
		}

		public string Users {

			get { return "Users"; }
		}

		public string Edit {

			get { return "Edit"; }
		}

		public string Details {

			get { return "Details"; }
		}

		public string View {

			get { return "View"; }
		}

		public string Create {

			get { return "Create"; }
		}

		public string Delete {

			get { return "Delete"; }
		}

		public string CreateGroupMembership {

			get { return "CreateGroupMembership"; }
		}

		public string CreateAccountMembership {

			get { return "CreateAccountMembership"; }
		}

		public string CreateDeviceMembership {

			get { return "CreateDeviceMembership"; }
		}

		public string Accounts {

			get { return "Accounts"; }
		}

		public string SupportAccounts {

			get { return "SupportAccounts"; }
		}

		public string Licenses {

			get { return "Licenses"; }
		}

		public string ActivateLicense {

			get { return "ActivateLicense"; }
		}

		public string RegisterDevice {

			get { return "RegisterDevice"; }
		}

        public string RegisterAwsDevice
        {

            get { return "RegisterAwsDevice"; }
        }

		public string BulkDeviceRegistration {

			get { return "BulkDeviceRegistration"; }
		}

		public string ActivateTrialLicense {

			get { return "ActivateTrialLicense"; }
		}

		public string GetDeviceUserByEmail {

			get { return "GetDeviceUserByEmail"; }
		}

		public string GetGroupUserByEmail {

			get { return "GetGroupUserByEmail"; }
		}

		public string GetSupportAccountUserByEmail {

			get { return "GetSupportAccountUserByEmail"; }
		}

		public string GetAscContactInfo {
			get { return "GetAscContactInfo"; }
		}

		public string PreRegister {
			get { return "PreRegister"; }
		}

        public string DeviceType
        {
            get { return "DeviceType"; }
        }

		public string Register {
			get { return "Register"; }
		}


		public string AddDeviceToGroup {
			get { return "AddDeviceToGroup"; }
		}

		public string DeviceCategories {
			get { return "DeviceCategories"; }
		}

		public string AddUpdateDeviceCategory {
			get { return "AddUpdateDeviceCategory"; }
		}

		public string DeleteDeviceCategory {
			get { return "DeleteDeviceCategory"; }
		}

		public string BulkRegistrationDevices {
			get { return "BulkRegistrationDevices"; }
		}

		public string BulkRegistrationLicenses {
			get { return "BulkRegistrationLicenses"; }
		}

		public string WildfireUsers {
			get { return "WildfireUsers"; }
		}

		public string AddUpdateWildfireUser {
			get { return "AddUpdateWildfireUser"; }
		}

		public string CreateWildfireUser {
			get { return "CreateWildfireUser"; }
		}

		public string DeleteWildfireUser {
			get { return "DeleteWildfireUser"; }
		}

		public string AddUpdateRole {
			get { return "AddUpdateRole"; }
		}

		public string AddUpdateAccountType {
			get { return "AddUpdateAccountType"; }
		}

		public string WipeOutNFRWildfire {
			get { return "WipeOutNFRWildfire"; }
		}

        public string CanProvisionPanorama
        {
            get { return "CanProvisionPanorama"; }
        }

        public string GetPendingDeviceTransferDetails {
            get { return "GetPendingDeviceTransferDetails"; }
        }
	}
}