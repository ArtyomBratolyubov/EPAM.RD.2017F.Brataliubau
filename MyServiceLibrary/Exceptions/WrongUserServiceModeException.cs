using System;

namespace ServiceLibrary.Exceptions
{
    public class WrongUserServiceModeException : Exception
    {
        public WrongUserServiceModeException()
        {
        }

        public WrongUserServiceModeException(string message)
            : base(message)
        {
        }

        public WrongUserServiceModeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}