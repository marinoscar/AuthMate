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


        }

        public AuthMateContext() : base() { }

        public AuthMateContext(DbContextOptions options) : base(options) { }

    }


}
