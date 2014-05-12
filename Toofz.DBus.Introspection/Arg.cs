using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    public abstract class Arg : DBusItem
    {
        protected Arg() : this(null) { }
        protected Arg(string name) : base(name) { DBusItemType = DBusItemType.Arg; }

        [XmlIgnore]
        public abstract Direction Direction { get; set; }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        [XmlAttribute("type")]
        public string Type { get; set; }
    }
}
