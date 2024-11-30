using Microsoft.EntityFrameworkCore;
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
            optionsBuilder.UseNpgsql(_connString);
            if(Debugger.IsAttached)
                optionsBuilder.LogTo(Console.WriteLine);
        }
    }
}
