using Csp.Api.DataAccess;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Csp.Infrastructure.Data.Tests.DBSetup;
using Csp.Infrastructure.Data.Tests.DBSetup.Seed;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Csp.Infrastructure.Data.Tests {
	[TestClass]
	public class VirtualMachineDataAccessTests : SafeDbTest {

		private readonly Mock<ISupportAccountRepository> _mockSupportAccountRepository = new Mock<ISupportAccountRepository>();
		private readonly Mock<IDeviceRepository> _mockDeviceRepository = new Mock<IDeviceRepository>();

		[TestInitialize]
		public void SetupTests() {
			ReSeedData();
		}

		//[TestMethod]
		//public void GetAvailableVms_Simple() {

		//  VmData.Seed();

		//  var dataParameters = new DataParameters();
		//  const int supportAccountId = 0;
		//  const string searchAuthCode = "";

		//  var virtualMachineDataAccess = new VirtualMachineDataAccess(_mockSupportAccountRepository.Object, _mockDeviceRepository.Object);


		//  var result = virtualMachineDataAccess.GetAvailableVms(dataParameters, supportAccountId, searchAuthCode);


		//  Assert.Fail();
		//}
	}
}
