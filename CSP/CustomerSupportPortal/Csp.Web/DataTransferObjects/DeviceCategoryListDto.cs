using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {
	public class DeviceCategoryListDto : DefaultDto {
		#region "Constuctor"

		public DeviceCategoryListDto() {
			DeviceCategories = new List<DeviceCategoryListItem>();
		}

		#endregion

		#region Public Properties

		public List<DeviceCategoryListItem> DeviceCategories {
			get;
			set;
		}

        public bool IsSupportAccountAdmin {
            get;
            set;
        }
		#endregion

	}
}