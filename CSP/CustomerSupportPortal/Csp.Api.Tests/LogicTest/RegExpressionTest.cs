using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Csp.Api.Tests.LogicTest
{
    [TestClass]
    public class RegExpressionTest
    {
        [TestMethod]
        public void VmElaBundleRegText()
        {
            var regExp = new Regex("^PAN-VM-.+-ELA-BND2-(PREM|USG|BKLN|BKLNUSG).+YR");
            //PAN-VM-100-ELA-BND2-USG-1YR

            Assert.IsTrue(regExp.IsMatch("PAN-VM-100-ELA-BND2-USG-1YR"));
            Assert.IsTrue(regExp.IsMatch("PAN-VM-500-ELA-BND2-USG-1YR"));
            Assert.IsTrue(regExp.IsMatch("PAN-VM-700-ELA-BND2-USG-1YR"));
            Assert.IsTrue(regExp.IsMatch("PAN-VM-100-ELA-BND2-PREM-1YR"));
            Assert.IsTrue(regExp.IsMatch("PAN-VM-50-ELA-BND2-PREM-1YR"));
            Assert.IsTrue(regExp.IsMatch("PAN-VM-700-ELA-BND2-BKLN-3YR"));
            Assert.IsTrue(regExp.IsMatch("PAN-VM-700-ELA-BND2-BKLNUSG-3YR"));

            Assert.IsFalse(regExp.IsMatch("PAN-VM-500-ELA-1YR"));
            Assert.IsFalse(regExp.IsMatch("PAN-VM-100-ELA-3YR"));
            Assert.IsFalse(regExp.IsMatch("PAN-TRAPS-ELA-D-1YR"));
        }
    }
}
