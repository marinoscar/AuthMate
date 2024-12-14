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

namespace Luval.AuthMate.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="RoleService"/> class.
    /// </summary>
    public class RoleServiceTests
    {
        /// <summary>
        /// Creates an instance of <see cref="RoleService"/> with an optional action to modify the context after creation.
        /// </summary>
        /// <param name="afterContextCreation">An action to modify the context after creation.</param>
        /// <returns>An instance of <see cref="RoleService"/>.</returns>
        private RoleService CreateService(Action<IAuthMateContext> afterContextCreation)
        {
            var context = new MemoryDataContext();
            context.Initialize();

            var logger = new NullLogger<RoleService>();
            var service = new RoleService(context, logger);

            afterContextCreation?.Invoke(context);
            return service;
        }

        [Fact]
        public async Task CreateRoleAsync_CreatesRoleSuccessfully()
        {
            // Arrange
            var service = CreateService(null);
            var name = "TestRole";
            var description = "Test role description";

            // Act
            var result = await service.CreateRoleAsync(name, description);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(description, result.Description);
        }

        [Fact]
        public async Task UpdateRoleAsync_UpdatesRoleSuccessfully()
        {
            var r = new Role();
            // Arrange
            var service = CreateService((c) =>
            {
                r = new Role { Name = "OldName", Description = "Old description", UtcCreatedOn = DateTime.UtcNow.AddDays(-1), Version = 1 };
                c.Roles.Add(r);
                c.SaveChanges();
            });
            var roleId = r.Id;
            var newName = "NewName";
            var newDescription = "New description";

            // Act
            var result = await service.UpdateRoleAsync(roleId, newName, newDescription);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newName, result.Name);
            Assert.Equal(newDescription, result.Description);
            Assert.Equal(2u, result.Version);
            Assert.True(result.UtcUpdatedOn > r.UtcCreatedOn);
        }

        [Fact]
        public async Task DeleteRoleAsync_DeletesRoleSuccessfully()
        {
            IAuthMateContext context = null;
            var roleId = 1UL;
            // Arrange
            var service = CreateService((c) =>
            {
                var role = new Role { Name = "TestRole", Description = "Test role description", UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow };
                c.Roles.Add(role);
                c.SaveChanges();
                roleId = role.Id;
                context = c;
            });

            // Act
            await service.DeleteRoleAsync(roleId);

            // Assert
            var role = await context.Roles.FindAsync(roleId);
            Assert.Null(role);
        }

        [Fact]
        public async Task DeleteRoleAsync_ThrowsException_WhenRoleDoesNotExist()
        {
            // Arrange
            var service = CreateService(null);
            var roleId = 999UL;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteRoleAsync(roleId));
            Assert.Equal($"Role with ID '{roleId}' not found.", exception.Message);
        }
    }
}


