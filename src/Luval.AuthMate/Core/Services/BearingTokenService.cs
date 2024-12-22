using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Microsoft.Extensions.Logging;
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
        private readonly IAuthMateContext _context;
        private readonly ILogger<BearingTokenService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BearingTokenService"/> class.
        /// </summary>
        /// <param name="secretKey">The secret key used for signing tokens.</param>
        /// <param name="userService">The user service for retrieving user information.</param>
        /// <param name="context">The context for managing authentication-related entities.</param>
        /// <param name="logger">The logger for the service.</param>
        /// <exception cref="ArgumentNullException">Thrown when secretKey or userService is null.</exception>
        public BearingTokenService(string secretKey, IAppUserService userService, IAuthMateContext context, ILogger<BearingTokenService> logger)
        {
            _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            {
                _logger.LogError("User email cannot be null or empty.");
                throw new ArgumentException("User email cannot be null or empty.", nameof(userEmail));
            }

            var user = await _userService.GetUserByEmailAsync(userEmail, cancellationToken);
            if (user == null)
            {
                _logger.LogError($"User with email {userEmail} not found.");
                throw new InvalidOperationException($"User with email {userEmail} not found.");
            }

            _logger.LogInformation($"Generating token for user with email {userEmail}.");
            return await GenerateTokenForUserAsync(user, tokenDuration, cancellationToken);
        }

        ///<summary>
        /// Generates a JWT token for a user.
        /// </summary>
        /// <param name="user">The user entity for which the token is generated.</param>
        /// <param name="tokenDuration">The duration for which the token is valid.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the generated JWT token.</returns>
        /// <exception cref="ArgumentException">Thrown when the user is null or the token duration is less than or equal to zero.</exception>
        public Task<string> GenerateTokenForUserAsync(AppUser user, TimeSpan tokenDuration, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                if (user == null)
                {
                    _logger.LogError("User cannot be null.");
                    throw new ArgumentException("User cannot be null.", nameof(user));
                }

                if (tokenDuration <= TimeSpan.Zero)
                {
                    _logger.LogError("Token duration must be greater than zero.");
                    throw new ArgumentException("Token duration must be greater than zero.", nameof(tokenDuration));
                }

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
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation($"Token generated for user {user.Email}.");
                return tokenString;
            }, cancellationToken);
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
