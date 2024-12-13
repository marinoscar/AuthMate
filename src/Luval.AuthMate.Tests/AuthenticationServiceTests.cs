using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Luval.AuthMate.Infrastructure.Logging;

namespace Luval.AuthMate.Tests
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<IAppUserService> _mockUserService;
        private readonly Mock<IAuthMateContext> _mockContext;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly AuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            _logger = new ColorConsoleLogger<AuthenticationService>();
            _mockContext = new Mock<IAuthMateContext>();
            _mockUserService = new Mock<IAppUserService>();
            
            

            _authService = new AuthenticationService(
                _mockUserService.Object,
                _mockContext.Object,
                _logger
            );
        }

        [Fact]
        public async Task AuthorizeUserAsync_ThrowsArgumentNullException_WhenIdentityIsNull()
        {
            // Arrange
            ClaimsIdentity identity = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _authService.AuthorizeUserAsync(identity));
        }

        [Fact]
        public async Task AuthorizeUserAsync_ThrowsArgumentException_WhenEmailIsMissing()
        {
            // Arrange
            var identity = new ClaimsIdentity();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.AuthorizeUserAsync(identity));
        }

        [Fact]
        public async Task AuthorizeUserAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var email = "test@example.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) });
            var existingUser = new AppUser { Email = email };

            _mockUserService
                .Setup(s => s.TryGetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authService.AuthorizeUserAsync(identity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task AuthorizeUserAsync_CreatesUserFromInvitation_WhenAccountInvitationExists()
        {
            // Arrange
            var email = "invited@example.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) });
            var accountInvite = new InviteToAccount { Email = email };
            var newUser = new AppUser { Email = email };

            _mockUserService
                .Setup(s => s.TryGetUserByEmailAsync(email, CancellationToken.None))
                .ReturnsAsync((AppUser)null);

            _mockContext
                .Setup(c => c.InvitesToAccount.Include(It.IsAny<string>()).FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<InviteToAccount, bool>>>(), CancellationToken.None))
                .ReturnsAsync(accountInvite);

            _mockUserService
                .Setup(s => s.CreateUserFromInvitationAsync(accountInvite, identity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(newUser);

            // Act
            var result = await _authService.AuthorizeUserAsync(identity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task ValidateInvitationToAccount_ReturnsInvite_WhenInviteExists()
        {
            // Arrange
            var email = "accountinvite@example.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) });
            var expectedInvite = new InviteToAccount { Email = email };

            _mockContext
                .Setup(c => c.InvitesToAccount.Include(It.IsAny<string>()).FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<InviteToAccount, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedInvite);

            // Act
            var result = await _authService.ValidateInvitationToAccount(identity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task ValidateInvitationToApplication_ReturnsNull_WhenInviteDoesNotExist()
        {
            // Arrange
            var email = "noinvite@example.com";

            _mockContext
                .Setup(c => c.InvitesToApplication.Include(It.IsAny<string>()).FirstOrDefaultAsync(
                    It.IsAny<Expression<Func<InviteToApplication, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InviteToApplication)null);

            // Act
            var result = await _authService.ValidateInvitationToApplication(email);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddLogHistoryAsync_ThrowsArgumentException_WhenEmailIsEmpty()
        {
            // Arrange
            var deviceInfo = new DeviceInfo();
            string email = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _authService.AddLogHistoryAsync(deviceInfo, email));
        }
    }
}
