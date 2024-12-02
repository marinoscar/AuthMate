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

namespace Luval.AuthMate.Entities
{
    /// <summary>
    /// Represents the type of an account in the system, such as Free, Tier1, or Tier2.
    /// </summary>
    [Table("AccountType")]
    public class AccountType
    {
        /// <summary>
        /// The unique identifier for the Account Type.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public ulong Id { get; set; }

        /// <summary>
        /// The name of the account type (e.g., Free, Tier1, Tier2).
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        [MinLength(1, ErrorMessage = "Name must be at least 1 character long.")]
        [Column("Name")]
        public string Name { get; set; }

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
        public AccountType()
        {
            UtcCreatedOn = DateTime.UtcNow;
            UtcUpdatedOn = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns a string representation of the AccountType object.
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
