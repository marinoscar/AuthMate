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
using Microsoft.Extensions.Logging;

namespace Luval.AuthMate.Web.Controllers
{
    /// <summary>
    /// Controller for handling authentication-related actions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppConnectionService _appConnection;
        private readonly OAuthConnectionManager _connectionManager;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="appConnectionService">The service to manage application connections.</param>
        /// <param name="connectionManager">The manager for OAuth provider configurations.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are null.</exception>
        public AuthController(AppConnectionService appConnectionService, OAuthConnectionManager connectionManager, ILogger<AuthController> logger)
        {
            _appConnection = appConnectionService ?? throw new ArgumentNullException(nameof(appConnectionService));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initiates the login process for the specified OAuth provider.
        /// </summary>
        /// <param name="provider">The name of the OAuth provider.</param>
        /// <param name="deviceInfo">Optional device information.</param>
        /// <param name="returnUrl">Optional return URL after login.</param>
        /// <returns>An <see cref="IActionResult"/> that initiates the OAuth login process.</returns>
        [AllowAnonymous]
        [HttpGet("login")]
        public IActionResult Login(string provider, string? deviceInfo, string? returnUrl)
        {
            _logger.LogInformation("Initiating login process for provider: {Provider}", provider);

            var prop = new AuthenticationProperties()
            {
                RedirectUri = returnUrl ?? "/"
            };

            prop.Items.Add("deviceInfo", deviceInfo);
            prop.Items.Add("returnUrl", returnUrl);

            var challange = Challenge(prop, provider);

            _logger.LogInformation("Login process initiated for provider: {Provider}", provider);

            return challange;
        }

        /// <summary>
        /// Initiates the consent process for the specified OAuth provider.
        /// </summary>
        /// <param name="provider">The name of the OAuth provider.</param>
        /// <returns>An <see cref="IActionResult"/> that redirects to the consent URL.</returns>
        [AllowAnonymous]
        [HttpGet("consent")]
        public IActionResult Consent(string provider)
        {
            _logger.LogInformation("Initiating consent process for provider: {Provider}", provider);

            var config = _connectionManager.GetConfiguration(provider);
            if (config == null)
            {
                _logger.LogWarning("Provider not supported: {Provider}", provider);
                return BadRequest("Provider not supported.");
            }

            var consentUrl = _appConnection.CreateAuthorizationConsentUrl(config, this.HttpContext.GetBaseUri()?.ToString());

            _logger.LogInformation("Consent process initiated for provider: {Provider}", provider);

            return Redirect(consentUrl);
        }

        /// <summary>
        /// Handles the callback from the OAuth provider after authorization code is received.
        /// </summary>
        /// <param name="state">The state of the OAuth request.</param>
        /// <param name="code">The authorization code received from the OAuth provider.</param>
        /// <param name="error">Optional error message from the OAuth provider.</param>
        /// <returns>An <see cref="IActionResult"/> that processes the authorization code and returns user info.</returns>
        [AllowAnonymous]
        [HttpGet("codecallback")]
        public async Task<IActionResult> CodeCallback([FromQuery] string? state, [FromQuery] string code, [FromQuery] string? error)
        {
            _logger.LogInformation("Handling code callback for state: {State}", state);

            var provider = "Google";
            OAuthStateCheck stateCheck = null;
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("Error from OAuth provider: {Error}", error);
                return BadRequest($"Error from OAuth provider: {error}");
            }

            if (string.IsNullOrEmpty(code))
            {
                _logger.LogWarning("Authorization code is missing.");
                return BadRequest("Authorization code is missing.");
            }

            if (!string.IsNullOrEmpty(state))
            {
                stateCheck = OAuthStateCheck.FromString(state);
                provider = stateCheck.ProviderName;
            }

            var config = _connectionManager.GetConfiguration(provider);
            var scopes = config?.Scopes ?? "";

            if (config == null)
            {
                _logger.LogWarning("Provider not supported: {Provider}", provider);
                return BadRequest("Provider not supported.");
            }

            OAuthTokenResponse tokenResponse;
            try
            {
                tokenResponse = await _appConnection.CreateAuthorizationCodeRequestAsync(config, code);
            }
            catch (InvalidOperationException invEx)
            {
                _logger.LogError("Error creating authorization code request: {Message}", invEx.Message);
                return BadRequest(invEx.Message);
            }

            var user = this.ControllerContext.HttpContext.User.ToUser();
            var connection = AppConnection.Create(tokenResponse, config, user);

            //gets the user information
            var email = await _appConnection.GetConnectionUserInformation(config, tokenResponse.AccessToken ?? "");
            if (!string.IsNullOrEmpty(email))
                connection.ConnectionEmail = email;

            if (stateCheck != null && !string.IsNullOrEmpty(stateCheck.Scopes))
                connection.Scope = stateCheck.Scopes;

            await _appConnection.PersistConnectionAsync(connection);

            _logger.LogInformation("Code callback handled successfully for state: {State}", state);

            return Redirect("/");
        }

        /// <summary>
        /// Logs out the current user and clears authentication state.
        /// </summary>
        /// <param name="redirectUrl">Optional URL to redirect to after logout.</param>
        /// <returns>An <see cref="IActionResult"/> that logs out the user and redirects to a safe page.</returns>
        [AllowAnonymous]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout(string? redirectUrl)
        {
            _logger.LogInformation("Logging out user.");

            await HttpContext.SignOutAsync();

            Response.Cookies.Delete(".AspNetCore.Cookies");

            Response.Headers["Cache-Control"] = "no-store";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            _logger.LogInformation("User logged out successfully.");

            return Redirect(redirectUrl ?? "/");
        }
    }
}
