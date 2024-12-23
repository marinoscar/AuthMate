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
using Luval.AuthMate.Core;
using System.Security.Claims;

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

            var userService = new AppUserService(context, new NullUserResolver(), new DebugLogger<AppUserService>());
            var service = new BearingTokenService(SecretKey, userService, context, new NullUserResolver(), new DebugLogger<BearingTokenService>());

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
            var refreshToken = BearingTokenService.GenerateRandomToken();

            // Assert
            Assert.NotNull(refreshToken);
            Assert.Equal(64, Convert.FromBase64String(refreshToken).Length);
        }
        [Fact]
        public async Task CreateRefreshTokenAsync_CreatesTokenSuccessfully()
        {
            // Arrange
            var email = "testuser@example.com";
            var service = CreateService((c) =>
            {
                c.AppUsers.Add(new AppUser { ProviderKey = "Key", ProviderType = "Google", Email = email, AccountId = c.Accounts.First().Id });
                c.SaveChanges();
            });
            var tokenDuration = TimeSpan.FromDays(30);

            // Act
            var refreshToken = await service.CreateRefreshTokenAsync(email, tokenDuration);

            // Assert
            Assert.NotNull(refreshToken);
            Assert.Equal(email, refreshToken.User.Email);
            Assert.True(refreshToken.UtcExpiresOn > DateTime.UtcNow);
            Assert.True(refreshToken.IsValid);
        }

        [Fact]
        public async Task CreateRefreshTokenAsync_ThrowsException_WhenUserEmailIsNull()
        {
            // Arrange
            var service = CreateService(null);
            var tokenDuration = TimeSpan.FromDays(30);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateRefreshTokenAsync(null, tokenDuration));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public async Task CreateRefreshTokenAsync_ThrowsException_WhenTokenDurationIsZero()
        {
            // Arrange
            var email = "testuser@example.com";
            var service = CreateService(null);
            var tokenDuration = TimeSpan.Zero;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateRefreshTokenAsync(email, tokenDuration));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public async Task CreateRefreshTokenAsync_ThrowsException_WhenUserNotFound()
        {
            // Arrange
            var email = "nonexistentuser@example.com";
            var service = CreateService(null);
            var tokenDuration = TimeSpan.FromDays(30);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateRefreshTokenAsync(email, tokenDuration));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public async Task CreateRefreshTokenAsync_FailsToCreateTokenWhenThereAreManyActive()
        {
            // Arrange
            var email = "testuser@example.com";
            Exception exception = null; 
            var service = CreateService((c) =>
            {
                var acId = c.Accounts.First().Id;
                c.AppUsers.Add(new AppUser { ProviderKey = "Key1", ProviderType = "Google", Email = email, AccountId = acId });
                c.SaveChanges();
            });
            var tokenDuration = TimeSpan.FromDays(30);

            // Act
            for (int i = 0; i < service.MaxActiveTokensPerUser; i++)
            {
                //Create 10 tokesn
                exception = await Record.ExceptionAsync(async () => await service.CreateRefreshTokenAsync(email, tokenDuration));
            }


            // Assert
            Assert.Null(exception);
            //Run the test and assert that the 11th token fails
            await Assert.ThrowsAsync<AuthMateException>(async () => await service.CreateRefreshTokenAsync(email, tokenDuration));
        }
        [Fact]
        public async Task GenerateTokenForUserFromRefreshAsync_GeneratesTokenSuccessfully()
        {
            // Arrange
            var testUser = default(AppUser);
            var service = CreateService((c) =>
            {
                testUser = c.AppUsers.First();  
            });

            var refreshToken = await service.CreateRefreshTokenAsync(testUser.Email, TimeSpan.FromDays(30));

            // Act
            var token = await service.GenerateTokenForUserFromRefreshAsync(refreshToken.Token);

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

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var roles = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role").ToList();
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value;

            //Token must have a role
            Assert.NotEmpty(roles);
            //Token must have the user email
            Assert.Equal(testUser.Email, email);
        }

        [Fact]
        public async Task GenerateTokenForUserFromRefreshAsync_ThrowsException_WhenRefreshTokenIsNull()
        {
            // Arrange
            var service = CreateService(null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GenerateTokenForUserFromRefreshAsync(null));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public async Task GenerateTokenForUserFromRefreshAsync_ThrowsException_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            var service = CreateService(null);
            var invalidRefreshToken = "invalid_refresh_token";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateTokenForUserFromRefreshAsync(invalidRefreshToken));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public async Task GenerateTokenForUserFromRefreshAsync_ThrowsException_WhenRefreshTokenIsExpired()
        {
            // Arrange
            var email = "testuser@example.com";
            var testToken = default(RefreshToken);
            var service = CreateService((c) =>
            {
                var user = new AppUser { ProviderKey = "Key", ProviderType = "Google", Email = email, AccountId = c.Accounts.First().Id };
                c.AppUsers.Add(user);
                c.SaveChanges();
                var refreshToken = new RefreshToken
                {
                    AppUserId = user.Id,
                    Token = BearingTokenService.GenerateRandomToken(),
                    UtcExpiresOn = DateTime.UtcNow.AddDays(-1),
                    DurationInSeconds = (ulong)TimeSpan.FromHours(24).TotalSeconds,
                    IsValid = true
                };
                c.RefreshTokens.Add(refreshToken);
                c.SaveChanges();
                testToken = refreshToken;
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AuthMateException>(() => service.GenerateTokenForUserFromRefreshAsync(testToken.Token));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }




    }
}


