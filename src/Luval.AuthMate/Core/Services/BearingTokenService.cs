using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
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
        private readonly BearingTokenConfig _bearingTokenConfig;
        private readonly IAppUserService _userService;
        private readonly IAuthMateContext _context;
        private readonly ILogger<BearingTokenService> _logger;
        private readonly IUserResolver _userResolver;

        /// <summary>
        /// <see langword="short"/> representing the maximum number of active tokens per user.
        /// </summary>
        public short MaxActiveTokensPerUser { get; private set; } = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="BearingTokenService"/> class.
        /// </summary>
        /// <param name="bearingTokenConfig"><see cref="BearingTokenConfig"/> with the service configuration.</param>
        /// <param name="userService">The user service for retrieving user information.</param>
        /// <param name="context">The context for managing authentication-related entities.</param>
        /// <param name="logger">The logger for the service.</param>
        /// <exception cref="ArgumentNullException">Thrown when secretKey or userService is null.</exception>
        public BearingTokenService(BearingTokenConfig bearingTokenConfig, IAppUserService userService, IAuthMateContext context, IUserResolver userResolver, ILogger<BearingTokenService> logger)
        {
            _bearingTokenConfig = bearingTokenConfig ?? throw new ArgumentNullException(nameof(bearingTokenConfig));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userResolver = userResolver ?? throw new ArgumentNullException(nameof(userResolver));
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
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the generated JWT token.</returns>
        /// <exception cref="ArgumentException">Thrown when the user is null or the token duration is less than or equal to zero.</exception>
        public Task<string> GenerateTokenForUserAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            return GenerateTokenForUserAsync(user, _bearingTokenConfig.DefaultTokenDuration, cancellationToken);
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

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_bearingTokenConfig.Secret));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = user.ToIdentity(),
                    Expires = DateTime.UtcNow.Add(tokenDuration),
                    SigningCredentials = credentials,
                    IssuedAt = DateTime.UtcNow,
                    Issuer = _bearingTokenConfig.Issuer,
                    Audience = _bearingTokenConfig.Audience,
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation($"Token generated for user {user.Email}.");
                return tokenString;
            }, cancellationToken);
        }

        /// <summary>
        /// Creates a refresh token for a user.
        /// </summary>
        /// <param name="userEmail">The email of the user.</param>
        /// <param name="duration">The duration for which the refresh token is valid.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created refresh token.</returns>
        /// <exception cref="ArgumentException">Thrown when userEmail is null or empty, or duration is less than or equal to zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the user with the specified email is not found.</exception>
        /// <exception cref="AuthMateException">Thrown when the user has more than 10 active tokens</exception>
        public async Task<RefreshToken> CreateRefreshTokenAsync(string userEmail, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                _logger.LogError("User email cannot be null or empty.");
                throw new ArgumentException("User email cannot be null or empty.", nameof(userEmail));
            }

            if (duration <= TimeSpan.Zero)
            {
                _logger.LogError("Duration must be greater than zero.");
                throw new ArgumentException("Duration must be greater than zero.", nameof(duration));
            }

            try
            {
                var user = await _userService.TryGetUserByEmailAsync(userEmail, cancellationToken).ConfigureAwait(true);
                if (user == null)
                {
                    _logger.LogError($"User with email {userEmail} not found.");
                    throw new InvalidOperationException($"User with email {userEmail} not found.");
                }

                ///checks how many active tokens the user have, the max allowed per user is 10
                var activeTokens = await _context.RefreshTokens
                    .Where(t => t.AppUserId == user.Id && t.IsValid)
                    .CountAsync(cancellationToken)
                    .ConfigureAwait(true);

                if (activeTokens >= MaxActiveTokensPerUser)
                {
                    _logger.LogError($"User with email {userEmail} has {activeTokens} which is the max allowed");
                    throw new AuthMateException($"User with email {userEmail} has {activeTokens} which is the max allowed");
                }

                var token = new RefreshToken
                {
                    AppUserId = user.Id,
                    Token = GenerateRandomToken(),
                    DurationInSeconds = (ulong)duration.TotalSeconds,
                    UtcExpiresOn = DateTime.UtcNow.Add(duration).ForceUtc(),
                    IsValid = true,
                    CreatedBy = _userResolver.GetUserEmail(),
                    UpdatedBy = _userResolver.GetUserEmail(),
                    Version = 1
                };

                _context.RefreshTokens.Add(token);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(true);

                _logger.LogInformation($"Refresh token created for user {userEmail}.");
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a refresh token.");
                throw;
            }
        }

        /// <summary>
        /// Generates a JWT token for a user from a refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The generated JWT token.</returns>
        /// <exception cref="ArgumentException">Thrown when refreshToken is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the refresh token is not found.</exception>
        /// <exception cref="AuthMateException">Thrown when the refresh token is no longer valid or has expired.</exception>
        public async Task<string> GenerateTokenForUserFromRefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogError("Refresh token cannot be null or empty.");
                throw new ArgumentException("Refresh token cannot be null or empty.", nameof(refreshToken));
            }

            try
            {
                var token = await _context.RefreshTokens
                    .Include(x => x.User)
                    .SingleOrDefaultAsync(t => t.Token == refreshToken, cancellationToken)
                    .ConfigureAwait(true);

                if (token == null)
                {
                    _logger.LogError($"Refresh token {refreshToken} not found.");
                    throw new InvalidOperationException($"Refresh token {refreshToken} not found.");
                }
                if (!token.IsValid)
                {
                    _logger.LogError($"Refresh token {refreshToken} is no longer valid.");
                    throw new AuthMateException($"Refresh token {refreshToken} is no longer valid.");
                }
                if (token.UtcExpiresOn < DateTime.UtcNow)
                {
                    _logger.LogError($"Refresh token {refreshToken} has expired");
                    throw new AuthMateException($"Refresh token {refreshToken} has expired");
                }

                //Get user from the token, to include roles and other information
                var user = await _userService.GetUserByEmailAsync(token.User.Email, cancellationToken).ConfigureAwait(true);
                token.User = user;

                var result = await GenerateTokenForUserAsync(token.User, TimeSpan.FromMinutes(15), cancellationToken).ConfigureAwait(true);

                //updates the token to be invalid
                token.IsValid = false;
                token.UpdatedBy = _userResolver.GetUserEmail();
                token.UtcUpdatedOn = DateTime.UtcNow;
                token.Version++;

                _context.RefreshTokens.Update(token);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(true);

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating a token from the refresh token.");
                throw;
            }
        }

    }
}
