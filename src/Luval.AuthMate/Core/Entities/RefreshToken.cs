using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Luval.AuthMate.Core.Entities
{
    /// <summary>
    /// Represents a refresh token entity.
    /// </summary>
    [Table("RefreshToken")]
    public class RefreshToken
    {
        /// <summary>
        /// Gets or sets the unique identifier for the refresh token.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public ulong Id { get; set; }

        /// <summary>
        /// Gets or sets the user ID associated with the refresh token.
        /// </summary>
        [Required]
        [Column("UserId")]
        public ulong UserId { get; set; } = default!;

        /// <summary>
        /// Navigation property for the associated user.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public AppUser User { get; set; } = default!;

        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        [Required]
        [MaxLength(500)]
        [Column("Token")]
        public string Token { get; set; } = default!;

        /// <summary>
        /// Gets or sets the duration in seconds for which the token is valid.
        /// </summary>
        [Required]
        [Column("DurationInSeconds")]
        public ulong DurationInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time when the token expires.
        /// </summary>
        [Required]
        [Column("UtcExpiresOn")]
        public DateTime UtcExpiresOn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the token is valid.
        /// </summary>
        [Required]
        [Column("IsValid")]
        public bool IsValid { get; set; }

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
        public RefreshToken()
        {
            UtcCreatedOn = DateTime.UtcNow;
            UtcUpdatedOn = DateTime.UtcNow;
        }
    }
}


