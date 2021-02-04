using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Halforbit.DocumentStores.CosmosDb
{
    public class CosmosDbDocumentStore<TDocument> : IDocumentStore<TDocument>
    {
        readonly string _connectionString;
        readonly string _databaseName;
        readonly string _containerName;
        readonly string _idPath;
        readonly string _partitionKeyPath;
        Lazy<Container> _container;

        public CosmosDbDocumentStore(
            string connectionString,
            string databaseName,
            string containerName,
            string idPath,
            string partitionKeyPath)
        {
            _connectionString = connectionString;
            
            _databaseName = databaseName;

            _containerName = containerName;

            _idPath = idPath;

            _partitionKeyPath = partitionKeyPath;

            _container = new Lazy<Container>(() =>
            {
                var cosmosClient = new CosmosClient(_connectionString);

                var database = cosmosClient.GetDatabase(_databaseName);

                return database.GetContainer(_containerName);
            });
        }

        public async Task CreateStoreIfNotExistsAsync()
        {
            var cosmosClient = new CosmosClient(_connectionString);

            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName);

            var container = await database.Database.CreateContainerIfNotExistsAsync(
                _containerName, 
                _partitionKeyPath);

            _container = new Lazy<Container>(container);
        }

        public async Task DeleteAsync(IDocumentKey<TDocument> key)
        {
            try
            {
                var response = await _container.Value.DeleteItemAsync<TDocument>(
                    key.Id,
                    new PartitionKey(key.PartitionKey));
            }
            catch(CosmosException cex)
            {
                if (cex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return;
                }

                throw;
            }
        }

        public async Task<TDocument> GetAsync(IDocumentKey<TDocument> key)
        {
            return await _container.Value.ReadItemAsync<TDocument>(
                key.Id, 
                new PartitionKey(key.PartitionKey));
        }

        public IAsyncEnumerator<TDocument> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return EnumerateDocumentsAsync().GetAsyncEnumerator(cancellationToken);
        }

        public async Task UpsertAsync(TDocument document)
        {
            var jObject = JObject.FromObject(document);

            jObject["id"] = DereferencePath(jObject, _idPath);

            var response = await _container.Value.UpsertItemAsync(
                jObject,
                new PartitionKey((string)DereferencePath(jObject, _partitionKeyPath)));
        }

        public IDocumentKey<TDocument> GetKey(TDocument document)
        {
            var j = JToken.FromObject(document);

            return new DocumentKey<TDocument>(
                (string)DereferencePath(j, _partitionKeyPath),
                (string)DereferencePath(j, _idPath));
        }

        public IQueryable<TDocument> Query(IDocumentKey<TDocument> partitionKey = null)
        {
            return _container.Value.GetItemLinqQueryable<TDocument>(
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = partitionKey != null ?
                        new PartitionKey(partitionKey.PartitionKey) :
                        PartitionKey.None
                });            
        }

        public async IAsyncEnumerable<TDocument> ListAsync(
            IDocumentKey<TDocument> partitionKey = null)
        {
            var feedIterator = _container.Value.GetItemQueryIterator<TDocument>(
                queryText: "SELECT * FROM c",
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = partitionKey != null ?
                        new PartitionKey(partitionKey.PartitionKey) :
                        PartitionKey.None
                });

            while (feedIterator.HasMoreResults)
            {
                var feedResponse = await feedIterator.ReadNextAsync();

                foreach (var item in feedResponse)
                {
                    yield return item;
                }
            }
        }

        async IAsyncEnumerable<TDocument> EnumerateDocumentsAsync()
        {
            var feedIterator = _container.Value.GetItemQueryIterator<TDocument>("SELECT * FROM c");

            while (feedIterator.HasMoreResults)
            {
                var feedResponse = await feedIterator.ReadNextAsync();

                foreach (var item in feedResponse)
                {
                    yield return item;
                }
            }
        }

        static JToken DereferencePath(JToken jToken, string path)
        {
            var value = jToken;

            foreach (var part in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                value = value[part];
            }

            return value;
        }
    }
}
