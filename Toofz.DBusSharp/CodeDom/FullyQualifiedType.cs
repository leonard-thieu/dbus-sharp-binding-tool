using System.CodeDom;

namespace Toofz.DBusSharp.CodeDom
{
    internal sealed class FullyQualifiedType
    {
        public FullyQualifiedType(string name)
        {
            TypeName = name;

            var lastDot = name.LastIndexOf('.');
            if (lastDot != -1)
            {
                Namespace = new CodeNamespace(name.Substring(0, lastDot));
                TypeName = name.Substring(lastDot + 1);
            }
        }

        public CodeNamespace Namespace { get; set; }
        public string TypeName { get; set; }
    }
}
