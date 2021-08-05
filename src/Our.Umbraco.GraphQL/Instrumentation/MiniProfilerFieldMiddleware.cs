using GraphQL.Instrumentation;
using GraphQL.Types;
using StackExchange.Profiling;
using System.Threading.Tasks;
using GraphQL;

namespace Our.Umbraco.GraphQL.Instrumentation
{
    public class MiniProfilerFieldMiddleware : IFieldMiddleware
    {
        public Task<object> Resolve(IResolveFieldContext context, FieldMiddlewareDelegate next)
        {
            using (MiniProfiler.Current.Step($"[GraphQL] Resolving {string.Join(".", context.Path)}"))
            {
                return next(context);
            }
        }
    }
}
