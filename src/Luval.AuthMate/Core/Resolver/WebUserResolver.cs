using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Luval.AuthMate.Core.Resolver
{
    /// <summary>
    /// Resolves the username and email of the current web user.
    /// </summary>
    public class WebUserResolver : IUserResolver
    {
        private IHttpContextAccessor _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebUserResolver"/> class.
        /// </summary>
        /// <param name="context">The HTTP context accessor.</param>
        /// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
        public WebUserResolver(IHttpContextAccessor context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets the username of the current web user.
        /// </summary>
        /// <returns>The username of the current web user, or "Anonymous" if the user is not authenticated.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the HttpContext is null.</exception>
        public string GetUserName()
        {
            if (_context.HttpContext == null)
                throw new InvalidOperationException("HttpContext is null");

            var user = _context.HttpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return "Anonymous";
            return user.Identity.Name ?? "Anonymous";
        }

        /// <summary>
        /// Gets the email of the current web user.
        /// </summary>
        /// <returns>The email of the current web user, or "Anonymous" if the user is not authenticated or the email claim is not present.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the HttpContext is null.</exception>
        public string GetUserEmail()
        {
            if (_context.HttpContext == null)
                throw new InvalidOperationException("HttpContext is null");

            var user = _context.HttpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return "Anonymous";
            return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "Anonymous";
        }

        /// <summary>
        /// Gets the current web user as an <see cref="AppUser"/> object.
        /// </summary>
        /// <returns>The current web user as an <see cref="AppUser"/> object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the HttpContext is null.</exception>
        public AppUser GetUser()
        {
            if (_context.HttpContext == null)
                throw new InvalidOperationException("HttpContext is null");

            return _context.HttpContext.User.ToUser();
        }

        /// <summary>
        /// Gets the timezone of the current web user.
        /// </summary>
        /// <returns>
        /// The timezone of the current web user as a <see cref="TimeZoneInfo"/> object.
        /// If the user is not authenticated or the timezone is not set, returns "Central Standard Time".
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown when the HttpContext is null.</exception>
        public TimeZoneInfo GetUserTimezone()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            var user = GetUser();
            if (user == null) return tz;
            if (user.Timezone == null) return tz;
            return TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
        }

        /// <summary>
        /// Converts the specified UTC date and time to the current web user's local date and time.
        /// </summary>
        /// <param name="dateTime">The UTC date and time to convert.</param>
        /// <returns>The converted date and time in the user's local timezone.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the HttpContext is null.</exception>
        public DateTime ConvertToUserDateTime(DateTime dateTime)
        {
            var tz = GetUserTimezone();
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, tz);
        }
    }
}
