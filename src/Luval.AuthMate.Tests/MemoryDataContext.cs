using Luval.AuthMate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Tests
{
    public class MemoryDataContext : AuthMateContext
    {
        public MemoryDataContext() : base(GetOptions())
        {
           

        }

        private static DbContextOptions GetOptions()
        {
            var options = new DbContextOptionsBuilder<MemoryDataContext>()
                        .UseInMemoryDatabase(databaseName: "TestDatabase")
                        .Options;
            return options;
        }


    }
}
