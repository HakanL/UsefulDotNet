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

        // May be null; if it is, Log.Logger will be lazily used
        readonly ILogger _logger;
        readonly Action dispose;

        /// <summary>
        /// Construct a <see cref="SerilogLoggerProvider"/>.
        /// </summary>
        /// <param name="logger">A Serilog logger to pipe events through; if null, the static <see cref="Log"/> class will be used.</param>
        /// <param name="dispose">If true, the provided logger or static log class will be disposed/closed when the provider is disposed.</param>
        public SerilogLoggerProvider(ILogger logger = null, bool dispose = false)
        {
            if (logger != null)
                this._logger = logger.ForContext(new[] { this });

            if (dispose)
            {
                if (logger != null)
                    this.dispose = () => (logger as IDisposable)?.Dispose();
                else
                    this.dispose = Log.CloseAndFlush;
            }
        }

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

        /// <inheritdoc />
        public void Dispose()
        {
            this.dispose?.Invoke();
        }
    }
}
