using Csp.Api.Logic;
using Csp.Api.Services.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Csp.Api.Tests.LogicTest {
	[TestClass]
	public class WildfireWipeoutServiceTests {

		private readonly Mock<IWildfireWipeoutWebService> _wildfireWipeoutWebService = new Mock<IWildfireWipeoutWebService>();
		private readonly Mock<IApplicationSettings> _applicationSettings = new Mock<IApplicationSettings>();
		private readonly Mock<IEmailHelper> _emailHelper = new Mock<IEmailHelper>();
		private readonly Mock<IWildfireRepository> _wildfireRepository = new Mock<IWildfireRepository>();
		private readonly Mock<IDeviceDataAccess> _deviceDataAccess = new Mock<IDeviceDataAccess>();

		[TestMethod]
		public void AllSuccess() {

			//Setup
			const string serialNumber = "SN0000001";
			const int runTime = 1500;
			const string url = "http://www.google.com/";
			_wildfireWipeoutWebService.Setup(x => x.WildFireWipeout(serialNumber)).Returns(new[]
				{
					new ApiResult {IsSuccess = true, RunTimeMilliseconds = runTime, Data = serialNumber, ServiceUrl = url},
					new ApiResult {IsSuccess = true, RunTimeMilliseconds = runTime, Data = serialNumber, ServiceUrl = url}
				});


			//Run
			var wildfireWipeoutService = new WildfireWipeoutService(_wildfireWipeoutWebService.Object, _applicationSettings.Object, _emailHelper.Object, _wildfireRepository.Object, _deviceDataAccess.Object);
			var result = wildfireWipeoutService.WildFireWipeout(serialNumber);


			//Verify
			//Verify overall this is a success
			Assert.IsTrue(result);
			//Verify there are only 2 calls of ANY type
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
			//Verify both calls are the expected call
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(serialNumber, url, runTime, "Success", null), Times.Exactly(2));
		}

		[TestMethod]
		public void AllFailure() {

			//Setup
			const string serialNumber = "SN0000001";
			const int runTime = 1500;
			const string url = "http://www.google.com/";
			_wildfireWipeoutWebService.Setup(x => x.WildFireWipeout(serialNumber)).Returns(new[]
				{
					new ApiResult {IsSuccess = false, RunTimeMilliseconds = runTime, Data = serialNumber, ServiceUrl = url, ResponseStatusCode = "FailureX"},
					new ApiResult {IsSuccess = false, RunTimeMilliseconds = runTime, Data = serialNumber, ServiceUrl = url, ResponseStatusCode = "FailureY"}
				});


			//Run
			var wildfireWipeoutService = new WildfireWipeoutService(_wildfireWipeoutWebService.Object, _applicationSettings.Object, _emailHelper.Object, _wildfireRepository.Object, _deviceDataAccess.Object);
			var result = wildfireWipeoutService.WildFireWipeout(serialNumber);


			//Verify
			//Verify overall this is a failure
			Assert.IsFalse(result);
			//Verify there are only 2 calls of ANY type
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
			//Verify both calls are the expected call
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(serialNumber, url, runTime, "FailureX - Invalid Status Code", null), Times.Once);
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(serialNumber, url, runTime, "FailureY - Invalid Status Code", null), Times.Once);
		}

		[TestMethod]
		public void AllErrors() {

			//Setup
			const string serialNumber = "SN0000001";
			const int runTime = 1500;
			const string url = "http://www.google.com/";
			const string emailTo = "emailTo@blah.com";
			_applicationSettings.Setup(x => x.WildfireErrorMailTo).Returns(emailTo);
			_wildfireWipeoutWebService.Setup(x => x.WildFireWipeout(serialNumber)).Returns(new[]
				{
					new ApiResult {IsSuccess = false, RunTimeMilliseconds = runTime, Data = serialNumber, ServiceUrl = url, ErrorMessage = "ErrorX"},
					new ApiResult {IsSuccess = false, RunTimeMilliseconds = runTime, Data = serialNumber, ServiceUrl = url, ErrorMessage = "ErrorY"}
				});


			//Run
			var wildfireWipeoutService = new WildfireWipeoutService(_wildfireWipeoutWebService.Object, _applicationSettings.Object, _emailHelper.Object, _wildfireRepository.Object, _deviceDataAccess.Object);
			var result = wildfireWipeoutService.WildFireWipeout(serialNumber);


			//Verify
			//Verify overall this is a failure
			Assert.IsFalse(result);
			//Verify there are only 2 calls of ANY type
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
			//Verify both calls are the expected call
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(serialNumber, url, runTime, "Exception", "ErrorX"), Times.Once);
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(serialNumber, url, runTime, "Exception", "ErrorY"), Times.Once);
			//Verify both errors are logged
			_emailHelper.Verify(x => x.SendGeneralMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
			_emailHelper.Verify(x => x.SendGeneralMail(emailTo, "Wildfire Integration Error.", "Details: ErrorX"), Times.Once);
			_emailHelper.Verify(x => x.SendGeneralMail(emailTo, "Wildfire Integration Error.", "Details: ErrorY"), Times.Once);
		}

		[TestMethod]
		public void MixedSuccessAndFailure() {

			//Setup
			const string serialNumber = "SN0000001";
			const int runTime = 1500;
			const string url = "http://www.google.com/";
			_wildfireWipeoutWebService.Setup(x => x.WildFireWipeout(serialNumber)).Returns(new[]
				{
					new ApiResult {IsSuccess = true, RunTimeMilliseconds = runTime, Data = serialNumber, ServiceUrl = url},
					new ApiResult {IsSuccess = false, RunTimeMilliseconds = runTime, Data = serialNumber, ServiceUrl = url, ResponseStatusCode = "FailureZ"}
				});


			//Run
			var wildfireWipeoutService = new WildfireWipeoutService(_wildfireWipeoutWebService.Object, _applicationSettings.Object, _emailHelper.Object, _wildfireRepository.Object, _deviceDataAccess.Object);
			var result = wildfireWipeoutService.WildFireWipeout(serialNumber);


			//Verify
			//Verify overall this is a failure
			Assert.IsFalse(result);
			//Verify there are only 2 calls of ANY type
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
			//Verify both calls are the expected call
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(serialNumber, url, runTime, "Success", null), Times.Once);
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(serialNumber, url, runTime, "FailureZ - Invalid Status Code", null), Times.Once);
		}

		[TestMethod]
		public void MixedSuccessAndError() {

			//Setup
			const string serialNumber = "SN0000001";
			const int runTime = 1500;
			const string url = "http://www.google.com/";
			const string emailTo = "emailTo@blah.com";
			_applicationSettings.Setup(x => x.WildfireErrorMailTo).Returns(emailTo);
			_wildfireWipeoutWebService.Setup(x => x.WildFireWipeout(serialNumber)).Returns(new[]
				{
					new ApiResult {IsSuccess = false, RunTimeMilliseconds = runTime, Data = serialNumber, ServiceUrl = url, ErrorMessage = "ErrorZ"},
					new ApiResult {IsSuccess = true, RunTimeMilliseconds = runTime, Data = serialNumber, ServiceUrl = url}
				});


			//Run
			var wildfireWipeoutService = new WildfireWipeoutService(_wildfireWipeoutWebService.Object, _applicationSettings.Object, _emailHelper.Object, _wildfireRepository.Object, _deviceDataAccess.Object);
			var result = wildfireWipeoutService.WildFireWipeout(serialNumber);


			//Verify
			//Verify overall this is a failure
			Assert.IsFalse(result);
			//Verify there are only 2 calls of ANY type
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
			//Verify both calls are the expected call
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(serialNumber, url, runTime, "Exception", "ErrorZ"), Times.Once);
			_deviceDataAccess.Verify(x => x.LogWipeOutNFRWildfire(serialNumber, url, runTime, "Success", null), Times.Once);
			//Verify both errors are logged
			_emailHelper.Verify(x => x.SendGeneralMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			_emailHelper.Verify(x => x.SendGeneralMail(emailTo, "Wildfire Integration Error.", "Details: ErrorZ"), Times.Once);
		}
	}
}
