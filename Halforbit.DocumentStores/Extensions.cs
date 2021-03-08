using System.Collections.Generic;
using System.Threading.Tasks;

namespace Halforbit.DocumentStores
{
    public static class Extensions
    {
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return await Task.FromResult(item);
            }
        }

        public static async Task<IReadOnlyList<TItem>> ToListAsync<TItem>(
            this IAsyncEnumerable<TItem> items)
        {
            var results = new List<TItem>();

            await foreach (var item in items)
            {
                results.Add(item);
            }

            return results;
        }

        public static async Task<TItem> FirstOrDefaultAsync<TItem>(
            this IAsyncEnumerable<TItem> items)
        {
            await foreach (var item in items)
            {
                return item;
            }

            return default;
        }
    }
}
