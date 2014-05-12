using System.ComponentModel;
using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    public sealed class MethodArg : Arg
    {
        public MethodArg() : this(null) { }
        public MethodArg(string name) : base(name) { Direction = Direction.In; }

        [XmlAttribute("direction")]
        [DefaultValue(Direction.In)]
        public override Direction Direction { get; set; }
    }
}
