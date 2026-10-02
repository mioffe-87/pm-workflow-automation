using System;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model.Attributes;

namespace Csp.Api.Model {

	public abstract class AuditCspModel : BaseCspModel {
		[DataAccess(AuditCspModelClassFields.CreatedOn, UseForUpdate = false, DataType = DbType.DateTime)]
		public DateTime CreatedOn {
			set;
			get;
		}


		[DataAccess(AuditCspModelClassFields.CreatedByUserAccountId, UseForUpdate = false, DataType = DbType.Int64)]
		public long CreatedByUserAccountId {
			get;
			set;
		}


		[DataAccess(AuditCspModelClassFields.LastUpdate, UseForCreate = false, DataType = DbType.DateTime)]
		public DateTime LastUpdate {
			set;
			get;
		}


		[DataAccess(AuditCspModelClassFields.LastUpdateByUserAccountId, UseForCreate = false, DataType = DbType.Int64)]
		public long LastUpdateByUserAccountId {
			get;
			set;
		}


		[DataAccess(AuditCspModelClassFields.CreatedByEmail, UseForCreate = false, UseForUpdate = false, DataType = DbType.String)]
		public string CreatedByEmail {
			get;
			set;
		}


		[DataAccess(AuditCspModelClassFields.LastUpdateByEmail, UseForCreate = false, UseForUpdate = false, DataType = DbType.String)]
		public string LastUpdateByEmail {
			get;
			set;
		}
	}
}
