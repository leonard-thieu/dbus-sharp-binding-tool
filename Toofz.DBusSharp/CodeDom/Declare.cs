using System.CodeDom;
using System.Linq;

namespace Toofz.DBusSharp.CodeDom
{
    internal static class Declare
    {
        public static MemberAttributes Public = MemberAttributes.Public | MemberAttributes.Final;

        public static CodeAttributeDeclaration Attribute(string name, params object[] args)
        {
            return new CodeAttributeDeclaration(
                name,
                args.Select(a =>
                    new CodeAttributeArgument(new CodePrimitiveExpression(a))).ToArray());
        }

        public static CodeMemberPropertyFull Property(string name, CodeTypeReference type)
        {
            var property = new CodeMemberPropertyFull
            {
                Name = name,
                Type = type,
                Attributes = Public,
                HasGet = true,
                HasSet = true
            };

            // get { return _Item1; }
            property.GetStatements.Add(new CodeMethodReturnStatement(property.BackingField.Reference()));
            // set { _Item1 = value; }
            property.SetStatements.Add(Assign(property.BackingField.Reference(), new CodePropertySetValueReferenceExpression()));

            return property;
        }

        public static CodeAssignStatement Assign(CodeExpression left, CodeExpression right)
        {
            return new CodeAssignStatement(left, right);
        }
    }
}
