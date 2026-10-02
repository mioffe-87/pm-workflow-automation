
using Newtonsoft.Json;

namespace Csp.Api.Model {
	public class NameValue {
		public string Name { get; set; }
		public string Value { get; set; }
		public long ValueLong { set { Value = value.ToString(); } }
		public bool IsActive { get; set; }
		public bool IsDisabled { get; set; }
	}

	public class NameValueDescription {
		public string Name { get; set; }
		public string Value { get; set; }
		public string Description { get; set; }
	}

	public class NameValueOnly {
		[JsonProperty(PropertyName = "label")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "value")]
		public string Value { get; set; }

	    public bool IsEA { get; set; }
	}
}
