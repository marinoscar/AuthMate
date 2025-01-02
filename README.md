![Actions Status Badge](https://github.com/marinoscar/AuthMate/actions/workflows/dotnet.yml/badge.svg)

# AuthMate: Simplify Authentication and Authorization for Your Apps

AuthMate is a comprehensive authentication and authorization system designed to manage user accounts, roles, and permissions within a Blazor application. It provides a robust and flexible framework for handling user authentication, including support for OAuth providers, user roles, and login history tracking.

## Key Features
- Social Login Integration: Quickly enable login via Google, Microsoft, and Facebook with minimal setup.
- SQL Backend Support: Manage users, roles, and permissions seamlessly using your existing SQL database
    - Support for Postgres and Sqlite, easily extensible for other providers with minimal code
- Provides capabilities for multi tenency, if desired using the concept of Accounts, multiple users can be assigned to an Account
- Account Management:
    - Assign users to one or multiple accounts.
    - Support for account types to accommodate different use cases.
    - Role-Based Access Control (RBAC): Define roles and permissions for users at the account level.
    - Developer-Friendly: Designed for quick implementation and scalability, saving you hours of development time.

## Installation
To install the AuthMate library, add the following NuGet package to your project:
```
dotnet add package Luval.AuthMate
```

## Getting Google Client Id and Client Secret
To configure the OAuth you need to get the information from google follow the steps in this article https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-8.0

## Getting started with the configuration of the AuthMate Library
This guide provides the initial configuration steps for setting up the AuthMate library in your .NET application. AuthMate is designed to manage authentication and authorization flows, including OAuth 2.0 integration.

### Configuration Settings
First, add the necessary configuration settings to your appsettings.json file. These settings include OAuth provider details and AuthMate-specific keys.

#### appsettings.json 
```json
{
  "OAuthProviders": {
    "Google": {
      "ClientID": "your-client-id",
      "OwnerEmail": "your-email",
      "AuthorizationEndpoint": "https://accounts.google.com/o/oauth2/v2/auth",
      "TokenEndpoint": "https://oauth2.googleapis.com/token",
      "UserInfoEndpoint": "https://www.googleapis.com/oauth2/v3/userinfo",
      "CodeFlowRedirectUri": "your-app.com/callback",
      "Scopes": "https://www.googleapis.com/auth/gmail.readonly"
      "OwnerEmail": "mainuser@gmail.com" // This is the email of the owner of the project
    }
  }
}
```
#### secrets.json
Update the secrets for the OAuth flows, here is an article on how to handle secrets [Safe storage of app secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-9.0&tabs=windows)
```json
{
    "OAuthProviders:Google:ClientSecret": "your-secret",
    "AuthMate:BearingTokenKey": "your-key"
}
```
### Program.cs Configuration
In your Program.cs file, configure the services and middleware required for AuthMate.

1. Create the WebApplication Builder:
```csharp
    var builder = WebApplication.CreateBuilder(args);
```
2. Add Services to the Container:
```csharp
    // Add Razor components and Fluent UI components
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();
    builder.Services.AddFluentUIComponents();

    // Add logging services (required dependency for AuthMate)
    builder.Services.AddLogging();

    // Add controllers, HTTP client, and context accessor
    builder.Services.AddControllers();
    builder.Services.AddHttpClient();
    builder.Services.AddHttpContextAccessor();
    
```
3. Add AuthMate Services:
```csharp
    var config = builder.Configuration;

    builder.Services.AddAuthMateServices(
        // The key to use for the bearing token implementation
        config["AuthMate:BearingTokenKey"],
        (s) => {
            // Returns a local instance of Sqlite
            // Replace this with your own implementation of Postgres, MySql, SqlServer, etc.
            return new SqliteAuthMateContext();
        }
    );
    
```
4. Add AuthMate Google OAuth Provider:
```csharp
    builder.Services.AddAuthMateGoogleAuth(new GoogleOAuthConfiguration()
    {
        // Client ID from your config file
        ClientId = config["OAuthProviders:Google:ClientId"] ?? throw new ArgumentNullException("The Google client id is required"),
        // Client secret from your config file
        ClientSecret = config["OAuthProviders:Google:ClientSecret"] ?? throw new ArgumentNullException("The Google client secret is required"),
        // Set the login path in the controller and pass the provider name
        LoginPath = "/api/auth",
    });
    
```
5. Build the Application:
```csharp
    var app = builder.Build();
```

6. Configure the HTTP Request Pipeline
```csharp
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();
    
```
7. Initialize the Database
```csharp
    var contextHelper = new AuthMateContextHelper(
        new SqliteAuthMateContext(),
        new ColorConsoleLogger<AuthMateContextHelper>()
    );

    // Ensure the database is created and initialize it with the owner email and required initial records
    contextHelper.InitializeDbAsync(config["OAuthProviders:Google:OwnerEmail"] ?? "")
        .GetAwaiter()
        .GetResult();
    
```

## Summary
By following these steps, you will have configured the AuthMate library in your .NET application, enabling OAuth 2.0 authentication and authorization flows. Make sure to replace placeholder values in the configuration with your actual credentials and settings.
Here is a complete example of a complete implementation [Program.cs](https://github.com/marinoscar/AuthMate/blob/main/src/Luval.AuthMate.Sample/Program.cs)


# Database Information
The database has been implemented in postgres but can be very easily implemented in any other database engine that supports Entity framework, all you need to do is to extend this class [AuthMateContext.cs](https://github.com/marinoscar/AuthMate/blob/main/src/Luval.AuthMate/Infrastructure/Data/AuthMateContext.cs)

## ERD Model
![ERD Model](https://raw.githubusercontent.com/marinoscar/AuthMate/refs/heads/main/media/erd.svg)

---

### Sample Data
- Default account type: `Free`
- Predefined roles: `Administrator`, `Owner`, `Member`, `Visitor`.
- Pre-authorized user: `oscar.marin.saenz@gmail.com` (associated with the "Free" account type).
