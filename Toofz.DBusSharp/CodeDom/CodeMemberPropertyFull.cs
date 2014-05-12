using System.CodeDom;

namespace Toofz.DBusSharp.CodeDom
{
    internal sealed class CodeMemberPropertyFull : CodeMemberProperty
    {
        private CodeMemberField _BackingField;
        public CodeMemberField BackingField
        {
            get
            {
                return _BackingField ?? (_BackingField =
                    new CodeMemberField(Type, "_" + Name) { Attributes = MemberAttributes.Private });
            }
        }
    }
}
