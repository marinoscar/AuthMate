using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Luval.AuthMate.Infrastructure.Logging;

namespace Luval.AuthMate.Tests
{
    public class AuthenticationServiceTests
    {

        private AuthenticationService CreateService(Action<IAuthMateContext> afterContextCreation)
        {
            var context = new MemoryDataContext();
            context.Initialize();

            var userService = new AppUserService(context, new NullLogger<AppUserService>());
            var authService = new AuthenticationService(userService, context, new NullLogger<AuthenticationService>());

            if (afterContextCreation != null) afterContextCreation(context);
            return authService;
        }

        [Fact]
        public async Task AuthorizeUserAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var email = "owner@email.com";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) });

            //Creates a new instance of the service and then it adds the user to the context
            var service = CreateService((c) =>
            {
            });

            // Act
            var result = await service.AuthorizeUserAsync(identity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

    }
}
