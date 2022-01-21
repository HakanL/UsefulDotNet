using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using Serilog.Events;

namespace Haukcode.UsefulDotNet
{
    public static class SerilogHelper
    {
        public const string FileTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {LogContext} [{Level}] {Message}{NewLine}{Exception}";
        public const string TraceTemplate = "{Timestamp:HH:mm:ss.fff} {LogContext} [{Level}] {Message}{NewLine}{Exception}";

        public static void WriteWithProperties(ILogger logger, object properties, LogEventLevel level, string messageTemplate, params object[] propertyValues)
        {
            var log = logger;

            if (properties != null)
            {
                var keyValues = new List<Serilog.Core.Enrichers.PropertyEnricher>();
                var propNames = properties.GetType().GetProperties();
                foreach (var prop in propNames)
                {
                    object value = prop.GetValue(properties);

                    keyValues.Add(new Serilog.Core.Enrichers.PropertyEnricher(prop.Name, value?.ToString()));
                }

                log = log.ForContext(keyValues);
            }

            log.Write(level, messageTemplate, propertyValues);
        }
    }
}
