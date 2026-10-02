using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Csp.Api.Model {

	public enum ResponseState {
		Success=1,
		Information=2,
		Warning=3,
		Error=4
	}


	public class ResponseNotification {
        
		#region public members


		public ResponseNotification(string message, ResponseState state) {
			Message = message;
			State = state;
		}

		public string Message {
			get;
			set;
		}

		public ResponseState State {
			get;
			set;
		}


		#endregion
	}
}