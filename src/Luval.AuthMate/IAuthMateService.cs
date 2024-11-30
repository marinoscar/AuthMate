using Luval.AuthMate.Entities;

namespace Luval.AuthMate
{
    /// <summary>
    /// Defines methods for managing accounts, roles, and users in the authentication service.
    /// </summary>
    public interface IAuthMateService
    {
        /// <summary>
        /// Adds a user to an account if they are not already associated with it.
        /// </summary>
        /// <param name="user">The user to add to the account.</param>
        /// <param name="ownerEmail">The email of the account owner.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AppUserInAccount"/> entity.</returns>
        Task<AppUserInAccount> AddUserToAccountAsync(AppUser user, string ownerEmail, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a user to a specific role if they are not already assigned to it.
        /// </summary>
        /// <param name="userEmail">The email of the user to assign the role.</param>
        /// <param name="roleName">The name of the role to assign.</param>
        /// <param name="ownerEmail">The email of the account owner performing the action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AppUserRole"/> entity.</returns>
        Task<AppUserRole> AddUserToRoleAsync(string userEmail, string roleName, string ownerEmail, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new account for the specified user if it does not exist.
        /// </summary>
        /// <param name="user">The user for whom the account is created.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Account"/> entity.</returns>
        Task<Account> CreateAccountAsync(AppUser user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new account type if it does not exist.
        /// </summary>
        /// <param name="name">The name of the account type.</param>
        /// <param name="user">The user creating the account type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AccountType"/> entity.</returns>
        Task<AccountType> CreateAccountTypeAsync(string name, AppUser user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user as an administrator.
        /// </summary>
        /// <param name="newUser">The new user to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AppUser"/> entity.</returns>
        Task<AppUser> CreateUserAsAdminAsync(AppUser newUser, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new role if it does not exist.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="description">The description of the role.</param>
        /// <param name="user">The user creating the role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Role"/> entity.</returns>
        Task<Role> CreatRoleAsync(string roleName, string description, AppUser user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a role based on its name.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if the role does not exist.</exception>
        Task DeleteRoleAsync(string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an account based on the owner's email.
        /// </summary>
        /// <param name="owner">The owner's email.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Account"/> entity.</returns>
        Task<Account> GetAccountAsync(string owner, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a role based on its name.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Role"/> entity.</returns>
        Task<Role> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AppUser"/> entity.</returns>
        Task<AppUser> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    }

}