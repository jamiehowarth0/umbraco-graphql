using Umbraco.Core.Composing;

namespace Our.Umbraco.GraphQL.Composing
{
    public static class CompositionExtensions
    {
        public static GraphVisitorCollectionBuilder GraphVisitors(this Composition builder) =>
            builder.WithCollectionBuilder<GraphVisitorCollectionBuilder>();

        public static FieldMiddlewareCollectionBuilder FieldMiddlewares(this Composition builder) =>
            builder.WithCollectionBuilder<FieldMiddlewareCollectionBuilder>();
    }
}
