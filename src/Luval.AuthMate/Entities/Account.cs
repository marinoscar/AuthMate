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
    /// Represents an account in the system, with a reference to its type and owner information.
    /// </summary>
    public class Account : BaseEntity
    {
        /// <summary>
        /// The unique identifier for the Account.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        /// <summary>
        /// The foreign key referencing the Account Type.
        /// </summary>
        [Required]
        public ulong AccountTypeId { get; set; }

        /// <summary>
        /// Navigation property for the Account Type.
        /// </summary>
        [ForeignKey(nameof(AccountTypeId))]
        public AccountType AccountType { get; set; }

        /// <summary>
        /// The owner of the account (typically the user who created it).
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Owner { get; set; }

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
        public Account()
        {
            UtcCreatedOn = DateTime.UtcNow;
            UtcUpdatedOn = DateTime.UtcNow;
        }
    }

}
