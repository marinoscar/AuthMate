using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Luval.AuthMate.Sqlite
{
    /// <summary>
    /// Represents a SQLite implementation of the <see cref="IAuthMateContext"/> interface.
    /// </summary>
    public class SqliteAuthMateContext : AuthMateContext, IAuthMateContext
    {
        /// <summary>
        /// The connection string for the SQLite database.
        /// </summary>
        private string _connString;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteAuthMateContext"/> class with the default connection string.
        /// </summary>
        public SqliteAuthMateContext() : this("Data Source=app.db")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteAuthMateContext"/> class with the specified options.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public SqliteAuthMateContext(DbContextOptions options) : this(options, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteAuthMateContext"/> class with the specified options and connection string.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        /// <param name="connectionString">The connection string for the SQLite database.</param>
        public SqliteAuthMateContext(DbContextOptions options, string connectionString) : base(options)
        {
            _connString = connectionString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteAuthMateContext"/> class with the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string for the SQLite database.</param>
        public SqliteAuthMateContext(string connectionString)
        {
            _connString = connectionString;
        }

        /// <summary>
        /// Configures the database (and other options) to be used for this context.
        /// </summary>
        /// <param name="optionsBuilder">A builder used to create or modify options for this context.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!string.IsNullOrEmpty(_connString))
                optionsBuilder.UseSqlite(_connString);
        }

        /// <summary>
        /// Configures the model and relationships between entities.
        /// </summary>
        /// <param name="modelBuilder">The builder used to configure the entities.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
