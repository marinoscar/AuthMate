﻿using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Services;
using Luval.AuthMate.Infrastructure.Logging;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Converts a <see cref="ClaimsPrincipal"/> to an <see cref="AppUser"/> instance.
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsPrincipal"/> instance containing the user's claims.</param>
        /// <returns>An <see cref="AppUser"/> populated with data from the claims.</returns>
        public static AppUser ToUser(this ClaimsPrincipal identity)
        {
            return ((ClaimsIdentity)identity.Identity).ToUser();
        }

        /// <summary>
        /// Converts a <see cref="ClaimsIdentity"/> to an <see cref="AppUser"/> instance.
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> instance containing the user's claims.</param>
        /// <returns>An <see cref="AppUser"/> populated with data from the claims.</returns>
        public static AppUser ToUser(this ClaimsIdentity identity)
        {
            if (!string.IsNullOrWhiteSpace(identity.GetValue("AppUserJson")))
            {
                return JsonSerializer.Deserialize<AppUser>(identity.GetValue("AppUserJson"));
            }
            return new AppUser()
            {
                ProviderType = identity.AuthenticationType,
                ProviderKey = identity.GetValue(ClaimTypes.NameIdentifier),
                DisplayName = identity.GetValue(ClaimTypes.Name),
                Email = identity.GetValue(ClaimTypes.Email),
                ProfilePictureUrl = identity.GetValue("urn:google:image")
            };
        }

        /// <summary>
        /// Retrieves the value of a specific claim type from a <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="c">The <see cref="ClaimsIdentity"/> instance containing the claims.</param>
        /// <param name="type">The type of the claim to retrieve.</param>
        /// <returns>The value of the claim if found; otherwise, <c>null</c>.</returns>
        public static string? GetValue(this ClaimsIdentity c, string type)
        {
            if (c == null) return null;
            if (!c.HasClaim(i => i.Type == type)) return null;
            return c.Claims.First(i => i.Type == type).Value;
        }

        /// <summary>
        /// Force the creation of the DateTime struct into a Utc Kind
        /// </summary>
        public static DateTime? ForceUtc(this DateTime? d)
        {
            if (!d.HasValue) return null;
            return d.Value.ForceUtc();
        }

        /// <summary>
        /// Force the creation of the DateTime struct into a Utc Kind
        /// </summary>
        public static DateTime ForceUtc(this DateTime d)
        {
            return new DateTime(d.Ticks, DateTimeKind.Utc);
        }

        /// <summary>
        /// Adds the AuthMater services
        /// </summary>
        public static IServiceCollection AddAuthMateServices(this IServiceCollection s, Func<IServiceProvider, IAuthMateContext> authMateDbContextFactory)
        {
            if (Debugger.IsAttached)
                s.AddSingleton(typeof(ILogger<>), typeof(ColorConsoleLogger<>));

            s.AddScoped<IAuthMateContext>(authMateDbContextFactory);
            s.AddScoped<IAppUserService, AppUserService>();
            s.AddScoped<RoleService>();
            s.AddScoped<AccountService>();
            s.AddScoped<AuthenticationService>();
            return s;
        }

    }
}
