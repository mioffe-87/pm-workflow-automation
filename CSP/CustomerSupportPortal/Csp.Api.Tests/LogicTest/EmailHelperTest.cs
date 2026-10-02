using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csp.Api.HelperClasses;
using System.Diagnostics;

namespace Csp.Api.Tests.LogicTest
{
    [TestClass]
    public class EmailHelperTest
    {
        [TestMethod]
        public void SendUserActivationEmailTest()
        {
            EmailHelper.SendUserActivationEmail("nviswanath@paloaltonetworks.com", "Neal Viswanath", "test");
            Assert.IsTrue(true);
        }

    }
}
