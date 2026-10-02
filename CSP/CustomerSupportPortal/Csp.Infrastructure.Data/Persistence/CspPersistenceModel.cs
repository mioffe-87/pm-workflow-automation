using FluentNHibernate;

namespace Csp.Infrastructure.Data.Persistence {
	public class CspPersistenceModel : PersistenceModel {
		public CspPersistenceModel() {
			AddMappingsFromThisAssembly();
		}
	}
}
