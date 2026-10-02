﻿using System;
using System.Security.Principal;
using System.Web;
using Csp.Api.Model;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Logic {
	public class TestLogic : ITestLogic {

		private readonly ISecurityLogic _securityLogic;
		private readonly IPermissionService _permissionService;

		public TestLogic(ISecurityLogic securityLogic, IPermissionService permissionService) {
			_securityLogic = securityLogic;
			_permissionService = permissionService;
		}

		public void SetupUserPrincipal(long supportAccountId, string email) {
			var cspPrincipal = _securityLogic.GetCspPrincipal(email);
			cspPrincipal.Identity = new GenericIdentity(email);
			HttpContext.Current.Cache.Insert(email, cspPrincipal, null, DateTime.UtcNow.AddHours(8), System.Web.Caching.Cache.NoSlidingExpiration);
			_permissionService.UpdateCurrentStateAndPermissions(cspPrincipal, "supportaccount", "test", supportAccountId.ToString(), new SupportAccount(), new Group(), new Device());
			HttpContext.Current.User = cspPrincipal;
		}
	}
}
