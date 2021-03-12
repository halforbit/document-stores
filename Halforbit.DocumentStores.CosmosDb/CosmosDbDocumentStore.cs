using Halforbit.DocumentStores.CosmosDb.Exceptions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Halforbit.DocumentStores
{
    class CosmosDbDocumentStore<TPartitionKey, TId, TDocument> :
        IDocumentStore<TPartitionKey, TId, TDocument>,
        IDocumentStore<TId, TDocument>,
        IDocumentStore<TDocument>
    {
        readonly string _connectionString;
        readonly string _database;
        readonly string _container;
        readonly string _partitionKeyPath;
        readonly string _idPath;
        readonly IDocumentValidator<TPartitionKey, TId, TDocument> _documentValidator;
        Lazy<Container> _containerInstance;
        readonly bool _partitionKeyTypeMatchesIdType;
        
        public CosmosDbDocumentStore(
            string connectionString,
            string database,
            string container,
            string partitionKeyPath,
            string idPath,
            IDocumentValidator<TPartitionKey, TId, TDocument> documentValidator = null)
        {
            _connectionString = !string.IsNullOrWhiteSpace(connectionString) ? 
                connectionString : 
                throw new ArgumentException(nameof(connectionString));

            _database = !string.IsNullOrWhiteSpace(database) ? 
                database : 
                throw new ArgumentException(nameof(database));

            _container = !string.IsNullOrWhiteSpace(container) ? 
                container :
                throw new ArgumentException(nameof(container));

            _partitionKeyPath = partitionKeyPath;

            _idPath = idPath;
            
            _documentValidator = documentValidator;

            _containerInstance = new Lazy<Container>(() =>
            {
                var cosmosClient = default(CosmosClient);

                try
                {
                    cosmosClient = new CosmosClient(_connectionString);
                }
                catch (ArgumentException aex)
                {
                    throw new ConnectionStringInvalidException(
                        "Could not connect to cosmos client. Check your connection string.",
                        aex);
                }

                var database = cosmosClient.GetDatabase(_database);

                return database.GetContainer(_container);
            });

            _partitionKeyTypeMatchesIdType = typeof(TPartitionKey).Equals(typeof(TId));
        }

        public async Task CreateStoreIfNotExistsAsync()
        {
            try
            {
                var cosmosClient = new CosmosClient(_connectionString);

                var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(_database);

                var container = await database.Database.CreateContainerIfNotExistsAsync(
                    _container,
                    string.IsNullOrWhiteSpace(_partitionKeyPath) ? "/id" : _partitionKeyPath);

                _containerInstance = new Lazy<Container>(container);
            }
            catch (Exception ex)
            {
                throw await InvestigateException(ex);
            }
        }

        public async Task DeleteAsync(TPartitionKey partitionKey, TId id)
        {
            if (_documentValidator != null)
            {
                var validationErrors = await _documentValidator.ValidateDelete(partitionKey, id).ConfigureAwait(false);

                if (validationErrors?.Any() ?? false)
                {
                    throw new DocumentValidationException(validationErrors);
                }
            }

            try
            {
                var response = await _containerInstance.Value.DeleteItemAsync<TDocument>(
                    Internals.GetIdString(id),
                    GetPartitionKey(partitionKey));
            }
            catch (CosmosException cex)
            {
                if (cex.ResponseBody.Contains("Owner resource does not exist"))
                {
                    throw await InvestigateException(cex);
                }

                if (cex.StatusCode == HttpStatusCode.NotFound)
                {
                    return;
                }

                throw;
            }
            catch (Exception ex)
            {
                throw await InvestigateException(ex);
            }
        }

        public async Task<bool> ExistsAsync(TPartitionKey partitionKey, TId id) =>
            (await GetAsync(partitionKey, id)) != null;

        public async Task<TDocument> GetAsync(TPartitionKey partitionKey, TId id)
        {
            try
            {
                var response = await _containerInstance.Value.ReadItemAsync<TDocument>(
                    Internals.GetIdString(id),
                    GetPartitionKey(partitionKey));

                return CleanResultDocument(response.Resource);
            }
            catch (CosmosException cex)
            {
                if (cex.ResponseBody.Contains("Owner resource does not exist"))
                {
                    throw await InvestigateException(cex);
                }

                if (cex.StatusCode == HttpStatusCode.NotFound)
                {
                    return default;
                }

                throw;
            }
            catch (Exception ex)
            {
                throw await InvestigateException(ex);
            }
        }

        async Task<Exception> InvestigateException(Exception ex)
        {
            var cosmosClient = default(CosmosClient);

            try
            {
                cosmosClient = new CosmosClient(_connectionString);
            }
            catch (ArgumentException aex)
            {
                throw new ConnectionStringInvalidException(
                    "Could not connect to cosmos client. Check your connection string.",
                    aex);
            }

            var database = cosmosClient.GetDatabase(_database);

            try
            {
                await database.ReadThroughputAsync();
            }
            catch (CosmosException cex)
            {
                if (cex.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new DatabaseNotFoundException(
                        $"The specified database '{_database}' was not found. Create the database, or call CreateStoreIfNotExists().",
                        cex);
                }
                return cex;
            }
            catch (HttpRequestException hrex)
            {
                throw new HostUnreachableException(
                    "HTTP error while connecting to cosmos. Check your connection string. " + hrex.Message,
                    hrex);
            }

            var container = database.GetContainer(_container);

            try
            {
                await container.ReadThroughputAsync();
            }
            catch (CosmosException cex)
            {
                if (cex.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ContainerNotFoundException(
                        $"The specified container '{_container}' was not found. Create the container, or call CreateStoreIfNotExists().",
                        cex);
                }
                return cex;
            }

            return ex;
        }

        public async Task UpsertAsync(TDocument document)
        {
            var (_, id) = GetKey(document);

            var jObject = JObject.FromObject(document);

            jObject["id"] = $"{JToken.FromObject(id)}";

            var (partitionKey, _) = Internals.GetKey<TPartitionKey, TId, JObject>(
                _partitionKeyPath,
                _idPath,
                jObject);

            if (_documentValidator != null)
            {
                var validationErrors = await _documentValidator.ValidatePut(partitionKey, id, document).ConfigureAwait(false);

                if (validationErrors?.Any() ?? false)
                {
                    throw new DocumentValidationException(validationErrors);
                }
            }

            try
            {
                await _containerInstance.Value.UpsertItemAsync(
                    jObject,
                    GetPartitionKey(partitionKey));
            }
            catch (Exception ex)
            {
                throw await InvestigateException(ex);
            }
        }

        public (TPartitionKey PartitionKey, TId Id) GetKey(TDocument document) =>
            Internals.GetKey<TPartitionKey, TId, TDocument>(
                _partitionKeyPath,
                _idPath,
                document);

        public async IAsyncEnumerable<TResult> QueryAsync<TResult>(
            Func<IQueryable<TDocument>, IQueryable<TResult>> getQueryable)
        {
            var queryable = getQueryable(_containerInstance.Value.GetItemLinqQueryable<TDocument>());

            var feedIterator = queryable.ToFeedIterator();

            while (feedIterator.HasMoreResults)
            {
                var feedResponse = await feedIterator.ReadNextAsync();

                foreach (var item in feedResponse)
                {
                    yield return CleanResultDocument(item);
                }
            }
        }

        public async IAsyncEnumerable<TResult> QueryAsync<TResult>(
            TPartitionKey partitionKey,
            Func<IQueryable<TDocument>, IQueryable<TResult>> getQueryable)
        {
            var queryable = getQueryable(_containerInstance.Value.GetItemLinqQueryable<TDocument>(
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = GetPartitionKey(partitionKey)
                }));

            var feedIterator = queryable.ToFeedIterator();

            while (feedIterator.HasMoreResults)
            {
                var feedResponse = await feedIterator.ReadNextAsync();

                foreach (var item in feedResponse)
                {
                    yield return CleanResultDocument(item);
                }
            }
        }

        public async IAsyncEnumerable<TResult> QueryAsync<TResult>(
            string query,
            params (string Name, object Value)[] arguments)
        {
            var queryDefinition = new QueryDefinition(query);

            foreach (var (name, value) in arguments)
            {
                queryDefinition = queryDefinition.WithParameter(name, value);
            }

            var feedIterator = _containerInstance.Value.GetItemQueryIterator<TResult>(
                queryDefinition);

            while (feedIterator.HasMoreResults)
            {
                var feedResponse = await feedIterator.ReadNextAsync();

                foreach (var item in feedResponse)
                {
                    yield return CleanResultDocument(item);
                }
            }
        }

        public async IAsyncEnumerable<TResult> QueryAsync<TResult>(
            TPartitionKey partitionKey,
            string query,
            params (string Name, object Value)[] arguments)
        {
            var queryDefinition = new QueryDefinition(query);

            foreach (var (name, value) in arguments)
            {
                queryDefinition = queryDefinition.WithParameter(name, value);
            }

            var feedIterator = _containerInstance.Value.GetItemQueryIterator<TResult>(
                queryDefinition,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = GetPartitionKey(partitionKey)
                });

            while (feedIterator.HasMoreResults)
            {
                var feedResponse = await feedIterator.ReadNextAsync();

                foreach (var item in feedResponse)
                {
                    yield return CleanResultDocument(item);
                }
            }
        }

        public async IAsyncEnumerable<TDocument> QueryAsync(
            string query = default,
            params (string Name, object Value)[] arguments)
        {
            var queryDefinition = new QueryDefinition(query ?? "SELECT * FROM c");

            foreach (var (name, value) in arguments)
            {
                queryDefinition = queryDefinition.WithParameter(name, value);
            }

            var feedIterator = _containerInstance.Value.GetItemQueryIterator<TDocument>(
                queryDefinition);

            while (feedIterator.HasMoreResults)
            {
                var feedResponse = await feedIterator.ReadNextAsync();

                foreach (var item in feedResponse)
                {
                    yield return CleanResultDocument(item);
                }
            }
        }

        public async IAsyncEnumerable<TDocument> QueryAsync(
            TPartitionKey partitionKey,
            string query,
            params (string Name, object Value)[] arguments)
        {
            var queryDefinition = new QueryDefinition(query);

            foreach (var (name, value) in arguments)
            {
                queryDefinition = queryDefinition.WithParameter(name, value);
            }

            var feedIterator = _containerInstance.Value.GetItemQueryIterator<TDocument>(
                queryDefinition,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = GetPartitionKey(partitionKey)
                });

            while (feedIterator.HasMoreResults)
            {
                var feedResponse = await feedIterator.ReadNextAsync();

                foreach (var item in feedResponse)
                {
                    yield return CleanResultDocument(item);
                }
            }
        }

        static PartitionKey GetPartitionKey(TPartitionKey partitionKey)
        {
            if (partitionKey == null) return new PartitionKey("0");

            var j = JToken.FromObject(partitionKey);

            switch (j.Type)
            {
                case JTokenType.Guid:
                    return new PartitionKey(Convert.ToString(partitionKey));

                case JTokenType.String:
                    return new PartitionKey(Convert.ToString(partitionKey));

                case JTokenType.Boolean:
                    return new PartitionKey(Convert.ToBoolean(partitionKey));

                case JTokenType.Float:
                case JTokenType.Integer:
                    return new PartitionKey(Convert.ToDouble(partitionKey));

                default: throw new ArgumentException("Partition key is unsupported type " + j.Type);
            }
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

        //(TPartitionKey PartitionKey, TId Id) GetKey<TItem>(TItem document)
        //{
        //    var j = JToken.FromObject(document);

        //    var partitionKey = DereferencePath(
        //        j, 
        //        string.IsNullOrWhiteSpace(_partitionKeyPath) ? 
        //            "/id" : 
        //            _partitionKeyPath) ?? "0";

        //    var id = DereferencePath(j, _idPath) ?? "0";

        //    return (
        //        partitionKey.ToObject<TPartitionKey>(),
        //        id.ToObject<TId>());
        //}

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
