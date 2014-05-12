using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Toofz.DBus.Introspection
{
    /// <summary>
    /// The main class of this assembly. Provides for deserialization and serialization of 
    /// D-BUS Object Introspection documents.
    /// </summary>
    public static class IntrospectionSerializer
    {
        /// <summary>
        /// Loads a D-BUS Object Introspection document and returns a strongly-typed 
        /// representation.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>A strongly-typed representation of the introspection.</returns>
        public static Node Load(Stream stream)
        {
            var xs = new XmlSerializer(typeof(Node));

            return (Node)xs.Deserialize(stream);
        }

        public static void Save(Stream stream, Node node)
        {
            var xs = new XmlSerializer(typeof(Node));
            var opt = new XmlWriterSettings();
            opt.Indent = true;
            opt.IndentChars = "    ";
            opt.OmitXmlDeclaration = true;

            using (var xw = XmlWriter.Create(stream, opt))
            {
                xw.WriteDocType("node",
                                "-//freedesktop//DTD D-BUS Object Introspection 1.0//EN",
                                "http://www.freedesktop.org/standards/dbus/1.0/introspect.dtd",
                                 null);
                xw.Flush();

                var ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                xs.Serialize(stream, node, ns);
            }
        }
    }
}
