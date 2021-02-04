using Microsoft.Azure.Cosmos.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Halforbit.DocumentStores
{
    public static class Extensions
    {
        public static async Task<IReadOnlyList<TSource>> ToListAsync<TSource>(
            this IQueryable<TSource> queryable)
        {
            var result = new List<TSource>();

            var feedIterator = queryable.ToFeedIterator();

            do
            {
                result.AddRange((await feedIterator.ReadNextAsync().ConfigureAwait(false)).Resource);
            }
            while (feedIterator.HasMoreResults);

            return result;
        }

        public static async IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(
            this IQueryable<TSource> queryable)
        {
            var feedIterator = queryable.ToFeedIterator();

            do
            {
                var page = await feedIterator.ReadNextAsync().ConfigureAwait(false);

                foreach (var item in page)
                {
                    yield return item;
                }
            }
            while (feedIterator.HasMoreResults);
        }
    }
}
