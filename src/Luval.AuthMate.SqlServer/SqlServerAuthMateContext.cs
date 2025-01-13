using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Luval.AuthMate.SqlServer
{
    /// <summary>
    /// Represents the SQL Server-specific implementation of the AuthMateContext.
    /// </summary>
    public class SqlServerAuthMateContext : AuthMateContext, IAuthMateContext
    {
        private string _connString;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerAuthMateContext"/> class with the specified options.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public SqlServerAuthMateContext(DbContextOptions options) : this(options, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerAuthMateContext"/> class with the specified options and connection string.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        /// <param name="connectionString">The connection string to be used for the SQL Server database.</param>
        public SqlServerAuthMateContext(DbContextOptions options, string connectionString) : base(options)
        {
            _connString = connectionString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerAuthMateContext"/> class with the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to be used for the SQL Server database.</param>
        public SqlServerAuthMateContext(string connectionString)
        {
            _connString = connectionString;
        }

        /// <summary>
        /// Configures the database context options, including the connection string and migration history table.
        /// </summary>
        /// <param name="optionsBuilder">The builder used to configure the context options.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Continue with regular implementation
            base.OnConfiguring(optionsBuilder);

            // Add connection string if provided
            if (!string.IsNullOrEmpty(_connString))
                optionsBuilder.UseSqlServer(_connString, (o) =>
                {
                    o.MigrationsHistoryTable("__EFMigrationsHistory_AuthMate", "authmate");
                });
        }
    }
}
