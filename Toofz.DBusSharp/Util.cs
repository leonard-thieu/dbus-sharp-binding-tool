using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Toofz.DBusSharp
{
    internal static class Util
    {
        public static string Join(IEnumerable<string> list, string conjunction)
        {
            var newList = list.ToArray();
            switch (newList.Length)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return newList[0];
                case 2:
                    // Bob and Bobbette
                    return string.Format("{0} {1} {2}", newList[0], conjunction, newList[1]);
                default:
                    // Bob, Bobbette, and Bobita
                    var rev = list.Reverse();
                    newList[newList.Length - 1] = string.Format("{0} {1} {2}", rev.Skip(1).Take(1), conjunction, rev.First());
                    return string.Join(", ", newList);
            }
        }

        /// <summary>
        /// Returns the relative path for the specified path string. The path will be 
        /// relative to the current directory.
        /// </summary>
        /// <param name="path">
        /// The file or directory for which to obtain relative path information.
        /// </param>
        /// <returns>
        /// A string containing the relative location of <paramref name="path" />, such as 
        /// ".\MyFile.txt".
        /// </returns>
        public static string GetRelativePath(string path)
        {
            path = Path.GetFullPath(path);
            var pathUri = new Uri(path);
            var folder = Environment.CurrentDirectory;

            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
                folder += Path.DirectorySeparatorChar;

            var folderUri = new Uri(folder);

            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString()
                      .Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
