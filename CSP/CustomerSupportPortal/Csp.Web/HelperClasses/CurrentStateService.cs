﻿using System.Web;
using Csp.Api.Model;
using Csp.Api.Security;
using Csp.Api.Services.Contracts;

namespace Csp.Web.HelperClasses {
	public class CurrentStateService : ICurrentStateService {
		public void SetSupportAccount(SupportAccount supportAccount) {
			CurrentState.SetSupportAccount(supportAccount);
		}

		public void SetGroup(Group group) {
			CurrentState.SetGroup(group);
		}

		public void SetDevice(Device device) {
			CurrentState.SetDevice(device);
		}

		public long? GetSupportAccountId() {
			return CurrentState.GetSupportAccountId();
		}

		public CspPrincipal GetCurrentUser() {
			var cspPrincipal = HttpContext.Current.User as CspPrincipal;
			return cspPrincipal;
		}

        public bool GetIsImpersonated() {
            return CurrentState.GetIsImpersonated();
        }

    }
}