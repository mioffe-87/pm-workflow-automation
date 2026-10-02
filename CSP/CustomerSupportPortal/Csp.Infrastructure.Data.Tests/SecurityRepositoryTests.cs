using System.Linq;
using Csp.Api.Model;
using Csp.Infrastructure.Data.Persistence;
using Csp.Infrastructure.Data.Tests.DBSetup;
using Csp.Infrastructure.Data.Tests.DBSetup.Seed;
using Csp.Web.App_Start;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Csp.Infrastructure.Data.Tests {
	[TestClass]
	public class SecurityRepositoryTests : SafeDbTest {

		[TestInitialize]
		public void SetupTests() {
			AutoMapperConfig.Configure();
			ReSeedData();
		}

		[TestMethod]
		public void GetAvailableAccounts_StandardMembership() {

			//Setup
			const long userAccountId = 40893;
			var dataParameters = new DataParameters();
			const long expectedSupportAccountId = 92;

			MembershipData.SeedStandardMembership();

			//Run
			var securityRepository = new SecurityRepository();
			var results = securityRepository.GetAvailableAccounts(userAccountId, dataParameters);

			//Verify
			Assert.AreEqual(1, results.Count());
			Assert.AreEqual(expectedSupportAccountId, results.Single().SupportAccountId);
		}

		[TestMethod]
		public void GetAvailableAccounts_NoUserAccount() {

			//Setup
			const long userAccountId = 1337;
			var dataParameters = new DataParameters();

			MembershipData.SeedStandardMembership();

			//Run
			var securityRepository = new SecurityRepository();
			var results = securityRepository.GetAvailableAccounts(userAccountId, dataParameters);

			//Verify

			//No support account access
			Assert.AreEqual(0, results.Count());

			//And not in our seed data
			Assert.AreEqual(0, UserAccountData.UserAccountsBasic.Count(x => x.UserAccountId == userAccountId));
		}

		//New tests

		//1) Test group membership
        [TestMethod]
        public void GetAvailableAccounts_GroupMembership()
        {

            //Setup
            const long userAccountId = 40893;
            var dataParameters = new DataParameters();
            const long expectedSupportAccountId = 92;

            MembershipData.SeedGroupMembership();

            //Run
            var securityRepository = new SecurityRepository();
            var results = securityRepository.GetAvailableAccounts(userAccountId, dataParameters);

            //Verify
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(expectedSupportAccountId, results.Single().SupportAccountId);
        }
		
		//2) Test ASC membership
        [TestMethod]
        public void GetAvailableAccounts_AscMembershipPass()
        {

            //Setup
            const long userAccountId = 40893;
            var dataParameters = new DataParameters();
            const long expectedSupportAccountId = 92;

            MembershipData.SeedAscMembership();
            DeviceData.AscDevicesSeed();
            LicenseData.AscSeed();

            //Run
            var securityRepository = new SecurityRepository();
            var results = securityRepository.GetAvailableAccounts(userAccountId, dataParameters);

            //Verify
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(expectedSupportAccountId, results.Single().SupportAccountId);
        }

		//3) Test TAC membership (that they can access all OTHER supportaccounts, but not the PAN support account - they'd need a std/super membership to PAN to do that)

		//4) Test combination of memberships (don't need to do everthing, but at least one typical combination test like standard w/ asc membership would be nice)

		//5) Test IsActive flag for all 4 types of membership (this is a bug so these tests should actually fail for ASC and TAC - it looks like std and grp memberships do filter isactive properly)

		//6) Test where membership is expired

		//7) Test where membership activation date is still in the future
	}
}
