using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Interface")]
    public sealed class Interface : Parent
    {
        public Interface() : this(null) { }
        public Interface(string name) : base(name) { DBusItemType = DBusItemType.Interface; }

        [XmlElement("signal", typeof(Signal))]
        [XmlElement("method", typeof(Method))]
        [XmlElement("property", typeof(Property))]
        [XmlElement("annotation", typeof(Annotation))]
        public DBusItemCollection Children { get { return base.InnerCollection; } }

        public override void Add(DBusItem child)
        {
            if (!(child is Signal || child is Method || child is Property || child is Annotation))
                throw new InvalidOperationException("child must be of type Signal, Method, Property, or Annotation");
            base.Add(child);
        }
    }
}
