namespace Luval.AuthMate.Infrastructure.Configuration
{
    /// <summary>
    /// Represents the configuration for Google OAuth, extending the base <see cref="OAuthConfiguration"/> class by setting the cookie name, login path, and callback path, with their respective default values.
    /// </summary>
    public class GoogleOAuthConfiguration : OAuthConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleOAuthConfiguration"/> class.
        /// </summary>
        public GoogleOAuthConfiguration()
        {
            CookieName = "GoogleOAuth";
            LoginPath = "/auth/login";
            CallbackPath = "/signin-google";
        }
    }
}
