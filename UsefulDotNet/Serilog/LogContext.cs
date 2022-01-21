﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Serilog;

namespace Haukcode.UsefulDotNet
{
    /// <summary>
    /// Implements a contextual object that enables logging the execution time of all activities within the context.
    /// </summary>
    public sealed class LogContext : IDisposable
    {
        private readonly ILogger logger;
        private readonly Stopwatch stopWatch;
        private readonly IDisposable ndcLogContext;
        private readonly Activity activity;
        private readonly string contextName;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogContext"/> object using the specified <paramref name="logger"/> and <paramref name="context"/> name.
        /// </summary>
        /// <param name="logger">The logger owning the context.</param>
        /// <param name="context">The arbitrary name of the logging context.</param>
        public LogContext(ILogger logger, [CallerMemberName] string context = "")
        {
            this.logger = logger;
            this.contextName = context;

            if (Activity.Current == null)
            {
                // Start a new activity
                this.activity = new Activity(context);
                this.activity.Start();
            }

            this.ndcLogContext = Serilog.Context.LogContext.PushProperty(Constants.LogContextPropertyName, context);

            this.logger.Debug(Constants.StartContextMessageTemplate, context);

            this.stopWatch = Stopwatch.StartNew();
        }

        public static LogContext Create(ILogger logger, [CallerMemberName] string context = "")
        {
            return new LogContext(logger, context);
        }

        /// <summary>
        /// Finalizes the object instance.
        /// </summary>
        ~LogContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of instance state.
        /// </summary>
        /// <param name="disposing">Determines whether this was called by Dispose or by the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;

                if (disposing)
                {
                    this.stopWatch.Stop();

                    this.logger.Information(
                        Constants.EndContextMessageTemplate,
                        this.stopWatch.Elapsed.TotalMilliseconds,
                        this.contextName);

                    this.ndcLogContext.Dispose();
                    this.activity?.Dispose();
                }
            }
        }

        /// <summary>
        /// Disposes of instance state.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
