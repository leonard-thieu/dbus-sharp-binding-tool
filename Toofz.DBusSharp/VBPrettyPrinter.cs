using System.IO;

namespace Toofz.DBusSharp
{
    internal class VBPrettyPrinter : IPrettyPrinter
    {
        #region IPrettyPrinter Members

        public string Print(TextReader reader)
        {
            return reader.ReadToEnd();
        }

        #endregion
    }
}
