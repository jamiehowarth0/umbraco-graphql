using System;
using System.Reflection;
using GraphQL;
using GraphQL.Types;
using Our.Umbraco.GraphQL.Adapters;
using Our.Umbraco.GraphQL.Adapters.Visitors;

namespace Our.Umbraco.GraphQL.Builders
{
    public class SchemaBuilder : ISchemaBuilder
    {
        private readonly IGraphTypeAdapter _graphTypeAdapter;
        private readonly IGraphVisitor _visitor;
        private readonly IServiceProvider _dependencyResolver;

        public SchemaBuilder(IGraphTypeAdapter graphTypeAdapter, IServiceProvider dependencyResolver, IGraphVisitor visitor)
        {
            _graphTypeAdapter = graphTypeAdapter ?? throw new ArgumentNullException(nameof(graphTypeAdapter));
            _dependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
            _visitor = visitor;
        }

        public ISchema Build<TSchema>(params TypeInfo[] additionalTypes) => Build(typeof(TSchema).GetTypeInfo(), additionalTypes);

        public ISchema Build(TypeInfo schemaType, params TypeInfo[] additionalTypes)
        {
            if (schemaType == null) throw new ArgumentNullException(nameof(schemaType));

            var schema = new Schema(_dependencyResolver);

            var queryPropertyInfo = schemaType.GetProperty("Query");
            if(queryPropertyInfo == null)
                throw new ArgumentException($"Could not find property 'Query' on {schemaType}.", nameof(schemaType));

            if(queryPropertyInfo.CanRead == false)
                throw new ArgumentException("'Query' does not have a getter.", nameof(schemaType));
            schema.Query = (IObjectGraphType) _graphTypeAdapter.Adapt(queryPropertyInfo.GetMethod.ReturnType.GetTypeInfo());

            foreach(var type in additionalTypes)
            {
                schema.RegisterType(_graphTypeAdapter.Adapt(type));
            }

            _visitor?.Visit(schema);

            return schema;
        }
    }
}
