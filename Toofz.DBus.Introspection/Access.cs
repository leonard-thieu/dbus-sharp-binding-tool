using System;
using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    [Flags]
    public enum Access
    {
        [XmlEnum("read")]
        Read = 1 << 0,
        [XmlEnum("write")]
        Write = 1 << 1,
        [XmlEnum("readwrite")]
        ReadWrite = Read | Write
    }
}
