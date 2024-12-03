using Luval.AuthMate.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Principal;

namespace Luval.AuthMate
{
    /// <summary>
    /// Service for managing authentication-related operations in the system.
    /// </summary>
    public class AuthMateService : IAuthMateService
    {
        private readonly IAuthMateContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthMateService"/> class.
        /// </summary>
        /// <param name="context">The database context interface to be used for database operations.</param>
        public AuthMateService(IAuthMateContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Creates a new account type.
        /// </summary>
        /// <param name="name">The name of the account type.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created account type entity.</returns>
        public async Task<AccountType> CreateAccountTypeAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Account type name is required.", nameof(name));

            var accountType = new AccountType { Name = name };
            await _context.AccountTypes.AddAsync(accountType, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return accountType;
        }


        /// <summary>
        /// Creates a new account.
        /// </summary>
        /// <param name="name">The name of the account.</param>
        /// <param name="accountTypeName">The name of the account type.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created account entity.</returns>
        public async Task<Account> CreateAccountAsync(string name, string accountTypeName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Account name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(accountTypeName)) throw new ArgumentException("Account type name is required.", nameof(accountTypeName));

            var accountType = await _context.AccountTypes.FirstOrDefaultAsync(at => at.Name == accountTypeName, cancellationToken);
            if (accountType == null) throw new InvalidOperationException($"Account type '{accountTypeName}' not found.");

            var account = new Account { Name = name, AccountTypeId = accountType.Id };
            await _context.Accounts.AddAsync(account, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return account;
        }

        /// <summary>
        /// Creates a new app user and associates it with an existing account.
        /// </summary>
        /// <param name="appUser">The app user entity to be created.</param>
        /// <param name="owner">The owner to retrieve the account for association.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created app user entity.</returns>
        public async Task<AppUser> CreateAppUserAsync(AppUser appUser, string owner, CancellationToken cancellationToken = default)
        {
            if (appUser == null) throw new ArgumentNullException(nameof(appUser));
            if (string.IsNullOrWhiteSpace(owner)) throw new ArgumentException("Owner is required to find the account.", nameof(owner));

            // Retrieve the account for the given owner
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Owner == owner, cancellationToken);
            if (account == null)
                throw new InvalidOperationException($"No account found for the owner '{owner}'.");

            // Associate the user with the account
            appUser.AccountId = account.Id;
            appUser.Account = account;

            // Add and save the new AppUser entity
            await _context.AppUsers.AddAsync(appUser, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return appUser;
        }


        /// <summary>
        /// Updates an app user.
        /// </summary>
        /// <param name="appUser">The updated app user entity.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The updated app user entity.</returns>
        public async Task<AppUser> UpdateAppUserAsync(AppUser appUser, CancellationToken cancellationToken = default)
        {
            if (appUser == null) throw new ArgumentNullException(nameof(appUser));
            appUser.UtcCreatedOn = new DateTime(appUser.UtcCreatedOn.Ticks, DateTimeKind.Utc);
            appUser.UtcUpdatedOn = DateTime.UtcNow;
            appUser.UpdatedBy = appUser.Email;
            appUser.Version++;
            _context.AppUsers.Update(appUser);
            await _context.SaveChangesAsync(cancellationToken);
            return appUser;
        }

        /// <summary>
        /// Creates a role.
        /// </summary>
        /// <param name="name">The name of the role.</param>
        /// <param name="description">The description of the role.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created role entity.</returns>
        public async Task<Role> CreateRoleAsync(string name, string description, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name is required.", nameof(name));

            var role = new Role { Name = name, Description = description };
            await _context.Roles.AddAsync(role, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return role;
        }

        /// <summary>
        /// Updates a role.
        /// </summary>
        /// <param name="role">The updated role entity.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The updated role entity.</returns>
        public async Task<Role> UpdateRoleAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            _context.Roles.Update(role);
            await _context.SaveChangesAsync(cancellationToken);
            return role;
        }

        /// <summary>
        /// Deletes a role.
        /// </summary>
        /// <param name="roleId">The ID of the role to delete.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteRoleAsync(ulong roleId, CancellationToken cancellationToken = default)
        {
            var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);
            if (role == null) throw new InvalidOperationException($"Role with ID '{roleId}' not found.");

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync(cancellationToken);
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
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);
            if (user == null) throw new InvalidOperationException($"User with email '{userEmail}' not found.");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
            if (role == null) throw new InvalidOperationException($"Role '{roleName}' not found.");

