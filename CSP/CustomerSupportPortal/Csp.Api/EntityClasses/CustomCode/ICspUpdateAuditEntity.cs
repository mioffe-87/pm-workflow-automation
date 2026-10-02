using System;

namespace Csp.Api.EntityClasses {
	public interface ICspUpdateAuditEntity {
		Int64? LastUpdateByUserAccountId { get; set; }
		DateTime? LastUpdate { get; set; }
	}
}
