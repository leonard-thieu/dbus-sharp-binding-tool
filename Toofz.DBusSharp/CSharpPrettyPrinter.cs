﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Toofz.DBusSharp
{
    internal sealed class CSharpPrettyPrinter : IPrettyPrinter
    {
        string IPrettyPrinter.Print(TextReader output)
        {
            return CSharpPrettyPrinter.Print(output);
        }

        // TODO: Yes, this is a horrible mess and needs to be done better.
        public static string Print(TextReader output)
        {
            var nl = Environment.NewLine;
            var opt = RegexOptions.Multiline | RegexOptions.Singleline;

            var contents = output.ReadToEnd();
            contents = Regex.Replace(contents, @"^\s*$\n?", string.Empty, opt);             // Remove blank lines
            contents = Regex.Replace(contents, @"(//------------------------------------------------------------------------------)\s*(using)",
                                                "$1" + nl +
                                                "// Generated by dbus-sharp-binding-tool" + nl +
                                                "//     https://github.com/leonard-thieu/dbus-sharp-binding-tool" + nl +
                                                "$1" + nl + nl +
                                                "$2");                                      // Insert header below provider-generated header
            contents = Regex.Replace(contents, @"(^namespace)", nl + "$1", opt);            // Insert line before namespaces
            contents = Regex.Replace(contents, @"}(\s*\[)", "}" + nl + "$1");               // Insert line between type declarations
            contents = Regex.Replace(contents, @"\s*{\s*get;\s*set;\s*}", " { get; set; }");
            contents = Regex.Replace(contents, @"(get|set)\s*{\s*(.*;)\s*}", "$1 { $2 }");  // Format property as single line
            contents = Regex.Replace(contents, @"\(\)]", "]");                              // Remove empty parentheses from attribute declarations
            contents = Regex.Replace(contents, @"(using.*;)(\s*\[)",
                                                "$1" + nl + "$2");                          // Insert line after inner namespace imports

            return contents;
        }
    }
}
