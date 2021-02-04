using Halforbit.DocumentStores.CosmosDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Halforbit.DocumentStores.Tests
{
    public class CosmosDbDocumentStoreTests
    {
        [Fact]
        public async Task Test1()
        {
            var store = new CosmosDbDocumentStore<Person>(
                connectionString: "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
                databaseName: "document-stores-test-database",
                containerName: "document-stores-test-container",
                idPath: "/PersonId",
                partitionKeyPath: "/PersonId") as IDocumentStore<Person>;

            await store.CreateStoreIfNotExistsAsync();

            var document = new Person(
                Guid.NewGuid(),
                "Steve",
                "Smith",
                new DateTime(1994, 01, 02, 00, 00, 00, DateTimeKind.Utc));

            await store.UpsertAsync(document);

            await store.UpsertAsync(document);

            var key = store.GetKey(document);

            var person = await store.GetAsync(key);

            var persons = new List<Person>();
            
            await foreach (var p in store.ListAsync())
            {
                persons.Add(p);
            }

            await foreach (var p in store.Query(key)
                .Where(p => p.DateOfBirth > DateTime.UtcNow.AddYears(-21))
                .ToAsyncEnumerable())
            {
                persons.Add(p);
            }

            var oldPersons = await store.Query()
                .Where(p => p.DateOfBirth < DateTime.UtcNow.AddYears(-21))
                .ToListAsync();

            await store.DeleteAsync(key);

            await store.DeleteAsync(key);

            // get a key from a document

            // specify a naked key

            // specify just the partition of a key
        }
    }

    public record Person(
        Guid PersonId,
        string FirstName,
        string LastName,
        DateTime DateOfBirth);
}
