using System.Collections.Generic;
using Csp.Api.Model;
using Csp.Web.HelperClasses;

namespace Csp.Web.DataTransferObjects {

	public class DefaultDto {


		#region Public Properties

		public DefaultDto() {
			Notifications = new List<ResponseNotification>();
		}


		public List<ResponseNotification> Notifications {

			get;
			private set;
		}

		public ActionHelper ActionHelper {

			get;
			set;
		}

		public DataParameters DataParameters {
			get;
			set;
		}

        public bool IsWelcomeSectionHidden {
            get;
            set;
        }

        public bool IsJumpToSectionHidden {
            get;
            set;
        }

		#endregion

	}
}