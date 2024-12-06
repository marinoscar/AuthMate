using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Blazor
{
    public class GridFilterManager<T>
    {
        private readonly IQueryable<T> _baseQuery;
        private readonly Dictionary<string, Expression<Func<T, bool>>> _filters = new();

        /// <summary>
        /// Initializes the FilterManager with the base query.
        /// </summary>
        /// <param name="baseQuery">The base query to apply filters on.</param>
        public GridFilterManager(IQueryable<T> baseQuery)
        {
            _baseQuery = baseQuery;
        }

        /// <summary>
        /// Adds or updates a filter for a specific column/property.
        /// </summary>
        /// <param name="column">The column/property name.</param>
        /// <param name="filter">The filter expression to apply.</param>
        public void AddOrUpdateFilter(string column, Expression<Func<T, bool>> filter)
        {
            _filters[column] = filter; // Add or update the filter for the column
        }

        /// <summary>
        /// Removes a filter for a specific column/property.
        /// </summary>
        /// <param name="column">The column/property name.</param>
        public void RemoveFilter(string column)
        {
            if (_filters.ContainsKey(column))
            {
                _filters.Remove(column);
            }
        }

        /// <summary>
        /// Clears all filters.
        /// </summary>
        public void ClearFilters()
        {
            _filters.Clear();
        }

        /// <summary>
        /// Applies all current filters to the base query and returns the filtered query.
        /// </summary>
        /// <returns>The filtered query.</returns>
        public IQueryable<T> ApplyFilters()
        {
            IQueryable<T> query = _baseQuery;

            foreach (var filter in _filters.Values)
            {
                query = query.Where(filter);
            }

            return query;
        }
    }

}
