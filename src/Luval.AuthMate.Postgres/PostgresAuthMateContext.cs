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
        /// Creates a new instance of the context
        /// </summary>
        /// <param name="connectionString">A valid postgres connection string</param>
        public PostgresAuthMateContext(string connectionString)
        {
                _connString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var logger = new ColorConsoleLogger();
            optionsBuilder.UseNpgsql(_connString);
            if(Debugger.IsAttached)
                optionsBuilder.LogTo((msg) => { 
                    logger.LogDebug(msg); 
                });
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

        }
    }
}
