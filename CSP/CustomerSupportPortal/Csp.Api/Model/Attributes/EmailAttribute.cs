using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Model.Attributes {
	public class EmailAttribute : RegularExpressionAttribute {
		public EmailAttribute()
			: base(GetRegex()) {
		}

		private static string GetRegex() {
            // Comments By LK On 09/29/2016
            // changes to allow special characters in an email address
			//return @"^[\w\.'-]+@[\w\.-]+\.\w{2,12}$";	// @"^[\w\.-]+@(?!" + publicDomains + @")[\w\.-]+\.\w{2,4}$";

            return @"^(([\w-'\+!#&amp;$%'*/=?^_`{|}~-]+\.)+[\w-'\+!#&amp;$%'*/=?^_`{|}~-]+|([a-zA-Z]{1}|[\w-'\+!#&amp;$%'*/=?^_`{|}~-]{2,}))+@[\w\.-]+\.\w{2,12}$";
		}
	}
}