            var userRole = new AppUserRole { AppUserId = user.Id, RoleId = role.Id };
            await _context.AppUserRoles.AddAsync(userRole, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
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
            var userRole = await _context.AppUserRoles
                .Include(aur => aur.User)
                .Include(aur => aur.Role)
                .FirstOrDefaultAsync(aur => aur.User.Email == userEmail && aur.Role.Name == roleName, cancellationToken);

            if (userRole == null) throw new InvalidOperationException($"No relationship found between user '{userEmail}' and role '{roleName}'.");

            _context.AppUserRoles.Remove(userRole);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a user by their email.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The user entity.</returns>
        public async Task<AppUser> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await TryGetUserByEmailAsync(email, cancellationToken);

            if (user == null) throw new InvalidOperationException($"User with email '{email}' not found.");

            return user;
        }

        /// <summary>
        /// Retrieves a user by their email. if no user is found returns null
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The user entity if found, otherwise returns null</returns>
        public async Task<AppUser> TryGetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _context.AppUsers
                .Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (user != null)
                user.UserRoles = await _context.AppUserRoles.Include(i => i.Role).Where(i => i.AppUserId == user.Id).ToListAsync(cancellationToken);

            return user;
        }

        /// <summary>
        /// Registers a new admin user by creating an account and assigning the Administrator role.
        /// </summary>
        /// <param name="appUser">The instance of the AppUser to be created.</param>
        /// <param name="accountType">The name of the <see cref="AccountType"/> for the user's account.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created AppUser entity.</returns>
        public async Task<AppUser> RegisterUserInAdminRoleAsync(AppUser appUser, AccountType accountType, CancellationToken cancellationToken = default)
        {
            if (appUser == null) throw new ArgumentNullException(nameof(appUser));
            if (string.IsNullOrWhiteSpace(appUser.Email)) throw new ArgumentException("Email is required for the AppUser.", nameof(appUser.Email));
            if (accountType == null) throw new ArgumentException("Account type name is required.", nameof(accountType));

            // Create a new account for the user
            var account = new Account
            {
                Name = appUser.Email,
                Owner = appUser.Email
            };

            account.AccountTypeId = accountType.Id;
            account.AccountType = accountType;

            await _context.Accounts.AddAsync(account, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Assign the account to the user
            appUser.AccountId = account.Id;
            appUser.Account = account;

            await _context.AppUsers.AddAsync(appUser, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Check if the Administrator role exists
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator", cancellationToken);
            if (adminRole == null)
            {
                // Create the Administrator role if it doesn't exist
                adminRole = new Role
                {
                    Name = "Administrator",
                    Description = "Administrator role with full permissions."
                };
                await _context.Roles.AddAsync(adminRole, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Assign the Administrator role to the user
            var userRole = new AppUserRole
            {
                AppUserId = appUser.Id,
                RoleId = adminRole.Id,
                User = appUser,
                Role = adminRole
            };
            await _context.AppUserRoles.AddAsync(userRole, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return appUser;
        }

        /// <summary>
        /// Retrieves a pre-authorized user by their email.
        /// </summary>
        /// <param name="email">The email of the pre-authorized user to retrieve.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The PreAuthorizedAppUser entity if found; otherwise, null.</returns>
        public async Task<PreAuthorizedAppUser> GetPreAuthorizedAppUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            return await _context.PreAuthorizedAppUsers
                .Include(p => p.AccountType) // Include AccountType for additional details
                .FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
        }

        /// <summary>
        /// Adds a login history record to the database asynchronously.
        /// </summary>
        /// <param name="history">
        /// The <see cref="AppUserLoginHistory"/> instance representing the login history to be added. 
        /// This parameter cannot be null.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// Defaults to <see cref="CancellationToken.None"/> if not specified.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains the <see cref="AppUserLoginHistory"/> instance that was added.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="history"/> parameter is null.
        /// </exception>
        public async Task<AppUserLoginHistory> AddLogHistoryAsync(AppUserLoginHistory history, CancellationToken cancellationToken = default)
        {
            if (history == null) throw new ArgumentNullException(nameof(history));

            await _context.AppUserLoginHistories.AddAsync(history, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return history;
        }

        /// <summary>
        /// Adds a login history record to the database asynchronously using device information and user email.
        /// </summary>
        /// <param name="deviceInfo">
        /// The <see cref="DeviceInfo"/> instance representing the device details (e.g., browser, OS, IP address).
        /// This parameter cannot be null.
        /// </param>
        /// <param name="email">
        /// The email address of the user. This parameter cannot be null or whitespace.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// Defaults to <see cref="CancellationToken.None"/> if not specified.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the <see cref="AppUserLoginHistory"/> instance that was added.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="deviceInfo"/> parameter is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="email"/> parameter is null or contains only whitespace.
        /// </exception>
        public async Task<AppUserLoginHistory> AddLogHistoryAsync(DeviceInfo deviceInfo, string email, CancellationToken cancellationToken = default)
        {
            if (deviceInfo == null) throw new ArgumentNullException(nameof(deviceInfo));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException(nameof(email));

            var history = await AddLogHistoryAsync(new AppUserLoginHistory()
            {
                Email = email,
                Browser = deviceInfo.Browser,
                IpAddress = deviceInfo.IpAddress,
                OS = deviceInfo.OS,
                UtcLogIn = DateTime.UtcNow
            }, cancellationToken);

            return history;
        }



        /// <summary>
        /// Handles the authorization process for a user based on their identity, associated claims, and other contextual information.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> representing the user's identity. This parameter cannot be null.
        /// </param>
        /// <param name="additionalValidation">
        /// An optional action for performing additional validation or customization of the <see cref="AppUser"/> and the <see cref="ClaimsIdentity"/>.
        /// This parameter allows injecting custom logic to validate or modify the user or claims.
        /// </param>
        /// <param name="deviceInfo">
        /// Optional <see cref="DeviceInfo"/> containing information about the device initiating the login request, such as IP address and browser details.
        /// Defaults to <c>null</c>.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete. Defaults to <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the authenticated <see cref="AppUser"/> entity.
        /// </returns>
        /// <exception cref="AuthMateException">
        /// Thrown when:
        /// <list type="bullet">
        /// <item><description>The <paramref name="identity"/> object is null or invalid.</description></item>
        /// <item><description>The user's email is missing or invalid.</description></item>
        /// <item><description>No user can be authenticated, pre-authorized, or found in invitations.</description></item>
        /// <item><description>Failed to register or authenticate a pre-authorized user.</description></item>
        /// </list>
        /// </exception>
        /// <remarks>
        /// This method performs a series of checks to authenticate and authorize a user:
        /// <list type="number">
        /// <item>Verifies that the <paramref name="identity"/> is valid and contains an email claim.</item>
        /// <item>Attempts to retrieve an existing user from the database using the user's email.</item>
        /// <item>If no user is found, checks for an invitation in the system associated with the user's email.</item>
        /// <item>If no invitation is found, checks the list of pre-authorized users to see if the user qualifies for a role.</item>
        /// <item>If the user is pre-authorized, creates a new account with the specified account type.</item>
        /// <item>Performs optional additional validation logic using the <paramref name="additionalValidation"/> action.</item>
        /// <item>Updates the user's login information, including device details, and returns the authenticated user entity.</item>
        /// </list>
        /// If all checks fail, an <see cref="AuthMateException"/> is thrown, indicating that the user could not be authenticated.
        /// </remarks>
        public async Task<AppUser> UserAuthorizationProcessAsync(ClaimsIdentity identity, Action<AppUser, ClaimsIdentity> additionalValidation, DeviceInfo? deviceInfo = default, CancellationToken cancellationToken = default)
        {
            if (identity == null) throw new AuthMateException("Unable to retrive Identity object from the session context");

            //variables
            var preAuthorizedUser = default(PreAuthorizedAppUser);

            //Add information about the google provider
            identity.AddClaim(new Claim("AppUserProviderType", "Google"));
            //Creates an App user object
            var contextUser = identity.ToUser();

            if (string.IsNullOrWhiteSpace(contextUser.Email)) throw new AuthMateException("User does not have a valid email and it is required");

            var user = default(AppUser);

            //Tries to get the user from the database
            user = await TryGetUserByEmailAsync(contextUser.Email);

            //if a user is found then, we complete the transaction
            if (user != null)
                return await UpdateUserLoginInformationAsync(identity, user, deviceInfo, cancellationToken);

            //if no user is found we look in the invites
            user = await CheckForInvitationForUserAsync(identity, cancellationToken);

            //If a user is found in the invite list, then we complete the transaction
            if (user != null)
                return await UpdateUserLoginInformationAsync(identity, user, deviceInfo, cancellationToken);

            // if no user is found, we look in the preauthorized list
            preAuthorizedUser = await GetPreAuthorizedAppUserByEmailAsync(contextUser.Email, cancellationToken);

            //If there is no user and nothing on the super user list, then throw an exception
            if (user == null && preAuthorizedUser == null) throw new AuthMateException($"Unable to authenticate user {contextUser.Email}");

            //If the user is null and it is a power user it creates a new account
            if(preAuthorizedUser != null)
                user = await RegisterUserInAdminRoleAsync(contextUser, preAuthorizedUser.AccountType);

            //If the RegisterUserInAdminRoleAsync method returns null then we cannot continue
            if (user == null) throw new AuthMateException($"Unable to authenticate user {contextUser.Email}");

            if (additionalValidation != null)
                additionalValidation(user, identity); //performs a user additional validation on the claims identity and the application user

            return await UpdateUserLoginInformationAsync(identity, user, deviceInfo, cancellationToken);
        }

        private async Task<AppUser> UpdateUserLoginInformationAsync(ClaimsIdentity identity, AppUser user, DeviceInfo deviceInfo, CancellationToken cancellationToken = default)
        {
            //Adds the user complete data to a claim
            identity.AddClaim(new Claim("AppUserJson", user.ToString()));
            //Updates the user last login value
            user.UtcLastLogin = DateTime.UtcNow;
            await UpdateAppUserAsync(user, cancellationToken);

            if (deviceInfo != null)
                //Add an entry to the login history table
                await AddLogHistoryAsync(deviceInfo, user.Email, cancellationToken);

            return user;
        }

        /// <summary>
        /// Checks for an invitation for a user based on their identity and, if found, creates a new user account associated with the invitation.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> representing the identity of the user. This parameter cannot be null.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// Defaults to <see cref="CancellationToken.None"/> if not specified.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the <see cref="AppUser"/> instance if the invitation exists and the user is created;
        /// otherwise, <c>null</c> if no invitation is found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="identity"/> parameter is null.
        /// </exception>
        public async Task<AppUser> CheckForInvitationForUserAsync(ClaimsIdentity identity, CancellationToken cancellationToken = default)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            var user = identity.ToUser();

            var invitation = await _context.AccountInvites.Include(i => i.Account).Include(i => i.Role)
                .FirstOrDefaultAsync(i => i.Email == user.Email);

            // If there is no invitation, return null
            if (invitation == null) return null;

            // Create user account
            user.AccountId = invitation.AccountId;
            user.Account = invitation.Account;
            user.CreatedBy = user.Email;
            user.UpdatedBy = user.Email;
            user.UtcCreatedOn = DateTime.UtcNow;
            user.UtcUpdatedOn = user.UtcCreatedOn;

            await _context.AppUsers.AddAsync(user, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            var userRole = new AppUserRole()
            {
                AppUserId = user.Id,
                RoleId = invitation.RoleId,
                User = user,
                Role = invitation.Role
            };

            await _context.AppUserRoles.AddAsync(userRole, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            user.UserRoles.Add(userRole);

            return user;
        }






    }


}
