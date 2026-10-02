using Kendo.Mvc;
using Microsoft.Security.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Csp.Web.HelperClasses
{
    public class KendoHelper
    {
        public static IList<AggregateDescriptor> SanitizeAggregates(IList<AggregateDescriptor> aggregateData)
        {
            if (aggregateData == null || !aggregateData.Any()) return aggregateData;
            foreach (var aggregate in aggregateData)
            {
                var descriptor = aggregate as AggregateDescriptor;
                if (descriptor != null && descriptor.Member.GetType() == typeof(string))
                {
                    descriptor.Member = Sanitizer.GetSafeHtmlFragment(descriptor.Member);
                }
            }
            return aggregateData;
        }

        public static IList<SortDescriptor> SanitizeSorts(IList<SortDescriptor> sortData)
        {
            if (sortData == null || !sortData.Any()) return sortData;
            foreach (var sort in sortData)
            {
                var descriptor = sort as SortDescriptor;
                if (descriptor != null && descriptor.Member.GetType() == typeof(string))
                {
                    descriptor.Member = Sanitizer.GetSafeHtmlFragment(descriptor.Member);
                }
            }
            return sortData;
        }

        public static IList<GroupDescriptor> SanitizeGroups(IList<GroupDescriptor> groupData)
        {
            if (groupData == null || !groupData.Any()) return groupData;
            foreach (var group in groupData)
            {
                var descriptor = group as GroupDescriptor;
                if (descriptor != null && descriptor.Member.GetType() == typeof(string))
                {
                    descriptor.Member = Sanitizer.GetSafeHtmlFragment(descriptor.Member);
                }
            }
            return groupData;
        }


        public static IList<IFilterDescriptor> SanitizeFilters(IList<IFilterDescriptor> filters)
        {
            if (filters == null || !filters.Any()) return filters;
            foreach (var filter in filters)
            {
                var descriptor = filter as FilterDescriptor;
                if (descriptor != null && descriptor.Member.GetType() == typeof(string))
                {
                    descriptor.Member = Sanitizer.GetSafeHtmlFragment(descriptor.Member);
                }
                else if (filter is CompositeFilterDescriptor)
                {
                    SanitizeFilters(((CompositeFilterDescriptor)filter).FilterDescriptors);
                }
            }
            return filters;
        }
    }
}