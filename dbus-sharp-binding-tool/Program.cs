using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.CSharp;

namespace Toofz.DBusSharp
{
    static class Program
    {
        const string Signal = "signal";
        const string Method = "method";
        const string Property = "property";
        const string Arg = "arg";
        const string Annotation = "annotation";
        const string Name = "name";
        const string Type = "type";
        const string Direction = "direction";

        static CodeNamespace rootNamespace;
        static CodeNamespace currentNamespace;
        static int structCount;

        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;
            var input = args[0];

            var ccu = new CodeCompileUnit();
            rootNamespace = new CodeNamespace();
            ccu.Namespaces.Add(rootNamespace);

            var compilerImport = new CodeNamespaceImport("System.CodeDom.Compiler");
            rootNamespace.Imports.Add(compilerImport);

            var dbusImport = new CodeNamespaceImport("DBus");
            rootNamespace.Imports.Add(dbusImport);

            var doc = new XmlDocument();
            doc.Load(input);

            var node = doc.LastChild;
            foreach (XmlNode nChild in node.ChildNodes)
                switch (nChild.Name)
                {
                    case "interface":
                        ParseInterface(ccu, nChild);
                        break;
                    case "node":
                        break;
                    default:
                        throw IDE(nChild.LocalName, "interface", "node");
                }

            using (var provider = new CSharpCodeProvider())
            {
                var output = input + "." + provider.FileExtension;
                var opt = new CodeGeneratorOptions();
                opt.BlankLinesBetweenMembers = false;
                opt.BracingStyle = "C";

                using (var sw = new StreamWriter(output, false))
                {
                    var tw = new IndentedTextWriter(sw);
                    provider.GenerateCodeFromCompileUnit(ccu, tw, opt);
                }
            }
        }

        private static void ParseInterface(CodeCompileUnit ccu, XmlNode iface)
        {
            var ifaceName = iface.Attributes[Name].Value;
            var fqt = new FullyQualifiedType(ifaceName);
            var ifaceNamespace = new CodeNamespace(fqt.Namespace);
            currentNamespace = ifaceNamespace;
            ccu.Namespaces.Add(ifaceNamespace);

            var ifaceTypeName = "I" + fqt.TypeName;
            var ifaceType = new CodeTypeDeclaration(ifaceTypeName);
            ifaceType.IsInterface = true;
            ifaceType.CustomAttributes.Add(
                new CodeAttributeDeclaration(
                    "Interface",
                    new CodeAttributeArgument(
                        new CodePrimitiveExpression(ifaceName))));
            ifaceType.CustomAttributes.Add(GCA);
            ifaceNamespace.Types.Add(ifaceType);

            var members = iface.ChildNodes;
            foreach (XmlNode member in members)
                switch (member.LocalName)
                {
                    case Signal:
                        ParseSignal(ifaceType, member, ifaceNamespace);
                        break;
                    case Method:
                        ParseMethod(ifaceType, member);
                        break;
                    case Property:
                        ParseProperty(ifaceType, member);
                        break;
                    case Annotation:
                        AddAnnotation(ifaceType, member);
                        break;
                    default:
                        throw IDE(member.LocalName, Signal, Method, Property, Annotation);
                }
        }

        private static void ParseSignal(CodeTypeDeclaration ifaceType, XmlNode member, CodeNamespace ifaceNamespace)
        {
            var memberName = member.Attributes.Get(Name);
            var parameters = member.ChildNodes;
            var sType = memberName + "Handler";

            var signal = new CodeMemberEvent { Type = new CodeTypeReference(sType), Name = memberName };
            signal.Attributes = MemberAttributes.Abstract;
            ifaceType.Members.Add(signal);

            var eventHandler = new CodeTypeDelegate(sType);
            eventHandler.ReturnType = CTR(typeof(void));
            eventHandler.CustomAttributes.Add(GCA);
            ifaceNamespace.Types.Add(eventHandler);

            for (int i = 0; i < parameters.Count; i++)
            {
                var atts = parameters[i].Attributes;
                switch (parameters[i].Name)
                {
                    case Arg:
                        var argType = GetType(atts.Get(Type));
                        var argName = atts.Get(Name);

                        eventHandler.Parameters.Add(new CodeParameterDeclarationExpression(argType, argName));
                        break;
                    case Annotation:
                        AddAnnotation(ifaceType, member);
                        break;
                    default:
                        throw IDE(member.LocalName, Arg, Annotation);
                }
            }
        }

