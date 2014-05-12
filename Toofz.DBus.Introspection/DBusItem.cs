using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    public abstract class DBusItem
    {
        protected DBusItem() : this(null) { }
        protected DBusItem(string name) { Name = name; }

        [XmlIgnore]
        public DBusItemType DBusItemType { get; protected set; }

        [XmlIgnore]
        public Parent Parent { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
