using Csp.Api.Logic;
using Csp.Api.Services.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
//using PAN.Common.Core.Persistence;

namespace Csp.Api.Tests.LogicTest {
	[TestClass]
	public class UserAccountLogicTests {
        /*
		//private readonly Mock<IRepository> _repository = new Mock<IRepository>();
		private readonly Mock<IUserAccountRepository> _userAccountRepository = new Mock<IUserAccountRepository>();
		private readonly Mock<ILookupLogic> _lookupLogic = new Mock<ILookupLogic>();
		private readonly Mock<ISupportAccountRepository> _supportAccountRepository = new Mock<ISupportAccountRepository>();
		private readonly Mock<IMembershipLogic> _membershipLogic = new Mock<IMembershipLogic>();
		private readonly Mock<ISalesforceService> _salesforceService = new Mock<ISalesforceService>();
		private readonly Mock<IMembershipRepository> _membershipRepository = new Mock<IMembershipRepository>();
		private readonly Mock<IUserRegistrationRepository> _userRegistrationRepository = new Mock<IUserRegistrationRepository>();
		private readonly Mock<IEvalTrackerRepository> _evalTrackerRepository = new Mock<IEvalTrackerRepository>();
		private readonly Mock<ISupportAccountDomainsService> _supportAccountDomainsService = new Mock<ISupportAccountDomainsService>();
		private readonly Mock<IWildfireWipeoutService> _wildfireWipeoutService = new Mock<IWildfireWipeoutService>();
		private readonly Mock<IEncryptionProvider> _encryptionProvider = new Mock<IEncryptionProvider>();
		private readonly Mock<ILookupRepository> _lookupRepository = new Mock<ILookupRepository>();

		[TestMethod]
		public void GoComMtTest() {

			//Setup
			const string email = "blah@go.com.mt";
			const string lookupValues =
				"testgo.com|blah.com";
			_lookupLogic.Setup(x => x.GetPublicDomains()).Returns(lookupValues);


			//Run test
			var userAccountLogic = new UserAccountLogic(_repository.Object, _userAccountRepository.Object, _lookupLogic.Object,
																									_membershipLogic.Object, _salesforceService.Object,
																									_membershipRepository.Object, _userRegistrationRepository.Object,
																									_evalTrackerRepository.Object, _supportAccountRepository.Object,
																									_supportAccountDomainsService.Object, _wildfireWipeoutService.Object, _encryptionProvider.Object, _lookupRepository.Object);
			var result = userAccountLogic.IsValidNonPublicEmail(email);

			//Verify
			Assert.AreEqual(true, result);
		}

		[TestMethod]
		public void RegistrationSfTest() {

			//Setup
			const string deviceCode = "deviceid";
			const long supportAccountId = 1337;
			_userRegistrationRepository.Setup(x => x.IsEmailInUse(It.IsAny<string>())).Returns(false);
			_userRegistrationRepository.Setup(x => x.IsDisplayNameInUse(It.IsAny<string>())).Returns(false);
			_userRegistrationRepository.Setup(x => x.IsEvalDeviceCode(It.IsAny<string>())).Returns(true);
			_userRegistrationRepository.Setup(x => x.GetSupportAccountIdsBySfAccountId(It.IsAny<string>(), It.IsAny<string>())).Returns(new long[] { supportAccountId });

			_evalTrackerRepository.Setup(x => x.GetSfAccountDataByEvalId(It.IsAny<string>())).Returns(new EvalTrackerData {
				DeviceCodes = new string[] { deviceCode },
				SfAccountId = "012345678901234567",
			});
			_salesforceService.Setup(x => x.GetSalesforceEvalDataByEvalId(It.IsAny<string>())).Returns(new SalesforceEvalData {
				SfAccountId = "012345678901234567",
			});

			//Run
			var userAccountLogic = new UserAccountLogic(_repository.Object, _userAccountRepository.Object, _lookupLogic.Object,
																									_membershipLogic.Object, _salesforceService.Object,
																									_membershipRepository.Object, _userRegistrationRepository.Object,
																									_evalTrackerRepository.Object, _supportAccountRepository.Object,
																									_supportAccountDomainsService.Object, _wildfireWipeoutService.Object, _encryptionProvider.Object, _lookupRepository.Object);
			var result = userAccountLogic.ValidateUserRegistration("test@test.test", "test", deviceCode, "custid");

			//Verify
			Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
			Assert.AreEqual(supportAccountId, result.SupportAccountId);
		}

		[TestMethod]
		public void RegistrationSfTest_NullData() {

			//Setup
			const string deviceCode = "deviceid";
			const long supportAccountId = 1337;
			_userRegistrationRepository.Setup(x => x.IsEmailInUse(It.IsAny<string>())).Returns(false);
			_userRegistrationRepository.Setup(x => x.IsDisplayNameInUse(It.IsAny<string>())).Returns(false);
			_userRegistrationRepository.Setup(x => x.IsEvalDeviceCode(It.IsAny<string>())).Returns(true);
			_userRegistrationRepository.Setup(x => x.GetSupportAccountIdsBySfAccountId(It.IsAny<string>(), It.IsAny<string>())).Returns(new long[] { supportAccountId });

			_evalTrackerRepository.Setup(x => x.GetSfAccountDataByEvalId(It.IsAny<string>())).Returns(new EvalTrackerData {
				DeviceCodes = new string[] { deviceCode },
				SfAccountId = null,
			});
			_salesforceService.Setup(x => x.GetSalesforceEvalDataByEvalId(It.IsAny<string>())).Returns(new SalesforceEvalData {
				SfAccountId = "012345678901234567",
			});

			//Run
			var userAccountLogic = new UserAccountLogic(_repository.Object, _userAccountRepository.Object, _lookupLogic.Object,
																									_membershipLogic.Object, _salesforceService.Object,
																									_membershipRepository.Object, _userRegistrationRepository.Object,
																									_evalTrackerRepository.Object, _supportAccountRepository.Object,
																									_supportAccountDomainsService.Object, _wildfireWipeoutService.Object, _encryptionProvider.Object, _lookupRepository.Object);
			var result = userAccountLogic.ValidateUserRegistration("test@test.test", "test", deviceCode, "custid");

			//Verify
			Assert.AreEqual("Error: Unable to create user account.  Please contact Support", result.ErrorMessage);
		}

		[TestMethod]
		public void RegistrationSfTest_TruncateMatch() {

			//Setup
			const string deviceCode = "deviceid";
			const long supportAccountId = 1337;
			_userRegistrationRepository.Setup(x => x.IsEmailInUse(It.IsAny<string>())).Returns(false);
			_userRegistrationRepository.Setup(x => x.IsDisplayNameInUse(It.IsAny<string>())).Returns(false);
			_userRegistrationRepository.Setup(x => x.IsEvalDeviceCode(It.IsAny<string>())).Returns(true);
			_userRegistrationRepository.Setup(x => x.GetSupportAccountIdsBySfAccountId(It.IsAny<string>(), It.IsAny<string>())).Returns(new long[] { supportAccountId });

			_evalTrackerRepository.Setup(x => x.GetSfAccountDataByEvalId(It.IsAny<string>())).Returns(new EvalTrackerData {
				DeviceCodes = new string[] { deviceCode },
				SfAccountId = "012345678901234",
			});
			_salesforceService.Setup(x => x.GetSalesforceEvalDataByEvalId(It.IsAny<string>())).Returns(new SalesforceEvalData {
				SfAccountId = "012345678901234567",
			});

			//Run
			var userAccountLogic = new UserAccountLogic(_repository.Object, _userAccountRepository.Object, _lookupLogic.Object,
																									_membershipLogic.Object, _salesforceService.Object,
																									_membershipRepository.Object, _userRegistrationRepository.Object,
																									_evalTrackerRepository.Object, _supportAccountRepository.Object,
																									_supportAccountDomainsService.Object, _wildfireWipeoutService.Object, _encryptionProvider.Object, _lookupRepository.Object);
			var result = userAccountLogic.ValidateUserRegistration("test@test.test", "test", deviceCode, "custid");

			//Verify
			Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
			Assert.AreEqual(supportAccountId, result.SupportAccountId);
		}

		[TestMethod]
		public void RegistrationSfTest_Truncate2Match() {

			//Setup
			const string deviceCode = "deviceid";
			const long supportAccountId = 1337;
			_userRegistrationRepository.Setup(x => x.IsEmailInUse(It.IsAny<string>())).Returns(false);
			_userRegistrationRepository.Setup(x => x.IsDisplayNameInUse(It.IsAny<string>())).Returns(false);
			_userRegistrationRepository.Setup(x => x.IsEvalDeviceCode(It.IsAny<string>())).Returns(true);
			_userRegistrationRepository.Setup(x => x.GetSupportAccountIdsBySfAccountId(It.IsAny<string>(), It.IsAny<string>())).Returns(new long[] { supportAccountId });

			_evalTrackerRepository.Setup(x => x.GetSfAccountDataByEvalId(It.IsAny<string>())).Returns(new EvalTrackerData {
				DeviceCodes = new string[] { deviceCode },
				SfAccountId = "012345678901234asfe23",
			});
			_salesforceService.Setup(x => x.GetSalesforceEvalDataByEvalId(It.IsAny<string>())).Returns(new SalesforceEvalData {
				SfAccountId = "012345678901234567",
			});

			//Run
			var userAccountLogic = new UserAccountLogic(_repository.Object, _userAccountRepository.Object, _lookupLogic.Object,
																									_membershipLogic.Object, _salesforceService.Object,
																									_membershipRepository.Object, _userRegistrationRepository.Object,
																									_evalTrackerRepository.Object, _supportAccountRepository.Object,
																									_supportAccountDomainsService.Object, _wildfireWipeoutService.Object, _encryptionProvider.Object, _lookupRepository.Object);
			var result = userAccountLogic.ValidateUserRegistration("test@test.test", "test", deviceCode, "custid");

			//Verify
			Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
			Assert.AreEqual(supportAccountId, result.SupportAccountId);
		}

		[TestMethod]
		public void RegistrationSfTest_TooShortMatch() {

			//Setup
			const string deviceCode = "deviceid";
			const long supportAccountId = 1337;
			_userRegistrationRepository.Setup(x => x.IsEmailInUse(It.IsAny<string>())).Returns(false);
			_userRegistrationRepository.Setup(x => x.IsDisplayNameInUse(It.IsAny<string>())).Returns(false);
			_userRegistrationRepository.Setup(x => x.IsEvalDeviceCode(It.IsAny<string>())).Returns(true);
			_userRegistrationRepository.Setup(x => x.GetSupportAccountIdsBySfAccountId(It.IsAny<string>(), It.IsAny<string>())).Returns(new long[] { supportAccountId });

			_evalTrackerRepository.Setup(x => x.GetSfAccountDataByEvalId(It.IsAny<string>())).Returns(new EvalTrackerData {
				DeviceCodes = new string[] { deviceCode },
				SfAccountId = "0123456",
			});
			_salesforceService.Setup(x => x.GetSalesforceEvalDataByEvalId(It.IsAny<string>())).Returns(new SalesforceEvalData {
				SfAccountId = "0123456",
			});

			//Run
			var userAccountLogic = new UserAccountLogic(_repository.Object, _userAccountRepository.Object, _lookupLogic.Object,
																									_membershipLogic.Object, _salesforceService.Object,
																									_membershipRepository.Object, _userRegistrationRepository.Object,
																									_evalTrackerRepository.Object, _supportAccountRepository.Object,
																									_supportAccountDomainsService.Object, _wildfireWipeoutService.Object, _encryptionProvider.Object, _lookupRepository.Object);
			var result = userAccountLogic.ValidateUserRegistration("test@test.test", "test", deviceCode, "custid");

			//Verify
			Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
			Assert.AreEqual(supportAccountId, result.SupportAccountId);
		}

		[TestMethod]
		public void RegistrationSfTest_TooShortNoMatch() {

			//Setup
			const string deviceCode = "deviceid";
			const long supportAccountId = 1337;
			_userRegistrationRepository.Setup(x => x.IsEmailInUse(It.IsAny<string>())).Returns(false);
			_userRegistrationRepository.Setup(x => x.IsDisplayNameInUse(It.IsAny<string>())).Returns(false);
			_userRegistrationRepository.Setup(x => x.IsEvalDeviceCode(It.IsAny<string>())).Returns(true);
			_userRegistrationRepository.Setup(x => x.GetSupportAccountIdsBySfAccountId(It.IsAny<string>(), It.IsAny<string>())).Returns(new long[] { supportAccountId });

			_evalTrackerRepository.Setup(x => x.GetSfAccountDataByEvalId(It.IsAny<string>())).Returns(new EvalTrackerData {
				DeviceCodes = new string[] { deviceCode },
				SfAccountId = "0123456",
			});
			_salesforceService.Setup(x => x.GetSalesforceEvalDataByEvalId(It.IsAny<string>())).Returns(new SalesforceEvalData {
				SfAccountId = "0123456f2323f2323233",
			});

			//Run
			var userAccountLogic = new UserAccountLogic(_repository.Object, _userAccountRepository.Object, _lookupLogic.Object,
																									_membershipLogic.Object, _salesforceService.Object,
																									_membershipRepository.Object, _userRegistrationRepository.Object,
																									_evalTrackerRepository.Object, _supportAccountRepository.Object,
																									_supportAccountDomainsService.Object, _wildfireWipeoutService.Object, _encryptionProvider.Object, _lookupRepository.Object);
			var result = userAccountLogic.ValidateUserRegistration("test@test.test", "test", deviceCode, "custid");

			//Verify
			Assert.AreEqual("Error: Unable to create user account.  Please contact Support", result.ErrorMessage);
		}
         *          * */
    }
}