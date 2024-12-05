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
        Task<AppUser> RegisterUserInAdminRoleAsync(AppUser appUser, AccountType accountType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a pre-authorized user by their email.
        /// </summary>
        /// <param name="email">The email of the pre-authorized user to retrieve.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The PreAuthorizedAppUser entity if found; otherwise, null.</returns>
        Task<InviteToApplication> GetPreAuthorizedAppUserByEmailAsync(string email, CancellationToken cancellationToken = default);

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
        Task<AppUser> UserAuthorizationProcessAsync(ClaimsIdentity identity, Action<AppUser, ClaimsIdentity> additionalValidation, DeviceInfo? deviceInfo = default, CancellationToken cancellationToken = default);

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
        Task<AppUserLoginHistory> AddLogHistoryAsync(DeviceInfo deviceInfo, string email, CancellationToken cancellationToken = default);


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
        Task<AppUserLoginHistory> AddLogHistoryAsync(AppUserLoginHistory history, CancellationToken cancellationToken = default);

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
        Task<AppUser> CheckForInvitationForUserAsync(ClaimsIdentity identity, CancellationToken cancellationToken = default);
    }

}