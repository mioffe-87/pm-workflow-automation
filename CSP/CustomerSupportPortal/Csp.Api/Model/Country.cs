using System;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;

namespace Csp.Api.Model {

	/// <summary>
	/// Lookup table that holds list of countries.					
	/// </summary>
	public class Country : AuditCspModel {

		#region Public Properties

		/// <summary>
		/// Primary key.
		/// </summary>
		[DataAccess(CountryFields.CountryId, DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			set;
			get;
		}


		/// <summary>
		/// Country code.
		/// </summary>
		[DataAccess(CountryFields.Code)]
		public string Code {
			set;
			get;
		}


		/// <summary>
		/// Phone format.
		/// </summary>
		[DataAccess(CountryFields.PhoneMask)]
		public string PhoneMask {
			set;
			get;
		}


		/// <summary>
		/// Country name.
		/// </summary>
		[DataAccess(CountryFields.Name)]
		public string Name {
			set;
			get;
		}


		/// <summary>
		/// Region text based on country like (State, Provinces, etc.)
		/// </summary>
		[DataAccess(CountryFields.RegionLabel)]
		public string RegionLabel {
			set;
			get;
		}


		/// <summary>
		/// Postal code format.
		/// </summary>
		[DataAccess(CountryFields.PostalCodeMask)]
		public string PostalCodeMask {
			set;
			get;
		}


		/// <summary>
		/// Postal code.
		/// </summary>
		[DataAccess(CountryFields.PostalCodeLabel)]
		public string PostalCodeLabel {
			set;
			get;
		}

		#endregion

	}
}
