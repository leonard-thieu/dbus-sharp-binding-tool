using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    public enum Direction
    {
        [XmlEnum("in")]
        In,
        [XmlEnum("out")]
        Out
    }
}
