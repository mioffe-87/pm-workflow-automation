
using System.Collections.Generic;

namespace Csp.Api.Model {
	 public class SearchAllViewModel {
		 public bool HasMultipleAccounts { get; set; }
		 public bool SearchCurrentSupportAccount { get; set; }
		 public string CurrentSupportAccountName { get; set; }
		 public long? DefaultSupportAccountId { get; set; }

		 public SearchModel SearchAssetTypes { get; set; }

		 public SearchAllModel[] SearchAllResults { get; set; }
		 public DataParameters DataParameters { get; set; }

         public bool IsSuperuser { get; set; }

        public Dictionary<long, string> CspIds { get; set; }

		 public class SearchModel {

			 public bool SearchDevices { get; set; }
			 public bool SearchSpares { get; set; }
			 public bool SearchTraps { get; set; }
			 public bool SearchVms { get; set; }
		 }
	 }
 }
