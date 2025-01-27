using Luval.AuthMate.Core;
using Luval.AuthMate.Infrastructure.Configuration;
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

            //TODO: Add config settings, see: https://github.com/marinoscar/AuthMate?tab=readme-ov-file#appsettingsjson

            //TODO: Add the configuration secret, see: https://github.com/marinoscar/AuthMate?tab=readme-ov-file#appsettingsjson

            var config = builder.Configuration;

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddFluentUIComponents();

            //Add logging services is a required dependency for AuthMate
            builder.Services.AddLogging();

            //Add the controllers and the http client and context accessor
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();

            //Add the AuthMate services
            builder.Services.AddAllAuthMateServices();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            //Add the authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            //Map the controllers and the razor components
            app.MapControllers();

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
            contextHelper.InitializeDbAsync(config["AuthMate:AppOwnerEmail"] ?? "")
                .GetAwaiter()
                .GetResult();


            app.Run();
        }
    }
}
