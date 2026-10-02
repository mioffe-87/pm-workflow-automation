using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model {

	public class LicenseKey {

		#region Public Members

		public string Name {
			get;
			set;
		}

		public string Key {
			get;
			set;
		}

        public string PartSubTypeName
        {
            get;
            set;
        }

        #endregion
    }
}
