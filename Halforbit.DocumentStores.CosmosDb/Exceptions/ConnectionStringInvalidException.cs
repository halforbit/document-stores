using System;

namespace Halforbit.DocumentStores.CosmosDb.Exceptions
{
    public class ConnectionStringInvalidException : ArgumentException
    {
        public ConnectionStringInvalidException(
            string message, 
            Exception innerException) : base(message, innerException)
        {
        }
    }
}
