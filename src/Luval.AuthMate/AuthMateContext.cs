using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luval.AuthMate.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Luval.AuthMate
{

    /// <summary>
    /// Represents the database context for authentication and authorization in the system.
    /// </summary>
    public class AuthMateContext : DbContext, IAuthMateContext
    {
        ///// <summary>
        ///// Initializes a new instance of the <see cref="AuthMateContext"/> class.
        ///// </summary>
        ///// <param name="options">The options to configure the context.</param>
        //public AuthMateContext(DbContextOptions<AuthMateContext> options)
        //    : base(options)
        //{
        //}

        /// <summary>
        /// DbSet for AccountType entities.
        /// </summary>
        public DbSet<AccountType> AccountTypes { get; set; }

        /// <summary>
        /// DbSet for Account entities.
        /// </summary>
        public DbSet<Account> Accounts { get; set; }

        /// <summary>
        /// DbSet for User entities.
        /// </summary>
        public DbSet<AppUser> Users { get; set; }

        /// <summary>
        /// DbSet for Role entities.
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// DbSet for UserRole entities.
        /// </summary>
        public DbSet<AppUserRole> UserRoles { get; set; }

        /// <summary>
        /// DbSet for UserInAccount entities.
        /// </summary>
        public DbSet<AppUserInAccount> UserInAccounts { get; set; }

        /// <summary>
        /// Configures the database model and relationships.
        /// </summary>
        /// <param name="modelBuilder">The model builder for configuring the schema.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AccountType Configuration
            modelBuilder.Entity<AccountType>()
                .HasKey(a => a.Id);
            modelBuilder.Entity<AccountType>()
                .HasIndex(a => a.Name)
                .IsUnique();
            modelBuilder.Entity<AccountType>()
                .Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);
            

            // Account Configuration
            modelBuilder.Entity<Account>()
                .HasKey(a => a.Id);
            modelBuilder.Entity<Account>()
                .HasIndex(i => i.Owner)
                .IsUnique();
            modelBuilder.Entity<Account>()
                .HasOne(a => a.AccountType)
                .WithMany()
                .HasForeignKey(a => a.AccountTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Account>()
                .Property(a => a.Owner)
                .IsRequired()
                .HasMaxLength(255);

            // User Configuration
            modelBuilder.Entity<AppUser>()
                .HasKey(u => u.Id);
            modelBuilder.Entity<AppUser>()
               .HasIndex(i => i.Email)
               .IsUnique();
            modelBuilder.Entity<AppUser>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);
            modelBuilder.Entity<AppUser>()
                .Property(u => u.ProviderKey)
                .IsRequired()
                .HasMaxLength(255);
            modelBuilder.Entity<AppUser>()
                .Property(u => u.ProviderType)
                .IsRequired()
                .HasMaxLength(50);
            modelBuilder.Entity<AppUser>()
                .Property(u => u.ProfilePictureUrl)
                .HasMaxLength(500);

            //modelBuilder.Entity<AppUser>()
            //    .HasMany<AppUserRole>(i => i.UserRoles);
            //modelBuilder.Entity<AppUser>()
            //    .HasMany<AppUserInAccount>(i => i.UserInAccounts);

            // Role Configuration
            modelBuilder.Entity<Role>()
                .HasKey(r => r.Id);
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();
            modelBuilder.Entity<Role>()
                .Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);
            modelBuilder.Entity<Role>()
                .Property(r => r.Description)
                .HasMaxLength(500);

            // UserRole Configuration
            modelBuilder.Entity<AppUserRole>()
                .HasKey(ur => ur.Id);
            modelBuilder.Entity<AppUserRole>()
                .HasOne(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AppUserRole>()
                .HasOne(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserInAccount Configuration
            modelBuilder.Entity<AppUserInAccount>()
                .HasKey(ua => ua.Id);
            modelBuilder.Entity<AppUserInAccount>()
                .HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AppUserInAccount>()
                .HasOne(ua => ua.Account)
                .WithMany()
                .HasForeignKey(ua => ua.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        /// <inheritdoc/>
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        /// <inheritdoc/>
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        /// <inheritdoc/>
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <inheritdoc/>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }

}
