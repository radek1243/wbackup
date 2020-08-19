using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WBackup.Exceptions
{
    class MissingDirectoryException : Exception
    {
        public MissingDirectoryException() : base("Brak utworzonego katalogu wbackup w docelowej lokalizacji.") { }
    }
}
