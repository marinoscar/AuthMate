using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luval.AuthMate.Infrastructure.Logging
{
    public class DebugLogger : ILogger
    {
        private readonly string _categoryName;

        public DebugLogger() : this("App")
        {

        }

        public DebugLogger(string category)
        {
            _categoryName = category;
        }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (exception != null)
            {
                message += $"\n{exception}";
            }

            Debug.WriteLine($"[{logLevel}] {_categoryName}: {message}");
        }
    }

    public class DebugLogger<TEntity> : DebugLogger, ILogger<TEntity>
    {
        public DebugLogger() : base(typeof(TEntity).Name)
        {

        }
    }
}
