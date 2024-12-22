using Luval.AuthMate.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Luval.AuthMate.Core.Interfaces
{
    /// <summary>
    /// Represents the interface for the AuthMate DbContext, used to manage authentication-related entities in the system.
    /// </summary>
    public interface IAuthMateContext
    {
        /// <summary>
        /// Gets the DbSet for AccountType entities.
        /// </summary>
        DbSet<AccountType> AccountTypes { get; }

        /// <summary>
        /// Gets the DbSet for Account entities.
        /// </summary>
        DbSet<Account> Accounts { get; }

        /// <summary>
        /// Gets the DbSet for AppUser entities.
        /// </summary>
        DbSet<AppUser> AppUsers { get; }

        /// <summary>
        /// Gets the DbSet for AppUserRole entities.
        /// </summary>
        DbSet<AppUserRole> AppUserRoles { get; }

        /// <summary>
        /// Gets the DbSet for Role entities.
        /// </summary>
        DbSet<Role> Roles { get; }

        /// <summary>
        /// Gets or sets the DbSet for InviteToApplication entities.
        /// </summary>
        public DbSet<InviteToApplication> InvitesToApplication { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for InviteToAccount entities
        /// </summary>
        public DbSet<InviteToAccount> InvitesToAccount { get; set; }

        /// <summary>
        /// Gets or sets the DBSet for AppUserLoginHistory entities
        /// </summary>
        public DbSet<AppUserLoginHistory> AppUserLoginHistories { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for RefreshToken entities
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Gets the DatabaseFacade used to manage the database.
        /// </summary>
        public DatabaseFacade Database { get; }

        /// <summary>
        /// Saves changes made in this context to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        int SaveChanges();

        /// <summary>
        /// Saves changes made in this context to the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

}