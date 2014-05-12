using System;
using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    [XmlRoot("node", IsNullable = false)]
    public sealed class Node : Parent
    {
        public Node() : this(null) { }
        public Node(string name) : base(name) { DBusItemType = DBusItemType.Node; }

        [XmlElement("interface", typeof(Interface))]
        [XmlElement("node", typeof(Node))]
        public DBusItemCollection Children { get { return base.InnerCollection; } }

        public override void Add(DBusItem child)
        {
            if (!(child is Interface || child is Node))
                throw new InvalidOperationException("child must be of type Interface or Node");
            base.Add(child);
        }
    }
}
