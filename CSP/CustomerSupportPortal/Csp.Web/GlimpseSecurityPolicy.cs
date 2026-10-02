using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Glimpse.AspNet.Extensions;
using Glimpse.Core.Extensibility;

namespace Csp.Web {
	public class GlimpseSecurityPolicy : IRuntimePolicy {
		public RuntimePolicy Execute(IRuntimePolicyContext policyContext) {
			// You can perform a check like the one below to control Glimpse's permissions within your application.
            // More information about RuntimePolicies can be found at http://getglimpse.com/Help/Custom-Runtime-Policy

#if DEBUG
            return RuntimePolicy.On;
#endif

#pragma warning disable
            var httpContext = policyContext.GetHttpContext();
			if (!GetGlimpseUsers().Contains(httpContext.User.Identity.Name)) {
				return RuntimePolicy.Off;
            }
#pragma warning restore
            return RuntimePolicy.On;
		}

		private IEnumerable<string> GetGlimpseUsers() {
			return
				ConfigurationManager.AppSettings["GlimpseUsers"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
																												.Select(x => x + "@paloaltonetworks.com");
		}

		public RuntimeEvent ExecuteOn {
			// The RuntimeEvent.ExecuteResource is only needed in case you create a security policy
			// Have a look at http://blog.getglimpse.com/2013/12/09/protect-glimpse-axd-with-your-custom-runtime-policy/ for more details
			get { return RuntimeEvent.EndRequest | RuntimeEvent.ExecuteResource; }
		}
	}
}
