using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Services;
using Luval.AuthMate.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;

namespace Luval.AuthMate.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="AccountService"/> class.
    /// </summary>
    public class AccountServiceTests
    {
        /// <summary>
        /// Creates an instance of <see cref="AccountService"/> with an optional action to modify the context after creation.
        /// </summary>
        /// <param name="afterContextCreation">An action to modify the context after creation.</param>
        /// <returns>An instance of <see cref="AccountService"/>.</returns>
        private AccountService CreateService(Action<IAuthMateContext> afterContextCreation)
        {
            var context = new MemoryDataContext();
            context.Initialize();

            var logger = new NullLogger<AccountService>();
            var service = new AccountService(context, logger);

            afterContextCreation?.Invoke(context);
            return service;
        }

        [Fact]
        public async Task CreateAccountTypeAsync_CreatesAccountTypeSuccessfully()
        {
            // Arrange
            var service = CreateService(null);
            var name = "TestAccountType";
            var createdBy = "testuser";

            // Act
            var result = await service.CreateAccountTypeAsync(name, createdBy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(createdBy, result.CreatedBy);
        }

        [Fact]
        public async Task UpdateAccountTypeAsync_UpdatesAccountTypeSuccessfully()
        {
            // Arrange
            var service = CreateService((c) =>
            {
                c.AccountTypes.Add(new AccountType { Name = "OldName", UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow });
                c.SaveChanges();
            });
            var accountTypeId = 1UL;
            var newName = "NewName";
            var updatedBy = "testuser";

            // Act
            var result = await service.UpdateAccountTypeAsync(accountTypeId, newName, updatedBy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newName, result.Name);
            Assert.Equal(updatedBy, result.UpdatedBy);
        }

        [Fact]
        public async Task DeleteAccountTypeAsync_DeletesAccountTypeSuccessfully()
        {
            IAuthMateContext context = null;
            var accountTypeId = 1UL;
            // Arrange
            var service = CreateService((c) =>
            {
                var at = new AccountType { Name = "TestAccountType", UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow };
                c.AccountTypes.Add(at);
                c.SaveChanges();
                accountTypeId = at.Id;
                context = c;
            });
            

            // Act
            await service.DeleteAccountTypeAsync(accountTypeId);

            // Assert
            var accountType = await context.AccountTypes.FindAsync(accountTypeId);
            Assert.Null(accountType);
        }

        [Fact]
        public async Task DeleteAccountTypeAsync_ThrowsException_WhenAccountTypeIsInUse()
        {
            IAuthMateContext context = null;
            var accountTypeId = 1UL;
            // Arrange
            var service = CreateService((c) =>
            {
                var at = new AccountType { Name = "TestAccountType", UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow };
                c.AccountTypes.Add(at);
                c.SaveChanges();
                c.Accounts.Add(new Account { Owner = "testowner", AccountTypeId = at.Id, UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow });
                c.SaveChanges();
                accountTypeId = at.Id;
                context = c;
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAccountTypeAsync(accountTypeId));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }


        [Fact]
        public async Task CreateAccountAsync_CreatesAccountSuccessfully()
        {
            // Arrange
            var service = CreateService((c) =>
            {
                c.AccountTypes.Add(new AccountType { Name = "TestAccountType" });
                c.SaveChanges();
            });
            var name = "TestAccount";
            var accountTypeName = "TestAccountType";
            var createdBy = "testuser";

            // Act
            var result = await service.CreateAccountAsync(name, accountTypeName, createdBy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
            Assert.Equal(createdBy, result.CreatedBy);
            Assert.Equal(createdBy, result.UpdatedBy);
            Assert.Equal(1u, result.Version);
        }

        [Fact]
        public async Task UpdateAccountExpirationDateByOwnerAsync_UpdatesExpirationDateSuccessfully()
        {
            var createdBy = "someone";
            var createdOn = DateTime.UtcNow.AddDays(-5);
            // Arrange
            var service = CreateService((c) =>
            {
                var at = new AccountType { Name = "TestAccountType", UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow };
                c.AccountTypes.Add(at);
                c.SaveChanges();
                c.Accounts.Add(new Account { Name = "testowner", Owner = "testowner", AccountTypeId = at.Id, Version = 1u, CreatedBy = createdBy, UtcCreatedOn = createdOn });
                c.SaveChanges();
            });
            var owner = "testowner";
            var newExpirationDate = DateTime.UtcNow.AddYears(1);
            var updatedBy = "testuser";

            // Act
            var result = await service.UpdateAccountExpirationDateByOwnerAsync(owner, newExpirationDate, updatedBy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newExpirationDate, result.UtcExpirationDate);
            Assert.Equal(updatedBy, result.UpdatedBy);
            Assert.Equal(createdBy, result.CreatedBy);
            Assert.Equal(createdOn, result.UtcCreatedOn);
            Assert.Equal(2u, result.Version);
        }

        [Fact]
        public async Task DeleteAccountByOwnerAsync_DeletesAccountSuccessfully()
        {
            IAuthMateContext context = null;
            // Arrange
            var service = CreateService((c) =>
            {
                var at = new AccountType { Name = "TestAccountType", UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow };
                c.AccountTypes.Add(at);
                c.SaveChanges();
                c.Accounts.Add(new Account { Owner = "testowner", AccountTypeId = at.Id, UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow });
                c.SaveChanges();
                context = c;
            });
            var owner = "testowner";

            // Act
            await service.DeleteAccountByOwnerAsync(owner);

            // Assert
            var account = await context.Accounts.FirstOrDefaultAsync(a => a.Owner == owner);
            Assert.Null(account);
        }

        [Fact]
        public async Task DeleteAccountByOwnerAsync_ThrowsException_WhenAccountDoesNotExist()
        {
            // Arrange
            var service = CreateService(null);
            var owner = "nonexistentowner";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAccountByOwnerAsync(owner));
            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
        }
    }
}
