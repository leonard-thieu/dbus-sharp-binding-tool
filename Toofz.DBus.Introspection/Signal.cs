using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    public sealed class Signal : Parent
    {
        public Signal() : this(null) { }
        public Signal(string name) : base(name) { DBusItemType = DBusItemType.Signal; }

        [XmlElement("arg", typeof(SignalArg))]
        [XmlElement("annotation", typeof(Annotation))]
        public DBusItemCollection Children { get { return base.InnerCollection; } }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SignalArg")]
        public override void Add(DBusItem child)
        {
            if (!(child is SignalArg || child is Annotation))
                throw new InvalidOperationException("child must be of type SignalArg or Annotation");
            base.Add(child);
        }
    }
}
