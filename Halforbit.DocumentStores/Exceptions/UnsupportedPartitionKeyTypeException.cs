using System;

namespace Halforbit.DocumentStores.Exceptions
{
    public class UnsupportedPartitionKeyTypeException : Exception
    {
        public UnsupportedPartitionKeyTypeException(
            string message) : base(message)
        { }
    }
}
