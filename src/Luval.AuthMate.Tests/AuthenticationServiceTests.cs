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
using Luval.AuthMate.Core;

namespace Luval.AuthMate.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="AuthenticationService"/> class.
    /// </summary>
    public class AuthenticationServiceTests
    {
        /// <summary>
        /// Creates an instance of <see cref="AuthenticationService"/> with an optional action to modify the context after creation.
        /// </summary>
        /// <param name="afterContextCreation">An action to modify the context after creation.</param>
        /// <returns>An instance of <see cref="AuthenticationService"/>.</returns>
        private AuthenticationService CreateService(Action<IAuthMateContext> afterContextCreation)
        {
            var context = new MemoryDataContext();
            context.Initialize();

            var userService = new AppUserService(context, new NullLogger<AppUserService>());
            var authService = new AuthenticationService(userService, context, new NullLogger<AuthenticationService>());

            if (afterContextCreation != null) afterContextCreation(context);
            return authService;
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.AuthorizeUserAsync(ClaimsIdentity, DeviceInfo, CancellationToken)"/> returns a user when the user exists.
        /// </summary>
        [Fact]
        public async Task AuthorizeUserAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var email = "owner@email.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) });

            // Creates a new instance of the service and then it adds the user to the context
            var service = CreateService((c) =>
            {
            });

            // Act
            var result = await service.AuthorizeUserAsync(identity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.AuthorizeUserAsync(ClaimsIdentity, DeviceInfo, CancellationToken)"/> throws <see cref="ArgumentNullException"/> when the identity is null.
        /// </summary>
        [Fact]
        public async Task AuthorizeUserAsync_ThrowsArgumentNullException_WhenIdentityIsNull()
        {
            // Arrange
            var service = CreateService(null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.AuthorizeUserAsync(null));
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.AuthorizeUserAsync(ClaimsIdentity, DeviceInfo, CancellationToken)"/> throws <see cref="AuthMateException"/> when the user does not exist.
        /// </summary>
        [Fact]
        public async Task AuthorizeUserAsync_ThrowsAuthMateException_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "nonexistent@email.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) });

            var service = CreateService(null);

            // Act & Assert
            await Assert.ThrowsAsync<AuthMateException>(() => service.AuthorizeUserAsync(identity));
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.ValidateInvitationToAccount(ClaimsIdentity, CancellationToken)"/> returns an invite when the invite exists.
        /// </summary>
        [Fact]
        public async Task ValidateInvitationToAccount_ReturnsInvite_WhenInviteExists()
        {
            // Arrange
            var email = "invitee@email.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) });

            var service = CreateService((c) =>
            {
                var a = c.Accounts.FirstOrDefault();
                var r = c.Roles.FirstOrDefault();
                var i = new InviteToAccount { Email = email, AccountId = a.Id, Account = a, RoleId = r.Id, Role = r };
                c.InvitesToAccount.Add(i);
                c.SaveChanges();
            });

            // Act
            var result = await service.ValidateInvitationToAccount(identity);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Account);
            Assert.NotNull(result.Role);
            Assert.Equal(email, result.Email);
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.ValidateInvitationToAccount(ClaimsIdentity, CancellationToken)"/> returns null when the invite does not exist.
        /// </summary>
        [Fact]
        public async Task ValidateInvitationToAccount_ReturnsNull_WhenInviteDoesNotExist()
        {
            // Arrange
            var email = "nonexistent@email.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) });

            var service = CreateService(null);

            // Act
            var result = await service.ValidateInvitationToAccount(identity);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.ValidateInvitationToApplication(string, CancellationToken)"/> returns an invite when the invite exists.
        /// </summary>
        [Fact]
        public async Task ValidateInvitationToApplication_ReturnsInvite_WhenInviteExists()
        {
            // Arrange
            var email = "invitee@app.com";

            var service = CreateService((c) =>
            {
                var at = c.AccountTypes.First();
                c.InvitesToApplication.Add(new InviteToApplication { Email = email, AccountTypeId = at.Id });
                c.SaveChanges();
            });

            // Act
            var result = await service.ValidateInvitationToApplication(email);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AccountType);
            Assert.Equal(email, result.Email);
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.ValidateInvitationToApplication(string, CancellationToken)"/> returns null when the invite does not exist.
        /// </summary>
        [Fact]
        public async Task ValidateInvitationToApplication_ReturnsNull_WhenInviteDoesNotExist()
        {
            // Arrange
            var email = "nonexistent@app.com";

            var service = CreateService(null);

            // Act
            var result = await service.ValidateInvitationToApplication(email);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.AuthorizeUserAsync(ClaimsIdentity, DeviceInfo, CancellationToken)"/> throws <see cref="ArgumentException"/> when the identity does not have an email claim.
        /// </summary>
        [Fact]
        public async Task AuthorizeUserAsync_ThrowsArgumentException_WhenIdentityDoesNotHaveEmailClaim()
        {
            // Arrange
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "username") });
            var service = CreateService(null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.AuthorizeUserAsync(identity));
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.AuthorizeUserAsync(ClaimsIdentity, DeviceInfo, CancellationToken)"/> updates the <see cref="AppUser"/> entity with the <see cref="AppUser.UtcLastLogin"/> field and the <see cref="AppUser.Version"/> field.
        /// </summary>
        [Fact]
        public async Task AuthorizeUserAsync_UpdatesUserWithUtcLastLoginAndVersion_WhenUserIsValid()
        {
            // Arrange
            var email = "validuser@email.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) });
            var initialVersion = 1u;

            var service = CreateService((c) =>
            {
                var account = c.Accounts.FirstOrDefault();
                var user = new AppUser
                {
                    ProviderKey = "randomkey",
                    ProviderType = "randomtype",
                    Email = email,
                    AccountId = account.Id,
                    Account = account,
                    UtcLastLogin = DateTime.UtcNow.AddDays(-1), // Set to a past date
                    Version = initialVersion
                };
                c.AppUsers.Add(user);
                c.SaveChanges();
            });

            // Act
            var result = await service.AuthorizeUserAsync(identity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            Assert.True(result.UtcLastLogin > DateTime.UtcNow.AddMinutes(-3)); // Ensure UtcLastLogin is updated to a recent time
            Assert.Equal(initialVersion + 1, result.Version); // Ensure Version is incremented
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.AuthorizeUserAsync(ClaimsIdentity, DeviceInfo, CancellationToken)"/> creates a row in the <see cref="AppUserLoginHistory"/> entity for a valid user.
        /// </summary>
        [Fact]
        public async Task AuthorizeUserAsync_CreatesLoginHistory_WhenUserIsValid()
        {
            // Arrange
            var email = "validuser@email.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) });
            var deviceInfo = new DeviceInfo("127.0.0.1", "Windows 10", "Chrome");
            IAuthMateContext context = default;

            var service = CreateService((c) =>
            {
                var account = c.Accounts.FirstOrDefault();
                var user = new AppUser
                {
                    ProviderKey = "randomkey",
                    ProviderType = "randomtype",
                    Email = email,
                    AccountId = account.Id,
                    Account = account,
                    UtcLastLogin = DateTime.UtcNow.AddDays(-1), // Set to a past date
                    Version = 1u
                };
                c.AppUsers.Add(user);
                c.SaveChanges();
                context = c;
            });

            // Act
            var result = await service.AuthorizeUserAsync(identity, deviceInfo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);

            var loginHistory = context.AppUserLoginHistories.FirstOrDefault(lh => lh.Email == email);

            Assert.NotNull(loginHistory);
            Assert.Equal(email, loginHistory.Email);
            Assert.Equal(deviceInfo.IpAddress, loginHistory.IpAddress);
            Assert.Equal(deviceInfo.OS, loginHistory.OS);
            Assert.Equal(deviceInfo.Browser, loginHistory.Browser);
            Assert.True(loginHistory.UtcLogIn > DateTime.UtcNow.AddMinutes(-3)); // Ensure UtcLogIn is recent
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.AuthorizeUserAsync(ClaimsIdentity, DeviceInfo, CancellationToken)"/> creates a new <see cref="AppUser"/> entity when an <see cref="InviteToApplication"/> is provided for a new user.
        /// </summary>
        [Fact]
        public async Task AuthorizeUserAsync_CreatesNewAppUser_WhenInviteToApplicationIsProvided()
        {
            // Arrange
            var email = "newuser@app.com";
            var identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, email)
            }, "Google");
            AccountType at = null;

            var service = CreateService((c) =>
            {
                at = c.AccountTypes.First();
                c.InvitesToApplication.Add(new InviteToApplication
                {
                    Email = email,
                    AccountTypeId = at.Id,
                    AccountType = at,
                    UtcExpiration = DateTime.UtcNow.AddDays(7),
                    UtcCreatedOn = DateTime.UtcNow,
                    UtcUpdatedOn = DateTime.UtcNow
                });
                c.SaveChanges();
            });

            // Act
            var result = await service.AuthorizeUserAsync(identity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            Assert.Equal(at.Id, result.Account.AccountTypeId);
            Assert.True(result.UserRoles.Count >= 1);
            Assert.Equal(result.Account.Owner, email);
            Assert.NotNull(result.Account);
            Assert.True(result.UtcCreatedOn > DateTime.UtcNow.AddMinutes(-3)); // Ensure UtcCreatedOn is recent
            Assert.True(result.UtcUpdatedOn > DateTime.UtcNow.AddMinutes(-3)); // Ensure UtcUpdatedOn is recent
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.AuthorizeUserAsync(ClaimsIdentity, DeviceInfo, CancellationToken)"/> creates a new <see cref="AppUser"/> entity when an <see cref="InviteToAccount"/> is provided for a new user.
        /// </summary>
        [Fact]
        public async Task AuthorizeUserAsync_CreatesNewAppUser_WhenInviteToAccountIsProvided()
        {
            // Arrange
            var email = "newuser@account.com";
            var identity = new ClaimsIdentity(new[] {
                                        new Claim(ClaimTypes.Email, email),
                                        new Claim(ClaimTypes.NameIdentifier, email)
                                    }, "Google");
            Account account = null;
            Role role = null;

            var service = CreateService((c) =>
            {
                account = c.Accounts.First();
                role = c.Roles.First();
                c.InvitesToAccount.Add(new InviteToAccount
                {
                    Email = email,
                    AccountId = account.Id,
                    Account = account,
                    RoleId = role.Id,
                    Role = role,
                    UtcExpiration = DateTime.UtcNow.AddDays(7),
                    UtcCreatedOn = DateTime.UtcNow,
                    UtcUpdatedOn = DateTime.UtcNow
                });
                c.SaveChanges();
            });

            // Act
            var result = await service.AuthorizeUserAsync(identity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            Assert.Equal(account.Id, result.AccountId);
            Assert.Equal(role.Id, result.UserRoles.First().RoleId);
            Assert.True(result.UserRoles.Count >= 1);
            Assert.NotNull(result.Account);
            Assert.True(result.UtcCreatedOn > DateTime.UtcNow.AddMinutes(-3)); // Ensure UtcCreatedOn is recent
            Assert.True(result.UtcUpdatedOn > DateTime.UtcNow.AddMinutes(-3)); // Ensure UtcUpdatedOn is recent
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.AuthorizeUserAsync(ClaimsIdentity, DeviceInfo, CancellationToken)"/> does not throw any exceptions for a valid user where the <see cref="Account.UtcExpirationDate"/> value is null.
        /// </summary>
        [Fact]
        public async Task AuthorizeUserAsync_DoesNotThrowException_WhenAccountExpirationDateIsNull()
        {
            // Arrange
            var email = "validuser@email.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) });

            var service = CreateService((c) =>
            {
                var account = new Account
                {
                    Owner = email,
                    AccountTypeId = c.AccountTypes.First().Id,
                    UtcExpirationDate = null // Set expiration date to null
                };
                c.Accounts.Add(account);

                var user = new AppUser
                {
                    ProviderKey = "randomkey",
                    ProviderType = "randomtype",
                    Email = email,
                    AccountId = account.Id,
                    Account = account,
                    UtcLastLogin = DateTime.UtcNow.AddDays(-1), // Set to a past date
                    Version = 1u
                };
                c.AppUsers.Add(user);
                c.SaveChanges();
            });

            // Act
            var exception = await Record.ExceptionAsync(() => service.AuthorizeUserAsync(identity));

            // Assert
            Assert.Null(exception); // Ensure no exceptions are thrown
        }

        /// <summary>
        /// Tests that <see cref="AuthenticationService.AuthorizeUserAsync(ClaimsIdentity, DeviceInfo, CancellationToken)"/> throws an <see cref="AuthMateException"/> when the <see cref="Account.UtcExpirationDate"/> has expired.
        /// </summary>
        [Fact]
        public async Task AuthorizeUserAsync_ThrowsException_WhenAccountExpirationDateHasExpired()
        {
            // Arrange
            var email = "expireduser@email.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) });

            var service = CreateService((c) =>
            {
                var account = new Account
                {
                    Owner = email,
                    AccountTypeId = c.AccountTypes.First().Id,
                    UtcExpirationDate = DateTime.UtcNow.AddDays(-1) // Set expiration date to a past date
                };
                c.Accounts.Add(account);

                var user = new AppUser
                {
                    ProviderKey = "randomkey",
                    ProviderType = "randomtype",
                    Email = email,
                    AccountId = account.Id,
                    Account = account,
                    UtcLastLogin = DateTime.UtcNow.AddDays(-1), // Set to a past date
                    Version = 1u
                };
                c.AppUsers.Add(user);
                c.SaveChanges();
            });

            // Act & Assert
            var exception = await Record.ExceptionAsync(() => service.AuthorizeUserAsync(identity));

            Assert.NotNull(exception); // Ensure exceptions are thrown
        }

        [Fact]
        public async Task AuthorizeUserAsync_ThrowsException_WhenUserUtcActiveUntilHasExpired()
        {
            // Arrange
            var email = "expireduser@example.com";
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, email)
            }, "Google");

            var service = CreateService((c) =>
            {
                var a = c.Accounts.First();
                c.SaveChanges();
                c.AppUsers.Add(new AppUser
                {
                    ProviderKey = "newkey",
                    ProviderType = "Google",
                    Email = email,
                    UtcActiveUntil = DateTime.UtcNow.AddDays(-1),
                    AccountId = a.Id,
                    Account = a
                });
                c.SaveChanges();
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AuthMateException>(() => service.AuthorizeUserAsync(identity));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }


    }
}
