using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppConnectionService _appConnection;

        public AuthController(AppConnectionService appConnectionService)
        {
            _appConnection = appConnectionService ?? throw new ArgumentNullException(nameof(appConnectionService));
        }

        [HttpGet("login/{provider}")]
        public IActionResult Login(string provider)
        {
            var config = _providers.FirstOrDefault(p => p.Name.Equals(provider, StringComparison.OrdinalIgnoreCase));
            if (config == null) return BadRequest("Provider not supported.");

            var loginUrl = $"{config.AuthorizationEndpoint}?response_type=code" +
                           $"&client_id={config.ClientId}" +
                           $"&redirect_uri={config.RedirectUri}" +
                           $"&scope={Uri.EscapeDataString(config.Scopes)}";

            return Redirect(loginUrl);
        }

    }
}
