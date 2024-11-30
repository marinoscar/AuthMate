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