        private static void ParseMethod(CodeTypeDeclaration ifaceType, XmlNode member)
        {
            var memberName = member.Attributes.Get(Name);
            var parameters = member.ChildNodes;

            var method = new CodeMemberMethod();
            method.Name = memberName;
            method.Attributes = MemberAttributes.Abstract;
            ifaceType.Members.Add(method);

            var hasReturnType = false;
            for (int i = 0; i < parameters.Count; i++)
                switch (parameters[i].Name)
                {
                    case Arg:
                        var atts = parameters[i].Attributes;

                        var argName = atts.Get(Name);
                        var argType = atts.Get(Type);
                        var argDirection = atts.Get(Direction);

                        var direction = argDirection == "in" ? FieldDirection.In : FieldDirection.Out;
                        var type = GetType(argType);

                        if (direction == FieldDirection.Out && !hasReturnType)
                        {
                            hasReturnType = true;
                            method.ReturnType = type;
                        }
                        else
                        {
                            var parameter = new CodeParameterDeclarationExpression(type, argName);
                            parameter.Direction = direction;

                            method.Parameters.Add(parameter);
                        }
                        break;
                    case Annotation:
                        AddAnnotation(method, parameters[i]);
                        break;
                    default:
                        throw IDE(member.LocalName, Arg, Annotation);
                }
        }

        private static void ParseProperty(CodeTypeDeclaration ifaceType, XmlNode member)
        {
            var memberName = member.Attributes.Get(Name);
            var atts = member.Attributes;

            var property = new CodeMemberProperty();
            property.Name = memberName;
            ifaceType.Members.Add(property);

            var access = atts.Get("access");
            property.HasGet = access.Contains("read");
            property.HasSet = access.Contains("write");

            property.Type = GetType(atts.Get(Type));

            foreach (XmlNode anno in member.ChildNodes)
                switch (anno.Name)
                {
                    case Annotation:
                        AddAnnotation(ifaceType, member);
                        break;
                    default:
                        throw IDE(member.LocalName, Annotation);
                }
        }

        static void AddAnnotation(CodeTypeMember element, XmlNode member)
        {
            var memberName = member.Attributes[Name].Value;
            var fqt = new FullyQualifiedType(memberName);
            var annoDeclName = fqt.TypeName + "Attribute";

            // Declare the Attribute only if it doesn't exist already.
            if (!currentNamespace.Types.OfType<CodeTypeDeclaration>().Any(a => a.Name == annoDeclName))
            {
                var annoDecl = new CodeTypeDeclaration(annoDeclName);
                annoDecl.IsClass = true;
                annoDecl.BaseTypes.Add(new CodeTypeReference("Attribute"));
                annoDecl.CustomAttributes.Add(GCA);
                currentNamespace.Types.Add(annoDecl);

                var systemImport = new CodeNamespaceImport("System");
                rootNamespace.Imports.Add(systemImport);
            }

            rootNamespace.Imports.Add(new CodeNamespaceImport(fqt.Namespace));

            var anno = new CodeAttributeDeclaration(fqt.TypeName);
            element.CustomAttributes.Add(anno);
        }

        private static string Get(this XmlAttributeCollection attributes, string name, string defaultIfNotFound = "")
        {
            var attValue = attributes.GetNamedItem(name);

            return attValue != null ? attValue.Value : defaultIfNotFound;
        }

        static CodeAttributeDeclaration GCA = new CodeAttributeDeclaration(
            new CodeTypeReference("GeneratedCodeAttribute"),
                new CodeAttributeArgument(new CodePrimitiveExpression("dbus-sharp-binding-tool")),
                new CodeAttributeArgument(new CodePrimitiveExpression("1.0.0.0")));

