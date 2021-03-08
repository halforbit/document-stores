using Newtonsoft.Json.Linq;
using System;

namespace Halforbit.DocumentStores
{
    public static class Internals
    {
        public static string GetIdString(object id)
        {
            if (id == null) return "0";

            var j = JToken.FromObject(id);

            return j.ToString();
        }

        public static (TPartitionKey PartitionKey, TId Id) GetKey<TPartitionKey, TId, TItem>(
            string partitionKeyPath, 
            string idPath,
            TItem document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var j = JToken.FromObject(document);

            var partitionKey = DereferencePath(
                j,
                string.IsNullOrWhiteSpace(partitionKeyPath) ?
                    "/id" :
                    partitionKeyPath) ?? "0";

            var id = DereferencePath(j, idPath) ?? "0";

            return (
                partitionKey.ToObject<TPartitionKey>(),
                id.ToObject<TId>());
        }

        public static JToken DereferencePath(JToken jToken, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return default;

            var value = jToken;

            foreach (var part in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                value = value[part];
            }

            return value;
        }
    }
}
