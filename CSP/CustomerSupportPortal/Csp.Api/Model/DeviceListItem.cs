using System;
using System.Collections.Generic;
using System.Linq;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Attributes;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.Data;
using LINQtoCSV;

namespace Csp.Api.Model {

	public class DeviceListItem : BaseCspModel {

		public DeviceListItem() {
			Licenses = new LicenseListItem[0];
			Group = string.Empty;
		}


        
		[DataAccess(DeviceListItemFields.DeviceId, DataType = DbType.Int64, UseForCreate = false, UseForUpdate = false)]
		public long DeviceId {
			get;
			set;
		}


		[DataAccess(DeviceListItemFields.SerialNumber, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
		public string SerialNumber {
			get;
			set;
		}


		[DataAccess(DeviceListItemFields.DeviceName, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
		public string DeviceName {
			get;
			set;
		}


		[DataAccess(DeviceListItemFields.ModelName, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
		public string ModelName {
			get;
			set;
		}

        public string PanOsVersion { get; set; }
        public string HyperVisorVersion { get; set; }
        public bool IsAwsGovCloud { get; set; }
		public string Group { get; set; }

		public string License {
			get {
				var licenseNames =
					(
						from l in Licenses
						select "<div class='licenserow'>" + l.LicenseNameDisplay + AddDownloadClass(l) + AddTrialNotification(l) + "</div>"
					).ToArray();

				return string.Join("\n", licenseNames);
			}
		}

		private string AddDownloadClass(LicenseListItem l)
		{

			if (string.IsNullOrEmpty(l.ModelName) == false && l.ModelName.ToUpper().Contains("AWS"))
				return string.Empty;

            string magicLink = string.Empty;
            if (l.ShowMagicLink)
                magicLink = string.Format(
                "<i class='fa-exclamation-triangle fas forMagicLink' style='margin-left: 5px; color: red'></i><div id='#divMagicLink' style='visibility:hidden;' >{0} setup required. <br><a class='hrefMagicLink' href='{1}' target='_blank'>Click here</a> to setup.</div>",
                l.LicenseNameDisplay,
                l.MagicLink);

            return
				l.AllowDownload
					? string.Format(
						"<a class='k-grid-csp-download' style='margin-left: 5px;' href='/License/DownloadLicenseKey?licenseId={0}' title='Download {1} License Key'><span> </span></a>",
						l.Id,
						l.LicenseNameDisplay)
                    : magicLink;
		}

		private string AddTrialNotification(LicenseListItem l) {

			return
				l.IsTrial
					? string.Format(
						"<span class='TrialLogo' title='Trial License'>T</span>",
						l.Id)
					: string.Empty;
		}

		public string AuthCode {
			get
			{
			    var authCodesHtml = new List<string>();
			    if (Licenses != null && Licenses.Any())
			    {
			        foreach (var license in Licenses)
			        {
			            if (license.MasterAuthCodeLicId > 0)
			            {
                            authCodesHtml.Add(string.Format("<a href=\"javascript: showMasterAuthCodeInfo({0}, '{1}', '{2}', '{3}')\"> {4} </a>",
                                license.MasterAuthCodeLicId, SerialNumber, ModelName, license.AuthorizationCode, license.AuthorizationCode));
			            }
			            else
    			            authCodesHtml.Add(string.Format("<div class='licenserow'> {0} </div>", license.AuthorizationCode));
			        }			        
			    }
			    var authCodes = authCodesHtml.ToArray();
				return string.Join("\n", authCodes);
			}
		}

		public string ExpirationDate {
			get {
				var dates =
					(
						from l in Licenses
						select "<div class='licenserow " + GetExpirationWarningClass(l) + "'>" + GetExpirationText(l) + "</div>"
					).ToArray();

				return string.Join("\n", dates);
			}
		}

		private const int NumberOfDaysToSubtract = 31;
		private const string ExpirationError = "expired-license";
		private const string ExpirationWarning = "expiration-warning-License";

		private string GetExpirationWarningClass(LicenseListItem l) {

			if (!l.ExpirationDate.HasValue || l.IsPerpetual) return string.Empty;

			var currentDate = DateTime.Now.Date;

			var isError = l.ExpirationDate.Value < currentDate;
			if (isError) return ExpirationError;

			var isWarning = l.ExpirationDate.Value.AddDays(-1 * NumberOfDaysToSubtract) < currentDate;
			if (isWarning) return ExpirationWarning;

			return string.Empty;
		}

		private string GetExpirationText(LicenseListItem l) {
		    
			if (l.LicenseNameDisplay == "Lab Bundle") return string.Empty;

            if (l.CsspProvisionedDevice) return "Pay Per Use"; 

			if (l.IsPerpetual) return "Perpetual";

			return (l.ExpirationDate.HasValue ? l.ExpirationDate.Value.ToShortDateString() : string.Empty);
		}

		public string DeviceTransferMessage { get; set; }

		public string DeviceTag { get; set; }

		[DataAccess("DeviceCategoryId", DataType = DbType.Int64, UseForCreate = false, UseForUpdate = false)]
		public long? DeviceCategoryId { get; set; }

		private string _comments;
		public string Comments {
			get { return _comments; }
			set { _comments = ApiCommon.DecodeString(value); }
		}

		[DataAccess(DeviceListItemFields.Comments, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
		public string CommentsEncoded {
			set { Comments = value; }
			get { return ApiCommon.EncodeString(Comments); }
		}

        [DataAccess(DeviceListItemFields.Address1, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
        public string Address1
        {
            get;
            set;
        }
        [DataAccess(DeviceListItemFields.Address2, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
        public string Address2
        {
            get;
            set;
        }

        [DataAccess(DeviceListItemFields.City, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
		public string City {
			get;
			set;
		}

		[DataAccess(DeviceListItemFields.RegionState, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
		public string RegionState {
			get;
			set;
		}

		[DataAccess(DeviceListItemFields.PostalCode, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
		public string PostalCode {
			get;
			set;
		}

		[DataAccess(DeviceListItemFields.CountryId, DataType = DbType.Int64, UseForCreate = false, UseForUpdate = false)]
		public long? CountryId {
			get;
			set;
		}

		public string Country { get; set; }

		private long? _ascId;
		public long? ASCId {
			get {
				var licenseListItem = GetLicenseListItem;

				if (licenseListItem != null) {
					_ascId = licenseListItem.SupportCenterSupportAccountId;
				}

				return _ascId;
			}
			set { _ascId = value; }
		}

		private string _ascName;
		[DataAccess(DeviceListItemFields.ASCName, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
		public string ASCName {
			get {
				var licenseListItem = GetLicenseListItem;

				if (licenseListItem != null) {
					_ascName = licenseListItem.SupportCenterSupportAccountName;
				}

				return _ascName;
			}
			set { _ascName = ApiCommon.DecodeString(value); }
		}

		public IEnumerable<LicenseListItem> Licenses { get; set; }

		// To get ASC within the active date range or with the latest expiration date
		private LicenseListItem GetLicenseListItem {
			get {
				LicenseListItem licenseListItem = null;

				if (Licenses != null && Licenses.Any()) {
					licenseListItem =
						Licenses.FirstOrDefault(l => l.EffectiveDate.GetValueOrDefault(DateTime.MinValue) >= DateTime.Now && l.ExpirationDate.GetValueOrDefault(DateTime.MinValue) <= DateTime.Now);

					if (licenseListItem != null) {
						return licenseListItem;
					}

					licenseListItem = Licenses.Where(l => l.SupportCenterSupportAccountId != null).OrderByDescending(l => l.ExpirationDate).FirstOrDefault();
				}

				return licenseListItem;
			}
		}

        public bool IsCsspProvisionedDevice { get; set; }

        public string ProductFamily { get; set; }

	    public bool IsChassis
	    {
	        get { return (ProductFamily != null &&
	            (ProductFamily.Equals("PA-5280") || ProductFamily.Contains("PA-70"))); }
	 
	    }

        [DataAccess(DeviceListItemFields.DeviceCategoryIdList, DataType = DbType.Int64, UseForCreate = false, UseForUpdate = false)]
        public string DeviceCategoryIdList
        {
            get;
            set;
        }


    }

    public class DeviceListItemModelCompatibility : DeviceListItem {


		[DataAccess(DeviceListItemFields.ModelCompatibility, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
		public string ModelCompatibility {
			get;
			set;
		}

        public string SupportAccountId { get; set; }
        public string SupportAccountName { get; set; }

		/// <summary>
		/// Used during RMA process by licensing services.  
		/// When a replacement device sent to user the old device src marked as 1.
		/// </summary>
		[DataAccess(DeviceFields.IsRMAReturn, DataType = DbType.Int32)]
		public bool IsRMAReturn {
			set;
			get;
		}


		/// <summary>
		/// Used during RMA process by licensing services.  
		/// When a replacement device sent to user the new device dest marked as1.
		/// </summary>
		[DataAccess(DeviceFields.IsRMAReplacement, DataType = DbType.Int32)]
		public bool IsRMAReplacement {
			set;
			get;
		}

	}

	public class DeviceListItemCsv {
		[CsvColumn(FieldIndex = 1, Name = "Serial Number")]
		public string SerialNumber { get; set; }

		[CsvColumn(FieldIndex = 2, Name = "Model Name")]
		public string ModelName { get; set; }

		private string _deviceName;
		[CsvColumn(FieldIndex = 3, Name = "Device Name")]
		public string DeviceName { get { return ApiCommon.DecodeString(_deviceName); } set { _deviceName = value; } }

        		private string _group;
		[CsvColumn(FieldIndex = 4, Name = "Group")]
		public string Group { get { return ApiCommon.DecodeString(_group); } set { _group = value; } }

        [CsvColumn(FieldIndex = 6, Name = "Authorization Code")]
        public string AuthorizationCode { get; set; }
        [CsvColumn(FieldIndex = 5, Name = "License")]
		public string License { get; set; }

		[CsvColumn(FieldIndex = 7, Name = "OS Version")]
		public string OSVersion { get; set; }

		private string _expirationDate;
		[CsvColumn(FieldIndex = 8, Name = "Expiration Date")]
		public string ExpirationDate { get { if (IsPerpetual) { return "Perpetual"; } else return _expirationDate; } set { _expirationDate = value; } }

		[CsvColumn(FieldIndex = 9, Name = "Authorized Support Center")]
		public string AuthorizedSupportCenter { get; set; }

		private string _deviceTag;
		[CsvColumn(FieldIndex = 10, Name = "Device Tag")]
		public string DeviceTag { get { return ApiCommon.DecodeString(_deviceTag); } set { _deviceTag = value; } }
        [CsvColumn(FieldIndex = 11, Name = "Street Address")]
        public string StreetAddress { get; set; }

        [CsvColumn(FieldIndex = 12, Name = "City")]
		public string City { get; set; }

		[CsvColumn(FieldIndex = 13, Name = "Region State")]
		public string RegionState { get; set; }

		[CsvColumn(FieldIndex = 14, Name = "Postal Code")]
		public string PostalCode { get; set; }

		public bool IsPerpetual { get; set; }

		//[CsvColumn(FieldIndex = 13, Name = "Country")]
		//public string Country { get; set; }
	}

	public class DeviceListItemSparesCsv {
		[CsvColumn(FieldIndex = 1, Name = "Serial Number")]
		public string SerialNumber { get; set; }

		[CsvColumn(FieldIndex = 2, Name = "Model Name")]
		public string ModelName { get; set; }

		[CsvColumn(FieldIndex = 3, Name = "Device Name")]
		public string DeviceName { get; set; }

		[CsvColumn(FieldIndex = 4, Name = "Group")]
		public string Group { get; set; }

		[CsvColumn(FieldIndex = 5, Name = "City")]
		public string City { get; set; }

		[CsvColumn(FieldIndex = 6, Name = "Region State")]
		public string RegionState { get; set; }

		[CsvColumn(FieldIndex = 7, Name = "Postal Code")]
		public string PostalCode { get; set; }

		[CsvColumn(FieldIndex = 8, Name = "Country")]
		public string Country { get; set; }

		[CsvColumn(FieldIndex = 9, Name = "Device Tag")]
		public string DeviceTag { get; set; }

		[CsvColumn(FieldIndex = 10, Name = "Remarks")]
		public string Remarks { get; set; }
	}

    public class DeviceTransferItem : DeviceListItem
    {
        public string VmAuthcode { get; set; }
        public bool IsVM { get; set; }
        public bool IsEA { get; set; }
    }
}
