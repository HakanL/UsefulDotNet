using System;
using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;

namespace Haukcode.UsefulDotNet
{
    public class SerilogLoggerScope : IDisposable
    {
        const string NoName = "None";

        readonly SerilogLoggerProvider provider;
        readonly object state;
        readonly IDisposable chainedDisposable;

        // An optimization only, no problem if there are data races on this.
        bool disposed;

        public SerilogLoggerScope(SerilogLoggerProvider provider, object state, IDisposable chainedDisposable = null)
        {
            this.provider = provider;
            this.state = state;

            Parent = this.provider.CurrentScope;
            this.provider.CurrentScope = this;
            this.chainedDisposable = chainedDisposable;
        }

        public SerilogLoggerScope Parent { get; }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                // In case one of the parent scopes has been disposed out-of-order, don't
                // just blindly reinstate our own parent.
                for (var scan = this.provider.CurrentScope; scan != null; scan = scan.Parent)
                {
                    if (ReferenceEquals(scan, this))
                        this.provider.CurrentScope = Parent;
                }

                this.chainedDisposable?.Dispose();
            }
        }

        public void EnrichAndCreateScopeItem(LogEvent logEvent, ILogEventPropertyFactory propertyFactory, out LogEventPropertyValue scopeItem)
        {
            if (this.state == null)
            {
                scopeItem = null;
                return;
            }

            if (this.state is IEnumerable<KeyValuePair<string, object>> stateProperties)
            {
                scopeItem = null; // Unless it's `FormattedLogValues`, these are treated as property bags rather than scope items.

                foreach (var stateProperty in stateProperties)
                {
                    if (stateProperty.Key == SerilogLoggerProvider.OriginalFormatPropertyName && stateProperty.Value is string)
                    {
                        scopeItem = new ScalarValue(this.state.ToString());
                        continue;
                    }

                    var key = stateProperty.Key;
                    var destructureObject = false;
                    var value = stateProperty.Value;

                    if (key.StartsWith("@"))
                    {
                        key = key.Substring(1);
                        destructureObject = true;
                    }

                    if (key.StartsWith("$"))
                    {
                        key = key.Substring(1);
                        value = value?.ToString();
                    }

                    var property = propertyFactory.CreateProperty(key, value, destructureObject);
                    logEvent.AddPropertyIfAbsent(property);
                }
            }
            else
            {
                scopeItem = propertyFactory.CreateProperty(NoName, this.state).Value;
            }
        }
    }
}
