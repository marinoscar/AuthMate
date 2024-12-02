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
builder.Services.AddFluentUIComponents();

// AuthMate: Add support for controllers
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// AuthMate: Configure the database implementation
var dbContext = 
    new PostgresAuthMateContext(ConfigHelper.GetValueAsString("ConnectionString:Authorization")); //postgres implementaion

// AuthMate: Creates an instance of the service
var authService = new AuthMateService(
        dbContext //provides the database context
    );

// AuthMate: Function to be called after the user is authorized by Google
Func<OAuthCreatingTicketContext, Task> onTicket = async context =>
{

    //Checks for the user in the database and performs other validations, see the implementation here
    //https://github.com/marinoscar/AuthMate/blob/64b55c66f8bcd2534b5f8d8e02d1c3d1a439a9ef/src/Luval.AuthMate/AuthMateService.cs#L306
    await authService.UserAuthorizationProcessAsync(context.Identity, (u, c) =>
    {
        if (Debugger.IsAttached)
            Console.WriteLine($"User Id: {u.Id} Email {u.Email} Provider Key: {u.ProviderKey}");

    }, CancellationToken.None);


};
// AuthMate: Add Google Authentication configuration
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
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

/*** AuthMate: Additional configuration  ****/
app.MapControllers();
app.UseRouting();
app.UseAuthorization();
app.UseAuthentication();
/*** AuthMate:                           ****/

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
# Database Information
The database has been implemented in postgres but can be very easily implemented in any other database engine that supports Entity framework, all you need to do is to extend this class https://github.com/marinoscar/AuthMate/blob/main/src/Luval.AuthMate/AuthMateContext.cs

## ERD Model
![ERD Model](https://raw.githubusercontent.com/marinoscar/AuthMate/refs/heads/main/media/database_erd.svg)

## Semantic Model

### 1. `AccountType`
- Represents the type of an account (e.g., Free, Tier1, Tier2).
- **Primary Key**: `Id`
- **Attributes**:
  - `Name`: The name of the account type.
  - `UtcCreatedOn`: Timestamp of record creation.
  - `CreatedBy`: Creator of the record.
  - `UtcUpdatedOn`: Timestamp of the last update.
  - `UpdatedBy`: Last updater of the record.
  - `Version`: Version control for the record.
- **Default Values**:
  - A default row for the "Free" account type.

### 2. `Account`
- Represents an account with reference to its type and owner information.
- **Primary Key**: `Id`
- **Foreign Key**: `AccountTypeId` (references `AccountType.Id`)
- **Attributes**:
  - `Owner`: Owner of the account.
  - `Name`: Account name.
  - `Description`: Description of the account.
  - `UtcCreatedOn`: Timestamp of record creation.
  - `CreatedBy`: Creator of the record.
  - `UtcUpdatedOn`: Timestamp of the last update.
  - `UpdatedBy`: Last updater of the record.
  - `Version`: Version control for the record.

### 3. `AppUser`
- Represents a user with authentication and profile information.
- **Primary Key**: `Id`
- **Foreign Key**: `AccountId` (references `Account.Id`)
- **Attributes**:
  - `DisplayName`: Display name of the user.
  - `Email`: Email address (unique).
  - `ProviderKey`: Authentication provider key.
  - `ProviderType`: Type of authentication provider (e.g., Google, Microsoft).
  - `ProfilePictureUrl`: URL for the user's profile picture.
  - `UtcActiveUntil`: Date until which the user is active.
  - `Metadata`: Additional metadata in JSON format.
  - `UtcCreatedOn`: Timestamp of record creation.
  - `CreatedBy`: Creator of the record.
  - `UtcUpdatedOn`: Timestamp of the last update.
  - `UpdatedBy`: Last updater of the record.
  - `Version`: Version control for the record.

### 4. `Role`
- Represents roles in the system (e.g., Admin, User).
- **Primary Key**: `Id`
- **Attributes**:
  - `Name`: Role name.
  - `Description`: Description of the role's responsibilities.
  - `UtcCreatedOn`: Timestamp of record creation.
  - `CreatedBy`: Creator of the record.
  - `UtcUpdatedOn`: Timestamp of the last update.
  - `UpdatedBy`: Last updater of the record.
  - `Version`: Version control for the record.
- **Default Values**:
  - Predefined roles include Administrator, Owner, Member, and Visitor.

### 5. `AppUserRole`
- Represents the relationship between users and roles.
- **Primary Key**: `Id`
- **Foreign Keys**:
  - `AppUserId` (references `AppUser.Id`)
  - `RoleId` (references `Role.Id`)
- **Attributes**:
  - `UtcCreatedOn`: Timestamp of record creation.
  - `CreatedBy`: Creator of the record.
  - `UtcUpdatedOn`: Timestamp of the last update.
  - `UpdatedBy`: Last updater of the record.
  - `Version`: Version control for the record.

### 6. `PreAuthorizedAppUser`
- Represents pre-authorized users who can create accounts in the system.
- **Primary Key**: `Id`
- **Foreign Key**: `AccountTypeId` (references `AccountType.Id`)
- **Attributes**:
  - `Email`: Email address of the pre-authorized user.
  - `UtcCreatedOn`: Timestamp of record creation.
  - `CreatedBy`: Creator of the record.
  - `UtcUpdatedOn`: Timestamp of the last update.
  - `UpdatedBy`: Last updater of the record.
  - `Version`: Version control for the record.

## Relationships
1. `AccountType` has a **one-to-many** relationship with `Account`.
2. `Account` has a **one-to-many** relationship with `AppUser`.
3. `Role` has a **many-to-many** relationship with `AppUser` via `AppUserRole`.
4. `AccountType` has a **one-to-many** relationship with `PreAuthorizedAppUser`.

---

### Diagram
Consider using tools like [dbdiagram.io](https://dbdiagram.io) or [draw.io](https://drawio.com) to create a visual representation of this model.

---

### Sample Data
- Default account type: `Free`
- Predefined roles: `Administrator`, `Owner`, `Member`, `Visitor`.
- Pre-authorized user: `oscar.marin.saenz@gmail.com` (associated with the "Free" account type).
