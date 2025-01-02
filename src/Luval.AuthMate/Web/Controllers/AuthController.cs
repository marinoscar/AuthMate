using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Services;
using Luval.AuthMate.Infrastructure.Configuration;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Luval.AuthMate.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppConnectionService _appConnection;
        private readonly OAuthConnectionManager _connectionManager;
        private readonly IUserResolver _userResolver;

        public AuthController(AppConnectionService appConnectionService, OAuthConnectionManager connectionManager, IUserResolver userResolver)
        {
            _appConnection = appConnectionService ?? throw new ArgumentNullException(nameof(appConnectionService));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _userResolver = userResolver ?? throw new ArgumentNullException(nameof(userResolver));
        }

        [AllowAnonymous]
        [HttpGet("login")]
        public IActionResult Login(string provider, string? deviceInfo, string? returnUrl)
        {

            // Adds the properties ad redirect information
            // this could be change to include a redirect as part
            // of a query string if required
            var prop = new AuthenticationProperties()
            {
                RedirectUri = "/"
            };

            prop.Items.Add("deviceInfo", deviceInfo);
            prop.Items.Add("returnUrl", returnUrl);

            // Creates tthe challange
            var challange = Challenge(prop, provider);

            return challange;
        }

        [HttpGet("consent/{provider}")]
        public IActionResult Consent(string provider)
        {
            var config = _connectionManager.GetConfiguration(provider);
            if (config == null) return BadRequest("Provider not supported.");

            var consentUrl = _appConnection.CreateAuthorizationConsentUrl(config);

            return Redirect(consentUrl);
        }

        [HttpGet("codecallback")]
        public async Task<IActionResult> CodeCallback([FromQuery] string provider, [FromQuery] string code, [FromQuery] string? error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest($"Error from OAuth provider: {error}");
            }

            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Authorization code is missing.");
            }

            if (string.IsNullOrEmpty(provider))
            {
                return BadRequest("Provider name missing.");
            }

            var config = _connectionManager.GetConfiguration(provider);
            if (config == null) return BadRequest("Provider not supported.");

            OAuthTokenResponse tokenResponse;
            try
            {
                tokenResponse = await _appConnection.CreateAuthorizationCodeRequestAsync(config, code, error);
            }
            catch (InvalidOperationException invEx)
            {
                return BadRequest(invEx.Message);
            }

            var user = this.ControllerContext.HttpContext.User.ToUser();
            var connection = AppConnection.Create(tokenResponse, config, user);

            //Save the connection with the tokens in the database
            await  _appConnection.PersistConnectionAsync(connection);

            // Return user info
            return Ok();
        }

    }
}
