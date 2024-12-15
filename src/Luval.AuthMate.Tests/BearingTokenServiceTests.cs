using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Luval.AuthMate.Infrastructure.Logging;

namespace Luval.AuthMate.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="BearingTokenService"/> class.
    /// </summary>
    public class BearingTokenServiceTests
    {
        private const string SecretKey = "5e125c820b4b685e93eda324039b59e6749e27b15bd21d40ef38f23ee0c541d5";

        /// <summary>
        /// Creates an instance of <see cref="BearingTokenService"/> with an optional action to modify the context after creation.
        /// </summary>
        /// <param name="afterContextCreation">An action to modify the context after creation.</param>
        /// <returns>An instance of <see cref="BearingTokenService"/>.</returns>
        private BearingTokenService CreateService(Action<IAuthMateContext> afterContextCreation)
        {
            var context = new MemoryDataContext();
            context.Initialize();

            var userService = new AppUserService(context, new DebugLogger<AppUserService>());
            var service = new BearingTokenService(SecretKey, userService);

            afterContextCreation?.Invoke(context);
            return service;
        }

        [Fact]
        public async Task GenerateTokenForUserAsync_GeneratesTokenSuccessfully()
        {
            // Arrange
            var user = new AppUser();
            var service = CreateService((c) =>
            {
                user = c.AppUsers.First();
            });
            var tokenDuration = TimeSpan.FromHours(1);

            // Act
            var token = await service.GenerateTokenForUserAsync(user.Email, tokenDuration);

            // Assert
            Assert.NotNull(token);
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "Luval.AuthMate",
                ValidAudience = "Luval.AuthMate",
                IssuerSigningKey = securityKey
            }, out SecurityToken validatedToken);

            Assert.NotNull(validatedToken);
        }

        [Fact]
        public async Task GenerateTokenForUserAsync_ThrowsException_WhenUserEmailIsNull()
        {
            // Arrange
            var service = CreateService(null);
            var tokenDuration = TimeSpan.FromHours(1);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GenerateTokenForUserAsync(default(string), tokenDuration));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public async Task GenerateTokenForUserAsync_ThrowsException_WhenTokenDurationIsZero()
        {
            // Arrange
            var email = "testuser@example.com";
            var service = CreateService(null);
            var tokenDuration = TimeSpan.Zero;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GenerateTokenForUserAsync(email, tokenDuration));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public async Task GenerateTokenForUserAsync_ThrowsException_WhenUserNotFound()
        {
            // Arrange
            var email = "nonexistentuser@example.com";
            var service = CreateService(null);
            var tokenDuration = TimeSpan.FromHours(1);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GenerateTokenForUserAsync(email, tokenDuration));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public void GenerateRefreshToken_GeneratesTokenSuccessfully()
        {
            // Act
            var refreshToken = BearingTokenService.GenerateRefreshToken();

            // Assert
            Assert.NotNull(refreshToken);
            Assert.Equal(32, Convert.FromBase64String(refreshToken).Length);
        }
    }
}


