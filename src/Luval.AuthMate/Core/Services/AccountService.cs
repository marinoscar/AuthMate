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

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService"/> class.
        /// </summary>
        /// <param name="context">The database context interface.</param>
        /// <param name="logger">The logger instance.</param>
        public AccountService(IAuthMateContext context, ILogger<AccountService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                var accountType = new AccountType { Name = name };
                await _context.AccountTypes.AddAsync(accountType, cancellationToken).ConfigureAwait(false);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Account type '{AccountTypeName}' created successfully.", name);
                return accountType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account type '{AccountTypeName}'.", name);
                throw;
            }
        }

        /// <summary>
        /// Creates a new account.
        /// </summary>
        /// <param name="name">The name of the account.</param>
        /// <param name="accountTypeName">The name of the account type.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created account entity.</returns>
        public async Task<Account> CreateAccountAsync(string name, string accountTypeName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Account name is required.", nameof(name));
                if (string.IsNullOrWhiteSpace(accountTypeName))
                    throw new ArgumentException("Account type name is required.", nameof(accountTypeName));

                var accountType = await _context.AccountTypes.FirstOrDefaultAsync(at => at.Name == accountTypeName, cancellationToken).ConfigureAwait(false);
                if (accountType == null)
                {
                    _logger.LogWarning("Account type '{AccountTypeName}' not found.", accountTypeName);
                    throw new InvalidOperationException($"Account type '{accountTypeName}' not found.");
                }

                var account = new Account { Name = name, AccountTypeId = accountType.Id };
                await _context.Accounts.AddAsync(account, cancellationToken).ConfigureAwait(false);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Account '{AccountName}' created successfully.", name);
                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account '{AccountName}'.", name);
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
                account.Version++;

                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Account with owner '{Owner}' expiration date updated successfully.", owner);
                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expiration date for account with owner '{Owner}'.", owner);
                throw;
            }
        }
    }
}
