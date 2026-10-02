using System.ComponentModel.DataAnnotations;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Attributes;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;

namespace Csp.Api.Model {

	public class GroupListItem : AuditCspModel {
		/// <summary>
		/// 
		/// </summary>
		private string _supportAccountGroupName;
		[DataAccess(GroupListItemFields.SupportAccountGroupName, DataType = DbType.String)]
		[Required(ErrorMessage = "The Group Name field is required.")]
		public string SupportAccountGroupName {
			set { _supportAccountGroupName = ApiCommon.DecodeString(value); }
			get { return _supportAccountGroupName; }
		}

		private string _description;
		[DataAccess(GroupListItemFields.Description, DataType = DbType.String)]
		public string Description {
			set { _description = ApiCommon.DecodeString(value); }
			get { return _description; }
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
	}
}
