using System.Linq;
using Csp.Infrastructure.Data.Persistence;
using Csp.Infrastructure.Data.Tests.DBSetup;
using Csp.Infrastructure.Data.Tests.DBSetup.Seed;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Csp.Infrastructure.Data.Tests {
	[TestClass]
	public class ResourceLinkRepositoryTests : SafeDbTest {

		[TestInitialize]
		public void SetupTests() {
			ClearData();
		}

		[TestMethod]
		public void GetResourceLinks() {

			ResourceLinksData.Seed();

			var resourceLinkRepository = new ResourceLinkRepository();

			var links = resourceLinkRepository.GetResourceLinks();

			Assert.AreEqual(ResourceLinksData.SeedData.Count(), links.Count());
		}
	}
}