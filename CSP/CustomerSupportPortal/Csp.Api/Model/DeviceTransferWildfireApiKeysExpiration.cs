using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.Model {
	public class DeviceTransferWildfireApiKeysExpiration {

		public long FromSupportAccountId
		{
			get; set;
		}

		public string FromSupportAccountApiKey {
			get;
			set;
		}

		public DateTime FromSupportAccountApiKeyExpiration {
			get;
			set;
		}

		public string FromSupportAccountEpApiKey {
			get;
			set;
		}

		public DateTime FromSupportAccountEpApiKeyExpiration {
			get;
			set;
		}

		public long ToSupportAccountId {
			get;
			set;
		}

		public string ToSupportAccountApiKey {
			get;
			set;
		}

		public DateTime ToSupportAccountApiKeyExpiration {
			get;
			set;
		}

		public string ToSupportAccountEpApiKey {
			get;
			set;
		}

		public DateTime ToSupportAccountEpApiKeyExpiration {
			get;
			set;
		}


	}

    public class BulkDeviceTransferRequest
    {
        public long BulkDevicetransferId { get; set; }
        public long DestinationAccountId { get; set; }
        public long SourceAccountId { get; set; }
    }
}
