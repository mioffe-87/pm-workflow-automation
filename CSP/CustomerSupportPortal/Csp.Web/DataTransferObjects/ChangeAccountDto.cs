using System.Collections.Generic;
using Csp.Api.Services.Contracts;

namespace Csp.Web.DataTransferObjects {

	public class ChangeAccountDto : DefaultDto {

        public IEnumerable<SelectableAccount> Accounts { 
            get; 
            set; 
        }

	    public bool IsRedirect {
	        get; 
            set;
        }

        public string TargetResource {
	        get; 
            set;
        }

        public bool IsUserAcctValid
        {
            get;
            set;
        }
	}
}