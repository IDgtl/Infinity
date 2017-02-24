using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity
{
    class UnHostedDistancesException : Exception
    {
        public UnHostedDistancesException(string message) : base(message)
        {
        }
    }
}
