using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WBackup.Exceptions
{
    class MissingDiscException : Exception
    {
        public MissingDiscException()
        : base("Nie podano dysku na którym utworzyć kopie zapasową!") { }
    }
}
