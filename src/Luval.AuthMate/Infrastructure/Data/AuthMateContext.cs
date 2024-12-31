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
        /// Gets or sets the DbSet for AppConnection entities
        /// </summary>
        public DbSet<AppConnection> AppConnections { get; set; }


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
                .Property(at => at.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<AccountType>()
                .Property(at => at.Name)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<AccountType>()
                .HasIndex(at => at.Name)
                .IsUnique();

            // Configure Account entity
            modelBuilder.Entity<Account>()
                .HasKey(a => a.Id);
            modelBuilder.Entity<Account>()
                .Property(a => a.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Account>()
                .Property(a => a.Name)
                .HasMaxLength(100);
            modelBuilder.Entity<Account>()
                .Property(a => a.Owner)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Account>()
                .Property(a => a.Description)
                .HasMaxLength(250);
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Name)
                .IsUnique();
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Owner)
                .IsUnique();
            modelBuilder.Entity<Account>()
                .HasOne(a => a.AccountType)
                .WithMany()
                .HasForeignKey(a => a.AccountTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure AppUser entity
            modelBuilder.Entity<AppUser>()
                .HasKey(au => au.Id);
            modelBuilder.Entity<AppUser>()
                .Property(au => au.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<AppUser>()
                .Property(au => au.Email)
                .HasMaxLength(255)
                .IsRequired();
            modelBuilder.Entity<AppUser>()
                .Property(au => au.ProviderKey)
                .HasMaxLength(255)
                .IsRequired();
            modelBuilder.Entity<AppUser>()
                .Property(au => au.ProviderType)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<AppUser>()
                .Property(au => au.ProfilePictureUrl)
                .HasMaxLength(500);
            modelBuilder.Entity<AppUser>()
                .Property(au => au.OAuthAccessToken)
                .HasMaxLength(500);
            modelBuilder.Entity<AppUser>()
                .Property(au => au.OAuthTokenType)
                .HasMaxLength(50);
            modelBuilder.Entity<AppUser>()
                .Property(au => au.OAuthRefreshToken)
                .HasMaxLength(250);
            modelBuilder.Entity<AppUser>()
                .Property(au => au.Timezone)
                .HasMaxLength(100);
            modelBuilder.Entity<AppUser>()
                .HasIndex(au => au.Email)
                .IsUnique();
            modelBuilder.Entity<AppUser>()
                .HasOne(au => au.Account)
                .WithMany()
                .HasForeignKey(au => au.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure AppUserRole entity
            modelBuilder.Entity<AppUserRole>()
                .HasKey(aur => aur.Id);
            modelBuilder.Entity<AppUserRole>()
                .Property(aur => aur.Id)
                .ValueGeneratedOnAdd();
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
            modelBuilder.Entity<Role>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Role>()
                .Property(r => r.Name)
                .HasMaxLength(100)
                .IsRequired();

            // Configure InviteToApplication entity
            modelBuilder.Entity<InviteToApplication>()
                .HasKey(ita => ita.Id);
            modelBuilder.Entity<InviteToApplication>()
                .Property(ita => ita.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<InviteToApplication>()
                .Property(ita => ita.Email)
                .HasMaxLength(255)
                .IsRequired();
            modelBuilder.Entity<InviteToApplication>()
                .Property(ita => ita.UserMessage)
                .HasMaxLength(1024);
            modelBuilder.Entity<InviteToApplication>()
                .HasIndex(ita => ita.Email)
                .IsUnique();
            modelBuilder.Entity<InviteToApplication>()
                .HasOne(ita => ita.AccountType)
                .WithMany()
                .HasForeignKey(ita => ita.AccountTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure InviteToAccount entity
            modelBuilder.Entity<InviteToAccount>()
                .HasKey(ita => ita.Id);
            modelBuilder.Entity<InviteToAccount>()
                .Property(ita => ita.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<InviteToAccount>()
                .Property(ita => ita.Email)
                .HasMaxLength(256)
                .IsRequired();
            modelBuilder.Entity<InviteToAccount>()
                .Property(ita => ita.UserMessage)
                .HasMaxLength(1024);
            modelBuilder.Entity<InviteToAccount>()
                .HasOne(ita => ita.Role)
                .WithMany()
                .HasForeignKey(ita => ita.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure AppUserLoginHistory entity
            modelBuilder.Entity<AppUserLoginHistory>()
                .HasKey(aulh => aulh.Id);
            modelBuilder.Entity<AppUserLoginHistory>()
                .Property(aulh => aulh.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<AppUserLoginHistory>()
                .Property(aulh => aulh.Email)
                .HasMaxLength(256)
                .IsRequired();
            modelBuilder.Entity<AppUserLoginHistory>()
                .Property(aulh => aulh.OS)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<AppUserLoginHistory>()
                .Property(aulh => aulh.IpAddress)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<AppUserLoginHistory>()
                .Property(aulh => aulh.Browser)
                .HasMaxLength(100)
                .IsRequired();

            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>()
                .HasKey(rt => rt.Id);
            modelBuilder.Entity<RefreshToken>()
                .Property(rt => rt.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<RefreshToken>()
                .Property(rt => rt.Token)
                .HasMaxLength(500)
                .IsRequired();
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public AuthMateContext() : base() { }

        public AuthMateContext(DbContextOptions options) : base(options) { }

    }


}
