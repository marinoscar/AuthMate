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
    /// Provides methods to manage accounts, roles, and users in an authentication service.
    /// </summary>
    public class AuthMateService : IAuthMateService
    {
        private readonly IAuthMateContext _authMateContext;
        private readonly string _defaultAccountType;
        private readonly string _defaultAdministratorRole;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthMateService"/> class.
        /// </summary>
        /// <param name="authMateContext">The database context for AuthMate.</param>
        /// <param name="defaultAcountTypeName">The default account type name.</param>
        /// <param name="defaultAdministratorRole">The default administrator role name.</param>
        public AuthMateService(IAuthMateContext authMateContext, string defaultAcountTypeName, string defaultAdministratorRole)
        {
            _authMateContext = authMateContext;
            _defaultAccountType = defaultAcountTypeName;
            _defaultAdministratorRole = defaultAdministratorRole;
        }

        #region Account Management

        /// <summary>
        /// Creates a new account type if it does not exist.
        /// </summary>
        /// <param name="name">The name of the account type.</param>
        /// <param name="user">The user creating the account type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created or existing account type.</returns>
        public async Task<AccountType> CreateAccountTypeAsync(string name, AppUser user, CancellationToken cancellationToken = default)
        {
            var accountType = await _authMateContext.AccountTypes.FirstOrDefaultAsync(x => x.Name == name);
            if (accountType == null)
            {
                accountType = _authMateContext.AccountTypes.Add(new AccountType()
                { Name = name, Version = 1, CreatedBy = user.Email, UpdatedBy = user.Email }).Entity;
                await _authMateContext.SaveChangesAsync(cancellationToken);
            }

            return accountType;
        }

        /// <summary>
        /// Creates a new account for the specified user if it does not exist.
        /// </summary>
        /// <param name="user">The user for whom the account is created.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created or existing account.</returns>
        public async Task<Account> CreateAccountAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            var account = await GetAccountAsync(user.Email, cancellationToken);
            if (account == null)
            {
                account = _authMateContext.Accounts.Add(new Account()
                {
                    Owner = user.Email,
                    CreatedBy = user.Email,
                    UpdatedBy = user.Email
                }).Entity;
                await _authMateContext.SaveChangesAsync(cancellationToken);
            }
            return account;
        }

        /// <summary>
        /// Retrieves an account based on the owner's email.
        /// </summary>
        /// <param name="owner">The owner's email.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The account if found; otherwise, null.</returns>
        public async Task<Account> GetAccountAsync(string owner, CancellationToken cancellationToken = default)
        {
            var account = await _authMateContext.Accounts.SingleOrDefaultAsync(x => x.Owner == owner, cancellationToken);
            if (account != null)
            {
                var accountType = await _authMateContext.AccountTypes.SingleOrDefaultAsync(x => x.Id == account.AccountTypeId, cancellationToken);
                account.AccountType = accountType;
            }
            return account;
        }

        /// <summary>
        /// Adds a user to an account if they are not already associated with it.
        /// </summary>
        /// <param name="user">The user to add to the account.</param>
        /// <param name="ownerEmail">The email of the account owner.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="AppUserInAccount"/> entity representing the user's association with the account.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the account associated with the ownerEmail is not found.</exception>
        public async Task<AppUserInAccount> AddUserToAccountAsync(AppUser user, string ownerEmail, CancellationToken cancellationToken = default)
        {
            var account = await _authMateContext.Accounts.SingleAsync(x => x.Owner == ownerEmail, cancellationToken);
            var userInAccount = await _authMateContext.UserInAccounts.SingleOrDefaultAsync(x => x.AppUserId == user.Id && x.AccountId == account.Id, cancellationToken);

            if (userInAccount == null)
            {
                userInAccount = _authMateContext.UserInAccounts.Add(new AppUserInAccount()
                {
                    CreatedBy = ownerEmail,
                    UpdatedBy = ownerEmail,
                    AppUserId = user.Id,
                    AccountId = account.Id
                }).Entity;
                await _authMateContext.SaveChangesAsync(cancellationToken);
            }
            return userInAccount;
        }

        #endregion

        #region Role Management

        /// <summary>
        /// Creates a new role if it does not exist.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="description">The description of the role.</param>
        /// <param name="user">The user creating the role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created or existing role.</returns>
        public async Task<Role> CreatRoleAsync(string roleName, string description, AppUser user, CancellationToken cancellationToken = default)
        {
            var role = await GetRoleByNameAsync(roleName, cancellationToken);
            if (role == null)
            {
                _authMateContext.Roles.Add(new Role
                {
                    Name = roleName,
                    CreatedBy = user.Email,
                    Description = description,
                    UpdatedBy = user.Email
                });
                await _authMateContext.SaveChangesAsync(cancellationToken);
            }
            return role;
        }

        /// <summary>
        /// Retrieves a role based on its name.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The role if found; otherwise, null.</returns>
        public async Task<Role> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken) =>
            await _authMateContext.Roles.SingleOrDefaultAsync(x => x.Name == roleName, cancellationToken);

        /// <summary>
        /// Deletes a role based on its name.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentException">Thrown if the role does not exist.</exception>
        public async Task DeleteRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var role = await _authMateContext.Roles.SingleOrDefaultAsync(x => x.Name == roleName, cancellationToken);
            if (role == null) throw new ArgumentException("The provided role name is not in the database", nameof(roleName));
            _authMateContext.Roles.Remove(role);
            await _authMateContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Adds a user to a specific role if they are not already assigned to it.
        /// </summary>
        /// <param name="userEmail">The email of the user to assign the role.</param>
        /// <param name="roleName">The name of the role to assign.</param>
        /// <param name="ownerEmail">The email of the account owner performing the action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="AppUserRole"/> entity representing the user's role assignment.</returns>
        public async Task<AppUserRole> AddUserToRoleAsync(string userEmail, string roleName, string ownerEmail, CancellationToken cancellationToken = default)
        {
            var user = await GetUserByEmailAsync(userEmail, cancellationToken);
            var role = await GetRoleByNameAsync(roleName, cancellationToken);

            var userRole = await _authMateContext.UserRoles.SingleOrDefaultAsync(i => i.AppUserId == user.Id && i.RoleId == role.Id, cancellationToken);
            if (userRole == null)
            {
                userRole = _authMateContext.UserRoles.Add(new AppUserRole()
                {
                    AppUserId = user.Id,
                    RoleId = role.Id,
                    CreatedBy = ownerEmail,
                    UpdatedBy = ownerEmail
                }).Entity;

                await _authMateContext.SaveChangesAsync(cancellationToken);
            }
            return userRole;
        }


        #endregion

        #region User Management

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The user if found; otherwise, null.</returns>
        public async Task<AppUser> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _authMateContext.Users.SingleOrDefaultAsync(i => i.Email == email, cancellationToken);
            if (user == null) return null;
            user.UserRoles = _authMateContext.UserRoles.Where(i => i.AppUserId == user.Id).Include(i => i.Role).ToList();
            user.UserInAccounts = _authMateContext.UserInAccounts.Where(i => i.AppUserId == user.Id).Include(i => i.Account).ToList();

            return user;
        }

        /// <summary>
        /// Creates a new user as an administrator.
        /// </summary>
        /// <param name="newUser">The new user to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The created user.</returns>
        public async Task<AppUser> CreateUserAsAdminAsync(AppUser newUser, CancellationToken cancellationToken = default)
        {
            var account = await _authMateContext.Accounts.SingleOrDefaultAsync(x => x.Owner == newUser.Email, cancellationToken);
            if (account == null)
            {
                var accountType = await _authMateContext.AccountTypes.SingleOrDefaultAsync(x => x.Name == _defaultAccountType);
                account = _authMateContext.Accounts.Add(new Account()
                {
                    Owner = newUser.Email,
                    CreatedBy = newUser.Email,
                    UpdatedBy = newUser.Email,
                    AccountTypeId = accountType.Id,
                    AccountType = accountType,
                }).Entity;
            }
            var user = await _authMateContext.Users.SingleOrDefaultAsync(i => i.Email == newUser.Email, cancellationToken);
            if (user == null)
            {
                newUser.UtcCreatedOn = DateTime.UtcNow;
                newUser.UtcUpdatedOn = DateTime.UtcNow;
                newUser.CreatedBy = newUser.Email;
                newUser.UpdatedBy = newUser.Email;
                user = _authMateContext.Users.Add(newUser).Entity;
            }
            await _authMateContext.SaveChangesAsync();

            await AddUserToRoleAsync(user.Email, _defaultAdministratorRole, user.Email, cancellationToken);
            await AddUserToAccountAsync(user, user.Email);

            return newUser;
        }

        public async Task OnUserAuthorizedAsync(ClaimsIdentity contextUser, string providerType, Func<ClaimsIdentity> authorizeUser)
        {
            var user = default(AppUser);

            if(contextUser.HasClaim(i => i.Type == "AppUserJson"))
            {
                user = JsonSerializer.Deserialize<AppUser>(contextUser.GetValue("AppUserJson"));
                user.UpdateClaims(contextUser, providerType);
                return;
            }

            user = await GetUserByEmailAsync(contextUser.GetValue(ClaimTypes.Email));
            if (user == null)
            {
                user = await CreateUserAsAdminAsync(contextUser.ToUser());
                user.UpdateClaims(contextUser, providerType);
            }
            else
                user.UpdateClaims(contextUser, providerType);

            if (authorizeUser != null) authorizeUser();
            return;

        }

        #endregion
    }

}
