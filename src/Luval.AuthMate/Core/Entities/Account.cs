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
    /// Represents an account in the system, with a reference to its type and owner information.
    /// </summary>
    [Table("Account")]
    public class Account
    {
        /// <summary>
        /// The unique identifier for the Account.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public ulong Id { get; set; }

        /// <summary>
        /// The foreign key referencing the Account Type.
        /// </summary>
        [Required(ErrorMessage = "AccountTypeId is required.")]
        [Column("AccountTypeId")]
        public ulong AccountTypeId { get; set; }

        /// <summary>
        /// Navigation property for the Account Type.
        /// </summary>
        [ForeignKey(nameof(AccountTypeId))]
        public AccountType AccountType { get; set; }

        /// <summary>
        /// The owner of the account (typically the user who created it).
        /// </summary>
        [Required(ErrorMessage = "Owner is required.")]
        [MaxLength(100, ErrorMessage = "Owner must not exceed 255 characters.")]
        [MinLength(1, ErrorMessage = "Owner must be at least 1 character long.")]
        [Column("Owner")]
        public string Owner { get; set; }

        /// <summary>
        /// The name of the account.
        /// </summary>
        [Column("Name")]
        [MaxLength(100)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets and expiration date for the account.
        /// </summary>
        [Column("UtcExpirationDate")]
        public DateTime? UtcExpirationDate { get; set; }

        /// <summary>
        /// A description of the account.
        /// </summary>
        [Column("Description")]
        [MaxLength(250)]
        public string? Description { get; set; }

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
        public Account()
        {
            UtcCreatedOn = DateTime.UtcNow;
            UtcUpdatedOn = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns a string representation of the Account object.
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
