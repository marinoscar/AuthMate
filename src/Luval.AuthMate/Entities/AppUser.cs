using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Luval.AuthMate.Entities
{

    /// <summary>
    /// Represents a user in the system, storing authentication and profile information.
    /// </summary>
    [Table("AppUser")]
    public class AppUser
    {
        /// <summary>
        /// The unique identifier for the User.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public ulong Id { get; set; }

        /// <summary>
        /// The name of the application user.
        /// </summary>
        [MaxLength(255, ErrorMessage = "DisplayName must not exceed 255 characters.")]
        [Column("DisplayName")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Column("Email")]
        public string Email { get; set; }

        /// <summary>
        /// The unique key provided by the authentication provider (e.g., Google, Microsoft).
        /// </summary>
        [Required(ErrorMessage = "ProviderKey is required.")]
        [MaxLength(255, ErrorMessage = "ProviderKey must not exceed 255 characters.")]
        [Column("ProviderKey")]
        public string ProviderKey { get; set; }

        /// <summary>
        /// The type of the authentication provider (e.g., Google, Microsoft, Facebook).
        /// </summary>
        [Required(ErrorMessage = "ProviderType is required.")]
        [MaxLength(50, ErrorMessage = "ProviderType must not exceed 50 characters.")]
        [Column("ProviderType")]
        public string ProviderType { get; set; }

        /// <summary>
        /// The URL of the user's profile picture.
        /// </summary>
        [MaxLength(500, ErrorMessage = "ProfilePictureUrl must not exceed 500 characters.")]
        [Column("ProfilePictureUrl")]
        public string? ProfilePictureUrl { get; set; }

        /// <summary>
        /// Indicates the UTC date until which the user is active in the system.
        /// </summary>
        [Column("UtcActiveUntil")]
        public DateTime? UtcActiveUntil { get; set; }

        /// <summary>
        /// Metadata for the user, stored as a JSON object in string format.
        /// </summary>
        [Column("Metadata")]
        public string? Metadata { get; set; }

        /// <summary>
        /// The foreign key referencing the associated Account.
        /// </summary>
        [Required(ErrorMessage = "AccountId is required.")]
        [Column("AccountId")]
        public ulong AccountId { get; set; }

        /// <summary>
        /// Navigation property for the associated Account.
        /// </summary>
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

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
        public AppUser()
        {
            UtcCreatedOn = DateTime.UtcNow;
            UtcUpdatedOn = DateTime.UtcNow;
        }

        /// <summary>
        /// Returns a string representation of the AppUser object.
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

        /// <summary>
        /// Gets the initials for the display name
        /// </summary>
        /// <returns>The initials or an empty string</returns>
        public string GetDisplayNameInitials()
        {
            if(string.IsNullOrWhiteSpace(DisplayName)) return string.Empty;
            string pattern = @"\S+";
            // Use Regex.Matches to find all matches
            var matches = Regex.Matches(DisplayName, pattern);
            if(matches == null || matches.Count < 1) return string.Empty;
            var items = matches.Select(i => i.Value).ToList();
            if(items.Count == 1) return items[0].Substring(0, 2).ToUpperInvariant();
            return string.Join("", items.Take(2).Select(i => i.First().ToString().ToUpperInvariant()));
        }
    }


}
