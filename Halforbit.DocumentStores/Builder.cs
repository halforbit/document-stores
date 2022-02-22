using Halforbit.DocumentStores.Exceptions;
using Halforbit.ObjectTools.DeferredConstruction;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Halforbit.DocumentStores
{
    public interface INeedsIntegration : IConstructionNode { }

    public interface INeedsMap : IConstructionNode { }

    public interface INeedsId<TPartitionKey> : IConstructionNode { }

    public interface INeedsDocumentType<TPartitionKey, TId> : IConstructionNode { }

    public interface IDocumentStoreDescription<TPartitionKey, TId, TDocument> : IConstructionNode { }

    public interface IDocumentStoreDescription<TId, TDocument> : IConstructionNode { }

    public interface IDocumentStoreDescription<TDocument> : IConstructionNode { }

    public interface INeedsMockDocuments : IConstructionNode { }

    public class Builder : 
        IConstructionNode,
        INeedsIntegration,
        INeedsMap,
        INeedsMockDocuments
    {
        public Builder(Constructable root)
        {
            Root = root;
        }

        public Constructable Root { get; }
    }

    class Builder<TPartitionKey, TId, TDocument> : 
        IConstructionNode,
        IDocumentStoreDescription<TPartitionKey, TId, TDocument>
    {
        public Builder(Constructable root)
        {
            Root = root;
        }

        public Constructable Root { get; }
    }

    class Builder<TId, TDocument> :
        IConstructionNode,
        IDocumentStoreDescription<TId, TDocument>
    {
        public Builder(Constructable root)
        {
            Root = root;
        }

        public Constructable Root { get; }
    }

    class Builder<TDocument> :
        IConstructionNode,
        IDocumentStoreDescription<TDocument>
    {
        public Builder(Constructable root)
        {
            Root = root;
        }

        public Constructable Root { get; }
    }

    public static class DocumentStore
    {
        public static INeedsIntegration Describe()
        {
            return new Builder(null);
        }
    }

    public static class DocumentStoreBuilderExtensions
    {
        static readonly HashSet<Type> _allowedPartitionKeyTypes = new HashSet<Type>(new[]
        {
            typeof(int),
            typeof(string),
            typeof(Guid)
        });

        // Keying /////////////////////////////////////////////////////////////

        public static IDocumentStoreDescription<TDocument> Document<TDocument>(this INeedsMap target)
        {
            return new Builder<TDocument>(target.Root
                .TypeArguments(typeof(string), typeof(string), typeof(TDocument))
                .Argument("partitionKeyPath", string.Empty)
                .Argument("idPath", string.Empty));
        }

        public static IDocumentStoreDescription<TPartitionKey, TId, TDocument> Key<TPartitionKey, TId, TDocument>(
            this IDocumentStoreDescription<TDocument> target,
            Expression<Func<TDocument, TPartitionKey>> partitionKey,
            Expression<Func<TDocument, TId>> id)
        {
            var partitionKeyProperty = GetPropertyInfo(partitionKey);

            var idProperty = GetPropertyInfo(id);

            return Key<TPartitionKey, TId, TDocument>(
                target,
                $"/{partitionKeyProperty.Name}",
                $"/{idProperty.Name}");
        }

        public static IDocumentStoreDescription<TId, TDocument> Key<TId, TDocument>(
            this IDocumentStoreDescription<TDocument> target,
            Expression<Func<TDocument, TId>> id,
            bool partitionKeyIsId = false)
        {
            var idProperty = GetPropertyInfo(id);

            return Key<TId, TDocument>(
                target, 
                $"/{idProperty.Name}", 
                partitionKeyIsId);
        }

        static IDocumentStoreDescription<TPartitionKey, TId, TDocument> Key<TPartitionKey, TId, TDocument>(
            this IDocumentStoreDescription<TDocument> target,
            string partitionKeyPath,
            string idPath)
        {
            var partitionKeyType = typeof(TPartitionKey);

            if (!_allowedPartitionKeyTypes.Contains(partitionKeyType))
            {
                throw new UnsupportedPartitionKeyTypeException(
                    $"The partition key type of {partitionKeyType.Name} is not supported. " +
                    $"Supported partition key types are: {string.Join(", ", _allowedPartitionKeyTypes)}");
            }

            return new Builder<TPartitionKey, TId, TDocument>(target.Root
                .TypeArguments(partitionKeyType, typeof(TId), typeof(TDocument))
                .Argument("partitionKeyPath", partitionKeyPath)
                .Argument("idPath", idPath));
        }

        static IDocumentStoreDescription<TId, TDocument> Key<TId, TDocument>(
            this IDocumentStoreDescription<TDocument> target,
            string idPath,
            bool partitionKeyIsId = false)
        {
            return new Builder<TId, TDocument>(target.Root
                .TypeArguments(typeof(string), typeof(TId), typeof(TDocument))
                .Argument("partitionKeyPath", partitionKeyIsId ? idPath : "/id")
                .Argument("idPath", idPath));
        }
                
        public static IDocumentStoreDescription<TPartitionKey, TId, JObject> Key<TPartitionKey, TId>(
            this IDocumentStoreDescription<JObject> target,
            string partitionKeyPath,
            string idPath)
        {
            return Key<TPartitionKey, TId, JObject>(target, partitionKeyPath, idPath);
        }

        public static IDocumentStoreDescription<TId, JObject> Key<TId>(
            this IDocumentStoreDescription<JObject> target,
            string idPath)
        {
            return Key<TId, JObject>(target, idPath);
        }

        // Validation /////////////////////////////////////////////////////////

        public static IDocumentStoreDescription<TPartitionKey, TId, TDocument> Validation<TPartitionKey, TId, TDocument, TValidator>(
            this IDocumentStoreDescription<TPartitionKey, TId, TDocument> target,
            TValidator documentValidator)
            where TValidator : IDocumentValidator<TPartitionKey, TId, TDocument>
        {
            return new Builder<TPartitionKey, TId, TDocument>(
                target.Root.Argument("documentValidator", documentValidator));
        }

        public static IDocumentStoreDescription<TId, TDocument> Validation<TId, TDocument, TValidator>(
            this IDocumentStoreDescription<TId, TDocument> target,
            TValidator documentValidator)
            where TValidator : IDocumentValidator<string, TId, TDocument>
        {
            return new Builder<TId, TDocument>(
                target.Root.Argument("documentValidator", documentValidator));
        }

        public static IDocumentStoreDescription<TDocument> Validation<TDocument, TValidator>(
            this IDocumentStoreDescription<TDocument> target,
            TValidator documentValidator)
            where TValidator : IDocumentValidator<string, string, TDocument>
        {
            return new Builder<TDocument>(
                target.Root.Argument("documentValidator", documentValidator));
        }

        // Mock ///////////////////////////////////////////////////////////////

        public static INeedsMap MockInMemory(
            this INeedsIntegration target)
        {
            return new Builder(target.Root.Type(typeof(MockDocumentStore<,,>)));
        }

        public static IDocumentStoreDescription<TPartitionKey, TId, TDocument> Documents<TPartitionKey, TId, TDocument>(
            this IDocumentStoreDescription<TPartitionKey, TId, TDocument> target, 
            IEnumerable<TDocument> documents)
        {
            return new Builder<TPartitionKey, TId, TDocument>(target.Root.Argument("documents", documents));
        }

        public static IDocumentStoreDescription<TId, TDocument> Documents<TId, TDocument>(
            this IDocumentStoreDescription<TId, TDocument> target,
            IEnumerable<TDocument> documents)
        {
            return new Builder<TId, TDocument>(target.Root.Argument("documents", documents));
        }

        public static IDocumentStoreDescription<TDocument> Documents<TDocument>(
            this IDocumentStoreDescription<TDocument> target,
            IEnumerable<TDocument> documents)
        {
            return new Builder<TDocument>(target.Root.Argument("documents", documents));
        }

        // Construction ///////////////////////////////////////////////////////

        public static IDocumentStore<TPartitionKey, TId, TDocument> Build<TPartitionKey, TId, TDocument>(
            this IDocumentStoreDescription<TPartitionKey, TId, TDocument> description)
        {
            return (IDocumentStore<TPartitionKey, TId, TDocument>)description.Root.Construct();
        }

        public static IDocumentStore<TId, TDocument> Build<TId, TDocument>(
            this IDocumentStoreDescription<TId, TDocument> description)
        {
            return (IDocumentStore<TId, TDocument>)description.Root.Construct();
        }

        public static IDocumentStore<TDocument> Build<TDocument>(
            this IDocumentStoreDescription<TDocument> description)
        {
            return (IDocumentStore<TDocument>)description.Root.Construct();
        }

        // Helpers ////////////////////////////////////////////////////////////

        static PropertyInfo GetPropertyInfo<TSource, TProperty>(
            Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member;

            if (propertyLambda.Body is UnaryExpression u)
            {
                member = u.Operand as MemberExpression;
            }
            else
            {
                member = propertyLambda.Body as MemberExpression;
            }

            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }
    }
}
