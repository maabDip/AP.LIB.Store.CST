using System;

namespace AP.LIB.Common.CST.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException()
        {

        }

        public BadRequestException(string codeError)
            : base(String.Format("Invalid Request json : {0}", codeError))
        {

        }

    }
}
