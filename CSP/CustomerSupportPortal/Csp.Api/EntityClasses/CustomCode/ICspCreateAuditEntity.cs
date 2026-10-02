using System;

namespace Csp.Api.EntityClasses {
	public interface ICspCreateAuditEntity {
		Int64? CreatedByUserAccountId { get; set; }
		DateTime? CreatedOn { get; set; }
	}
}
