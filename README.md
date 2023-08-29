# Halforbit Document Stores

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE) &nbsp;[![Build status](https://ci.appveyor.com/api/projects/status/jncopu91kmdi38p5?svg=true)](https://ci.appveyor.com/project/halforbit/document-stores) &nbsp;[![Nuget Package](https://img.shields.io/nuget/v/Halforbit.DocumentStores.svg)](#nuget-packages)

Document Stores lets you easily create queryable stores with strongly-typed keying over document databases like CosmosDb, without the need for high-ceremony repository patterns or bare-metal integrations.

## Features

- **Queryable:** Document stores let you use `IQueryable` or SQL language to query for strongly- or dynamically-typed documents.
  
- **Easy Strongly-Typed Keying:** Define partition keys and document IDs as properties using simple lambda expressions.

- **No Repository Pattern Needed:** Ditch the hand-made, error-prone, high-ceremony repository implementations.

## Getting Started

1. **Install NuGet Packages:** Install the NuGet packages for your desired storage providers and formats:
    ```powershell
    Install-Package Halforbit.DocumentStores.CosmosDb
    ```
    See the [NuGet Packages](#nuget-packages) section below for a list of available NuGet packages and what storage providers and formats they support.

2. **Define Your Stores:** Use the `DocumentStore` type to create ad-hoc stores, or define them as properties on a data context.
   
3. **Use Your Stores:** Persist, retrieve, and query data with your stores.

A store can be defined as a property of a **data context**, or created **ad-hoc** and stored in a local variable.

## Example Usage

### What do you want to store?

To create a document store, first decide on your **document type**. This can be any JSON-friendly `class`, `record`, or `JObject`. Let's make a record to track a person:

```csharp
public record Person(
    Guid PersonId,
    string FirstName,
    string LastName);
```

Our document type has a `Guid` **key property** named `PersonId`. Key properties can be simple value types like `Guid`, `int`, and `string`. 

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


<a name="nuget-packages"></a>

## NuGet Packages

The following NuGet packages are provided, parted out by their dependencies. Install the ones that contain the storage providers and formats you wish to use.

| Storage Provider or Format | NuGet Package |
|----------------------------|---------------|
| (Base Library) | [`Halforbit.DocumentStores`](https://www.nuget.org/packages/Halforbit.DocumentStores) |
| Azure CosmosDb | [`Halforbit.DocumentStores.CosmosDb`](https://www.nuget.org/packages/Halforbit.DocumentStores.CosmosDb) |

## License

Data Stores is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
