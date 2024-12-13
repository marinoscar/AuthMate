using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
