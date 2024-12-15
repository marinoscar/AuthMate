namespace Luval.AuthMate.Infrastructure.Configuration
{
    public class GoogleOAuthConfiguration : OAuthConfiguration
    {
        public GoogleOAuthConfiguration()
        {
            CookieName = "GoogleOAuth";
            LoginPath = "/auth/google-login";
            CallbackPath = "/signin-google";
        }
    }
}
