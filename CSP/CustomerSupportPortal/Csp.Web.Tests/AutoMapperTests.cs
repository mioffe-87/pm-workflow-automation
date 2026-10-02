using AutoMapper;
using Csp.Web.App_Start;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Csp.Web.Tests {

	[TestClass]
	public class AutoMapperTests {

		public AutoMapperTests() {
			AutoMapperConfig.Configure();
		}

		[TestMethod]
		public void AutoMapperConfigIsValid() {
			Mapper.AssertConfigurationIsValid();
		}
	}
}
