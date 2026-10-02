using System.Collections.Generic;
using System.Text;
using Csp.Api.Model;
using Csp.Api.Security;

namespace Csp.Api.Services.Contracts
{
    public interface ISearchAllRepository
    {
        SearchAllModel[] SearchAll(long userAccountId, long? supportAccountId, DataParameters parameters, List<RoleSecurity> roleSecurities, SearchAllFilters searchAllFilters, StringBuilder sb = null);
        IEnumerable<ActivationLinksLookupItem> GetActivationLinksLookup(DataParameters parameters, long supportAccountId, string search);
        Dictionary<string, string> GetPanSupportWebConfigKeys();

        IEnumerable<SearchAllModel> SearchAllV2(long userAccountId, long? supportAccountId, DataParameters parameters, List<RoleSecurity> roleSecurities);
    }

    public static class AssetTypes
    {
        public const string Prod = "Device";
        public const string Spare = "Spare";
        public const string Traps = "Endpoint";
        public const string Vm = "VM Auth";
        public const string SearchAll = "SearchAll";
        public const string SiteLicenses = "SiteLicenses";
        public const string CloudServices = "Cloud Services";
        public const string Casb = "CASB";
        public const string CnSeries = "CN Auth";
    }

    public class SearchAllFilters
    {

        public SearchAllFilters()
            : this(true)
        {

        }

        public SearchAllFilters(bool toggleAll)
        {
            SearchProd = SearchSpare = SearchTraps = SearchVms = toggleAll;
        }

        public bool SearchProd { get; set; }
        public bool SearchSpare { get; set; }
        public bool SearchTraps { get; set; }
        public bool SearchVms { get; set; }
    }
}