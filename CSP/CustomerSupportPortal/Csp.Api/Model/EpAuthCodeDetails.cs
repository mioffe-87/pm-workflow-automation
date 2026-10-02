using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Csp.Api.DataAccess.HelperClasses.Literals;
using Csp.Api.Model.Attributes;
using LINQtoCSV;

namespace Csp.Api.Model {
	public class AvailableEp : BaseCspModel {
		[DataAccess(EndpointFields.AuthorizationCode, DataType = DbType.String)]
		[Required]
		public string AuthorizationCode { get; set; }

        public string ELAAuthCode { get; set; }

		[DataAccess(EndpointFields.EsmAuthorizationCode, DataType = DbType.String)]
		public string EsmAuthorizationCode { get; set; }

		[DataAccess(EndpointFields.UseCount, DataType = DbType.Int64)]
		public int? UseCount { get; set; }

		[DataAccess(EndpointFields.Quantity, DataType = DbType.String)]
		public string Quantity { get; set; }

		[DataAccess(EndpointFields.SerialNumber, DataType = DbType.String)]
		public string SerialNumber { get; set; }

		[DataAccess(EndpointFields.SerialNumber, DataType = DbType.String)]
		public string DivSerialNumber { get; set; }

		[DataAccess(EndpointFields.Description, DataType = DbType.String)]
		public string Description { get; set; }

		[DataAccess(EndpointFields.DeviceId, DataType = DbType.Int64)]
		public long? DeviceId { get; set; }

		[DataAccess(EndpointFields.RegistrationDate, DataType = DbType.DateTime)]
		public DateTime? RegistrationDate { get; set; }

		[DataAccess(EndpointFields.ExpirationDate, DataType = DbType.DateTime)]
		public DateTime? ExpirationDate { get; set; }

		[DataAccess(EndpointFields.ExpirationDate, DataType = DbType.String)]
		public string DivExpirationDate { get; set; }

		[DataAccess(EndpointFields.SubType, DataType = DbType.String)]
		public string PartSubType { get; set; }

		[DataAccess(EndpointFields.PartSubTypeName, DataType = DbType.String)]
		public string PartSubTypeName { get; set; }

		[DataAccess(EndpointFields.LicenseFeatureName, DataType = DbType.String)]
		public string LicenseFeatureName { get; set; }

		[DataAccess(EndpointFields.ShowLicenseKey, DataType = DbType.Boolean)]
		public bool ShowLicenseKey { get; set; }

		[DataAccess(EndpointFields.ShowSoftwareUpdates, DataType = DbType.Boolean)]
		public bool ShowSoftwareUpdates { get; set; }

		public string Actions { get; set; }

		[DataAccess(EndpointFields.DeviceName, DataType = DbType.String)]
		public string DeviceName { get; set; }

		[DataAccess(EndpointFields.DeviceTag, DataType = DbType.String)]
		public string DeviceTag { get; set; }

		[DataAccess(EndpointFields.LicenseName, DataType = DbType.String)]
		public string LicenseName { get; set; }

		public bool IsEsm { get; set; }
		public bool IsServer { get; set; }
		public bool IsWorkstation { get; set; }
		public bool IsSupport { get; set; }
		public bool IsWarranty { get; set; }
        public bool IsTrapsMobile { get; set; }
        public string PartNumber { get; set; }

		[DataAccess(EndpointFields.ASCName, DataType = DbType.String)]
		public string ASCName { get; set; }

		[DataAccess(EndpointFields.ASCId, DataType = DbType.Int64)]
		public long? ASCId { get; set; }
	}

	public class AvailableEpCsv
	{
		[CsvColumn(FieldIndex = 1, Name = "Serial Number")]
		public string SerialNumber { get; set; }

		[CsvColumn(FieldIndex = 2, Name = "Authorization Code")]
		public string AuthorizationCode { get; set; }

		[CsvColumn(FieldIndex = 3, Name = "Quantity")]
		public string Quantity { get; set; }

		[CsvColumn(FieldIndex = 4, Name = "License")]
		public string License { get; set; }

		[CsvColumn(FieldIndex = 5, Name = "Expiration Date")]
		public string ExpirationDate { get; set; }

		[CsvColumn(FieldIndex = 6, Name = "Device Name")]
		public string DeviceName { get; set; }

		[CsvColumn(FieldIndex = 7, Name = "Device Tag")]
		public string DeviceTag { get; set; }

		[CsvColumn(FieldIndex = 8, Name = "ASC")]
		public string ASCName { get; set; }
	}

	public class EpLicenses : BaseCspModel {
		public long? LicenseId { get; set; }
		public string FeatureName { get; set; }
		public string AuthorizationCode { get; set; }
		public string ExpirationDate { get; set; }
		public string PartSubTypeName { get; set; }
	}

	public class AddEpAuthCodeResult {
		public AddEpAuthCodeResult() { }

		public AddEpAuthCodeResult(string errorMessage) {
			ErrorMessage = errorMessage;
			IsSuccess = false;
		}

		public string ErrorMessage { get; set; }

		public string SuccessMessage { get; set; }

		public bool IsSuccess { get; set; }

		public long? DeviceId { get; set; }

		public bool IsEsmAuthCode { get; set; }

		public string EsmAuthCode { get; set; }
	}

	public class EpLicenseResult {
		public EpLicenseResult() { }

		public EpLicenseResult(string errorMessage) {
			ErrorMessage = errorMessage;
		}

		public string ErrorMessage { get; set; }
	}

	public class EpValidationResult {
		public EpValidationResult() {
			HasError = false;
			RequiresUserConfirmation = false;
			RequiresUserSelection = false;

		}

		public EpValidationResult(string errorMessage) {
			ErrorMessage = errorMessage;
		}

		public string ErrorMessage { get; set; }

		public bool HasError { get; set; }

		public bool RequiresUserConfirmation { get; set; }

		public string UserConfirmationMessage { get; set; }

		public bool RequiresUserSelection { get; set; }

		public string UserSelectionMessage { get; set; }

        public bool HasSuccess { get; set; }

        public string SuccessMessage { get; set; }
	}

	public class EpLicenseGenerationResult {
		public EpLicenseGenerationResult() { }

		public EpLicenseGenerationResult(string errorMessage) {
			ErrorMessage = errorMessage;
			HasError = true;
		}

		public string ErrorMessage { get; set; }

		public bool HasError { get; set; }

		public string LicenseKey { get; set; }
	}

	public class EpLicenseType
	{
		public string LicType { get; set; }
		public bool IsEsm { get; set; }
		public bool IsWorkstation { get; set; }
		public bool IsServer { get; set; }
		public bool IsSupport { get; set; }
		public int SortIdx { get; set; }
        public bool IsVdi { get; set; }
        public bool IsAgent { get; set; }
	}

    public class ELACoreAPIRequest
    {
        public string auth_code { get; set; }
        public string user_email { get; set; }       
        public string support_account_id { get; set; }
    }

    public class ELACoreAPIResponse
    {
        public bool issuccess { get; set; }
        public string error_message { get; set; }

    }

}
