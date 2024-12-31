using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luval.AuthMate.Infrastructure.Configuration
{
    /// <summary>
    /// Provides abstraction for the OAuth Authorization Code flow configuration.
    /// </summary>
    public class OAuthConnectionConfig
    {
        /// <summary>
        /// Gets or sets the name of the OAuth provider (e.g., "Google", "Microsoft", or "Facebook").
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Gets or sets the client ID for the OAuth provider.
        /// </summary>
        public string ClientId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the client secret for the OAuth provider.
        /// </summary>
        public string ClientSecret { get; set; } = default!;

        /// <summary>
        /// Gets or sets the authorization endpoint URL for the OAuth provider.
        /// </summary>
        public string AuthorizationEndpoint { get; set; } = default!;

        /// <summary>
        /// Gets or sets the token endpoint URL for the OAuth provider.
        /// </summary>
        public string TokenEndpoint { get; set; } = default!;

        /// <summary>
        /// Gets or sets the user info endpoint URL for the OAuth provider.
        /// </summary>
        public string UserInfoEndpoint { get; set; } = default!;

        /// <summary>
        /// Gets or sets the redirect URI for the OAuth provider.
        /// </summary>
        public string RedirectUri { get; set; } = default!;

        /// <summary>
        /// Gets or sets the scopes for the OAuth provider.
        /// </summary>
        public string Scopes { get; set; } = default!;

        /// <summary>
        /// Creates a new instance of <see cref="OAuthConnectionConfig"/> configured for Google OAuth.
        /// </summary>
        /// <param name="clientId">The client ID for the Google OAuth application.</param>
        /// <param name="clientSecret">The client secret for the Google OAuth application.</param>
        /// <param name="redirectUrl">The redirect URI for the Google OAuth application.</param>
        /// <param name="scopes">The scopes for the Google OAuth application.</param>
        /// <returns>A configured <see cref="OAuthConnectionConfig"/> instance for Google OAuth.</returns>
        /// <exception cref="ArgumentException">Thrown when any of the input parameters are null or empty.</exception>
        public static OAuthConnectionConfig CreateGoogle(string clientId, string clientSecret, string redirectUrl, string scopes)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("Client ID cannot be null or empty.", nameof(clientId));
            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new ArgumentException("Client Secret cannot be null or empty.", nameof(clientSecret));
            if (string.IsNullOrWhiteSpace(redirectUrl))
                throw new ArgumentException("Redirect URL cannot be null or empty.", nameof(redirectUrl));
            if (string.IsNullOrWhiteSpace(scopes))
                throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));

            return new OAuthConnectionConfig
            {
                Name = "Google",
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUri = redirectUrl,
                Scopes = scopes,
                AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth",
                TokenEndpoint = "https://oauth2.googleapis.com/token",
                UserInfoEndpoint = "https://openidconnect.googleapis.com/v1/userinfo"
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="OAuthConnectionConfig"/> configured for Microsoft OAuth.
        /// </summary>
        /// <param name="clientId">The client ID for the Microsoft OAuth application.</param>
        /// <param name="clientSecret">The client secret for the Microsoft OAuth application.</param>
        /// <param name="redirectUrl">The redirect URI for the Microsoft OAuth application.</param>
        /// <param name="scopes">The scopes for the Microsoft OAuth application.</param>
        /// <returns>A configured <see cref="OAuthConnectionConfig"/> instance for Microsoft OAuth.</returns>
        /// <exception cref="ArgumentException">Thrown when any of the input parameters are null or empty.</exception>
        public static OAuthConnectionConfig CreateMicrosoft(string clientId, string clientSecret, string redirectUrl, string scopes)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("Client ID cannot be null or empty.", nameof(clientId));
            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new ArgumentException("Client Secret cannot be null or empty.", nameof(clientSecret));
            if (string.IsNullOrWhiteSpace(redirectUrl))
                throw new ArgumentException("Redirect URL cannot be null or empty.", nameof(redirectUrl));
            if (string.IsNullOrWhiteSpace(scopes))
                throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));

            return new OAuthConnectionConfig
            {
                Name = "Microsoft",
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUri = redirectUrl,
                Scopes = scopes,
                AuthorizationEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize",
                TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token",
                UserInfoEndpoint = "https://graph.microsoft.com/oidc/userinfo"
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="OAuthConnectionConfig"/> configured for Facebook OAuth.
        /// </summary>
        /// <param name="clientId">The client ID for the Facebook OAuth application.</param>
        /// <param name="clientSecret">The client secret for the Facebook OAuth application.</param>
        /// <param name="redirectUrl">The redirect URI for the Facebook OAuth application.</param>
        /// <param name="scopes">The scopes for the Facebook OAuth application.</param>
        /// <returns>A configured <see cref="OAuthConnectionConfig"/> instance for Facebook OAuth.</returns>
        /// <exception cref="ArgumentException">Thrown when any of the input parameters are null or empty.</exception>
        public static OAuthConnectionConfig CreateFacebook(string clientId, string clientSecret, string redirectUrl, string scopes)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("Client ID cannot be null or empty.", nameof(clientId));
            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new ArgumentException("Client Secret cannot be null or empty.", nameof(clientSecret));
            if (string.IsNullOrWhiteSpace(redirectUrl))
                throw new ArgumentException("Redirect URL cannot be null or empty.", nameof(redirectUrl));
            if (string.IsNullOrWhiteSpace(scopes))
                throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));

            return new OAuthConnectionConfig
            {
                Name = "Facebook",
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUri = redirectUrl,
                Scopes = scopes,
                AuthorizationEndpoint = "https://www.facebook.com/v10.0/dialog/oauth",
                TokenEndpoint = "https://graph.facebook.com/v10.0/oauth/access_token",
                UserInfoEndpoint = "https://graph.facebook.com/me"
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="OAuthConnectionConfig"/> configured for Twitter OAuth.
        /// </summary>
        /// <param name="clientId">The client ID for the Twitter OAuth application.</param>
        /// <param name="clientSecret">The client secret for the Twitter OAuth application.</param>
        /// <param name="redirectUrl">The redirect URI for the Twitter OAuth application.</param>
        /// <param name="scopes">The scopes for the Twitter OAuth application.</param>
        /// <returns>A configured <see cref="OAuthConnectionConfig"/> instance for Twitter OAuth.</returns>
        /// <exception cref="ArgumentException">Thrown when any of the input parameters are null or empty.</exception>
        public static OAuthConnectionConfig CreateTwitter(string clientId, string clientSecret, string redirectUrl, string scopes)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("Client ID cannot be null or empty.", nameof(clientId));
            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new ArgumentException("Client Secret cannot be null or empty.", nameof(clientSecret));
            if (string.IsNullOrWhiteSpace(redirectUrl))
                throw new ArgumentException("Redirect URL cannot be null or empty.", nameof(redirectUrl));
            if (string.IsNullOrWhiteSpace(scopes))
                throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));

            return new OAuthConnectionConfig
            {
                Name = "Twitter",
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUri = redirectUrl,
                Scopes = scopes,
                AuthorizationEndpoint = "https://api.twitter.com/oauth/authorize",
                TokenEndpoint = "https://api.twitter.com/oauth/access_token",
                UserInfoEndpoint = "https://api.twitter.com/1.1/account/verify_credentials.json"
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="OAuthConnectionConfig"/> configured for GitHub OAuth.
        /// </summary>
        /// <param name="clientId">The client ID for the GitHub OAuth application.</param>
        /// <param name="clientSecret">The client secret for the GitHub OAuth application.</param>
        /// <param name="redirectUrl">The redirect URI for the GitHub OAuth application.</param>
        /// <param name="scopes">The scopes for the GitHub OAuth application.</param>
        /// <returns>A configured <see cref="OAuthConnectionConfig"/> instance for GitHub OAuth.</returns>
        /// <exception cref="ArgumentException">Thrown when any of the input parameters are null or empty.</exception>
        public static OAuthConnectionConfig CreateGithub(string clientId, string clientSecret, string redirectUrl, string scopes)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("Client ID cannot be null or empty.", nameof(clientId));
            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new ArgumentException("Client Secret cannot be null or empty.", nameof(clientSecret));
            if (string.IsNullOrWhiteSpace(redirectUrl))
                throw new ArgumentException("Redirect URL cannot be null or empty.", nameof(redirectUrl));
            if (string.IsNullOrWhiteSpace(scopes))
                throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));

            return new OAuthConnectionConfig
            {
                Name = "GitHub",
                ClientId = clientId,
                ClientSecret = clientSecret,
                RedirectUri = redirectUrl,
                Scopes = scopes,
                AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
                TokenEndpoint = "https://github.com/login/oauth/access_token",
                UserInfoEndpoint = "https://api.github.com/user"
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="OAuthConnectionConfig"/> from a JSON document.
        /// </summary>
        /// <param name="jsonDocument">The JSON document containing the OAuth configuration.</param>
        /// <returns>A configured <see cref="OAuthConnectionConfig"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input JSON document is null.</exception>
        public static OAuthConnectionConfig Create(JsonDocument jsonDocument)
        {
            if (jsonDocument == null)
                throw new ArgumentNullException(nameof(jsonDocument), "JSON document cannot be null.");

            return JsonSerializer.Deserialize<OAuthConnectionConfig>(jsonDocument.RootElement.GetRawText()) ?? throw new InvalidOperationException("Failed to deserialize JSON document.");
        }

        /// <summary>
        /// Creates a new instance of <see cref="OAuthConnectionConfig"/> from a JSON string.
        /// </summary>
        /// <param name="json"></param>
        /// <returns>A configured <see cref="OAuthConnectionConfig"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the input json is or empty null.</exception>
        public static OAuthConnectionConfig Create(string json)
        {
            if(string.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json), "JSON string cannot be null or empty.");
            return Create(JsonDocument.Parse(json));
        }



    }
}
