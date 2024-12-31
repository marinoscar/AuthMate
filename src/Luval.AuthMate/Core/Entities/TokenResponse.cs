using Microsoft.AspNetCore.Authentication.OAuth;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Luval.AuthMate.Core.Entities
{
    /// <summary>  
    /// Represents a response containing OAuth tokens.  
    /// </summary>  
    public class TokenResponse
    {
        /// <summary>  
        /// Gets or sets the access token.  
        /// </summary>  
        public string? AccessToken { get; set; } = default!;

        /// <summary>  
        /// Gets or sets the refresh token.  
        /// </summary>  
        public string? RefreshToken { get; set; } = default!;

        /// <summary>  
        /// Gets or sets the type of the token.  
        /// </summary>  
        public string? TokenType { get; set; } = default!;

        /// <summary>  
        /// Gets or sets the number of seconds until the token expires.  
        /// </summary>  
        public int? ExpiresIn { get; set; }

        /// <summary>  
        /// Gets or sets the UTC date and time when the token expires.  
        /// </summary>  
        public DateTime? UtcExpiresAt { get; set; }

        /// <summary>  
        /// Creates a new instance of <see cref="TokenResponse"/> from an <see cref="OAuthTokenResponse"/>.  
        /// </summary>  
        /// <param name="response">The OAuth token response.</param>  
        /// <returns>A new instance of <see cref="TokenResponse"/>.</returns>  
        /// <exception cref="Exception">Thrown when the response contains an error.</exception>  
        public static TokenResponse Create(OAuthTokenResponse response)
        {
            if (response.Error != null)
                throw response.Error;
            return new TokenResponse
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                TokenType = response.TokenType,
                ExpiresIn = response.ExpiresIn != null ? int.Parse(response.ExpiresIn) : (int?)null,
                UtcExpiresAt = response.ExpiresIn != null ? DateTime.UtcNow.AddSeconds(int.Parse(response.ExpiresIn)) : (DateTime?)null
            };
        }

        /// <summary>
        /// Creates a new instance of <see cref="TokenResponse"/> from a <see cref="JsonDocument"/>.
        /// </summary>
        /// <param name="json">The JSON document containing the token response data.</param>
        /// <returns>A new instance of <see cref="TokenResponse"/>.</returns>
        public static TokenResponse Create(JsonDocument json)
        {
            return Create(OAuthTokenResponse.Success(json));
        }

        /// <summary>
        /// Creates a new instance of <see cref="TokenResponse"/> from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string containing the token response data.</param>
        /// <returns>A new instance of <see cref="TokenResponse"/>.</returns>
        public static TokenResponse Create(string json)
        {
            return Create(JsonDocument.Parse(json));
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>A JSON-formatted string representing the object.</returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });
        }
    }
}
