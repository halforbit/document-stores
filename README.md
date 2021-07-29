# Halforbit Document Stores

Document Stores let you work with document databases in a way that is quick, easy, and reliable.

## Getting Started

Add a NuGet reference to your project for your database technology:

[Halforbit.DocumentStores.CosmosDb](https://www.nuget.org/packages/Halforbit.DocumentStores.CosmosDb)

### What do you want to store?

To create a document store, first decide on your **document type**. This can be any JSON-friendly `class`, `record`, or `JObject`. Let's make a record to track a person:

```csharp
public record Person(
    Guid PersonId,
    string FirstName,
    string LastName);
```

Our document has a `Guid` **id property** named `PersonId`. Id properties can be simple value types like `Guid`, `int`, and `string`. 

For more information on keying, see [Keying with Halforbit Document Stores](keying.md).

### Build a Document Store

Use the fluent builder to describe and create your document stores:

```csharp
IDocumentStore<Guid, Person> store = DocumentStore
    .Describe()
    .CosmosDb()
    .ConnectionString("<connection-string-here>")
    .Database("test-database")
    .Container("test-container")
    .Document<Person>()
    .Key(d => d.PersonId)
    .Build();
```

If your database or container do not exist, you can use your store to create them:

```csharp
await store.CreateStoreIfNotExistsAsync();
```

### Put a document in a store

Let's create a `Person` and put it in our document store:

```csharp
var person = new Person(
    PersonId: Guid.NewGuid(),
    FirstName: "Steve",
    LastName: "Smith");

await store.UpsertAsync(person);
```

### Get a document from a store

You can get a document with its key:

```csharp
var person = await store.GetAsync(personId);
```

### Query the documents in a store

You can use LINQ `IQueryable` or raw SQL to query your store with the `QueryAsync` methods. The results are given to you as an `IAsyncEnumerable<>`.

```csharp
await foreach (var person in store.QueryAsync(q => q
    .Where(p => p.LastName == "Smith")))
{
    // do something with person here.
}
```

### Delete a document from a store

You can delete a document with its key:

```csharp
await store.DeleteAsync(personId);
```
