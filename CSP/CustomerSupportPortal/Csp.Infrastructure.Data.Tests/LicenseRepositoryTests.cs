using System.Linq;
using Csp.Infrastructure.Data.DBs.CSP;
using Csp.Infrastructure.Data.Persistence;
using Csp.Infrastructure.Data.Tests.DBSetup;
using Csp.Web.App_Start;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Csp.Infrastructure.Data.Tests {

	[TestClass]
	public class LicenseRepositoryTests : SafeDbTest {

		[TestInitialize]
		public void SetupTests() {
			AutoMapperConfig.Configure();
			ReSeedData();
		}

		[TestMethod]
		public void ReadLicenses_Empty() {

			var licenseRepository = new LicenseRepository(null);

			var licIds = new long[] { };

			var lics = licenseRepository.ReadLicenses(licIds);

			Assert.AreEqual(0, lics.Count());
		}

		[TestMethod]
		public void ReadLicenses_Basic() {

			var licenseRepository = new LicenseRepository(null);

			const long fakeLicId = 1337;
			var licIds = new long[] { 817656, 665396, 1643, fakeLicId }; // 3 real, One fake number: 1337

			var lics = licenseRepository.ReadLicenses(licIds);

			Assert.AreEqual(3, lics.Count());
			Assert.AreEqual(0, lics.Count(x => x.LicenseId == fakeLicId));
		}

		/// <summary>
		/// This test is for code that should be removed eventually
		/// Licenses shouldn't hold a supportaccountid field at all.
		/// 
		/// for some reason, some code uses this field.  All that code
		/// must first be re-written to use the device.supportaccountid field
		/// and then we can delete these method entirely
		/// </summary>
		[TestMethod]
		public void UpdateLicenseSupportAccountId() {

			using (var db = new cspEntities()) {
				var licenseRepository = new LicenseRepository(null);

				const long deviceId = 2;
				const long oldSupportAccountId = 245;
				const long newSupportAccountId = 92;
				const long licCount = 4;

				Assert.AreEqual(licCount, db.licenses.Count(l => l.DeviceId == deviceId
																												 && l.SupportAccountId == oldSupportAccountId));

				licenseRepository.UpdateLicenseSupportAccountId(deviceId, newSupportAccountId);

				Assert.AreEqual(licCount, db.licenses.Count(l => l.DeviceId == deviceId
																												 && l.SupportAccountId == newSupportAccountId));
			}
		}
	}
}