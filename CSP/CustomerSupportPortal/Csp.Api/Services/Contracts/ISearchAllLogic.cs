﻿using System.Collections.Generic;
﻿using System.Text;
using Csp.Api.Model;

namespace Csp.Api.Services.Contracts {
	public interface ISearchAllLogic {
		SearchAllViewModel GetDefaultViewModel(long userAccountId, long? defaultSupportAccountId);
		SearchAllModel[] SearchAll(long userAccountId, long? defaultSupportAccountId, DataParameters dataParameters, bool isDebug = false, StringBuilder sb = null);
		byte[] SearchAllCsv(long? supportAccountId, long restrictedByUserAccountId, string searchField, string searchValue);
        IEnumerable<ActivationLinksLookupItem> GetActivationLinksLookup(DataParameters parameters, long supportAccountId, string search);

        IEnumerable<SearchAllModel> SearchAllV2(long userAccountId, long? defaultSupportAccountId, DataParameters dataParameters);
    }

    public static class SearchAllConts
    {
        public const string VmPartNumber = "PAN-PA-VM-SW";
        public const string VmPstPanDbId = "PHOENIX";
        public const string TrapsProductFamily = "TRAPS";
        public const string OssPanDbId = "OSS";
        public const string SparePanDbId = "SPARE";
        public const string DevPandDbId = "DEV";
        public const string XdrBundle = "XDR-MGFR-TRAPS";

        public static readonly IEnumerable<string> PanoramaIds = new[]
			{
				"PHOENIX10ASB",
				"PHOENIX10BSB",
				"PHOENIX10APB",
				"PHOENIX10BPB",
				"PHOENIX10ESB"
			};
    }
}