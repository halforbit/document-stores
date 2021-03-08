using Halforbit.ObjectTools.DeferredConstruction;
using Halforbit.DocumentStores.CosmosDb;

namespace Halforbit.DocumentStores
{
    namespace CosmosDb
    {
        public interface INeedsConnectionString : IConstructionNode { }

        public interface INeedsDatabase : IConstructionNode { }

        public interface INeedsContainer : IConstructionNode { }

        class Builder :
            IConstructionNode,
            INeedsConnectionString,
            INeedsDatabase,
            INeedsContainer
        {
            public Builder(Constructable root)
            {
                Root = root;
            }

            public Constructable Root { get; }
        }
    }

    public static class CosmosDbBuilderExtensions
    {
        public static INeedsConnectionString CosmosDb(
            this INeedsIntegration target)
        {
            return new CosmosDb.Builder(target.Root.Type(typeof(CosmosDbDocumentStore<,,>)));
        }

        public static INeedsDatabase ConnectionString(
            this INeedsConnectionString target,
            string connectionString)
        {
            return new CosmosDb.Builder(target.Root.Argument("connectionString", connectionString));
        }

        public static INeedsContainer Database(
            this INeedsDatabase target,
            string database)
        {
            return new CosmosDb.Builder(target.Root.Argument("database", database));
        }

        public static INeedsMap Container(
            this INeedsContainer target,
            string container)
        {
            return new Builder(target.Root.Argument("container", container));
        }
    }
}
