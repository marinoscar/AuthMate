using Luval.AuthMate.Infrastructure.Configuration;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Luval.AuthMate.Sample.Infrastructure.Data
{
    /// <summary>
    /// Represents a data transfer object for a connection.
    /// </summary>
    public class ConnectionDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the connection.
        /// </summary>
        public ulong? Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the provider.
        /// </summary>
        public string? ProviderName { get; set; }

        /// <summary>
        /// Gets or sets the scopes associated with the connection.
        /// </summary>
        public string? Scopes { get; set; }

        /// <summary>
        /// Gets or sets the user account associated with the connection.
        /// </summary>
        public string? UserAccount { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the connection was issued.
        /// </summary>
        public DateTime? IssuedOn { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the connection expires.
        /// </summary>
        public DateTime? ExpiresOn { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the connection expires in UTC.
        /// </summary>
        public DateTime? UtcExpiresOn { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the connection was last modified.
        /// </summary>
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// Gets or sets the additional data associated with the connection.
        /// </summary>
        public bool HasExpired => UtcExpiresOn < DateTime.UtcNow;

        /// <summary>
        /// Gets the status of the connection.
        /// </summary>
        public string Status => HasExpired ? "Expired" : "Active";

        /// <summary>
        /// Gets the appearance of the connection status.
        /// </summary>
        public Appearance StatusAppearance => HasExpired ? Appearance.Neutral : Appearance.Accent;
    }
}
