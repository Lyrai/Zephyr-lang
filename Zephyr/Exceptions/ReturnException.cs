using System;
using Zephyr.Interpreting;

namespace Zephyr
{
    public class ReturnException : Exception
    {
        public RuntimeValue Value { get; }

        public ReturnException(object value)
        {
            Value = new(value);
        }
    }
}