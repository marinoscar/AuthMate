# AuthMate: Simplify Authentication and Authorization for Your Apps

AuthMate is a comprehensive authentication and authorization system designed to manage user accounts, roles, and permissions within a Blazor application. It provides a robust and flexible framework for handling user authentication, including support for OAuth providers, user roles, and login history tracking.

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
To configure the Program.cs file for the AuthMate project, follow these steps:
1.	Add Required Services: Register the necessary services for the DbContext and authentication.
2.	Configure the DbContext: Set up the DbContext with the appropriate connection string.
3.	Configure Authentication: Set up authentication schemes and options.
Here is an example configuration for Program.cs:

``` csharp
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure DbContext
builder.Services.AddDbContext<IAuthMateContext, AuthMateContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthMateConnection")));

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

// Add other necessary services
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

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
# Database Information
The database has been implemented in postgres but can be very easily implemented in any other database engine that supports Entity framework, all you need to do is to extend this class [AuthMateContext.cs](https://github.com/marinoscar/AuthMate/blob/main/src/Luval.AuthMate/Infrastructure/Data/AuthMateContext.cs)

## ERD Model
![ERD Model](https://raw.githubusercontent.com/marinoscar/AuthMate/refs/heads/main/media/erd.svg)

---

### Sample Data
- Default account type: `Free`
- Predefined roles: `Administrator`, `Owner`, `Member`, `Visitor`.
- Pre-authorized user: `oscar.marin.saenz@gmail.com` (associated with the "Free" account type).
