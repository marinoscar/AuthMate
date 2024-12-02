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
    /// Represents a log entry for application user login activity in the system.
    /// </summary>
    [Table("AppUserLoginHistory")]
    public class AppUserLoginHistory
    {
        /// <summary>
        /// The unique identifier for the login entry.
        /// </summary>
        [Key]
        [Required(ErrorMessage = "Id is required.")]
        [Column("Id")]
        public ulong Id { get; set; }

        /// <summary>
        /// The UTC timestamp when the login occurred.
        /// </summary>
        [Required(ErrorMessage = "UtcLogIn is required.")]
        [Column("UtcLogIn")]
        public DateTime UtcLogIn { get; set; }

        /// <summary>
        /// The email of the user who logged in.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [MinLength(5, ErrorMessage = "Email must be at least 5 characters long.")]
        [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        [Column("Email")]
        public string Email { get; set; }

        /// <summary>
        /// The operating system of the device used for login.
        /// </summary>
        [Required(ErrorMessage = "DeviceOperatingSystem is required.")]
        [MinLength(2, ErrorMessage = "DeviceOperatingSystem must be at least 2 characters long.")]
        [MaxLength(128, ErrorMessage = "DeviceOperatingSystem cannot exceed 128 characters.")]
        [Column("DeviceOperatingSystem")]
        public string DeviceOperatingSystem { get; set; }

        /// <summary>
        /// The IP address from which the login occurred.
        /// </summary>
        [Required(ErrorMessage = "IpAddress is required.")]
        [MinLength(7, ErrorMessage = "IpAddress must be at least 7 characters long.")]
        [MaxLength(45, ErrorMessage = "IpAddress cannot exceed 45 characters.")]
        [Column("IpAddress")]
        public string IpAddress { get; set; }

        /// <summary>
        /// The name of the device used for login.
        /// </summary>
        [Required(ErrorMessage = "DeviceName is required.")]
        [MinLength(2, ErrorMessage = "DeviceName must be at least 2 characters long.")]
        [MaxLength(128, ErrorMessage = "DeviceName cannot exceed 128 characters.")]
        [Column("DeviceName")]
        public string DeviceName { get; set; }

        /// <summary>
        /// Initializes control fields to their default values.
        /// </summary>
        public AppUserLoginHistory()
        {
            
        }

        /// <summary>
        /// Returns a string representation of the AppUserLoginHistory object.
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
