using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.GraphQL.Adapters.Visitors;
using Umbraco.Core.Composing;

namespace Our.Umbraco.GraphQL.Composing
{
    public class GraphVisitorCollectionBuilder : OrderedCollectionBuilderBase<GraphVisitorCollectionBuilder, GraphVisitorCollection, IGraphVisitor>
    {
        public GraphVisitorCollectionBuilder()
        {
        }

        protected override Lifetime CollectionLifetime => Lifetime.Singleton;

        protected override GraphVisitorCollectionBuilder This => this;
    }
}
