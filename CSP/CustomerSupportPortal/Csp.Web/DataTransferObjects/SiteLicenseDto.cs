using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects
{
    public class SiteLicenseDto : DefaultDto
    {

        public SiteLicenseDto()
        {
            SiteLicenses = new List<SiteLicenseItem>();
        }

        public List<SiteLicenseItem> SiteLicenses;

        public long SupportAccountId { get; set; }

        public SiteLicenseActions? SiteLicenseAction { get; set; }
        public long? SiteLicenseActionLicenseId { get; set; }

        public string DisplayApiKey { get; set; }

        public bool HasEditAutoFocusApiKeyPerm { get; set; }

        public enum SiteLicenseActions
        {
            Enable,
            Disable,
            GetKey,
            Regenerate,
        }
    }
}