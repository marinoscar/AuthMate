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
        private HttpContextAccessor _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebUserResolver"/> class.
        /// </summary>
        /// <param name="context">The HTTP context accessor.</param>
        /// <exception cref="ArgumentNullException">Thrown when the context is null.</exception>
        public WebUserResolver(HttpContextAccessor context)
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
    }
}
