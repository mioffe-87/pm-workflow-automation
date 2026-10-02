using System.Data;
using Csp.Api.Model.Attributes;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.ComponentModel.DataAnnotations;

namespace Csp.Api.Model {

	/// <summary>
	/// Storage of the address for Support account, User account and Groups.					
	/// </summary>
	public class Address : BaseCspModel {

		#region Constructors

		public Address() {

		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Address 1.
		/// </summary>
		[StringLength(255, ErrorMessage = "Maximum length is 255 characters"),
		//Required(ErrorMessage = "Address Line 1 is required"),
		DataAccess(AddressFields.Address1)]
		public string Address1 {
			set;
			get;
		}


		/// <summary>
		/// Address 2.
		/// </summary>
		[StringLength(255, ErrorMessage = "Maximum length is 255 characters"),
		DataAccess(AddressFields.Address2)]
		public string Address2 {
			set;
			get;
		}


		/// <summary>
		/// City
		/// </summary>
		[StringLength(100, ErrorMessage = "Maximum length is 100 characters"),
		//Required(ErrorMessage = "City is required"),
		DataAccess(AddressFields.City)]
		public string City {
			set;
			get;
		}


		/// <summary>
		/// Value on this field depends on country.
		/// </summary>
		[StringLength(45, ErrorMessage = "Maximum length is 45 characters"),
		//Required(ErrorMessage = "Region/State is required"),
		DataAccess(AddressFields.Region)]
		public string Region {
			set;
			get;
		}


		/// <summary>
		/// Postal code.
		/// </summary>
		[StringLength(25, ErrorMessage = "Maximum length is 25 characters"),
		//Required(ErrorMessage = "Postal Code is required"),
		DataAccess(AddressFields.PostalCode)]
		public string PostalCode {
			set;
			get;
		}


		/// <summary>
		/// Country
		/// </summary>
		[DataAccess(AddressFields.CountryId, DataType = DbType.Int64)]
		//Required(ErrorMessage = "Country is required")]
		public long? CountryId {
			set;
			get;
		}


		/// <summary>
		/// Country
		/// </summary>
		[DataAccess(AddressFields.Country)]
		public string Country {
			set;
			get;
		}

		#endregion

	}
}
