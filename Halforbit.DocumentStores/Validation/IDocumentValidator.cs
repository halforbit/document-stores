using Halforbit.ObjectTools.Validation;
using System.Threading.Tasks;

namespace Halforbit.DocumentStores
{
    public interface IDocumentValidator { }

    public interface IDocumentValidator<TPartitionKey, TId, TDocument> : IDocumentValidator 
    {
        Task<ValidationErrors> ValidatePut(
            TPartitionKey partitionKey,
            TId id,
            TDocument document);

        Task<ValidationErrors> ValidateDelete(
            TPartitionKey partitionKey,
            TId id);
    }
}
