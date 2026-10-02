using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.DataTransferObjects
{
    public class AccountInfoDto : DefaultDto {
        public long? PartialId { get; set; }
        public AccountInfoTabs SelectedTab { get; set; }
    }

    public enum AccountInfoTabs {
        Home,
        AscPreferences
    }
}