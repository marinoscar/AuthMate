using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core.Services
{
    /// <summary>
    /// Service to manage application connections.
    /// </summary>
    public class AppConnectionService
    {
        private readonly IAuthMateContext _context;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AppConnectionService> _logger;
        private readonly IUserResolver _userResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConnectionService"/> class.
        /// </summary>
        /// <param name="context">The context to interact with the database.</param>
        /// <param name="clientFactory">The factory to create HTTP clients.</param>
        /// <param name="userResolver">The user resolver to get user information.</param>
        /// <param name="logger">The logger to log information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are null.</exception>
        public AppConnectionService(IAuthMateContext context, IHttpClientFactory clientFactory, IUserResolver userResolver, ILogger<AppConnectionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userResolver = userResolver ?? throw new ArgumentNullException(nameof(userResolver));
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }


        /// <summary>
        /// Persists the given application connection to the database.
        /// </summary>
        /// <param name="connection">The application connection to persist.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The persisted application connection.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the connection is null.</exception>
        /// <remarks>
        /// This method increments the version of the connection. If the connection is new (Id is 0),
        /// it sets the creation date and creator information, and adds the connection to the context.
        /// If the connection already exists, it updates the modification date and modifier information,
        /// and updates the connection in the context. Finally, it saves the changes to the database.
        /// </remarks>
        public async Task<AppConnection> PersistConnectionAsync(AppConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            try
            {
                connection.Version++;
                if (connection.Id == 0)
                {
                    connection.UtcCreatedOn = DateTime.UtcNow;
                    connection.CreatedBy = _userResolver.GetUserEmail();
                    _context.AppConnections.Add(connection);
                    _logger.LogInformation("New AppConnection created by {UserEmail}", connection.CreatedBy);
                }
                else
                {
                    connection.UtcUpdatedOn = DateTime.UtcNow;
                    connection.UpdatedBy = _userResolver.GetUserEmail();
                    _context.AppConnections.Update(connection);
                    _logger.LogInformation("AppConnection updated by {UserEmail}", connection.UpdatedBy);
                }
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("AppConnection persisted successfully with Id {ConnectionId}", connection.Id);
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while persisting the AppConnection");
                throw;
            }
        }

        /// <summary>
        /// Retrieves an application connection based on the provider name and owner email.
        /// </summary>
        /// <param name="providerName">The name of the provider.</param>
        /// <param name="ownerEmail">The email of the owner.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The application connection that matches the specified provider name and owner email.</returns>
        /// <exception cref="ArgumentNullException">Thrown when providerName or ownerEmail is null or empty.</exception>
        public async Task<AppConnection> GetConnectionAsync(string providerName, string ownerEmail, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(providerName)) throw new ArgumentNullException(nameof(providerName));
            if (string.IsNullOrEmpty(ownerEmail)) throw new ArgumentNullException(nameof(ownerEmail));

            return await _context.AppConnections.SingleAsync(c => c.ProviderName == providerName && c.OwnerEmail == ownerEmail, cancellationToken);
        }

        /// <summary>
        /// Retrieves an application connection based on the provider name and account ID.
        /// </summary>
        /// <param name="providerName">The name of the provider.</param>
        /// <param name="accountId">The account ID.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The application connection that matches the specified provider name and account ID.</returns>
        /// <exception cref="ArgumentNullException">Thrown when providerName is null or empty.</exception>
        public async Task<AppConnection> GetConnectionAsync(string providerName, ulong accountId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(providerName)) throw new ArgumentNullException(nameof(providerName));

            return await _context.AppConnections.SingleAsync(c => c.ProviderName == providerName && c.AccountId == accountId, cancellationToken);
        }

        /// <summary>
        /// Retrieves a collection of application connections that match the specified filter expression.
        /// </summary>
        /// <param name="filterExpression">The filter expression to apply.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A collection of application connections that match the specified filter expression.</returns>
        /// <exception cref="ArgumentNullException">Thrown when filterExpression is null.</exception>
        public async Task<IQueryable<AppConnection>> GetConnectionsAsync(Expression<Func<AppConnection, bool>> filterExpression, CancellationToken cancellationToken = default)
        {
            if (filterExpression == null) throw new ArgumentNullException(nameof(filterExpression));

            return await Task.Run(() => _context.AppConnections.Where(filterExpression), cancellationToken);
        }

        /// <summary>
        /// Creates the authorization consent URL for the OAuth provider.
        /// </summary>
        /// <param name="config">The OAuth connection configuration.</param>
        /// <returns>The authorization consent URL.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the config is null.</exception>
        public string CreateAuthorizationConsentUrl(OAuthConnectionConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            return $"{config.AuthorizationEndpoint}?response_type=code" +
                       $"&client_id={config.ClientId}" +
                       $"&redirect_uri={config.RedirectUri}" +
                       $"&scope={Uri.EscapeDataString(config.Scopes)}" +
                       "access_type=offline&prompt=consent";
        }

        /// <summary>
        /// Creates an authorization code request to exchange the code for an access token.
        /// </summary>
        /// <param name="config">The OAuth connection configuration.</param>
        /// <param name="code">The authorization code received from the OAuth provider.</param>
        /// <param name="error">The error message, if any, received from the OAuth provider.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The OAuth token response.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the config or code is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an error is received or the token request fails.</exception>
        public async Task<OAuthTokenResponse> CreateAuthorizationCodeRequestAsync(OAuthConnectionConfig config, string code, string? error, CancellationToken cancellationToken = default)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
            if (string.IsNullOrEmpty(code)) throw new ArgumentNullException(nameof(code));

            try
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

                var tokenResponse = await client.PostAsync(config.TokenEndpoint, tokenRequestBody, cancellationToken);

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get token. Status code: {StatusCode}", tokenResponse.StatusCode);
                    throw new InvalidOperationException("Failed to get token");
                }

                var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenData = OAuthTokenResponse.Success(JsonDocument.Parse(tokenResponseContent));

                if (tokenData == null)
                {
                    _logger.LogError("Failed to parse token response");
                    throw new InvalidOperationException("Failed to parse token response");
                }

                _logger.LogInformation("Token successfully retrieved and parsed");
                return tokenData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the authorization code request");
                throw;
            }
        }


    }
}
