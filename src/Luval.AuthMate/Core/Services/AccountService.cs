using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core.Services
{
    /// <summary>
    /// Service for managing account-related operations.
    /// </summary>
    public class AccountService
    {
        private readonly IAuthMateContext _context;
        private readonly ILogger<AccountService> _logger;
        private readonly IUserResolver _userResolver;
        private readonly string _userEmail = "Anonymous";

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService"/> class.
        /// </summary>
        /// <param name="context">The database context interface.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="userResolver">The user resolver instance.</param>
        public AccountService(IAuthMateContext context, IUserResolver userResolver,  ILogger<AccountService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userResolver = userResolver ?? throw new ArgumentNullException(nameof(userResolver));
            _userEmail = _userResolver.GetUserEmail();
        }

        /// <summary>
        /// Creates a new account type.
        /// </summary>
        /// <param name="name">The name of the account type.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created account type entity.</returns>
        public async Task<AccountType> CreateAccountTypeAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Account type name is required.", nameof(name));

                var accountType = new AccountType { Name = name, CreatedBy = _userEmail, UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow };
                await _context.AccountTypes.AddAsync(accountType, cancellationToken).ConfigureAwait(false);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Account type '{AccountTypeName}' created successfully by '{CreatedBy}'.", name, _userEmail);
                return accountType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account type '{AccountTypeName}' by '{CreatedBy}'.", name, _userEmail);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing account type.
        /// </summary>
        /// <param name="accountTypeId">The ID of the account type to update.</param>
        /// <param name="newName">The new name for the account type.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The updated account type entity.</returns>
        /// <exception cref="ArgumentException">Thrown when the account type ID or new name is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the account type with the specified ID is not found.</exception>
        public async Task<AccountType> UpdateAccountTypeAsync(ulong accountTypeId, string newName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (accountTypeId == 0)
                    throw new ArgumentException("Account type ID is required.", nameof(accountTypeId));
                if (string.IsNullOrWhiteSpace(newName))
                    throw new ArgumentException("New name is required.", nameof(newName));

                var accountType = await _context.AccountTypes.FirstOrDefaultAsync(at => at.Id == accountTypeId, cancellationToken).ConfigureAwait(false);
                if (accountType == null)
                {
                    _logger.LogWarning("Account type with ID '{AccountTypeId}' not found.", accountTypeId);
                    throw new InvalidOperationException($"Account type with ID '{accountTypeId}' not found.");
                }

                accountType.Name = newName;
                accountType.UtcUpdatedOn = DateTime.UtcNow;
                accountType.UpdatedBy = _userEmail;
                accountType.Version++;

                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Account type with ID '{AccountTypeId}' updated successfully by '{UpdatedBy}'.", accountTypeId, _userEmail);
                return accountType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account type with ID '{AccountTypeId}' by '{UpdatedBy}'.", accountTypeId, _userEmail);
                throw;
            }
        }

        /// <summary>
        /// Deletes an account type by its ID.
        /// </summary>
        /// <param name="accountTypeId">The ID of the account type to delete.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the account type ID is zero.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the account type with the specified ID is not found or is in use.</exception>
        public async Task DeleteAccountTypeAsync(ulong accountTypeId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (accountTypeId == 0)
                    throw new ArgumentException("Account type ID is required.", nameof(accountTypeId));

                var accountType = await _context.AccountTypes.FirstOrDefaultAsync(at => at.Id == accountTypeId, cancellationToken).ConfigureAwait(false);
                if (accountType == null)
                {
                    _logger.LogWarning("Account type with ID '{AccountTypeId}' not found.", accountTypeId);
                    throw new InvalidOperationException($"Account type with ID '{accountTypeId}' not found.");
                }

                var isAccountTypeInUse = await _context.Accounts.AnyAsync(a => a.AccountTypeId == accountTypeId, cancellationToken).ConfigureAwait(false);
                if (isAccountTypeInUse)
                {
                    _logger.LogWarning("Account type with ID '{AccountTypeId}' is in use and cannot be deleted.", accountTypeId);
                    throw new InvalidOperationException($"Account type with ID '{accountTypeId}' is in use and cannot be deleted.");
                }

                _context.AccountTypes.Remove(accountType);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Account type with ID '{AccountTypeId}' deleted successfully.", accountTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account type with ID '{AccountTypeId}'.", accountTypeId);
                throw;
            }
        }

        /// <summary>
        /// Creates a new account.
        /// </summary>
        /// <param name="accountOwner">The owner of the account.</param>
        /// <param name="accountTypeName">The name of the account type.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created account entity.</returns>
        public async Task<Account> CreateAccountAsync(string accountOwner, string accountTypeName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountOwner))
                    throw new ArgumentException("Account owner is required.", nameof(accountOwner));
                if (string.IsNullOrWhiteSpace(accountTypeName))
                    throw new ArgumentException("Account type name is required.", nameof(accountTypeName));

                var accountType = await _context.AccountTypes.FirstOrDefaultAsync(at => at.Name == accountTypeName, cancellationToken).ConfigureAwait(false);
                if (accountType == null)
                {
                    _logger.LogWarning("Account type '{AccountTypeName}' not found.", accountTypeName);
                    throw new InvalidOperationException($"Account type '{accountTypeName}' not found.");
                }

                var account = new Account
                {
                    Name = accountOwner,
                    Owner = accountOwner,
                    AccountTypeId = accountType.Id,
                    CreatedBy = _userEmail,
                    UpdatedBy = _userEmail,
                    UtcCreatedOn = DateTime.UtcNow,
                    UtcUpdatedOn = DateTime.UtcNow,
                    Version = 1
                };
                await _context.Accounts.AddAsync(account, cancellationToken).ConfigureAwait(false);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Account '{AccountName}' created successfully by '{CreatedBy}'.", accountOwner, _userEmail);
                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account '{AccountName}' by '{CreatedBy}'.", accountOwner, _userEmail);
                throw;
            }
        }

        /// <summary>
        /// Updates the expiration date of an account by the account owner.
        /// </summary>
        /// <param name="owner">The owner of the account to update.</param>
        /// <param name="utcNewExpirationDate">The new expiration date for the account.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The updated account entity.</returns>
        public async Task<Account> UpdateAccountExpirationDateByOwnerAsync(string owner, DateTime utcNewExpirationDate, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(owner))
                    throw new ArgumentException("Account owner is required.", nameof(owner));
                if (utcNewExpirationDate.Kind != DateTimeKind.Utc)
                    throw new ArgumentException("Expiration date must be in UTC.", nameof(utcNewExpirationDate));

                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Owner == owner, cancellationToken).ConfigureAwait(false);
                if (account == null)
                {
                    _logger.LogWarning("Account with owner '{Owner}' not found.", owner);
                    throw new InvalidOperationException($"Account with owner '{owner}' not found.");
                }

                account.UtcExpirationDate = utcNewExpirationDate;
                account.UtcUpdatedOn = DateTime.UtcNow;
                account.UpdatedBy = _userEmail;
                account.Version++;

                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Account with owner '{Owner}' expiration date updated successfully by '{UpdatedBy}'.", owner, _userEmail);
                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expiration date for account with owner '{Owner}' by '{UpdatedBy}'.", owner, _userEmail);
                throw;
            }
        }

        /// <summary>
        /// Deletes an account by the account owner.
        /// </summary>
        /// <param name="owner">The owner of the account to delete.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when the owner is null or whitespace.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the account with the specified owner is not found.</exception>
        public async Task DeleteAccountByOwnerAsync(string owner, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(owner))
                    throw new ArgumentException("Account owner is required.", nameof(owner));

                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Owner == owner, cancellationToken).ConfigureAwait(false);
                if (account == null)
                {
                    _logger.LogWarning("Account with owner '{Owner}' not found.", owner);
                    throw new InvalidOperationException($"Account with owner '{owner}' not found.");
                }

                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Account with owner '{Owner}' deleted successfully.", owner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account with owner '{Owner}'.", owner);
                throw;
            }
        }
    }
}
