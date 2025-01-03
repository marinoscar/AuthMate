using Luval.AuthMate.Infrastructure.Configuration;

namespace Luval.AuthMate.Sample.Infrastructure.Data
{
    public class ConnectionDto
    {
        public ulong? Id { get; set; }
        public string? ProviderName { get; set; }
        public string? Scopes { get; set; }

    }
}
