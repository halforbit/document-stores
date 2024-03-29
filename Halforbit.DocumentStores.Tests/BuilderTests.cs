﻿using Halforbit.DocumentStores.Exceptions;
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
        public void Build_CosmosDb_PartitionKey_Lambda_Record_Pk_String_Validation()
        {
            var store = DocumentStore
                .Describe()
                .CosmosDb()
                .ConnectionString("connection-string")
                .Database("database")
                .Container("container")
                .Document<Person_String_Guid>()
                .Key(d => d.LastName, d => d.PersonId)
                .Validation(new PersonValidator_string_Guid())
                .Build();

            Assert.IsType<CosmosDbDocumentStore<string, Guid, Person_String_Guid>>(store);

            Assert.Equal("connection-string", store.Field<string>("_connectionString"));

            Assert.Equal("database", store.Field<string>("_database"));

            Assert.Equal("container", store.Field<string>("_container"));

            Assert.Equal("/LastName", store.Field<string>("_partitionKeyPath"));

            Assert.Equal("/PersonId", store.Field<string>("_idPath"));

            Assert.NotNull(store.Field<IDocumentValidator<string, Guid, Person_String_Guid>>("_documentValidator"));
        }

        [Fact]
        public void Build_CosmosDb_PartitionKey_Lambda_Record_Pk_Guid_Validation()
        {
            var store = DocumentStore
                .Describe()
                .CosmosDb()
                .ConnectionString("connection-string")
                .Database("database")
                .Container("container")
                .Document<Person_Guid_Guid>()
                .Key(d => d.AccountId, d => d.PersonId)
                .Validation(new PersonValidator_Guid_Guid())
                .Build();

            Assert.IsType<CosmosDbDocumentStore<Guid, Guid, Person_Guid_Guid>>(store);

            Assert.Equal("connection-string", store.Field<string>("_connectionString"));

            Assert.Equal("database", store.Field<string>("_database"));

            Assert.Equal("container", store.Field<string>("_container"));

            Assert.Equal("/AccountId", store.Field<string>("_partitionKeyPath"));

            Assert.Equal("/PersonId", store.Field<string>("_idPath"));

            Assert.NotNull(store.Field<IDocumentValidator<Guid, Guid, Person_Guid_Guid>>("_documentValidator"));
        }

        [Fact]
        public void Build_CosmosDb_PartitionKey_Lambda_Record_Pk_Invalid_Fails()
        {
            Assert.Throws<UnsupportedPartitionKeyTypeException>(() => DocumentStore
                .Describe()
                .CosmosDb()
                .ConnectionString("connection-string")
                .Database("database")
                .Container("container")
                .Document<Person_Guid_Guid>()
                .Key(d => d.DateOfBirth, d => d.PersonId)
                .Build());
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
        public void Build_CosmosDb_IdPartitioned_Lambda_Record_Validation()
        {
            var store = DocumentStore
                .Describe()
                .CosmosDb()
                .ConnectionString("connection-string")
                .Database("database")
                .Container("container")
                .Document<Person_String_Guid>()
                .Key(d => d.PersonId)
                .Validation(new PersonValidator_Guid())
                .Build();

            Assert.IsType<CosmosDbDocumentStore<string, Guid, Person_String_Guid>>(store);

            Assert.Equal("connection-string", store.Field<string>("_connectionString"));

            Assert.Equal("database", store.Field<string>("_database"));

            Assert.Equal("container", store.Field<string>("_container"));

            Assert.Equal("/id", store.Field<string>("_partitionKeyPath"));

            Assert.Equal("/PersonId", store.Field<string>("_idPath"));

            Assert.NotNull(store.Field<IDocumentValidator<string, Guid, Person_String_Guid>>("_documentValidator"));
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
        public void Build_CosmosDb_Singleton_Record_Validation()
        {
            var store = DocumentStore
                .Describe()
                .CosmosDb()
                .ConnectionString("connection-string")
                .Database("database")
                .Container("container")
                .Document<Person_String_Guid>()
                .Validation(new PersonValidator())
                .Build();

            Assert.IsType<CosmosDbDocumentStore<string, string, Person_String_Guid>>(store);

            Assert.Equal("connection-string", store.Field<string>("_connectionString"));

            Assert.Equal("database", store.Field<string>("_database"));

            Assert.Equal("container", store.Field<string>("_container"));

            Assert.Equal(string.Empty, store.Field<string>("_partitionKeyPath"));

            Assert.Equal(string.Empty, store.Field<string>("_idPath"));

            Assert.NotNull(store.Field<IDocumentValidator<string, string, Person_String_Guid>>("_documentValidator"));
        }

        [Fact]
        public void Build_MockInMemory_PartitionKey_JObject()
        {
            var store = DocumentStore
                .Describe()
                .MockInMemory()
                .Document<JObject>()
                .Key<string, Guid>("/LastName", "/PersonId")
                .Documents(new[] { JObject.Parse($"{{ \"LastName\": \"Smith\", \"PersonId\": \"{Guid.NewGuid()}\" }}") })
                .Build();

            Assert.IsType<MockDocumentStore<string, Guid, JObject>>(store);

            Assert.Equal("/LastName", store.Field<string>("_partitionKeyPath"));

            Assert.Equal("/PersonId", store.Field<string>("_idPath"));
        }

        [Fact]
        public void Build_MockInMemory_IdPartitioned_JObject_Documents()
        {
            var store = DocumentStore
                .Describe()
                .MockInMemory()
                .Document<JObject>()
                .Key<Guid>("/PersonId")
                .Documents(new[] { JObject.Parse($"{{ \"PersonId\": \"{Guid.NewGuid()}\" }}") })
                .Build();

            Assert.IsType<MockDocumentStore<string, Guid, JObject>>(store);

            Assert.Equal("/id", store.Field<string>("_partitionKeyPath"));

            Assert.Equal("/PersonId", store.Field<string>("_idPath"));
        }

        [Fact]
        public void Build_MockInMemory_Singleton_JObject_Documents()
        {
            var store = DocumentStore
                .Describe()
                .MockInMemory()
                .Document<JObject>()
                .Documents(new[] { JObject.Parse("{}") })
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

    public class PersonValidator_string_Guid : DocumentValidatorBase<string, Guid, Person_String_Guid>
    { }

    public class PersonValidator_Guid_Guid : DocumentValidatorBase<Guid, Guid, Person_Guid_Guid>
    { }

    public class PersonValidator_Guid : DocumentValidatorBase<Guid, Person_String_Guid>
    { }

    public class PersonValidator : DocumentValidatorBase<Person_String_Guid>
    { }
}
