using System;
using Csp.Api.Services.Contracts;

namespace Csp.Api.Model
{
    public class SiteLicenseItem
    {
        public long LicenseId { get; set; }
        public string AuthorizationCode { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public string ExpirationDateDisplay
        {
            get { return ExpirationDate.HasValue ? ExpirationDate.Value.ToShortDateString() : string.Empty; }
        }

        public bool IsExpired
        {
            get { return ExpirationDate.HasValue && ExpirationDate.Value < DateTime.Now; }
        }

        public string ExpirationDateCssClass
        {
            get { return IsExpired ? "expired-license" : string.Empty; }
        }

        public string AscName { get; set; }
        public long? AscId { get; set; }

        public bool IsAutofocus { get; set; }
        public bool IsEnabled { get; set; }

        public AfApiKeyStatuses ApiKeyState { get; set; }

        public string ApiKeyStatesString
        {
            get { return ApiKeyState.ToString(); }
        }

        public string Notes
        {
            get
            {
                if (MaxLicenses.HasValue && UsedLicenses.HasValue)
                {
                    var maxUseCount = MaxLicenses.Value == 999999 ? "Unlimited" : MaxLicenses.Value.ToString();
                    return string.Format("{0} / {1}", UsedLicenses.Value, maxUseCount);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public long? MaxLicenses { get; set; }
        public long? UsedLicenses { get; set; }

        public long? BundleLicenseId { get; set; }
        public string MainAuthCode { get; set; }

        public string DisplayAuthorizationCode
        {
            get
            {
                if (BundleLicenseId.HasValue && BundleLicenseId.Value > 0 && AuthorizationCode.StartsWith("A") && !string.IsNullOrEmpty(MainAuthCode))
                {
                    return MainAuthCode;
                }
                else
                {
                    return AuthorizationCode;
                }
            }
        }

        public long? ParentLicenseId { get; set; }

        /// <summary>
        /// THIS IS A PLACEHOLDER FIELD USED ONLY SO THE KENDO GRID CAN HAVE A NAMED COLUMN
        /// NEVER USE THIS FIELD READ OR WRITE FOR ANYTHING OTHER THAN SHOWING/HIDING THE
        /// KENDO GRID COLUMN
        /// </summary>
        public string Actions { get; private set; }
    }
}
