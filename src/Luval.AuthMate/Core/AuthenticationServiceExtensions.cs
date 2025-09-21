using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Formats.Tar;

namespace Luval.AuthMate.Core
{
    /// <summary>
    /// Provides extension methods for configuring authentication services.
    /// </summary>
    public static class AuthenticationServiceExtensions
    {

        /// <summary>
        /// Creates an instance of <see cref="OAuthConfiguration"/> from the configuration.
        /// </summary>
        /// <param name="s">The service collection.</param>
        /// <returns>An instance of <see cref="OAuthConfiguration"/>.</returns>
        private static OAuthConfiguration CreateFromConfig(IServiceCollection s)
        {
            return OAuthConfiguration.GetAuthentication(s.GetConfiguration());
        }

        /// <summary>
        /// Adds AuthMate authentication services to the specified service collection.
        /// </summary>
        /// <param name="s">The service collection.</param>
        /// <returns>The service collection with AuthMate authentication services added.</returns>
        public static IServiceCollection AddAuthMateAuthentication(this IServiceCollection s)
        {
            return AddAuthMateAuthentication(s, CreateFromConfig(s));
        }

        /// <summary>
        /// Adds AuthMate authentication services to the specified service collection with a custom ticket creation function.
        /// </summary>
        /// <param name="s">The service collection.</param>
        /// <param name="onCreatingTicket">The function to be called when creating the OAuth ticket.</param>
        /// <returns>The service collection with AuthMate authentication services added.</returns>
        public static IServiceCollection AddAuthMateAuthentication(this IServiceCollection s, Func<OAuthCreatingTicketContext, Task> onCreatingTicket)
        {
            var oAuthConfig = CreateFromConfig(s);
            oAuthConfig.OnCreatingTicket = onCreatingTicket;
            return AddAuthMateAuthentication(s, oAuthConfig);
        }

        /// <summary>
        /// Adds AuthMate authentication services to the specified service collection with the specified configuration.
        /// </summary>
        /// <param name="s">The service collection.</param>
        /// <param name="config">The OAuth configuration.</param>
        /// <returns>The service collection with AuthMate authentication services added.</returns>
        public static IServiceCollection AddAuthMateAuthentication(this IServiceCollection s, OAuthConfiguration config)
        {
            s.AddAuthentication("Cookies")
                .AddCookie(opt =>
                {
                    opt.Cookie.Name = config.CookieName;
                    opt.LoginPath = config.LoginPath;
                    opt.ReturnUrlParameter = config.ReturnUrlParameter;
                })
                .AddGoogle(opt =>
                {
                    opt.ClientId = config.ClientId;
                    opt.ClientSecret = config.ClientSecret;
                    opt.SaveTokens = config.SaveTokens;
                    opt.ReturnUrlParameter = config.ReturnUrlParameter;

                    if (config.Scopes != null && config.Scopes.Any())
                        config.Scopes.ForEach(scope => opt.Scope.Add(scope));

                    opt.AccessType = config.AccessType;
                    opt.CallbackPath = config.CallbackPath;

                    opt.ClaimActions.MapJsonKey("urn:google:profile", "link");
                    opt.ClaimActions.MapJsonKey("urn:google:image", "picture");

                    if (config.OnCreatingTicket == null)
                    {
                        opt.Events.OnCreatingTicket = async context =>
                        {
                            var authService = s.BuildServiceProvider().GetRequiredService<AuthMate.Core.Services.AuthenticationService>();
                            await authService.AuthorizeUserAsync(context.Identity, TokenResponse.Create(context.TokenResponse), DeviceInfo.Create(context.Properties), CancellationToken.None);
                        };
                    }
                    else
                    {
                        opt.Events.OnCreatingTicket = config.OnCreatingTicket;
                    }
                });
            return s;
        }
    }
}
