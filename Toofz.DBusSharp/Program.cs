using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Mono.Options;
using Toofz.DBusSharp.CodeDom;
using Toofz.DBusSharp.Properties;

namespace Toofz.DBusSharp
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += Loader.FindAssembly;
            var appLocation = Loader.ExecutingAssembly.Location;
            AppName = Path.GetFileNameWithoutExtension(appLocation);

            string language = null;
            var prettyPrint = true;
            var showBanner = true;
            var showHelp = false;
            var showVersion = false;

            var optionSet = new OptionSet
            {
                { "l|language=", string.Format("Sets the output language ({0}). The default is CS.", Language.SupportedLanguages.Options), v => language = v },
                { "p|pretty", "Pretty prints the output. The default is on.", v => prettyPrint = v != null },
                { "nologo", "Suppresses the startup banner.", v => showBanner = v != null },
                { "h|?|help", "Lists all generator options.", v => showHelp = v != null },
                { "v|version", "Display the version number of the dbus-binding-tool command.", v => showVersion = v != null}
            };

            List<string> files;
            try
            {
                files = optionSet.Parse(args);
                Language = Language.GetLanguage(language) ?? DefaultLanguage;
            }
            catch (OptionException ex)
            {
                WriteError(ex.Message);
                return;
            }

            if (showBanner && !showVersion)
            {
                ShowBanner();
                Console.WriteLine();
            }

            if (showHelp)
            {
                ShowHelp(optionSet);
                return;
            }

            if (showVersion)
            {
                ShowBanner();
                return;
            }

#if DEBUG
            if (files.Count == 0)
                files = Directory.GetFiles(".", "*.xml").ToList();
#endif
            if (files.Count == 0)
            {
                WriteError(Resources.NoFileNamesGiven);
                return;
            }

            Messenger.Message += new EventHandler<MessageEventArgs>(MessageEventHandler);
            Generate(files, prettyPrint);

            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static string AppName;
        private static Language DefaultLanguage = Language.CSharp;
        private static Language Language;

        private static void ShowBanner()
        {
            Console.WriteLine(Resources.Banner);
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DBusSharp")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object)")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
        private static void ShowHelp(OptionSet optionSet)
        {
            Console.WriteLine("Usage: {0} [OPTIONS]+ introspection-files", AppName);
            Console.WriteLine("Generates code for use with DBusSharp from introspection XML files.");
            Console.WriteLine("Supported output languages: {0}", Language.SupportedLanguages.Names);
            Console.WriteLine();
            Console.WriteLine("Options:");
            optionSet.WriteOptionDescriptions(Console.Out);
        }

        private static void WriteError(string message)
        {
            Console.Write("{0}: ", AppName);
            Console.WriteLine(message);
            Console.WriteLine("Try '{0} --help' for more information.", AppName);
        }

        private static void MessageEventHandler(object sender, MessageEventArgs e)
        {
            switch (e.Severity)
            {
                case TraceEventType.Information:
                    Console.WriteLine("[Information] {0}", e.Message);
                    break;
                case TraceEventType.Warning:
                    Console.WriteLine("    [Warning] {0}", e.Message);
                    break;
                case TraceEventType.Critical:
                case TraceEventType.Error:
                case TraceEventType.Resume:
                case TraceEventType.Start:
                case TraceEventType.Stop:
                case TraceEventType.Suspend:
                case TraceEventType.Transfer:
                case TraceEventType.Verbose:
                default:
                    break;
            }
        }

        private static void Generate(List<string> files, bool prettyPrint)
        {
            foreach (var file in files)
            {
                IntrospectionReader reader;
                using (var tr = File.OpenRead(file))
                    reader = new IntrospectionReader(tr);

                string prettyCode;
                var generated = reader.Generate(Language);
                using (var tr = new StreamReader(generated))
                    prettyCode = prettyPrint ?
                        Language.PrettyPrinter.Print(tr) : NullPrettyPrinter.Instance.Print(tr);

                if (prettyCode.Length == 0)
                {
                    Console.WriteLine(Resources.NoTypesFound, file);
                    continue;
                }

                var outFile = Path.ChangeExtension(file, Language.FileExtension);

                Console.WriteLine(Resources.Creating, Util.GetRelativePath(outFile));
                using (var fw = new StreamWriter(outFile, false))
                    fw.Write(prettyCode);

                Compile(Language, outFile);
            }
        }

        private static void Compile(Language language, string fileName)
        {
            var results = IntrospectionReader.Compile(language, fileName);

            if (results.Errors.Any())
            {
                // Display compilation errors.
                var originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                {
                    Console.WriteLine(Resources.Creating, Util.GetRelativePath(results.PathToAssembly));
                    Console.WriteLine();

                    foreach (var error in results.Errors)
                    {
                        Console.WriteLine(error);
                        Console.WriteLine();
                    }
                }
                Console.ForegroundColor = originalColor;

                Console.WriteLine(Resources.PressAnyKeyToContinue);
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine(Resources.Creating, Util.GetRelativePath(results.PathToAssembly));
            }
        }
    }
}
