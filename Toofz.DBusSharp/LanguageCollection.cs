using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Toofz.DBusSharp
{
    internal class LanguageCollection : Collection<Language>
    {
        public IEnumerable<string> Names
        {
            get { return this.Select(l => l.Name); }
        }

        public IEnumerable<string> SafeNames
        {
            get { return this.Select(l => l.SafeName); }
        }

        public IEnumerable<string> Options
        {
            get { return this.Select(l => l.Option); }
        }

        public IEnumerable<string> FileExtensions
        {
            get { return this.Select(l => l.FileExtension); }
        }
    }
}
