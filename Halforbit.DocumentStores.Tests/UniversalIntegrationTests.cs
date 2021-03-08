using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Halforbit.DocumentStores.Tests
{
    static class UniversalIntegrationTests
    {
        public static async Task TestPartitionKeyedDocumentStore<TPartitionKey, TId, TDocument>(
            IDocumentStore<TPartitionKey, TId, TDocument> store,
            TPartitionKey testPartitionKey,
            TId testId,
            TDocument testValueA,
            TDocument testValueB, 
            Func<TDocument, TDocument, bool> areEqual = default)
        {
            areEqual ??= (a, b) => a.Equals(b);

            await store.CreateStoreIfNotExistsAsync();

            await foreach (var item in store.QueryAsync())
            {
                var (partitionKey, id) = store.GetKey(item);

                await store.DeleteAsync(partitionKey, id);
            }

            var preExistsResult = await store.ExistsAsync(testPartitionKey, testId);

            Assert.False(preExistsResult);

            var preGetResult = await store.GetAsync(testPartitionKey, testId);

            Assert.Null(preGetResult);

            var preListResult = await store.QueryAsync().ToListAsync();

            Assert.False(preListResult.Any());

            await store.UpsertAsync(testValueA);

            var firstExistsResult = await store.ExistsAsync(testPartitionKey, testId);

            Assert.True(firstExistsResult);

            var firstGetResult = await store.GetAsync(testPartitionKey, testId);

            Assert.True(areEqual(testValueA, firstGetResult));

            var (firstGetPartitionKey, firstGetId) = store.GetKey(firstGetResult);

            Assert.Equal(testPartitionKey, firstGetPartitionKey);

            Assert.Equal(testId, firstGetId);

            var firstListResult = await store.QueryAsync().ToListAsync();

            Assert.True(areEqual(testValueA, firstListResult.Single()));

            await store.UpsertAsync(testValueB);

            var secondExistsResult = await store.ExistsAsync(testPartitionKey, testId);

            Assert.True(secondExistsResult);

            var secondGetResult = await store.GetAsync(testPartitionKey, testId);

            Assert.True(areEqual(testValueB, secondGetResult));

            await store.UpsertAsync(testValueA);

            var thirdGetResult = await store.GetAsync(testPartitionKey, testId);

            Assert.True(areEqual(testValueA, thirdGetResult));

            await store.DeleteAsync(testPartitionKey, testId);

            var postExistsResult = await store.ExistsAsync(testPartitionKey, testId);

            Assert.False(postExistsResult);

            var postGetResult = await store.GetAsync(testPartitionKey, testId);

            Assert.Null(postGetResult);

            var postListResult = await store.QueryAsync().ToListAsync();

            Assert.False(postListResult.Any());

            await store.DeleteAsync(testPartitionKey, testId);
        }

        public static async Task TestIdPartitionedDocumentStore<TId, TDocument>(
            IDocumentStore<TId, TDocument> store,
            TId testId,
            TDocument testValueA,
            TDocument testValueB,
            Func<TDocument, TDocument, bool> areEqual = default)
        {
            areEqual ??= (a, b) => a.Equals(b);

            await store.CreateStoreIfNotExistsAsync();

            await foreach (var item in store.QueryAsync())
            {
                var id = store.GetKey(item);

                await store.DeleteAsync(id);
            }

            var preExistsResult = await store.ExistsAsync(testId);

            Assert.False(preExistsResult);

            var preGetResult = await store.GetAsync(testId);

            Assert.Null(preGetResult);

            var preListResult = await store.QueryAsync().ToListAsync();

            Assert.False(preListResult.Any());

            await store.UpsertAsync(testValueA);

            var firstExistsResult = await store.ExistsAsync(testId);

            Assert.True(firstExistsResult);

            var firstGetResult = await store.GetAsync(testId);

            Assert.True(areEqual(testValueA, firstGetResult));

            var firstGetId = store.GetKey(firstGetResult);

            Assert.Equal(testId, firstGetId);

            var firstListResult = await store.QueryAsync().ToListAsync();

            Assert.True(areEqual(testValueA, firstListResult.Single()));

            await store.UpsertAsync(testValueB);

            var secondExistsResult = await store.ExistsAsync(testId);

            Assert.True(secondExistsResult);

            var secondGetResult = await store.GetAsync(testId);

            Assert.True(areEqual(testValueB, secondGetResult));

            await store.UpsertAsync(testValueA);

            var thirdGetResult = await store.GetAsync(testId);

            Assert.True(areEqual(testValueA, thirdGetResult));

            await store.DeleteAsync(testId);

            var postExistsResult = await store.ExistsAsync(testId);

            Assert.False(postExistsResult);

            var postGetResult = await store.GetAsync(testId);

            Assert.Null(postGetResult);

            var postListResult = await store.QueryAsync().ToListAsync();

            Assert.False(postListResult.Any());

            await store.DeleteAsync(testId);
        }

        public static async Task TestSingletonDocumentStore<TDocument>(
            IDocumentStore<TDocument> store,
            TDocument testValueA,
            TDocument testValueB,
            Func<TDocument, TDocument, bool> areEqual = default)
        {
            areEqual ??= (a, b) => a.Equals(b);

            await store.CreateStoreIfNotExistsAsync();

            await store.DeleteAsync();

            var preExistsResult = await store.ExistsAsync();

            Assert.False(preExistsResult);

            var preGetResult = await store.GetAsync();

            Assert.Null(preGetResult);

            await store.UpsertAsync(testValueA);

            var firstExistsResult = await store.ExistsAsync();

            Assert.True(firstExistsResult);

            var firstGetResult = await store.GetAsync();

            Assert.True(areEqual(testValueA, firstGetResult));

            await store.UpsertAsync(testValueB);

            var secondExistsResult = await store.ExistsAsync();

            Assert.True(secondExistsResult);

            var secondGetResult = await store.GetAsync();

            Assert.True(areEqual(testValueB, secondGetResult));

            await store.UpsertAsync(testValueA);

            var thirdGetResult = await store.GetAsync();

            Assert.True(areEqual(testValueA, thirdGetResult));

            await store.DeleteAsync();

            var postExistsResult = await store.ExistsAsync();

            Assert.False(postExistsResult);

            var postGetResult = await store.GetAsync();

            Assert.Null(postGetResult);

            await store.DeleteAsync();
        }
    }
}
