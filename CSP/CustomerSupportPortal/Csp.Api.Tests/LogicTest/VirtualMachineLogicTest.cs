using System;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Csp.Api.Tests.LogicTest {

	[TestClass]
	public class VirtualMachineLogicTest {

		private readonly Mock<IVirtualMachineDataAccess> _virtualMachineDataAccess = new Mock<IVirtualMachineDataAccess>();
		private readonly Mock<IDeviceRepository> _deviceRepository = new Mock<IDeviceRepository>();
		private readonly Mock<ILicenseLogic> _licenseLogic = new Mock<ILicenseLogic>();
		private readonly Mock<IDeviceLogic> _deviceLogic = new Mock<IDeviceLogic>();
		private readonly Mock<IUserRegistrationRepository> _userRegistrationRepository = new Mock<IUserRegistrationRepository>();
		private readonly Mock<ISupportAccountRepository> _supportAccountRepository = new Mock<ISupportAccountRepository>();
		private readonly Mock<IVmRegistrationRepository> _vmRegistrationRepository = new Mock<IVmRegistrationRepository>();
        private readonly Mock<ISiteLicenseRepository> _siteLicenseRepository = new Mock<ISiteLicenseRepository>();
        private readonly Mock<ISupportAccountLogic> _supportAccountLogic = new Mock<ISupportAccountLogic>();

		[TestMethod]
		public void AddVmAuthCodeTest_InvalidAuthCode() {
			//_virtualMachineDataAccess.Setup(x => )

			const string authCode = "";
			const long supportAccountId = 1337;
			const long userAccountId = 1;

			var virtualMachineLogic = new VirtualMachineLogic(_virtualMachineDataAccess.Object, _deviceRepository.Object, _licenseLogic.Object,
                _userRegistrationRepository.Object, _supportAccountRepository.Object, _deviceLogic.Object, _vmRegistrationRepository.Object,
                _siteLicenseRepository.Object, _supportAccountLogic.Object);
			var result = virtualMachineLogic.AddVmAuthCode(EVmType.Vm, authCode, supportAccountId, userAccountId);

			Assert.AreEqual(VirtualMachineLogic.VmAuthCodeMessages.EmptyAuthCode, result.ErrorMessage);
		}

		[TestMethod]
		public void AddVmAuthCodeTest_AlreadyRegistered() {

			const string authCode = "abc123";
			const long supportAccountId = 1337;
			const long userAccountId = 1;

			_virtualMachineDataAccess.Setup(x => x.IsAuthCodeAlreadyRegistered(EVmType.Vm, authCode)).Returns(true);

			var virtualMachineLogic = new VirtualMachineLogic(_virtualMachineDataAccess.Object, _deviceRepository.Object, _licenseLogic.Object, _userRegistrationRepository.Object,
                _supportAccountRepository.Object, _deviceLogic.Object, _vmRegistrationRepository.Object, _siteLicenseRepository.Object, _supportAccountLogic.Object);
			var result = virtualMachineLogic.AddVmAuthCode(EVmType.Vm, authCode, supportAccountId, userAccountId);

			Assert.AreEqual(VirtualMachineLogic.VmAuthCodeMessages.AlreadyInUse, result.ErrorMessage);
		}

		[TestMethod]
		public void AddVmAuthCodeTest_AuthCodeNotFound() {

			const string authCode = "abc123";
			const long supportAccountId = 1337;
			const long userAccountId = 1;

			_virtualMachineDataAccess.Setup(x => x.IsAuthCodeAlreadyRegistered(EVmType.Vm, authCode)).Returns(false);
			_virtualMachineDataAccess.Setup(x => x.GetAuthCodeDetails(EVmType.Vm, authCode)).Returns((VmAuthCodeDetails)null);

			var virtualMachineLogic = new VirtualMachineLogic(_virtualMachineDataAccess.Object, _deviceRepository.Object, _licenseLogic.Object, _userRegistrationRepository.Object,
                _supportAccountRepository.Object, _deviceLogic.Object, _vmRegistrationRepository.Object, _siteLicenseRepository.Object, _supportAccountLogic.Object);
			var result = virtualMachineLogic.AddVmAuthCode(EVmType.Vm, authCode, supportAccountId, userAccountId);

			Assert.AreEqual(VirtualMachineLogic.VmAuthCodeMessages.AuthCodeNotFound, result.ErrorMessage);
		}

		[TestMethod]
		public void AddVmAuthCodeTest_AuthCodeInUse() {

			const string authCode = "abc123";
			const long supportAccountId = 1337;
			const long userAccountId = 1;

			_virtualMachineDataAccess.Setup(x => x.IsAuthCodeAlreadyRegistered(EVmType.Vm, authCode)).Returns(false);
			_virtualMachineDataAccess.Setup(x => x.GetAuthCodeDetails(EVmType.Vm, authCode))
															 .Returns(new VmAuthCodeDetails {
																 IsActive = true,
																 RegistrationDate = new DateTime(2011, 1, 1)
															 });

			var virtualMachineLogic = new VirtualMachineLogic(_virtualMachineDataAccess.Object, _deviceRepository.Object, _licenseLogic.Object, _userRegistrationRepository.Object, _supportAccountRepository.Object,
                _deviceLogic.Object, _vmRegistrationRepository.Object, _siteLicenseRepository.Object, _supportAccountLogic.Object);
			var result = virtualMachineLogic.AddVmAuthCode(EVmType.Vm, authCode, supportAccountId, userAccountId);

			Assert.AreEqual(VirtualMachineLogic.VmAuthCodeMessages.AuthCodeAlreadyAssociated, result.ErrorMessage);
		}

		[TestMethod]
		public void AddVmAuthCodeTest_AuthNotForVm() {

			const string authCode = "abc123";
			const long supportAccountId = 1337;
			const long userAccountId = 1;

			_virtualMachineDataAccess.Setup(x => x.IsAuthCodeAlreadyRegistered(EVmType.Vm, authCode)).Returns(false);
			_virtualMachineDataAccess.Setup(x => x.GetAuthCodeDetails(EVmType.Vm, authCode))
															 .Returns(new VmAuthCodeDetails {
																 IsActive = true,
																 RegistrationDate = null,
																 VmStubDeviceId = null,
																 ExpirationDate = null,
																 PanDbIdentification = "notvm"
															 });

			var virtualMachineLogic = new VirtualMachineLogic(_virtualMachineDataAccess.Object, _deviceRepository.Object, _licenseLogic.Object, _userRegistrationRepository.Object, _supportAccountRepository.Object,
                _deviceLogic.Object, _vmRegistrationRepository.Object, _siteLicenseRepository.Object, _supportAccountLogic.Object);
			var result = virtualMachineLogic.AddVmAuthCode(EVmType.Vm, authCode, supportAccountId, userAccountId);

			Assert.AreEqual(VirtualMachineLogic.VmAuthCodeMessages.NotAVmDevice, result.ErrorMessage);
		}

		[TestMethod]
		public void AddVmAuthCodeTest_ValidAuthCode() {

			const string authCode = "abc123";
			const long supportAccountId = 1337;
			const long userAccountId = 1;
			const string newSerialNumber = "zyx456";
			var shipDate = new DateTime(2013, 05, 01);

			_virtualMachineDataAccess.Setup(x => x.IsAuthCodeAlreadyRegistered(EVmType.Vm, authCode)).Returns(false);
			_virtualMachineDataAccess.Setup(x => x.GetAuthCodeDetails(EVmType.Vm, authCode))
															 .Returns(new VmAuthCodeDetails {
																 IsActive = true,
																 RegistrationDate = null,
																 VmStubDeviceId = null,
																 ExpirationDate = null,
																 PanDbIdentification = "phoenix123"
															 });
			_virtualMachineDataAccess.Setup(x => x.GetNewStubSn(EVmType.Vm)).Returns(newSerialNumber);
			_virtualMachineDataAccess.Setup(x => x.GetShipDate(EVmType.Vm, authCode)).Returns(shipDate);


			var virtualMachineLogic = new VirtualMachineLogic(_virtualMachineDataAccess.Object, _deviceRepository.Object, _licenseLogic.Object, _userRegistrationRepository.Object, _supportAccountRepository.Object,
                _deviceLogic.Object, _vmRegistrationRepository.Object, _siteLicenseRepository.Object, _supportAccountLogic.Object);
			var result = virtualMachineLogic.AddVmAuthCode(EVmType.Vm, authCode, supportAccountId, userAccountId);


			Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
			_virtualMachineDataAccess.Verify(x => x.GetNewStubSn(EVmType.Vm), Times.Once);
			_virtualMachineDataAccess.Verify(x => x.GetShipDate(EVmType.Vm, authCode), Times.Once);
			_virtualMachineDataAccess.Verify(x => x.CreateStubDevice(EVmType.Vm, authCode, newSerialNumber, shipDate, supportAccountId, userAccountId, false), Times.Once);
		}
	}
}
