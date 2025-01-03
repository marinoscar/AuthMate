using Luval.AuthMate.Core;
using Luval.AuthMate.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luval.AuthMate.Infrastructure.Configuration
{
    /// <summary>
    /// Represents a check for OAuth state, including provider name, date, and additional data.
    /// </summary>
    public class OAuthStateCheck
    {
        /// <summary>
        /// Gets or sets the name of the OAuth provider.
        /// </summary>
        public string? ProviderName { get; set; }

        /// <summary>
        /// Gets or sets the date of the OAuth state check.
        /// </summary>
        public string? Date { get; set; }

        /// <summary>
        /// Gets or sets additional data for the OAuth state check.
        /// </summary>
        public Dictionary<string, string>? AdditionalData { get; set; }

        /// <summary>
        /// Private constructor to prevent direct instantiation.
        /// </summary>
        private OAuthStateCheck()
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="OAuthStateCheck"/> with the specified provider name.
        /// </summary>
        /// <param name="providerName">The name of the OAuth provider.</param>
        /// <returns>A new instance of <see cref="OAuthStateCheck"/>.</returns>
        public OAuthStateCheck Create(string providerName)
        {
            return new OAuthStateCheck()
            {
                ProviderName = providerName,
                Date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };
        }

        /// <summary>
        /// Converts the current instance to a base64 encoded string.
        /// </summary>
        /// <returns>A base64 encoded string representation of the current instance.</returns>
        public override string ToString()
        {
            return this.ToJson().ToBase64();
        }

        /// <summary>
        /// Creates an instance of <see cref="OAuthStateCheck"/> from a base64 encoded string.
        /// </summary>
        /// <param name="value">The base64 encoded string.</param>
        /// <returns>An instance of <see cref="OAuthStateCheck"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the value cannot be parsed.</exception>
        public static OAuthStateCheck FromString(string value)
        {
            var bytes = Convert.FromBase64String(value);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<OAuthStateCheck>(json) ?? throw new ArgumentException("Unable to parse the value argument");
        }

        /// <summary>
        /// Validates the OAuth state check based on the provided base64 encoded string.
        /// </summary>
        /// <param name="base64">The base64 encoded string representing the OAuth state check.</param>
        /// <returns>True if the OAuth state check is valid; otherwise, false.</returns>
        public bool IsValid(string base64)
        {
            var providers = new List<string> { "google", "facebook", "microsoft", "twitter", "github", "reddit", "amazon" };
            try
            {
                var val = FromString(base64);
                if (!providers.Contains(val.ProviderName.ToLower())) return false;
                var startTime = DateTime.UtcNow.AddHours(-2);
                var endTime = DateTime.UtcNow.AddHours(2);
                var checkTime = DateTime.Parse(val.Date);
                if (checkTime < startTime || checkTime > endTime)
                    return false;
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
