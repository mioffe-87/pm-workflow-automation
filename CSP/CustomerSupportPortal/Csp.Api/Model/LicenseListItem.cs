using System;
using System.Collections.Generic;
using Csp.Api.Model.Attributes;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.Data;
using System.Configuration;

namespace Csp.Api.Model {

	public class LicenseListItem : AuditCspModel {

		[DataAccess(LicenseListItemFields.LicenseId, DataType = DbType.Int64)]
		public Int64 Id {
			get;
			set;
		}

		//This was added after NHibernate was introduced and is only used there so no DataAccess attribute is added
		public string AuthorizationCode { get; set; }


		[DataAccess(LicenseListItemFields.LicenseName)]
		public string LicenseName {
			get;
			set;
		}

		private string _licenseFeaturesName;
		[DataAccess(LicenseListItemFields.LicenseFeaturesName)]
		public string LicenseFeaturesName {
			get { return _licenseFeaturesName; }
			set {
				if (value == null)
					value = "";
				if (value.Length > 1) {
					// Clean up URL
					if (value.Contains("url"))
						value = value.Replace("url", "URL");
					// Otherwise capitalize the first char
					else {
						var firstChar = value.Substring(0, 1);
						var restOfValue = value.Substring(1, value.Length - 1);
						value = firstChar.ToUpper() + restOfValue;
					}
				}
				_licenseFeaturesName = value;
			}
		}

		public string LicenseNameDisplay {
			get {
				var isSupportLicense = LicenseFeaturesName != null && LicenseFeaturesName.ToLower().Trim().Equals("support");

				if (isSupportLicense) {
					return (LicenseName + " " + LicenseFeaturesName).Trim();
				}

				if (string.IsNullOrEmpty(LicenseName)) {
					return (LicenseFeaturesName ?? string.Empty).Trim();
				}

				// Start

				#region Cirro Secure change

				// For cirro secure, we need to show the total number of users as part of the license we display in CSP Device/License grid.

				string cirroSecureIdentification = ConfigurationManager.AppSettings["ApertureLicenseIdentification"];
				bool isCirroSecure = false;
				var aCirroIden = cirroSecureIdentification.Split(',');

				if (Pandbidentification != null && VMUseCount > 0) {
					foreach (string iden in aCirroIden) {
						if (Pandbidentification.ToUpper().Contains(iden)) {
							isCirroSecure = true;
							break;
						}
					}
				}

				if (isCirroSecure) {
					return (LicenseName + " for " + VMUseCount + " users").Trim();
				}

				#endregion

				// End
				return LicenseName;

			}
		}

		[DataAccess(LicenseListItemFields.EffectiveDate, DataType = DbType.DateTime)]
		public DateTime? EffectiveDate {
			get;
			set;
		}

		[DataAccess(LicenseListItemFields.ExpirationDate, DataType = DbType.DateTime)]
		public DateTime? ExpirationDate {
			get;
			set;
		}


		[DataAccess(LicenseListItemFields.HasLicenseKey, DataType = DbType.Int32)]
		public bool HasLicenseKey {
			get;
			set;
		}


		[DataAccess(LicenseListItemFields.ModelName)]
		public string ModelName {
			get;
			set;
		}

		/// <summary>
		/// It must
		/// 1) Have a license key
		/// 2) Either Is Perpetual or not expired
		/// </summary>
		public bool AllowDownload {
			get {
				return HasLicenseKey && (IsPerpetual || (ExpirationDate.HasValue && ExpirationDate.Value >= DateTime.Now));
			}
		}

		[DataAccess(LicenseListItemFields.IsPerpetual, DataType = DbType.Int32)]
		public bool IsPerpetual {
			get;
			set;
		}

		public bool IsTrial {
			get {
				if (string.IsNullOrEmpty(ModelName)) return false;

				return ModelName.ToLower().EndsWith("-trial");
			}
		}

		public long? SupportCenterSupportAccountId { get; set; }
		public string SupportCenterSupportAccountName { get; set; }


		public int VMUseCount { get; set; }

		public string Pandbidentification { get; set; }

	    public bool CsspProvisionedDevice { get; set; }

        public string TransactionId { get; set; }

        public long MasterAuthCodeLicId { get; set; }

        public int? SaasQuantity { get; set; }
        public bool ShowMagicLink
        {
            get
            {
                return !AllowDownload && !string.IsNullOrEmpty(MagicLink);
            }
        }

        public string MagicLink { get; set; }
    }

    public class DeviceLicenseListItemGroup {
		public long DeviceId { get; set; }
		public List<LicenseListItem> Licenses { get; set; }
	}
}
