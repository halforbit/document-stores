using System;

namespace Halforbit.DocumentStores.CosmosDb.Exceptions
{
    public class HostUnreachableException : ArgumentException
    {
        public HostUnreachableException(
            string message, 
            Exception innerException) : base(message, innerException)
        {
        }
    }
}
