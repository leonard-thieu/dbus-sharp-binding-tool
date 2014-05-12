using System.CodeDom;
using System.Collections;

namespace Toofz.DBusSharp.CodeDom
{
    internal interface ICodeDelegate
    {
        #region CodeMemberMethod // CodeTypeDelegate/CodeTypeDeclaration

        CodeParameterDeclarationExpressionCollection Parameters { get; }
        CodeTypeReference ReturnType { get; set; }
        CodeTypeParameterCollection TypeParameters { get; }

        #endregion

        #region CodeTypeMember

        MemberAttributes Attributes { get; set; }
        CodeCommentStatementCollection Comments { get; }
        CodeAttributeDeclarationCollection CustomAttributes { get; set; }
        CodeDirectiveCollection EndDirectives { get; }
        CodeLinePragma LinePragma { get; set; }
        string Name { get; set; }
        CodeDirectiveCollection StartDirectives { get; }

        #endregion

        #region CodeObject

        IDictionary UserData { get; }

        #endregion
    }
}
