using Luval.AuthMate.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core.Interfaces
{
    /// <summary>
    /// Interface for handling OAuth 2.0 Authorization Code Flow.
    /// </summary>
    public interface IAuthorizationCodeFlowService
    {
        /// <summary>
        /// Sends a POST request to the token endpoint to exchange the authorization code for an access token.
        /// </summary>
        /// <param name="config">The OAuth connection configuration.</param>
        /// <param name="code">The authorization code received from the authorization server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        Task<HttpResponseMessage> PostAuthorizationCodeRequestAsync(OAuthConnectionConfig config, string code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a POST request to the token endpoint to exchange the refresh token for a new access token.
        /// </summary>
        /// <param name="config">The OAuth connection configuration.</param>
        /// <param name="refreshToken">The refresh token received from the authorization server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        Task<HttpResponseMessage> PostRefreshTokenRequestAsync(OAuthConnectionConfig config, string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a GET request to the user info endpoint to retrieve user information.
        /// </summary>
        /// <param name="config">The OAuth connection configuration.</param>
        /// <param name="accessToken">The access token received from the authorization server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        Task<HttpResponseMessage> GetUserInformation(OAuthConnectionConfig config, string accessToken, CancellationToken cancellationToken = default);
    }
}
