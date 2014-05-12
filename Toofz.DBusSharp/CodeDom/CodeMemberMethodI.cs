using System.CodeDom;

namespace Toofz.DBusSharp.CodeDom
{
    internal sealed class CodeMemberMethodI : CodeMemberMethod, ICodeDelegate
    {
        public CodeMemberMethodI(string name)
        {
            Name = name;
            Attributes = MemberAttributes.Abstract;
        }
    }
}
