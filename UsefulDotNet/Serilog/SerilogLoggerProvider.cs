using System;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Threading;
using Serilog.Context;
using Serilog;

namespace Haukcode.UsefulDotNet
{
    public class SerilogLoggerProvider : ILogEventEnricher
    {
        internal const string OriginalFormatPropertyName = "{OriginalFormat}";
        internal const string ScopePropertyName = "Scope";

        /// <inheritdoc cref="IDisposable" />
        public IDisposable BeginScope<T>(T state)
        {
            if (CurrentScope != null)
                return new SerilogLoggerScope(this, state);

            // The outermost scope pushes and pops the Serilog `LogContext` - once
            // this enricher is on the stack, the `CurrentScope` property takes care
            // of the rest of the `BeginScope()` stack.
            var popSerilogContext = Serilog.Context.LogContext.Push(this);
            return new SerilogLoggerScope(this, state, popSerilogContext);
        }

        /// <inheritdoc />
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            List<LogEventPropertyValue> scopeItems = null;
            for (var scope = CurrentScope; scope != null; scope = scope.Parent)
            {
                scope.EnrichAndCreateScopeItem(logEvent, propertyFactory, out LogEventPropertyValue scopeItem);

                if (scopeItem != null)
                {
                    scopeItems ??= new List<LogEventPropertyValue>();
                    scopeItems.Add(scopeItem);
                }
            }

            if (scopeItems != null)
            {
                scopeItems.Reverse();
                logEvent.AddPropertyIfAbsent(new LogEventProperty(ScopePropertyName, new SequenceValue(scopeItems)));
            }
        }

        readonly AsyncLocal<SerilogLoggerScope> value = new AsyncLocal<SerilogLoggerScope>();

        internal SerilogLoggerScope CurrentScope
        {
            get => this.value.Value;
            set => this.value.Value = value;
        }
    }
}
