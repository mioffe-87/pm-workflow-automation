using System.Collections.Generic;

namespace Csp.Api.Model {
	public class ResourceLink {
		public string GroupName { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
		public long? HelpId { get; set; }
		public string ResourceId { get; set; }
		public IEnumerable<ResourceSubLink> ResourceSubLinks { get; set; }
		public string ImageUrl { get; set; }
		public bool IsExternalLink { get; set; }
	}

	public class ResourceSubLink {
		public long? ResourceSublinkId { get; set; }
		public long? ResourcelinkId { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
		public long? HelpId { get; set; }
		public string ResourceId { get; set; }
		public long OrderBy { get; set; }
		public string ImageUrl { get; set; }
		public bool IsExternalLink { get; set; }
	}

}