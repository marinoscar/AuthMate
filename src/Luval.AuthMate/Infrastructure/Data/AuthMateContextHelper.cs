using Luval.AuthMate.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Infrastructure.Data
{

    /// <summary>
    /// Provides helper methods for initializing and managing the AuthMate database context.
    /// </summary>
    public class AuthMateContextHelper
    {
        private readonly AuthMateContext _context;
        private readonly ILogger<AuthMateContextHelper> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthMateContextHelper"/> class.
        /// </summary>
        /// <param name="context">The database context to be used.</param>
        /// <param name="logger">The logger to be used for logging operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when the context or logger is null.</exception>
        public AuthMateContextHelper(AuthMateContext context, ILogger<AuthMateContextHelper> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        
        ///<summary>
        /// Initializes the database by ensuring it is created, applying any pending migrations, and creating default records if the database is empty.
        /// </summary>
        /// <param name="ownerEmail">The email of the owner who is creating the records.</param>
        /// <param name="roles">An optional list of roles to be added. If not provided, default roles will be created.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Validates the owner email parameter to ensure it is not null or empty.
        /// 2. Attempts to connect to the database:
        ///    - If the database cannot be connected, it ensures the database is created.
        ///    - If the database can be connected, it checks for any pending migrations and applies them if they exist.
        /// 3. Checks if the database is empty by verifying the absence of any roles, account types, and invites to account.
        ///    - If the database is empty, it calls the CreateDefaultRecordsAsync method to populate it with default records.
        ///    - If the database is not empty, it logs that the database is already initialized.
        /// 4. Handles any exceptions that occur during the process by logging the error and rethrowing the exception.
        /// </remarks>
        public virtual async Task InitializeDbAsync(string ownerEmail, IEnumerable<Role>? roles = default, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ownerEmail))
            {
                _logger.LogError("Owner email is null or empty.");
                throw new ArgumentException("Owner email cannot be null or empty.", nameof(ownerEmail));
            }

            try
            {
                if (!await _context.Database.CanConnectAsync())
                {
                    _logger.LogInformation("Database is not connected. Ensuring database is created.");
                    var createScript = _context.Database.GenerateCreateScript();
                    await _context.Database.ExecuteSqlRawAsync(createScript, cancellationToken);
                }
                if (!_context.Roles.Any() && !_context.AccountTypes.Any() && !_context.InvitesToAccount.Any())
                {
                    _logger.LogInformation("Database is empty. Creating default records.");
                    await CreateDefaultRecordsAsync(ownerEmail, roles, cancellationToken);
                }
                else
                {
                    _logger.LogInformation("Database already initialized.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }


        /// <summary>
        /// Creates default records in the database, including account types, roles, and application invites.
        /// </summary>
        /// <param name="ownerEmail">The email of the owner who is creating the records.</param>
        /// <param name="roles">An optional list of roles to be added. If not provided, default roles will be created.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Creates a default account type with the name "Default" and associates it with the provided owner email.
        /// 2. Saves the account type to the database.
        /// 3. Checks if a list of roles is provided:
        ///    - If roles are provided, it adds each role to the database.
        ///    - If no roles are provided, it creates a set of default roles (Admin, Owner, Member, Visitor) and adds them to the database.
        /// 4. Saves the roles to the database.
        /// 5. Creates an application invite for the owner email, associating it with the created account type.
        /// 6. Saves the application invite to the database.
        /// </remarks>
        public virtual async Task CreateDefaultRecordsAsync(string ownerEmail, IEnumerable<Role>? roles = default, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ownerEmail))
            {
                _logger.LogError("Owner email is null or empty.");
                throw new ArgumentException("Owner email cannot be null or empty.", nameof(ownerEmail));
            }

            try
            {
                // Creates initial records
                var accountType = new AccountType()
                {
                    Name = "Default",
                    UpdatedBy = ownerEmail,
                    CreatedBy = ownerEmail,
                    Version = 1,
                    UtcCreatedOn = DateTime.UtcNow,
                    UtcUpdatedOn = DateTime.UtcNow
                };
                _context.AccountTypes.Add(accountType);
                await _context.SaveChangesAsync(cancellationToken);

                // Create the roles
                if (roles != null && roles.Any())
                {
                    foreach (var role in roles)
                    {
                        if (string.IsNullOrWhiteSpace(role.Name))
                        {
                            _logger.LogError("Role name is null or empty.");
                            throw new ArgumentException("Role name cannot be null or empty.", nameof(role.Name));
                        }
                        _context.Roles.Add(role);
                    }
                }
                else
                {
                    _logger.LogInformation("Creating new roles, Admin, Owner, Member, Visitor.");
                    var defaultRoles = new List<Role>()
                    {
                        new () { Name = "Admin", Description = "Full access to all features and settings in the system.", CreatedBy = ownerEmail, UpdatedBy = ownerEmail, UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow },
                        new () { Name = "Owner", Description = "Responsible for managing an organization or account.", CreatedBy = ownerEmail, UpdatedBy = ownerEmail, UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow },
                        new () { Name = "Member", Description = "Standard user with access to core system features.", CreatedBy = ownerEmail, UpdatedBy = ownerEmail, UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow },
                        new () { Name = "Visitor", Description = "Limited access for viewing purposes only.", CreatedBy = ownerEmail, UpdatedBy = ownerEmail, UtcCreatedOn = DateTime.UtcNow, UtcUpdatedOn = DateTime.UtcNow },
                    };
                    foreach (var role in defaultRoles)
                    {
                        _context.Roles.Add(role);
                    }
                }
                await _context.SaveChangesAsync(cancellationToken); // Save the roles

                // Add user to the AppInvite List
                _logger.LogInformation($"Adding user {ownerEmail} to InviteToApplication table.");
                var appInvite = new InviteToApplication()
                {
                    Email = ownerEmail,
                    AccountTypeId = accountType.Id,
                    AccountType = accountType,
                    UtcExpiration = DateTime.UtcNow.AddYears(5),
                    CreatedBy = ownerEmail,
                    UpdatedBy = ownerEmail,
                    UtcCreatedOn = DateTime.UtcNow,
                    UtcUpdatedOn = DateTime.UtcNow
                };
                _context.InvitesToApplication.Add(appInvite);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"Default records created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating default records.");
                throw;
            }
        }


        /// <summary>  
        /// Resets the database by deleting and reinitializing it with default records.  
        /// </summary>  
        /// <param name="ownerEmail">The email of the owner who is creating the records.</param>  
        /// <param name="roles">An optional list of roles to be added. If not provided, default roles will be created.</param>  
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>  
        /// <returns>A task that represents the asynchronous operation.</returns>  
        /// <remarks>  
        /// This method performs the following steps:  
        /// 1. Ensures the database is deleted.  
        /// 2. Calls the InitializeDbAsync method to reinitialize the database with default records.  
        ///    - The InitializeDbAsync method performs the following:  
        ///      a. Ensures the database is created.  
        ///      b. Checks if the database is empty by verifying the absence of any roles, account types, and invites to account.  
        ///      c. If the database is empty, it calls the CreateDefaultRecordsAsync method to populate it with default records.  
        ///         - The CreateDefaultRecordsAsync method performs the following:  
        ///           i. Creates a default account type with the name "Default" and associates it with the provided owner email.  
        ///           ii. Saves the account type to the database.  
        ///           iii. Checks if a list of roles is provided:  
        ///                - If roles are provided, it adds each role to the database.  
        ///                - If no roles are provided, it creates a set of default roles (Admin, Owner, Member, Visitor) and adds them to the database.  
        ///           iv. Saves the roles to the database.  
        ///           v. Creates an application invite for the owner email, associating it with the created account type.  
        ///           vi. Saves the application invite to the database.  
        /// </remarks>  
        public virtual async Task ResetDatabaseAsync(string ownerEmail, IEnumerable<Role>? roles = default, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ownerEmail))
            {
                _logger.LogError("Owner email is null or empty.");
                throw new ArgumentException("Owner email cannot be null or empty.", nameof(ownerEmail));
            }

            try
            {
                _logger.LogInformation("Attempting to delete the database.");
                await _context.Database.EnsureDeletedAsync(cancellationToken);
                _logger.LogInformation("Database deleted successfully.");

                _logger.LogInformation("Attempting to initialize the database with default records.");
                await InitializeDbAsync(ownerEmail, roles, cancellationToken);
                _logger.LogInformation("Database initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while resetting the database.");
                throw;
            }
        }

    }
}
