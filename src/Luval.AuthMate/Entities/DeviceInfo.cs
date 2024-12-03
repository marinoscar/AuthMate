using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Luval.AuthMate.Entities
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
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the operating system of the device.
        /// </summary>
        /// <remarks>
        /// The operating system information includes the name and version of the OS,
        /// e.g., "Windows 10" or "iOS 16.2".
        /// </remarks>
        public string OS { get; set; }

        /// <summary>
        /// Gets or sets the browser used on the device.
        /// </summary>
        /// <remarks>
        /// The browser information includes the name and version of the browser,
        /// e.g., "Chrome 118.0" or "Safari 17.1".
        /// </remarks>
        public string Browser { get; set; }

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
        public DeviceInfo( string ipAddress, string os, string browser)
        {
            IpAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            OS = os ?? throw new ArgumentNullException(nameof(os));
            Browser = browser ?? throw new ArgumentNullException(nameof(browser));
        }

        /// <summary>
        /// Returns a JSON string representation of the device information.
        /// </summary>
        /// <returns>A JSON string that represents the device information.</returns>
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
