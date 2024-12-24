using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Infrastructure.Data;
using Luval.AuthMate.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Tests
{
    public class AuthMateContextHelperTests
    {
        private AuthMateContextHelper _helper;
        private AuthMateContext _context;

        public AuthMateContextHelperTests()
        {
            Init();
        }

        private void Init()
        {
            _context = new MemoryDataContext();
            _helper = new AuthMateContextHelper(_context, new DebugLogger<AuthMateContextHelper>());
        }

        [Fact]
        public async Task InitializeDbAsync_ShouldCreateDefaultRecords_WhenDatabaseIsEmpty()
        {
            // Arrange
            Init();
            var ownerEmail = "owner@example.com";

            // Act
            await _helper.InitializeDbAsync(ownerEmail);

            // Assert
            Assert.True(_context.AccountTypes.Any());
            Assert.True(_context.Roles.Any());
            Assert.True(_context.InvitesToApplication.Any());
            Assert.Equal(ownerEmail, _context.InvitesToApplication.First().Email);
        }

        [Fact]
        public async Task InitializeDbAsync_ShouldNotCreateDefaultRecords_WhenDatabaseIsNotEmpty()
        {
            // Arrange
            Init();
            var ownerEmail = "owner@example.com";
            _context.AccountTypes.Add(new AccountType { Name = "Existing" });
            await _context.SaveChangesAsync();

            // Act
            await _helper.InitializeDbAsync(ownerEmail);

            // Assert
            Assert.Equal(1, _context.AccountTypes.Count());
            Assert.False(_context.Roles.Any());
            Assert.False(_context.InvitesToApplication.Any());
        }

        [Fact]
        public async Task CreateDefaultRecordsAsync_ShouldCreateDefaultRoles_WhenNoRolesProvided()
        {
            // Arrange
            Init();
            var ownerEmail = "owner@example.com";

            // Act
            await _helper.CreateDefaultRecordsAsync(ownerEmail);

            // Assert
            var roles = _context.Roles.ToList();
            Assert.Equal(4, roles.Count);
            Assert.Contains(roles, r => r.Name == "Admin");
            Assert.Contains(roles, r => r.Name == "Owner");
            Assert.Contains(roles, r => r.Name == "Member");
            Assert.Contains(roles, r => r.Name == "Visitor");
        }

        [Fact]
        public async Task CreateDefaultRecordsAsync_ShouldCreateProvidedRoles()
        {
            // Arrange
            Init();
            var ownerEmail = "owner@example.com";
            var roles = new List<Role>
        {
            new Role { Name = "CustomRole1" },
            new Role { Name = "CustomRole2" }
        };

            // Act
            await _helper.CreateDefaultRecordsAsync(ownerEmail, roles);

            // Assert
            var dbRoles = _context.Roles.ToList();
            Assert.Equal(2, dbRoles.Count);
            Assert.Contains(dbRoles, r => r.Name == "CustomRole1");
            Assert.Contains(dbRoles, r => r.Name == "CustomRole2");
        }
    }
}
