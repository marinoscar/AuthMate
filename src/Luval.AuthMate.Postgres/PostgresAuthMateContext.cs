using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Data;
using Luval.AuthMate.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Luval.AuthMate.Postgres
{
    public class PostgresAuthMateContext : AuthMateContext, IAuthMateContext
    {
        private string _connString;


        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresAuthMateContext"/> class with the specified options.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public PostgresAuthMateContext(DbContextOptions options) : this(options, string.Empty)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresAuthMateContext"/> class with the specified options and connection string.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        /// <param name="connectionString">The connection string to the PostgreSQL database.</param>
        public PostgresAuthMateContext(DbContextOptions options, string connectionString) : base(options)
        {
            _connString = connectionString;
        }

        
        ///<summary>
        /// Initializes a new instance of the <see cref="PostgresAuthMateContext"/> class with the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to the PostgreSQL database.</param>
        public PostgresAuthMateContext(string connectionString)
        {
            _connString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //continue with regular implementation
            base.OnConfiguring(optionsBuilder);

            //add conn string if provided
            if (!string.IsNullOrEmpty(_connString))
                optionsBuilder.UseNpgsql(_connString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AccountType>()
                .Property(at => at.Id)
                .UseIdentityColumn()
                .HasColumnType("BIGINT");

            modelBuilder.Entity<Account>()
                .Property(at => at.Id)
                .UseIdentityColumn()
                .HasColumnType("BIGINT");

            modelBuilder.Entity<AppUser>()
                .Property(at => at.Id)
                .UseIdentityColumn()
                .HasColumnType("BIGINT");

            modelBuilder.Entity<AppUserLoginHistory>()
                .Property(at => at.Id)
                .UseIdentityColumn()
                .HasColumnType("BIGINT");

            modelBuilder.Entity<AppUserRole>()
                .Property(at => at.Id)
                .UseIdentityColumn()
                .HasColumnType("BIGINT");

            modelBuilder.Entity<InviteToAccount>()
                .Property(at => at.Id)
                .UseIdentityColumn()
                .HasColumnType("BIGINT");

            modelBuilder.Entity<InviteToApplication>()
                .Property(at => at.Id)
                .UseIdentityColumn()
                .HasColumnType("BIGINT");

            modelBuilder.Entity<RefreshToken>()
                .Property(at => at.Id)
                .UseIdentityColumn()
                .HasColumnType("BIGINT");

            modelBuilder.Entity<Role>()
                .Property(at => at.Id)
                .UseIdentityColumn()
                .HasColumnType("BIGINT");

            modelBuilder.Entity<AppConnection>()
                .Property(at => at.Id)
                .UseIdentityColumn()
                .HasColumnType("BIGINT");

        }
    }
}
