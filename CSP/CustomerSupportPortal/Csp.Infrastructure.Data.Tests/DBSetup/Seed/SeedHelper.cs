using System;

namespace Csp.Infrastructure.Data.Tests.DBSetup.Seed {
	public class SeedHelper {

		protected static string NullableDate(DateTime? value) {
			return value.HasValue ? "'" + value.Value.ToString("yyyy-MM-dd") + "'" : "null";
		}

		protected static string NullableLong(long? value) {
			return value.HasValue ? value.Value.ToString() : "null";
		}
	}
}