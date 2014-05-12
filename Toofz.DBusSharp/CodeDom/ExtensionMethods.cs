using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace Toofz.DBusSharp.CodeDom
{
    internal static class ExtensionMethods
    {
        public static IEnumerable<CodeNamespace> Namespaces(this CodeCompileUnit compileUnit)
        {
            return compileUnit.Namespaces.OfType<CodeNamespace>();
        }

        public static IEnumerable<CodeParameterDeclarationExpression> Parameters(this ICodeDelegate method)
        {
            return method.Parameters.OfType<CodeParameterDeclarationExpression>();
        }

        public static IEnumerable<CodeTypeDeclaration> Types(this CodeNamespace @namespace)
        {
            return @namespace.Types.OfType<CodeTypeDeclaration>();
        }

        public static bool ContainsTypeName(this CodeNamespace @namespace, string name)
        {
            return @namespace.Types().Any(t => name == t.Name);
        }

        public static CodeParameterDeclarationExpression AsParameter(this CodeMemberProperty property, string name = null)
        {
            if (name == null)
            {
                name = property.Name;
                if (!string.IsNullOrWhiteSpace(name))
                    name = name[0].ToString().ToLower() + name.Substring(1);
            }

            return new CodeParameterDeclarationExpression(property.Type, name);
        }

        public static CodeExpression Reference(this CodeMemberField field)
        {
            return new CodeFieldReferenceExpression { FieldName = field.Name };
        }

        public static CodeExpression Reference(this CodeMemberProperty property)
        {
            return new CodePropertyReferenceExpression { PropertyName = property.Name };
        }

        public static CodeExpression Reference(this CodeParameterDeclarationExpression parameter)
        {
            return new CodeFieldReferenceExpression { FieldName = parameter.Name };
        }

        public static void Add(this IList<CodeNamespaceImport> imports, string import)
        {
            imports.Add(new CodeNamespaceImport(import));
        }
    }
}
