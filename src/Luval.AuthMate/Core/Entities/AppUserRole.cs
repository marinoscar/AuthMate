using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Luval.AuthMate.Core.Entities
{

    /// <summary>
    /// Represents the relationship between a user and a role in the system.
    /// </summary>
    [Table("AppUserRole")]
    public class AppUserRole
    {
        /// <summary>
        /// The unique identifier for the UserRole relationship.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public ulong Id { get; set; }

        /// <summary>
        /// The foreign key referencing the User table.
        /// </summary>
        [Required(ErrorMessage = "AppUserId is required.")]
        [Column("AppUserId")]
        public ulong AppUserId { get; set; }

        /// <summary>
        /// Navigation property for the User.
        /// </summary>
        [ForeignKey(nameof(AppUserId))]
        public AppUser User { get; set; }

        /// <summary>
        /// The foreign key referencing the Role table.
        /// </summary>
        [Required(ErrorMessage = "RoleId is required.")]
        [Column("RoleId")]
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
        [Required(ErrorMessage = "UtcCreatedOn is required.")]
        [Column("UtcCreatedOn")]
        public DateTime UtcCreatedOn { get; set; }

        /// <summary>
        /// The user who created the record.
        /// </summary>
        [Column("CreatedBy")]
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// The UTC timestamp when the record was last updated.
        /// </summary>
        [Required(ErrorMessage = "UtcUpdatedOn is required.")]
        [Column("UtcUpdatedOn")]
        public DateTime UtcUpdatedOn { get; set; }

        /// <summary>
        /// The user who last updated the record.
        /// </summary>
        [Column("UpdatedBy")]
        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// The version of the record, incremented on updates.
        /// </summary>
        [ConcurrencyCheck]
        [Column("Version")]
        public uint Version { get; set; }

        #endregion

        /// <summary>
        /// Initializes the control fields for the entity.
        /// </summary>
        public AppUserRole()
        {
            UtcCreatedOn = DateTime.UtcNow;
            UtcUpdatedOn = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>A JSON-formatted string representing the object.</returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });
        }
    }



}
