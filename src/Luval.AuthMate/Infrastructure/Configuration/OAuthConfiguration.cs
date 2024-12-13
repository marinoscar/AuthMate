using Microsoft.AspNetCore.Authentication.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Infrastructure.Configuration
{
    public class OAuthConfiguration
    {
        public string CookieName { get; set; } = string.Empty;
        public string LoginPath { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string CallbackPath { get; set; } = string.Empty;

        public Func<OAuthCreatingTicketContext, Task> OnCreatingTicket { get; set; } = context => Task.CompletedTask;
    }
}
