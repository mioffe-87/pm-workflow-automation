using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;
using Moq;

namespace Csp.Api.Tests.LogicTest
{
    [TestClass]
    public class EndPointLogicTest
    {
        private readonly Mock<IEPDataAccessLogic> _epDataAccessLogic = new Mock<IEPDataAccessLogic>();
        private readonly Mock<ILicenseLogic> _licLogic = new Mock<ILicenseLogic>();
		private readonly Mock<IDeviceLogic> _devLogic = new Mock<IDeviceLogic>();
		private readonly Mock<IEpRepository> _epRepository = new Mock<IEpRepository>();
		private readonly Mock<ICurrentStateService> _stateService = new Mock<ICurrentStateService>();
        private readonly Mock<IConnectionStrings> _connectionString = new Mock<IConnectionStrings>();

        [TestMethod]
        public void GenerateLicense()
        {
            var lic = new EpLogic(_epDataAccessLogic.Object, _licLogic.Object, _devLogic.Object, _epRepository.Object, _stateService.Object, _connectionString.Object);
            DateTime expirationDate = new DateTime();
            string customerName = string.Empty;
            string agent = new Guid().ToString();
            lic.GenerateLicense(expirationDate, customerName,100, 100,100,0, agent,string.Empty,  false);
        }
    }
}
