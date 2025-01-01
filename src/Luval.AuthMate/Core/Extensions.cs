using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Resolver;
using Luval.AuthMate.Core.Services;
using Luval.AuthMate.Infrastructure.Configuration;
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
        /// Converts an <see cref="AppUser"/> instance to a <see cref="ClaimsIdentity"/> instance.
        /// </summary>
        /// <param name="user">The <see cref="AppUser"/> instance containing the user's data.</param>
        /// <returns>A <see cref="ClaimsIdentity"/> populated with claims from the <see cref="AppUser"/> instance.</returns>
        public static ClaimsIdentity ToIdentity(this AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.DisplayName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email  ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.ProviderKey  ?? string.Empty),
                new Claim("urn:google:image", user.ProfilePictureUrl  ?? string.Empty),
                new Claim("ProfilePictureUrl", user.ProfilePictureUrl  ?? string.Empty)
            };
            var roleClaims = user.UserRoles.Where(i => i.Role != null && !string.IsNullOrEmpty(i.Role.Name))
                .Select(i => new Claim(ClaimTypes.Role, i.Role.Name)).ToList();

            claims.AddRange(roleClaims);

            return new ClaimsIdentity(claims, user.ProviderType);
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
        /// Adds the AuthMate services to the specified <see cref="IServiceCollection"/>.
        /// This method registers the necessary services for the AuthMate authentication system,
        /// including user resolution, user management, role management, account management,
        /// authentication, and token handling services.
        /// It then registers the provided <paramref name="authMateDbContextFactory"/> as a scoped service for creating <see cref="IAuthMateContext"/> instances.
        /// </summary>
        /// <param name="s">The <see cref="IServiceCollection"/> to which the services will be added.</param>
        /// <param name="bearingTokenKey">The key used to sign and verify the bearing token.</param>
        /// <param name="authMateDbContextFactory">A factory function that creates instances of <see cref="IAuthMateContext"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> with the AuthMate services and the <see cref="IAuthMateContext"/> factory added.</returns>
        public static IServiceCollection AddAuthMateServices(this IServiceCollection s, string bearingTokenKey, Func<IServiceProvider, IAuthMateContext> authMateDbContextFactory)
        {
            s = AddAuthMateServices(s, bearingTokenKey);
            s.AddScoped<IAuthMateContext>(authMateDbContextFactory);
            return s;
        }


        /// <summary>
        /// Adds the AuthMate services to the specified <see cref="IServiceCollection"/>.
        /// This method registers the necessary services for the AuthMate authentication system,
        /// including user resolution, user management, role management, account management,
        /// authentication, and token handling services.
        /// </summary>
        /// <param name="s">The <see cref="IServiceCollection"/> to which the services will be added.</param>
        /// <returns>The <see cref="IServiceCollection"/> with the AuthMate services added.</returns>
        public static IServiceCollection AddAuthMateServices(this IServiceCollection s, string bearingTokenKey = default!)
        {
            if(!string.IsNullOrEmpty(bearingTokenKey))
                s.AddSingleton(new BearingTokenConfig() { Secret = bearingTokenKey });
            else
                s.AddSingleton(new BearingTokenConfig());

            s.AddScoped<IAuthorizationCodeFlowService, AuthorizationCodeFlowService>();
            s.AddScoped<IUserResolver, WebUserResolver>();
            s.AddScoped<IAppUserService, AppUserService>();
            s.AddScoped<RoleService>();
            s.AddScoped<AccountService>();
            s.AddScoped<AuthenticationService>();
            s.AddScoped<BearingTokenService>();
            return s;
        }

    }
}
