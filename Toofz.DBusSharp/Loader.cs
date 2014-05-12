using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Toofz.DBusSharp
{
    internal static class Loader
    {
        private static readonly Dictionary<string, Assembly> Libraries = new Dictionary<string, Assembly>();

        internal static readonly Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();

        internal static Assembly FindAssembly(object sender, ResolveEventArgs e)
        {
            var simpleName = new AssemblyName(e.Name).Name;
            if (Libraries.ContainsKey(simpleName))
                return Libraries[simpleName];

            using (var s = ExecutingAssembly.GetManifestResourceStream(
                string.Format(CultureInfo.InvariantCulture,
                "{0}.{1}.{2}",
                typeof(Program).Namespace, simpleName, "dll")))
            {
                var data = new BinaryReader(s).ReadBytes((int)s.Length);
                var assembly = Assembly.Load(data);
                Libraries[simpleName] = assembly;

                return assembly;
            }
        }
    }
}
