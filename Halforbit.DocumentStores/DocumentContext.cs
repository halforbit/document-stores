using System;
using System.Collections.Concurrent;

namespace Halforbit.DocumentStores
{
    public class DocumentContext : IDocumentContext
    {
        readonly ConcurrentDictionary<object, object> _cache = new ConcurrentDictionary<object, object>();

        public IDocumentStore<TPartitionKey, TId, TDocument> Get<TPartitionKey, TId, TDocument>(
            Func<INeedsIntegration, IDocumentStoreDescription<TPartitionKey, TId, TDocument>> getDocumentStoreDescription)
        {
            if (_cache.TryGetValue(getDocumentStoreDescription, out var cacheHit))
            {
                return (IDocumentStore<TPartitionKey, TId, TDocument>)cacheHit;
            }

            var instance = getDocumentStoreDescription(DocumentStore.Describe()).Build();

            _cache[getDocumentStoreDescription] = instance;

            return instance;
        }

        public IDocumentStore<TId, TDocument> Get<TId, TDocument>(
            Func<INeedsIntegration, IDocumentStoreDescription<TId, TDocument>> getDocumentStoreDescription)
        {
            if (_cache.TryGetValue(getDocumentStoreDescription, out var cacheHit))
            {
                return (IDocumentStore<TId, TDocument>)cacheHit;
            }

            var instance = getDocumentStoreDescription(DocumentStore.Describe()).Build();

            _cache[getDocumentStoreDescription] = instance;

            return instance;
        }

        public IDocumentStore<TDocument> Get<TDocument>(
            Func<INeedsIntegration, IDocumentStoreDescription<TDocument>> getDocumentStoreDescription)
        {
            if (_cache.TryGetValue(getDocumentStoreDescription, out var cacheHit))
            {
                return (IDocumentStore<TDocument>)cacheHit;
            }

            var instance = getDocumentStoreDescription(DocumentStore.Describe()).Build();

            _cache[getDocumentStoreDescription] = instance;

            return instance;
        }
    }
}
