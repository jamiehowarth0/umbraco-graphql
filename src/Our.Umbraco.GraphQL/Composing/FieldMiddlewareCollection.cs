using System.Collections.Generic;
using GraphQL.Instrumentation;
using Umbraco.Core.Composing;

namespace Our.Umbraco.GraphQL.Composing
{
    public class FieldMiddlewareCollection : BuilderCollectionBase<IFieldMiddleware>
    {
        public FieldMiddlewareCollection(IEnumerable<IFieldMiddleware> items)
            : base(items)
        {
        }
    }
}
