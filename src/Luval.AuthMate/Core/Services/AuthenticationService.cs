using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core.Services
{
    /// <summary>
    /// Service for managing authentication-related operations.
    /// </summary>
    public class AuthenticationService
    {
        private readonly IAppUserService _userService;
        private readonly IAuthMateContext _context;
        private readonly ILogger<AuthenticationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// </summary>
        /// <param name="userService">The app user service instance.</param>
        /// <param name="context">The database context interface.</param>
        /// <param name="logger">The logger instance.</param>
        public AuthenticationService(IAppUserService userService, IAuthMateContext context, ILogger<AuthenticationService> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authorizes a user based on their identity and invitation information.
        /// </summary>
        /// <param name="identity">The claims identity of the user to authorize.</param>
        /// <param name="tokenResponse">The <see cref="TokenResponse"/> coming from the user OAuth context.</param>
        /// <param name="deviceInfo">Optional device information for the login event.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The authorized app user entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="identity"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when required identity information is missing.</exception>
        /// <exception cref="AuthMateException">Thrown when the user cannot be authenticated.</exception>
        public async Task<AppUser> AuthorizeUserAsync(ClaimsIdentity identity, TokenResponse tokenResponse, DeviceInfo deviceInfo = default, CancellationToken cancellationToken = default)
        {
            try
            {
                if (identity == null)
                    throw new ArgumentNullException(nameof(identity), "Identity cannot be null.");

                if (tokenResponse == null)
                    throw new ArgumentNullException(nameof(tokenResponse), "Token response cannot be null.");

                var email = identity.GetValue(ClaimTypes.Email);
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email is required and not found on ClaimsIdentity.", nameof(identity));

                _logger.LogInformation("Authorizing user with email '{Email}'.", email);

                // Attempt to retrieve the user
                var user = await _userService.TryGetUserByEmailAsync(email, cancellationToken).ConfigureAwait(false);

                if (user != null)
                {
                    if(user.UtcActiveUntil != null && user.UtcActiveUntil < DateTime.UtcNow)
                    {
                        _logger.LogError("Account {0} expired on {1} UTC", user.Account.Owner, user.UtcActiveUntil);
                        throw new AuthMateException($"User account is not active.");
                    }
                    return await UpdateUserAndLoginInformationAsync(identity, tokenResponse, user, deviceInfo, cancellationToken).ConfigureAwait(false);
                }

                // Check for account invitations
                var accountInvite = await ValidateInvitationToAccount(identity, cancellationToken).ConfigureAwait(false);
                if (accountInvite != null)
                    user = await _userService.CreateUserFromInvitationAsync(accountInvite, identity, cancellationToken).ConfigureAwait(false);
                else
                {
                    // Check for application-level invitations
                    var appInvite = await ValidateInvitationToApplication(email, cancellationToken).ConfigureAwait(false);
                    if (appInvite != null)
                        user = await _userService.CreateUserFromInvitationAsync(appInvite, identity, cancellationToken).ConfigureAwait(false);

                    if (user == null)
                    {
                        _logger.LogWarning("Authorization failed for user with email '{Email}'.", email);
                        throw new AuthMateException($"Unable to authenticate user: {email}");
                    }
                }

                return await UpdateUserAndLoginInformationAsync(identity, tokenResponse, user, deviceInfo, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authorizing user.");
                throw;
            }
        }

        /// <summary>
        /// Validates an invitation to an account based on the user's identity.
        /// </summary>
        /// <param name="identity">The claims identity of the user.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The invitation to the account if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="identity"/> is null.</exception>
        public async Task<InviteToAccount> ValidateInvitationToAccount(ClaimsIdentity identity, CancellationToken cancellationToken = default)
        {
            try
            {
                if (identity == null)
                    throw new ArgumentNullException(nameof(identity), "Identity cannot be null.");

                var user = identity.ToUser();

                var invite = await _context.InvitesToAccount.Include(i => i.Account).Include(i => i.Role)
                    .FirstOrDefaultAsync(i => i.Email == user.Email, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(invite != null
                    ? "Account invitation found for email '{Email}'."
                    : "No account invitation found for email '{Email}'.", user.Email);

                return invite;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating account invitation.");
                throw;
            }
        }

        /// <summary>
        /// Validates an invitation to the application based on the user's email.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The invitation to the application if found; otherwise, null.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is null or whitespace.</exception>
        public async Task<InviteToApplication> ValidateInvitationToApplication(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email is required.", nameof(email));

                var invite = await _context.InvitesToApplication
                    .Include(i => i.AccountType)
                    .FirstOrDefaultAsync(i => i.Email == email, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation(invite != null
                    ? "Application invitation found for email '{Email}'."
                    : "No application invitation found for email '{Email}'.", email);

                return invite;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating application invitation.");
                throw;
            }
        }

        private async Task<AppUser> UpdateUserAndLoginInformationAsync(ClaimsIdentity identity, TokenResponse tokenResponse, AppUser user, DeviceInfo deviceInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                //Check for the user account to be active
                ValidateAccountIsActive(user);

                _logger.LogInformation("Updating login information for user '{UserEmail}'.", user.Email);

                user.UtcLastLogin = DateTime.UtcNow;
                user.OAuthAccessToken = tokenResponse.AccessToken;
                user.OAuthRefreshToken = tokenResponse.RefreshToken;
                user.OAuthTokenUtcExpiresAt = tokenResponse.UtcExpiresAt;
                user.OAuthTokenType = tokenResponse.TokenType;

                await _userService.UpdateAppUserAsync(user, cancellationToken).ConfigureAwait(false);

                identity.AddClaim(new Claim("AppUserJson", user.ToString()));

                await AddLogHistoryAsync(deviceInfo, user.Email, cancellationToken).ConfigureAwait(false);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating login information for user '{UserEmail}'.", user.Email);
                throw;
            }
        }

        private void ValidateAccountIsActive(AppUser appUser)
        {
            var isValid = appUser.Account != null && (appUser.Account.UtcExpirationDate == null  || appUser.Account.UtcExpirationDate > DateTime.UtcNow);
            if (!isValid)
            {
                _logger.LogError("Account {0} expired on {1} UTC", appUser.Account.Owner, appUser.Account.UtcExpirationDate);
                throw new AuthMateException($"User account is not active.");
            }
        }

        /// <summary>
        /// Adds a login history record to the database asynchronously using device information and user email.
        /// </summary>
        /// <param name="deviceInfo">The device information, including browser, OS, and IP address.</param>
        /// <param name="email">The email address of the user logging in.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created <see cref="AppUserLoginHistory"/> entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="deviceInfo"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="email"/> is null or whitespace.</exception>
        public async Task<AppUserLoginHistory> AddLogHistoryAsync(DeviceInfo deviceInfo, string email, CancellationToken cancellationToken = default)
        {
            try
            {
                if (deviceInfo == null)
                    deviceInfo = DeviceInfo.CreateEmpty();

                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Email is required.", nameof(email));

                var history = new AppUserLoginHistory
                {
                    Email = email,
                    Browser = deviceInfo.Browser,
                    IpAddress = deviceInfo.IpAddress,
                    OS = deviceInfo.OS,
                    UtcLogIn = DateTime.UtcNow
                };

                await _context.AppUserLoginHistories.AddAsync(history, cancellationToken).ConfigureAwait(false);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Login history added for user '{Email}'.", email);

                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding login history for user '{Email}'.", email);
                throw;
            }
        }
    }
}
