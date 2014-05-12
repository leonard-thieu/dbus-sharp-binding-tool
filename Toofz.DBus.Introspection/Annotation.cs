using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    public sealed class Annotation : DBusItem
    {
        public Annotation() : this(null) { }
        public Annotation(string name) : base(name) { DBusItemType = DBusItemType.Annotation; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}
