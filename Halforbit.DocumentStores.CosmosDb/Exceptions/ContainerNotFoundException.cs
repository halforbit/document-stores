using System;

namespace Halforbit.DocumentStores.CosmosDb.Exceptions
{
    public class ContainerNotFoundException : ArgumentException
    {
        public ContainerNotFoundException(
            string message, 
            Exception innerException) : base(message, innerException)
        {
        }
    }
}
