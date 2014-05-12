using System;
using System.Diagnostics;

namespace Toofz.DBusSharp
{
    internal class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(TraceEventType severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public TraceEventType Severity { get; private set; }
        public string Message { get; private set; }
    }
}
