using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core.Services
{
    /// <summary>
    /// Service for handling OAuth 2.0 Authorization Code Flow.
    /// </summary>
    public class AuthorizationCodeFlowService : IAuthorizationCodeFlowService
    {
        private readonly IHttpClientFactory _clientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationCodeFlowService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory to create HTTP clients.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="httpClientFactory"/> is null.</exception>
        public AuthorizationCodeFlowService(IHttpClientFactory httpClientFactory)
        {
            _clientFactory = httpClientFactory ?? throw new ArgumentException(nameof(httpClientFactory));
        }

        /// <summary>
        /// Sends a POST request to the token endpoint to exchange the authorization code for an access token.
        /// </summary>
        /// <param name="config">The OAuth connection configuration.</param>
        /// <param name="code">The authorization code received from the authorization server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        public virtual async Task<HttpResponseMessage> PostAuthorizationCodeRequestAsync(OAuthConnectionConfig config, string code, CancellationToken cancellationToken = default)
        {
            var client = _clientFactory.CreateClient();
            var tokenRequestBody = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("client_id", config.ClientId),
                    new KeyValuePair<string, string>("client_secret", config.ClientSecret),
                    new KeyValuePair<string, string>("redirect_uri", config.RedirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                });
            return await client.PostAsync(config.TokenEndpoint, tokenRequestBody, cancellationToken);
        }

        /// <summary>
        /// Sends a POST request to the token endpoint to exchange the refresh token for a new access token.
        /// </summary>
        /// <param name="config">The OAuth connection configuration.</param>
        /// <param name="refreshToken">The refresh token received from the authorization server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        public virtual async Task<HttpResponseMessage> PostRefreshTokenRequestAsync(OAuthConnectionConfig config, string refreshToken, CancellationToken cancellationToken = default)
        {
            var client = _clientFactory.CreateClient();
            var tokenRequestBody = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("client_id", config.ClientId),
                    new KeyValuePair<string, string>("client_secret", config.ClientSecret),
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                    new KeyValuePair<string, string>("grant_type", "refresh_token")
                });
            return await client.PostAsync(config.TokenEndpoint, tokenRequestBody, cancellationToken);
        }
    }
}
