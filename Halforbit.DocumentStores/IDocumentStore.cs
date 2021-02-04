using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Halforbit.DocumentStores
{
    public interface IDocumentStore
    {
        IAsyncEnumerable<TDocument> EnumerateAsync<TDocument>();

        Task<TDocument> Get<TDocument>(IDocumentKey key);

        Task Upsert<TDocument>(TDocument document);

        Task Delete(IDocumentKey key);

        IQueryable<TDocument> Query<TDocument>();
    }

    public interface IDocumentStore<TDocument>
    {
        Task CreateStoreIfNotExistsAsync();

        Task UpsertAsync(TDocument document);

        Task<TDocument> GetAsync(IDocumentKey<TDocument> key);

        IAsyncEnumerable<TDocument> ListAsync(IDocumentKey<TDocument> partitionKey = default);

        IQueryable<TDocument> Query(IDocumentKey<TDocument> partitionKey = default);

        Task DeleteAsync(IDocumentKey<TDocument> key);

        IDocumentKey<TDocument> GetKey(TDocument document);
    }

    public interface IDocumentKey
    { }

    public interface IDocumentKey<TDocument>
    {
        public string Id { get; }

        public string PartitionKey { get; }
    }

    public class DocumentKey
    { }

    public class DocumentKey<TDocument> : IDocumentKey<TDocument>
    { 
        public DocumentKey(
            string partitionKey,
            string id)
        {
            PartitionKey = partitionKey;
            Id = id;
        }

        public string PartitionKey { get; }
        public string Id { get; }
    }

    public static class Extensions
    {
        public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(
            this IQueryable<TSource> source)
        {
            throw new NotImplementedException();
        }
    }
}
