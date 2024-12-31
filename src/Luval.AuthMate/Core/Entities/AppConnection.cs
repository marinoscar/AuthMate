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
    /// Represents an integration entity for storing information from an OAuth2 code authorization flow.
    /// </summary>
    [Table(nameof(AppConnection))]
    public class AppConnection
    {
        /// <summary>
        /// The primary key of the Integration entity.
        /// </summary>
        [Key]
        [Required(ErrorMessage = "The Id field is required.")]
        [Column("Id")]
        public ulong Id { get; set; }

        /// <summary>
        /// The name of the provider (e.g., Google, Facebook, Microsoft).
        /// </summary>
        [MinLength(1, ErrorMessage = "The ProviderName must be at least 1 character long.")]
        [MaxLength(100, ErrorMessage = "The ProviderName cannot exceed 100 characters.")]
        [Column("ProviderName")]
        public string? ProviderName { get; set; }


        /// <summary>
        /// The email of the user associated with the integration.
        /// </summary>
        [MinLength(1, ErrorMessage = "The Email must be at least 1 character long.")]
        [MaxLength(255, ErrorMessage = "The Email cannot exceed 255 characters.")]
        [Column("Email")]
        public string? OwnerEmail { get; set; }

        /// <summary>
        /// The account identifier of the user.
        /// </summary>
        [Required]
        [Column(nameof(AccountId))]
        public ulong AccountId { get; set; }

        /// <summary>
        /// The account entity associated with the app connection
        /// </summary>
        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; } = default!;

        /// <summary>
        /// The JWT access token.
        /// </summary>
        [Required(ErrorMessage = "The AccessToken field is required.")]
        [Column("AccessToken")]
        public string AccessToken { get; set; }

        /// <summary>
        /// The refresh token for the integration.
        /// </summary>
        [Column("RefreshToken")]
        public string? RefreshToken { get; set; }

        /// <summary>
        /// The scope information associated with the authorization.
        /// </summary>
        [MaxLength(8000, ErrorMessage = "The Scope cannot exceed 1000 characters.")]
        [Column("Scope")]
        public string? Scope { get; set; }

        /// <summary>
        /// The UTC timestamp of when the token was issued.
        /// </summary>
        [Required(ErrorMessage = "The UtcIssuedOn field is required.")]
        [Column("UtcIssuedOn")]
        public DateTime UtcIssuedOn { get; set; }

        /// <summary>
        /// The duration in seconds for which the token is valid.
        /// </summary>
        [Required(ErrorMessage = "The DurationInSeconds field is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "The DurationInSeconds must be greater than 0.")]
        [Column("DurationInSeconds")]
        public long DurationInSeconds { get; set; }

        /// <summary>
        /// The UTC timestamp of when the token expires.
        /// </summary>
        [NotMapped]
        public DateTime UtcExpiresOn => UtcIssuedOn.AddSeconds(DurationInSeconds);


        /// <summary>
        /// Gets or sets the token type as specified in http://tools.ietf.org/html/rfc6749#section-7.1.
        /// </summary>
        [MaxLength(50)]
        [Column("TokenType")]
        public string? TokenType { get; set; }

        /// <summary>
        /// The identifier of the token.
        /// </summary>
        [MaxLength(1000)]
        [Column(nameof(TokenId))]
        public string? TokenId { get; set; }

        /// <summary>
        /// The UTC timestamp of when the entity was created.
        /// </summary>
        [Required(ErrorMessage = "The UtcCreatedOn field is required.")]
        [Column("UtcCreatedOn")]
        public DateTime UtcCreatedOn { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The username or identifier of the creator.
        /// </summary>
        [Column("CreatedBy")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// The UTC timestamp of when the entity was last updated.
        /// </summary>
        [Column("UtcUpdatedOn")]
        public DateTime? UtcUpdatedOn { get; set; }

        /// <summary>
        /// The username or identifier of the updater.
        /// </summary>
        [Column("UpdatedBy")]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// The version of the entity, incremented on updates.
        /// </summary>
        [Required(ErrorMessage = "The Version field is required.")]
        [Column("Version")]
        public uint Version { get; set; }

        /// <summary>
        /// Converts the entity to a JSON string representation.
        /// </summary>
        /// <returns>The JSON string representation of the entity.</returns>
        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });
        }
    }

}
