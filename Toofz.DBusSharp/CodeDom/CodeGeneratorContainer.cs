using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace Toofz.DBusSharp.CodeDom
{
    internal sealed class CodeGeneratorContainer : IDisposable
    {
        public CodeGeneratorContainer(Language language, CodeGeneratorOptions codeGeneratorOptions)
        {
            if (language == Language.CSharp)
                CodeDomProvider = new CSharpCodeProvider();
            else if (language == Language.VisualBasic)
                CodeDomProvider = new VBCodeProvider();
            else
                throw new NotSupportedException("Only C# and Visual Basic.NET are supported.");

            CodeGeneratorOptions = codeGeneratorOptions;
        }

        public CodeDomProvider CodeDomProvider { get; private set; }
        public CodeGeneratorOptions CodeGeneratorOptions { get; private set; }

        public void GenerateCode(CodeCompileUnit compileUnit, TextWriter writer)
        {
            CodeDomProvider.GenerateCodeFromCompileUnit(compileUnit, writer, CodeGeneratorOptions);
        }

        public CompilerResults CompileAssembly(CompilerParameters options, params string[] fileNames)
        {
            return CodeDomProvider.CompileAssemblyFromFile(options, fileNames);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (CodeDomProvider != null)
                CodeDomProvider.Dispose();
        }

        #endregion
    }
}
