using System.IO;

namespace Toofz.DBusSharp
{
    internal interface IPrettyPrinter
    {
        string Print(TextReader reader);
    }
}
