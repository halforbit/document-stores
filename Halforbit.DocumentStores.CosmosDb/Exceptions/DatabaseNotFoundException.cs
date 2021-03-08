using System;

namespace Halforbit.DocumentStores.CosmosDb.Exceptions
{
    public class DatabaseNotFoundException : ArgumentException
    {
        public DatabaseNotFoundException(
            string message, 
            Exception innerException) : base(message, innerException)
        {
        }
    }
}
