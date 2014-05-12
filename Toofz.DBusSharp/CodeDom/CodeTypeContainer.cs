using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;

namespace Toofz.DBusSharp.CodeDom
{
    internal sealed class CodeTypeContainer : ICodeDelegate
    {
        private static CodeAttributeDeclaration GeneratedCodeAttribute = new CodeAttributeDeclaration(
            new CodeTypeReference("GeneratedCode"),
            new CodeAttributeArgument(new CodePrimitiveExpression("dbus-sharp-binding-tool")),
            new CodeAttributeArgument(new CodePrimitiveExpression("1.0.0.0")));

        public static CodeTypeContainer CreateInterface(string name)
        {
            var interfaceName = "I" + name;
            var container = Create(interfaceName);
            container.This.IsInterface = true;
            container.CustomAttributes.Add(Declare.Attribute("Interface", interfaceName));
            container.RequiredImports.Add("DBus");

            return container;
        }

        public static CodeTypeContainer CreateDelegate(string name)
        {
            return new CodeTypeContainer(new CodeTypeDelegate(name));
        }

        public static CodeTypeContainer CreateStruct()
        {
            var container = Create("struct");
            container.IsStruct = true;

            return container;
        }

        public static CodeTypeContainer CreateArray()
        {
            var container = Create("Array");
            container.Reference.ArrayRank = 1;

            return container;
        }

        public static CodeTypeContainer CreateIDictionary()
        {
            var container = CodeTypeContainer.Create("IDictionary");
            container.IsIDictionary = true;
            container.RequiredImports.Add("System.Collections.Generic");

            return container;
        }

        public static CodeTypeContainer CreateAttribute(string name)
        {
            var container = CodeTypeContainer.Create(name);
            container.This.BaseTypes.Add(CodeTypeContainer.Create("Attribute").Reference);
            container.RequiredImports.Add("System");

            return container;
        }

        // Introduced in DBusSharp v0.8 (API v2.0)
        public static CodeTypeContainer CreateSignature()
        {
            var container = CodeTypeContainer.Create("Signature");
            container.RequiredImports.Add("DBus.Protocol");

            return container;
        }

        public static CodeTypeContainer Create(Type type)
        {
            return Create(type.ToString());
        }

        public static CodeTypeContainer Create(string name)
        {
            return new CodeTypeContainer(new CodeTypeDeclaration(name) { IsPartial = true });
        }

        private CodeTypeContainer(CodeTypeDeclaration typeDeclaration)
        {
            This = typeDeclaration;
            Reference = new CodeTypeReference();
            RequiredImports = new List<CodeNamespaceImport>();
            Name = typeDeclaration.Name;

            CustomAttributes.Add(GeneratedCodeAttribute);
            RequiredImports.Add("System.CodeDom.Compiler");
        }

        private CodeTypeDeclaration This { get; set; }

        public bool IsComplete { get; set; }
        public CodeTypeReference Reference { get; private set; }
        public IList<CodeNamespaceImport> RequiredImports { get; private set; }

        public CodeTypeReferenceCollection TypeArguments
        {
            get { return Reference.TypeArguments; }
        }

        public CodeTypeMemberCollection Members
        {
            get { return This.Members; }
        }

        public bool IsStruct
        {
            get { return This.IsStruct; }
            private set { This.IsStruct = value; }
        }

        public bool IsArray
        {
            get { return Reference.ArrayRank >= 1; }
        }

        public bool IsIDictionary { get; private set; }

        private CodeTypeContainer ArrayElementType;
        public void SetArrayElementType(CodeTypeContainer container)
        {
            ArrayElementType = container;
            Reference.ArrayElementType = ArrayElementType.Reference;
        }

        public int AddEvent(string name, CodeTypeReference type)
        {
            var @event = new CodeMemberEvent();
            @event.Attributes = MemberAttributes.Abstract;
            @event.Name = name;
            @event.Type = type;

            return Members.Add(@event);
        }

        #region ICodeMethod (Explicit) Members

        CodeParameterDeclarationExpressionCollection ICodeDelegate.Parameters
        {
            get
            {
                var del = This as CodeTypeDelegate;
                if (del != null)
                    return del.Parameters;
                return null;
            }
        }

        CodeTypeReference ICodeDelegate.ReturnType
        {
            get
            {
                var del = This as CodeTypeDelegate;
                if (del != null)
                    return del.ReturnType;
                return null;
            }
            set
            {
                var del = This as CodeTypeDelegate;
                if (del != null)
                    del.ReturnType = value;
                else
                    throw new InvalidOperationException("Cannot set return type on non-delegate type.");
            }
        }

        #endregion

        #region ICodeMethod

        public CodeTypeParameterCollection TypeParameters
        {
            get { return This.TypeParameters; }
        }

        public MemberAttributes Attributes
        {
            get { return This.Attributes; }
            set { This.Attributes = value; }
        }

        public CodeCommentStatementCollection Comments
        {
            get { return This.Comments; }
        }

        public CodeAttributeDeclarationCollection CustomAttributes
        {
            get { return This.CustomAttributes; }
            set { This.CustomAttributes = value; }
        }

        public CodeDirectiveCollection EndDirectives
        {
            get { return This.EndDirectives; }
        }

        public CodeLinePragma LinePragma
        {
            get { return This.LinePragma; }
            set { This.LinePragma = value; }
        }

        public string Name
        {
            get { return This.Name; }
            set
            {
                This.Name = value;
                Reference.BaseType = Name;
            }
        }

        public CodeDirectiveCollection StartDirectives
        {
            get { return This.StartDirectives; }
        }

        public IDictionary UserData
        {
            get { return This.UserData; }
        }

        #endregion

        public static implicit operator CodeTypeDeclaration(CodeTypeContainer container)
        {
            return container.This;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
