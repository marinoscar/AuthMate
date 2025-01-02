﻿using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Services;
using Luval.AuthMate.Infrastructure.Configuration;
using Luval.AuthMate.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
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
        
        private AppConnectionService CreateService(Action<IAuthMateContext>? contextSetup, Action<Mock<IAuthorizationCodeFlowService>>? authCodeServiceMockSetup = null)
        {
            var context = new MemoryDataContext();
            context.Initialize();

            var authCodeServiceMock = new Mock<IAuthorizationCodeFlowService>();

            if(authCodeServiceMockSetup == null)
            {
                authCodeServiceMock.Setup(i => i.PostAuthorizationCodeRequestAsync(It.IsAny<OAuthConnectionConfig>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(SampleHttpResponse())
                });
            }
            else authCodeServiceMockSetup(authCodeServiceMock);


            var service = new AppConnectionService(context, authCodeServiceMock.Object, new NullUserResolver(), new NullLogger<AppConnectionService>());

            contextSetup?.Invoke(context);
            return service;
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
            // Arrange
            var id = 0ul;
            var token = "token123";
            var connection = default(AppConnection);
            var service = CreateService((c) =>
            {
                var acc = c.Accounts.First();
                connection = new AppConnection { AccountId = acc.Id, AccessToken = token, ProviderName = "Provider", OwnerEmail = "o@email.com" };
                id = connection.Id;
                c.AppConnections.Add(connection);
                c.SaveChanges();
            });

            // Act
            await service.DeleteConnectionAsync(connection.Id);

            // Assert
            var result = await service.GetConnectionsAsync(c => c.Id == id);
            Assert.Null(result.SingleOrDefault());
        }

        [Fact]
        public async Task DeleteAppConnectionAsync_ThrowsInvalidOperationException_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = 983UL;
            var service = CreateService((c) =>
            {
                var acc = c.Accounts.First();
                c.AppConnections.Add(new AppConnection { AccountId = acc.Id, AccessToken = "token123", ProviderName = "Provider", OwnerEmail = "o@email.com" });
                c.SaveChanges();
            });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.DeleteConnectionAsync(invalidId));
        }

        [Fact]
        public async Task DeleteAppConnectionAsync_ThrowsArgumentException_WhenIdIsZero()
        {
            // Arrange
            var service = CreateService(null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await service.DeleteConnectionAsync(0));
        }

        [Fact]
        public void CreateAuthorizationConsentUrl_ThrowsArgumentNullException_WhenConfigIsNull()
        {
            // Arrange
            var service = CreateService(null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => service.CreateAuthorizationConsentUrl(null));
        }

        [Fact]
        public void CreateAuthorizationConsentUrl_CreatesValidUrl()
        {
            // Arrange
            var config = new OAuthConnectionConfig
            {
                AuthorizationEndpoint = "https://example.com/auth",
                ClientId = "client-id",
                RedirectUri = "https://example.com/callback",
                Scopes = "scope1 scope2"
            };
            var service = CreateService(null);

            // Act
            var result = service.CreateAuthorizationConsentUrl(config);

            // Assert
            var expectedUrl = "https://example.com/auth?response_type=code&client_id=client-id&redirect_uri=https://example.com/callback&scope=scope1%20scope2access_type=offline&prompt=consent";
            Assert.Equal(expectedUrl, result);
        }

        [Fact]
        public void CreateAuthorizationConsentUrl_EscapesScopeProperly()
        {
            // Arrange
            var config = new OAuthConnectionConfig
            {
                AuthorizationEndpoint = "https://example.com/auth",
                ClientId = "client-id",
                RedirectUri = "https://example.com/callback",
                Scopes = "scope1 scope2"
            };
            var service = CreateService(null);

            // Act
            var result = service.CreateAuthorizationConsentUrl(config);

            // Assert
            Assert.Contains("scope=scope1%20scope2", result);
        }

        [Fact]
        public async Task CreateAuthorizationCodeRequestAsync_ThrowsArgumentNullException_WhenConfigIsNull()
        {
            // Arrange
            var service = CreateService(null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateAuthorizationCodeRequestAsync(null, "code", null));
        }

        [Fact]
        public async Task CreateAuthorizationCodeRequestAsync_ThrowsArgumentNullException_WhenCodeIsNull()
        {
            // Arrange
            var config = new OAuthConnectionConfig();
            var service = CreateService(null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateAuthorizationCodeRequestAsync(config, null, null));
        }



        [Fact]
        public async Task CreateAuthorizationCodeRequestAsync_ThrowsInvalidOperationException_WhenErrorIsNotNull()
        {
            // Arrange
            var config = new OAuthConnectionConfig();
            var service = CreateService(null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAuthorizationCodeRequestAsync(config, "code", "error"));
        }


        [Fact]
        public async Task CreateAuthorizationCodeRequestAsync_ReturnsOAuthTokenResponse_WhenSuccessful()
        {
            // Arrange
            var config = new OAuthConnectionConfig();
            var service = CreateService(null);

            // Act
            var result = await service.CreateAuthorizationCodeRequestAsync(config, "code", null);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateAuthorizationCodeRequestAsync_ThrowsInvalidOperationException_WhenStatusCodeIsNotSuccess()
        {
            // Arrange
            Action<Mock<IAuthorizationCodeFlowService>> mockSetup = (m) =>
            {
                m.Setup(i => i.PostAuthorizationCodeRequestAsync(It.IsAny<OAuthConnectionConfig>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request")
                });
            };
            var config = new OAuthConnectionConfig();
            var service = CreateService(null, mockSetup);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAuthorizationCodeRequestAsync(config, "code", null));
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