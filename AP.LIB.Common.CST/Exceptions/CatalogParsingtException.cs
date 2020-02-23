using System;

namespace AP.LIB.Common.CST.Exceptions
{
    public class CatalogParsingtException : Exception
    {
        public CatalogParsingtException()
        {

        }

        public CatalogParsingtException(string codeError)
            : base(String.Format("Null catalog Object Parsing Error: {0}", codeError))
        {

        }

    }
}
