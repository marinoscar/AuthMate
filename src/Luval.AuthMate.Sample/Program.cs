using Luval.AuthMate.Core;
using Luval.AuthMate.Infrastructure.Data;
using Luval.AuthMate.Infrastructure.Logging;
using Luval.AuthMate.Sample.Components;
using Luval.AuthMate.Sqlite;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Luval.AuthMate.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //TODO: Add config settings
            /*
            "OAuthProviders": {
            "Google": {
                "ClientID": "your-client-id",
                "OwnerEmail": "your-email"
                "AuthorizationEndpoint": "https://accounts.google.com/o/oauth2/v2/auth",
                "TokenEndpoint": "https://oauth2.googleapis.com/token",
                "UserInfoEndpoint": "https://www.googleapis.com/oauth2/v3/userinfo",
                "CodeFlowRedirectUri": "",
                "Scopes": "https://www.googleapis.com/auth/gmail.readonly https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile"
            }
            */
            //TODO: Add the configuration secret
            /*
             "OAuthProviders:Google:ClientSecret": "your-secret"
             "AuthMate:BearingTokenKey": "your-key"
             */
            var config = builder.Configuration;

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddFluentUIComponents();

            //Add logging services
            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

            //Add the controllers and the http client and context accessor
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();

            //Add the AuthMate services
            builder.Services.AddAuthMateServices(
                //The key to use for the bearing token implementation
                config["AuthMate:BearingTokenKey"],
                (s) => {
                    //returns a local instance of Sqlite
                    //replace this with your own implementation of Postgres, MySql, SqlServer, etc
                    return new SqliteAuthMateContext();
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            //Map the controllers and the razor components
            app.MapControllers();
            app.UseRouting();
            //Add the authentication and authorization
            app.UseAuthorization();
            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            //Inialize the app database using Sqlite
            var contextHelper = new AuthMateContextHelper(
                new SqliteAuthMateContext(),
                new ColorConsoleLogger<AuthMateContextHelper>());
            //Makes sure the db is created, then initializes the db with the owner email
            //and required initial records
            contextHelper.InitializeDbAsync(config["OAuthProviders:Google:OwnerEmail"] ?? "")
                .GetAwaiter()
                .GetResult();


            app.Run();
        }
    }
}
