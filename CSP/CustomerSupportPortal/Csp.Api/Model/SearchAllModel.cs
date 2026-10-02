using System;
using System.Collections.Generic;
using System.Linq;
using Csp.Api.HelperClasses;
using LINQtoCSV;

namespace Csp.Api.Model {

	public class SearchAllModel {

		public SearchAllModel() {
			SearchAllDetails = new SearchAllDetail[0];
			VmDetails = new VmDetail();
			RawGroups = new string[0];
		}

		public long SupportAccountId { get; set; }
		private string _supportAccountName;
		public string SupportAccountName { get { return _supportAccountName; } set { _supportAccountName = ApiCommon.DecodeString(value); } }
		public string AssetType { get; set; }
		public long Id { get; set; }

	    private string _deviceCode;
        public string DeviceCode
        {
            get
            {
                if (!BundleLicenseId.HasValue || BundleLicenseId.Value <= 0 || BundleLicenses == null || !BundleLicenses.Any()) return _deviceCode;

                var serialNumbers =
                (
                    from l in BundleLicenses
                    select l.SerialNumber
                ).ToArray();


                return string.Join("\n", serialNumbers);

            }
            set { _deviceCode = value; }
        }

	    private string _modelName;
        public string ModelName
        {
            get
            {
                if (!BundleLicenseId.HasValue || BundleLicenseId.Value <= 0 || BundleLicenses == null || !BundleLicenses.Any()) return _modelName;

                var modelNames =
                (
                    from l in BundleLicenses
                    select string.Format("<div class=''>{0}</div>", l.ModelName)
                ).ToArray();


                return string.Join("\n", modelNames);

            }
            set { _modelName = value; }
        }
		private string _deviceName;
		public string DeviceName { get { return _deviceName; } set { _deviceName = ApiCommon.DecodeString(value); } }
	   
	    private string _licenses;
	    public string Licenses {
			get
			{
                if (BundleLicenses != null) return _licenses;

                var lics =
                    from d in SearchAllDetails
                    select "<div style=\"white-space: nowrap;\">" + d.License + "</div>";

				return string.Join("\n", lics.ToArray());
			}

            set { _licenses = value; }
		}

	    private string _authcodes;
		public string AuthorizationCodes {
			get
			{
			    if (BundleLicenses != null) return _authcodes;
			    var lics =
			        from d in SearchAllDetails
			        select "<div style=\"white-space: nowrap;\">" + d.AuthorizationCode + "</div>"
			               + (string.IsNullOrEmpty(d.ELAAuthCode) ? "" :
			                   "\n<div style=\"white-space: nowrap;\">" + d.ELAAuthCode + "</div>");

			    return string.Join("\n", lics.ToArray());
			}

		    set { _authcodes = value; }
		}
		public string ExpirationDates {
			get {
			    if (BundleLicenses!= null)
			    {
			        var expirationDates =
			        (
			            from l in BundleLicenses
			            select string.Format("<div class=''>{0}</div>", l.ExpirationDate)
			        ).ToArray();


                    return string.Join("\n", expirationDates);
			    }

			    var lics =
					from d in SearchAllDetails
					let isExpired = (d.DtExpirationDate.HasValue && d.DtExpirationDate < DateTime.Now)
					let isNearlyExpired = (d.DtExpirationDate.HasValue && d.DtExpirationDate < DateTime.Now.AddDays(31))
					let className = isExpired ? "expired-license" : (isNearlyExpired ? "expiration-warning-License" : string.Empty)
					select "<div style=\"white-space: nowrap;\">" +
								 (d.IsPerpetual
										? "Perpetual"
										: "<div class=\"" + className + "\">" + d.ExpirationDate + "</div>") + "</div>";

				return string.Join("\n", lics.ToArray());
			}
		}
		public string Quantity {
			get {
				
			    if (!string.IsNullOrEmpty(VmQuantity)) {
					return VmQuantity;
				}

			    if (BundleLicenseId.HasValue && BundleLicenseId > 0 && BundleLicenses != null)
			    {
			        var quantities =
			        (
			            from l in BundleLicenses
			            select string.Format("<div class=''>{0}</div>", l.Quantity)
			        ).ToArray();


			        return string.Join("\n", quantities.ToArray());
			    }

			    var lics =
					from d in SearchAllDetails
					select "<div style=\"white-space: nowrap;\">" + d.Quantity + "</div>";

				return string.Join("\n", lics.ToArray());
			}
		}
		public string VmQuantity {
			get {
				return VmDetails.DisplayQuantity;
			}
		}
		public string HtmlLocation {
			get {
				return "<div class=\"locrow\">" + (string.IsNullOrEmpty(City) ? string.Empty : "<span class=\"loc\">" + City + "</span>") +
					(string.IsNullOrEmpty(Region) ? string.Empty : "<span class=\"loc\">" + Region + "</span>") +
					(string.IsNullOrEmpty(Country) ? string.Empty : "<span class=\"loc\">" + Country + "</span>") +
					(string.IsNullOrEmpty(PostalCode) ? string.Empty : "<span class=\"loc\">" + PostalCode + "</span>") + "</div>";
			}
		}
		public string CsvLocation { get { return City + " " + Region + " " + Country + " " + PostalCode; } }
		public string City { get; set; }
		public string PostalCode { get; set; }
		public string Region { get; set; }
		public string Country { get; set; }
		public string HtmlGroups { get { return string.Join("\n", RawGroups.Select(x => "<div style=\"white-space: nowrap;\">" + ApiCommon.DecodeString(x) + "</div>")); } }
		public string CsvGroups { get { return string.Join("\n", RawGroups.Select(x => ApiCommon.DecodeString(x))); } }
		public string[] RawGroups { private get; set; }
		private string _deviceTag;
		public string DeviceTag { get { return _deviceTag; } set { _deviceTag = ApiCommon.DecodeString(value); } }
		private string _authorizedSupportCenter;
		public string AuthorizedSupportCenter { get { return _authorizedSupportCenter; } set { _authorizedSupportCenter = ApiCommon.DecodeString(value); } }

