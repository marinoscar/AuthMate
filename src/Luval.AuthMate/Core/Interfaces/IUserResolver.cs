using Luval.AuthMate.Core.Entities;

namespace Luval.AuthMate.Core.Interfaces
{
    /// <summary>
    /// Resolves the username and email of the current user.
    /// </summary>
    public interface IUserResolver
    {
        /// <summary>
        /// Gets the username of the current web user.
        /// </summary>
        /// <returns>The username of the current web user, or "Anonymous" if the user is not authenticated.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the HttpContext is null.</exception>
        string GetUserEmail();
        /// <summary>
        /// Gets the email of the current web user.
        /// </summary>
        /// <returns>The email of the current web user, or "Anonymous" if the user is not authenticated or the email claim is not present.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the HttpContext is null.</exception>
        string GetUserName();

        /// <summary>
        /// Gets the current web user as an <see cref="AppUser"/> object.
        /// </summary>
        /// <returns>The current web user as an <see cref="AppUser"/> object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the HttpContext is null.</exception>
        AppUser GetUser();
    }
}