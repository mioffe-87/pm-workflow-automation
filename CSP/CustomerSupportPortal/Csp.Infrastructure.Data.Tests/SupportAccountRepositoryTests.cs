using System.Linq;
using Csp.Infrastructure.Data.DBs.CSP;
using Csp.Infrastructure.Data.Persistence;
using Csp.Infrastructure.Data.Tests.DBSetup;
using Csp.Web.App_Start;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Csp.Infrastructure.Data.Tests {

	//[TestClass]
	//public class SupportAccountRepositoryTests : SafeDbTest {

	//	[TestInitialize]
	//	public void SetupTests() {
	//		AutoMapperConfig.Configure();
	//		ReSeedData();
	//	}

	//	[TestMethod]
	//	public void GuessUserAccountIdBySupportAccountId_Basic() {

	//		using (var nhRepo = new NHHelpers.GetNhRepo()) {

	//			var supportAccountRepository = new SupportAccountRepository(nhRepo.UnitOfWork, nhRepo.Repository);

	//			const long supportAccountId = 245;

	//			var userAccountId = supportAccountRepository.GuessUserAccountIdBySupportAccountId(supportAccountId);

	//			Assert.Fail();
	//		}
	//	}
	//}
}