		public SearchAllDetail[] SearchAllDetails { get; set; }
		public VmDetail VmDetails { get; set; }

	    public long? BundleLicenseId { get; set; }
	    public List<BundleLicenseItem> BundleLicenses { get; set; }
        public EAllAssetType AllAssetType { get; set; }
        public SearchAllDetail[] ChildLicenses { get; set; }
	}
    public enum EAllAssetType
    {
        Other = 0,
        Pae = 1,
        Xdr = 2,
        Cdl = 3,
        XsIam = 4
    }

    public class SearchAllModelComparer : IEqualityComparer<SearchAllModel> {

		public bool Equals(SearchAllModel x, SearchAllModel y) {
			return x.AssetType == y.AssetType && x.Id == y.Id;
		}

		public int GetHashCode(SearchAllModel obj) {
			return (obj.AssetType + obj.Id).GetHashCode();
		}
	}

    public class SearchAllSnComparer : IEqualityComparer<SearchAllModel>
    {
        public bool Equals(SearchAllModel x, SearchAllModel y)
        {
            return x.DeviceCode == y.DeviceCode && x.ModelName == y.ModelName;
        }

        public int GetHashCode(SearchAllModel obj)
        {
            return (obj.DeviceCode.ToString() + "_" + obj.ModelName.ToString()).GetHashCode();
        }
    }

	public class SearchAllAccountsModelCsv : SearchAllAssetsModelCsv {
		[CsvColumn(FieldIndex = 1, Name = "Support Account")]
		public string SupportAccount { get; set; }



		[CsvColumn(FieldIndex = 15, Name = "Support Account URL")]
		public string SupportAccountUrl { get; set; }
	}

	public class SearchAllAssetsModelCsv {

		[CsvColumn(FieldIndex = 2, Name = "Asset Type")]
		public string AssetType { get; set; }

		[CsvColumn(FieldIndex = 3, Name = "Device Code")]
		public string DeviceCode { get; set; }

		[CsvColumn(FieldIndex = 4, Name = "Model Name")]
		public string ModelName { get; set; }

		[CsvColumn(FieldIndex = 5, Name = "Device Name")]
		public string DeviceName { get; set; }

		[CsvColumn(FieldIndex = 6, Name = "License")]
		public string License { get; set; }

		[CsvColumn(FieldIndex = 7, Name = "Auth Code")]
		public string AuthCode { get; set; }

		[CsvColumn(FieldIndex = 8, Name = "Expiration Date")]
		public string ExpirationDate { get; set; }

		[CsvColumn(FieldIndex = 9, Name = "Quantity")]
		public string Quantity { get; set; }

		[CsvColumn(FieldIndex = 10, Name = "Location")]
		public string Location { get; set; }

		[CsvColumn(FieldIndex = 11, Name = "Groups")]
		public string Groups { get; set; }

		[CsvColumn(FieldIndex = 12, Name = "Device Tag")]
		public string DeviceTag { get; set; }

		[CsvColumn(FieldIndex = 13, Name = "ASC")]
		public string Asc { get; set; }

		[CsvColumn(FieldIndex = 14, Name = "Device URL")]
		public string DeviceUrl { get; set; }
	}

	public class VmDetail {

		public int? VmUseCount { get; set; }

		public long? VmTotalCount { get; set; }

		public int? IsNsx { get; set; }

		public int? IsPerpetualNsx { get; set; }

		public string DisplayQuantity {
			get {

				var isVm = VmUseCount.HasValue && VmTotalCount.HasValue && IsNsx.HasValue && IsPerpetualNsx.HasValue;

				if (!isVm) {
					return null;
				}

				long qtyProvisioned = 0;

				if (VmTotalCount >= VmUseCount) {
					qtyProvisioned = VmTotalCount.Value - VmUseCount.Value;
				}

				return IsNsx == 0 || IsPerpetualNsx == 1
								 ? qtyProvisioned + "/" + VmTotalCount
								 : qtyProvisioned.ToString();
			}
		}
	}

	public class SearchAllDetail {
		public string License { get; set; }
		public string AuthorizationCode { get; set; }
		public string ExpirationDate { get; set; }
		public string Quantity { get; set; }
		public bool IsPerpetual { get; set; }
		public DateTime? DtExpirationDate { get; set; }
		public string AuthorizedSupportCenter { get; set; }
        public string ELAAuthCode { get; set; }
        public string Model { get; set; }
    }
}
