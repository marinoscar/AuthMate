using Luval.AuthMate.Core.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core.Services
{
    /// <summary>
    /// Service for generating and managing JWT tokens.
    /// </summary>
    public class BearingTokenService
    {
        private readonly string _secretKey;
        private readonly IAppUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BearingTokenService"/> class.
        /// </summary>
        /// <param name="secretKey">The secret key used for signing tokens.</param>
        /// <param name="userService">The user service for retrieving user information.</param>
        /// <exception cref="ArgumentNullException">Thrown when secretKey or userService is null.</exception>
        public BearingTokenService(string secretKey, IAppUserService userService)
        {
            _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        /// Generates a JWT token for a user.
        /// </summary>
        /// <param name="userEmail">The email of the user.</param>
        /// <param name="tokenDuration">The duration for which the token is valid.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The generated JWT token.</returns>
        /// <exception cref="ArgumentException">Thrown when userEmail is null or empty, or tokenDuration is less than or equal to zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the user with the specified email is not found.</exception>
        public async Task<string> GenerateTokenForUserAsync(string userEmail, TimeSpan tokenDuration, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                throw new ArgumentException("User email cannot be null or empty.", nameof(userEmail));

            if (tokenDuration <= TimeSpan.Zero)
                throw new ArgumentException("Token duration must be greater than zero.", nameof(tokenDuration));

            var user = await _userService.GetUserByEmailAsync(userEmail, cancellationToken);
            if (user == null)
                throw new InvalidOperationException($"User with email {userEmail} not found.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = user.ToIdentity(),
                Expires = DateTime.UtcNow.Add(tokenDuration),
                SigningCredentials = credentials,
                IssuedAt = DateTime.UtcNow,
                Issuer = "Luval.AuthMate",
                Audience = "Luval.AuthMate",
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generates a refresh token.
        /// </summary>
        /// <returns>The generated refresh token.</returns>
        public static string GenerateRefreshToken()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }
    }
}
