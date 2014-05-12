using System.Collections.Generic;

namespace Toofz.DBusSharp.CodeDom
{
    internal sealed class CompilationResults
    {
        public CompilationResults(string pathToAssembly, IEnumerable<string> errors)
        {
            PathToAssembly = pathToAssembly;
            Errors = errors;
        }

        public string PathToAssembly { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
