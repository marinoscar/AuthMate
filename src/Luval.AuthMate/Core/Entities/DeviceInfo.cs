using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luval.AuthMate.Core.Entities
{
    /// <summary>
    /// Represents information about a device, including its name, IP address, operating system, and browser.
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// Gets or sets the IP address of the device.
        /// </summary>
        /// <remarks>
        /// The IP address is used to identify the device on a network.
        /// This may be an IPv4 or IPv6 address.
        /// </remarks>
        public string IpAddress { get; set; } = "Unknown";

        /// <summary>
        /// Gets or sets the operating system of the device.
        /// </summary>
        /// <remarks>
        /// The operating system information includes the name and version of the OS,
        /// e.g., "Windows 10" or "iOS 16.2".
        /// </remarks>
        public string OS { get; set; } = "Unknown";

        /// <summary>
        /// Gets or sets the browser used on the device.
        /// </summary>
        /// <remarks>
        /// The browser information includes the name and version of the browser,
        /// e.g., "Chrome 118.0" or "Safari 17.1".
        /// </remarks>
        public string Browser { get; set; } = "Unknown";

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInfo"/> class.
        /// </summary>
        public DeviceInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceInfo"/> class with specified parameters.
        /// </summary>
        /// <param name="ipAddress">The IP address of the device.</param>
        /// <param name="os">The operating system of the device.</param>
        /// <param name="browser">The browser used on the device.</param>
        public DeviceInfo(string ipAddress, string os, string browser)
        {
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            OS = os ?? throw new ArgumentNullException(nameof(os));
            Browser = browser ?? throw new ArgumentNullException(nameof(browser));
        }

        /// <summary>
        /// Returns a string representation of the device information.
        /// </summary>
        /// <returns>A string that represents the device information.</returns>
        public override string ToString()
        {
            return this.ToJson();
        }

        /// <summary>
        /// Creates a new instance of <see cref="DeviceInfo"/> from a string representation.
        /// </summary>
        /// <param name="base64String">The string representation of the device information.</param>
        /// <returns>A new <see cref="DeviceInfo"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown if the input string format is invalid.</exception>
        public static DeviceInfo Create(string base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String))
            {
                throw new ArgumentException("Input data cannot be null or empty.", nameof(base64String));
            }

            var bytes = Convert.FromBase64String(base64String);
            var json = Encoding.UTF8.GetString(bytes);

            return JsonSerializer.Deserialize<DeviceInfo>(json) ?? throw new ArgumentException("Unable to parse the base64String argument");
        }

        /// <summary>
        /// Creates a new instance from a <see cref="AuthenticationProperties"/> object
        /// </summary>
        /// <param name="properties">The instance to extract the information from</param>
        /// <returns>A new instance of <see cref="DeviceInfo"/></returns>
        public static DeviceInfo Create(AuthenticationProperties properties)
        {
            if(properties == null) return CreateEmpty();
            return Create(properties.Items);
        }

        /// <summary>
        /// Creates a new instance from a <see cref="IDictionary{TKey, TValue}"/> object
        /// </summary>
        /// <param name="items">The instance to extract the information from</param>
        /// <returns>A new instance of <see cref="DeviceInfo"/></returns>
        public static DeviceInfo Create(IDictionary<string, string?> items)
        {
            if (items == null) return CreateEmpty();
            if (!items.ContainsKey("deviceInfo")) return CreateEmpty();
            return Create(items["deviceInfo"]);
        }

        /// <summary>
        /// Creates an empty instance of the class
        /// </summary>
        /// <returns>A new instance</returns>
        public static DeviceInfo CreateEmpty()
        {
            return new DeviceInfo() { 
               
            };
        }
    }
}
