using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Instrumentation;

namespace Our.Umbraco.GraphQL
{
    public static class FieldMiddlewareBuilderExtensions
    {
        public static IFieldMiddlewareBuilder Use<T>(this IFieldMiddlewareBuilder builder) where T: IFieldMiddleware, new()
            => builder.Use(next => context => new T().Resolve(context, next));
    }
}
