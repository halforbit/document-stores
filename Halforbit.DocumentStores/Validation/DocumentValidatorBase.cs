using Halforbit.ObjectTools.Validation;
using System.Threading.Tasks;

namespace Halforbit.DocumentStores
{
    public abstract class DocumentValidatorBase<TPartitionKey, TId, TDocument> :
        IDocumentValidator<TPartitionKey, TId, TDocument>
    {
        public async Task<ValidationErrors> ValidateDelete(
            TPartitionKey partitionKey, 
            TId id)
        {
            return await ValidateDelete(partitionKey, id, ValidationErrors.Empty).ConfigureAwait(false);
        }

        public async Task<ValidationErrors> ValidatePut(
            TPartitionKey partitionKey, 
            TId id, 
            TDocument document)
        {
            return await ValidatePut(partitionKey, id, document, ValidationErrors.Empty).ConfigureAwait(false);
        }

        protected virtual async Task<ValidationErrors> ValidateDelete(
            TPartitionKey partitionKey,
            TId id,
            ValidationErrors errors) => ValidationErrors.Empty;

        protected virtual async Task<ValidationErrors> ValidatePut(
            TPartitionKey partitionKey,
            TId id,
            TDocument document,
            ValidationErrors errors) => ValidationErrors.Empty;
    }

    public abstract class DocumentValidatorBase<TId, TDocument> :
        IDocumentValidator<string, TId, TDocument>
    {
        public async Task<ValidationErrors> ValidateDelete(
            string partitionKey,
            TId id)
        {
            return await ValidateDelete(id, ValidationErrors.Empty).ConfigureAwait(false);
        }

        public async Task<ValidationErrors> ValidatePut(
            string partitionKey,
            TId id,
            TDocument document)
        {
            return await ValidatePut(id, document, ValidationErrors.Empty).ConfigureAwait(false);
        }

        protected virtual async Task<ValidationErrors> ValidateDelete(
            TId id,
            ValidationErrors errors) => ValidationErrors.Empty;

        protected virtual async Task<ValidationErrors> ValidatePut(
            TId id,
            TDocument document,
            ValidationErrors errors) => ValidationErrors.Empty;
    }

    public abstract class DocumentValidatorBase<TDocument> :
        IDocumentValidator<string, string, TDocument>
    {
        public async Task<ValidationErrors> ValidateDelete(
            string partitionKey, 
            string id)
        {
            return await ValidateDelete(ValidationErrors.Empty).ConfigureAwait(false);
        }

        public async Task<ValidationErrors> ValidatePut(
            string partitionKey,
            string id,
            TDocument document)
        {
            return await ValidatePut(document, ValidationErrors.Empty).ConfigureAwait(false);
        }

        protected virtual async Task<ValidationErrors> ValidateDelete(
            ValidationErrors errors) => ValidationErrors.Empty;

        protected virtual async Task<ValidationErrors> ValidatePut(
            TDocument document,
            ValidationErrors errors) => ValidationErrors.Empty;
    }
}
