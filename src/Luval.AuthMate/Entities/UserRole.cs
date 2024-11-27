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
    /// Represents the relationship between a user and a role in the system.
    /// </summary>
    public class UserRole
    {
        /// <summary>
        /// The unique identifier for the UserRole relationship.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        /// <summary>
        /// The foreign key referencing the User table.
        /// </summary>
        [Required]
        public ulong UserId { get; set; }

        /// <summary>
        /// Navigation property for the User.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public AppUser User { get; set; }

        /// <summary>
        /// The foreign key referencing the Role table.
        /// </summary>
        [Required]
        public ulong RoleId { get; set; }

        /// <summary>
        /// Navigation property for the Role.
        /// </summary>
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

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

        #endregion

        /// <summary>
        /// Initializes the control fields for the entity.
        /// </summary>
        public UserRole()
        {
            UtcCreatedOn = DateTime.UtcNow;
            UtcUpdatedOn = DateTime.UtcNow;
        }
    }

}
