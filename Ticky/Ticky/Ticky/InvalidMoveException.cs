using System;
using System.Collections.Generic;
using System.Text;

namespace Ticky
{
    public class InvalidMoveException : Exception
    {
        public InvalidMoveException(string message) : base(message) { }
    }
}
