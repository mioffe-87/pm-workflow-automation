using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;

namespace Csp.Api.Model {
	public class SelectorAccountListItem : SupportAccountListItem {

		[DataAccess(GroupListItemFields.GroupId, DataType = DbType.Int64)]
		public long GroupId {
			set;
			get;
		}

		[DataAccess(GroupListItemFields.SupportAccountGroupName, DataType = DbType.String)]
		public string SupportAccountGroupName {
			set;
			get;
		}

		[DataAccess(MembershipFields.MembershipId, DataType = DbType.Int64)]
		public long MembershipId {
			set;
			get;
		}

	}
}
