using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {

	public class UserAccountDto : DefaultDto {

		public UserAccount UserAccount {
			get;
			set;
		}

		/// <summary>
		/// Password Regex Explanation:
		/// 1. 8 minimum characters
		/// 2. 3 of 4 (upper - U, lower - L, number - N, special - S)
		/// 
		/// Here is a breakdown of the Regex into the 4 groups
		/// ULN - ^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,}$
		/// ULS - ^(?=.*[A-Z])(?=.*[a-z])(?=.*[^a-zA-Z0-9\s]).{8,}$
		/// UNS - ^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9\s]).{8,}$
		/// LNS - ^(?=.*[a-z])(?=.*[0-9])(?=.*[^a-zA-Z0-9\s]).{8,}$
		/// 
		/// The final regex is an "or" combination of those 4:
		/// ^(((?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]))|((?=.*[A-Z])(?=.*[a-z])(?=.*[^a-zA-Z0-9\s]))|((?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9\s]))|((?=.*[a-z])(?=.*[0-9])(?=.*[^a-zA-Z0-9\s]))).{8,}$
		/// 
		/// Currently not including min length in regex as it is checked by the StringLength validator
		/// </summary>
		[StringLength(100, MinimumLength = 11, ErrorMessage = "Must be between 11 and 100 characters"),
		DataType(DataType.Password),
		Required(ErrorMessage = "Password is required"),
		RegularExpression(@"^(((?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]))|((?=.*[A-Z])(?=.*[a-z])(?=.*[^a-zA-Z0-9\s]))|((?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9\s]))|((?=.*[a-z])(?=.*[0-9])(?=.*[^a-zA-Z0-9\s]))).*$",
			ErrorMessage = "Passwords must contain 3 of the following: uppercase letter, lowercase letter, number, symbol")]
		public string Password { get; set; }

        public bool SetDefaultAddress { get; set; }

        public bool CreateMembership { get; set; }
	}
}