        static CodeTypeReference GetType(string type)
        {
            using (var sr = new StringReader(type))
            {
                CodeTypeReference returnType = null;
                int next;
                var containers = new Stack<CodeTypeReference>();
                var structs = new Stack<CodeTypeDeclaration>();

                while ((next = sr.Read()) != -1)
                    switch (next)
                    {
                        case 'a': // Array start
                            {
                                var container = CTR(typeof(Array));
                                container.ArrayRank = 1;
                                containers.Push(container);
                                break;
                            }
                        case '{': // IDictionary start
                            {
                                // Assume last character was 'a'
                                containers.Pop(); // bye2u

                                var container = new CodeTypeReference("IDictionary");
                                containers.Push(container);
                                break;
                            }
                        case '}': // IDictionary end
                            var collectionsImport = new CodeNamespaceImport("System.Collections.Generic");
                            rootNamespace.Imports.Add(collectionsImport);

                            return containers.Pop();
                        case '(': // struct start
                            {
                                var structName = "Struct" + structCount++;
                                var container = new CodeTypeReference(structName);
                                containers.Push(container);

                                var structt = new CodeTypeDeclaration(structName);
                                structt.IsStruct = true;
                                currentNamespace.Types.Add(structt);

                                structs.Push(structt);
                                break;
                            }
                        case ')': // struct end
                            structs.Pop();

                            return containers.Pop();
                        default: // All other types
                            {
                                var container = containers.Any() ?
                                    containers.Peek() : null;
                                var nextType = GetType(next);

                                if (container == null)
                                    return nextType;

                                // Becomes IDictionary`1 when it gains a type parameter
                                if (container.BaseType.StartsWith("IDictionary", StringComparison.OrdinalIgnoreCase))
                                {
                                    container.TypeArguments.Add(nextType);
                                    break;
                                }

                                // Array end
                                if (container.BaseType == typeof(Array).ToString())
                                {
                                    container.ArrayElementType = nextType;

                                    return container;
                                }

                                if (container.BaseType.StartsWith("Struct", StringComparison.OrdinalIgnoreCase))
                                {
                                    var structt = structs.Peek();
                                    var item = new CodeMemberProperty();
                                    item.Type = nextType;
                                    item.Name = GetNextItemName(structt);
                                    item.Attributes = MemberAttributes.Public | MemberAttributes.Final;

                                    structt.Members.Add(item);
                                    break;
                                }
                                break;
                            }
                    }

                return returnType;
            }
        }

        private static string GetNextItemName(CodeTypeDeclaration structt)
        {
            var i = 0;
            string itemName;
            var memberNames =
                structt.Members
                       .OfType<CodeTypeMember>()
                       .Select(m => m.Name);

            do
                itemName = "Item" + ++i;
            while (memberNames.Contains(itemName));

            return itemName;
        }

        static CodeTypeReference GetType(int type)
        {
            switch (type)
            {
                case 'y': return CTR(typeof(byte));
                case 'b': return CTR(typeof(bool));
                case 'n': return CTR(typeof(short));
                case 'q': return CTR(typeof(ushort));
                case 'i': return CTR(typeof(int));
                case 'u': return CTR(typeof(uint));
                case 'x': return CTR(typeof(long));
                case 't': return CTR(typeof(ulong));
                case 'f': return CTR(typeof(float));
                case 'd': return CTR(typeof(double));
                case 's': return CTR(typeof(string));
                case 'v': return CTR(typeof(object));
                case 'o': return new CodeTypeReference("ObjectPath");
                case 'g':
                    var protocolImport = new CodeNamespaceImport("DBus.Protocol");
                    rootNamespace.Imports.Add(protocolImport);
                    return new CodeTypeReference("Signature");
                default: return CTR(typeof(void));
            }
        }

        static InvalidDataException IDE(string memberLocalName, params string[] elements)
        {
            return new InvalidDataException(
                string.Format(CultureInfo.CurrentCulture,
                              "{0} is not a valid element. Expected elements are: {1}.",
                              memberLocalName,
                              string.Join(", ", elements)));
        }

        static CodeTypeReference CTR(Type type)
        {
            return new CodeTypeReference(type);
        }

        class FullyQualifiedType
        {
            public FullyQualifiedType(string full)
            {
                var lastDot = full.LastIndexOf('.');
                if (lastDot == -1)
                    TypeName = full;
                else
                {
                    Namespace = full.Substring(0, lastDot);
                    TypeName = full.Substring(lastDot + 1);
                }
            }

            public string Namespace { get; private set; }
            public string TypeName { get; private set; }
        }
    }
}
