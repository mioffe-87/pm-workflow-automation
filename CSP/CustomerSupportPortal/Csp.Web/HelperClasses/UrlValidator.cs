using System;
using System.Text.RegularExpressions;

namespace Csp.Web.HelperClasses {
	public class UrlValidator {
		private readonly string _leftPart;
		private readonly string _pathAndQuery;

		private readonly Regex _isFullyQualified = new Regex(@"\.paloaltonetworks\.(local|com)$");
		private const string InternalDomain = ".paloaltonetworks.local";
		private const string ExternalDomain = ".paloaltonetworks.com";

		public UrlValidator(Uri currentUri) {
			_leftPart = currentUri.GetLeftPart(UriPartial.Authority);
			_pathAndQuery = currentUri.PathAndQuery;
		}

		public bool IsFullyQualifiedHost() {
			return _isFullyQualified.IsMatch(_leftPart);
		}

		public string GetFullyQualifiedUrl() {
			return
				_leftPart.Contains("sjcccsp")
				? _leftPart + InternalDomain + _pathAndQuery
				: _leftPart + ExternalDomain + _pathAndQuery;
		}
	}
}
