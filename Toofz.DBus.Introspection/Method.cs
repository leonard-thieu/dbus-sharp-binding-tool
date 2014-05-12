using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    public sealed class Method : Parent
    {
        public Method() : this(null) { }
        public Method(string name) : base(name) { DBusItemType = DBusItemType.Method; }

        [XmlElement("arg", typeof(MethodArg))]
        [XmlElement("annotation", typeof(Annotation))]
        public DBusItemCollection Children { get { return base.InnerCollection; } }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MethodArg")]
        public override void Add(DBusItem child)
        {
            if (!(child is MethodArg || child is Annotation))
                throw new InvalidOperationException("child must be of type MethodArg or Annotation");
            base.Add(child);
        }
    }
}
