using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Halforbit.DocumentStores
{
    class MockDocumentStore<TPartitionKey, TId, TDocument> : 
        IDocumentStore<TPartitionKey, TId, TDocument>,
        IDocumentStore<TId, TDocument>,
        IDocumentStore<TDocument>
    {
        readonly string _partitionKeyPath;
        readonly string _idPath;
        readonly ConcurrentDictionary<(TPartitionKey PartitionKey, TId Id), JObject> _documents;
        readonly bool _partitionKeyTypeMatchesIdType;

        public MockDocumentStore(
            string partitionKeyPath,
            string idPath,
            IEnumerable<TDocument> documents = default)
        {
            _partitionKeyPath = partitionKeyPath;
            
            _idPath = idPath;

            _documents = new ConcurrentDictionary<(TPartitionKey PartitionKey, TId Id), JObject>((documents ?? new List<TDocument>())
                .Select(d => new KeyValuePair<(TPartitionKey PartitionKey, TId Id), JObject>(GetKey(d), JObject.FromObject(d))));

            _partitionKeyTypeMatchesIdType = typeof(TPartitionKey).Equals(typeof(TId));
        }

        public Task CreateStoreIfNotExistsAsync() => Task.CompletedTask;

        public Task DeleteAsync(
            TPartitionKey partitionKey, 
            TId id)
        {
            _documents.TryRemove((partitionKey, id), out var _);

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(
            TPartitionKey partitionKey, 
            TId id)
        {
            return Task.FromResult(_documents.ContainsKey((partitionKey, id)));
        }

        public Task<TDocument> GetAsync(
            TPartitionKey partitionKey, 
            TId id)
        {
            return Task.FromResult(_documents.TryGetValue((partitionKey, id), out var value) ? 
                CleanResultDocument(value).ToObject<TDocument>() : 
                default);
        }

        public (TPartitionKey PartitionKey, TId Id) GetKey(TDocument document) =>
            Internals.GetKey<TPartitionKey, TId, TDocument>(
                _partitionKeyPath,
                _idPath,
                document);

        public async IAsyncEnumerable<TResult> QueryAsync<TResult>(
            Func<IQueryable<TDocument>, IQueryable<TResult>> getQueryable)
        {
            var queryable = getQueryable(_documents
                .Select(d => d.Value.ToObject<TDocument>())
                .AsQueryable());

            foreach (var item in queryable)
            {
                yield return item;
            }
        }

        public IAsyncEnumerable<TResult> QueryAsync<TResult>(
            TPartitionKey partitionKey,
            Func<IQueryable<TDocument>, IQueryable<TResult>> getQueryable) => QueryAsync(getQueryable);

        public IAsyncEnumerable<TResult> QueryAsync<TResult>(
            string query, 
            params (string Name, object Value)[] arguments)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TResult> QueryAsync<TResult>(
            TPartitionKey partitionKey, 
            string query, 
            params (string Name, object Value)[] arguments)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TDocument> QueryAsync(
            string query = null, 
            params (string Name, object Value)[] arguments)
        {
            if (query == null)
            {
                return _documents.Values
                    .Select(v => v.ToObject<TDocument>())
                    .ToAsyncEnumerable();
            }

            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TDocument> QueryAsync(
            TPartitionKey partitionKey, 
            string query, 
            params (string Name, object Value)[] arguments)
        {
            throw new NotImplementedException();
        }

        public Task UpsertAsync(TDocument document)
        {
            var (_, id) = GetKey(document);

            var jObject = JObject.FromObject(document);

            jObject["id"] = $"{JToken.FromObject(id)}";

            var (partitionKey, _) = Internals.GetKey<TPartitionKey, TId, JObject>(
                _partitionKeyPath,
                _idPath,
                jObject);

            _documents[(partitionKey, id)] = jObject;

            return Task.CompletedTask;
        }

        static TResult CleanResultDocument<TResult>(TResult result)
        {
            if (result is JObject jObject)
            {
                jObject.Remove("id");
                jObject.Remove("_attachments");
                jObject.Remove("_etag");
                jObject.Remove("_rid");
                jObject.Remove("_self");
                jObject.Remove("_ts");
            }

            return result;
        }

        // ID PARTITIONED /////////////////////////////////////////////////////

        async Task<bool> IDocumentStore<TId, TDocument>.ExistsAsync(TId id)
        {
            if (_partitionKeyTypeMatchesIdType)
            {
                return await ExistsAsync((TPartitionKey)(object)id, id);
            }

            return await ExistsAsync((TPartitionKey)(object)Internals.GetIdString(id), id);
        }

        async Task<TDocument> IDocumentStore<TId, TDocument>.GetAsync(TId id)
        {
            if (_partitionKeyTypeMatchesIdType)
            {
                return await GetAsync((TPartitionKey)(object)id, id);
            }

            return await GetAsync((TPartitionKey)(object)Internals.GetIdString(id), id);
        }

        async Task IDocumentStore<TId, TDocument>.DeleteAsync(TId id)
        {
            if (_partitionKeyTypeMatchesIdType)
            {
                await DeleteAsync((TPartitionKey)(object)id, id);
            }
            else
            {
                await DeleteAsync((TPartitionKey)(object)Internals.GetIdString(id), id);
            }
        }

        TId IDocumentStore<TId, TDocument>.GetKey(TDocument document) => GetKey(document).Id;

        // SINGLETON //////////////////////////////////////////////////////////

        async Task<bool> IDocumentStore<TDocument>.ExistsAsync() => await ExistsAsync(
            (TPartitionKey)(object)"0", 
            (TId)(object)"0");

        async Task<TDocument> IDocumentStore<TDocument>.GetAsync() => await GetAsync(
            (TPartitionKey)(object)"0", 
            (TId)(object)"0");

        async Task IDocumentStore<TDocument>.DeleteAsync() => await DeleteAsync(
            (TPartitionKey)(object)"0", 
            (TId)(object)"0");
    }
}
