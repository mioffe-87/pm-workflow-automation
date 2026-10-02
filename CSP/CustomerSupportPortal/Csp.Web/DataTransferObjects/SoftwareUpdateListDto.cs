using System.Collections.Generic;
using Csp.Api.Model;

namespace Csp.Web.DataTransferObjects {
	public class SoftwareUpdateListDto : DefaultDto {
		#region "Constuctor"

		public SoftwareUpdateListDto() {
            SoftwareUpdates = new List<SoftwareUpdateListItemDto>();
			FilterOptions = new Dictionary<string, string>();
		}

		#endregion

		#region Public Properties

        public List<SoftwareUpdateListItemDto> SoftwareUpdates
        {
			get;
			set;
		}

        public List<SoftwareUpdateListItemDto> PanoramaPluginUpdates
        {
            get;
            set;
        }

		public Dictionary<string, string> FilterOptions {
			get;
			set;
		}

		#endregion
	}
}