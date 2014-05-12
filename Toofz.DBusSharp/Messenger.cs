using System;
using System.Diagnostics;

namespace Toofz.DBusSharp
{
    internal class Messenger
    {
        public static event EventHandler<MessageEventArgs> Message;

        public static void SendWarning(object sender, string message)
        {
            OnMessage(sender, TraceEventType.Warning, message);
        }

        public static void SendInformation(object sender, string message)
        {
            OnMessage(sender, TraceEventType.Information, message);
        }

        private static void OnMessage(object sender, TraceEventType severity, string message)
        {
            if (Message != null)
                Message(sender, new MessageEventArgs(severity, message));
        }
    }
}
