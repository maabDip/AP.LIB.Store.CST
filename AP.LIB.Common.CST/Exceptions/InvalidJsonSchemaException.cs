using System;

namespace AP.LIB.Common.CST.Exceptions
{
    public class InvalidJsonSchemaException : Exception
    {
        public InvalidJsonSchemaException()
        {

        }

        public InvalidJsonSchemaException(string messages)
            : base(String.Format("{0}", messages))
        {

        }

    }
}
