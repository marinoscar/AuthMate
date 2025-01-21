using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Represents the configuration settings for OAuth authentication.
/// </summary>
public class OAuthConfiguration
{
    private static readonly string _rootSection = "OAuthProviders";

    /// <summary>
    /// Gets or sets the name of the cookie used for authentication.
    /// </summary>
    public string CookieName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the login path for the authentication.
    /// </summary>
    public string LoginPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client ID for the OAuth application.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret for the OAuth application.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback path for the OAuth authentication.
    /// </summary>
    public string CallbackPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to save tokens.
    /// </summary>
    public bool SaveTokens { get; set; } = false;

    /// <summary>
    /// Gets or sets the access type for the OAuth authentication.
    /// </summary>
    public string? AccessType { get; set; } = "online";

    /// <summary>
    /// Gets or sets the authorization endpoint for the OAuth authentication.
    /// </summary>
    public string? AuthorizationEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the code flow redirect URI for the OAuth authentication.
    /// </summary>
    public string? CodeFlowRedirectUri { get; set; }

    /// <summary>
    /// Gets or sets the token endpoint for the OAuth authentication.
    /// </summary>
    public string? TokenEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the user information endpoint for the OAuth authentication.
    /// </summary>
    public string? UserInfoEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the list of scopes for the OAuth authentication.
    /// </summary>
    public List<string> Scopes { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the function to be called when creating the OAuth ticket.
    /// </summary>
    public Func<OAuthCreatingTicketContext, Task>? OnCreatingTicket { get; set; } = default;

    /// <summary>
    /// Creates an instance of <see cref="OAuthConfiguration"/> from the configuration section.
    /// </summary>
    /// <param name="config">The configuration instance.</param>
    /// <param name="name">The name of the OAuth provider.</param>
    /// <returns>An instance of <see cref="OAuthConfiguration"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the configuration section is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the configuration section cannot be processed.</exception>
    public static OAuthConfiguration CreateFromConfingSection(IConfiguration config, string name)
    {
        var fullName = $"{_rootSection}:{name}";
        var section = config.GetSection(fullName);
        if (section == null || section.GetChildren() == null || !section.GetChildren().Any())
            throw new ArgumentException($"There is no configuration for {fullName} in the configuration file");

        var configuration = section.Get<OAuthConfiguration>() ?? throw new InvalidOperationException($"Unable to process the information on {fullName}");

        return configuration;
    }

    /// <summary>
    /// Gets the Google OAuth configuration.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>An instance of <see cref="OAuthConfiguration"/> for Google.</returns>
    public static OAuthConfiguration GetGoogle(IConfiguration configuration)
    {
        return CreateFromConfingSection(configuration, "Google");
    }

    /// <summary>
    /// Gets the Microsoft OAuth configuration.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>An instance of <see cref="OAuthConfiguration"/> for Microsoft.</returns>
    public static OAuthConfiguration GetMicrosoft(IConfiguration configuration)
    {
        return CreateFromConfingSection(configuration, "Microsoft");
    }
}
