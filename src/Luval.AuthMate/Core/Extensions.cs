using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Resolver;
using Luval.AuthMate.Core.Services;
using Luval.AuthMate.Infrastructure.Configuration;
using Luval.AuthMate.Infrastructure.Logging;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core
{
    /// <summary>
    /// Provides extension methods for the AuthMate library.
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <param name="obj">The instance to serialize</param>
        /// <returns>A json serialized string of the target object</returns>
        public static string ToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });
        }

        /// <summary>
        /// Converts an object to a byte array.
        /// </summary>
        /// <param name="obj">The instance to serialize.</param>
        /// <returns>A byte array representing the JSON serialized object.</returns>
        public static byte[] ToBytes(this object obj)
        {
            var json = obj.ToJson();
            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// Converts an object to a Base64 encoded string.
        /// </summary>
        /// <param name="obj">The instance to serialize and encode.</param>
        /// <returns>A Base64 encoded string representing the JSON serialized object.</returns>
        public static string ToBase64(this object obj)
        {
            return Convert.ToBase64String(ToBytes(obj));
        }

        /// <summary>
        /// Decodes a Base64 encoded string and deserializes it into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="base64">The Base64 encoded string representing the serialized object.</param>
        /// <returns>An instance of type <typeparamref name="T"/> deserialized from the Base64 encoded string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="base64"/> parameter is null or empty.</exception>
        /// <exception cref="FormatException">Thrown when the <paramref name="base64"/> parameter is not a valid Base64 string.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized into an instance of type <typeparamref name="T"/>.</exception>
        public static T FromBase64<T>(this string base64)
        {
            if (string.IsNullOrEmpty(base64))
                throw new ArgumentNullException(nameof(base64), "The Base64 string cannot be null or empty.");

            var bytes = Convert.FromBase64String(base64);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>  
        /// Retrieves the base URI from the current HTTP context.  
        /// </summary>  
        /// <param name="contextAccessor">The IHttpContextAccessor instance providing access to the current HTTP context.</param>  
        /// <returns>The base URI if available; otherwise, <c>null</c>.</returns>  
        public static Uri? GetBaseUri(this IHttpContextAccessor contextAccessor)
        {
            if (contextAccessor == null) return null;
            if (contextAccessor.HttpContext == null) return null;
            if (contextAccessor.HttpContext.Request == null) return null;
            return GetBaseUri(contextAccessor.HttpContext.Request);
        }

        /// <summary>  
        /// Retrieves the base URI from the current HTTP context.  
        /// </summary>  
        /// <param name="context">The HttpContext instance providing access to the current HTTP context.</param>  
        /// <returns>The base URI if available; otherwise, <c>null</c>.</returns>  
        public static Uri? GetBaseUri(this HttpContext context)
        {
            if (context == null) return null;
            if (context.Request == null) return null;
            return GetBaseUri(context.Request);
        }

        /// <summary>  
        /// Retrieves the base URI from the specified HTTP request.  
        /// </summary>  
        /// <param name="request">The HttpRequest instance containing the request data.</param>  
        /// <returns>The base URI if available; otherwise, <c>null</c>.</returns>  
        public static Uri? GetBaseUri(this HttpRequest request)
        {
            if (request == null) return null;
            var uri = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
            };
            if (request.Host.Port != null && request.Host.Port != 80) uri.Port = request.Host.Port.Value;

            return uri.Uri;
        }

        /// <summary>  
        /// Retrieves the base URI from the specified URI.  
        /// </summary>  
        /// <param name="uri">The URI instance containing the URI data.</param>  
        /// <returns>The base URI if available; otherwise, <c>null</c>.</returns>  
        public static Uri? GetBaseUri(this Uri uri)
        {
            if (uri == null) return null;
            var uriB = new UriBuilder
            {
                Scheme = uri.Scheme,
                Host = uri.Host,
            };
            return uri;
        }

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
        /// Retrieves the configuration instance from the service collection.
        /// </summary>
        /// <param name="s">The service collection.</param>
        /// <returns>The configuration instance from the service collection.</returns>
        public static IConfiguration GetConfiguration(this IServiceCollection s)
        {
            var serviceProvider = s.BuildServiceProvider(false);
            return serviceProvider.GetRequiredService<IConfiguration>();
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

            s.AddScoped<OAuthConnectionManager>((s) => { 
                var d = s.GetRequiredService<IConfiguration>();
                return new OAuthConnectionManager(d);
            });

            s.AddScoped<IUserResolver, WebUserResolver>();
            s.AddScoped<AccountService>();
            s.AddScoped<AppConnectionService>();
            s.AddScoped<IAppUserService, AppUserService>();
            s.AddScoped<AuthenticationService>();
            s.AddScoped<IAuthorizationCodeFlowService, AuthorizationCodeFlowService>();
            s.AddScoped<BearingTokenService>();
            s.AddScoped<RoleService>();
            return s;
        }

        /// <summary>
        /// Retrieves the AuthMate context from the configuration.
        /// </summary>
        /// <param name="s">The service collection.</param>
        /// <returns>The AuthMate context instance.</returns>
        /// <remarks>
        /// This method retrieves the AuthMate context configuration from the service collection's configuration.
        /// It expects the configuration section "AuthMate:DataContex" to contain the following keys:
        /// - "Provider": The fully qualified name of the context class, including the assembly name.
        /// - "ConnectionString": The connection string to be used by the context.
        /// 
        /// The method performs the following steps:
        /// 1. Retrieves the configuration instance from the service collection.
        /// 2. Gets the "AuthMate:DataContex" section from the configuration.
        /// 3. Validates that the section and its children are not null or empty.
        /// 4. Retrieves the "Provider" and "ConnectionString" values from the section.
        /// 5. Validates that the "Provider" and "ConnectionString" values are not null or empty.
        /// 6. Splits the "Provider" value into assembly and type names.
        /// 7. Creates an instance of the context class using the connection string.
        /// 8. Returns the created context instance.
        /// 
        /// If any validation fails, an InvalidDataException is thrown with an appropriate message.
        /// </remarks>
        private static IAuthMateContext GetContextFromConfiguration(this IServiceCollection s)
        {
            var config = s.GetConfiguration();
            var section = config.GetSection("AuthMate:DataContex");
            if (section == null || section.GetChildren() == null || !section.GetChildren().Any())
                throw new InvalidDataException("The configuration section 'AuthMate:DataContex' is not found or empty");
            var contextClassType = section.GetValue<string>("Provider");
            if (string.IsNullOrWhiteSpace(contextClassType))
                throw new InvalidDataException("The configuration section 'AuthMate:DataContex' does not have a 'Provider' value");
            var connString = section.GetValue<string>("ConnectionString");
            if (string.IsNullOrEmpty(connString))
                throw new InvalidDataException("The configuration section 'AuthMate:DataContex' does not have a 'ConnectionString' value");

            var typeInfo = contextClassType.Split(',').Select(i => i.Trim()).ToArray();
            if (typeInfo.Length < 2)
                throw new InvalidDataException("The configuration section 'AuthMate:DataContex' 'Provider' value is not a valid type name of Assembly, Type");

            var inputs = new object[] { connString };
            var obj = Activator.CreateInstance(typeInfo[0], typeInfo[1], inputs);
            return (IAuthMateContext)obj;
        }

    }
}
