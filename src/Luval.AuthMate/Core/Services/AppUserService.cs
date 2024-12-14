using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core.Services
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Service for managing app user-related operations.
    /// </summary>
    public class AppUserService : IAppUserService
    {
        private readonly IAuthMateContext _context;
        private readonly ILogger<AppUserService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppUserService"/> class.
        /// </summary>
        /// <param name="context">The database context interface.</param>
        /// <param name="logger">The logger instance.</param>
        public AppUserService(IAuthMateContext context, ILogger<AppUserService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a user by their email.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The user entity.</returns>
        public async Task<AppUser> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await TryGetUserByEmailAsync(email, cancellationToken).ConfigureAwait(false);
            if (user == null)
            {
                _logger.LogWarning("User with email {Email} not found.", email);
                throw new ArgumentException($"User with email '{email}' not found.", nameof(email));

            }
            return user;
        }

        /// <summary>
        /// Updates an app user.
        /// </summary>
        /// <param name="appUser">The updated app user entity.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The updated app user entity.</returns>
        public async Task<AppUser> UpdateAppUserAsync(AppUser appUser, CancellationToken cancellationToken = default)
        {
            try
            {
                if (appUser == null) throw new ArgumentNullException(nameof(appUser));

                // Ensure dates are in UTC and update metadata
                appUser.UtcCreatedOn = new DateTime(appUser.UtcCreatedOn.Ticks, DateTimeKind.Utc);
                appUser.UtcUpdatedOn = DateTime.UtcNow;
                appUser.UpdatedBy = appUser.Email;
                appUser.Version++;

                // Update the app user entity in the database
                var ent = _context.AppUsers.Update(appUser);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Successfully updated app user with email {Email}.", appUser.Email);
                return appUser;
            }
            catch (Exception ex)
            {
                // Log the error and rethrow to the caller
                _logger.LogError(ex, "Error updating app user with email {Email}.", appUser?.Email);
                throw;
            }
        }

        /// <summary>
        /// Adds a user to a role.
        /// </summary>
        /// <param name="userEmail">The email of the user.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddUserToRoleAsync(string userEmail, string roleName, CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieve the user by email
                var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken).ConfigureAwait(false);
                if (user == null) throw new InvalidOperationException($"User with email '{userEmail}' not found.");

                // Retrieve the role by name
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken).ConfigureAwait(false);
                if (role == null) throw new InvalidOperationException($"Role '{roleName}' not found.");

                // Create a new AppUserRole relationship
                var userRole = new AppUserRole { AppUserId = user.Id, RoleId = role.Id };
                await _context.AppUserRoles.AddAsync(userRole, cancellationToken).ConfigureAwait(false);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Successfully added user {UserEmail} to role {RoleName}.", userEmail, roleName);
            }
            catch (Exception ex)
            {
                // Log the error and rethrow to the caller
                _logger.LogError(ex, "Error adding user {UserEmail} to role {RoleName}.", userEmail, roleName);
                throw;
            }
        }

        /// <summary>
        /// Removes a user from a role.
        /// </summary>
        /// <param name="userEmail">The email of the user.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveUserFromRoleAsync(string userEmail, string roleName, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find the user-role relationship
                var userRole = await _context.AppUserRoles
                    .Include(aur => aur.User)
                    .Include(aur => aur.Role)
                    .FirstOrDefaultAsync(aur => aur.User.Email == userEmail && aur.Role.Name == roleName, cancellationToken).ConfigureAwait(false);

                if (userRole == null) throw new InvalidOperationException($"No relationship found between user '{userEmail}' and role '{roleName}'.");

                // Remove the relationship and save changes
                _context.AppUserRoles.Remove(userRole);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Successfully removed user {UserEmail} from role {RoleName}.", userEmail, roleName);
            }
            catch (Exception ex)
            {
                // Log the error and rethrow to the caller
                _logger.LogError(ex, "Error removing user {UserEmail} from role {RoleName}.", userEmail, roleName);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a user by their email. If no user is found, returns null.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The user entity if found, otherwise returns null.</returns>
        public async Task<AppUser> TryGetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                // Query the user by email and include their account information
                var user = await _context.AppUsers
                    .Include(u => u.Account)
                    .FirstOrDefaultAsync(u => u.Email == email, cancellationToken).ConfigureAwait(false);

                if (user != null)
                {
                    // Load the user's roles if the user exists
                    user.UserRoles = await _context.AppUserRoles.Include(i => i.Role).Where(i => i.AppUserId == user.Id).ToListAsync(cancellationToken).ConfigureAwait(false);
                }

                _logger.LogInformation(user != null ? "User {Email} retrieved successfully." : "User {Email} not found.", email);
                return user;
            }
            catch (Exception ex)
            {
                // Log the error and rethrow to the caller
                _logger.LogError(ex, "Error retrieving user {Email}.", email);
                throw;
            }
        }

        /// <summary>
        /// Creates a new user based on an invitation and associates the user with the specified account and role.
        /// </summary>
        /// <param name="invite">The invitation containing account and role information.</param>
        /// <param name="identity">The identity of the user to be created.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created app user entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="invite"/> or <paramref name="identity"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when required data is missing in the invitation.</exception>
        public async Task<AppUser> CreateUserFromInvitationAsync(InviteToAccount invite, ClaimsIdentity identity, CancellationToken cancellationToken = default)
        {
            try
            {
                if (invite == null)
                    throw new ArgumentNullException(nameof(invite));

                if (identity == null)
                    throw new ArgumentNullException(nameof(identity));

                // Create the user from the identity
                var user = identity.ToUser();

                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                    throw new InvalidOperationException("Invalid user identity. Email is required.");

                // Validate the invitation
                if (invite.Account == null || invite.Role == null)
                {
                    _logger.LogWarning("Invitation is missing required data: Account or Role is null.");
                    throw new InvalidOperationException("Invitation is missing required account or role information.");
                }

                // Populate user fields with invitation data
                user.AccountId = invite.AccountId;
                user.Account = invite.Account;
                user.CreatedBy = user.Email;
                user.UpdatedBy = user.Email;
                user.UtcCreatedOn = DateTime.UtcNow;
                user.UtcUpdatedOn = user.UtcCreatedOn;

                // Add the new user to the database
                await _context.AppUsers.AddAsync(user, cancellationToken).ConfigureAwait(false);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("User '{UserEmail}' created successfully and associated with account '{AccountName}'.", user.Email, invite.Account.Name);

                // Create and associate the user's role
                var userRole = new AppUserRole
                {
                    AppUserId = user.Id,
                    RoleId = invite.RoleId,
                    User = user,
                    Role = invite.Role
                };

                await _context.AppUserRoles.AddAsync(userRole, cancellationToken).ConfigureAwait(false);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                // Add the role to the user's role collection
                user.UserRoles.Add(userRole);

                _logger.LogInformation("Role '{RoleName}' associated successfully with user '{UserEmail}'.", invite.Role.Name, user.Email);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user from invitation for email '{UserEmail}'.", identity?.FindFirst(ClaimTypes.Email)?.Value);
                throw;
            }
        }


        /// <summary>
        /// Creates a new user from an invitation, assigns them an account, and grants the Administrator role.
        /// </summary>
        /// <param name="invite">The invitation containing account type information.</param>
        /// <param name="identity">The claims identity of the user being created.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created app user entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="invite"/> or <paramref name="identity"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when required data (e.g., account type or role) is missing.</exception>
        public async Task<AppUser> CreateUserFromInvitationAsync(InviteToApplication invite, ClaimsIdentity identity, CancellationToken cancellationToken = default)
        {
            try
            {
                if (invite == null)
                    throw new ArgumentNullException(nameof(invite), "Invitation cannot be null.");

                if (identity == null)
                    throw new ArgumentNullException(nameof(identity), "Identity cannot be null.");

                // Extract user from identity
                var user = identity.ToUser();
                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                    throw new InvalidOperationException("Invalid user identity. Email is required.");

                _logger.LogInformation("Creating user '{UserEmail}' from invitation.", user.Email);

                // Retrieve the account type
                var accountType = await _context.AccountTypes.SingleOrDefaultAsync(
                    x => x.Id == invite.AccountTypeId, cancellationToken).ConfigureAwait(false);

                if (accountType == null)
                {
                    _logger.LogWarning("Account type with ID {AccountTypeId} not found.", invite.AccountTypeId);
                    throw new InvalidOperationException($"Account type with ID '{invite.AccountTypeId}' not found.");
                }

                // Create a new account for the user
                var account = new Account
                {
                    Name = user.Email,
                    Owner = user.Email,
                    AccountTypeId = accountType.Id,
                    AccountType = accountType,
                    CreatedBy = user.Email,
                    UpdatedBy = user.Email,
                    Version = 1
                };

                await _context.Accounts.AddAsync(account, cancellationToken).ConfigureAwait(false);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Account '{AccountName}' created successfully for user '{UserEmail}'.", account.Name, user.Email);

                // Assign the account to the user
                user.AccountId = account.Id;
                user.Account = account;

                await _context.AppUsers.AddAsync(user, cancellationToken).ConfigureAwait(false);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("User '{UserEmail}' created and associated with account '{AccountName}'.", user.Email, account.Name);

                // Check if the Administrator role exists
                var adminRole = await _context.Roles.FirstOrDefaultAsync(
                    r => r.Name == "Administrator", cancellationToken).ConfigureAwait(false);

                if (adminRole == null)
                {
                    _logger.LogInformation("Administrator role not found. Creating new Administrator role.");

                    adminRole = new Role
                    {
                        Name = "Administrator",
                        Description = "Administrator role with full permissions."
                    };

                    await _context.Roles.AddAsync(adminRole, cancellationToken).ConfigureAwait(false);
                    await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    _logger.LogInformation("Administrator role created successfully.");
                }

                // Assign the Administrator role to the user
                var userRole = new AppUserRole
                {
                    AppUserId = user.Id,
                    RoleId = adminRole.Id,
                    User = user,
                    Role = adminRole
                };

                await _context.AppUserRoles.AddAsync(userRole, cancellationToken).ConfigureAwait(false);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                //add the role to the user's role collection
                user.UserRoles.Add(userRole);

                _logger.LogInformation("Administrator role assigned to user '{UserEmail}'.", user.Email);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user from invitation.");
                throw;
            }
        }
    }

}
