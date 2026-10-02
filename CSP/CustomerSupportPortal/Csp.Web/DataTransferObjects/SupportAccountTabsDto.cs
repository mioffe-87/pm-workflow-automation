namespace Csp.Web.DataTransferObjects {

	public class SupportAccountTabsDto : DefaultDto {

		public SupportAccountTabs SelectedTab { get; set; }

		public long? PartialId { get; set; }

		public bool HasGroups { get; set; }

		public string DefaultAssetType { get; set; }
		public string DefaultDevicecode { get; set; }

        public bool IsCsspAccount { get; set; }

        public string FirstQueryStringValue { get; set; }

	}

	public enum SupportAccountTabs {
		Home,
		Account,
		Groups,
		Devices,
		Users,
        Cssp
	}
}