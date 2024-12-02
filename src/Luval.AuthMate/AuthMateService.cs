using Luval.AuthMate.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
        /// Handles the authorization process for a user based on their identity and associated claims.
        /// </summary>
        /// <param name="identity">The claims identity of the user attempting to authenticate.</param>
        /// <param name="additionalValidation">An optional action for performing additional validation or customization of the user and claims identity.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>The authenticated <see cref="AppUser"/> entity.</returns>
        /// <exception cref="AuthMateException">
        /// Thrown when:
        /// <list type="bullet">
        /// <item><description>The identity object is null or invalid.</description></item>
        /// <item><description>The user's email is not valid or missing.</description></item>
        /// <item><description>The user cannot be authenticated or pre-authorized.</description></item>
        /// </list>
        /// </exception>
        /// <remarks>
        public async Task<AppUser> UserAuthorizationProcessAsync(ClaimsIdentity identity, Action<AppUser, ClaimsIdentity> additionalValidation, CancellationToken cancellationToken)
        {
            if (identity == null) throw new AuthMateException("Unable to retrive Identity object from the session context");

            //Add information about the google provider
            identity.AddClaim(new Claim("AppUserProviderType", "Google"));
            //Creates an App user object
            var contextUser = identity.ToUser();

            if (string.IsNullOrWhiteSpace(contextUser.Email)) throw new AuthMateException("User does not have a valid email and it is required");

            var user = default(AppUser);

            //Tries to get the user from the database
            user = await TryGetUserByEmailAsync(contextUser.Email);

            var preAuthorizedUser = default(PreAuthorizedAppUser);

            if (user == null)
                preAuthorizedUser = await GetPreAuthorizedAppUserByEmailAsync(contextUser.Email, cancellationToken);

            if(preAuthorizedUser == null) throw new AuthMateException($"Unable to authenticate user {contextUser.Email}");



            //If the user is null and it is a power user it creates a new account
             user = await RegisterUserInAdminRoleAsync(contextUser,
                    preAuthorizedUser.AccountType);

            if (user == null) throw new AuthMateException($"Unable to authenticate user {contextUser.Email}");

            //Adds the user complete data to a claim
            identity.AddClaim(new Claim("AppUserJson", user.ToString()));

            if (additionalValidation != null) 
                additionalValidation(user, identity); //performs a user additional validation on the claims identity and the application user

            return user;
        }



    }


}
