using System;

namespace Halforbit.DocumentStores
{
    public interface IDocumentContext
    {
        IDocumentStore<TPartitionKey, TId, TDocument> Get<TPartitionKey, TId, TDocument>(
            Func<INeedsIntegration, IDocumentStoreDescription<TPartitionKey, TId, TDocument>> getDocumentStoreDescription);
            
        IDocumentStore<TId, TDocument> Get<TId, TDocument>(
            Func<INeedsIntegration, IDocumentStoreDescription<TId, TDocument>> getDocumentStoreDescription);
        
        IDocumentStore<TDocument> Get<TDocument>(
            Func<INeedsIntegration, IDocumentStoreDescription<TDocument>> getDocumentStoreDescription);
    }
}
