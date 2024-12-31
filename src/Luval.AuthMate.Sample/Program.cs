using Luval.AuthMate.Sample.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Luval.AuthMate.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //TODO: Add config settings for the the Client ID and ClientSecret in the appsettings.json file and secrets.json

            var builder = WebApplication.CreateBuilder(args);

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

            app.Run();
        }
    }
}
