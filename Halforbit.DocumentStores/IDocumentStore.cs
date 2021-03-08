using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Halforbit.DocumentStores
{
    public interface IDocumentStore<TPartitionKey, TId, TDocument>
    {
        Task CreateStoreIfNotExistsAsync();

        Task UpsertAsync(TDocument document);

        Task<bool> ExistsAsync(TPartitionKey partitionKey, TId id);

        Task<TDocument> GetAsync(TPartitionKey partitionKey, TId id);

        Task DeleteAsync(TPartitionKey partitionKey, TId id);

        (TPartitionKey PartitionKey, TId Id) GetKey(TDocument document);

        IAsyncEnumerable<TResult> QueryAsync<TResult>(
            Func<IQueryable<TDocument>, IQueryable<TResult>> getQueryable);

        IAsyncEnumerable<TResult> QueryAsync<TResult>(
            TPartitionKey partitionKey,
            Func<IQueryable<TDocument>, IQueryable<TResult>> getQueryable);

        IAsyncEnumerable<TResult> QueryAsync<TResult>(
            string query,
            params (string Name, object Value)[] arguments);

        IAsyncEnumerable<TResult> QueryAsync<TResult>(
            TPartitionKey partitionKey,
            string query,
            params (string Name, object Value)[] arguments);

        IAsyncEnumerable<TDocument> QueryAsync(
            string query = default,
            params (string Name, object Value)[] arguments);

        IAsyncEnumerable<TDocument> QueryAsync(
            TPartitionKey partitionKey,
            string query,
            params (string Name, object Value)[] arguments);
    }

    public interface IDocumentStore<TId, TDocument>
    {
        Task CreateStoreIfNotExistsAsync();

        Task UpsertAsync(TDocument document);

        Task<bool> ExistsAsync(TId id);

        Task<TDocument> GetAsync(TId id);

        IAsyncEnumerable<TResult> QueryAsync<TResult>(
            Func<IQueryable<TDocument>, IQueryable<TResult>> getQueryable);

        IAsyncEnumerable<TResult> QueryAsync<TResult>(
            string query,
            params (string Name, object Value)[] arguments);

        IAsyncEnumerable<TDocument> QueryAsync(
            string query = default,
            params (string Name, object Value)[] arguments);

        Task DeleteAsync(TId id);

        TId GetKey(TDocument document);
    }

    public interface IDocumentStore<TDocument>
    {
        Task CreateStoreIfNotExistsAsync();

        Task UpsertAsync(TDocument document);

        Task<bool> ExistsAsync();

        Task<TDocument> GetAsync();

        Task DeleteAsync();
    }
}
