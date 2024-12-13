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
    }
}
