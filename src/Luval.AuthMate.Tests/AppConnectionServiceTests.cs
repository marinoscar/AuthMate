using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core.Services;
using Luval.AuthMate.Infrastructure.Configuration;
using Luval.AuthMate.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

            if (authCodeServiceMockSetup == null)
            {
                authCodeServiceMock.Setup(i => i.PostAuthorizationCodeRequestAsync(It.IsAny<OAuthConnectionConfig>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(GetCodeResponse())
                });
            }
            else authCodeServiceMockSetup(authCodeServiceMock);

            var configList = new List<OAuthConnectionConfig>() {
                    new() {
                        Name = "Google",
                        ClientId = "client-id",
                        ClientSecret = "client",
                        RedirectUri = "api/callback",
                        AuthorizationEndpoint = "https://example.com/auth",
                        TokenEndpoint = "https://example.com/token",
                        Scopes = "scope1 scope2"
                    },
                    new() {
                        Name = "Microsoft",
                        ClientId = "client-id",
                        ClientSecret = "client",
                        RedirectUri = "api/callback",
                        AuthorizationEndpoint = "https://example.com/auth",
                        TokenEndpoint = "https://example.com/token",
                        Scopes = "scope1 scope2"
                    }
            };

            var service = new AppConnectionService(context, authCodeServiceMock.Object, new NullUserResolver(), new OAuthConnectionManager(configList), new NullLogger<AppConnectionService>());

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
        public async Task GetAppConnectionAsync_ReturnsConnection_WithExpression()
        {
            // Arrange
            var accountId = 1UL;
            var provName = "My Provider";
            var email = "user@email.com";
            var service = CreateService((c) =>
            {
                var account = c.Accounts.First();
                accountId = account.Id;
                var secondAccount = new Account() { AccountTypeId = 1, Name = "AnotherOne", Owner = "someone@mail.com" };
                c.Accounts.Add(secondAccount);
                c.SaveChanges();
                c.AppConnections.Add(new AppConnection { AccountId = account.Id, OwnerEmail = email, ProviderName = provName, AccessToken = "new token" });
                c.AppConnections.Add(new AppConnection { AccountId = secondAccount.Id, OwnerEmail = "someone@mail.com", ProviderName = provName, AccessToken = "new token" });
                c.SaveChanges();
            });

            // Act
            var result = await service.GetConnectionsAsync(i => i.ProviderName == provName);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count());
            Assert.Equal(accountId, result.First().AccountId);
            Assert.Equal(provName, result.First().ProviderName);
            Assert.Equal(email, result.First().OwnerEmail);
        }

        [Fact]
        public async Task UpdateAppConnectionAsync_UpdatesConnection_WhenValid()
        {
            // Arrange
            var provName = "My Provider";
            var email = "user@email.com";
            var newToken = "new acess token";
            AppConnection connection = default!;

            var service = CreateService((c) =>
            {
                var account = c.Accounts.First();
                connection = new AppConnection { AccountId = account.Id, OwnerEmail = email, ProviderName = provName, AccessToken = "new token", Version = 1 };
                c.AppConnections.Add(connection);
                c.SaveChanges();
            });

            connection.AccessToken = newToken;
            // Act
            var result = await service.PersistConnectionAsync(connection);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2ul, connection.Version);
            Assert.Equal(provName, result.ProviderName);
            Assert.Equal(newToken, result.AccessToken);
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
            Assert.Throws<ArgumentNullException>(() => service.CreateAuthorizationConsentUrl(null, "https://localhost:7001"));
        }

        [Fact]
        public void CreateAuthorizationConsentUrl_CreatesValidUrl()
        {
            // Arrange
            var baseUrl = "https://localhost:7001";
            var config = new OAuthConnectionConfig
            {
                AuthorizationEndpoint = "https://example.com/auth",
                ClientId = "client-id",
                RedirectUri = "api/callback",
                Scopes = "scope1 scope2",
                Name = "Google"
            };
            var service = CreateService(null);

            // Act
            var result = service.CreateAuthorizationConsentUrl(config, baseUrl);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(Uri.EscapeDataString(baseUrl), result);
            Assert.Contains(Uri.EscapeDataString(config.Scopes), result);
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
                Scopes = "scope1 scope2",
                Name = "Google"
            };
            var service = CreateService(null);

            // Act
            var result = service.CreateAuthorizationConsentUrl(config, "https://localhost:7001");

            // Assert
            Assert.Contains("scope=scope1%20scope2", result);
        }

        [Fact]
        public async Task CreateAuthorizationCodeRequestAsync_ThrowsArgumentNullException_WhenConfigIsNull()
        {
            // Arrange
            var service = CreateService(null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateAuthorizationCodeRequestAsync(null, "code"));
        }

        [Fact]
        public async Task CreateAuthorizationCodeRequestAsync_ThrowsArgumentNullException_WhenCodeIsNull()
        {
            // Arrange
            var config = new OAuthConnectionConfig();
            var service = CreateService(null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateAuthorizationCodeRequestAsync(config, null));
        }


        [Fact]
        public async Task CreateAuthorizationCodeRequestAsync_ReturnsOAuthTokenResponse_WhenSuccessful()
        {
            // Arrange
            var config = new OAuthConnectionConfig();
            var service = CreateService(null);

            // Act
            var result = await service.CreateAuthorizationCodeRequestAsync(config, "code");

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
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAuthorizationCodeRequestAsync(config, "code"));
        }

        [Fact]
        public async Task ResolveAccessTokenAsync_ValidTokenReturned()
        {
            // Arrange
            var providerName = "Google";
            var ownerEmail = "user@example.com";
            var service = CreateService((c) =>
            {
                var acc = c.Accounts.First();
                var conn = new AppConnection { AccountId = acc.Id, ProviderName = providerName, OwnerEmail = ownerEmail, AccessToken = "valid_token", UtcIssuedOn = DateTime.UtcNow.AddMinutes(30), DurationInSeconds = 9000 };
                c.AppConnections.Add(conn);
                c.SaveChanges();
            });

            // Act
            var result = await service.ResolveAccessTokenAsync(providerName, ownerEmail);

            // Assert
            Assert.Equal("valid_token", result);
        }

        [Fact]
        public async Task ResolveAccessTokenAsync_ConnectionDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var providerName = "Google";
            var ownerEmail = "user@example.com";
            var service = CreateService((c) =>
            {
                var acc = c.Accounts.First();
                var conn = new AppConnection { AccountId = acc.Id, ProviderName = "AnotherProvider", OwnerEmail = ownerEmail, AccessToken = "valid_token", UtcIssuedOn = DateTime.UtcNow.AddMinutes(30), DurationInSeconds = 9000 };
                c.AppConnections.Add(conn);
                c.SaveChanges();
            });

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ResolveAccessTokenAsync(providerName, ownerEmail));
        }

        [Fact]
        public async Task ResolveAccessTokenAsync_ConnectionExists_AccessTokenExpired()
        {
            // Arrange
            var providerName = "Google";
            var ownerEmail = "user@example.com";
            var service = CreateService((c) =>
            {
                var acc = c.Accounts.First();
                var conn = new AppConnection
                {
                    AccountId = acc.Id,
                    ProviderName = providerName,
                    OwnerEmail = ownerEmail,
                    AccessToken = "valid_token",
                    RefreshToken = "refresh_token",
                    UtcIssuedOn = DateTime.UtcNow.AddDays(-1),
                    DurationInSeconds = 3600
                };
                c.AppConnections.Add(conn);
                c.SaveChanges();
            }, (s) =>
            {
                s.Setup(i => i.PostRefreshTokenRequestAsync(It.IsAny<OAuthConnectionConfig>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(GetRefreshResponse()) });
            });

            // Act
            var result = await service.ResolveAccessTokenAsync(providerName, ownerEmail);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ResolveAccessTokenAsync_ConnectionExists_AccessTokenExpired_NoRefreshToken_ThrowsInvalidOperationException()
        {
            // Arrange
            var providerName = "Google";
            var ownerEmail = "user@example.com";
            var service = CreateService((c) =>
            {
                var acc = c.Accounts.First();
                var conn = new AppConnection
                {
                    AccountId = acc.Id,
                    ProviderName = providerName,
                    OwnerEmail = ownerEmail,
                    AccessToken = "valid_token",
                    RefreshToken = default,
                    UtcIssuedOn = DateTime.UtcNow.AddDays(-1),
                    DurationInSeconds = 3600
                };
                c.AppConnections.Add(conn);
                c.SaveChanges();
            }, (s) =>
            {
                s.Setup(i => i.PostRefreshTokenRequestAsync(It.IsAny<OAuthConnectionConfig>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(GetRefreshResponse()) });
            });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ResolveAccessTokenAsync(providerName, ownerEmail));
        }



        private static string GetCodeResponse()
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

        private static string GetRefreshResponse()
        {
            return @"
{
  ""token_type"": ""Bearer"",
  ""scope"": ""scope1 scope2"",
  ""expires_in"": 3600,
  ""ext_expires_in"": 3600,
  ""access_token"": ""access_token"",
  ""refresh_token"": ""refresh_token""
}
";
        }
    }
}
