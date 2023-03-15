using System;

namespace Zephyr
{
    public class GeneralException : Exception
    {
        public override string Message { get; }

        public GeneralException(string message)
        {
            Message = message;
        }

        public GeneralException()
        {
            Message = "General exception";
        }
    }
}