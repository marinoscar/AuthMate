using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Core.Interfaces;

namespace Luval.AuthMate.Tests
{
    public class NullUserResolver : IUserResolver
    {

        public static string DefaultResult { get; set; } = "NullValue";

        public NullUserResolver()
        {
        }

        public NullUserResolver(string defaultValue)
        {
            DefaultResult = defaultValue;
        }

        public string GetUserEmail()
        {
            return DefaultResult;
        }

        public string GetUserName()
        {
            return DefaultResult;
        }

        public AppUser GetUser()
        {
            return new AppUser()
            {
                Email = DefaultResult
            };
        }
    }
}