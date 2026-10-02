using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Attributes;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.Data;

namespace Csp.Api.Model {

	public class DeviceCategory : AuditCspModel {

		#region Public Properties


		[DataAccess(DeviceCategoryFields.DeviceCategoryId, DataType = DbType.Int64, UseForCreate = false)]
		public long DeviceCategoryId {
			get;
			set;
		}

		private string _deviceCategoryName;
		[StringLength(50, ErrorMessage = "Maximum length is 50 characters")]
		[Required(ErrorMessage = ApiConstants.DEVICE_CATEGORY + " Name is required")]
		public string DeviceCategoryName {
			set { _deviceCategoryName = ApiCommon.DecodeString(value); }
			get { return _deviceCategoryName; }
		}

		[DataAccess(DeviceCategoryFields.DeviceCategoryName)]
		public string DeviceCategoryNameEncoded {
			set { DeviceCategoryName = value; }
			get { return ApiCommon.EncodeString(DeviceCategoryName); }
		}

		private string _descriptionName;
		public string Description {
			set { _descriptionName = ApiCommon.DecodeString(value); }
			get { return _descriptionName; }
		}

		[DataAccess(DeviceCategoryFields.Description, DataType = DbType.String)]
		public string DescriptionEncoded {
			set { Description = value; }
			get { return ApiCommon.EncodeString(Description); }
		}

		[DataAccess(DeviceCategoryFields.SupportAccountId, DataType = DbType.Int32)]
		public long SupportAccountId {
			get;
			set;
		}

		[DataAccess(DeviceCategoryFields.UserAccountId, DataType = DbType.Int32)]
		public long UserAccountId {
			get;
			set;
		}

		[DataAccess(DeviceCategoryFields.IsActive, DataType = DbType.Boolean)]
		public bool IsActive {
			get;
			set;
		}

		[DataAccess(DeviceCategoryFields.IsPublic, DataType = DbType.Boolean)]
		public bool IsPublic {
			get;
			set;
		}
		#endregion

	}
}
