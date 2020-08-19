using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WBackup.Exceptions
{
    class MissingPathException : Exception
    {
        public MissingPathException() : base("Brak ustawionej i utworzonej ściezki do kopii zapasowych.") { }
    }
}
