using Csp.Api.Model.Attributes;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.Data;

namespace Csp.Api.Model {

	public class GroupMembershipListItem : MembershipListItem {

		#region Public Properties
		[DataAccess(GroupListItemFields.SupportAccountGroupName, DataType = DbType.String)]
		public string SupportAccountGroupName {
			set;
			get;
		}

		[DataAccess(GroupListItemFields.SupportAccountId, DataType = DbType.Int64)]
		public long SupportAccountId {
			set;
			get;
		}


		[DataAccess(GroupListItemFields.SupportAccountName, DataType = DbType.String)]
		public string SupportAccountName {
			set;
			get;
		}

		[DataAccess(GroupListItemFields.Address1, DataType = DbType.String)]
		public string Address1 {
			set;
			get;
		}

		[DataAccess(GroupListItemFields.City, DataType = DbType.String)]
		public string City {
			set;
			get;
		}

		[DataAccess(GroupListItemFields.Region, DataType = DbType.String)]
		public string Region {
			set;
			get;
		}


		[DataAccess(GroupListItemFields.Country, DataType = DbType.String)]
		public string Country {
			set;
			get;
		}

		[DataAccess(GroupListItemFields.GroupId, DataType = DbType.Int64)]
		public long GroupId {
			set;
			get;
		}

		#endregion
	}
}
