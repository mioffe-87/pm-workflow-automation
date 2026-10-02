using System;
using System.Collections.Generic;
using System.Linq;
using Csp.Api.Logic;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Csp.Api.Tests.LogicTest {

	[TestClass]
	public class PermissionServiceTests {

		private readonly Mock<ISupportAccountLogic> _supportAccountLogic = new Mock<ISupportAccountLogic>();
		private readonly Mock<IGroupLogic> _groupLogic = new Mock<IGroupLogic>();
		private readonly Mock<ISecurityLogic> _securityLogic = new Mock<ISecurityLogic>();
		private readonly Mock<ICurrentStateService> _currentStateService = new Mock<ICurrentStateService>();
        private readonly Mock<ISupportAccountUserLogic> _supportAccountUserLogic = new Mock<ISupportAccountUserLogic>();

		[TestMethod]
		public void SupportAccount_Simple() {

			var user = new CspPrincipal { UserAccount = new UserAccount { Id = 1 } };
			const string controller = "supportaccount";
			const string action = "";
			const string id = "1";
			var supportAccount = new SupportAccount();
			var group = new Group();
			var device = new Device();

			var expectedPerms = new HashSet<Permission> { Permission.ViewSupportAccount, Permission.ViewSupportAccountDevices };
			_securityLogic.Setup(x => x.GetSupportAccountPermissionsForPrincipal(Convert.ToInt64(id), user.UserAccount)).Returns(expectedPerms);


			var permissionService = new PermissionService(_supportAccountLogic.Object, _groupLogic.Object, _securityLogic.Object, _currentStateService.Object, _supportAccountUserLogic.Object);
			permissionService.UpdateCurrentStateAndPermissions(user, controller, action, id, supportAccount, group, device);



			Assert.AreEqual(Convert.ToInt64(id), user.SupportAccountIdForPermissionSet);
			Assert.AreEqual(null, user.GroupIdForPermissionSet);
			Assert.IsTrue(TestHelper.AreEqualPermissions(expectedPerms, user.ActivePermissions));
		}

		[TestMethod]
		public void SupportAccount_OddCapitalization() {

			var user = new CspPrincipal { UserAccount = new UserAccount { Id = 1 } };
			const string controller = "SupporTAccOUnt";
			const string action = "";
			const string id = "1";
			var supportAccount = new SupportAccount();
			var group = new Group();
			var device = new Device();

			var expectedPerms = new HashSet<Permission> { Permission.ViewSupportAccount, Permission.ViewSupportAccountDevices };
			_securityLogic.Setup(x => x.GetSupportAccountPermissionsForPrincipal(Convert.ToInt64(id), user.UserAccount)).Returns(expectedPerms);


			var permissionService = new PermissionService(_supportAccountLogic.Object, _groupLogic.Object, _securityLogic.Object, _currentStateService.Object, _supportAccountUserLogic.Object);
			permissionService.UpdateCurrentStateAndPermissions(user, controller, action, id, supportAccount, group, device);



			Assert.AreEqual(Convert.ToInt64(id), user.SupportAccountIdForPermissionSet);
			Assert.AreEqual(null, user.GroupIdForPermissionSet);
			Assert.IsTrue(TestHelper.AreEqualPermissions(expectedPerms, user.ActivePermissions));
		}
	}

	public static class TestHelper {
		public static bool AreEqualPermissions(IEnumerable<Permission> expected, IEnumerable<Permission> actual) {
			return expected.OrderBy(x => x).SequenceEqual(actual.OrderBy(x => x));
		}
	}
}
