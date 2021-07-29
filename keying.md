
# Keying with Halforbit Document Stores

## Every Document Needs a Key

A **document key** uniquely identifies a document. 

When creating a document store, you will specify a document key with the `.Key(...)` builder method:

```cs
var store = DocumentStore
    .Describe()
    .CosmosDb()
    .ConnectionString("<connection-string>")
    .Database("<database>")
    .Container("<container>")
    .Document<Person>()
    .Key(d => d.PersonId) // The key is Person.PersonId
    .Build();
```

You can choose any property on your document type as your key.

## Some Hidden Details

Document databases have peculiar requirements that leak into the developer's world and complicate things. An aim of Document Stores is to simplify things. Here are a couple of ways this is done:

### The `id` Property

Document databases require your documents to have a `string` property named `id`. When using Document Stores you do not need to worry about this. Document Stores will automatically deal with the `id` requirements behind the scenes.

**You do not need an `id` property on your document type.**

### Partition Keys in CosmosDb

When you make a CosmosDb container, you are required to choose a **partition key**. Usually, you will use `/id` (which is the default) for your partition key. 

Unless you want an explicit partition key, Document Stores lets you ignore the concept of partitioning completely.

**The partition key of your CosmosDb container should usually be `/id`.**

**You do not need a property for partition key on your document type.**

#### Explicit Partition Key

If you _do_ want an explicit partition key, this can be done as well:

```cs
var store = DocumentStore
    .Describe()
    .CosmosDb()
    .ConnectionString("<connection-string>")
    .Database("<database>")
    .Container("<container>")
    .Document<Person>()
    .Key(d => d.LastName, d => d.PersonId) // The partition key is Person.LastName
    .Build();
```
 
## Singleton Document Stores

If you would like to store a single document in its own collection, you can create a **singleton** document store. To do this, omit the `.Key(...)` method from your description:

```cs
var store = DocumentStore
    .Describe()
    .CosmosDb()
    .ConnectionString("<connection-string>")
    .Database("<database>")
    .Container("<container>")
    .Document<Person>()
    .Build(); // This is a singleton, since no key is specified.
```

This creates a 'keyless' `IDocumentStore<TValue>`, with a simplified interface that does not require keying when used.