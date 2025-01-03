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

        /// <summary>
        /// Gets the timezone of the current web user.
        /// </summary>
        /// <returns>
        /// The timezone of the current web user as a <see cref="TimeZoneInfo"/> object.
        /// If the user is not authenticated or the timezone is not set, returns "Central Standard Time".
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown when the HttpContext is null.</exception>
        TimeZoneInfo GetUserTimezone();

        /// <summary>
        /// Converts the specified UTC date and time to the current web user's local date and time.
        /// </summary>
        /// <param name="dateTime">The UTC date and time to convert.</param>
        /// <returns>The converted date and time in the user's local timezone.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the HttpContext is null.</exception>
        DateTime ConvertToUserDateTime(DateTime dateTime);
        
    }
}