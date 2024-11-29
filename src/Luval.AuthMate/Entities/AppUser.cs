using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luval.AuthMate.Entities
{

    /// <summary>
    /// Represents a user in the system, storing authentication and profile information.
    /// </summary>
    public class AppUser : BaseEntity
    {
        /// <summary>
        /// The unique identifier for the User.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        /// <summary>
        /// The name of the application user
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// The unique key provided by the authentication provider (e.g., Google, Microsoft).
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string ProviderKey { get; set; }

        /// <summary>
        /// The type of the authentication provider (e.g., Google, Microsoft, Facebook).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ProviderType { get; set; }

        /// <summary>
        /// The URL of the user's profile picture.
        /// </summary>
        [MaxLength(500)]
        public string ProfilePictureUrl { get; set; }

        /// <summary>
        /// Indicates the UTC data in which the user is active in the system
        /// </summary>
        public DateTime? UtcActiveUntil { get; set; }

        /// <summary>
        /// Metadata for the user, stored as a JSON object in string format.
        /// </summary>
        public string Metadata { get; set; }

        #region Control Fields

        /// <summary>
        /// The UTC timestamp when the record was created.
        /// </summary>
        public DateTime UtcCreatedOn { get; set; }

        /// <summary>
        /// The user who created the record.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// The UTC timestamp when the record was last updated.
        /// </summary>
        public DateTime UtcUpdatedOn { get; set; }

        /// <summary>
        /// The user who last updated the record.
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// The version of the record, incremented on updates.
        /// </summary>
        [ConcurrencyCheck]
        public uint Version { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }

        public ICollection<UserInAccount> UserInAccounts { get; set; }

        #endregion

        /// <summary>
        /// Initializes the control fields for the entity.
        /// </summary>
        public AppUser()
        {
            UtcCreatedOn = DateTime.UtcNow;
            UtcUpdatedOn = DateTime.UtcNow;
        }

        /// <summary>
        /// Indicates if the user has a role assigned
        /// </summary>
        /// <param name="roleName">The name of the role</param>
        public bool HasRole(string roleName)
        {
            return UserRoles != null && UserRoles.Any(i => i.Role != null && i.Role.Name.ToLowerInvariant() == roleName.ToLowerInvariant());
        }
    }

}
