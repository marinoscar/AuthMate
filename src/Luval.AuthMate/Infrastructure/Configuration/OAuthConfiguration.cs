using Microsoft.AspNetCore.Authentication.OAuth;

/// <summary>
/// Represents the configuration settings for OAuth authentication.
/// </summary>
public class OAuthConfiguration
{
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
    /// Gets or sets the list of scopes for the OAuth authentication.
    /// </summary>
    public List<string> Scope { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the function to be called when creating the OAuth ticket.
    /// </summary>
    public Func<OAuthCreatingTicketContext, Task>? OnCreatingTicket { get; set; } = default;
}
