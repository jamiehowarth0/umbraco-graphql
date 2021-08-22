using System;
using System.Collections.Generic;

namespace Our.Umbraco.GraphQL.Filters
{
    public class AndFilter : IFilter
    {
        private readonly IEnumerable<IFilter> _subFilters;

        public AndFilter(IEnumerable<IFilter> subFilters)
        {
            _subFilters = subFilters ?? throw new ArgumentNullException(nameof(subFilters));
        }

        public bool IsSatisfiedBy(object input)
        {
            foreach (var filter in _subFilters)
            {
                if (false == filter.IsSatisfiedBy(input)) return false;
            }

            return true;
        }
    }
}
