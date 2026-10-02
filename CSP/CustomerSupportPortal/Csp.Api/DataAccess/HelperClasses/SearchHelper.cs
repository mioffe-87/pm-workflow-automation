using Csp.Api.HelperClasses;
using Csp.Api.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.DataAccess.HelperClasses
{
    public static class SearchHelper
    {
        public static bool ValidateSearchField(string fieldName)
        {
            var searchFieldList = Enum.GetValues(typeof(SearchFieldEnum)).Cast<SearchFieldEnum>().Where(e => e.ToDescription()?.ToString().ToLower() == fieldName?.ToLower()).ToList();
            return (searchFieldList?.Count() > 0);
        }
    }
}
