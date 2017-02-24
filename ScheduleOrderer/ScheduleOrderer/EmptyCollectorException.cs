using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity
{
    class EmptyCollectorException : Exception
    {
        public EmptyCollectorException(string message) : base(message)
        {
        }
    }
}
