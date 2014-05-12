using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using DBus;
using Toofz.DBus.Introspection;

namespace Toofz.DBusSharp.CodeDom
{
    internal sealed class IntrospectionReader
    {
        public IntrospectionReader(Stream stream)
        {
            compileUnit = new CodeCompileUnit();

            rootNamespace = new CodeNamespace();
            compileUnit.Namespaces.Add(rootNamespace);

            var node = IntrospectionSerializer.Load(stream);
            ReadNode(node);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Stream Generate(Language language)
        {
            using (var provider = GetCodeDomProvider(language))
            {
                var ms = new MemoryStream();
                var sw = new StreamWriter(ms);

                // Generate code
                var hasTypes =
                    compileUnit.Namespaces()
                               .Any(n => n.Types.Count > 0);
                if (hasTypes)
                    provider.GenerateCode(compileUnit, sw);

                // Flush and reset
                sw.Flush();
                ms.Position = 0;

                return ms;
            }
        }

        public static CompilationResults Compile(Language language, string fileName)
        {
            using (var provider = GetCodeDomProvider(language))
            {
                var par = new CompilerParameters(new[] { "System.dll", "dbus-sharp.dll" });
                par.CompilerOptions = "/o";
                par.OutputAssembly = Path.ChangeExtension(fileName, "dll");

                var results = provider.CompileAssembly(par, fileName);
                var pathToAssembly = results.PathToAssembly;
                var errors =
                    results.Errors
                           .OfType<CompilerError>()
                           .Select(e => e.ToString());

                return new CompilationResults(pathToAssembly, errors);
            }
        }

        private static CodeGeneratorContainer GetCodeDomProvider(Language language)
        {
            var opt = new CodeGeneratorOptions();
            if (language == Language.CSharp)
            {
                opt.BlankLinesBetweenMembers = false;
                opt.BracingStyle = "C";
            }
            else if (language == Language.VisualBasic)
            {
                // TODO: What are reasonable defaults for VB?
            }

            return new CodeGeneratorContainer(language, opt);
        }

        private CodeCompileUnit compileUnit;
        private CodeNamespace rootNamespace;

        private void ReadNode(Node node)
        {
            foreach (var nodeChild in node.Children)
            {
                switch (nodeChild.DBusItemType)
                {
                    case DBusItemType.Interface:
                        var name = nodeChild.Name;
                        if (WellKnownInterfaces.ContainsKey(name))
                        {
                            Messenger.SendInformation(this,
                                string.Format("{0} is a well-known type. Use {1} from DBusSharp.",
                                              name, WellKnownInterfaces[name]));
                        }
                        else
                        {
                            ReadInterface((Interface)nodeChild);
                        }
                        break;
                    case DBusItemType.Node:
                        // Do nothing for a sub-node
                        break;
                    default:
                        throw CreateInvalidDataException(node.DBusItemType, DBusItemType.Interface, DBusItemType.Node);
                }
            }
        }

        private void ReadInterface(Interface @interface)
        {
            var interfaceName = @interface.Name;
            var fqt = new FullyQualifiedType(interfaceName);
            var declaration = CodeTypeContainer.CreateInterface(fqt.TypeName);
            ImportNamespaces(declaration.RequiredImports);

            var @namespace = fqt.Namespace;
            @namespace.Types.Add(declaration);
            AddNamespace(@namespace);

            foreach (var item in @interface.Children)
            {
                switch (item.DBusItemType)
                {
                    case DBusItemType.Signal:
                        var eventHandler = ReadSignal((Signal)item, @namespace);
                        declaration.AddEvent(item.Name, eventHandler.Reference);
                        break;
                    case DBusItemType.Method:
                        var method = ReadMethod((Method)item, @namespace);
                        declaration.Members.Add(method);
                        break;
                    case DBusItemType.Property:
                        var property = ReadProperty((Property)item, @namespace);
                        declaration.Members.Add(property);
                        break;
                    case DBusItemType.Annotation:
                        var attribute = ReadAnnotation((Annotation)item, @namespace);
                        declaration.CustomAttributes.Add(attribute);
                        break;
                    default:
                        throw CreateInvalidDataException(@interface.DBusItemType, DBusItemType.Signal, DBusItemType.Method, DBusItemType.Property, DBusItemType.Annotation);
                }
            }
        }

        private CodeTypeContainer ReadSignal(Signal signal, CodeNamespace @namespace)
        {
            var typeDeclaration = CodeTypeContainer.CreateDelegate(signal.Name + "Handler");
            ProcessArgs(signal.Children, typeDeclaration, @namespace);

            // Add the type declaration if it doesn't already exist
            if (!@namespace.Types.Contains(typeDeclaration))
                @namespace.Types.Add(typeDeclaration); // What should happen if there's a type name conflict?

            return typeDeclaration;
        }

        private CodeMemberMethod ReadMethod(Method method, CodeNamespace @namespace)
        {
            var declaration = new CodeMemberMethodI(method.Name);
            ProcessArgs(method.Children, declaration, @namespace);

            return declaration;
        }

        private void ProcessArgs(DBusItemCollection items, ICodeDelegate @delegate, CodeNamespace @namespace)
        {
            var hasReturnType = false;
            var p = 0;
            foreach (var item in items)
            {
                switch (item.DBusItemType)
                {
                    case DBusItemType.Arg:
                        var arg = (Arg)item;
                        var type = GetDBusType(arg.Type, @namespace, @delegate);

                        var declaration = new CodeParameterDeclarationExpression();
                        declaration.Type = type.Reference;

                        if (arg.Parent is Signal)
                        {
                            // Even though signal args are always out, DBusSharp allows us to treat them as in
                            declaration.Direction = FieldDirection.In;
                        }
                        else
                        {
                            declaration.Direction = arg.Direction == Direction.In ?
                                FieldDirection.In : FieldDirection.Out;
                        }

                        // Becomes the return type if we don't have one yet
                        if (declaration.Direction == FieldDirection.Out && !hasReturnType)
                        {
                            @delegate.ReturnType = declaration.Type;
                            hasReturnType = true;

                            if (!string.IsNullOrWhiteSpace(arg.Name))
                            {
                                var argumentAttribute = Declare.Attribute("Argument", arg.Name);
                                ImportNamespace("DBus");
                                var method = (CodeMemberMethod)@delegate;
                                method.ReturnTypeCustomAttributes.Add(argumentAttribute);
                            }
                        }
                        else
                        {
                            var name = arg.Name;
                            if (string.IsNullOrWhiteSpace(name))
                                name = "arg" + p++;

                            var parameters = @delegate.Parameters();
                            var n = 2;
                            while (parameters.Any(c => name == c.Name))
                                name += n++;

                            declaration.Name = name;

                            @delegate.Parameters.Add(declaration);
                        }
                        break;
                    case DBusItemType.Annotation:
                        var attribute = ReadAnnotation((Annotation)item, @namespace);
                        @delegate.CustomAttributes.Add(attribute);
                        break;
                    default:
                        throw CreateInvalidDataException(item.DBusItemType, DBusItemType.Arg, DBusItemType.Annotation);
                }
            }
        }

        private CodeMemberProperty ReadProperty(Property property, CodeNamespace @namespace)
        {
            var access = property.Access;
            var dBusType = GetDBusType(property.Type, @namespace, null);

            var declaration = new CodeMemberProperty();
            declaration.Name = property.Name;
            declaration.Type = dBusType.Reference;
            declaration.Attributes = Declare.Public;
            declaration.HasGet = (access & Access.Read) != 0;
            declaration.HasSet = (access & Access.Write) != 0;

            foreach (var propertyChild in property.Children)
            {
                switch (propertyChild.DBusItemType)
                {
                    case DBusItemType.Annotation:
                        var attribute = ReadAnnotation((Annotation)propertyChild, @namespace);
                        declaration.CustomAttributes.Add(attribute);
                        break;
                    default:
                        throw CreateInvalidDataException(propertyChild.DBusItemType, DBusItemType.Annotation);
                }
            }

            return declaration;
        }

        private CodeAttributeDeclaration ReadAnnotation(Annotation annotation, CodeNamespace nameSpace)
        {
            var declaration = WellKnownAnnotations[annotation.Name];
            if (declaration != null)
                return declaration;

            var memberName = annotation.Name;

            var fqt = new FullyQualifiedType(memberName);
            var attributeName = fqt.TypeName + "Attribute";

            var attributeNamespace =
                compileUnit.Namespaces()
                               .FirstOrDefault(n => n.Name == fqt.Namespace.Name);
            if (attributeNamespace == null)
            {
                attributeNamespace = fqt.Namespace ?? nameSpace;
                AddNamespace(attributeNamespace);
            }
            ImportNamespace(attributeNamespace.Name, nameSpace);

            // Declare the Attribute only if it doesn't exist already.
            var exists =
                attributeNamespace.Types()
                                  .Any(a => a.Name == attributeName);
            if (!exists)
            {
                var typeDeclaration = CodeTypeContainer.CreateAttribute(attributeName);

                var valueProperty = Declare.Property("Value", CodeTypeContainer.Create(typeof(string)).Reference);
                var valueParameter = valueProperty.AsParameter();

                var constructor = new CodeConstructor();
                constructor.Attributes = MemberAttributes.Public;
                constructor.Parameters.Add(valueParameter);
                constructor.Statements.Add(new CodeAssignStatement(valueProperty.Reference(), valueParameter.Reference()));

                typeDeclaration.Members.Add(constructor);
                typeDeclaration.Members.Add(valueProperty);

                attributeNamespace.Types.Add(typeDeclaration);
                ImportNamespaces(typeDeclaration.RequiredImports);
            }

            return Declare.Attribute(fqt.TypeName, annotation.Value);
        }

        private IDictionary<string, string> WellKnownInterfaces = new Dictionary<string, string>
        {
            { "org.freedesktop.DBus.Peer", "org.freedesktop.DBus.Peer" },
            { "org.freedesktop.DBus.Introspectable", "org.freedesktop.DBus.Introspectable" },
            { "org.freedesktop.DBus.Properties", "org.freedesktop.DBus.Properties" },
            { "org.freedesktop.DBus", "org.freedesktop.IBus" }
        };

        // DBusSharp seems to only provide special handling for "org.freedesktop.DBus.Deprecated"
        private IDictionary<string, CodeAttributeDeclaration> WellKnownAnnotations = new Dictionary<string, CodeAttributeDeclaration>
        {
            { "org.freedesktop.DBus.Deprecated", Declare.Attribute("Obsolete") },
            { "org.freedesktop.DBus.GLib.CSymbol", null },
            { "org.freedesktop.DBus.Method.NoReply", null },
            { "org.freedesktop.DBus.Property.EmitsChangedSignal", null }
        };

        private CodeTypeContainer GetDBusType(string type, CodeNamespace @namespace, ICodeDelegate @delegate = null)
        {
            using (var sr = new StringReader(type))
                return GetNextDBusType(sr, new Stack<CodeTypeContainer>(), @namespace, @delegate);
        }

        private CodeTypeContainer GetNextDBusType(StringReader sr, Stack<CodeTypeContainer> containers, CodeNamespace @namespace, ICodeDelegate @delegate)
        {
            CodeTypeContainer container;
            int code;
            switch (code = sr.Read())
            {
                case '(': // STRUCT
                    container = CodeTypeContainer.CreateStruct();
                    containers.Push(container);

                    var i = 1;
                    do
                    {
                        var type = GetNextDBusType(sr, containers, @namespace, @delegate);

                        // Seeing ourselves signals that we are complete
                        if (type == container)
                            break;

                        // Add each member as a property
                        var property = Declare.Property("Item" + i++, type.Reference);
                        container.Members.Add(property.BackingField);
                        container.Members.Add(property);
                    } while (true);

                    // Empty structures are not allowed; there must be at least one type code between 
                    // the parentheses. [1]
                    // [1] http://dbus.freedesktop.org/doc/dbus-specification.html#container-types
                    if (container.Members.Count == 0)
                        throw new InvalidDataException("Empty structures are not allowed; " +
                            "there must be at least one type code between the parentheses.");

                    var j = 2;
                    var name = @delegate.Name + "Data";
                    while (@namespace.ContainsTypeName(name))
                        name = @delegate.Name + "Data" + j++;
                    container.Name = name;
                    @namespace.Types.Add(container);

                    return container;
                case 'a': // ARRAY
                    container = CodeTypeContainer.CreateArray();
                    containers.Push(container);

                    // The array type code must be followed by a single complete type. [1]
                    var nextType = GetNextDBusType(sr, containers, @namespace, @delegate);
                    if (GetContainerType(nextType) == Container.DictEntryArray && !nextType.IsComplete)
                    {
                        nextType.IsComplete = true;
                        return nextType;
                    }

                    containers.Pop();
                    container.SetArrayElementType(nextType);

                    return container;
                case '{': // DICT_ENTRY
                    // Implementations must not accept dict entries outside of arrays... [1]
                    if (GetContainerType(containers.Pop()) != Container.Array)
                        throw new InvalidDataException("DICT_ENTRY must occur in an array. e.g.: type=\"a{us}\"");

                    container = CodeTypeContainer.CreateIDictionary();
                    containers.Push(container);

                    var key = GetNextDBusType(sr, containers, @namespace, @delegate);

                    // ...the first single complete type (the "key") must be a basic type rather than 
                    // a container type. [1]
                    if (GetContainerType(key) != Container.None && key.Name != typeof(object).ToString())
                        throw new InvalidDataException("The key for DICT_ENTRY must be a basic type.");

                    var value = GetNextDBusType(sr, containers, @namespace, @delegate);

                    container.TypeArguments.Add(key.Reference);
                    container.TypeArguments.Add(value.Reference);
                    containers.Pop();

                    ImportNamespaces(container.RequiredImports);

                    return container;
                case ')': // STRUCT closing delimiter
                    return containers.Pop();
                case '}': // DICT_ENTRY closing delimiter
                    return containers.Pop();
                case -1: // Stream end
                    Messenger.SendWarning(this, "Reached end of stream.");
                    return containers.Pop();
                default: // Basic types
                    return GetBasicType(code);
            }
        }

        private static Container GetContainerType(CodeTypeContainer container)
        {
            if (container == null)
                return Container.None;

            if (container.IsArray)
                return Container.Array;
            if (container.IsIDictionary)
                return Container.DictEntryArray;
            if (container.IsStruct)
                return Container.Struct;
            return Container.None;
        }

        private CodeTypeContainer GetBasicType(int type)
        {
            switch (type)
            {
                case 'y': return CodeTypeContainer.Create(typeof(byte));
                case 'b': return CodeTypeContainer.Create(typeof(bool));
                case 'n': return CodeTypeContainer.Create(typeof(short));
                case 'q': return CodeTypeContainer.Create(typeof(ushort));
                case 'i': return CodeTypeContainer.Create(typeof(int));
                case 'u': return CodeTypeContainer.Create(typeof(uint));
                case 'x': return CodeTypeContainer.Create(typeof(long));
                case 't': return CodeTypeContainer.Create(typeof(ulong));
                case 'f':
                    Messenger.SendWarning(this, "Encountered single ('f') type. Please check that your version of DBusSharp supports it.");
                    return CodeTypeContainer.Create(typeof(float));
                case 'd': return CodeTypeContainer.Create(typeof(double));
                case 's': return CodeTypeContainer.Create(typeof(string));
                case 'v': return CodeTypeContainer.Create(typeof(object));
                case 'o': return CodeTypeContainer.Create(typeof(ObjectPath));
                case 'g': return CodeTypeContainer.CreateSignature();
                default: return CodeTypeContainer.Create(typeof(void));
            }
        }

        private static InvalidDataException CreateInvalidDataException(DBusItemType element, params DBusItemType[] elements)
        {
            return new InvalidDataException(
                string.Format(CultureInfo.CurrentCulture,
                             "{0} is not a valid element. Expected elements are: {1}.",
                              element.ToString(),
                              string.Join(", ", elements)));
        }

        private void AddNamespace(CodeNamespace nameSpace)
        {
            if (nameSpace == null)
                throw new ArgumentNullException("nameSpace");

            // This check might not be necessary
            var exists =
                compileUnit.Namespaces()
                               .Any(n => n.Name == nameSpace.Name);
            if (!exists)
                compileUnit.Namespaces.Add(nameSpace);
        }

        private void ImportNamespace(string import, CodeNamespace @namespace = null)
        {
            ImportNamespaces(new[] { new CodeNamespaceImport(import) }, @namespace);
        }

        private void ImportNamespaces(IList<CodeNamespaceImport> imports, CodeNamespace @namespace = null)
        {
            if (imports == null)
                throw new ArgumentNullException("imports");

            @namespace = @namespace ?? rootNamespace;
            @namespace.Imports.AddRange(imports.Where(n => n != null).ToArray());
        }
    }
}
