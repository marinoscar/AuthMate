using Luval.AuthMate.Entities;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luval.AuthMate.Blazor
{
    public class RolesPresenter
    {
        IAuthMateContext _context;

        public RolesPresenter(IAuthMateContext context)
        {
            _context = context;
        }

        public void LoadData()
        {
            Data = _context.Roles;
        }

        public void ApplyNameFilter(string filter)
        {

            if (string.IsNullOrEmpty(filter))
            {
                LoadData();
                return;
            }


            Data = _context.Roles.Where(i => !string.IsNullOrEmpty(i.Name) && i.Name.Contains(filter));
        }


        public IQueryable<Role> Data { get; private set; }
    }
}
