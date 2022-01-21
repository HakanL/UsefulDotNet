using System;
using System.Collections.Generic;
using System.Text;

namespace Haukcode.UsefulDotNet
{
    public struct Constants
    {
        public const string LogContextPropertyName = "LogContext";

        public const string StartContextMessageTemplate = "Start of log context {StartLogContext}";
        public const string EndContextMessageTemplate = "Duration {DurationMs:N1} ms for log context {EndLogContext}";
    }
}
