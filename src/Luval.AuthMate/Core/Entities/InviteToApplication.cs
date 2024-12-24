using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core.Entities
{
    /// <summary>
    /// Represents a pre-authorized user who has the ability to create an account in the system.
    /// </summary>
    [Table("InviteToApplication")]
    public class InviteToApplication
    {
        /// <summary>
        /// The unique identifier for the pre-authorized user.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public ulong Id { get; set; }

        /// <summary>
        /// The email address of the pre-authorized user.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Column("Email")]
        public string Email { get; set; }

        /// <summary>
        /// The foreign key referencing the AccountType entity.
        /// </summary>
        [Required(ErrorMessage = "AccountTypeId is required.")]
        [Column("AccountTypeId")]
        public ulong AccountTypeId { get; set; }

        /// <summary>
        /// Navigation property for the associated AccountType.
        /// </summary>
        [ForeignKey(nameof(AccountTypeId))]
        public AccountType AccountType { get; set; }

        /// <summary>
        /// The UTC timestamp when the invitation expires.
        /// </summary>
        [Required(ErrorMessage = "UtcExpiration is required.")]
        [Column("UtcExpiration")]
        public DateTime? UtcExpiration { get; set; }

        /// <summary>
        /// The message included in the invitation.
        /// </summary>
        [MaxLength(1024, ErrorMessage = "UserMessage cannot exceed 1024 characters.")]
        [Column("UserMessage")]
        public string? UserMessage { get; set; }

        /// <summary>
        /// The UTC timestamp when the invitation was accepted.
        /// </summary>
        [Column("UtcAcceptedOn")]
        public DateTime? UtcAcceptedOn { get; set; }

        /// <summary>
        /// The UTC timestamp when the invitation was rejected.
        /// </summary>
        [Column("UtcRejectedOn")]
        public DateTime? UtcRejectedOn { get; set; }

        /// <summary>
        /// The reason provided for rejecting the invitation.
        /// </summary>
        [MaxLength(512, ErrorMessage = "RejectedReason cannot exceed 512 characters.")]
        [Column("RejectedReason")]
        public string? RejectedReason { get; set; }

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
        public InviteToApplication()
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
