using System;
using Microsoft.Extensions.Logging;
using static System.Console;

namespace Packt.Shared
{
    public class ConsoleLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLogger();
        }

        public void Dispose()
        {

        }
    }

    public class ConsoleLogger : ILogger
    {
        // if your logger uses unmanaged resources, you can
        // return the class that implements IDisposable here
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // to avoid overlogging, you can filter
            // on the log level
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Information:
                case LogLevel.None:
                    return false;
                case LogLevel.Debug:
                case LogLevel.Warning:
                case LogLevel.Error:
                case LogLevel.Critical:
                default:
                    return true;
            };
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if(eventId.Id == 20100)
            {
                // log the level event and event identifier
                Write("Level: {0}, Event ID: {1}, Event: {2}", logLevel, eventId.Id, eventId.Name);

                // only output the state or exception if it exists
                if(state != null)
                {
                    Write($", State: {state}");
                }
                if(exception != null)
                {
                    Write($", Exception: {exception.Message}");
                }
                WriteLine();
            }
            
           
        }
    }
}