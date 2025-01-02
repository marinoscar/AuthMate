using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Infrastructure.Configuration
{
    /// <summary>
    /// Represents a manager for OAuth provider configurations.
    /// </summary>
    public class OAuthConnectionManager
    {
        private IEnumerable<OAuthConnectionConfig> _connections;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthConnectionManager"/> class using the specified configuration.
        /// </summary>
        /// <param name="configuration">The configuration instance to load OAuth provider settings from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the configuration parameter is null.</exception>
        /// <remarks>
        /// This constructor initializes the <see cref="OAuthConnectionManager"/> with an <see cref="IConfiguration"/> instance.
        /// It first checks if the provided configuration is null and throws an <see cref="ArgumentNullException"/> if it is.
        /// Then, it calls the <see cref="LoadConfigurations"/> method to load the OAuth provider configurations from the configuration instance.
        /// The <see cref="LoadConfigurations"/> method retrieves the "OAuthProviders" section from the configuration and iterates through its children to create a collection of <see cref="OAuthConnectionConfig"/> instances.
        /// If the "OAuthProviders" section is not found or contains no children, appropriate exceptions are thrown.
        /// </remarks>
        public OAuthConnectionManager(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            LoadConfigurations();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthConnectionManager"/> class using the specified collection of OAuth connection configurations.
        /// </summary>
        /// <param name="connections">The collection of OAuth connection configurations.</param>
        /// <exception cref="ArgumentNullException">Thrown when the connections parameter is null.</exception>
        /// <remarks>
        /// This constructor initializes the <see cref="OAuthConnectionManager"/> with a collection of <see cref="OAuthConnectionConfig"/> instances.
        /// It first checks if the provided collection is null and throws an <see cref="ArgumentNullException"/> if it is.
        /// </remarks>
        public OAuthConnectionManager(IEnumerable<OAuthConnectionConfig> connections)
        {
            _connections = connections ?? throw new ArgumentNullException(nameof(connections));
        }

        /// <summary>
        /// Retrieves the OAuth connection configuration for the specified provider.
        /// </summary>
        /// <param name="providerName">The name of the OAuth provider.</param>
        /// <returns>The <see cref="OAuthConnectionConfig"/> instance for the specified provider.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no configuration is found for the specified provider.</exception>
        public OAuthConnectionConfig GetConfiguration(string providerName)
        {
            return _connections.Single(c => c.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));
        }


        /// <summary>
        /// Loads the OAuth provider configurations from the configuration instance.
        /// </summary>
        /// <remarks>
        /// This method retrieves the "OAuthProviders" section from the configuration and iterates through its children to create a collection of <see cref="OAuthConnectionConfig"/> instances.
        /// If the "OAuthProviders" section is not found or contains no children, appropriate exceptions are thrown.
        /// Each child configuration is mapped to an <see cref="OAuthConnectionConfig"/> instance with properties such as Name, AuthorizationEndpoint, ClientId, ClientSecret, RedirectUri, Scopes, TokenEndpoint, and UserInfoEndpoint.
        /// </remarks>
        protected virtual void LoadConfigurations()
        {
            var section = _configuration.GetSection("OAuthProviders");
            if (section == null) throw new InvalidOperationException("OAuthProviders section not found in configuration.");
            var children = section.GetChildren();
            if (children == null || !children.Any()) throw new InvalidCastException("No OAuthProviders found in configuration.");
            _connections = children.Select(c => new OAuthConnectionConfig
            {
                Name = section.Key,
                AuthorizationEndpoint = c["AuthorizationEndpoint"] ?? "",
                ClientId = c["ClientId"] ?? "",
                ClientSecret = c["ClientSecret"] ?? "",
                RedirectUri = c["CodeFlowRedirectUri"] ?? "",
                Scopes = c["Scopes"] ?? "",
                TokenEndpoint = c["TokenEndpoint"] ?? "",
                UserInfoEndpoint = c["UserInfoEndpoint"] ?? ""
            });
        }
    }
}
