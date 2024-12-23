using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Infrastructure.Configuration
{

    /// <summary>
    /// Configuration settings for generating and validating bearer tokens.
    /// </summary>
    public class BearingTokenConfig
    {
        /// <summary>
        /// Gets or sets the secret key used for token generation.
        /// </summary>
        /// <value>
        /// A randomly generated base64 string used as the secret key.
        /// </value>
        public string Secret { get; set; } = GenerateRandomToken();

        /// <summary>
        /// Gets or sets the issuer of the token.
        /// </summary>
        /// <value>
        /// A string representing the issuer of the token. Default is "Luval.AuthMate".
        /// </value>
        public string Issuer { get; set; } = "Luval.AuthMate";

        /// <summary>
        /// Gets or sets the audience for the token.
        /// </summary>
        /// <value>
        /// A string representing the audience of the token. Default is "Luval.AuthMate".
        /// </value>
        public string Audience { get; set; } = "Luval.AuthMate";

        /// <summary>
        /// Gets or sets the default duration for which the token is valid.
        /// </summary>
        /// <value>
        /// A <see cref="TimeSpan"/> representing the default token duration. Default is 30 minutes.
        /// </value>
        public TimeSpan DefaultTokenDuration { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Generates a random token.
        /// </summary>
        /// <returns>
        /// A base64 encoded string representing the generated token.
        /// </returns>
        public static string GenerateRandomToken()
        {
            var bytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }
    }
}
