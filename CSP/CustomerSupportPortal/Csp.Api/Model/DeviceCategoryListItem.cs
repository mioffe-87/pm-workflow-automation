using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csp.Api.HelperClasses;
using Csp.Api.Model.Attributes;
using Csp.Api.DataAccess.HelperClasses.Literals;
using System.Data;

namespace Csp.Api.Model {

	public class DeviceCategoryListItem : BaseCspModel {
		[DataAccess(DeviceCategoryListItemFields.DeviceCategoryId, DataType = DbType.Int64, UseForCreate = false, UseForUpdate = false)]
		public long DeviceCategoryId {
			get;
			set;
		}

		private string _deviceCategoryName;
		public string DeviceCategoryName {
			set { _deviceCategoryName = ApiCommon.DecodeString(value); }
			get { return _deviceCategoryName; }
		}

		[DataAccess(DeviceCategoryFields.DeviceCategoryName, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
		public string DeviceCategoryNameEncoded {
			set { DeviceCategoryName = value; }
			get { return ApiCommon.EncodeString(DeviceCategoryName); }
		}

		[DataAccess(DeviceCategoryListItemFields.Description, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
		public string Description {
			get;
			set;
		}

		[DataAccess(DeviceCategoryListItemFields.SupportAccountId, DataType = DbType.Int32, UseForCreate = false, UseForUpdate = false)]
		public long SupportAccountId {
			get;
			set;
		}

		[DataAccess(DeviceCategoryListItemFields.UserAccountId, DataType = DbType.Int32, UseForCreate = false, UseForUpdate = false)]
		public long UserAccountId {
			get;
			set;
		}

		[DataAccess(DeviceCategoryListItemFields.Email, DataType = DbType.String, UseForCreate = false, UseForUpdate = false)]
		public string Email {
			get;
			set;
		}

		[DataAccess(DeviceCategoryListItemFields.IsActive, DataType = DbType.Boolean, UseForCreate = false, UseForUpdate = false)]
		public bool IsActive {
			get;
			set;
		}

		[DataAccess(DeviceCategoryListItemFields.IsPublic, DataType = DbType.Boolean, UseForCreate = false, UseForUpdate = false)]
		public bool IsPublic {
			get;
			set;
		}

        [DataAccess(DeviceCategoryListItemFields.DeviceCount, DataType = DbType.Int32, UseForCreate = false, UseForUpdate = false)]
        public long DeviceCount
        {
            get;
            set;
        }
	}
}
