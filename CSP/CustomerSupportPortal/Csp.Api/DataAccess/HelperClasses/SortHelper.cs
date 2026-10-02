using Csp.Api.HelperClasses;
using Csp.Api.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Csp.Api.DataAccess.HelperClasses
{
    public static class SortHelper
    {
        public static bool ValidateSorting(string sortField, string sortOrder)
        {
            var sortFieldNameList = Enum.GetValues(typeof(SortFieldEnum)).Cast<SortFieldEnum>().Where(e => e.ToDescription()?.ToString().ToLower() == sortField?.ToLower()).ToList();
            var sortOrderList = Enum.GetValues(typeof(SortOrderEnum)).Cast<SortOrderEnum>().Where(e => e.ToDescription()?.ToString().ToLower() == sortOrder?.ToLower()).ToList();
            return (sortFieldNameList?.Count() > 0 && sortOrderList?.Count() > 0);
        }
    }
}
