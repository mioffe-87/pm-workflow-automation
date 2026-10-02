using System;

namespace Csp.Api.EntityClasses {
	public partial class Wildfireuseraccount : ICspAuditEntity
	{
		public virtual Int64? CreatedByUserAccountId { get; set; }
		public virtual Int64? LastUpdateByUserAccountId { get; set; }
	}
}
