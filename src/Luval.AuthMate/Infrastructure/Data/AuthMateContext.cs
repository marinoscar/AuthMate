using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;

namespace Luval.AuthMate.Infrastructure.Data
{

    /// <summary>
    /// Represents the EntityFramework DbContext for managing the authentication-related entities in the system.
    /// </summary>
    public class AuthMateContext : DbContext, IAuthMateContext
    {
        /// <summary>
        /// Gets or sets the DbSet for AccountType entities.
        /// </summary>
        public DbSet<AccountType> AccountTypes { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Account entities.
        /// </summary>
        public DbSet<Account> Accounts { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for AppUser entities.
        /// </summary>
        public DbSet<AppUser> AppUsers { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for AppUserRole entities.
        /// </summary>
        public DbSet<AppUserRole> AppUserRoles { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for Role entities.
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for PreAuthorizedAppUser entities.
        /// </summary>
        public DbSet<InviteToApplication> InvitesToApplication { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for AccountInvite entities
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
        /// Configures the model and relationships between entities.
        /// </summary>
        /// <param name="modelBuilder">The builder used to configure the entities.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AccountType entity
            modelBuilder.Entity<AccountType>()
                .HasKey(at => at.Id);
            modelBuilder.Entity<AccountType>()
                .HasIndex(a => a.Name).IsUnique();

            // Configure Account entity
            modelBuilder.Entity<Account>()
                .HasKey(a => a.Id);
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Name).IsUnique();
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Owner).IsUnique();
            modelBuilder.Entity<Account>()
                .HasOne(a => a.AccountType)
                .WithMany()
                .HasForeignKey(a => a.AccountTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure AppUser entity
            modelBuilder.Entity<AppUser>()
                .HasKey(au => au.Id);
            modelBuilder.Entity<AppUser>()
                .HasOne(au => au.Account)
                .WithMany()
                .HasForeignKey(au => au.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AppUser>()
                .HasIndex(a => a.Email).IsUnique();


            // Configure AppUserRole entity
            modelBuilder.Entity<AppUserRole>()
                .HasKey(aur => aur.Id);
            modelBuilder.Entity<AppUserRole>()
                .HasOne(aur => aur.User)
                .WithMany()
                .HasForeignKey(aur => aur.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AppUserRole>()
                .HasOne(aur => aur.Role)
                .WithMany()
                .HasForeignKey(aur => aur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Role entity
            modelBuilder.Entity<Role>()
                .HasKey(r => r.Id);

            // Configure InviteToApplication entity
            modelBuilder.Entity<InviteToApplication>()
                .HasKey(au => au.Id);
            modelBuilder.Entity<InviteToApplication>()
                .HasOne(au => au.AccountType)
                .WithMany()
                .HasForeignKey(au => au.AccountTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<InviteToApplication>()
                .HasIndex(a => a.Email).IsUnique();


            // Configure AccountInvite entity
            modelBuilder.Entity<InviteToAccount>()
                .HasKey(au => au.Id);
            modelBuilder.Entity<InviteToAccount>()
                .HasOne(aur => aur.Role)
                .WithMany()
                .HasForeignKey(aur => aur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);


            // Configure AppUserLoginHistory entity
            modelBuilder.Entity<AppUserLoginHistory>()
                .HasKey(au => au.Id);

            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RefreshToken>()
               .HasIndex(a => a.Token).IsUnique();


        }

        /// <summary>
        /// Initializes the database asynchronously with default records if it is empty.
        /// </summary>
        /// <param name="ownerEmail">The email of the owner who is creating the records.</param>
        /// <param name="roles">An optional list of roles to be added. If not provided, default roles will be created.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Ensures the database is created.
        /// 2. Checks if the database is empty by verifying the absence of any roles, account types, and invites to account.
        /// 3. If the database is empty, it calls the CreateDefaultRecordsAsync method to populate it with default records.
        ///    - The CreateDefaultRecordsAsync method performs the following:
        ///      a. Creates a default account type with the name "Default" and associates it with the provided owner email.
        ///      b. Saves the account type to the database.
        ///      c. Checks if a list of roles is provided:
        ///         - If roles are provided, it adds each role to the database.
        ///         - If no roles are provided, it creates a set of default roles (Admin, Owner, Member, Visitor) and adds them to the database.
        ///      d. Saves the roles to the database.
        ///      e. Creates an application invite for the owner email, associating it with the created account type.
        ///      f. Saves the application invite to the database.
        /// </remarks>
        public virtual async Task InitializeDbAsync(string ownerEmail, IEnumerable<Role>? roles = default, CancellationToken cancellationToken = default)
        {
            await Database.EnsureCreatedAsync(cancellationToken);
            if (!Roles.Any() && !AccountTypes.Any() && !InvitesToAccount.Any())
                await CreateDefaultRecordsAsync(ownerEmail, roles, cancellationToken);
        }


        /// <summary>
        /// Creates default records in the database, including account types, roles, and application invites.
        /// </summary>
        /// <param name="ownerEmail">The email of the owner who is creating the records.</param>
        /// <param name="roles">An optional list of roles to be added. If not provided, default roles will be created.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Creates a default account type with the name "Default" and associates it with the provided owner email.
        /// 2. Saves the account type to the database.
        /// 3. Checks if a list of roles is provided:
        ///    - If roles are provided, it adds each role to the database.
        ///    - If no roles are provided, it creates a set of default roles (Admin, Owner, Member, Visitor) and adds them to the database.
        /// 4. Saves the roles to the database.
        /// 5. Creates an application invite for the owner email, associating it with the created account type.
        /// 6. Saves the application invite to the database.
        /// </remarks>
        public virtual async Task CreateDefaultRecordsAsync(string ownerEmail, IEnumerable<Role>? roles = default, CancellationToken cancellationToken = default)
        {
            //Creates initial records
            var accountType = new AccountType()
            {
                Name = "Default",
                UpdatedBy = ownerEmail,
                CreatedBy = ownerEmail,
                Version = 1
            };
            AccountTypes.Add(accountType);
            await SaveChangesAsync(cancellationToken);

            //create the roles
            if (roles != null && roles.Any())
                foreach (var role in roles)
                    Roles.Add(role);
            else
            {
                var defaultRoles = new List<Role>()
                {
                    new () { Name = "Admin", Description = "Full access to all features and settings in the system.", CreatedBy = ownerEmail, UpdatedBy = ownerEmail, UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow },
                    new () { Name = "Owner", Description = "Responsible for managing an organization or account.", CreatedBy = ownerEmail, UpdatedBy = ownerEmail, UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow },
                    new () { Name = "Member", Description = "Standard user with access to core system features.", CreatedBy = ownerEmail, UpdatedBy = ownerEmail, UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow },
                    new () { Name = "Visitor", Description = "Limited access for viewing purposes only.", CreatedBy = ownerEmail, UpdatedBy = ownerEmail, UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow },
                };
                foreach (var role in defaultRoles)
                    Roles.Add(role);
            }
            await SaveChangesAsync(cancellationToken); //save the roles

            //Add user to the AppInvite List
            var appInvite = new InviteToApplication()
            {
                Email = ownerEmail,
                AccountTypeId = accountType.Id,
                CreatedBy = ownerEmail,
                UpdatedBy = ownerEmail,
                UtcCreatedOn = DateTime.UtcNow,
                UtcUpdatedOn = DateTime.UtcNow
            };
            InvitesToApplication.Add(appInvite);
            await SaveChangesAsync(cancellationToken);
        }


        /// <summary>  
        /// Resets the database by deleting and reinitializing it with default records.  
        /// </summary>  
        /// <param name="ownerEmail">The email of the owner who is creating the records.</param>  
        /// <param name="roles">An optional list of roles to be added. If not provided, default roles will be created.</param>  
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>  
        /// <returns>A task that represents the asynchronous operation.</returns>  
        /// <remarks>  
        /// This method performs the following steps:  
        /// 1. Ensures the database is deleted.  
        /// 2. Calls the InitializeDbAsync method to reinitialize the database with default records.  
        ///    - The InitializeDbAsync method performs the following:  
        ///      a. Ensures the database is created.  
        ///      b. Checks if the database is empty by verifying the absence of any roles, account types, and invites to account.  
        ///      c. If the database is empty, it calls the CreateDefaultRecordsAsync method to populate it with default records.  
        ///         - The CreateDefaultRecordsAsync method performs the following:  
        ///           i. Creates a default account type with the name "Default" and associates it with the provided owner email.  
        ///           ii. Saves the account type to the database.  
        ///           iii. Checks if a list of roles is provided:  
        ///                - If roles are provided, it adds each role to the database.  
        ///                - If no roles are provided, it creates a set of default roles (Admin, Owner, Member, Visitor) and adds them to the database.  
        ///           iv. Saves the roles to the database.  
        ///           v. Creates an application invite for the owner email, associating it with the created account type.  
        ///           vi. Saves the application invite to the database.  
        /// </remarks>  
        public virtual async Task ResetDatabaseAsync(string ownerEmail, IEnumerable<Role>? roles = default, CancellationToken cancellationToken = default)
        {
            await Database.EnsureDeletedAsync(cancellationToken);
            await InitializeDbAsync(ownerEmail, roles, cancellationToken);
        }

        public AuthMateContext() : base() { }

        public AuthMateContext(DbContextOptions options) : base(options) { }

    }


}
