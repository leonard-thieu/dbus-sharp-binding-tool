using System.ComponentModel;
using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    public sealed class SignalArg : Arg
    {
        public SignalArg() : this(null) { }
        public SignalArg(string name) : base(name) { Direction = Direction.Out; }

        [XmlAttribute("direction")]
        [DefaultValue(Direction.Out)]
        public override Direction Direction { get; set; }
    }
}
