using System;
using Csp.Api.Logic;
using Csp.Api.Services.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Csp.Api.Tests.LogicTest {
	[TestClass]
	public class SupportAccountDomainsServiceTests {

		private readonly Mock<ISupportAccountDomainsRepository> _supportAccountDomainsRepository = new Mock<ISupportAccountDomainsRepository>();

		[TestMethod]
		public void DomainParsingTest() {

			//Setup
			const long supportAccountId = 245;
			const string email = "aaa.test@testdomain.com";
			const string domain = "testdomain.com";

			//Run test
			var supportAccountDomainsService = new SupportAccountDomainsService(_supportAccountDomainsRepository.Object);
			supportAccountDomainsService.CreateFirstSupportAccountDomain(supportAccountId, email);

			//Verify
			_supportAccountDomainsRepository.Verify(x => x.GetOrCreateDomain(domain), Times.Once);
		}

		[TestMethod]
		public void DomainParsingTest_InvalidEmail() {

			//Setup
			const long supportAccountId = 245;
			const string email = "aaa.testtestdomain.com";
			string exceptionMessage = null;

			//Run test
			var supportAccountDomainsService = new SupportAccountDomainsService(_supportAccountDomainsRepository.Object);
			try {
				supportAccountDomainsService.CreateFirstSupportAccountDomain(supportAccountId, email);
			} catch (Exception ex) {
				exceptionMessage = ex.Message;
			}

			//Verify
			_supportAccountDomainsRepository.Verify(x => x.GetOrCreateDomain(It.IsAny<string>()), Times.Never);
			Assert.AreEqual("Invalid email, can't parse domain: aaa.testtestdomain.com", exceptionMessage);
		}
	}
}
