using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace jaMAL
{
    class jaMALException : Exception
    {
        public jaMALException()
        {
        }

        public jaMALException(string message)
            : base(message)
        {
        }

        public jaMALException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
