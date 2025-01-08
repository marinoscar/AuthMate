using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Luval.AuthMate.Core.Services
{
    /// <summary>
    /// Service to manage application connections.
    /// </summary>
    public class AppConnectionService
    {
        private readonly IAuthMateContext _context;
        private readonly IAuthorizationCodeFlowService _codeFlowService;
        private readonly ILogger<AppConnectionService> _logger;
        private readonly IUserResolver _userResolver;
        private readonly OAuthConnectionManager _connectionConfigManager;


        /// <summary>
        /// Initializes a new instance of the <see cref="AppConnectionService"/> class.
        /// </summary>
        /// <param name="context">The context to interact with the database.</param>
        /// <param name="codeFlowService">The service to make the authorization code flow request.</param>
        /// <param name="userResolver">The user resolver to get user information.</param>
        /// <param name="connectionConfigManager">The <see cref="OAuthConnectionManager"/> to handle the Oauth configuration parameters</param>
        /// <param name="logger">The logger to log information.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are null.</exception>
        public AppConnectionService(IAuthMateContext context, IAuthorizationCodeFlowService codeFlowService, IUserResolver userResolver, OAuthConnectionManager connectionConfigManager, ILogger<AppConnectionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userResolver = userResolver ?? throw new ArgumentNullException(nameof(userResolver));
            _codeFlowService = codeFlowService ?? throw new ArgumentNullException(nameof(codeFlowService));
            _connectionConfigManager = connectionConfigManager ?? throw new ArgumentNullException(nameof(connectionConfigManager));
        }

        /// <summary>
        /// Resolves the access token for the given provider and owner email.
        /// </summary>
        /// <param name="connectionId">The name id of the connection.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The access token for the application connection.</returns>
        /// <exception cref="ArgumentNullException">Thrown when providerName or ownerEmail is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not found.</exception>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Validates the input parameters.
        /// 2. Retrieves the application connection based on the provider name and owner email.
        /// 3. If the connection is found, resolves the access token for the connection.
        /// 4. Returns the access token.
        /// </remarks>
        public async Task<string> ResolveAccessTokenAsync(ulong connectionId, CancellationToken cancellationToken = default)
        {
            var conn = await GetConnectionAsync(connectionId, cancellationToken);
            if (conn == null)
            {
                _logger.LogWarning("Connection not found");
                throw new InvalidOperationException("Connection not found");
            }
            return await ResolveAccessTokenAsync(conn, cancellationToken);
        }

        /// <summary>
        /// Resolves the access token for the given provider and owner email.
        /// </summary>
        /// <param name="providerName">The name of the OAuth provider.</param>
        /// <param name="ownerEmail">The email of the owner of the connection.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The access token for the application connection.</returns>
        /// <exception cref="ArgumentNullException">Thrown when providerName or ownerEmail is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection is not found.</exception>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Validates the input parameters.
        /// 2. Retrieves the application connection based on the provider name and owner email.
        /// 3. If the connection is found, resolves the access token for the connection.
        /// 4. Returns the access token.
        /// </remarks>
        public async Task<string> ResolveAccessTokenAsync(string providerName, string ownerEmail, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(providerName)) throw new ArgumentNullException(nameof(providerName));
            if (string.IsNullOrEmpty(ownerEmail)) throw new ArgumentNullException(nameof(ownerEmail));

            _logger.LogInformation("Resolving access token for provider {ProviderName} and owner {OwnerEmail}", providerName, ownerEmail);

            var conn = await GetConnectionAsync(providerName, ownerEmail, cancellationToken);
            if (conn == null)
            {
                _logger.LogWarning("Connection not found for provider {ProviderName} and owner {OwnerEmail}", providerName, ownerEmail);
                throw new InvalidOperationException("Connection not found");
            }

            _logger.LogInformation("Connection found for provider {ProviderName} and owner {OwnerEmail}", providerName, ownerEmail);
            return await ResolveAccessTokenAsync(conn, cancellationToken);
        }

        /// <summary>
        /// Resolves the access token for the given application connection.
        /// </summary>
        /// <param name="connection">The application connection to resolve the access token for.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The access token for the application connection.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the connection is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the access token is empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the configuration for the provider is not found or the refresh token is not available and the access token has expired.</exception>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Validates the input parameters.
        /// 2. Retrieves the OAuth configuration for the provider.
        /// 3. Checks if the access token has expired.
        /// 4. If the access token has expired, attempts to refresh the token using the refresh token.
        /// 5. Returns the access token.
        /// </remarks>
        public async Task<string> ResolveAccessTokenAsync(AppConnection connection, CancellationToken cancellationToken = default)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (string.IsNullOrEmpty(connection.AccessToken)) throw new ArgumentException("Access token is empty", nameof(connection.AccessToken));
            var config = _connectionConfigManager.GetConfiguration(connection.ProviderName);
            if (config == null) throw new InvalidOperationException($"Configuration for provider {connection.ProviderName} not found");

            if (connection.HasExpired)
            {
                if (string.IsNullOrEmpty(connection.RefreshToken))
                    throw new InvalidOperationException("Refresh token is not available and the access token has expired");

                connection = await RefreshTokenAsync(config, connection, cancellationToken);
            }

            return connection.AccessToken;
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

            var inContext = await GetConnectionAsync(connection.ProviderName, connection.OwnerEmail, cancellationToken);

            try
            {
                connection.Version++;
                if (inContext == null)
                {
                    connection.UtcCreatedOn = DateTime.UtcNow;
                    connection.CreatedBy = _userResolver.GetUserEmail();
                    connection.UpdatedBy = _userResolver.GetUserEmail();
                    connection.UtcUpdatedOn = DateTime.UtcNow;
                    _context.AppConnections.Add(connection);
                    _logger.LogInformation("New AppConnection created by {CreatedBy}", connection.CreatedBy);
                }
                else
                {
                    connection.Id = inContext.Id;
                    connection.UtcCreatedOn = inContext.UtcCreatedOn;
                    connection.CreatedBy = inContext.CreatedBy;
                    connection.UtcUpdatedOn = DateTime.UtcNow;
                    connection.UpdatedBy = _userResolver.GetUserEmail();

                    _context.Entry(inContext).CurrentValues.SetValues(connection);
                    _context.Entry(inContext).State = EntityState.Modified;

                    _logger.LogInformation("AppConnection updated by {UpdatedBy}", connection.UpdatedBy);
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
        /// <returns>The application connection that matches the specified provider name and owner email, or null if no match is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when providerName or ownerEmail is null or empty.</exception>
        public async Task<AppConnection?> GetConnectionAsync(string providerName, string ownerEmail, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(providerName)) throw new ArgumentNullException(nameof(providerName));
            if (string.IsNullOrEmpty(ownerEmail)) throw new ArgumentNullException(nameof(ownerEmail));

            return await _context.AppConnections
                .SingleOrDefaultAsync(c => c.ProviderName == providerName && c.OwnerEmail == ownerEmail, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves an application connection based on the connection ID.
        /// </summary>
        /// <param name="connectionId">The ID of the connection to retrieve.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The application connection that matches the specified connection ID, or null if no match is found.</returns>
        /// <exception cref="ArgumentException">Thrown when the connectionId is invalid.</exception>
        public async Task<AppConnection?> GetConnectionAsync(ulong connectionId, CancellationToken cancellationToken = default)
        {
            if (connectionId == 0) throw new ArgumentException("Invalid connection ID", nameof(connectionId));
            return await _context.AppConnections.FindAsync(new object[] { connectionId }, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an application connection based on the connection ID.
        /// </summary>
        /// <param name="connectionId">The ID of the connection to delete.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the connectionId is invalid.</exception>
        /// <exception cref="Exception">Thrown when an error occurs during the deletion process.</exception>
        public async Task DeleteConnectionAsync(ulong connectionId, CancellationToken cancellationToken = default)
        {
            if (connectionId == 0) throw new ArgumentException("Invalid connection ID", nameof(connectionId));

            try
            {
                var connection = await _context.AppConnections.FindAsync(new object[] { connectionId }, cancellationToken);
                if (connection == null)
                {
                    _logger.LogWarning("AppConnection with ID {0} not found", connectionId);
                    throw new InvalidOperationException(string.Format("AppConnection with ID {0} not found", connectionId));
                }

                _context.AppConnections.Remove(connection);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("AppConnection with ID {0} deleted successfully", connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the AppConnection with ID {0}", connectionId);
                throw;
            }
        }

        /// <summary>
        /// Retrieves an application connection based on the provider name and account ID.
        /// </summary>
        /// <param name="providerName">The name of the provider.</param>
        /// <param name="accountId">The account ID.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The application connection that matches the specified provider name and account ID.</returns>
        /// <exception cref="ArgumentNullException">Thrown when providerName is null or empty.</exception>
        public async Task<AppConnection?> GetConnectionAsync(string providerName, ulong accountId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(providerName)) throw new ArgumentNullException(nameof(providerName));

            return await _context.AppConnections
                .SingleAsync(c => c.ProviderName == providerName && c.AccountId == accountId, cancellationToken)
                .ConfigureAwait(false);
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
            var accountId = _userResolver.GetUser().AccountId;
            return await Task.Run(() => _context.AppConnections.Where(i => i.AccountId == accountId).Where(filterExpression), cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves all application connections for the current user's account.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A collection of application connections for the current user's account.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the user is not authenticated.</exception>
        /// <remarks>
        /// This method retrieves all application connections associated with the current user's account.
        /// It uses the user resolver to get the current user's account ID and filters the connections
        /// based on this account ID.
        /// </remarks>
        public async Task<IQueryable<AppConnection>> GetAllConnectionsAsync(CancellationToken cancellationToken = default)
        {
            var accountId = _userResolver.GetUser().AccountId;
            return await Task.Run(() => _context.AppConnections.Where(i => i.AccountId == accountId), cancellationToken)
                .ConfigureAwait(false);
        }



        /// <summary>
        /// Creates the authorization consent URL for the OAuth provider.
        /// </summary>
        /// <param name="config">The OAuth connection configuration.</param>
        /// <param name="baseUrl">The base URL of the application.</param>
        /// <returns>The authorization consent URL.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the config is null.</exception>
        public string CreateAuthorizationConsentUrl(OAuthConnectionConfig config, string baseUrl)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if(string.IsNullOrEmpty(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));

            var uri = new UriBuilder(baseUrl);
            uri.Path = config.RedirectUri;

            if(config.Name.ToLower().Equals("google")) return GetGoogleConsentUrl(config, uri.Uri.ToString());
            if (config.Name.ToLower().Equals("microsoft")) return GetMicrosoftConsentUrl(config, uri.Uri.ToString());

            throw new NotImplementedException($"The provider {config.Name} is not implemented");
        }

        private string GetGoogleConsentUrl(OAuthConnectionConfig config, string callback)
        {
            var stateCheck = OAuthStateCheck.Create(config.Name, config.Scopes).ToString();
            return $"{config.AuthorizationEndpoint}?response_type=code" +
                       $"&client_id={config.ClientId}" +
                       $"&redirect_uri={Uri.EscapeDataString(callback)}" +
                       $"&scope={Uri.EscapeDataString(config.Scopes)}" +
                       $"&state={stateCheck}" +
                       "&access_type=offline&prompt=consent";
        }

        private string GetMicrosoftConsentUrl(OAuthConnectionConfig config, string callback)
        {
            var stateCheck = OAuthStateCheck.Create(config.Name, config.Scopes).ToString();
            var newScopes = "offline_access " + config.Scopes;
            return $"{config.AuthorizationEndpoint}?response_type=code" +
                   $"&client_id={config.ClientId}" +
                   $"&redirect_uri={Uri.EscapeDataString(callback)}" +
                   $"&scope={Uri.EscapeDataString(newScopes)}" +
                   $"&state={stateCheck}" +
                   "&response_mode=query" +
                   "&prompt=consent";
        }

        /// <summary>
        /// Creates an authorization code request to exchange the code for an access token.
        /// </summary>
        /// <param name="config">The OAuth connection configuration.</param>
        /// <param name="code">The authorization code received from the OAuth provider.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The OAuth token response.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the config or code is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an error is received or the token request fails.</exception>
        public async Task<OAuthTokenResponse> CreateAuthorizationCodeRequestAsync(OAuthConnectionConfig config, string code, CancellationToken cancellationToken = default)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrEmpty(code)) throw new ArgumentNullException(nameof(code));

            try
            {
                var res = await _codeFlowService.PostAuthorizationCodeRequestAsync(config, code, cancellationToken);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get token. Status code: {0} and Message {1}", res.StatusCode, await res.Content.ReadAsStringAsync());
                    throw new InvalidOperationException("Failed to get token");
                }

                var tokenResponseContent = await res.Content.ReadAsStringAsync();

                _logger.LogDebug("Token response content: {TokenResponseContent}\n", tokenResponseContent);
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

        /// <summary>
        /// Retrieves the user information associated with the given OAuth access token.
        /// </summary>
        /// <param name="config">The OAuth connection configuration.</param>
        /// <param name="accessToken">The access token received from the OAuth provider.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The email address of the user if found, otherwise null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the config or accessToken is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an error occurs while retrieving user information.</exception>
        public async Task<string?> GetConnectionUserInformation(OAuthConnectionConfig config, string accessToken, CancellationToken cancellationToken = default)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrEmpty(accessToken)) throw new ArgumentNullException(nameof(accessToken));

            var res = await _codeFlowService.GetUserInformation(config, accessToken, cancellationToken);
            if (!res.IsSuccessStatusCode) return null;

            var content = await res.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var root = json.RootElement;
            JsonElement el;

            if (root.TryGetProperty("mail", out el))
                return el.GetString();
            if (root.TryGetProperty("email", out el))
                return el.GetString();

            return null;
        }

        /// <summary>
        /// Refreshes the OAuth token for the given application connection.
        /// </summary>
        /// <param name="config">The OAuth connection configuration.</param>
        /// <param name="connection">The application connection to refresh the token for.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The updated application connection with the refreshed token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the config or connection is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the refresh token is empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the token refresh request fails.</exception>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Validates the input parameters.
        /// 2. Sends a POST request to the token endpoint to exchange the refresh token for a new access token.
        /// 3. Parses the token response.
        /// 4. Updates the connection with the new token information.
        /// 5. Persists the updated connection to the database.
        /// </remarks>
        public async Task<AppConnection> RefreshTokenAsync(OAuthConnectionConfig config, AppConnection connection, CancellationToken cancellationToken = default)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (connection == null) throw new ArgumentException(nameof(connection));
            if (string.IsNullOrEmpty(connection.RefreshToken)) throw new ArgumentException("Refresh token is empty", nameof(connection.RefreshToken));

            try
            {
                var res = await _codeFlowService.PostRefreshTokenRequestAsync(config, connection.RefreshToken, cancellationToken);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get refreshed token. Status code: {StatusCode}", res.StatusCode);
                    throw new InvalidOperationException("Failed to get token refreshed");
                }

                var tokenResponseContent = await res.Content.ReadAsStringAsync();
                var tokenData = OAuthTokenResponse.Success(JsonDocument.Parse(tokenResponseContent));

                if (tokenData == null)
                {
                    _logger.LogError("Failed to parse token response");
                    throw new InvalidOperationException("Failed to parse token response");
                }

                _logger.LogInformation("Token successfully retrieved and parsed");
                var newConn = AppConnection.Create(tokenData, config, new AppUser()
                {
                    Email = connection.OwnerEmail,
                    AccountId = connection.AccountId
                });
                //updates the token information
                connection.AccessToken = newConn.AccessToken;
                
                if(!string.IsNullOrEmpty(newConn.RefreshToken))
                    connection.RefreshToken = newConn.RefreshToken;

                connection.DurationInSeconds = newConn.DurationInSeconds;
                connection.TokenType = newConn.TokenType;
                connection.TokenId = newConn.TokenId;
                connection.UtcIssuedOn = newConn.UtcIssuedOn;
                connection.Scope = newConn.Scope;

                //persists the connection
                return await PersistConnectionAsync(connection, cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the authorization code request");
                throw;
            }
        }
    }
}
