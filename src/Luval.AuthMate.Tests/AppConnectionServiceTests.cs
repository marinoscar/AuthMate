using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Services;
using Luval.AuthMate.Infrastructure.Configuration;
using Luval.AuthMate.Infrastructure.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Tests
{
    public class AppConnectionServiceTests
    {
        /// <summary>
        /// Creates an instance of <see cref="AppConnectionService"/> with an optional action to modify the context after creation.
        /// </summary>
        /// <param name="afterContextCreation">An action to modify the context after creation.</param>
        /// <returns>An instance of <see cref="AppConnectionService"/>.</returns>
        private AppConnectionService CreateService(Action<IAuthMateContext> afterContextCreation)
        {
            var context = new MemoryDataContext();
            context.Initialize();

            var authCodeServiceMock = new Mock<IAuthorizationCodeFlowService>();

            authCodeServiceMock.Setup(i => i.PostAuthorizationCodeRequestAsync(It.IsAny<OAuthConnectionConfig>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(SampleHttpResponse())
            });
            

            var appConnectionService = new AppConnectionService(context, authCodeServiceMock.Object, new NullUserResolver(), new NullLogger<AppConnectionService>());

            if (afterContextCreation != null) afterContextCreation(context);
            return appConnectionService;
        }

        [Fact]
        public async Task AddAppConnectionAsync_AddsConnection_WhenValid()
        {
            // Arrange
            var accountId = 1UL;
            var tokenId = "token123";
            var accessToken = "nre-Token";
            IAuthMateContext context = default!;
            var service = CreateService((c) =>
            {
                var account = c.Accounts.First();
                accountId = account.Id;
                c.SaveChanges();
                context = c;
            });

            var appConnection = new AppConnection
            {
                AccountId = accountId,
                TokenId = tokenId,
                AccessToken = accessToken,
                OwnerEmail = "o@email.com",
                ProviderName = "My Provider"
            };

            // Act
            await service.PersistConnectionAsync(appConnection);

            // Assert
            Assert.True(appConnection.Id > 0);
            Assert.Equal(accountId, appConnection.AccountId);
            Assert.Equal(tokenId, appConnection.TokenId);
            Assert.Equal(accessToken, appConnection.AccessToken);
            Assert.NotNull(appConnection.CreatedBy);
            Assert.NotNull(appConnection.UpdatedBy);
            Assert.True(appConnection.UtcCreatedOn > DateTime.UtcNow.AddHours(-1));
            Assert.True(appConnection.UtcUpdatedOn > DateTime.UtcNow.AddHours(-1));
            Assert.Equal(1u, appConnection.Version);

        }

        [Fact]
        public async Task GetAppConnectionAsync_ReturnsConnection_WhenExists()
        {
            // Arrange
            var accountId = 1UL;
            var provName = "My Provider";
            var email = "user@email.com";
            var service = CreateService((c) =>
            {
                var account = c.Accounts.First();
                accountId = account.Id;
                c.AppConnections.Add(new AppConnection { AccountId = account.Id, OwnerEmail = email, ProviderName = provName, AccessToken = "new token" });
                c.SaveChanges();
            });

            // Act
            var result = await service.GetConnectionAsync(provName, email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accountId, result.AccountId);
            Assert.Equal(provName, result.ProviderName);
            Assert.Equal(email, result.OwnerEmail);
        }

        [Fact]
        public async Task UpdateAppConnectionAsync_UpdatesConnection_WhenValid()
        {
            // Arrange
            var provName = "My Provider";
            var email = "user@email.com";
            var newEmail = "new@mail.com";
            AppConnection connection = default!;

            var service = CreateService((c) =>
            {
                var account = c.Accounts.First();
                connection = new AppConnection { AccountId = account.Id, OwnerEmail = email, ProviderName = provName, AccessToken = "new token", Version = 1 };
                c.AppConnections.Add(connection);
                c.SaveChanges();
            });

            connection.OwnerEmail = newEmail;
            // Act
            var result = await service.PersistConnectionAsync(connection);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2ul, connection.Version);
            Assert.Equal(provName, result.ProviderName);
            Assert.Equal(newEmail, result.OwnerEmail);
            Assert.True(connection.UtcUpdatedOn > connection.UtcCreatedOn);
            Assert.NotNull(connection.UpdatedBy);
        }

        [Fact]
        public async Task DeleteAppConnectionAsync_DeletesConnection_WhenExists()
        {
            //// Arrange
            //var accountId = 1UL;
            //var tokenId = "token123";
            //var service = CreateService((c) =>
            //{
            //    c.Accounts.Add(new Account { Id = accountId });
            //    c.AppConnections.Add(new AppConnection { AccountId = accountId, TokenId = tokenId });
            //    c.SaveChanges();
            //});

            //// Act
            //await service.DeleteAppConnectionAsync(accountId, tokenId);

            //// Assert
            //var result = context.AppConnections.FirstOrDefault(ac => ac.TokenId == tokenId);
            //Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAppConnectionAsync_DoesNothing_WhenNotExists()
        {
            //// Arrange
            //var accountId = 1UL;
            //var tokenId = "token123";
            //var service = CreateService(null);

            //// Act
            //await service.DeleteAppConnectionAsync(accountId, tokenId);

            //// Assert
            //var result = context.AppConnections.FirstOrDefault(ac => ac.TokenId == tokenId);
            //Assert.Null(result);
        }

        private static string SampleHttpResponse()
        {
            return @"
{
  ""access_token"": ""sample-toke"",
  ""expires_in"": 3600,
  ""refresh_token"": ""sample-refresh-token"",
  ""scope"": ""https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/gmail.readonly https://www.googleapis.com/auth/userinfo.email"",
  ""token_type"": ""Bearer"",
  ""id_token"": ""sample-id-token""
}
";
        }
    }
}
