using System.IO;

namespace Toofz.DBusSharp
{
    internal sealed class NullPrettyPrinter : IPrettyPrinter
    {
        public static readonly NullPrettyPrinter Instance = new NullPrettyPrinter();

        #region IPrettyPrinter Members

        public string Print(TextReader reader)
        {
            return reader.ReadToEnd();
        }

        #endregion
    }
}
