using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using Xunit;

namespace Halforbit.DocumentStores.Tests
{
    public class BuilderTests
    {
        [Fact]
        public void Build_CosmosDb_PartitionKey_JObject()
        {
            var store = DocumentStore
                .Describe()
                .CosmosDb()
                .ConnectionString("connection-string")
                .Database("database")
                .Container("container")
                .Document<JObject>()
                .Key<string, Guid>("/LastName", "/PersonId")
                .Build();

            Assert.IsType<CosmosDbDocumentStore<string, Guid, JObject>>(store);

            Assert.Equal("connection-string", store.Field<string>("_connectionString"));

            Assert.Equal("database", store.Field<string>("_database"));

            Assert.Equal("container", store.Field<string>("_container"));

            Assert.Equal("/LastName", store.Field<string>("_partitionKeyPath"));

            Assert.Equal("/PersonId", store.Field<string>("_idPath"));
        }

        [Fact]
        public void Build_CosmosDb_PartitionKey_Lambda_Record()
        {
            var store = DocumentStore
                .Describe()
                .CosmosDb()
                .ConnectionString("connection-string")
                .Database("database")
                .Container("container")
                .Document<Person_String_Guid>()
                .Key(d => d.LastName, d => d.PersonId)
                .Build();

            Assert.IsType<CosmosDbDocumentStore<string, Guid, Person_String_Guid>>(store);

            Assert.Equal("connection-string", store.Field<string>("_connectionString"));

            Assert.Equal("database", store.Field<string>("_database"));

            Assert.Equal("container", store.Field<string>("_container"));

            Assert.Equal("/LastName", store.Field<string>("_partitionKeyPath"));

            Assert.Equal("/PersonId", store.Field<string>("_idPath"));
        }

        [Fact]
        public void Build_CosmosDb_IdPartitioned_JObject()
        {
            var store = DocumentStore
                .Describe()
                .CosmosDb()
                .ConnectionString("connection-string")
                .Database("database")
                .Container("container")
                .Document<JObject>()
                .Key<Guid>("/PersonId")
                .Build();

            Assert.IsType<CosmosDbDocumentStore<string, Guid, JObject>>(store);

            Assert.Equal("connection-string", store.Field<string>("_connectionString"));

            Assert.Equal("database", store.Field<string>("_database"));

            Assert.Equal("container", store.Field<string>("_container"));

            Assert.Equal("/id", store.Field<string>("_partitionKeyPath"));

            Assert.Equal("/PersonId", store.Field<string>("_idPath"));
        }

        [Fact]
        public void Build_CosmosDb_IdPartitioned_Lambda_Record()
        {
            var store = DocumentStore
                .Describe()
                .CosmosDb()
                .ConnectionString("connection-string")
                .Database("database")
                .Container("container")
                .Document<Person_String_Guid>()
                .Key(d => d.PersonId)
                .Build();

            Assert.IsType<CosmosDbDocumentStore<string, Guid, Person_String_Guid>>(store);

            Assert.Equal("connection-string", store.Field<string>("_connectionString"));

            Assert.Equal("database", store.Field<string>("_database"));

            Assert.Equal("container", store.Field<string>("_container"));

            Assert.Equal("/id", store.Field<string>("_partitionKeyPath"));

            Assert.Equal("/PersonId", store.Field<string>("_idPath"));
        }

        [Fact]
        public void Build_CosmosDb_Singleton_JObject()
        {
            var store = DocumentStore
                .Describe()
                .CosmosDb()
                .ConnectionString("connection-string")
                .Database("database")
                .Container("container")
                .Document<JObject>()
                .Build();

            Assert.IsType<CosmosDbDocumentStore<string, string, JObject>>(store);

            Assert.Equal("connection-string", store.Field<string>("_connectionString"));

            Assert.Equal("database", store.Field<string>("_database"));

            Assert.Equal("container", store.Field<string>("_container"));

            Assert.Equal(string.Empty, store.Field<string>("_partitionKeyPath"));

            Assert.Equal(string.Empty, store.Field<string>("_idPath"));
        }

        [Fact]
        public void Build_MockInMemory_PartitionKey_JObject()
        {
            var store = DocumentStore
                .Describe()
                .MockInMemory()
                .Document<JObject>()
                .Key<string, Guid>("/LastName", "/PersonId")
                .Build();

            Assert.IsType<MockDocumentStore<string, Guid, JObject>>(store);

            Assert.Equal("/LastName", store.Field<string>("_partitionKeyPath"));

            Assert.Equal("/PersonId", store.Field<string>("_idPath"));
        }

        [Fact]
        public void Build_MockInMemory_IdPartitioned_JObject()
        {
            var store = DocumentStore
                .Describe()
                .MockInMemory()
                .Document<JObject>()
                .Key<Guid>("/PersonId")
                .Build();

            Assert.IsType<MockDocumentStore<string, Guid, JObject>>(store);

            Assert.Equal("/id", store.Field<string>("_partitionKeyPath"));

            Assert.Equal("/PersonId", store.Field<string>("_idPath"));
        }

        [Fact]
        public void Build_MockInMemory_Singleton_JObject()
        {
            var store = DocumentStore
                .Describe()
                .MockInMemory()
                .Document<JObject>()
                .Build();

            Assert.IsType<MockDocumentStore<string, string, JObject>>(store);

            Assert.Equal(string.Empty, store.Field<string>("_partitionKeyPath"));

            Assert.Equal(string.Empty, store.Field<string>("_idPath"));
        }
    }

    public static class PeekExtensions
    {
        public static TField Field<TField>(this object obj, string field)
        {
            if (obj is JObject jObject)
            {
                return jObject.Value<TField>(field);
            }

            return (TField)obj
                .GetType()
                .GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(obj);
        }
    }
}
