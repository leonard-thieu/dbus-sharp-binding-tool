using System;

namespace Toofz.DBusSharp
{
    internal sealed class Language
    {
        public static readonly Language CSharp = new Language("C#", "CSharp", "CS", ".cs", new CSharpPrettyPrinter());
        public static readonly Language VisualBasic = new Language("Visual Basic .NET", "VisualBasic", "VB", ".vb", new VBPrettyPrinter());
        public static readonly LanguageCollection SupportedLanguages = new LanguageCollection { CSharp, VisualBasic };

        public static Language GetLanguage(string languageOption)
        {
            if (string.IsNullOrWhiteSpace(languageOption))
                return null;

            if (languageOption == CSharp.Option)
                return CSharp;
            if (languageOption == VisualBasic.Option)
                return VisualBasic;
            return null;
        }

        public Language(string name, string safeName, string option, string fileExtension, IPrettyPrinter prettyPrinter)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace(safeName))
                throw new ArgumentNullException("safeName");
            if (string.IsNullOrWhiteSpace(option))
                throw new ArgumentNullException("option");
            if (string.IsNullOrWhiteSpace(fileExtension))
                throw new ArgumentNullException("fileExtension");

            Name = name;
            SafeName = safeName;
            Option = option;
            FileExtension = fileExtension;
            PrettyPrinter = prettyPrinter ?? NullPrettyPrinter.Instance;
        }

        public string Name { get; private set; }
        public string SafeName { get; private set; }
        public string Option { get; private set; }
        public string FileExtension { get; private set; }
        public IPrettyPrinter PrettyPrinter { get; private set; }
    }
}
