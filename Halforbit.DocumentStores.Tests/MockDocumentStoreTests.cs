using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Halforbit.DocumentStores.Tests
{
    public class MockDocumentStoreTests
    {
        const string _id = "8992c74cc49546d5900f1e30b1df1cb3";

        Person_String_Guid _stringGuidPersonA = new Person_String_Guid(
            new Guid(_id),
            "Steve",
            "Smith",
            new DateTime(1994, 01, 02, 00, 00, 00, DateTimeKind.Utc));

        Person_String_Guid _stringGuidPersonB = new Person_String_Guid(
            new Guid(_id),
            "John",
            "Smith",
            new DateTime(1992, 02, 03, 00, 00, 00, DateTimeKind.Utc));

        Person_Int_Int _intIntPersonA = new Person_Int_Int(
            123,
            234,
            "Steve",
            "Smith",
            new DateTime(1994, 01, 02, 00, 00, 00, DateTimeKind.Utc));

        Person_Int_Int _intIntPersonB = new Person_Int_Int(
            123,
            234,
            "John",
            "Smith",
            new DateTime(1992, 02, 03, 00, 00, 00, DateTimeKind.Utc));

        [Fact]
        public async Task Pk_String_Id_Guid_Doc_Record()
        {
            var store = GetPartitionKeyedStore<string, Guid, Person_String_Guid>(
                containerName: "pk-string-id-guid-doc-record",
                partitionKeyPath: "/LastName",
                idPath: "/PersonId");

            await UniversalIntegrationTests.TestPartitionKeyedDocumentStore(
                store,
                _stringGuidPersonA.LastName,
                _stringGuidPersonA.PersonId,
                _stringGuidPersonA,
                _stringGuidPersonB);
        }

        [Fact]
        public async Task Pk_Int_Id_Int_Doc_Record()
        {
            var store = GetPartitionKeyedStore<int, int, Person_Int_Int>(
                containerName: "pk-int-id-int-doc-record",
                partitionKeyPath: "/DepartmentId",
                idPath: "/PersonId");

            await UniversalIntegrationTests.TestPartitionKeyedDocumentStore(
                store,
                _intIntPersonA.DepartmentId,
                _intIntPersonA.PersonId,
                _intIntPersonA,
                _intIntPersonB);
        }

        [Fact]
        public async Task Pk_String_Id_Guid_Doc_JObject()
        {
            var store = GetPartitionKeyedStore<string, Guid, JObject>(
                containerName: "pk-string-id-guid-doc-jobject",
                partitionKeyPath: "/LastName",
                idPath: "/PersonId");

            await UniversalIntegrationTests.TestPartitionKeyedDocumentStore(
                store,
                _stringGuidPersonA.LastName,
                _stringGuidPersonA.PersonId,
                JObject.FromObject(_stringGuidPersonA),
                JObject.FromObject(_stringGuidPersonB),
                (a, b) => a.ToString() == b.ToString());
        }

        [Fact]
        public async Task Pk_Int_Id_Int_Doc_JObject()
        {
            var store = GetPartitionKeyedStore<int, int, JObject>(
                containerName: "pk-int-id-int-doc-jobject",
                partitionKeyPath: "/DepartmentId",
                idPath: "/PersonId");

            await UniversalIntegrationTests.TestPartitionKeyedDocumentStore(
                store,
                _intIntPersonA.DepartmentId,
                _intIntPersonA.PersonId,
                JObject.FromObject(_intIntPersonA),
                JObject.FromObject(_intIntPersonB),
                (a, b) => a.ToString() == b.ToString());
        }

        [Fact]
        public async Task Id_Guid_Doc_Record()
        {
            var store = GetIdPartitionedStore<Guid, Person_String_Guid>(
                containerName: "id-guid-doc-record",
                idPath: "/PersonId");

            await UniversalIntegrationTests.TestIdPartitionedDocumentStore(
                store,
                _stringGuidPersonA.PersonId,
                _stringGuidPersonA,
                _stringGuidPersonB);
        }

        [Fact]
        public async Task Id_Guid_Doc_Record_Explicit_Pk()
        {
            var store = GetIdPartitionedStore_ExplicitPk<Guid, Person_String_Guid>(
                containerName: "id-guid-doc-record-explicit-pk",
                partitionKeyPath: "/PersonId",
                idPath: "/PersonId");

            await UniversalIntegrationTests.TestIdPartitionedDocumentStore(
                store,
                _stringGuidPersonA.PersonId,
                _stringGuidPersonA,
                _stringGuidPersonB);
        }

        [Fact]
        public async Task Id_Int_Doc_Record()
        {
            var store = GetIdPartitionedStore<int, Person_Int_Int>(
                containerName: "id-int-doc-record",
                idPath: "/PersonId");

            await UniversalIntegrationTests.TestIdPartitionedDocumentStore(
                store,
                _intIntPersonA.PersonId,
                _intIntPersonA,
                _intIntPersonB);
        }

        [Fact]
        public async Task Id_Int_Doc_Record_Explicit_Pk()
        {
            var store = GetIdPartitionedStore_ExplicitPk<int, Person_Int_Int>(
                containerName: "id-int-doc-record-explicit-pk",
                partitionKeyPath: "/PersonId",
                idPath: "/PersonId");

            await UniversalIntegrationTests.TestIdPartitionedDocumentStore(
                store,
                _intIntPersonA.PersonId,
                _intIntPersonA,
                _intIntPersonB);
        }

        [Fact]
        public async Task Id_Guid_Doc_JObject()
        {
            var store = GetIdPartitionedStore<Guid, JObject>(
                containerName: "id-guid-doc-jobject",
                idPath: "/PersonId");

            await UniversalIntegrationTests.TestIdPartitionedDocumentStore(
                store,
                _stringGuidPersonA.PersonId,
                JObject.FromObject(_stringGuidPersonA),
                JObject.FromObject(_stringGuidPersonB),
                (a, b) => a.ToString() == b.ToString());
        }

        [Fact]
        public async Task Id_Guid_Doc_JObject_Explicit_Pk()
        {
            var store = GetIdPartitionedStore_ExplicitPk<Guid, Person_String_Guid>(
                containerName: "id-guid-doc-jobject-explicit-pk",
                partitionKeyPath: "/PersonId",
                idPath: "/PersonId");

            await UniversalIntegrationTests.TestIdPartitionedDocumentStore(
                store,
                _stringGuidPersonA.PersonId,
                _stringGuidPersonA,
                _stringGuidPersonB);
        }

        [Fact]
        public async Task Id_Int_Doc_JObject()
        {
            var store = GetIdPartitionedStore<int, JObject>(
                containerName: "id-int-doc-jobject",
                idPath: "/PersonId");

            await UniversalIntegrationTests.TestIdPartitionedDocumentStore(
                store,
                _intIntPersonA.PersonId,
                JObject.FromObject(_intIntPersonA),
                JObject.FromObject(_intIntPersonB),
                (a, b) => a.ToString() == b.ToString());
        }

        [Fact]
        public async Task Id_Int_Doc_JObject_Explicit_Pk()
        {
            var store = GetIdPartitionedStore_ExplicitPk<int, JObject>(
                containerName: "id-int-doc-jobject-explicit-pk",
                partitionKeyPath: "/PersonId",
                idPath: "/PersonId");

            await UniversalIntegrationTests.TestIdPartitionedDocumentStore(
                store,
                _intIntPersonA.PersonId,
                JObject.FromObject(_intIntPersonA),
                JObject.FromObject(_intIntPersonB),
                (a, b) => a.ToString() == b.ToString());
        }

        [Fact]
        public async Task Doc_Record()
        {
            var store = GetSingletonStore<Person_String_Guid>(
                containerName: "doc-record");

            await UniversalIntegrationTests.TestSingletonDocumentStore(
                store,
                _stringGuidPersonA,
                _stringGuidPersonB);
        }

        [Fact]
        public async Task Doc_JObject()
        {
            var store = GetSingletonStore<JObject>(
                containerName: "doc-jobject");

            await UniversalIntegrationTests.TestSingletonDocumentStore(
                store,
                JObject.FromObject(_stringGuidPersonA),
                JObject.FromObject(_stringGuidPersonB),
                (a, b) => a.ToString() == b.ToString());
        }

        static IDocumentStore<TPartitionKey, TId, TDocument> GetPartitionKeyedStore<TPartitionKey, TId, TDocument>(
            string containerName,
            string partitionKeyPath,
            string idPath)
        {
            return new MockDocumentStore<TPartitionKey, TId, TDocument>(
                partitionKeyPath: partitionKeyPath,
                idPath: idPath,
                documents: new List<TDocument>());
        }

        static IDocumentStore<TId, TDocument> GetIdPartitionedStore<TId, TDocument>(
            string containerName,
            string idPath)
        {
            return new MockDocumentStore<string, TId, TDocument>(
                partitionKeyPath: "/id",
                idPath: idPath,
                documents: new List<TDocument>());
        }

        static IDocumentStore<TId, TDocument> GetIdPartitionedStore_ExplicitPk<TId, TDocument>(
            string containerName,
            string partitionKeyPath,
            string idPath)
        {
            return new MockDocumentStore<TId, TId, TDocument>(
                partitionKeyPath: partitionKeyPath ?? "/id",
                idPath: idPath,
                documents: new List<TDocument>());
        }

        static IDocumentStore<TDocument> GetSingletonStore<TDocument>(
            string containerName)
        {
            return new MockDocumentStore<string, string, TDocument>(
                partitionKeyPath: string.Empty,
                idPath: string.Empty,
                documents: new List<TDocument>());
        }
    }
}
