using System;
using Csp.Api.Logic;
using Csp.Api.Services.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Csp.Api.Tests.LogicTest
{
    [TestClass]
    public class CsspEnduserInfoTest
    {
        private readonly Mock<IDpmRepository> _dmpRepository = new Mock<IDpmRepository>();
        private readonly Mock<IDeviceRepository> _devRepository = new Mock<IDeviceRepository>();
        private readonly Mock<ILicenseRepository> _licRepository = new Mock<ILicenseRepository>();
        private readonly Mock<IUserAccountLogic> _userAccount = new Mock<IUserAccountLogic>();
        [TestMethod]
        public void ReportEndUserInfo()
        {
            var lic = new LicenseLogic(null,null,null,null,null, null);
            var deviceIds = new long[1];
            deviceIds[0] = 2197133;
            lic.ReportCsspEndUserInfo(deviceIds, csspSupportAccountId: 961337, customerAccountId: "879305",
                companyName: "VISIONBank", dnbNumber: null, address: "3000 25TH ST S", country: "US",
                region: "North Dakota", city: "FARGO", postalCode: "58103",
                endUserContactEmail: "help@consolidated.com", phoneNumber: "(612) 555-1212", website: null,
                industry: null, createdBy: "Chris.Halverson@consolidated.com");
        }
    }
}
