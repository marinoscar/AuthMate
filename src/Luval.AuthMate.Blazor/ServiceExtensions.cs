using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Blazor
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddAuthMateBlazorPresenters(this IServiceCollection s)
        {
            s.AddScoped<RolesPresenter>();
            return s;
        }
    }
}
