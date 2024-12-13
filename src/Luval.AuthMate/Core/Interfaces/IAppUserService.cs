using Luval.AuthMate.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core.Interfaces
{
    /// <summary>
    /// Interface for managing app user-related operations.
    /// </summary>
    public interface IAppUserService
    {
        /// <summary>
        /// Retrieves a user by their email.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The user entity.</returns>
        Task<AppUser> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an app user.
        /// </summary>
        /// <param name="appUser">The updated app user entity.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The updated app user entity.</returns>
        Task<AppUser> UpdateAppUserAsync(AppUser appUser, CancellationToken cancellationToken = default);

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
        /// Retrieves a user by their email. If no user is found, returns null.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The user entity if found, otherwise returns null.</returns>
        Task<AppUser> TryGetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user based on an invitation and associates the user with the specified account and role.
        /// </summary>
        /// <param name="invite">The invitation containing account and role information.</param>
        /// <param name="identity">The identity of the user to be created.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created app user entity.</returns>
        Task<AppUser> CreateUserFromInvitationAsync(InviteToAccount invite, ClaimsIdentity identity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user from an invitation, assigns them an account, and grants the Administrator role.
        /// </summary>
        /// <param name="invite">The invitation containing account type information.</param>
        /// <param name="identity">The claims identity of the user being created.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created app user entity.</returns>
        Task<AppUser> CreateUserFromInvitationAsync(InviteToApplication invite, ClaimsIdentity identity, CancellationToken cancellationToken = default);
    }

}
