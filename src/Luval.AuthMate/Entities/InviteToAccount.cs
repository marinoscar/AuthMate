using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luval.AuthMate.Entities
{
    /// <summary>
    /// Represents an invitation sent to a user to join an account with a specific role.
    /// </summary>
    [Table("InviteToAccount")]
    public class InviteToAccount
    {
        /// <summary>
        /// The unique identifier for the invitation.
        /// </summary>
        [Key]
        [Required(ErrorMessage = "Id is required.")]
        [Column("Id")]
        public ulong Id { get; set; }

        /// <summary>
        /// The foreign key to the Account table.
        /// </summary>
        [Required(ErrorMessage = "AccountId is required.")]
        [Column("AccountId")]
        public ulong AccountId { get; set; }

        /// <summary>
        /// Navigation property for the associated Account.
        /// </summary>
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        /// <summary>
        /// The email address of the invitee.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [MinLength(5, ErrorMessage = "Email must be at least 5 characters long.")]
        [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        [Column("Email")]
        public string Email { get; set; }

        /// <summary>
        /// The UTC timestamp when the invitation expires.
        /// </summary>
        [Required(ErrorMessage = "UtcExpiration is required.")]
        [Column("UtcExpiration")]
        public DateTime UtcExpiration { get; set; }

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

        /// <summary>
        /// The UTC timestamp when the record was created.
        /// </summary>
        [Required(ErrorMessage = "UtcCreatedOn is required.")]
        [Column("UtcCreatedOn")]
        public DateTime UtcCreatedOn { get; set; }

        /// <summary>
        /// The identifier of the user who created the record.
        /// </summary>
        [Column("CreatedBy")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// The UTC timestamp when the record was last updated.
        /// </summary>
        [Column("UtcUpdatedOn")]
        public DateTime? UtcUpdatedOn { get; set; }

        /// <summary>
        /// The identifier of the user who last updated the record.
        /// </summary>
        [Column("UpdatedBy")]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// The version of the record for concurrency handling.
        /// </summary>
        [Required(ErrorMessage = "Version is required.")]
        [Column("Version")]
        public uint Version { get; set; }

        /// <summary>
        /// Initializes control fields to their default values.
        /// </summary>
        public InviteToAccount()
        {
            UtcCreatedOn = DateTime.UtcNow;
            Version = 1;
        }

        /// <summary>
        /// Returns a string representation of the AccountInvite object.
        /// </summary>
        /// <returns>A JSON-formatted string representation of the object.</returns>
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
