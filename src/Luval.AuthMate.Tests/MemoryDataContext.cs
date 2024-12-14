using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Infrastructure.Data;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Tests
{
    public class MemoryDataContext : AuthMateContext
    {
        public MemoryDataContext() : base(GetOptions())
        {
            Database.OpenConnection();
            Database.EnsureCreated();
        }

        private static DbContextOptions GetOptions()
        {
            var options = new DbContextOptionsBuilder<MemoryDataContext>()
                        .UseSqlite("Filename=:memory:") // Use in-memory SQLite database
                        .LogTo((s) => Debug.WriteLine(s))
                        .EnableSensitiveDataLogging()
                        .Options;
            return options;
        }

        public void Initialize()
        {
            Database.OpenConnection();
            Database.EnsureCreated();

            var adminRole = new Role() { Name = "Administrator" };
            Roles.Add(adminRole); 
            SaveChanges();

            var acType = new AccountType() { Name = "Free" };
            AccountTypes.Add(acType);
            SaveChanges();

            var account = new Account() { Name = "owner@email.com", Owner = "owner@email.com", AccountTypeId = acType.Id };
            Accounts.Add(account);
            SaveChanges();


            var user = new AppUser()
            {
                AccountId = account.Id,
                Email = "owner@email.com",
                ProviderKey = "GOOGLE:ID",
                ProviderType = "Google",
            };

            AppUsers.Add(user);
            SaveChanges();

        }

        public override void Dispose()
        {
            Database.EnsureDeleted();
            base.Dispose();
        }


    }
}
