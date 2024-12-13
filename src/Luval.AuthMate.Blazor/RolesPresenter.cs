using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luval.AuthMate.Blazor
{
    public class RolesPresenter
    {
        private readonly GridFilterManager<Role> _filterManager;

        public RolesPresenter(IAuthMateContext context)
        {
            // Pass the initial IQueryable (unfiltered data) to the FilterManager
            _filterManager = new GridFilterManager<Role>(context.Roles.AsQueryable());
        }

        /// <summary>
        /// The current filtered data.
        /// </summary>
        public IQueryable<Role> Data => _filterManager.ApplyFilters();

        /// <summary>
        /// Adds or updates a filter for a specific column/property.
        /// </summary>
        /// <param name="column">The column/property name.</param>
        /// <param name="filter">The filter expression to apply.</param>
        public void AddOrUpdateFilter(string column, Expression<Func<Role, bool>> filter)
        {
            _filterManager.AddOrUpdateFilter(column, filter);
        }

        /// <summary>
        /// Removes a filter for a specific column/property.
        /// </summary>
        /// <param name="column">The column/property name.</param>
        public void RemoveFilter(string column)
        {
            _filterManager.RemoveFilter(column);
        }

        /// <summary>
        /// Clears all filters.
        /// </summary>
        public void ClearFilters()
        {
            _filterManager.ClearFilters();
        }
    }


}
