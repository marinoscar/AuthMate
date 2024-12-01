# AuthMate: Simplify Authentication and Authorization for Your Apps

AuthMate is a powerful yet easy-to-use NuGet package designed to streamline user authentication and authorization for web and mobile applications. Whether you're building an app from scratch or integrating advanced user management features, AuthMate has you covered.

## Key Features
* Social Login Integration: Quickly enable login via Google, Microsoft, and Facebook with minimal setup.
* SQL Backend Support: Manage users, roles, and permissions seamlessly using your existing SQL database.
* Account Management:
** Assign users to one or multiple accounts.
** Support for account types to accommodate different use cases.
** Role-Based Access Control (RBAC): Define roles and permissions for users at the account level.
** Developer-Friendly: Designed for quick implementation and scalability, saving you hours of development time.

## Configuring the Sql Database
The project uses EntityFramework, if you need to implement the database storage for any database engine here is an example, just extend the `AuthMateContext` class here is an example

``` csharp
public class PostgresAuthMateContext : AuthMateContext, IAuthMateContext
{
    private string _connString;

    /// <summary>
    /// Creates a new instance of the context
    /// </summary>
    /// <param name="connectionString">A valid postgres connection string</param>
    public PostgresAuthMateContext(string connectionString)
    {
            _connString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connString);
        if(Debugger.IsAttached)
            optionsBuilder.LogTo(Console.WriteLine);
    }
}
```
## Getting Google Client Id and Client Secret
To configure the OAuth you need to get the information from google follow the steps in this article https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-8.0

## Configuring your application
Here is an example code of how to configure your application to use Google Authentication

``` csharp
public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();
    // Add controllers
    builder.Services.AddControllers();
    builder.Services.AddHttpClient();
    builder.Services.AddHttpContextAccessor();
    // Configure AuthMate
    var dbContext = new PostgresAuthMateContext(ConfigHelper.GetValueAsString("ConnectionString:Authorization"));
    var authService = new AuthMateService(
            dbContext,
            "Free", "Administrator"
        );
    // Function to be called after the user is authorized by Google
    Func<OAuthCreatingTicketContext, Task> onTicket = async contex =>
    {
        await authService.OnUserAuthorizedAsync(contex.Identity, "Google", null);
    };
    // Add Google Authentication configuration
    builder.Services.AddGoogleAuth(new GoogleOAuthConfiguration()
    {
        // client id from your config file
        ClientId = ConfigHelper.GetValueAsString("Authentication:Google:ClientID"),
        // the client secret from your config file
        ClientSecret = ConfigHelper.GetValueAsString("Authentication:Google:ClientSecret"),
        OnCreatingTicket = onTicket // function call
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
    /*** Adds support for controllers     ****/
    app.MapControllers();
    app.UseRouting();
    app.UseAuthorization();
    app.UseAuthentication();
    /*** End code to suupport controllers ****/
    app.UseStaticFiles();
    app.UseAntiforgery();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();
    app.Run();
}
```
## Create a Controller in your Web Application
In order to handle the authentication request you will need to create a controller. here is a simple working code

``` csharp
[Route("/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("google-login")] //This mathches the configuration of the Google Auth
    public IActionResult GoogleLogin()
    {

        // Adds the properties ad redirect information
        // this could be change to include a redirect as part
        // of a query string if required
        var prop = new AuthenticationProperties()
        {
            RedirectUri = "/"
        };

        // Creates tthe challange
        var challange = Challenge(prop, GoogleDefaults.AuthenticationScheme);

        return challange;
    }

    [AllowAnonymous]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        // Signout the user from the OAuth flow
        await HttpContext.SignOutAsync();

        // Redirect to root so that when logging back in, it takes to home page
        return Redirect("/");
    }

}
```
