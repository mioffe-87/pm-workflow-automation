using Csp.Api.Model.Attributes;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.Data;


namespace Csp.Api.Model {

	public class SupportAccountMembershipListItem : MembershipListItem {


		#region Public Properties

		/// <summary>
		/// Company id.
		/// </summary>
		[DataAccess(SupportAccountFields.SupportAccountId, DataType = DbType.Int64, UseForCreate = false)]
		public long Id {
			set;
			get;
		}


		/// <summary>
		/// Company name.
		/// </summary>
		[DataAccess(SupportAccountFields.CustomerNumber, DataType = DbType.String)]
		public string CustomerNumber {
			set;
			get;
		}


		/// <summary>
		/// Company name.
		/// </summary>
		[DataAccess(SupportAccountFields.CompanyName, DataType = DbType.String)]
		public string CompanyName {
			set;
			get;
		}


		[DataAccess(SupportAccountFields.Address1, DataType = DbType.String)]
		public string Address1 {
			set;
			get;
		}

		[DataAccess(SupportAccountFields.City, DataType = DbType.String)]
		public string City {
			set;
			get;
		}

		[DataAccess(SupportAccountFields.Region, DataType = DbType.String)]
		public string Region {
			set;
			get;
		}


		[DataAccess(SupportAccountFields.Country, DataType = DbType.String)]
		public string Country {
			set;
			get;
		}


		[DataAccess(SupportAccountFields.PhoneNumber, DataType = DbType.String)]
		public string PhoneNumber {
			set;
			get;
		}

		#endregion
	}
}
