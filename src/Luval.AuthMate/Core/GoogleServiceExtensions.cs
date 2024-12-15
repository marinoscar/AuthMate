using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;

namespace Luval.AuthMate.Core
{
    public static class GoogleServiceExtensions
    {

        public static IServiceCollection AddAuthMateGoogleAuth(this IServiceCollection s, Func<OAuthCreatingTicketContext, Task> onCreatingTicket)
        {
            return s.AddAuthMateGoogleAuth(new GoogleOAuthConfiguration() { OnCreatingTicket = onCreatingTicket });
        }

        public static IServiceCollection AddAuthMateGoogleAuth(this IServiceCollection s, GoogleOAuthConfiguration config)
        {

            s.AddAuthentication("Cookies")

                .AddCookie(opt =>
                {
                    // This will be the name of the cookie and the path the authentication flow
                    // will use to create the challange
                    opt.Cookie.Name = config.CookieName;
                    opt.LoginPath = config.LoginPath;

                }).AddGoogle(opt =>
                {
                    //Google credentials for OAuth
                    opt.ClientId = config.ClientId;
                    opt.ClientSecret = config.ClientSecret;

                    //Maps the information coming from Google into the correct claims
                    opt.ClaimActions.MapJsonKey("urn:google:profile", "link");
                    opt.ClaimActions.MapJsonKey("urn:google:image", "picture");

                    //This is the route that will redirected to after the challenge, you need to make sure
                    //it is also configured in the Google console under the API Credentials
                    //https://console.cloud.google.com/apis/credentials
                    //For the Linux environment there is an issue around the path not using https by default
                    //and using http that causes the path not to match because Google always expect https, in this
                    //guide there is more information on how to solve the issue, but here is a reference to the problem
                    //https://github.com/googleapis/google-api-dotnet-client/issues/1899#issue-951145621
                    opt.CallbackPath = config.CallbackPath;

                    //Event that is triggered after the use is authenticated
                    if(config.OnCreatingTicket == null)
                    {
                        config.OnCreatingTicket = async context =>
                        {
                            var authService = s.BuildServiceProvider().GetRequiredService<AuthMate.Core.Services.AuthenticationService>();

                            await authService.AuthorizeUserAsync(context.Identity, TokenResponse.Create(context.TokenResponse), DeviceInfo.Create(context.Properties), CancellationToken.None);


                        };
                    }
                    opt.Events.OnCreatingTicket = config.OnCreatingTicket;
                });
            return s;
        }
    }
}
