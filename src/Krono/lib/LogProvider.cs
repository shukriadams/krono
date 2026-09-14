using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;

namespace Krono 
{
    public class LogProvider<T>
    {
        public Microsoft.Extensions.Logging.ILogger<T> Create<T>(string jobName)
        {
            LogLevel logLevel = LogLevel.Information;
            string logPath = $"./logs/{jobName}/internal-.txt";

            Serilog.Core.Logger fileLogger = new LoggerConfiguration()
                .MinimumLevel.Is((LogEventLevel)Enum.Parse(typeof(LogEventLevel), logLevel.ToString()))
                .WriteTo
                .File(logPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            ILoggerFactory loggerFactory = new LoggerFactory().AddSerilog(fileLogger);
            Microsoft.Extensions.Logging.ILogger<T> logger = loggerFactory.CreateLogger<T>();

            return logger;            
        }
    }
}
