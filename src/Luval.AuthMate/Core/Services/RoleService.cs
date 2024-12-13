using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core.Services
{
    /// <summary>
    /// Service for managing role-related operations.
    /// </summary>
    public class RoleService
    {
        private readonly IAuthMateContext _context;
        private readonly ILogger<RoleService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleService"/> class.
        /// </summary>
        /// <param name="context">The database context interface.</param>
        /// <param name="logger">The logger instance.</param>
        public RoleService(IAuthMateContext context, ILogger<RoleService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a role.
        /// </summary>
        /// <param name="name">The name of the role.</param>
        /// <param name="description">The description of the role.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The created role entity.</returns>
        public async Task<Role> CreateRoleAsync(string name, string description, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Role name is required.", nameof(name));

                var role = new Role { Name = name, Description = description };
                await _context.Roles.AddAsync(role, cancellationToken).ConfigureAwait(false);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Role '{RoleName}' created successfully.", name);
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role '{RoleName}'.", name);
                throw;
            }
        }

        /// <summary>
        /// Updates a role.
        /// </summary>
        /// <param name="role">The updated role entity.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>The updated role entity.</returns>
        public async Task<Role> UpdateRoleAsync(Role role, CancellationToken cancellationToken = default)
        {
            try
            {
                if (role == null)
                    throw new ArgumentNullException(nameof(role));

                _context.Roles.Update(role);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Role with ID {RoleId} updated successfully.", role.Id);
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role with ID {RoleId}.", role?.Id);
                throw;
            }
        }

        /// <summary>
        /// Deletes a role.
        /// </summary>
        /// <param name="roleId">The ID of the role to delete.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteRoleAsync(ulong roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken).ConfigureAwait(false);
                if (role == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found.", roleId);
                    throw new InvalidOperationException($"Role with ID '{roleId}' not found.");
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Role with ID {RoleId} deleted successfully.", roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role with ID {RoleId}.", roleId);
                throw;
            }
        }
    }
}
