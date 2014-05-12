using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
    public sealed class Property : Parent
    {
        public Property() : this(null) { }
        public Property(string name) : base(name) { DBusItemType = DBusItemType.Property; }

        [XmlElement("annotation", typeof(Annotation))]
        public DBusItemCollection Children { get { return base.InnerCollection; } }

        public override void Add(DBusItem child)
        {
            if (!(child is Annotation))
                throw new InvalidOperationException("child must be of type Annotation");
            base.Add(child);
        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("access")]
        public Access Access { get; set; }
    }
}
