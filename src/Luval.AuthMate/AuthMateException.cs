using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate
{
    public class AuthMateException : Exception
    {
        public AuthMateException() : base() { }

        public AuthMateException(string message) : base(message) { }
        public AuthMateException(string message, Exception innerException) : base(message, innerException) { }
    }
}
