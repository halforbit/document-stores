using Halforbit.DocumentStores.CosmosDb.Exceptions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Halforbit.DocumentStores.Tests
{
    public class CosmosDbDocumentStoreTests_Failure
    {
        // TODO:
        // Container does not exist
        // Partition key mismatch

        [Fact]
        public async Task InvalidConnectionString()
        {
            var documentStore = new CosmosDbDocumentStore<string, Guid, Person_String_Guid>(
                connectionString: "total-garbage",
                database: "whatever",
                container: "whatever",
                partitionKeyPath: "whatever",
                idPath: "whatever");

            await Assert.ThrowsAsync<ConnectionStringInvalidException>(
                async () => await documentStore.ExistsAsync("0", Guid.Empty));
        }

        [Fact]
        public async Task ConnectionString_HostUnreachable()
        {
            var documentStore = new CosmosDbDocumentStore<string, Guid, Person_String_Guid>(
                connectionString: "AccountEndpoint=https://somewhere.doesnt.exist.nope:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                database: "whatever",
                container: "whatever",
                partitionKeyPath: "/id",
                idPath: "/id");

            await Assert.ThrowsAsync<HostUnreachableException>(
                async () => await documentStore.ExistsAsync("0", Guid.Empty));
        }

        [Fact]
        public async Task DatabaseDoesNotExist()
        {
            var documentStore = new CosmosDbDocumentStore<string, Guid, Person_String_Guid>(
                connectionString: TestValues.CosmosDbConnectionString,
                database: "does-not-exist",
                container: "whatever",
                partitionKeyPath: "/id",
                idPath: "/id");

            await Assert.ThrowsAsync<DatabaseNotFoundException>(
                async () => await documentStore.ExistsAsync("0", Guid.Empty));
        }
    }
}
