using Luval.AuthMate.Entities;
using System.Security.Claims;

namespace Luval.AuthMate
{

    /// <summary>
    /// Interface for managing authentication-related operations in the system.
    /// </summary>
    public interface IAuthMateService
    {
        /// <summary>
        /// Creates a new account type.
        /// </summary>
        /// <param name="name">The name of the account type.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created account type entity.</returns>
        Task<AccountType> CreateAccountTypeAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new account.
        /// </summary>
        /// <param name="name">The name of the account.</param>
        /// <param name="accountTypeName">The name of the account type.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created account entity.</returns>
        Task<Account> CreateAccountAsync(string name, string accountTypeName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new app user and associates it with an existing account.
        /// </summary>
        /// <param name="appUser">The app user entity to be created.</param>
        /// <param name="owner">The owner to retrieve the account for association.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created app user entity.</returns>
        Task<AppUser> CreateAppUserAsync(AppUser appUser, string owner, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an app user.
        /// </summary>
        /// <param name="appUser">The updated app user entity.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The updated app user entity.</returns>
        Task<AppUser> UpdateAppUserAsync(AppUser appUser, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a role.
        /// </summary>
        /// <param name="name">The name of the role.</param>
        /// <param name="description">The description of the role.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created role entity.</returns>
        Task<Role> CreateRoleAsync(string name, string description, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a role.
        /// </summary>
        /// <param name="role">The updated role entity.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The updated role entity.</returns>
        Task<Role> UpdateRoleAsync(Role role, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a role.
        /// </summary>
        /// <param name="roleId">The ID of the role to delete.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteRoleAsync(ulong roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a user to a role.
        /// </summary>
        /// <param name="userEmail">The email of the user.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddUserToRoleAsync(string userEmail, string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a user from a role.
        /// </summary>
        /// <param name="userEmail">The email of the user.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveUserFromRoleAsync(string userEmail, string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a user by their email.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The user entity.</returns>
        Task<AppUser> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a user by their email. if no user is found returns null
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The user entity if found, otherwise returns null</returns>
        Task<AppUser> TryGetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Registers a new admin user by creating an account and assigning the Administrator role.
        /// </summary>
        /// <param name="appUser">The instance of the AppUser to be created.</param>
        /// <param name="accountType">The name of the <see cref="AccountType"/> for the user's account.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created AppUser entity.</returns>
        Task<AppUser> RegisterUserInAdminRoleAsync(AppUser appUser, AccountType accountType, CancellationToken cancellationToken = default)

        /// <summary>
        /// Retrieves a pre-authorized user by their email.
        /// </summary>
        /// <param name="email">The email of the pre-authorized user to retrieve.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The PreAuthorizedAppUser entity if found; otherwise, null.</returns>
        Task<PreAuthorizedAppUser> GetPreAuthorizedAppUserByEmailAsync(string email, CancellationToken cancellationToken = default);

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
        Task<AppUser> UserAuthorizationProcessAsync(ClaimsIdentity identity, Action<AppUser, ClaimsIdentity> additionalValidation, CancellationToken cancellationToken)
    }

}