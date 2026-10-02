
using Csp.Api.Model.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using Url = Csp.Api.Model.Attributes.UrlAttribute;
using System.Collections.Generic;

namespace Csp.Api.Model {
	[Serializable]
    public class AscContactInfo : IEquatable<AscContactInfo> {
		public string Id { get; set; }
		public string Name { get; set; }

        [StringLength(80, ErrorMessage = "Maximum length is 80 characters"),
        Email(ErrorMessage = "Please enter a valid email")]
		public string Email { get; set; }
		public string Phone { get; set; }

        [Url(ErrorMessage = "Please enter a valid url")]
		public string WebSite { get; set; }
        public string ContactId { get; set; }
        public NameValueDescription Contact { get; set; }

        public bool Equals(AscContactInfo other) {
            if (other == null)
                return false;

            if (this.Id == other.Id && this.Email == other.Email && this.Phone == other.Phone 
                && this.WebSite == other.WebSite && this.ContactId == other.ContactId)
                return true;
            else
                return false;
        }
	}
}
