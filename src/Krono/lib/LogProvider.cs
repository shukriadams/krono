using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;

namespace Krono 
{
    public class LogProvider<T>
    {
        private Settings _settings;

        public LogProvider(Settings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Creates a logger file for a job
        /// </summary>
        public Microsoft.Extensions.Logging.ILogger<T> CreateJobLog<T>(string jobName)
        {
            LogLevel logLevel = LogLevel.Information;
            string logPath = Path.Join(_settings.LogRoot, jobName, "log-.txt");

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
