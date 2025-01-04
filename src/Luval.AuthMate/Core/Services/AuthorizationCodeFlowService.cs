using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationCodeFlowService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory to create HTTP clients.</param>
        /// <param name="contextAccessor">The <see cref="IHttpContextAccessor"/> to extract the context information for the requests</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="httpClientFactory"/> is null.</exception>
        public AuthorizationCodeFlowService(IHttpClientFactory httpClientFactory, IHttpContextAccessor contextAccessor)
        {
            _clientFactory = httpClientFactory ?? throw new ArgumentException(nameof(httpClientFactory));
            _contextAccessor = contextAccessor ?? throw new ArgumentException(nameof(contextAccessor));
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
            var baseUrl = _contextAccessor.GetBaseUri();
            var redirectUri = new Uri(baseUrl, config.RedirectUri);

            var tokenRequestBody = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("client_id", config.ClientId),
                    new KeyValuePair<string, string>("client_secret", config.ClientSecret),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri.ToString()),
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


        /// <summary>
        /// Sends a GET request to the user info endpoint to retrieve user information.
        /// </summary>
        /// <param name="config">The OAuth connection configuration.</param>
        /// <param name="accessToken">The access token received from the authorization server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        public virtual async Task<HttpResponseMessage> GetUserInformation(OAuthConnectionConfig config, string accessToken, CancellationToken cancellationToken = default)
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            return await client.GetAsync(config.UserInfoEndpoint, cancellationToken);
        }


    }
}
