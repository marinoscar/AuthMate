using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Services;
using Microsoft.EntityFrameworkCore;
using Luval.AuthMate.Infrastructure.Logging;
using Luval.AuthMate.Core;

namespace Luval.AuthMate.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="AppUserService"/> class.
    /// </summary>
    public class AppUserServiceTests
    {
        /// <summary>
        /// Creates an instance of <see cref="AppUserService"/> with an optional action to modify the context after creation.
        /// </summary>
        /// <param name="afterContextCreation">An action to modify the context after creation.</param>
        /// <returns>An instance of <see cref="AppUserService"/>.</returns>
        private AppUserService CreateService(Action<IAuthMateContext> afterContextCreation)
        {
            var context = new MemoryDataContext();
            context.Initialize();

            var logger = new NullLogger<AppUserService>();
            var service = new AppUserService(context, logger);

            afterContextCreation?.Invoke(context);
            return service;
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var email = "testuser@example.com";
            var service = CreateService((c) =>
            {
                var a = c.Accounts.First();
                var u = new AppUser { Email = email, ProviderKey = "radomkey", ProviderType = "Google", AccountId = a.Id };
                c.AppUsers.Add(u);
                c.SaveChanges();
            });

            // Act
            var result = await service.GetUserByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ThrowsException_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "nonexistentuser@example.com";
            var service = CreateService(null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GetUserByEmailAsync(email));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public async Task UpdateAppUserAsync_UpdatesUserSuccessfully()
        {
            // Arrange
            var user = new AppUser();
            var email = "testuser@example.com";
            var service = CreateService((c) =>
            {
                var a = c.Accounts.First();
                user = new AppUser
                {
                    Email = email,
                    ProviderKey = "radomkey",
                    ProviderType = "Google",
                    AccountId = a.Id,
                    Version = 1,
                    UtcCreatedOn = DateTime.UtcNow.AddDays(-1)
                };
                c.AppUsers.Add(user);
                c.SaveChanges();
            });

            user.ProviderKey = "newkey";

            // Act
            var result = await service.UpdateAppUserAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            Assert.Equal(email, result.UpdatedBy);
            Assert.True(result.UtcUpdatedOn > result.UtcCreatedOn);
            Assert.Equal("newkey", result.ProviderKey);
            Assert.Equal(2u, result.Version);
        }

        [Fact]
        public async Task AddUserToRoleAsync_AddsRoleSuccessfully()
        {
            IAuthMateContext context = null;
            // Arrange
            var email = "testuser@example.com";
            var roleName = "TestRole";
            var service = CreateService((c) =>
            {
                var a = c.Accounts.First();
                var u = new AppUser { Email = email, ProviderKey = "radomkey", ProviderType = "Google", AccountId = a.Id };
                c.AppUsers.Add(u);
                c.SaveChanges();
                c.Roles.Add(new Role { Name = roleName });
                c.SaveChanges();
                context = c;
            });

            // Act
            await service.AddUserToRoleAsync(email, roleName);

            // Assert
            var userRole = await context.AppUserRoles.FirstOrDefaultAsync(ur => ur.User.Email == email && ur.Role.Name == roleName);
            Assert.NotNull(userRole);
        }

        [Fact]
        public async Task RemoveUserFromRoleAsync_RemovesRoleSuccessfully()
        {
            IAuthMateContext context = null;
            // Arrange
            var email = "testuser@example.com";
            var roleName = "TestRole";
            var service = CreateService((c) =>
            {
                var a = c.Accounts.First();
                var u = new AppUser { Email = email, ProviderKey = "radomkey", ProviderType = "Google", AccountId = a.Id };
                var r = new Role { Name = roleName };
                c.AppUsers.Add(u);
                c.SaveChanges();
                c.Roles.Add(r);
                c.SaveChanges();
                c.AppUserRoles.Add(new AppUserRole { AppUserId = u.Id, RoleId = r.Id, User = u, Role = r });
                c.SaveChanges();
                context = c;
            });

            // Act
            await service.RemoveUserFromRoleAsync(email, roleName);

            // Assert
            var userRole = await context.AppUserRoles.FirstOrDefaultAsync(ur => ur.User.Email == email && ur.Role.Name == roleName);
            Assert.Null(userRole);
        }

        [Fact]
        public async Task TryGetUserByEmailAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var email = "testuser@example.com";
            var service = CreateService((c) =>
            {
                var a = c.Accounts.First();
                var u = new AppUser { Email = email, ProviderKey = "radomkey", ProviderType = "Google", AccountId = a.Id };
                c.AppUsers.Add(u);
                c.SaveChanges();
            });

            // Act
            var result = await service.TryGetUserByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task TryGetUserByEmailAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "nonexistentuser@example.com";
            var service = CreateService(null);

            // Act
            var result = await service.TryGetUserByEmailAsync(email);

            // Assert
            Assert.Null(result);
        }
    }
}

